
using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using TFG_Godot.Properties;

[Tool]
public partial class DockInterfaceManager : Control
{
    bool threeDee = false;
    [Export] PackedScene zoneContentPrefab;
    [Export] PackedScene newZoneContentPrefab;
    //[Export] PackedScene zoneCoherencePrefab;

    [Export] NodePath dimesionSwitchPath;
    [Export] NodePath zoneDistributionPath;
    [Export] NodePath blockSizingLineEditPath;
    [Export] NodePath wideLineEditPath;
    [Export] NodePath deepLineEditPath;
    [Export] NodePath tileTypeOptionButtonPath;
    [Export] NodePath seedLineEditPath;
    [Export] NodePath noZoneLabelPath;
    [Export] NodePath zoneListPath;
    [Export] NodePath zoneScrollContainerPath;
    [Export] NodePath marginContainerPath;
    [Export] NodePath coherenceTableButtonPath;

    CheckButton _dimesionSwitch;
    ZoneDistribution _zoneDistribution;
    LineEdit _blockSizingLineEdit;
    LineEdit _wideLineEdit;
    LineEdit _deepLineEdit;
    OptionButton _tileTypeOptionButton;
    LineEdit _seedLineEdit;
    Label _noZoneLabel;
    Control _zoneList;
    ScrollContainer _zoneScrollContainer;
    Control _marginContainer;
    Button _coherenceTableButton;

    bool newZoneActive = false;
    bool editedZoneActive = false;
    string loadedFilePath = "";
    public GenerationManager GenerationManager { get; private set; }
    public DockInterfaceManager()
    {
        GenerationManager = new GenerationManager();
    }

    //GODOT OVERRIDE FUNCTIONS:
    public override void _Ready()
    {
        _dimesionSwitch = GetNode<CheckButton>(dimesionSwitchPath);
        _zoneDistribution = GetNode<ZoneDistribution>(zoneDistributionPath);
        _blockSizingLineEdit = GetNode<LineEdit>(blockSizingLineEditPath);
        _wideLineEdit = GetNode<LineEdit>(wideLineEditPath);
        _deepLineEdit = GetNode<LineEdit>(deepLineEditPath);
        _tileTypeOptionButton = GetNode<OptionButton>(tileTypeOptionButtonPath);
        _seedLineEdit = GetNode<LineEdit>(seedLineEditPath);
        _noZoneLabel = GetNode<Label>(noZoneLabelPath);
        _zoneList = GetNode<Control>(zoneListPath);
        _zoneScrollContainer = GetNode<ScrollContainer>(zoneScrollContainerPath);
        _marginContainer = GetNode<Control>(marginContainerPath);
        _coherenceTableButton = GetNode<Button>(coherenceTableButtonPath);
        
        _seedLineEdit.Text = "12345678"; //GenerateSeed();
        LoadCurrentConfig();
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
        threeDee = false;
        _dimesionSwitch._Toggled(false);
        _blockSizingLineEdit.Text = "1";
        _wideLineEdit.Text = "";
        _deepLineEdit.Text = "";
        _tileTypeOptionButton.Selected = 0;
        _seedLineEdit.Text = "12345678";

        GenerationManager = new GenerationManager();
        for (int i = 0; i < GenerationManager.GetZoneCount(); i++)
            GenerationManager.DeleteZone(i);

        int[,] table = new int[10, 10];
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
                table[y, x] = -1;
        }

        int[] heightOverride = new int[10];
        for (int i = 0; i < heightOverride.Length; i++)
            heightOverride[i] = -1;

        _zoneDistribution.Initialize(this, table, threeDee, heightOverride);

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
            ["3D"] = threeDee,
            ["BlockSize"] = _blockSizingLineEdit.Text,
            ["ChunkWide"] = _wideLineEdit.Text,
            ["ChunkDeep"] = _deepLineEdit.Text,
            ["TileType"] = _tileTypeOptionButton.Selected,
            ["Seed"] = _seedLineEdit.Text,
        };

        var zonesArr = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>();
        foreach (var z in GenerationManager.GetZones())
            zonesArr.Add(z.ToJson());
        data["Zones"] = zonesArr;

        int[,] coherenceTable = _zoneDistribution.GetCoherenceTable();
        var outer = new Godot.Collections.Array<Godot.Collections.Array<int>>();
        for (int y = 0; y < coherenceTable.GetLength(0); y++)
        {
            var row = new Godot.Collections.Array<int>();
            for (int x = 0; x < coherenceTable.GetLength(1); x++)
                row.Add(coherenceTable[y, x]);
            outer.Add(row);
        }
        data["CoherenceTable"] = outer;
        data["HeightOverride"] = _zoneDistribution.GetHeightOverride();

        return data;
    }
    private void RestoreConfigFromVariant(Variant v)
    {
        if (v.VariantType != Variant.Type.Dictionary) return;
        var data = (Godot.Collections.Dictionary<string, Variant>)v;

        threeDee = (bool)data["3D"];
        //_dimesionSwitch._Toggled(threeDee);
        _dimesionSwitch.ButtonPressed = threeDee;
        _blockSizingLineEdit.Text = data["BlockSize"].AsString();
        _wideLineEdit.Text = data["ChunkWide"].AsString();
        _deepLineEdit.Text = data["ChunkDeep"].AsString();
        _tileTypeOptionButton.Selected = (int)data["TileType"];
        _seedLineEdit.Text = data["Seed"].AsString();

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

        int[] heightTable = (int[]) data["HeightOverride"];

        _zoneDistribution.Initialize(this, table, threeDee, heightTable);

        LoadZones();
    }

    //BUTTONS FUNCTIONS:
    public void GenerateButtonPressed()
    {
        //Debug.Print("Hello Scene!");
        //generationManager = new GenerationManager();

        //generationManager.blockScale = 
        SaveCurrentConfig();

        string blockSize = _blockSizingLineEdit.Text;
        string blockSizeName = "\"Block Size Size\"";
        if (blockSize == "") 
        {
            blockSize = "1"; // Default value
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

        string wide = _wideLineEdit.Text;
        string deep = _deepLineEdit.Text;

        string wideName = "\"Chunk Wide\"";
        string deepName = "\"Deep Wide\"";

        if (wide == "")
        {
            wide = "100";
        }
        if (deep == "")
        {
            deep = "100";
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

        int tileType = _tileTypeOptionButton.Selected;
        
        string seedText = _seedLineEdit.Text;
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

        GenerationManager.Start(blockSize.ToFloat(), new Vector2I(wide.ToInt(), deep.ToInt()), (Tile.TileType)tileType, seed, _zoneDistribution.GetCoherenceTable(), threeDee, _zoneDistribution.GetHeightOverride());
    }

    public void DimensionButtonToggle(bool toggled)
    {
        threeDee = (toggled);
        SaveCurrentConfig();
    }

    public void RefreshButtonPressed()
    {
        SaveCurrentConfig();
        EditorInterface.Singleton.SetPluginEnabled("MapGeneratorPlugin", false);
        EditorInterface.Singleton.SetPluginEnabled("MapGeneratorPlugin", true);
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

    public void SaveZone(string name, Color color, ResourcePathList resources, int index = -1)
    {
        if (index == -1)
        {
            GenerationManager.AddZone(name, color, resources);
        }
        else
        {
            GenerationManager.EditZone(index, name, color, resources);
        }
        newZoneActive = false;
        editedZoneActive = false;

        LoadZones();
        SaveCurrentConfig();
    }

    public void DeleteZone(int index = -1)
    {
        //remove the zone from the coherence table, and restar 1 en todos los demas.
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

        if (zones.Length > 0 || newZoneActive)
        {
            _zoneScrollContainer.CustomMinimumSize = new Vector2(0, zoneContentPrefab.Instantiate<Control>().Size.Y);

            if (_zoneList != null)
            {
                foreach (Node child in _zoneList.GetChildren())
                {
                    _zoneList.RemoveChild(child);
                    child.QueueFree();
                }

                _zoneList.GetParent<Control>().CustomMinimumSize = new Vector2(0, 200);
                _noZoneLabel.Visible = false;

                //zoneList = GetNode<Control>("%ZoneContainer/MarginContainer/ScrollContainer/ZoneList");

                if (newZoneActive)
                {
                    var editZone = newZoneContentPrefab.Instantiate();
                    editZone.GetNode<NewZoneContent>(".").Initialize(this);
                    _zoneList.AddChild(editZone);
                    editZone.Owner = _zoneList.Owner;
                }

                for (int i = 0; i < zones.Length; i++)
                {
                    if (editedZoneActive && i == index)
                    {
                        var editZone = newZoneContentPrefab.Instantiate();
                        editZone.GetNode<NewZoneContent>(".").Initialize(this);
                        _zoneList.AddChild(editZone);
                        editZone.Owner = _zoneList.Owner;

                        editZone.GetNode<NewZoneContent>(".").Edit(zones[i].GetZoneName(), zones[i].GetColor(), zones[i].GetResources());
                    }
                    else
                    {
                        var zoneContent = zoneContentPrefab.Instantiate();
                        zoneContent.GetNode<ZoneContent>(".").Initialize(this);
                        _zoneList.AddChild(zoneContent);
                        zoneContent.Owner = _zoneList.Owner;

                        zoneContent.GetNode<ZoneContent>(".").SetElements(zones[i].GetZoneName(), zones[i].GetColor());
                    }
                }
            }
        }
        else
        {
            if (_zoneList != null)
            {
                _zoneList.GetParent<Control>().CustomMinimumSize = new Vector2(0, 0);
                foreach (Node child in _zoneList.GetChildren())
                {
                    _zoneList.RemoveChild(child);
                    child.QueueFree();
                }
            }
            _noZoneLabel.Visible = true;
        }
        //Debug.Print(GetNode<Control>("%ZoneList").GetChildCount().ToString());
    }

    public void GenerateSeed()
    {
        _seedLineEdit.Text = GenerationManager.GenerateSeed().ToString();
        SaveCurrentConfig();
    }

    public void CoherenceTableButtonPressed()
    {
        _marginContainer.Visible = false;
        _coherenceTableButton.Visible = false;
        _zoneDistribution.Visible = true;

        _zoneDistribution.Show(threeDee);
    }

    public void SaveZoneDistribution()
    {
        _marginContainer.Visible = true;
        _coherenceTableButton.Visible = true;
        _zoneDistribution.Visible = false;
        SaveCurrentConfig();
    }
}

