using Godot;
using Godot.Collections;
using System;

public partial class Zone : Node
{
	private StandardMaterial3D _baseMaterial;
    string zoneName;

    public Zone()
	{
        _baseMaterial = new StandardMaterial3D();
        _baseMaterial.AlbedoColor = new Color(new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf(), new RandomNumberGenerator().Randf());
    }

    static public Zone JsonToZone(Dictionary<string, Variant> dic)
    {
        Zone zone = new Zone();
        zone.Initialize(dic["Name"].AsString(), new Color((uint)dic["Color"]));
        return zone;
    }

	public void Initialize(String zoneName, Color color)
	{
        _baseMaterial.AlbedoColor = color;
        this.zoneName = zoneName;
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

    internal Dictionary<string, Variant> ToJson()
    {
        return new Godot.Collections.Dictionary<string, Variant>
        {
            ["Name"] = zoneName,
            ["Color"] = GetColor().ToRgba32()
        };
    }
}
