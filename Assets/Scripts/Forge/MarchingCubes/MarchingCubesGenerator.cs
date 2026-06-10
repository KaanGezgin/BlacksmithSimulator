using System.Collections.Generic;
using UnityEngine;
using Forge.Core;

namespace Forge.MarchingCubes
{
    /// <summary>
    /// Extracts a chunk-local triangle mesh from the global density field using
    /// the Marching Cubes algorithm (Paul Bourke's edge/triangle tables, see
    /// <see cref="MarchingCubesTables"/>).
    ///
    /// The chunk owns 8^3 cells. Cell (lx,ly,lz) reads its eight corner densities
    /// from the GLOBAL field at (chunkCoord*chunkSize + local + cornerOffset), so
    /// the per-chunk sampling windows overlap by one point and neighbouring chunks
    /// produce identical, perfectly coincident boundary vertices (watertight, no
    /// seams). Vertices are emitted in CHUNK-LOCAL space (meters): the same local
    /// point on a shared face maps to the same world position once the chunk's
    /// GameObject offset is applied.
    ///
    /// Cube-index convention: bit i is set when corner i is INSIDE the metal
    /// (density >= iso). Our field is "solid = high density", and Unity is a
    /// left-handed / clockwise-front-face engine whose RecalculateNormals derives
    /// the front normal as (b-a)x(c-a). With this bit convention and the table's
    /// vertex order, that normal points OUTWARD. (If a future Unity test shows the
    /// surface rendering inside-out, the one-line fix is to reverse the triangle
    /// winding where indices are appended below — do not touch the tables.)
    ///
    /// Vertices are not welded between cells; RecalculateNormals on the chunk mesh
    /// yields faceted shading for now. Smooth (shared-normal) output is a
    /// deliberately deferred Oturum 6 polish item.
    /// </summary>
    public sealed class MarchingCubesGenerator
    {
        // Per-cell scratch, kept as fields so a full regeneration allocates nothing.
        private readonly float[] _cornerDensity = new float[8];   // corner order = CornerOffsets
        private readonly Vector3[] _cornerLocal = new Vector3[8]; // chunk-local meters
        private readonly Vector3[] _edgeVertices = new Vector3[12]; // filled per EdgeTable mask

        public void Generate(
            VoxelData data, Vector3Int chunkCoord, int chunkSize,
            List<Vector3> vertices, List<int> triangles)
        {
            vertices.Clear();
            triangles.Clear();

            Vector3Int origin = chunkCoord * chunkSize; // this chunk's first global sample point
            float voxelSize = data.VoxelSize;

            for (int lz = 0; lz < chunkSize; lz++)
            for (int ly = 0; ly < chunkSize; ly++)
            for (int lx = 0; lx < chunkSize; lx++)
            {
                // --- Sample the eight cube corners (density + local position) ---
                int cubeIndex = 0;
                for (int i = 0; i < 8; i++)
                {
                    Vector3Int o = MarchingCubesTables.CornerOffsets[i];
                    float density = data.GetDensity(origin.x + lx + o.x,
                                                    origin.y + ly + o.y,
                                                    origin.z + lz + o.z);
                    _cornerDensity[i] = density;
                    _cornerLocal[i] = new Vector3(lx + o.x, ly + o.y, lz + o.z) * voxelSize;

                    if (density >= VoxelData.IsoThreshold)
                        cubeIndex |= 1 << i;
                }

                // Fully inside or fully outside the metal -> the surface misses this cell.
                int edgeMask = MarchingCubesTables.EdgeTable[cubeIndex];
                if (edgeMask == 0) continue;

                // --- Interpolate a vertex on every edge the surface crosses ---
                for (int e = 0; e < 12; e++)
                {
                    if ((edgeMask & (1 << e)) == 0) continue;
                    int a = MarchingCubesTables.EdgeConnections[e][0];
                    int b = MarchingCubesTables.EdgeConnections[e][1];
                    _edgeVertices[e] = InterpolateVertex(
                        _cornerLocal[a], _cornerLocal[b], _cornerDensity[a], _cornerDensity[b]);
                }

                // --- Emit triangles, reading edge ids from TriTable (-1 terminated) ---
                for (int t = 0; MarchingCubesTables.TriTable[cubeIndex, t] != -1; t += 3)
                {
                    int e0 = MarchingCubesTables.TriTable[cubeIndex, t];
                    int e1 = MarchingCubesTables.TriTable[cubeIndex, t + 1];
                    int e2 = MarchingCubesTables.TriTable[cubeIndex, t + 2];

                    int baseIndex = vertices.Count;
                    vertices.Add(_edgeVertices[e0]);
                    vertices.Add(_edgeVertices[e1]);
                    vertices.Add(_edgeVertices[e2]);
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                }
            }
        }

        /// <summary>
        /// Linearly interpolates the surface crossing on the edge p1-p2, where the
        /// density passes through the iso level. Returns the point in the same
        /// (chunk-local) space the corner positions were given in.
        /// </summary>
        private static Vector3 InterpolateVertex(Vector3 p1, Vector3 p2, float d1, float d2)
        {
            const float iso = VoxelData.IsoThreshold;
            const float epsilon = 1e-6f;

            // Degenerate edges: snap to an endpoint instead of dividing by ~0.
            if (Mathf.Abs(iso - d1) < epsilon) return p1;
            if (Mathf.Abs(iso - d2) < epsilon) return p2;
            if (Mathf.Abs(d1 - d2) < epsilon) return p1;

            float t = (iso - d1) / (d2 - d1);
            return p1 + t * (p2 - p1);
        }
    }
}
