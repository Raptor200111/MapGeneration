using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;

struct Pos
{
    public int x { get; }
    public int y { get; }

    public Pos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    readonly static public Pos[] neighbours = { new Pos(1, 0), new Pos(0, 1), new Pos(-1, 0), new Pos(0, -1), };

    public static Pos operator +(Pos left, Pos right) => new Pos(left.x + right.x, left.y + right.y);
}

public partial class Chunk : Node3D
{
    private Tile _tile;
    private Zone[] _zones;
    private float blockScale;
    private Vector2I chunkSize;
    private uint _seed;

    private MultiMeshInstance3D _visual;

    public Chunk() 
	{
		_tile = new SquareTile();
        OnChunkCreated();
	}
	public Chunk(Tile tile) 
	{
		_tile = tile;
        OnChunkCreated();
	}

    private void OnChunkCreated()
    {
        _seed = MapGeneratorPlugin.Singleton.GenerationManager.GetSeed();

        /*
        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _noise.Seed = (int)_seed;
        */

        //_noise.CellularReturnType = FastNoiseLite.CellularReturnTypeEnum.CellValue;
        //_noise.CellularDistanceFunction = FastNoiseLite.CellularDistanceFunctionEnum.Manhattan;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

	public void Initialize(Basis basis, Vector3 pos, float blockScale, Vector2I chunkSize, int[,] coherenceTable, bool threeDee, int[] heightOverride)
	{
        this.blockScale = blockScale;
        this.chunkSize = chunkSize;

		_zones = MapGeneratorPlugin.Singleton.GenerationManager.GetZones();
        _seed = MapGeneratorPlugin.Singleton.GenerationManager.GetSeed();

        if (_zones == null || _zones.Length == 0) 
		{
			Debug.Print("WTF");
			return;
		}

        Transform3D[,] matrix = _tile.GetMatrix(blockScale, chunkSize, this.Position);

        Dictionary<int, Dictionary<int, List<Vector3>>> zonesUsed;
        
        if (threeDee)
            zonesUsed = Noise3D(chunkSize.X, chunkSize.Y, coherenceTable, heightOverride);
        else //if (dimension == Dimension.D2)
            zonesUsed = Noise2D(chunkSize.X, chunkSize.Y, coherenceTable);

        foreach (var auxZone in zonesUsed)
        {
            var zoneChild = new Node3D();
            zoneChild.Name = _zones[auxZone.Key].GetZoneName();
            AddChild(zoneChild);
            zoneChild.Owner = Owner;

            foreach (var auxResource in auxZone.Value)
            {
                var mesh = _zones[auxZone.Key].GetResource(auxResource.Key).GetMesh();
                var path = _zones[auxZone.Key].GetResource(auxResource.Key).Path;
                if (mesh == null)
                {
                    GD.PushError($"[{Name}] El recurso {{{path}}} no contiene Mesh.");
                    continue;
                }

                var aabb = mesh.GetAabb();
                var size = aabb.Size;
                float s = 1.0f / Mathf.Max(size.X, size.Z);
                Basis scaleBasis = Basis.Identity.Scaled(Vector3.One * s);

                var mm = new MultiMesh
                {
                    Mesh = mesh,
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                    InstanceCount = auxResource.Value.Count,
                    VisibleInstanceCount = auxResource.Value.Count
                };

                float liftY = -(aabb.Position.Y * s);

                int i = 0;
                foreach (var auxPos in auxResource.Value)
                {
                    Transform3D t = matrix[(int)auxPos.X, (int)auxPos.Z];
                    t.Origin.Y = auxPos.Y * 10;
                    t.Basis *= scaleBasis;
                    t.Origin.Y += liftY;
                    mm.SetInstanceTransform(i++, t);
                }

                var mmi = new MultiMeshInstance3D
                {
                    Name = "Resource_" + auxResource.Key.ToString(),
                    Multimesh = mm,
                    //MaterialOverride = mesh.SurfaceGetMaterialCount() > 0 ? mesh.SurfaceGetMaterial(0) : null
                };

                zoneChild.AddChild(mmi);
                mmi.Owner = zoneChild.Owner;
            }
        }
    }

    private Dictionary<int, Dictionary<int, List<Vector3>>> Noise3D(int width, int length, int[,] coherenceTable, int[] heightOverride)
    {
        FastNoiseLite tempNoise = new FastNoiseLite();
        tempNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        tempNoise.SetSeed((int)_seed);
        tempNoise.SetFrequency(0.05f);
        tempNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        tempNoise.DomainWarpEnabled = false;
        tempNoise.Offset = new Vector3(0, 0, 0);

        FastNoiseLite moistNoise = new FastNoiseLite();
        moistNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        //int reverseSeed = _seed.ToString().Reverse().Aggregate(0, (b, x) => 10 * b + x - '0');
        moistNoise.SetSeed((int)_seed + 1);
        moistNoise.SetFrequency(0.05f);
        moistNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        moistNoise.DomainWarpEnabled = false;
        moistNoise.Offset = new Vector3(0, 0, 0);

        FastNoiseLite heightMap = new FastNoiseLite();
        heightMap.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        //int reverseSeed = _seed.ToString().Reverse().Aggregate(0, (b, x) => 10 * b + x - '0');
        heightMap.SetSeed((int)_seed + 2);
        heightMap.SetFrequency(0.05f);
        heightMap.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        heightMap.DomainWarpEnabled = false;
        heightMap.Offset = new Vector3(0, 0, 0);

        ResourcePathList[] r = GetZonesResources();

        var zonesUsed = new Dictionary<int, Dictionary<int, List<Vector3>>>();

        for (int j = 0; j < length; j++)
        {
            for (int i = 0; i < width; i++)
            {
                int indiTempNoise = 9 - Mathf.Clamp((int)((tempNoise.GetNoise2D(j, i) + 1) * 5), 0, 9); // from 0 to 9 both inclusive
                int indiMoistNoise = Mathf.Clamp((int)((moistNoise.GetNoise2D(j, i) + 1) * 5), 0, 9);
                int resourceNoise = (indiTempNoise + indiMoistNoise) * 5;
                float height = heightMap.GetNoise2D(j, i) + 1f * 5f;

                int zoneIdHeight = heightOverride[Mathf.Clamp((int)height, 0, 9)];
                int zoneId = coherenceTable[indiTempNoise, indiMoistNoise];
                if (zoneId != -1 || zoneIdHeight != -1)
                {
                    if (zoneIdHeight != -1)
                        zoneId = zoneIdHeight;

                    int resourceId = r[zoneId].GetResourceIndexByProbanility(resourceNoise);
                    //GD.Print("Zone: " + zoneId + " Resource: " + resourceId + " at (" + i + ", " + j + ")");
                    //GD.Print("indiTempNoise:" + indiTempNoise + " indiMoistNoise:" + indiMoistNoise + "NoiseSum: " + resourceNoise);


                    if (!zonesUsed.ContainsKey(zoneId))
                        zonesUsed[zoneId] = new Dictionary<int, List<Vector3>>();
                    if (!zonesUsed[zoneId].ContainsKey(resourceId))
                        zonesUsed[zoneId][resourceId] = new List<Vector3>();

                    zonesUsed[zoneId][resourceId].Add(new Vector3(i, height, j));
                }
            }
        }

        return zonesUsed;
    }

    private Dictionary<int, Dictionary<int, List<Vector3>>> Noise2D(int width, int height, int[,] coherenceTable)
    {
        FastNoiseLite tempNoise = new FastNoiseLite();
        tempNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        tempNoise.SetSeed((int)_seed);
        tempNoise.SetFrequency(0.05f);
        tempNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        tempNoise.DomainWarpEnabled = false;
        tempNoise.Offset = new Vector3(0, 0, 0);

        FastNoiseLite moistNoise = new FastNoiseLite();
        moistNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        //int reverseSeed = _seed.ToString().Reverse().Aggregate(0, (b, x) => 10 * b + x - '0');
        moistNoise.SetSeed((int)_seed + 1);
        moistNoise.SetFrequency(0.05f);
        moistNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        moistNoise.DomainWarpEnabled = false;
        moistNoise.Offset = new Vector3(0, 0, 0);

        ResourcePathList[] r = GetZonesResources();
        
        var zonesUsed = new Dictionary<int, Dictionary<int, List<Vector3>>>();

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                int indiTempNoise = 9 - Mathf.Clamp((int)((tempNoise.GetNoise2D(j, i) + 1) * 5), 0, 9); // from 0 to 9 both inclusive
                int indiMoistNoise = Mathf.Clamp((int)((moistNoise.GetNoise2D(j, i) + 1) * 5), 0, 9);
                int resourceNoise = (indiTempNoise + indiMoistNoise) * 5;

                //Debug.Print("0."+indiTempNoise.ToString() + " 0." + indiMoistNoise.ToString() + " : " + coherenceTable[indiTempNoise, indiMoistNoise].ToString());

                int zoneId = coherenceTable[indiTempNoise, indiMoistNoise];
                if (zoneId != -1)
                {
                    int resourceId = r[zoneId].GetResourceIndexByProbanility(resourceNoise);
                    //GD.Print("Zone: " + zoneId + " Resource: " + resourceId + " at (" + i + ", " + j + ")");
                    //GD.Print("indiTempNoise:" + indiTempNoise + " indiMoistNoise:" + indiMoistNoise + "NoiseSum: " + resourceNoise);


                    if (!zonesUsed.ContainsKey(zoneId))
                        zonesUsed[zoneId] = new Dictionary<int, List<Vector3>>();
                    if (!zonesUsed[zoneId].ContainsKey(resourceId))
                        zonesUsed[zoneId][resourceId] = new List<Vector3>();

                    zonesUsed[zoneId][resourceId].Add(new Vector3(i, 0, j));
                }
            }
        }

        return zonesUsed;
    }

    private bool ViablePos (Pos pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < chunkSize.X && pos.y < chunkSize.Y;
    }

    private ResourcePathList[] GetZonesResources()
    {
        ResourcePathList[] zonesWithResources = new ResourcePathList[_zones.Length];

        int zondeId = 0;
        foreach (Zone z in _zones)
            zonesWithResources[zondeId++] = z.GetResources();

        return zonesWithResources;
    }
}