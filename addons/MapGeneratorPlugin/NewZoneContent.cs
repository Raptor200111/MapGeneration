using Godot;
using System;
using System.Diagnostics;
using System.Globalization;

[Tool]
public partial class NewZoneContent : PanelContainer
{
	private bool editing = false;

	private DockInterfaceManager dockInterfaceManager;
    public NewZoneContent()
	{
        dockInterfaceManager = DockInterfaceManager.Singleton;
    }

    public void Initialize(DockInterfaceManager d)
    {
        dockInterfaceManager = d;
        //CustomMinimumSize = new Vector2(0, 200);
    }

    public void Edit(string name, Color color)
	{
		editing = true;

        GetNode<LineEdit>("./VBoxContainer/ZoneNameLabel").Text = name;
		GetNode<ColorPickerButton>("./VBoxContainer/HBoxContainer/ColorPickerButton").Color = color;
    }

	public void Save()
	{
		if (editing)
		{
			editing = false;
            dockInterfaceManager.SaveZone(GetNode<LineEdit>("./VBoxContainer/ZoneNameLabel").Text, GetNode<ColorPickerButton>("./VBoxContainer/HBoxContainer/ColorPickerButton").Color, GetIndex());
        }
		else
            dockInterfaceManager.SaveZone(GetNode<LineEdit>("./VBoxContainer/ZoneNameLabel").Text, GetNode<ColorPickerButton>("./VBoxContainer/HBoxContainer/ColorPickerButton").Color);
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
}