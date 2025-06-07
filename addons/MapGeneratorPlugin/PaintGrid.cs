using Godot;
using System;

[Tool]
public partial class PaintGrid : GridContainer
{
    int selectedZoneIdx = 0;
    protected Color[] colors;

    public void SetColors(Color[] colors)
    {
        this.colors = colors;
    }

    public Color GetColorFromSelectedZone()
    {
        if (colors == null)
        {
            colors = GetNode<ZoneDistribution>("../../..").GetZonesColors();
        }
        return colors[selectedZoneIdx];
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
}