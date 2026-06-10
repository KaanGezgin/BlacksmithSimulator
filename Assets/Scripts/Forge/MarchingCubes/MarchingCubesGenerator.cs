using System.Collections.Generic;
using UnityEngine;
using Forge.Core;

namespace Forge.MarchingCubes
{
    /// <summary>
    /// Extracts a chunk-local triangle mesh from the global density field using
    /// the Marching Cubes algorithm.
    ///
    /// STUB (Oturum 2): this only locks the contract so the chunk/dirty/regen
    /// pipeline compiles and can be wired end to end. It produces no geometry
    /// yet — chunks exist and track dirty state but render nothing.
    ///
    /// TODO (Oturum 4): implement the real algorithm. For each of the chunk's
    /// 8^3 cells, sample the eight corner densities from <paramref name="data"/>
    /// at global indices (chunkCoord * chunkSize + localCorner), build the cube
    /// index against VoxelData.IsoThreshold, look up edges/triangles in
    /// MarchingCubesTables (Oturum 3), and interpolate vertices along the edges.
    /// Vertices must be written in chunk-local space (meters).
    /// </summary>
    public sealed class MarchingCubesGenerator
    {
        public void Generate(
            VoxelData data, Vector3Int chunkCoord, int chunkSize,
            List<Vector3> vertices, List<int> triangles)
        {
            vertices.Clear();
            triangles.Clear();
            // Intentionally empty until Oturum 4.
        }
    }
}
