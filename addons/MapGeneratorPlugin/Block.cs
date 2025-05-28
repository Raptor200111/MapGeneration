using Godot;
using System;

public partial class Block : Node3D
{
	private Tile _tile;
	private Zone _zone;
	private Transform3D _offset;
	private float _scale;

	public Block() {}

    public void Initialize(Zone zone, Tile tile, Transform3D offset, float scale)
	{
		_zone = zone;
		_tile = tile;
		_offset = offset;
		_scale = scale;

        CsgPrimitive3D box = _tile.GetPoligon();
        this.AddChild(box);
		box.Owner = this.Owner;

		box.Scale = new Vector3(_scale / box.Scale.X, _scale / box.Scale.Y, _scale / box.Scale.X);
        Transform3D finalTransform = _offset * box.Transform;
        box.Transform = finalTransform;
        box.MaterialOverride = _zone.BaseMaterial();
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        /*
        CsgPrimitive3D box = _tile.GetPoligon();
        
		box.Scale = new Vector3(_scale / box.Scale.X, _scale / box.Scale.Y, _scale / box.Scale.X);
        Transform3D finalTransform = _offset * box.Transform;
        box.Transform = finalTransform;
        box.MaterialOverride = _zone.BaseMaterial();
		
        this.AddChild(box);
        box.Owner = this.Owner;
		*/
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
