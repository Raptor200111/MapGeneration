using Godot;
using System;

public abstract partial class Tile : Node
{

	public enum TileType
	{
		Square = 0,
		Triangle = 1,
		Hexagon = 2,
		Isometric = 3
	}
	public abstract Transform3D[,] GetMatrix(float blockScale, Vector2I chunkSize, Vector3 origin);

	public abstract ArrayMesh GetMesh();

    public static Tile GetTileType(TileType tileType)
	{
		if (tileType == TileType.Square)
		{
			return new SquareTile();
        }
        else if (tileType == TileType.Triangle)
        {
            return new TriangleTile();
        }
        else if (tileType == TileType.Hexagon)
        {
            return new HexTile();
        }
        else //if (tileType == Tiles.Isometric)
        {
            return new IsoTile();
        }
	}
}
