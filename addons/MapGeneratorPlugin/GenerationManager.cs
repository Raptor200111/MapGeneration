using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TFG_Godot.Properties;
using static Tile;

[Tool]
public partial class GenerationManager
{
    public GenerationManager()
    {
        configInfo = new ConfigInfo();
    }

    public ConfigInfo configInfo;

    //JSON methods
    public void LoadCurrentConfig()
    {
        const string filePath = "res://addons/MapGeneratorPlugin/currentConfig.json";
        Variant v = JsonConfigIO.Load(filePath);
        RestoreConfigFromVariant(v);
    }
    public void SaveCurrentConfig()
    {
        const string filePath = "res://addons/MapGeneratorPlugin/currentConfig.json";
        var data = BuildCurrentConfigDictionary();
        JsonConfigIO.Save(filePath, data);
    }
    
    public Variant BuildCurrentConfigDictionary()
    {
        return configInfo.ToJson();
    }
    public void RestoreConfigFromVariant(Variant v)
    {
        configInfo.LoadFromJson(v);
    }

    //Generation methods
    public void Start()
    {
        var editedSceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();
        if (editedSceneRoot != null)
        {
            var sw = Stopwatch.StartNew();

            var sceneFilePath = editedSceneRoot.SceneFilePath;
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneFilePath);

            var packedScene = GD.Load<PackedScene>(sceneFilePath);
            if (packedScene == null)
            {
                GD.PrintErr($"Failed to load scene at {sceneFilePath}");
                return;
            }

            // Instance the scene
            var sceneInstance = packedScene.Instantiate();
            if (sceneInstance == null)
            {
                GD.PrintErr("Failed to instance the scene.");
                return;
            }

            //Chunk Creation
            if (configInfo.DinamicWorld)
            {
                Node oldChunky = sceneInstance.FindChild("IndividualChunk");
                if (oldChunky != null)
                {
                    sceneInstance.RemoveChild(oldChunky);
                    oldChunky.QueueFree();
                }

                Node oldGen = sceneInstance.FindChild("MapGenerationManager");
                if (oldGen == null)
                {
                    var gen = new Map();
                    gen.Name = "MapGenerationManager";
                    sceneInstance.AddChild(gen);
                    gen.Owner = sceneInstance;
                }

                JsonConfigIO.Save("res://addons/MapGeneratorPlugin/executeConfig.json", BuildCurrentConfigDictionary());
            }
            else
            {
                Node oldChunky = sceneInstance.FindChild("IndividualChunk");
                if (oldChunky != null)
                {
                    sceneInstance.RemoveChild(oldChunky);
                    oldChunky.QueueFree();
                }

                Node oldGen = sceneInstance.FindChild("MapGenerationManager");
                if (oldGen != null)
                {
                    sceneInstance.RemoveChild(oldGen);
                    oldGen.QueueFree();
                }
                GenerateChunk(sceneInstance, "IndividualChunk", new Vector2I(0, 0));
            }

            //Pack Scene
            var packResult = packedScene.Pack(sceneInstance);
            if (packResult != Error.Ok)
            {
                GD.PrintErr($"Failed to pack the scene: {packResult}");
                return;
            }

            // Save the PackedScene to the same path
            var saveError = ResourceSaver.Save(packedScene, sceneFilePath);
            sw.Stop();
            if (saveError != Error.Ok)
            {
                GD.PrintErr($"Failed to save the scene: " + saveError + ", time: " + sw.Elapsed);
            }
            else
            {
                GD.Print("Scene saved successfully at " + sceneFilePath + ", time: " + sw.Elapsed);

                EditorInterface.Singleton.ReloadSceneFromPath(sceneFilePath);
            }
        }
        else
        {
            GD.Print("No scene is currently being edited.");
        }
    }

    private Chunk GenerateChunk(Node node, string name, Vector2I chunkPos)
    {
        var IndividualChunk = new Chunk(GetTileType((TileType)configInfo.TileType));
        IndividualChunk.Name = name;
        node.AddChild(IndividualChunk);
        IndividualChunk.Owner = node;
        IndividualChunk.Initialize(new Vector3(chunkPos.X, 0, chunkPos.Y), configInfo.BlockSize, new Vector2I(configInfo.ChunkWide, configInfo.ChunkDeep), GetZones(), configInfo.CoherenceTable, configInfo.ThreeDee, configInfo.HeightOverride, new NoiseAlgorithm(configInfo.Seed, configInfo.Freq2D, configInfo.Freq3D));

        return IndividualChunk;
    }

    //Zone management methods
    public void AddZone(String zoneName, Color color, ResourcePathList resources)
    {
        Zone zone = new Zone();
        zone.Initialize(zoneName, color, resources);
        configInfo.Zones.Add(zone);
    }

    public void AddZone(Zone zone)
    {
        configInfo.Zones.Add(zone);
    }

    public void EditZone(int index, String zoneName, Color color, ResourcePathList resources)
    {
        configInfo.Zones[index].Initialize(zoneName, color, resources);
    }

    public void DeleteZone(int index)
    {
        configInfo.Zones.RemoveAt(index);
    }

    public int GetZoneCount()
    {
        return configInfo.Zones.Count;
    }

    public Zone[] GetZones() 
    {
        return configInfo.Zones.ToArray();
    }

    public void ClearZones()
    {
        configInfo.Zones.Clear();
    }

    public void ChangeColorZone(int index, Color color)
    {
        configInfo.Zones[index].SetColor(color);
    }

    public void SetZonesFromJson(Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> data)
    {
        foreach (var zoneData in data)
        {
            var zone = Zone.JsonToZone(zoneData);
            if (zone != null)
            {
                configInfo.Zones.Add(zone);
            }
            else
            {
                GD.PrintErr("Failed to create zone from JSON data.");
            }
        }
    }
}