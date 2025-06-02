using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

public struct Resource
{
    public string Path;
    public int Probability;
    public Resource(string path, int probability)
    {
        Path = path;
        Probability = probability;
    }
}

public struct Resources
{
    public List<Resource> list;
    public Resources()
    {
        list = new List<Resource>();
    }

    public Resources(Godot.Collections.Dictionary<string, Variant> json)
    {
        list = new List<Resource>();
        foreach (var kvp in json)
        {
            list.Add(new Resource(kvp.Key, (int)kvp.Value));
        }
    }

    public Godot.Collections.Dictionary<string, Variant> ToJson()
    {
        Godot.Collections.Dictionary<string, Variant> resourcesJson = new Godot.Collections.Dictionary<string, Variant>();
        foreach (Resource resource in list)
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

    public Resource GetResourceByIndex(int index)
    {
        return list[index];
    }

    public void GetResourceIndexByPath(string path, out int index, out Resource resource)
    {
        index = -1;
        resource = new Resource();
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
        list.Add(new Resource(path, probability));
    }
}

public partial class Zone : Node
{
    private StandardMaterial3D _baseMaterial;
    string zoneName;
    //Dictionary<string, Variant> resources;
    private Resources resources;// = new List<Resource>();

    public Zone()
	{
        _baseMaterial = new StandardMaterial3D();
        _baseMaterial.AlbedoColor = new Color(new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf());
    }

	public void Initialize(String zoneName, Color color, Resources resources)
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
        zone.Initialize(dic["Name"].AsString(), new Color((uint)dic["Color"]), new Resources((Godot.Collections.Dictionary<string, Variant>)dic["Resources"]) );
        return zone;
    }

    public Resources GetResources()
    {
        return resources;
    }
}