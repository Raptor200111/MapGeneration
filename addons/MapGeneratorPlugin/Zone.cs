using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

public struct ResourcePath
{
    public string Path;
    public int Probability;
    public ResourcePath(string path, int probability)
    {
        Path = path;
        Probability = probability;
    }

    public Mesh GetMesh()
    {
        var res = GD.Load(Path);
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

}

public struct ResourcePathList
{
    public List<ResourcePath> list;
    public ResourcePathList()
    {
        list = new List<ResourcePath>();
    }

    public ResourcePathList(Godot.Collections.Dictionary<string, Variant> json)
    {
        list = new List<ResourcePath>();
        foreach (var kvp in json)
        {
            list.Add(new ResourcePath(kvp.Key, (int)kvp.Value));
        }
    }

    public Godot.Collections.Dictionary<string, Variant> ToJson()
    {
        Godot.Collections.Dictionary<string, Variant> resourcesJson = new Godot.Collections.Dictionary<string, Variant>();
        foreach (ResourcePath resource in list)
        {
            resourcesJson.Add(resource.Path, resource.Probability);
        }
        return resourcesJson;
    }

    public int GetResourceIndexByProbanility(int percentage)
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
        return list.Count-1;
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

    public void AddResource(string path, int probability)
    {
        list.Add(new ResourcePath(path, probability));
    }
}

public partial class Zone : Node
{
    private StandardMaterial3D _baseMaterial;
    string zoneName;
    //Dictionary<string, Variant> resources;
    private ResourcePathList resources;// = new List<Resource>();

    public Zone()
	{
        _baseMaterial = new StandardMaterial3D();
        _baseMaterial.AlbedoColor = new Color(new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf());
    }

	public void Initialize(String zoneName, Color color, ResourcePathList resources)
	{
        _baseMaterial.AlbedoColor = color;
        this.zoneName = zoneName;
        this.resources = resources;
    }

    public StandardMaterial3D BaseMaterial()
	{
		return _baseMaterial;
	}

	public Color GetColor()
	{
		return _baseMaterial.AlbedoColor;
	}

	public void SetColor(Color color)
	{
        _baseMaterial.AlbedoColor = color;
    }

    public String GetZoneName()
    {
        return zoneName;
    }

    internal Godot.Collections.Dictionary<string, Variant> ToJson()
    {
        return new Godot.Collections.Dictionary<string, Variant>
        {
            ["Name"] = zoneName,
            ["Color"] = GetColor().ToRgba32(),
            ["Resources"] = resources.ToJson()
        };
    }
    
    static public Zone JsonToZone(Godot.Collections.Dictionary<string, Variant> dic)
    {
        Zone zone = new Zone();
        zone.Initialize(dic["Name"].AsString(), new Color((uint)dic["Color"]), new ResourcePathList((Godot.Collections.Dictionary<string, Variant>)dic["Resources"]) );
        return zone;
    }

    public ResourcePathList GetResources()
    {
        return resources;
    }

    public ResourcePath GetResource(int index)
    {
        return resources.GetResourceByIndex(index);
    }
}