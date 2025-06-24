using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Chunk : Node3D
{
    private Tile _tile;
    private float blockScale;
    private Vector2I chunkSize;
    private Vector2I chunkOffset;
    //private uint _seed;

    private MultiMeshInstance3D _visual;

    public Chunk() 
	{
		_tile = new SquareTile();
	}
	public Chunk(Tile tile) 
	{
		_tile = tile;
	}



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

	public void Initialize(Vector3 offset, float blockScale, Vector2I chunkSize, Zone[] zones, Godot.Collections.Array<Godot.Collections.Array<int>> coherenceTable, bool threeDee, Godot.Collections.Array<int> heightOverride, NoiseAlgorithm noiseAlgorithm)
	{
        this.chunkOffset = new Vector2I((int)offset.X, (int)offset.Z);
        this.blockScale = blockScale;
        this.chunkSize = chunkSize;

        if (zones == null || zones.Length == 0) 
		{
            Debug.Print("WTF");
			return;
		}

        Transform3D[,] matrix = _tile.GetMatrix(chunkSize);

        Dictionary<int, Dictionary<int, List<Vector3>>> blocksByZones;
        
        offset = new Vector3(offset.X * chunkSize.X * blockScale, 0, offset.Z * chunkSize.Y * blockScale);

        blocksByZones = noiseAlgorithm.GenerateNoise(offset, chunkSize.X, chunkSize.Y, coherenceTable, zones, heightOverride, threeDee); 

        foreach (var zone in blocksByZones)
        {
            var zoneNode = new Node3D();
            zoneNode.Name = zones[zone.Key].GetZoneName();
            AddChild(zoneNode);
            zoneNode.Owner = Owner;

            foreach (var resource in zone.Value)
            {
                var mesh = zones[zone.Key].GetResource(resource.Key).GetMesh();
                if (mesh == null)
                {
                    GD.PushError($"[{Name}] El recurso {{{zones[zone.Key].GetResource(resource.Key).Path}}} no contiene Mesh.");
                    continue;
                }

                var aabb = mesh.GetAabb();
                var size = aabb.Size;
                float s = 1f / size.X;
                if (_tile is HexTile)
                    s = 2f / size.X;
                else if (_tile is IsoTile)
                    s = Mathf.Sqrt(0.75f) * 2 / size.X;
                Basis scaleBasis = Basis.Identity.Scaled(Vector3.One * s);

                var mm = new MultiMesh
                {
                    Mesh = mesh,
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                    InstanceCount = resource.Value.Count,
                    VisibleInstanceCount = resource.Value.Count
                };

                float liftY = -(aabb.Position.Y * s);

                int i = 0;
                foreach (var blockPos in resource.Value)
                {
                    Transform3D t = matrix[(int)blockPos.X, (int)blockPos.Z];
                    t.Origin.Y = blockPos.Y * 10; //add offset
                    t.Basis *= scaleBasis;
                    t.Origin.Y += liftY;
                    mm.SetInstanceTransform(i++, t);
                }

                var mmi = new MultiMeshInstance3D
                {
                    Name = "Resource_" + resource.Key.ToString(),
                    Multimesh = mm,
                    Transform = new Transform3D()
                    {
                        Origin = offset,
                        Basis = Basis.Identity.Scaled(new Vector3(blockScale, blockScale, blockScale))
                    },
                    //MaterialOverride = mesh.SurfaceGetMaterialCount() > 0 ? mesh.SurfaceGetMaterial(0) : null
                };

                zoneNode.AddChild(mmi);
                mmi.Owner = zoneNode.Owner;
            }
        }
    }

    private bool ViablePos (Pos pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < chunkSize.X && pos.y < chunkSize.Y;
    }
}