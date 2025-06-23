using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;


[Tool]
public partial class Zone
{
    private Color baseColor;
    private string zoneName;
    private ResourcePathList resources;

    public Zone()
	{
        baseColor = new Color(new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf());
    }

	public void Initialize(String zoneName, Color color, ResourcePathList resources)
	{
        baseColor = color;
        this.zoneName = zoneName;
        this.resources = resources;
    }

	public Color GetColor()
	{
        return baseColor;
    }

	public void SetColor(Color color)
	{
        baseColor = color;
    }

    public String GetZoneName()
    {
        return zoneName;
    }

    public Godot.Collections.Dictionary<string, Variant> ToJson()
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
        zone.Initialize(dic["Name"].AsString(), new Color((uint)dic["Color"]), new ResourcePathList((Godot.Collections.Array<Variant>)dic["Resources"]) );
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