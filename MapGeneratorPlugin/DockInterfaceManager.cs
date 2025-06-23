using Godot;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

[Tool]
public partial class DockInterfaceManager : ScrollContainer
{
    [Export] PackedScene zoneContentPrefab;
    [Export] PackedScene newZoneContentPrefab;
    //[Export] PackedScene zoneCoherencePrefab;

    [Export] NodePath dinamicSwitchPath;
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
    [Export] NodePath frequency2DSpinBoxPath;
    [Export] NodePath frequency3DSpinBoxPath;

    CheckButton _dinamicSwitch;
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
    SpinBox _frequency2DSpinBox;
    SpinBox _frequency3DSpinBox;

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
        _dinamicSwitch = GetNode<CheckButton>(dinamicSwitchPath);
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
        _frequency2DSpinBox = GetNode<SpinBox>(frequency2DSpinBoxPath);
        _frequency3DSpinBox = GetNode<SpinBox>(frequency3DSpinBoxPath);

        LoadCurrentConfig();
    }

    //CONFIGURATION FUNCTIONS:
    private void SetConfigInfo()
    {
        ConfigInfo configInfo = GenerationManager.configInfo;
        _dinamicSwitch.ButtonPressed = configInfo.DinamicWorld;
        _dimesionSwitch.ButtonPressed = configInfo.ThreeDee;
        _blockSizingLineEdit.Text = configInfo.BlockSize.ToString();
        _wideLineEdit.Text = configInfo.ChunkWide.ToString();
        _deepLineEdit.Text = configInfo.ChunkDeep.ToString();
        _tileTypeOptionButton.Selected = configInfo.TileType;
        _seedLineEdit.Text = configInfo.Seed.ToString();
        _frequency2DSpinBox.Value = configInfo.Freq2D;
        _frequency3DSpinBox.Value = configInfo.Freq3D;

        int[][] coherenceTable = configInfo.CoherenceTable
            .Select(innerArray => innerArray.ToArray())
            .ToArray();

        _zoneDistribution.Initialize(this, coherenceTable, configInfo.HeightOverride.ToArray());
        
        LoadZones();
    }

    public void ResetCurrentConfig()
    {
        GenerationManager = new GenerationManager();

        SetConfigInfo();
        SaveCurrentConfig();
    }
    private void LoadCurrentConfig()
    {
        GenerationManager.LoadCurrentConfig();
        SetConfigInfo();
    }

    private void SaveCurrentConfig()
    {
        GenerationManager.SaveCurrentConfig();
    }

    //BUTTON INPUTS

    public void RefreshButtonPressed()
    {
        SaveCurrentConfig();
        EditorInterface.Singleton.SetPluginEnabled("MapGeneratorPlugin", false);
        EditorInterface.Singleton.SetPluginEnabled("MapGeneratorPlugin", true);
    }

    public void SaveConfigAs()
    {
        JsonConfigIO.ShowSaveDialog(GenerationManager.BuildCurrentConfigDictionary);
    }

    public void LoadConfigFromDialog()
    {
        JsonConfigIO.ShowLoadDialog(RestoreConfigFromVariant);
    }

    private void RestoreConfigFromVariant(Variant v)
    {
        GenerationManager.RestoreConfigFromVariant(v);
        SetConfigInfo();
    }

    public void StaticButtonToggle(bool toggled)
    {
        GenerationManager.configInfo.DinamicWorld = toggled;
        SaveCurrentConfig();
    }
    public void DimensionButtonToggle(bool toggled)
    {
        GenerationManager.configInfo.ThreeDee = toggled;
        SaveCurrentConfig();
    }

    public void TileTypeChanged(int index)
    {
        GenerationManager.configInfo.TileType = index;
        SaveCurrentConfig();
    }

    public void BlockSizeChanged(string text)
    {
        if (text == "" || text == null)
        {
            GenerationManager.configInfo.BlockSize = 1.0f;
            SaveCurrentConfig();
        }
        else if (text.IsValidFloat())
        {
            GenerationManager.configInfo.BlockSize = text.ToFloat();
            SaveCurrentConfig();
        }
        else
        {
            Debug.Print("Block Size Not valid number");
        }
    }

    public void WideChanged(string text)
    {
        if (text == "" || text == null)
        {
            GenerationManager.configInfo.ChunkWide = 100;
            SaveCurrentConfig();
        }
        else if (text.IsValidInt())
        {
            GenerationManager.configInfo.ChunkWide = text.ToInt();
            SaveCurrentConfig();
        }
        else
        {
            Debug.Print("Chunk Wide Not valid number");
        }
    }

    public void DeepChanged(string text)
    {
        if (text == "" || text == null)
        {
            GenerationManager.configInfo.ChunkDeep = 100;
            SaveCurrentConfig();
        }
        else if (text.IsValidInt())
        {   
            GenerationManager.configInfo.ChunkDeep = text.ToInt();
            SaveCurrentConfig();
        }
        else
        {
            Debug.Print("Chunk Deep Not valid number");
        }
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
            Debug.Print("Please, save the editing zone.");
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
            _zoneDistribution.AddZone();
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
            GenerationManager.DeleteZone(index);
            _zoneDistribution.DeleteZone(index);
            LoadZones(index);
            return;
        }
        LoadZones();
        SaveCurrentConfig();
    }

    private void LoadZones(int index = -1)
    {
        if (index == -1 && editedZoneActive)
        {
            Debug.Print("Error, pass an index");
            return;
        }

        if (GenerationManager.configInfo.Zones.Count > 0 || newZoneActive)
        {
            //_zoneScrollContainer.CustomMinimumSize = new Vector2(0, 10000);

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

                var zones = GenerationManager.configInfo.Zones;

                for (int i = 0; i < zones.Count; i++)
                {
                    if (editedZoneActive && i == index)
                    {
                        var editZone = newZoneContentPrefab.Instantiate();
                        editZone.GetNode<NewZoneContent>(".").Initialize(this);
                        _zoneList.AddChild(editZone);
                        editZone.Owner = _zoneList.Owner;

                        editZone.GetNode<NewZoneContent>(".").Edit(zones[i].GetZoneName(), zones[i].GetColor(),  zones[i].GetResources());
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

    public void CoherenceTableButtonPressed()
    {
        _marginContainer.Visible = false;
        _coherenceTableButton.Visible = false;
        _zoneDistribution.Visible = true;
    }

    public void SaveZoneDistribution(Godot.Collections.Array<Godot.Collections.Array<int>> coherenceTable, Godot.Collections.Array<int> heightOverride)
    {
        GenerationManager.configInfo.CoherenceTable = coherenceTable;
        GenerationManager.configInfo.HeightOverride = heightOverride;
        _marginContainer.Visible = true;
        _coherenceTableButton.Visible = true;
        _zoneDistribution.Visible = false;
        SaveCurrentConfig();
    }

    public void SeedChanged(string text)
    {
        if (text.IsValidInt())
        {
            GenerationManager.configInfo.Seed = (uint)text.ToInt();
            SaveCurrentConfig();
        }
        else
        {
            Debug.Print("Seed Not valid number");
        }
    }
    public void GenerateSeed()
    {
        uint newSeed = new RandomNumberGenerator().Randi();
        GenerationManager.configInfo.Seed = newSeed;
        _seedLineEdit.Text = newSeed.ToString();
        SaveCurrentConfig();
    }

    public void Frequency2DChanged(float value)
    {
        GenerationManager.configInfo.Freq2D = (float)_frequency2DSpinBox.Value;
        SaveCurrentConfig();
    }

    public void Frequency3DChanged(float value)
    {
        GenerationManager.configInfo.Freq3D = (float)_frequency3DSpinBox.Value;
        SaveCurrentConfig();
    }

    public void GenerateButtonPressed()
    {
        GenerationManager.Start();
    }
}