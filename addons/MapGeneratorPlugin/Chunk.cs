using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	private int[,] _zoneBlockDistribution;
	private Block[,] _blocks;
    private Zone[] _zones;
    private float blockScale;
    private Vector2I chunkSize;
    private uint _seed;

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
        _seed = DockInterfaceManager.Singleton.GenerationManager.GetSeed();

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

	public void Initialize(Basis basis, Vector3 pos, float blockScale, Vector2I chunkSize, int[,] coherenceTable)
	{
        this.blockScale = blockScale;
        this.chunkSize = chunkSize;

		_zones = DockInterfaceManager.Singleton.GenerationManager.GetZones();
        _seed = DockInterfaceManager.Singleton.GenerationManager.GetSeed();

        if (_zones == null || _zones.Length == 0) 
		{
			Debug.Print("WTF");
			return;
		}

        Transform3D[,] matrix = _tile.GetMatrix(blockScale, chunkSize, this.Position);
		_blocks = new Block[chunkSize.Y, chunkSize.X];
        
        PerlinNoise(chunkSize.X, chunkSize.Y, coherenceTable);

        Zone black = new Zone();
        black.Initialize("black", new Color(0, 0, 0));

        for (int j = 0; j < chunkSize.Y; j++)
		{
			Node3D rowChild = new Node3D();
			rowChild.Name = "row" + j.ToString();
			this.AddChild(rowChild);
            rowChild.Owner = this.Owner;
			
            for (int i = 0; i < chunkSize.X; i++)
			{
				_blocks[j, i] = new Block();
                _blocks[j, i].Name = "Block" + j.ToString() + i.ToString();
                
                rowChild.AddChild( _blocks[j, i]);
                _blocks[j, i].Owner = rowChild.Owner;

                if (_zoneBlockDistribution[j, i] == -1)
                {
                    _blocks[j, i].Initialize(black, _tile, matrix[j, i], blockScale);
                }
                else
                    _blocks[j, i].Initialize(_zones[_zoneBlockDistribution[j, i]], _tile, matrix[j, i], blockScale);
            }
        }
    }


    private void PerlinNoise(int width, int height, int[,] coherenceTable)
    {
        FastNoiseLite tempNoise = new FastNoiseLite();
        tempNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
        tempNoise.SetSeed((int)_seed);
        tempNoise.SetFrequency(0.05f);
        tempNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        tempNoise.DomainWarpEnabled = false;
        tempNoise.Offset = new Vector3(0, 0, 0);

        FastNoiseLite moistNoise = new FastNoiseLite();
        moistNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
        int reverseSeed = _seed.ToString().Reverse().Aggregate(0, (b, x) => 10 * b + x - '0');
        moistNoise.SetSeed(reverseSeed);
        moistNoise.SetFrequency(0.05f);
        moistNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        moistNoise.DomainWarpEnabled = false;
        moistNoise.Offset = new Vector3(0, 0, 0);

        /*
        fnl.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunctionEnum.Hybrid);
        fnl.SetCellularReturnType(FastNoiseLite.CellularReturnTypeEnum.CellValue);
        fnl.SetCellularJitter(1f);
        */

        _zoneBlockDistribution = new int[height, width];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                _zoneBlockDistribution[j, i] = -1;
            }
        }

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                int indiTempNoise = 9 - Mathf.Clamp((int)((tempNoise.GetNoise2D(j, i) + 1) * 5), 0, 9); // from 0 to 10 both inclusive
                int indiMoistNoise = Mathf.Clamp((int)((moistNoise.GetNoise2D(j, i) + 1) * 5), 0, 9);
                

                //Debug.Print("0."+indiTempNoise.ToString() + " 0." + indiMoistNoise.ToString() + " : " + coherenceTable[indiTempNoise, indiMoistNoise].ToString());

                _zoneBlockDistribution[j, i] = coherenceTable[indiTempNoise, indiMoistNoise];
            }
        }
    }

    private bool ViablePos (Pos pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < chunkSize.X && pos.y < chunkSize.Y;
    }
}
