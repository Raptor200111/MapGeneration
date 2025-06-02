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

    public override ArrayMesh GetMesh()
    {
        Vector3[] v = new Vector3[12];
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.Pi / 3f;
            float x = 1 * Mathf.Cos(angle);
            float z = 1 * Mathf.Sin(angle);

            v[i] = new Vector3(x, 0f, z); // base
            v[i + 6] = new Vector3(x, 1, z); // tapa
        }

        var tri = new int[60];
        int t = 0;

        //Base
        for (int i = 1; i < 5; i++)
        {
            tri[t++] = i + 1;
            tri[t++] = i;
            tri[t++] = 0;
        }

        //Tapa
        for (int i = 1; i < 5; i++)
        {
            tri[t++] = 6;
            tri[t++] = i + 6;
            tri[t++] = i + 1 + 6;
        }

        //Caras laterales
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            tri[t++] = i;
            tri[t++] = next;
            tri[t++] = next + 6;

            tri[t++] = i;
            tri[t++] = next + 6;
            tri[t++] = i + 6;
        }

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < tri.Length; i += 3)
        {
            Vector3 a = v[tri[i]];
            Vector3 b = v[tri[i + 1]];
            Vector3 c = v[tri[i + 2]];

            Vector3 n = (b - a).Cross(c - a).Normalized();

            st.SetNormal(n); st.AddVertex(a);
            st.SetNormal(n); st.AddVertex(b);
            st.SetNormal(n); st.AddVertex(c);
        }

        return st.Commit();
    }
}
