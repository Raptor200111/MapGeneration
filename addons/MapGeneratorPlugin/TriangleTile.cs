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

    public override ArrayMesh GetMesh()
    {
        float r = 1f / Mathf.Sqrt(3.0f);
        float startAngle = Mathf.Pi / 2.0f;

        Vector3[] v = new Vector3[6];
        for (int i = 0; i < 3; i++)
        {
            float ang = startAngle + i * 2.0f * Mathf.Pi / 3.0f;
            float x = r * Mathf.Cos(ang);
            float z = r * Mathf.Sin(ang);

            v[i] = new Vector3(-x, 0.5f, z);
            v[i + 3] = new Vector3(-x, -0.5f, z);
        }

        int[] tri =
        {
            //base
            2,1,0,
            //tapa
            3,4,5,
            //lados
            0,1,4, 0,4,3,
            1,2,5, 1,5,4,
            2,0,3, 2,3,5
        };

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
