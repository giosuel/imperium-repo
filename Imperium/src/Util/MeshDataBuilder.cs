#region

using UnityEngine;

#endregion

namespace Imperium.Util;

/// <summary>
///     Used to recalculate the meshd data for the wireframe shader.
///     Source: https://github.com/ArturoNereu/WireframeShaderGraph
/// </summary>
public class MeshDataBuilder : MonoBehaviour
{
    private void Awake()
    {
        GenerateMeshData();
    }

    /// <summary>
    ///     We will assign a color to each Vertex in a Triangle on the object's mesh
    /// </summary>
    private void GenerateMeshData()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;

        SplitMesh(mesh);
        SetVertexColors(mesh);
    }

    /// <summary>
    ///     For this approach, we need to make sure there are not shared vertices
    ///     on the mesh, that's why we use this method to split the mesh.
    ///     This will increase the number of vertices, so less optimized.
    /// </summary>
    /// <param name="mesh"></param>
    private void SplitMesh(Mesh mesh)
    {
        var triangles = mesh.triangles;
        var verts = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;

        Vector3[] newVerts;
        Vector3[] newNormals;
        Vector2[] newUvs;

        var n = triangles.Length;
        newVerts = new Vector3[n];
        newNormals = new Vector3[n];
        newUvs = new Vector2[n];

        for (var i = 0; i < n; i++)
        {
            newVerts[i] = verts[triangles[i]];
            if (triangles.Length > i && normals.Length > triangles[i]) newNormals[i] = normals[triangles[i]];
            if (uvs.Length > 0)
            {
                newUvs[i] = uvs[triangles[i]];
            }

            triangles[i] = i;
        }

        mesh.vertices = newVerts;
        mesh.normals = newNormals;
        mesh.uv = newUvs;
        mesh.triangles = triangles;
    }

    /// <summary>
    ///     We paint the vertex color
    /// </summary>
    /// <param name="mesh"></param>
    private void SetVertexColors(Mesh mesh)
    {
        var colorCoords = new[]
        {
            new Color(1, 0, 0),
            new Color(0, 1, 0),
            new Color(0, 0, 1),
        };

        var vertexColors = new Color32[mesh.vertices.Length];

        for (var i = 0; i < vertexColors.Length; i += 3)
        {
            vertexColors[i] = colorCoords[0];
            vertexColors[i + 1] = colorCoords[1];
            vertexColors[i + 2] = colorCoords[2];
        }

        mesh.colors32 = vertexColors;
    }
}