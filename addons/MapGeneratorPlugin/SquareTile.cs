using Godot;
using System;

public partial class SquareTile : Tile
{
    public override Transform3D[,] GetMatrix(float blockScale, Vector2I chunkSize, Vector3 origin)
    {
        Transform3D[,] matrix = new Transform3D[chunkSize.X, chunkSize.Y];
        Vector3 offset = new Vector3(0.5f - chunkSize.X/2f, 0, 0.5f - chunkSize.Y/2f);

        for (int j = 0; j < chunkSize.Y; j++)
        {
            for (int i = 0; i < chunkSize.X; i++)
            {
                Vector3 translation = new Vector3(i, 0, j);

                translation += offset;
                translation *= blockScale;
                translation += origin;

                matrix[i, j] = new Transform3D(Basis.Identity, translation);
            }
        }

        return matrix;
    }

    public override CsgPrimitive3D GetPoligon()
    {
        return new CsgBox3D();
    }
}