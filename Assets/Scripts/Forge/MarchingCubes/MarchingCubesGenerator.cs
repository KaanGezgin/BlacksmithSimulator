using System.Collections.Generic;
using UnityEngine;
using Forge.Core;

namespace Forge.MarchingCubes
{
    /// <summary>
    /// Extracts a chunk-local triangle mesh (positions + smooth normals) from the
    /// global density field using the Marching Cubes algorithm (Paul Bourke's
    /// edge/triangle tables, see <see cref="MarchingCubesTables"/>).
    ///
    /// The chunk owns 8^3 cells. Cell (lx,ly,lz) reads its eight corner densities
    /// from the GLOBAL field at (chunkCoord*chunkSize + local + cornerOffset), so
    /// the per-chunk sampling windows overlap by one point and neighbouring chunks
    /// produce identical, perfectly coincident boundary vertices (watertight, no
    /// seams). Vertices are emitted in CHUNK-LOCAL space (meters).
    ///
    /// Cube-index convention: bit i is set when corner i is INSIDE the metal
    /// (density >= iso). With this convention and the table's vertex order, the
    /// triangle winding gives OUTWARD front faces under Unity's left-handed /
    /// clockwise rasterizer. (If a future test shows the surface inside-out, the
    /// one-line fix is to reverse the winding where indices are appended — do not
    /// touch the tables.)
    ///
    /// Normals (Oturum 6): each vertex normal comes from the density field's
    /// gradient, not from face averaging. Because the gradient is sampled from the
    /// shared global field, vertices on a chunk boundary get identical normals and
    /// the lighting is seamless across chunks without welding vertices.
    /// </summary>
    public sealed class MarchingCubesGenerator
    {
        // Per-cell scratch, kept as fields so a full regeneration allocates nothing.
        private readonly float[] _cornerDensity = new float[8];     // corner order = CornerOffsets
        private readonly Vector3[] _cornerLocal = new Vector3[8];   // chunk-local meters
        private readonly Vector3[] _cornerGradient = new Vector3[8]; // density gradient (points inward)
        private readonly Vector3[] _edgeVertices = new Vector3[12]; // filled per EdgeTable mask
        private readonly Vector3[] _edgeNormals = new Vector3[12];

        public void Generate(
            VoxelData data, Vector3Int chunkCoord, int chunkSize,
            List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
        {
            vertices.Clear();
            normals.Clear();
            triangles.Clear();

            Vector3Int origin = chunkCoord * chunkSize; // this chunk's first global sample point
            float voxelSize = data.VoxelSize;

            for (int lz = 0; lz < chunkSize; lz++)
            for (int ly = 0; ly < chunkSize; ly++)
            for (int lx = 0; lx < chunkSize; lx++)
            {
                // --- Sample the eight cube corners (density, local position, gradient) ---
                int cubeIndex = 0;
                for (int i = 0; i < 8; i++)
                {
                    Vector3Int o = MarchingCubesTables.CornerOffsets[i];
                    int gx = origin.x + lx + o.x;
                    int gy = origin.y + ly + o.y;
                    int gz = origin.z + lz + o.z;

                    float density = data.GetDensity(gx, gy, gz);
                    _cornerDensity[i] = density;
                    _cornerLocal[i] = new Vector3(lx + o.x, ly + o.y, lz + o.z) * voxelSize;
                    _cornerGradient[i] = SampleGradient(data, gx, gy, gz);

                    if (density >= VoxelData.IsoThreshold)
                        cubeIndex |= 1 << i;
                }

                // Fully inside or fully outside the metal -> the surface misses this cell.
                int edgeMask = MarchingCubesTables.EdgeTable[cubeIndex];
                if (edgeMask == 0) continue;

                // --- Interpolate a vertex + normal on every edge the surface crosses ---
                for (int e = 0; e < 12; e++)
                {
                    if ((edgeMask & (1 << e)) == 0) continue;
                    int a = MarchingCubesTables.EdgeConnections[e][0];
                    int b = MarchingCubesTables.EdgeConnections[e][1];

                    float t = InterpFactor(_cornerDensity[a], _cornerDensity[b]);
                    _edgeVertices[e] = Vector3.Lerp(_cornerLocal[a], _cornerLocal[b], t);

                    // Outward normal = away from solid = direction of decreasing
                    // density = -gradient, interpolated with the same factor.
                    Vector3 g = Vector3.Lerp(_cornerGradient[a], _cornerGradient[b], t);
                    _edgeNormals[e] = NormalizeOutward(g);
                }

                // --- Emit triangles, reading edge ids from TriTable (-1 terminated) ---
                for (int t = 0; MarchingCubesTables.TriTable[cubeIndex, t] != -1; t += 3)
                {
                    int e0 = MarchingCubesTables.TriTable[cubeIndex, t];
                    int e1 = MarchingCubesTables.TriTable[cubeIndex, t + 1];
                    int e2 = MarchingCubesTables.TriTable[cubeIndex, t + 2];

                    int baseIndex = vertices.Count;
                    vertices.Add(_edgeVertices[e0]); normals.Add(_edgeNormals[e0]);
                    vertices.Add(_edgeVertices[e1]); normals.Add(_edgeNormals[e1]);
                    vertices.Add(_edgeVertices[e2]); normals.Add(_edgeNormals[e2]);
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                }
            }
        }

        /// <summary>
        /// Fraction along the edge p1-p2 where the density crosses the iso level.
        /// Degenerate edges snap to an endpoint instead of dividing by ~0.
        /// </summary>
        private static float InterpFactor(float d1, float d2)
        {
            const float iso = VoxelData.IsoThreshold;
            const float epsilon = 1e-6f;
            if (Mathf.Abs(iso - d1) < epsilon) return 0f;
            if (Mathf.Abs(iso - d2) < epsilon) return 1f;
            if (Mathf.Abs(d1 - d2) < epsilon) return 0f;
            return (iso - d1) / (d2 - d1);
        }

        /// <summary>
        /// Central-difference gradient of the density field at a sample point.
        /// Points toward increasing density (inward, since solid = high density).
        /// Out-of-range samples read as air via GetDensity, so block edges still
        /// produce sensible gradients.
        /// </summary>
        private static Vector3 SampleGradient(VoxelData data, int x, int y, int z)
        {
            float gx = data.GetDensity(x + 1, y, z) - data.GetDensity(x - 1, y, z);
            float gy = data.GetDensity(x, y + 1, z) - data.GetDensity(x, y - 1, z);
            float gz = data.GetDensity(x, y, z + 1) - data.GetDensity(x, y, z - 1);
            return new Vector3(gx, gy, gz);
        }

        private static Vector3 NormalizeOutward(Vector3 gradient)
        {
            Vector3 n = -gradient;
            float m = n.magnitude;
            return m > 1e-6f ? n / m : Vector3.up; // flat fully-solid/air fallback (rare)
        }
    }
}
