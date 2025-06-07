using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class CoherenceTable : PaintGrid
{
    int gridSize = 10;
    int selectedZoneIdx = 0;
    Color[] colors;

    public int[,] GetCoherenceTable()
    {
        int numChildren = GetChildCount();
        int[,] result = new int[10, 10];

        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i < 10; i++)
            {
                result[j, i] = GetChild(j*10 + i).GetNode<ColorRectPrefab>(".").GetIdx();
            }
        }

        return result;
    }

    public void SetCoherenceTable(int[,] coherenceTable, Color[] colors)
    {
        this.colors = colors;
        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i < 10; i++)
            {
                var child = GetChild(j * 10 + i).GetNode<ColorRectPrefab>(".");
                if (coherenceTable[j, i] == -1)
                    child.Clear();
                else
                    child.SetData(colors[coherenceTable[j, i]], coherenceTable[j, i]);
            }
        }
    }
}