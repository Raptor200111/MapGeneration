using Godot;
using System;

public abstract partial class Tile
{

	
	public abstract Transform3D[,] GetMatrix(Vector2I chunkSize);

	public abstract ArrayMesh GetMesh();

    public static Tile GetTileType(TileType tileType)
	{
        switch (tileType)
        {
            case TileType.Square:
                return new SquareTile();
            case TileType.Triangle:
                return new TriangleTile();
            case TileType.Hexagon:
                return new HexTile();
            case TileType.Isometric:
                return new IsoTile();
            default:
                throw new ArgumentException("Unknown tile type: " + tileType);
        }
	}
}
