using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class ZoneDistribution : Control
{
    private DockInterfaceManager _dockInterfaceManager;

    private readonly Color selectedColor = Colors.MediumVioletRed;
    private readonly Color deselectedColor = Colors.Black;

    private readonly int selectedBorderWidth = 4;
    private readonly int deselectedBorderWidth = 2;

    private Color[] _zonesColors;

    private int _currentZoneIdx = 0;
    private bool _threeDee = false;

    private StyleBoxFlat emptySB = new StyleBoxFlat
    {
        DrawCenter = false,
        BorderWidthTop = 1,
        BorderWidthBottom = 1,
        BorderWidthLeft = 1,
        BorderWidthRight = 1,
        BorderColor = new Color("#666")
    };

    [Export] NodePath zonesHBoxPath;
    [Export] NodePath coherenceTablePath;
    [Export] NodePath heightOverridePath;
    [Export] NodePath heightOverrideNumbersPath;


    private HBoxContainer _zonesHBox;
    private CoherenceTable _coherenceTable;
    private HeightOverride _heightOverride;
    private Control _heightOverrideNumbers;

    public override void _Ready()
    {
        _zonesHBox = GetNode<HBoxContainer>(zonesHBoxPath);
        _coherenceTable = GetNode<CoherenceTable>(coherenceTablePath);
        _heightOverride = GetNode<HeightOverride>(heightOverridePath);
        _heightOverrideNumbers = GetNode<Control>(heightOverrideNumbersPath);

        _zonesColors = GetZonesColors();
        SpawnZoneButtons();


        //SpawnGridButtons();

        //int minimumSize = (int)BTN_SIZE.Y * (GRID_SIZE + 6);
        //this.CustomMinimumSize = new Vector2(0, minimumSize);
    }

    public void Initialize(DockInterfaceManager dockInterfaceManager, int[,] table, bool threeDee, int[] heightOverride)
    {
        _dockInterfaceManager = MapGeneratorPlugin.Singleton;
        _zonesColors = GetZonesColors();
        SpawnZoneButtons();
        _coherenceTable.SetCoherenceTable(table, _zonesColors);
        _threeDee = threeDee;
        if (_threeDee == true)
        { 
            _heightOverride.SetHeightOverride(heightOverride, _zonesColors);
            _heightOverride.Visible = _heightOverrideNumbers.Visible = true;
        }
        else
        {
            _heightOverride.Visible = _heightOverrideNumbers.Visible = false;
        }
    }

    public void Show(bool threeDee)
    {
        _threeDee = threeDee;
        _heightOverride.Visible = _heightOverrideNumbers.Visible = _threeDee;
    }

    public Color[] GetZonesColors()
    {
        if (_dockInterfaceManager == null)
            return new Color[0];
        var zones = _dockInterfaceManager.GenerationManager.GetZones();

        Color[] colors = new Color[zones.Length];
        for (int i = 0; i < zones.Length; i++)
            colors[i] = zones[i].GetColor();

        return colors;
    }

    private void SpawnZoneButtons()
    {
        if(_zonesHBox.GetChildCount() != 0)
        {
            var children = _zonesHBox.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                _zonesHBox.RemoveChild(children[i]);
                children[i].QueueFree();
            }
        }

        for (int idx = 0; idx < _zonesColors.Length; idx++)
        {
            var zBtn = new Button
            {
                CustomMinimumSize = new Vector2(20, 20),
                ToggleMode = true,
                ButtonPressed = idx == _currentZoneIdx
            };

            if (idx == _currentZoneIdx)
            {
                var sb = new StyleBoxFlat
                {
                    BgColor = _zonesColors[idx],
                    
                    BorderWidthTop = selectedBorderWidth,
                    BorderWidthBottom = selectedBorderWidth,
                    BorderWidthLeft = selectedBorderWidth,
                    BorderWidthRight = selectedBorderWidth,

                    BorderColor = selectedColor
                }
                ;
                zBtn.AddThemeStyleboxOverride("normal", sb);
                zBtn.AddThemeStyleboxOverride("hover", sb);
                zBtn.AddThemeStyleboxOverride("pressed", sb);
            }
            else { 
                var sb = new StyleBoxFlat
                {
                    BgColor = _zonesColors[idx],

                    BorderWidthTop = deselectedBorderWidth,
                    BorderWidthBottom = deselectedBorderWidth,
                    BorderWidthLeft = deselectedBorderWidth,
                    BorderWidthRight = deselectedBorderWidth,

                    BorderColor = deselectedColor
                };
                zBtn.AddThemeStyleboxOverride("normal", sb);
                zBtn.AddThemeStyleboxOverride("hover", sb);
                zBtn.AddThemeStyleboxOverride("pressed", sb);
            }

            int fixedIdx = idx;
            zBtn.Pressed += () => OnZoneButtonPressed(fixedIdx, zBtn);

            _zonesHBox.AddChild(zBtn);
        }
    }

    private void OnZoneButtonPressed(int idx, Button btn)
    {
        Color color;
        if (_zonesHBox.GetChildOrNull<Button>(_currentZoneIdx) is { } prev)
        {
            color = _zonesColors[_currentZoneIdx];

            var filledSB = new StyleBoxFlat
            {
                BgColor = color,
                BorderWidthTop = deselectedBorderWidth,
                BorderWidthBottom = deselectedBorderWidth,
                BorderWidthLeft = deselectedBorderWidth,
                BorderWidthRight = deselectedBorderWidth,
                BorderColor = deselectedColor
            };

            prev.AddThemeStyleboxOverride("normal", filledSB);
            prev.AddThemeStyleboxOverride("hover", filledSB);
            prev.AddThemeStyleboxOverride("pressed", filledSB);

            prev.ButtonPressed = false;
        }

        
        color = _zonesColors[idx];

        var selectedSB = new StyleBoxFlat
        {
            BgColor = color,
            BorderWidthTop = selectedBorderWidth,
            BorderWidthBottom = selectedBorderWidth,
            BorderWidthLeft = selectedBorderWidth,
            BorderWidthRight = selectedBorderWidth,
            BorderColor = selectedColor
        };

        btn.AddThemeStyleboxOverride("normal", selectedSB);
        btn.AddThemeStyleboxOverride("hover", selectedSB);
        btn.AddThemeStyleboxOverride("pressed", selectedSB);

        _currentZoneIdx = idx;
        _coherenceTable.SetSelectedZoneIdx(_currentZoneIdx);
        if (_threeDee)
            _heightOverride.SetSelectedZoneIdx(_currentZoneIdx);
        btn.ButtonPressed = true;
    }

    private void OnSavePressed()
    {
        _dockInterfaceManager.SaveZoneDistribution();
    }

    public void Clear()
    {
        _coherenceTable.Clear();
        if (_threeDee)
            _heightOverride.Clear();
    }

    internal int[,] GetCoherenceTable()
    {
        return _coherenceTable.GetCoherenceTable();
    }

    internal int[] GetHeightOverride()
    {
        return _heightOverride.GetHeightOverride();
    }
}