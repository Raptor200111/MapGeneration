using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class ZoneDistribution : Control
{
    private DockInterfaceManager _dockInterfaceManager;

    private const int GRID_SIZE = 10;
    private static readonly Vector2 BTN_SIZE = new(32, 32);

    private readonly Color selectedColor = Colors.MediumVioletRed;
    private readonly Color deselectedColor = Colors.Black;

    private readonly int selectedBorderWidth = 4;
    private readonly int deselectedBorderWidth = 2;

    private Color[] _zones =
    {
        new Color("#DC143C"),
        new Color("#228B22"),
        new Color("#1E90FF"),
        new Color("#FFA500"),
        new Color("#9400D3")
    };

    private int _currentZoneIdx = 0;
    private int[,] _paintMap = new int[GRID_SIZE, GRID_SIZE];

    private StyleBoxFlat emptySB = new StyleBoxFlat
    {
        DrawCenter = false,
        BorderWidthTop = 1,
        BorderWidthBottom = 1,
        BorderWidthLeft = 1,
        BorderWidthRight = 1,
        BorderColor = new Color("#666")
    };

    private HBoxContainer _zonesHBox;
    private GridContainer _gridRef;
    private Button _btnSave;

    public void Initialize(DockInterfaceManager dockInterfaceManager)
    {
        _dockInterfaceManager = dockInterfaceManager;
        _zones = GetZonesColors();
        SpawnZoneButtons();
        PaintGrid();
    }

    public override void _Ready()
    {
        for (int y = 0; y < GRID_SIZE; y++)
            for (int x = 0; x < GRID_SIZE; x++)
                _paintMap[y, x] = -1;

        _zonesHBox = GetNode<HBoxContainer>("VBox_UI/HBox_Zones");
        _gridRef = GetNode<GridContainer>("VBox_UI/Grid");
        _btnSave = GetNode<Button>("VBox_UI/Btn_Save");

        /*
        _zones = GetZonesColors();
        SpawnZoneButtons();
        */

        SpawnGridButtons();

        //int minimumSize = (int)BTN_SIZE.Y * (GRID_SIZE + 6);
        //this.CustomMinimumSize = new Vector2(0, minimumSize);

        _btnSave.Pressed += OnSavePressed;
    }

    private Color[] GetZonesColors()
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

        for (int idx = 0; idx < _zones.Length; idx++)
        {
            var zBtn = new Button
            {
                CustomMinimumSize = BTN_SIZE,
                ToggleMode = true,
                ButtonPressed = idx == _currentZoneIdx
            };

            if (idx == _currentZoneIdx)
            {
                var sb = new StyleBoxFlat
                {
                    BgColor = _zones[idx],
                    
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
                    BgColor = _zones[idx],

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

        var delBtn = new Button
        {
            CustomMinimumSize = BTN_SIZE,
            ToggleMode = true
        };

        delBtn.Icon = GetThemeIcon("GuiClose", "EditorIcons");
        delBtn.ExpandIcon = true;
        delBtn.AddThemeConstantOverride("icon_max_width", (int)BTN_SIZE.X * 4);

        var delStyle = new StyleBoxFlat
        {
            BgColor = Colors.Transparent,

            BorderWidthTop = deselectedBorderWidth,
            BorderWidthBottom = deselectedBorderWidth,
            BorderWidthLeft = deselectedBorderWidth,
            BorderWidthRight = deselectedBorderWidth,

            BorderColor = deselectedColor,
        };

        delBtn.AddThemeStyleboxOverride("normal", delStyle);
        delBtn.AddThemeStyleboxOverride("hover", delStyle);
        delBtn.AddThemeStyleboxOverride("pressed", delStyle);

        delBtn.AddThemeColorOverride("font_color", Colors.Red);
        delBtn.AddThemeColorOverride("font_color_hover", Colors.Red);
        delBtn.AddThemeColorOverride("font_color_pressed", Colors.Red);
        
        delBtn.Pressed += () => OnZoneButtonPressed(_zones.Length, delBtn);

        _zonesHBox.AddChild(delBtn);
    }

    private void SpawnGridButtons()
    {
        
        for (int y = 0; y < GRID_SIZE; y++)
        {
            Label coma = new Label
            {
                Text = "0." + (GRID_SIZE - y - 1).ToString(),
                CustomMinimumSize = new Vector2(0, BTN_SIZE.Y / 2),
            };
            coma.AddThemeFontSizeOverride("font_size", 15);
            _gridRef.AddChild(coma);

            for (int x = 0; x < GRID_SIZE; x++)
            {
                var gBtn = new Button
                {
                    CustomMinimumSize = BTN_SIZE
                };

                gBtn.AddThemeStyleboxOverride("normal", emptySB);
                gBtn.AddThemeStyleboxOverride("hover", emptySB);
                gBtn.AddThemeStyleboxOverride("pressed", emptySB);

                int cx = x, cy = y;
                gBtn.Pressed += () => OnGridButtonPressed(cx, cy, gBtn);

                _gridRef.AddChild(gBtn);
            }
        }
        _gridRef.AddChild(new Label
        {
            Text = "",
            CustomMinimumSize = new Vector2(0, BTN_SIZE.Y / 2)
        });

        for (int i = 0; i < GRID_SIZE; i++)
        {

            Label coma = new Label
            {
                Text = "0." + (i).ToString(),
                CustomMinimumSize = new Vector2(0, BTN_SIZE.Y / 2),
            };
            coma.AddThemeFontSizeOverride("font_size", 15);
            _gridRef.AddChild(coma);
        }
    }

    private void OnZoneButtonPressed(int idx, Button btn)
    {
        Color color;
        if (_zonesHBox.GetChildOrNull<Button>(_currentZoneIdx) is { } prev)
        {
            if (_currentZoneIdx == _zones.Length)
            {
                color = Colors.Transparent;
            }
            else
            {
                color = _zones[_currentZoneIdx];
            }

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

        
        if (idx == _zones.Length)
        {
            color = Colors.Transparent;
        }
        else
        {
            color = _zones[idx];
        }

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
        btn.ButtonPressed = true;
    }

    private void OnGridButtonPressed(int x, int y, Button gBtn)
    {
        if (_currentZoneIdx == _zones.Length)
        {
            gBtn.AddThemeStyleboxOverride("normal", emptySB);
            gBtn.AddThemeStyleboxOverride("hover", emptySB);
            gBtn.AddThemeStyleboxOverride("pressed", emptySB);
            _paintMap[y, x] = -1;
            return;
        }

        var filledSB = new StyleBoxFlat
        {
            BgColor = _zones[_currentZoneIdx],
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderColor = new Color("#DDD")
        };

        gBtn.AddThemeStyleboxOverride("normal", filledSB);
        gBtn.AddThemeStyleboxOverride("hover", filledSB);
        gBtn.AddThemeStyleboxOverride("pressed", filledSB);

        _paintMap[y, x] = _currentZoneIdx;
        //Debug.Print("0." + x.ToString() + " - 0." + (9 - y).ToString());
    }

    private void PaintGrid()
    {
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 1; x < GRID_SIZE + 1; x++)
            {
                var gBtn = _gridRef.GetChild<Button>(y * (GRID_SIZE+1) + x);
                if (_paintMap[y, x-1] == -1)
                {
                    gBtn.AddThemeStyleboxOverride("normal", emptySB);
                    gBtn.AddThemeStyleboxOverride("hover", emptySB);
                    gBtn.AddThemeStyleboxOverride("pressed", emptySB);
                }
                else
                {
                    var filledSB = new StyleBoxFlat
                    {
                        BgColor = _zones[_paintMap[y, x-1]],
                        BorderWidthTop = 1,
                        BorderWidthBottom = 1,
                        BorderWidthLeft = 1,
                        BorderWidthRight = 1,
                        BorderColor = new Color("#DDD")
                    };
                    gBtn.AddThemeStyleboxOverride("normal", filledSB);
                    gBtn.AddThemeStyleboxOverride("hover", filledSB);
                    gBtn.AddThemeStyleboxOverride("pressed", filledSB);
                }
            }
        }
    }

    private void OnSavePressed()
    {
        _dockInterfaceManager.SaveCoherenceTable(_paintMap);
    }

    public int[,] GetCoherenceTable()
    {
        return _paintMap;
    }

    internal void SetCoherenceTable(int[,] table)
    {
        _paintMap = table;
        PaintGrid();
    }

    public void ClearCoherenceTable()
    {
        for (int y = 0; y < GRID_SIZE; y++)
            for (int x = 0; x < GRID_SIZE; x++)
                _paintMap[y, x] = -1;
        PaintGrid();
    }
}