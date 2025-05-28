using Godot;
using System;
using System.Diagnostics;
using System.Globalization;

[Tool]
public partial class ZoneContent : PanelContainer
{
    private string name;

    private DockInterfaceManager dockInterfaceManager;
    public ZoneContent()
    {
        dockInterfaceManager = DockInterfaceManager.Singleton;
    }

    public void Initialize(DockInterfaceManager d)
    {
        dockInterfaceManager = d;
    }

    public void SetElements(string name, Color color)
	{
		this.name = name;
		GetNode<Label>("./HBoxContainer/ZoneNameLabel").Text = name;
        GetNode<ColorRect>("./HBoxContainer/ColorContent").Color = color;
    }

	public void Edit()
	{
		Debug.Print("Edit");
        dockInterfaceManager.EditZone(GetIndex());
	}
}