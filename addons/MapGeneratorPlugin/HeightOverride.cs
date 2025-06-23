using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class HeightOverride : PaintGrid
{
    public Godot.Collections.Array<int> GetHeightOverride()
    {
        int numChildren = GetChildCount();
        var result = new Godot.Collections.Array<int>();

        for (int i = 0; i < 10; i++)
        {
            result.Add(GetChild(i).GetNode<ColorRectPrefab>(".").GetIdx());
        }
        
        return result;
    }

    public void SetHeightOverride(int[] heightOverride)
    {
        var colors = GetNode<ZoneDistribution>("../../..").GetZonesColors();
        for (int i = 0; i < 10; i++)
        {
            var child = GetChild(i).GetNode<ColorRectPrefab>(".");
            if (heightOverride[i] == -1)
                child.Clear();
            else
                child.SetData(colors[heightOverride[i]], heightOverride[i]);
        }
        
    }
}