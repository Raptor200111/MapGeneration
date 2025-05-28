using Godot;
using System;

public partial class IsoTile : Tile
{
    float triangleHeight = (float)Math.Sqrt(3) / 2f;
    public override Transform3D[,] GetMatrix(float blockScale, Vector2I chunkSize, Vector3 origin)
    {
        Transform3D[,] matrix = new Transform3D[chunkSize.X, chunkSize.Y];
        Vector3 offset = new Vector3(triangleHeight/2f * (1-chunkSize.X), 0, (0.5f - chunkSize.Y)/2f);

        for (int j = 0; j < chunkSize.Y; j++)
        {
            for (int i = 0; i < chunkSize.X; i++)
            {
                Vector3 translation = new Vector3(i * triangleHeight, 0, (j + 0.5f * (i % 2)) * 1);
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
        var poligon = new CsgPolygon3D();

        poligon.Polygon = new Vector2[4] { new Vector2(0, 0), new Vector2(triangleHeight, 0.5f), new Vector2(2*triangleHeight, 0), new Vector2(triangleHeight, -0.5f) };
        poligon.Mode = CsgPolygon3D.ModeEnum.Depth;
        poligon.Position = new Vector3(-triangleHeight, 0.5f, 0);
        poligon.Basis = new Basis(new Vector3(1, 0, 0), (float)((-90f / 180f) * Math.PI));

        return poligon;
    }
}
