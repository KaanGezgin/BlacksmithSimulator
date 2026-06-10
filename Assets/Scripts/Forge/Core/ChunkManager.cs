using System.Collections.Generic;
using UnityEngine;
using Forge.MarchingCubes;

namespace Forge.Core
{
    /// <summary>
    /// Owns the density field and the grid of chunks. Translates a deformation
    /// into the set of affected chunks (dirty tracking) and rebuilds only those
    /// chunks' meshes. This is the piece that makes forging cheap: a hammer hit
    /// never recomputes the whole block, only the chunks it actually touched.
    ///
    /// Plain class, not a MonoBehaviour. The owning ForgeBlock injects the parent
    /// Transform and Material and drives Build/Regenerate from its Unity loop.
    /// </summary>
    public sealed class ChunkManager
    {
        /// <summary>Cells per chunk axis (matches kararlar.md: 8^3).</summary>
        public const int ChunkSize = 8;

        public VoxelData Voxels { get; }

        private readonly Dictionary<Vector3Int, Chunk> _chunks = new();
        private readonly HashSet<Chunk> _dirty = new();
        private readonly MarchingCubesGenerator _generator = new();

        private readonly Transform _parent;
        private readonly Material _material;

        private readonly int _chunksX, _chunksY, _chunksZ;

        // Reused per-regeneration buffers to avoid per-frame GC allocations.
        private readonly List<Vector3> _vertices = new();
        private readonly List<int> _triangles = new();

        public ChunkManager(VoxelData voxels, Transform parent, Material material)
        {
            Voxels = voxels;
            _parent = parent;
            _material = material;

            // Chunk counts are derived from the CELL count (points - 1).
            _chunksX = Mathf.CeilToInt((voxels.Width  - 1) / (float)ChunkSize);
            _chunksY = Mathf.CeilToInt((voxels.Height - 1) / (float)ChunkSize);
            _chunksZ = Mathf.CeilToInt((voxels.Depth  - 1) / (float)ChunkSize);

            CreateChunks();
        }

        private void CreateChunks()
        {
            for (int z = 0; z < _chunksZ; z++)
            for (int y = 0; y < _chunksY; y++)
            for (int x = 0; x < _chunksX; x++)
            {
                var coord = new Vector3Int(x, y, z);
                var chunk = new Chunk(coord, _parent, _material, Voxels.VoxelSize, ChunkSize);
                _chunks[coord] = chunk;
                _dirty.Add(chunk); // every chunk needs its first mesh build
            }
        }

        /// <summary>Builds the initial mesh for every chunk. Call once after construction.</summary>
        public void BuildAll() => RegenerateDirty();

        /// <summary>
        /// Applies a deformation in block-local space and marks every chunk whose
        /// 9-point sampling window was touched as dirty (boundary-shared chunks
        /// included). Does not rebuild meshes — call RegenerateDirty() afterwards.
        /// </summary>
        public void Deform(Vector3 localPoint, float radius, float strength)
        {
            if (Voxels.ApplyDeformation(localPoint, radius, strength, out var min, out var max))
                MarkRange(min, max);
        }

        /// <summary>Rebuilds the mesh of every dirty chunk. No-op when nothing is dirty.</summary>
        public void RegenerateDirty()
        {
            if (_dirty.Count == 0) return;

            foreach (var chunk in _dirty)
            {
                _generator.Generate(Voxels, chunk.Coord, ChunkSize, _vertices, _triangles);
                chunk.ApplyMesh(_vertices, _triangles);
            }
            _dirty.Clear();
        }

        public void Dispose()
        {
            foreach (var chunk in _chunks.Values)
                chunk.Dispose();
            _chunks.Clear();
            _dirty.Clear();
        }

        // --- Dirty mapping --------------------------------------------------

        private void MarkRange(Vector3Int pMin, Vector3Int pMax)
        {
            int cxMin = ChunkMinForPoint(pMin.x);
            int cyMin = ChunkMinForPoint(pMin.y);
            int czMin = ChunkMinForPoint(pMin.z);
            int cxMax = ChunkMaxForPoint(pMax.x, _chunksX);
            int cyMax = ChunkMaxForPoint(pMax.y, _chunksY);
            int czMax = ChunkMaxForPoint(pMax.z, _chunksZ);

            for (int z = czMin; z <= czMax; z++)
            for (int y = cyMin; y <= cyMax; y++)
            for (int x = cxMin; x <= cxMax; x++)
            {
                if (_chunks.TryGetValue(new Vector3Int(x, y, z), out var chunk))
                {
                    chunk.MarkDirty();
                    _dirty.Add(chunk);
                }
            }
        }

        // A chunk c reads points [c*ChunkSize .. c*ChunkSize + ChunkSize]. It
        // overlaps point p when c*ChunkSize + ChunkSize >= p, i.e. c >= (p - ChunkSize)/ChunkSize.
        private static int ChunkMinForPoint(int p) =>
            Mathf.Max(0, Mathf.CeilToInt((p - ChunkSize) / (float)ChunkSize));

        // ...and when c*ChunkSize <= p, i.e. c <= p/ChunkSize. A point sitting
        // exactly on a chunk seam therefore marks both neighbouring chunks.
        private static int ChunkMaxForPoint(int p, int count) =>
            Mathf.Min(count - 1, p / ChunkSize);
    }
}
