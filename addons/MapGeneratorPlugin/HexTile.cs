using Godot;
using System;

public partial class HexTile : Tile
{
    float hexagonHeight = (float)Math.Sqrt(3);
    public override Transform3D[,] GetMatrix(float blockScale, Vector2I chunkSize, Vector3 origin)
    {
        Transform3D[,] matrix = new Transform3D[chunkSize.X, chunkSize.Y];
        Vector3 offset = new Vector3((3 - (3*chunkSize.X))/4f, 0, hexagonHeight/2f * (0.5f - chunkSize.Y) );

        for (int j = 0; j < chunkSize.Y; j++)
        {
            for (int i = 0; i < chunkSize.X; i++)
            {
                Vector3 translation = new Vector3(i * 1.5f, 0, (j + 0.5f*(i%2)) * hexagonHeight );
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

        poligon.Polygon = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        poligon.Mode = CsgPolygon3D.ModeEnum.Spin;
        poligon.SpinSides = 6;
        poligon.Position = new Vector3(0, 0, 0);
        //poligon.Basis = new Basis(new Vector3(0, 1, 0), (float)((270f / 180f) * Math.PI));

        return poligon;
    }
}
