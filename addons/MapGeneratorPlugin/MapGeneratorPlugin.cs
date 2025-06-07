#if TOOLS
using Godot;
using System;

[Tool]
public partial class MapGeneratorPlugin : EditorPlugin
{
    private static DockInterfaceManager _singleton = null;
    static public DockInterfaceManager Singleton
    {
        get
        {
            if (_singleton == null)
            {
                GD.PrintErr("DockInterfaceManager is not initialized.");
            }
            return _singleton;
        }
        private set { _singleton = value; }
    }

    public override void _EnterTree()
	{
        PackedScene toolbar = ResourceLoader.Load<PackedScene>("res://addons/MapGeneratorPlugin/DockInterface.tscn");
        _singleton = (DockInterfaceManager)toolbar.Instantiate();
        
        AddControlToDock(EditorPlugin.DockSlot.LeftUl, _singleton);
    }
    
	public override void _ExitTree()
	{
        RemoveControlFromDocks(_singleton);

        //control.Free();
        //toolbar.Free();
        //control.Free();
        _singleton.QueueFree();
    }
}
#endif
