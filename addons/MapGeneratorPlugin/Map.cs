using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TFG_Godot.Properties;
using static Tile;

public partial class Map : Node
{
    public ConfigInfo configInfo;

    private List<List<Chunk>> chunkMatrix;
    private Vector2I chunkmatrixCenter = new Vector2I(0, 0);

    public override void _EnterTree()
    {
        configInfo.LoadFromJson(JsonConfigIO.Load("res://addons/MapGeneratorPlugin/executeConfig.json"));
    }
    public override void _Ready()
    {
        InitializeChunkMatrix(this);
    }

    bool wPressed = false;
    bool sPressed = false;
    bool aPressed = false;
    bool dPressed = false;

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.W))
        {
            if (!wPressed)
            {
                ExtendChunkMatrix(Direction.North);
                wPressed = true;
            }
        }
        else if (wPressed)
        {
            wPressed = false;
        }

        if (Input.IsKeyPressed(Key.S))
        {
            if (!sPressed)
            {
                ExtendChunkMatrix(Direction.South);
                sPressed = true;
            }
        }
        else if (sPressed)
        {
            sPressed = false;
        }

        if (Input.IsKeyPressed(Key.A))
        {
            if (!aPressed)
            {
                ExtendChunkMatrix(Direction.West);
                aPressed = true;
            }
        }
        else if (aPressed)
        {
            aPressed = false;
        }

        if (Input.IsKeyPressed(Key.D))
        {
            if (dPressed == false)
            {
                ExtendChunkMatrix(Direction.East);
                dPressed = true;
            }
        }
        else if (dPressed)
        {
            dPressed = false;
        }
    }
    public override void _PhysicsProcess(double delta)
    {

    }

    private Chunk GenerateChunk(Node node, string name, Vector2I chunkPos)
    {
        var IndividualChunk = new Chunk(GetTileType((TileType)configInfo.TileType));
        IndividualChunk.Name = name;
        node.AddChild(IndividualChunk);
        IndividualChunk.Owner = node;
        IndividualChunk.Initialize(new Vector3(chunkPos.X, 0, chunkPos.Y), configInfo.BlockSize, new Vector2I(configInfo.ChunkWide, configInfo.ChunkDeep), configInfo.Zones.ToArray(), configInfo.CoherenceTable, configInfo.ThreeDee, configInfo.HeightOverride, new NoiseAlgorithm(configInfo.Seed, configInfo.Freq2D, configInfo.Freq3D));

        return IndividualChunk;
    }

    private void InitializeChunkMatrix(Node node)
    {
        chunkmatrixCenter = new Vector2I(0, 0);
        chunkMatrix = new List<List<Chunk>>();

        for (int j = -1; j < 2; j++)
        {
            chunkMatrix.Add(new List<Chunk>());
            for (int i = -1; i < 2; i++)
            {
                chunkMatrix[j + 1].Add(GenerateChunk(node, "Chunk_" + i + "_" + j, new Vector2I(i, j)));
            }
        }
    }

    public void ExtendChunkMatrix(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                chunkmatrixCenter.Y -= 1;
                for (int i = 0; i < 3; i++)
                {
                    chunkMatrix[2][i].QueueFree();
                }
                chunkMatrix.RemoveAt(2);
                chunkMatrix.Insert(0, new List<Chunk>());
                for (int i = -1; i < 2; i++)
                {
                    chunkMatrix[0].Add(GenerateChunk(this, "Chunk_" + (chunkmatrixCenter.X + i) + "_" + (chunkmatrixCenter.Y - 1), new Vector2I(chunkmatrixCenter.X + i, chunkmatrixCenter.Y - 1)));
                }
                break;
            case Direction.East:
                chunkmatrixCenter.X += 1;
                for (int j = -1; j < 2; j++)
                {
                    chunkMatrix[j + 1][0].QueueFree();
                    chunkMatrix[j + 1].RemoveAt(0);
                    chunkMatrix[j + 1].Add(GenerateChunk(this, "Chunk_" + (chunkmatrixCenter.X + 1) + "_" + (chunkmatrixCenter.Y + j), new Vector2I(chunkmatrixCenter.X + 1, chunkmatrixCenter.Y + j)));
                }
                break;
            case Direction.South:
                chunkmatrixCenter.Y += 1;
                for (int i = 0; i < 3; i++)
                {
                    chunkMatrix[0][i].QueueFree();
                }
                chunkMatrix.RemoveAt(0);
                chunkMatrix.Add(new List<Chunk>());
                for (int i = -1; i < 2; i++)
                {
                    chunkMatrix[2].Add(GenerateChunk(this, "Chunk_" + (chunkmatrixCenter.X + i) + "_" + (chunkmatrixCenter.Y + 1), new Vector2I(chunkmatrixCenter.X + i, chunkmatrixCenter.Y + 1)));
                }
                break;
            case Direction.West:
                chunkmatrixCenter.X -= 1;
                for (int j = -1; j < 2; j++)
                {
                    chunkMatrix[j + 1][2].QueueFree();
                    chunkMatrix[j + 1].RemoveAt(2);
                    chunkMatrix[j + 1].Insert(0, GenerateChunk(this, "Chunk_" + (chunkmatrixCenter.X - 1) + "_" + (chunkmatrixCenter.Y + j), new Vector2I(chunkmatrixCenter.X - 1, chunkmatrixCenter.Y + j)));
                }
                break;
            case Direction.NorthEast:
                ExtendChunkMatrix(Direction.North);
                ExtendChunkMatrix(Direction.East);
                break;
            case Direction.SouthEast:
                ExtendChunkMatrix(Direction.South);
                ExtendChunkMatrix(Direction.East);
                break;
            case Direction.SouthWest:
                ExtendChunkMatrix(Direction.South);
                ExtendChunkMatrix(Direction.West);
                break;
            case Direction.NorthWest:
                ExtendChunkMatrix(Direction.North);
                ExtendChunkMatrix(Direction.West);
                break;
            default:
                GD.PrintErr("Invalid direction for extending chunk matrix");
                break;
        }
    }
}