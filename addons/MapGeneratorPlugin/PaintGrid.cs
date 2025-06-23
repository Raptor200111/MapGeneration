using Godot;
using System;
using System.Collections.Generic;

[Tool]
public abstract partial class  PaintGrid : GridContainer
{
    int selectedZoneIdx = 0;

    public Color GetColorFromSelectedZone()
    {
        return GetNode<ZoneDistribution>("../../..").GetZonesColors()[selectedZoneIdx];
    }

    public int GetSelectedZoneIdx()
    {
        return selectedZoneIdx;
    }

    public void GetData(out Color color, out int selectedZoneIdx)
    {
        selectedZoneIdx = this.selectedZoneIdx;
        color = GetColorFromSelectedZone();
    }

    public void SetSelectedZoneIdx(int idx)
    {
        selectedZoneIdx = idx;
    }

    public void Clear()
    {
        foreach (ColorRectPrefab crp in GetChildren())
        {
            crp.Clear();
        }
    }

    public void RemoveZone(int index)
    {
        var colors = GetNode<ZoneDistribution>("../../..").GetZonesColors();
        foreach (var child in GetChildren())
        {
            if (child is ColorRectPrefab colorRectPrefab)
            {
                var aux = colorRectPrefab.GetIdx();
                if (aux == index)
                    colorRectPrefab.Clear();
                else if (aux > index)
                    colorRectPrefab.SetData(colors[aux - 1], aux - 1);
            }
        }
    }
}