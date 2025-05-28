using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Tile;

[Tool]
public partial class GenerationManager : Node
{
    float blockScale;
    Vector2I chunkSize;
    List<Zone> _zones;
    List<List<Chunk>> chunkMatrix;
    uint seed;

    public GenerationManager()
    {
        _zones = new List<Zone>();

        //Zone z1 = new Zone();
        //z1.Initialize("Zone1", new Color("009b00"));
        //_zones.Add(z1);

        //Zone z2 = new Zone();
        //z2.Initialize("Zone2", new Color("cd9a11"));
        //_zones.Add(z2);
    }

    private bool AlreadyStarted()
    {
        return true;
    }

	public Node SceneFinder()
	{
        return EditorInterface.Singleton.GetEditedSceneRoot();
    }

	public void Start(float blockScale, Vector2I chunkSize, Tile.TileType tileType, uint seed, int[,] coherenceTable)
	{
        if (AlreadyStarted())
        {
            this.blockScale = blockScale;
            this.chunkSize = chunkSize;
            this.seed = seed;

            var editedSceneRoot = SceneFinder();

            if (editedSceneRoot != null)
            {
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
                Node oldChunky = sceneInstance.FindChild("InitialChunk");
                if (oldChunky != null)
                {
                    sceneInstance.RemoveChild(oldChunky);
                    oldChunky.QueueFree();
                }
                Tile tile = GetTileType(tileType);

                chunkMatrix = new List<List<Chunk>> { new List<Chunk>() };
                chunkMatrix[0].Add(new Chunk(tile));
                //chunkMatrix[0][0] = new Chunk(tile);
                chunkMatrix[0][0].Name = "InitialChunk";
                sceneInstance.AddChild(chunkMatrix[0][0]);
                chunkMatrix[0][0].Owner = sceneInstance;
                chunkMatrix[0][0].Initialize(Basis.Identity, new Vector3(0, 0, 0), this.blockScale, this.chunkSize, coherenceTable);
                
                //Pack Scene
                var packResult = packedScene.Pack(sceneInstance);
                if (packResult != Error.Ok)
                {
                    GD.PrintErr($"Failed to pack the scene: {packResult}");
                    return;
                }

                // Save the PackedScene to the same path
                var saveError = ResourceSaver.Save(packedScene, sceneFilePath);
                if (saveError != Error.Ok)
                {
                    GD.PrintErr($"Failed to save the scene: {saveError}");
                }
                else
                {
                    GD.Print($"Scene saved successfully at {sceneFilePath}");

                    EditorInterface.Singleton.ReloadSceneFromPath(sceneFilePath);
                }
            }
            else
            {
                GD.Print("No scene is currently being edited.");
            }
        }
        else
            GD.Print("Generation Manager already started.");
    }

    public void AddZone(String zoneName, Color color)
    {
        Zone zone = new Zone();
        zone.Initialize(zoneName, color);
        _zones.Add(zone);
    }

    public void AddZone(Zone zone)
    {
        _zones.Add(zone);
    }

    public void EditZone(int index, String zoneName, Color color)
    {
        _zones[index].Initialize(zoneName, color);
    }

    public void DeleteZone(int index)
    {
        _zones[index].Dispose();
    }

    public int GetZoneCount()
    {
        return _zones.Count;
    }

    public Zone[] GetZones() 
    { 
        //Debug.Print(_zones.Count.ToString() + " " + _zones.ToArray().Length.ToString());
        return _zones.ToArray();
    }

    public void ClearZones()
    {
        _zones.Clear();
    }

    public void ChangeColorZone(int index, Color color)
    {
        _zones[index].SetColor(color);
    }

    public uint GetSeed()
    {
        return seed;
    }

    public uint GenerateSeed()
    {
        return seed = new RandomNumberGenerator().Randi();
    }
}
