using Godot;
using System;
using System.IO;

[Tool]
public partial class NewZoneContent : PanelContainer
{
    [Export] private NodePath ColorPickerPath;
    [Export] private NodePath ZoneNameLineEditPath;
    [Export] private NodePath ResourcePickerListPath;
    [Export] private PackedScene ResourcePickerPrefabPath;

    private DockInterfaceManager dockInterfaceManager;
    private ColorPickerButton _colorPicker;
    private LineEdit _zoneNameLabel;
    private Node _resourcePickerList;
    private bool editing = false;

    public override void _Ready()
    {
        _colorPicker = GetNode<ColorPickerButton>(ColorPickerPath);
        _zoneNameLabel = GetNode<LineEdit>(ZoneNameLineEditPath);
        _resourcePickerList = GetNode(ResourcePickerListPath);

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
        ResourcePathList resources = new ResourcePathList();
        foreach (Node child in _resourcePickerList.GetChildren())
        {
            if (child is ResourcePicker resourcePicker)
                resources.AddResource(resourcePicker.GetResourcePath(), resourcePicker.GetProbability());
        }
        return resources;
    }
}