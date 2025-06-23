using Godot;
using System;
using System.Drawing;

public partial class SquareTile : Tile
{
    public override Transform3D[,] GetMatrix(Vector2I chunkSize)
    {
        Transform3D[,] matrix = new Transform3D[chunkSize.X, chunkSize.Y];
        Vector3 offset = new Vector3(0.5f - chunkSize.X/2f, 0, 0.5f - chunkSize.Y/2f);

        for (int j = 0; j < chunkSize.Y; j++)
        {
            for (int i = 0; i < chunkSize.X; i++)
            {
                Vector3 translation = new Vector3(i, 0, j);

                translation += offset;

                matrix[i, j] = new Transform3D(Basis.Identity, translation);
            }
        }

        return matrix;
    }

    public override ArrayMesh GetMesh()
    {
        float h = 0.5f;

        Vector3[] v =
        {
                new Vector3(-h, -h, -h), // 0
                new Vector3( h, -h, -h), // 1
                new Vector3( h,  h, -h), // 2
                new Vector3(-h,  h, -h), // 3
                new Vector3(-h, -h,  h), // 4
                new Vector3( h, -h,  h), // 5
                new Vector3( h,  h,  h), // 6
                new Vector3(-h,  h,  h)  // 7
            };

        int[] tri =
        {
                0, 1, 2, 0, 2, 3, // Cara -Z
                5, 4, 7, 5, 7, 6, // Cara +Z
                1, 5, 6, 1, 6, 2, // Cara +X
                4, 0, 3, 4, 3, 7, // Cara -X
                3, 2, 6, 3, 6, 7, // Cara +Y
                4, 5, 1, 4, 1, 0  // Cara -Y
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