using Godot;
using System;

[Tool]
public partial class HeightOverride : PaintGrid
{
    public int[] GetHeightOverride()
    {
        int numChildren = GetChildCount();
        int[] result = new int[10];

        for (int i = 0; i < 10; i++)
        {
            result[i] = GetChild(i).GetNode<ColorRectPrefab>(".").GetIdx();
        }
        
        return result;
    }

    public void SetHeightOverride(int[] heightOverride, Color[] colors)
    {
        this.colors = colors;
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
