using Godot;
using System;
using System.IO;

[Tool]
public partial class NewZoneContent : PanelContainer
{
    private DockInterfaceManager dockInterfaceManager;
    private ColorPickerButton _colorPicker;
    private LineEdit _zoneNameLabel;
    private Node _resourcePickerList;
    private bool editing = false;
    private PackedScene ResourcePickerPrefabPath;
    public override void _Ready()
    {
        _colorPicker = GetNode<ColorPickerButton>("VBoxContainer/ColorPickerButton");
        _zoneNameLabel = GetNode<LineEdit>("VBoxContainer/ZoneNameLineEdit");
        _resourcePickerList = GetNode("VBoxContainer/ResourcePickerList");
        ResourcePickerPrefabPath = GD.Load<PackedScene>("res://addons/MapGeneratorPlugin/Prefabs/resource_picker.tscn");

        _colorPicker.Color = new Color(new RandomNumberGenerator().Randi()) { A = 1.0f };
    }

    public void Initialize(DockInterfaceManager d)
    {
        dockInterfaceManager = d;
        //CustomMinimumSize = new Vector2(0, 200);
    }

    public void Edit(string name, Color color, ResourcePathList resources)
	{
		editing = true;

        _zoneNameLabel.Text = name;
        _colorPicker.Color = color;

        foreach (ResourcePath r in resources.list)
        {
            Node aux = ResourcePickerPrefabPath.Instantiate();
            _resourcePickerList.AddChild(aux);
            aux.Owner = _resourcePickerList.Owner;

            if (aux is ResourcePicker resourcePicker)
            {
                resourcePicker.SetResourcePath(r.Path);
                resourcePicker.SetProbability((int)r.Probability);
            }
        }

        //foreach (Node child in _resourcePickerList.GetChildren())
        //{
        //    if (child is ResourcePicker resourcePicker)
        //        resources.AddResource(resourcePicker.GetResourcePath(), resourcePicker.GetProbability());
        //}
    }

	public void Save()
	{
		if (editing)
		{
			editing = false;
            dockInterfaceManager.SaveZone(_zoneNameLabel.Text, _colorPicker.Color, GetResources(), GetIndex());
        }
		else
            dockInterfaceManager.SaveZone(_zoneNameLabel.Text, _colorPicker.Color, GetResources());
    }

	public void Delete()
	{
        if (editing)
        {
            editing = false;
            dockInterfaceManager.DeleteZone(GetIndex());
        }
		else
            dockInterfaceManager.DeleteZone();
    }

    public void AddResourceButton()
    {
        Node aux = ResourcePickerPrefabPath.Instantiate();
        _resourcePickerList.AddChild(aux);
        aux.Owner = _resourcePickerList.Owner;
    }

    private ResourcePathList GetResources()
    {
        int probailitySum = 0;
        ResourcePathList resources = new ResourcePathList();
        if (_resourcePickerList.GetChildCount() <= 0)
        {
            GD.Print("WARNING: No resources added to the Zone: " + _zoneNameLabel.Text + ".");
        }
        else
        {
            foreach (Node child in _resourcePickerList.GetChildren())
            {
                if (child is ResourcePicker resourcePicker)
                    resources.Add(resourcePicker.GetResourcePath(), probailitySum += resourcePicker.GetProbability());
            }
            if (probailitySum != 100)
            {
                string errorMessage = "The probaility Sum is not 100, readjusted to:";
                resources = new ResourcePathList();
                foreach (Node child in _resourcePickerList.GetChildren())
                {
                    if (child is ResourcePicker resourcePicker)
                    {
                        int newProb = (int)(resourcePicker.GetProbability() * 100f / probailitySum);
                        resources.Add(resourcePicker.GetResourcePath(), newProb);
                        resourcePicker.SetProbability(newProb);
                        errorMessage += " " + newProb + ",";
                    }
                }
                errorMessage = errorMessage.TrimEnd(',');
                errorMessage += ".";
                GD.Print(errorMessage);
            }
        }
        return resources;
    }
}