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

        var instance = toolbar.Instantiate();
        Singleton = instance.GetNode<DockInterfaceManager>(".");
        
        AddControlToDock(EditorPlugin.DockSlot.LeftUl, Singleton);
    }
    
	public override void _ExitTree()
	{
        RemoveControlFromDocks(Singleton);

        //control.Free();
        //toolbar.Free();
        //control.Free();
        Singleton.QueueFree();
    }
}
#endif
