using Godot;
using System;

[Tool]
public partial class ColorRectPrefab : ColorRect
{
    bool entered = false;
    bool clickedOnce = false;
    bool cleared = false;
    //private PaintGrid _paintGrid;
    int idx = -1;

    public override void _Ready()
    {
        //_paintGrid = GetParent<PaintGrid>();
        //if (_paintGrid == null)
        //{
        //    GD.PrintErr("PaintGrid not found in ColorRectPrefab.");
        //}
        //CustomMinimumSize = new Vector2 (25, 25);
    }

    public override void _Process(double delta)
    {
        if (entered)
        {
            if (!clickedOnce)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    GetParent<PaintGrid>().GetData(out Color color, out int selectedZoneIdx);
                    SetData(color, selectedZoneIdx);
                }
                else if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    Clear();
                }
                

                clickedOnce = true;
            }
            if (!Input.IsMouseButtonPressed(MouseButton.Left) && !Input.IsMouseButtonPressed(MouseButton.Right))
            {
                clickedOnce = false;
            }
        }
    }

    public void OnEntered()
    {
        entered = true;
        Color color = this.Color;
        color.A = 0.5f;
        this.Color = color;
    }

    public void OnExited()
    {
        
        Color color = this.Color;
        color.A = 1f;
        this.Color = color;

        entered = false;
        clickedOnce = false;
    }

    public int GetIdx()
    {
        return idx;
    }

    public void Clear()
    {
        this.Color = Colors.White;
        idx = -1;
        cleared = true;
    }

    public void SetData(Color color, int idx)
    {
        this.Color = color;
        this.idx = idx;
        //this.AddThemeStyleboxOverride("", new StyleBoxFlat());
        this.RemoveThemeStyleboxOverride("empty");
        cleared = false;
    }
}
