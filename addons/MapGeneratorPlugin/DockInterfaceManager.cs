
using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

[Tool]
public partial class DockInterfaceManager : Control
{
    public enum Dimensions
    {
        D2, D3
    }

    EditorInterface editorInterface;
    Dimensions dimension;
    PackedScene zoneContentPrefab;
    PackedScene newZoneContentPrefab;
    PackedScene zoneCoherencePrefab;

    bool newZoneActive = false;
    bool editedZoneActive = false;
    string loadedFilePath = "";


    public static DockInterfaceManager Singleton { get; private set; }
    public GenerationManager GenerationManager { get; private set; }
    public DockInterfaceManager()
    {
        editorInterface = EditorInterface.Singleton;
        dimension = Dimensions.D2;
        zoneContentPrefab = ResourceLoader.Load<PackedScene>("res://addons/MapGeneratorPlugin/Prefabs/zone_content.tscn");
        newZoneContentPrefab = ResourceLoader.Load<PackedScene>("res://addons/MapGeneratorPlugin/Prefabs/new_zone_content.tscn");
        GenerationManager = new GenerationManager();
    }

    //GODOT OVERRIDE FUNCTIONS:
    public override void _Ready()
    {
        Singleton = this;
        //GenerateSeed();
        LoadCurrentConfig();
    }
    public override void _EnterTree()
    {
        LoadZones();
        GetNode<LineEdit>("%SeedLineEdit").Text = "12345678"; //GenerateSeed();
    }

    //CONFIGURATION FUNCTIONS:
    private void LoadCurrentConfig()
    {
        const string filePath = "res://addons/MapGeneratorPlugin/currentConfig.json";
        Variant v = JsonConfigIO.Load(filePath);
        RestoreConfigFromVariant(v);
    }
    private void SaveCurrentConfig()
    {
        const string filePath = "res://addons/MapGeneratorPlugin/currentConfig.json";
        var data = BuildCurrentConfigDictionary();
        JsonConfigIO.Save(filePath, data);
    }
    public void ResetCurrentConfig()
    {
        dimension = 0;
        GetNode<LineEdit>("%BlockSizingLineEdit").Text = "1";
        GetNode<LineEdit>("%wideLineEdit").Text = "";
        GetNode<LineEdit>("%deepLineEdit").Text = "";
        GetNode<OptionButton>("%TileTypeOptionButton").Selected = 0;
        GetNode<LineEdit>("%SeedLineEdit").Text = "12345678";

        GenerationManager = new GenerationManager();
        for (int i = 0; i < GenerationManager.GetZoneCount(); i++)
            GenerationManager.DeleteZone(i);

        int[,] table = new int[10, 10];
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
                table[y, x] = -1;
        }
        GetNode<ZoneDistribution>("%ZoneContainer/ZoneDistribution").SetCoherenceTable(table);

        LoadZones();

        SaveCurrentConfig();
    }
    public void SaveConfigAs()
    {
        JsonConfigIO.ShowSaveDialog(BuildCurrentConfigDictionary);
    }
    public void LoadConfigFromDialog()
    {
        JsonConfigIO.ShowLoadDialog(RestoreConfigFromVariant);
    }
    private Variant BuildCurrentConfigDictionary()
    {
        var data = new Godot.Collections.Dictionary<string, Variant>
        {
            ["Dimension"] = (int)dimension,
            ["BlockSize"] = GetNode<LineEdit>("%BlockSizingLineEdit").Text,
            ["ChunkWide"] = GetNode<LineEdit>("%wideLineEdit").Text,
            ["ChunkDeep"] = GetNode<LineEdit>("%deepLineEdit").Text,
            ["TileType"] = GetNode<OptionButton>("%TileTypeOptionButton").Selected,
            ["Seed"] = GetNode<LineEdit>("%SeedLineEdit").Text
        };

        var zonesArr = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>();
        foreach (var z in GenerationManager.GetZones())
            zonesArr.Add(z.ToJson());
        data["Zones"] = zonesArr;

        int[,] table = GetNode<ZoneDistribution>("%ZoneContainer/ZoneDistribution").GetCoherenceTable();
        var outer = new Godot.Collections.Array<Godot.Collections.Array<int>>();
        for (int y = 0; y < table.GetLength(0); y++)
        {
            var row = new Godot.Collections.Array<int>();
            for (int x = 0; x < table.GetLength(1); x++)
                row.Add(table[y, x]);
            outer.Add(row);
        }
        data["CoherenceTable"] = outer;

        return data;
    }
    private void RestoreConfigFromVariant(Variant v)
    {
        if (v.VariantType != Variant.Type.Dictionary) return;
        var data = (Godot.Collections.Dictionary<string, Variant>)v;

        dimension = (Dimensions)(int)data["Dimension"];
        GetNode<LineEdit>("%BlockSizingLineEdit").Text = data["BlockSize"].AsString();
        GetNode<LineEdit>("%wideLineEdit").Text = data["ChunkWide"].AsString();
        GetNode<LineEdit>("%deepLineEdit").Text = data["ChunkDeep"].AsString();
        GetNode<OptionButton>("%TileTypeOptionButton").Selected = (int)data["TileType"];
        GetNode<LineEdit>("%SeedLineEdit").Text = data["Seed"].AsString();

        GenerationManager = new GenerationManager();
        var zonesArray = (Godot.Collections.Array)data["Zones"];
        foreach (Godot.Collections.Dictionary<string, Variant> z in zonesArray)
            GenerationManager.AddZone(Zone.JsonToZone(z));

        var outer = (Godot.Collections.Array)data["CoherenceTable"];
        int h = outer.Count, w = h > 0 ? ((Godot.Collections.Array)outer[0]).Count : 0;
        int[,] table = new int[h, w];
        for (int y = 0; y < h; y++)
        {
            var row = (Godot.Collections.Array)outer[y];
            for (int x = 0; x < w; x++)
                table[y, x] = row[x].AsInt32();
        }
        GetNode<ZoneDistribution>("%ZoneContainer/ZoneDistribution").SetCoherenceTable(table);

        LoadZones();
    }

    //BUTTONS FUNCTIONS:
    public void GenerateButtonPressed()
    {
        //Debug.Print("Hello Scene!");
        //generationManager = new GenerationManager();

        //generationManager.blockScale = 
        SaveCurrentConfig();

        string blockSize = GetNode<LineEdit>("%BlockSizingLineEdit").Text;
        string blockSizeName = "\"Block Size Size\"";
        if (blockSize == "") 
        {
            Debug.Print(blockSizeName + " Empty");
            return;
        }
        if (! blockSize.IsValidFloat())
        {
            Debug.Print(blockSizeName + " Not valid number");
            return;
        }
        if (blockSize.ToFloat() < 0)
        {
            Debug.Print(blockSizeName + " Has to be positive");
            return;
        }

        string wide = GetNode<LineEdit>("%wideLineEdit").Text;
        string deep = GetNode<LineEdit>("%deepLineEdit").Text;

        string wideName = "\"Chunk Wide\"";
        string deepName = "\"Deep Wide\"";

        if (wide == "" || deep == "")
        {
            Debug.Print(deepName + " or " + wideName + " Empty");
            return;
        }
        if (!wide.IsValidInt() || !deep.IsValidInt())
        {
            Debug.Print(deepName + " or " + wideName + " Not valid number");
            return;
        }
        if (wide.ToInt() < 0 || deep.ToInt() < 0)
        {
            Debug.Print(deepName + " or " + wideName + " Has to be positive");
            return;
        }

        if (GenerationManager.GetZoneCount() <= 0)
        {
            Debug.Print("At least 1 zone has to be created.");
            return;
        }

        int tileType = GetNode<OptionButton>("%TileTypeOptionButton").Selected;
        
        string seedText = GetNode<LineEdit>("%SeedLineEdit").Text;
        if (seedText == "")
        {
            Debug.Print("Seed is Empty");
            return;
        }
        if (!uint.TryParse(seedText, out uint seed))
        {
            Debug.Print("Seed is Not valid number");
            return;
        }
        
        GenerationManager.Start(blockSize.ToFloat(), new Vector2I(wide.ToInt(), deep.ToInt()), (Tile.TileType)tileType, seed, GetNode<ZoneDistribution>("%ZoneContainer/ZoneDistribution").GetCoherenceTable());
    }

    public void DimensionButtonToggle(bool toggled)
    {
        dimension = (Dimensions)Convert.ToInt32(toggled);
        Debug.Print(dimension.ToString());
        SaveCurrentConfig();
    }

    public void RefreshButtonPressed()
    {
        SaveCurrentConfig();
        editorInterface.SetPluginEnabled("MapGeneratorPlugin", false);
        editorInterface.SetPluginEnabled("MapGeneratorPlugin", true);
    }

    public void PlusZoneButtonPressed()
    {
        if (!newZoneActive && !editedZoneActive)
        {
            newZoneActive = true;
            LoadZones();
        }
        else
        {
            Debug.Print("Please, save the other zone.");
        }
    }

    public void EditZone(int index)
    {
        if (!newZoneActive && !editedZoneActive)
        {
            editedZoneActive = true;
            LoadZones(index);
        }
        else
        {
            Debug.Print("Please, save the other zone.");
        }
    }

    public void SaveZone(string name, Color color, int index = -1)
    {
        if (index == -1)
        {
            GenerationManager.AddZone(name, color);
        }
        else
        {
            GenerationManager.EditZone(index, name, color);
        }
        newZoneActive = false;
        editedZoneActive = false;

        LoadZones();
        SaveCurrentConfig();
    }

    public void DeleteZone(int index = -1)
    {
        if (newZoneActive)
        {
            newZoneActive = false;
            LoadZones();
            return;
        }
        if (editedZoneActive)
        {
            editedZoneActive = false;
            LoadZones(index);
            return;
        }
        LoadZones();
        SaveCurrentConfig();
    }

    private void LoadZones(int index = -1)
    {
        if ( index == -1 && editedZoneActive)
        {
            Debug.Print("Error, pass an index");
            return;
        }
        var zones = GenerationManager.GetZones();

        Label noZonesLabel = GetNode<Label>("%ZoneContainer/MarginContainer/EmptyLabel");
        Control zoneList = GetNode<Control>("%ZoneContainer/MarginContainer/ScrollContainer/ZoneList");

        if (zones.Length > 0 || newZoneActive)
        {
            noZonesLabel.Visible = false;
            GetNode<ScrollContainer>("%ZoneContainer/MarginContainer/ScrollContainer").CustomMinimumSize = new Vector2(0, zoneContentPrefab.Instantiate<Control>().Size.Y);

            if (zoneList != null)
            {
                foreach (Node child in zoneList.GetChildren())
                {
                    zoneList.RemoveChild(child);
                    child.QueueFree();
                }

                zoneList.GetParent<Control>().CustomMinimumSize = new Vector2(0, 200);
                Label emptyLabel = GetNode<Label>("%ZoneContainer/MarginContainer/EmptyLabel");
                if (emptyLabel != null)
                {
                    emptyLabel.Visible = false;
                }

                //zoneList = GetNode<Control>("%ZoneContainer/MarginContainer/ScrollContainer/ZoneList");

                if (newZoneActive)
                {
                    var editZone = newZoneContentPrefab.Instantiate();
                    editZone.GetNode<NewZoneContent>(".").Initialize(this);
                    zoneList.AddChild(editZone);
                    editZone.Owner = zoneList.Owner;
                }

                for (int i = 0; i < zones.Length; i++)
                {
                    if (editedZoneActive && i == index)
                    {
                        var editZone = newZoneContentPrefab.Instantiate();
                        editZone.GetNode<NewZoneContent>(".").Initialize(this);
                        zoneList.AddChild(editZone);
                        editZone.Owner = zoneList.Owner;

                        editZone.GetNode<NewZoneContent>(".").Edit(zones[i].GetZoneName(), zones[i].GetColor());
                    }
                    else
                    {
                        var zoneContent = zoneContentPrefab.Instantiate();
                        zoneContent.GetNode<ZoneContent>(".").Initialize(this);
                        zoneList.AddChild(zoneContent);
                        zoneContent.Owner = zoneList.Owner;

                        zoneContent.GetNode<ZoneContent>(".").SetElements(zones[i].GetZoneName(), zones[i].GetColor());
                    }
                }
            }
        }
        else
        {
            if (zoneList != null)
            {
                zoneList.GetParent<Control>().CustomMinimumSize = new Vector2(0, 0);
                foreach (Node child in zoneList.GetChildren())
                {
                    zoneList.RemoveChild(child);
                    child.QueueFree();
                }
            }
            noZonesLabel.Visible = true;
        }
        //Debug.Print(GetNode<Control>("%ZoneList").GetChildCount().ToString());
    }

    public void GenerateSeed()
    {
        GetNode<LineEdit>("%SeedLineEdit").Text = GenerationManager.GenerateSeed().ToString();
        SaveCurrentConfig();
    }

    public void CoherenceTableButtonPressed()
    {
        GetNode<Control>("%ZoneContainer/MarginContainer").Visible = false;
        GetNode<Control>("%ZoneContainer/CoherenceTableButton").Visible = false;

        var zoneDistribution = GetNode<ZoneDistribution>("%ZoneContainer/ZoneDistribution");
        zoneDistribution.Visible = true;
        zoneDistribution.Initialize(this);
    }

    public void SaveCoherenceTable(int[,] paintMap)
    {
        //GD.Print("=== SAVE GRID ===");
        //for (int y = 0; y < 10; y++)
        //{
        //    string row = "";
        //    for (int x = 0; x < 10; x++)
        //        row += $"{paintMap[y, x],2} ";
        //    GD.Print(row);
        //}
        //GD.Print("=================");

        GetNode<Control>("%ZoneContainer/MarginContainer").Visible = true;
        GetNode<Control>("%ZoneContainer/CoherenceTableButton").Visible = true;
        GetNode<ZoneDistribution>("%ZoneContainer/ZoneDistribution").Visible = false;
        SaveCurrentConfig();
    }
}

