#if TOOLS
using Godot;
using System;

[Tool]
public partial class MapGeneratorPlugin : EditorPlugin
{
    Control control;

    public override void _EnterTree()
	{
        PackedScene toolbar = ResourceLoader.Load<PackedScene>("res://addons/MapGeneratorPlugin/DockInterface.tscn");
        control = (Control)toolbar.Instantiate();
        
        AddControlToDock(EditorPlugin.DockSlot.LeftUl, control);
    }
    
	public override void _ExitTree()
	{
        RemoveControlFromDocks(control);
        
        //control.Free();
        //toolbar.Free();
        //control.Free();
        control.QueueFree();
    }
}
#endif
