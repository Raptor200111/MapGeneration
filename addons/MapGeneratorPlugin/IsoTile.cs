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

    public override ArrayMesh GetMesh()
    {
        float hx = 1 * Mathf.Sqrt(3f) * 0.5f;
        float halfVer = 1 * 0.5f;

        Vector3[] v =
        {
            new Vector3(-hx, 0f, -halfVer), // 0
            new Vector3( 0f, 0f, -1),       // 1
            new Vector3( hx, 0f, -halfVer), // 2
            new Vector3( 0f, 0f,  0f),      // 3

            new Vector3(-hx, 1, -halfVer),  // 4
            new Vector3( 0f, 1, -1),        // 5
            new Vector3( hx, 1, -halfVer),  // 6
            new Vector3( 0f, 1,  0f)        // 7
        };

        int[] tri =
        {
            // Base
            2, 1, 0,
            3, 2, 0,

            // Tapa
            4, 5, 6,
            4, 6, 7,

            // Laterales
            0, 1, 5,   0, 5, 4,
            1, 2, 6,   1, 6, 5,
            2, 3, 7,   2, 7, 6,
            3, 0, 4,   3, 4, 7
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
