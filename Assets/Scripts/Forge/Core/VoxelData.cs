using UnityEngine;

namespace Forge.Core
{
    /// <summary>
    /// Holds the scalar density field for the entire forge block and owns the
    /// rules for how a deformation changes that field. This class is pure data
    /// plus math: it knows nothing about chunks, meshes or GameObjects.
    ///
    /// Density convention: 1.0 = solid metal, 0.0 = air. The Marching Cubes
    /// surface is extracted at the ISO threshold (0.5).
    ///
    /// Storage is a flat managed float[] in x-fastest order. The layout is
    /// deliberately identical to a NativeArray&lt;float&gt;, so a later migration
    /// to the Burst/Job System is an internal change behind this API only and
    /// does not touch any calling code.
    ///
    /// Coordinates: positions handed to this class are in the block's LOCAL
    /// space (meters, relative to the block origin). The owning MonoBehaviour
    /// (ForgeBlock) is responsible for world &lt;-&gt; local conversion.
    /// </summary>
    public sealed class VoxelData
    {
        /// <summary>Iso level the surface is extracted at. 1.0 = metal, 0.0 = air.</summary>
        public const float IsoThreshold = 0.5f;

        // Number of density SAMPLE POINTS along each axis (not cells). A region
        // of N cells needs N+1 sample points; Marching Cubes reads the points,
        // so callers pass cells+1 here.
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        /// <summary>World-space size of a single voxel cell, in meters.</summary>
        public float VoxelSize { get; }

        private readonly float[] _density;

        public VoxelData(int width, int height, int depth, float voxelSize)
        {
            Width = width;
            Height = height;
            Depth = depth;
            VoxelSize = voxelSize;
            _density = new float[width * height * depth];
        }

        /// <summary>Flat index for (x,y,z) in x-fastest order. No bounds check.</summary>
        public int Index(int x, int y, int z)
        {
            return x + y * Width + z * Width * Height;
        }

        public bool InBounds(int x, int y, int z)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height &&
                   z >= 0 && z < Depth;
        }

        /// <summary>
        /// Reads a sample. Out-of-range samples read as air (0) so chunk borders
        /// and the block edges close into a watertight surface.
        /// </summary>
        public float GetDensity(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return 0f;
            return _density[Index(x, y, z)];
        }

        public void SetDensity(int x, int y, int z, float value)
        {
            if (!InBounds(x, y, z)) return;
            _density[Index(x, y, z)] = Mathf.Clamp01(value);
        }

        /// <summary>Local-space position (meters) of the sample point (x,y,z).</summary>
        public Vector3 PointToLocal(int x, int y, int z)
        {
            return new Vector3(x, y, z) * VoxelSize;
        }

        /// <summary>
        /// Initializes the field as a solid billet. Each point's density is
        /// derived from the signed distance to the billet surface, so Marching
        /// Cubes produces clean, smooth faces instead of hard binary stair-steps.
        ///
        /// The billet is inset by one voxel from the grid edge so the iso-surface
        /// is fully enclosed by air on every side (a watertight mesh).
        /// </summary>
        public void FillSolid()
        {
            Vector3 min = Vector3.one * VoxelSize;                       // one-voxel margin
            Vector3 max = new Vector3(Width - 2, Height - 2, Depth - 2) * VoxelSize;
            Vector3 center = (min + max) * 0.5f;
            Vector3 halfExtents = (max - min) * 0.5f;

            for (int z = 0; z < Depth; z++)
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                Vector3 p = PointToLocal(x, y, z);
                float sd = BoxSignedDistance(p - center, halfExtents);  // <0 inside, >0 outside

                // Map signed distance (meters) onto density around the iso level.
                // Dividing by VoxelSize spreads the transition across roughly one
                // voxel, which keeps the surface smooth without rounding corners.
                float density = IsoThreshold - sd / VoxelSize;
                _density[Index(x, y, z)] = Mathf.Clamp01(density);
            }
        }

        /// <summary>
        /// Applies a soft spherical dent at <paramref name="localCenter"/>: density
        /// is reduced most at the brush center and falls off smoothly to zero at
        /// the radius. Hafta 1 keeps this as pure removal (denting); material
        /// displacement / overflow is a deliberately deferred Hafta 3+ feature.
        ///
        /// Returns the inclusive voxel index range that was touched via the out
        /// params, so the caller (ChunkManager) can map it to dirty chunks. The
        /// return value is false when nothing changed (brush missed the grid or
        /// the region was already air).
        /// </summary>
        public bool ApplyDeformation(
            Vector3 localCenter, float radius, float strength,
            out Vector3Int affectedMin, out Vector3Int affectedMax)
        {
            // Convert the affected world sphere to an inclusive, clamped voxel range.
            int minX = Mathf.Clamp(Mathf.FloorToInt((localCenter.x - radius) / VoxelSize), 0, Width - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt((localCenter.y - radius) / VoxelSize), 0, Height - 1);
            int minZ = Mathf.Clamp(Mathf.FloorToInt((localCenter.z - radius) / VoxelSize), 0, Depth - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt((localCenter.x + radius) / VoxelSize), 0, Width - 1);
            int maxY = Mathf.Clamp(Mathf.CeilToInt((localCenter.y + radius) / VoxelSize), 0, Height - 1);
            int maxZ = Mathf.Clamp(Mathf.CeilToInt((localCenter.z + radius) / VoxelSize), 0, Depth - 1);

            affectedMin = new Vector3Int(minX, minY, minZ);
            affectedMax = new Vector3Int(maxX, maxY, maxZ);

            bool changed = false;

            for (int z = minZ; z <= maxZ; z++)
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                Vector3 p = PointToLocal(x, y, z);
                float dist = Vector3.Distance(p, localCenter);
                if (dist > radius) continue;

                // Smoothstep falloff: full strength at the center, zero at the rim.
                float t = 1f - dist / radius;
                float falloff = t * t * (3f - 2f * t);

                int idx = Index(x, y, z);
                float newValue = Mathf.Clamp01(_density[idx] - strength * falloff);
                if (newValue != _density[idx])
                {
                    _density[idx] = newValue;
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// Signed distance from a box centered at the origin. Negative inside,
        /// positive outside. Standard analytic box SDF.
        /// </summary>
        private static float BoxSignedDistance(Vector3 p, Vector3 halfExtents)
        {
            Vector3 q = new Vector3(
                Mathf.Abs(p.x) - halfExtents.x,
                Mathf.Abs(p.y) - halfExtents.y,
                Mathf.Abs(p.z) - halfExtents.z);

            float outside = new Vector3(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f), Mathf.Max(q.z, 0f)).magnitude;
            float inside = Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f);
            return outside + inside;
        }
    }
}
