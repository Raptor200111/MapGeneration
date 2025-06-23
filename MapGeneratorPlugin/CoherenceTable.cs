using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[Tool]
public partial class CoherenceTable : PaintGrid
{
    int gridSize = 10;
    int selectedZoneIdx = 0;

    public Godot.Collections.Array<Godot.Collections.Array<int>> GetCoherenceTable()
    {
        int numChildren = GetChildCount();
        var result = new Godot.Collections.Array<Godot.Collections.Array<int>>();

        for (int j = 0; j < 10; j++)
        {
            result.Add(new Godot.Collections.Array<int>());
            for (int i = 0; i < 10; i++)
            {
                result[j].Add(GetChild(j*10 + i).GetNode<ColorRectPrefab>(".").GetIdx());
            }
        }

        return result;
    }

    public void SetCoherenceTable(int[][] coherenceTable)
    {
        var colors = GetNode<ZoneDistribution>("../../..").GetZonesColors();
        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i < 10; i++)
            {
                var child = GetChild(j * 10 + i).GetNode<ColorRectPrefab>(".");
                if (coherenceTable[j][i] == -1)
                    child.Clear();
                else
                    child.SetData(colors[coherenceTable[j][i]], coherenceTable[j][i]);
            }
        }
    }
}