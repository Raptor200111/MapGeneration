using Godot;
using System;

public partial class TriangleTile : Tile
{
    float triangleHeight = (float)Math.Sqrt(3) / 2f;
    public override Transform3D[,] GetMatrix(float blockScale, Vector2I chunkSize, Vector3 origin)
    {
        Transform3D[,] matrix = new Transform3D[chunkSize.X, chunkSize.Y];
        Vector3 offset = new Vector3(-( (chunkSize.X/2) - (1 - chunkSize.X%2)*0.5f ) / 2.0f, 0, (1 - chunkSize.Y) * triangleHeight / 2f);

        for (int j = 0; j < chunkSize.Y; j++)
        {
            for (int i = 0; i < chunkSize.X; i++)
            {
                bool isOdd = ((i + j) % 2 == 0);

                Basis basis = isOdd ? new Basis(new Vector3(0, 1, 0), Mathf.Pi) : Basis.Identity;

                float shift = isOdd ? 1 : -1;
                Vector3 translation = new Vector3(i * 0.5f, 0, triangleHeight * (j + (shift * (0.5f - (1 / 3f)))));
                translation += offset;
                translation *= blockScale;
                translation += origin;

                matrix[i, j] = new Transform3D(basis, translation);
            }
        }

        return matrix;
    }

    public override CsgPrimitive3D GetPoligon()
    {
        var poligon = new CsgPolygon3D();

        poligon.Polygon = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(triangleHeight * 2 / 3f, 1), new Vector2(triangleHeight * 2 / 3f, 0)};
        poligon.Mode = CsgPolygon3D.ModeEnum.Spin;
        //pol.SpinDegrees = 360;
        poligon.SpinSides = 3;
        poligon.Position = new Vector3(0, 0, 0);
        poligon.Basis = new Basis(new Vector3(0, 1, 0), (float)((270f / 180f) * Math.PI));

        return poligon;
    }
}
