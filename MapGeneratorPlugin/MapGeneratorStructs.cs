using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;

public struct Pos
{
    public int x { get; }
    public int y { get; }

    public Pos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    readonly static public Pos[] neighbours = { new Pos(1, 0), new Pos(0, 1), new Pos(-1, 0), new Pos(0, -1), };

    public static Pos operator +(Pos left, Pos right) => new Pos(left.x + right.x, left.y + right.y);
}

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    NorthEast = 4,
    SouthEast = 5,
    SouthWest = 6,
    NorthWest = 7
}

public struct ConfigInfo
{
    public bool ThreeDee;
    public bool DinamicWorld;
    public float BlockSize;
    public int ChunkDeep;
    public int ChunkWide;
    public Godot.Collections.Array<Godot.Collections.Array<int>> CoherenceTable;
    public Godot.Collections.Array<int> HeightOverride;
    public uint Seed;
    public int TileType;
    public List<Zone> Zones;
    public float Freq2D;
    public float Freq3D;

    public ConfigInfo()
    {
        ThreeDee = false;
        DinamicWorld = false;
        BlockSize = 1.0f;
        ChunkDeep = 100;
        ChunkWide = 100;
        CoherenceTable = new Godot.Collections.Array<Godot.Collections.Array<int>>();
        HeightOverride = new Godot.Collections.Array<int>();
        for (int i = 0; i < 10; i++)
        {
            HeightOverride.Add(-1);
            var inner = new Godot.Collections.Array<int>();
            for (int j = 0; j < 10; j++)
                inner.Add(-1);
            CoherenceTable.Add(inner);
        }
        Seed = 12345678;
        TileType = 0;
        Zones = new List<Zone>();
        Freq2D = 0.003f;
        Freq3D = 0.006f;
    }

    public Variant ToJson()
    {
        var data = new Godot.Collections.Dictionary<string, Variant>
        {
            ["3D"] = ThreeDee,
            ["DinamicWorld"] = DinamicWorld,
            ["BlockSize"] = BlockSize,
            ["ChunkWide"] = ChunkWide,
            ["ChunkDeep"] = ChunkDeep,
            ["TileType"] = TileType,
            ["HeightOverride"] = HeightOverride,
            ["Seed"] = Seed,
            ["Freq2D"] = Freq2D,
            ["Freq3D"] = Freq3D
        };

        var outer = new Godot.Collections.Array<Godot.Collections.Array<int>>();
        for (int y = 0; y < CoherenceTable.Count; y++)
        {
            var row = new Godot.Collections.Array<int>();
            for (int x = 0; x < CoherenceTable[y].Count; x++)
                row.Add(CoherenceTable[y][x]);
            outer.Add(row);
        }
        data["CoherenceTable"] = outer;

        var zonesArray = new Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>();
        for (int i = 0; i < Zones.Count; i++)
        {
            zonesArray.Add(Zones[i].ToJson());
        }
        data["Zones"] = zonesArray;

        return data;
    }

    public void LoadFromJson(Variant v)
    {
        var data = (Godot.Collections.Dictionary<string, Variant>)v;
        Variant outVariant;
        ThreeDee = data.TryGetValue("3D", out outVariant) ? (bool)outVariant : false;
        DinamicWorld = data.TryGetValue("DinamicWorld", out outVariant) ? (bool)outVariant : false;
        BlockSize = data.TryGetValue("BlockSize", out outVariant) ? (float)outVariant : 1f;

        Freq2D = data.TryGetValue("Freq2D", out outVariant) ? (float)outVariant : 0.003f;
        Freq3D = data.TryGetValue("Freq3D", out outVariant) ? (float)outVariant : 0.006f;

        ChunkWide = data.TryGetValue("ChunkWide", out outVariant) ? (int)outVariant : 100;
        ChunkDeep = data.TryGetValue("ChunkDeep", out outVariant) ? (int)outVariant : 100;
        TileType = data.TryGetValue("TileType", out outVariant) ? (int)outVariant : 0;
        Seed = data.TryGetValue("Seed", out outVariant) ? (uint)outVariant : 0;
        if (data.TryGetValue("HeightOverride", out outVariant))
            HeightOverride = (Godot.Collections.Array<int>)outVariant;
        else
            HeightOverride = new Godot.Collections.Array<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        Zones = new List<Zone>();
        if (data.TryGetValue("Zones", out outVariant))
        {
            foreach (var zoneData in (Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>>)outVariant)
            {
                var zone = Zone.JsonToZone(zoneData);
                Zones.Add(zone);
            }
        }

        if (data.TryGetValue("CoherenceTable", out outVariant))
            CoherenceTable = (Godot.Collections.Array<Godot.Collections.Array<int>>)outVariant;
        else
            for (int i = 0; i < 10; i++)
            {
                CoherenceTable[i] = new Godot.Collections.Array<int>();
                for (int j = 0; j < 10; j++)
                    CoherenceTable[i].Add(-1);
            }
    }
}

public struct ResourcePath
{
    private string path;
    private int probability;
    public ResourcePath(string path, int probability)
    {
        this.path = path;
        this.probability = probability;
    }
    public string Path
    {
        get { return path; }
        set { path = value; }
    }
    public int Probability
    {
        get { return probability; }
        set { probability = value; }
    }

    public Mesh GetMesh()
    {
        var res = GD.Load(path);
        if (res is Mesh directMesh) return directMesh;
        if (res is PackedScene packed)
        {
            var root = packed.Instantiate();
            var stack = new System.Collections.Generic.Stack<Node>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is MeshInstance3D mi && mi.Mesh != null)
                {
                    root.QueueFree();
                    return mi.Mesh;
                }
                foreach (Node child in current.GetChildren())
                    stack.Push(child);
            }
            root.QueueFree();
        }
        //GD.PushError("El recurso {{" + Path + "} no contiene un Mesh valido.");
        return null;
    }

    public Godot.Collections.Array<Variant> ToJson()
    {
        var data = new Godot.Collections.Array<Variant>
        { path, probability };
        return data;
    }
}

public struct ResourcePathList
{
    public List<ResourcePath> list;
    public int Lenght => list.Count;

    public ResourcePathList()
    {
        list = new List<ResourcePath>();
    }

    public ResourcePathList(Godot.Collections.Array<Variant> json)
    {
        list = new List<ResourcePath>();
        foreach (var kvp in json)
        {
            var array = (Godot.Collections.Array<Variant>)kvp;
            list.Add(new ResourcePath((string)array[0], (int)array[1]));
        }
    }

    public Godot.Collections.Array<Variant> ToJson()
    {
        var resourcesJson = new Godot.Collections.Array<Variant>();
        foreach (ResourcePath resource in list)
        {
            resourcesJson.Add(resource.ToJson());
        }
        return resourcesJson;
    }

    public int GetResourceIndexByProbability(int percentage)
    {
        int acumulatedPercentage = 0;
        for (int i = 0; i < list.Count; i++)
        {
            acumulatedPercentage += list[i].Probability;
            if (percentage <= acumulatedPercentage)
            {
                return i;
            }
        }
        return list.Count - 1;
    }

    public ResourcePath GetResourceByIndex(int index)
    {
        return list[index];
    }

    public void GetResourceIndexByPath(string path, out int index, out ResourcePath resource)
    {
        index = -1;
        resource = new ResourcePath();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Path == path)
            {
                index = i;
                resource = list[i];
                return;
            }
        }
    }

    public void Add(string path, int probability)
    {
        list.Add(new ResourcePath(path, probability));
    }
}
public enum TileType
{
    Square = 0,
    Triangle = 1,
    Hexagon = 2,
    Isometric = 3
}