using System.Collections.Generic;
using UnityEngine;

namespace Forge.Core
{
    /// <summary>
    /// Wraps a single 8^3-cell region of the block as a Unity GameObject with a
    /// MeshFilter + MeshRenderer + MeshCollider. This is a plain class (not a
    /// MonoBehaviour): it owns its GameObject but does not live on it, which
    /// keeps the Core layer independent of Unity's component lifecycle.
    ///
    /// Mesh vertices are in CHUNK-LOCAL space (origin at the chunk's first
    /// corner). The GameObject is offset by the chunk origin, so Unity gets a
    /// tight per-chunk bounds for frustum culling and clean raycasts.
    /// </summary>
    public sealed class Chunk
    {
        public Vector3Int Coord { get; }
        public bool IsDirty { get; private set; }
        public GameObject GameObject => _go;

        private readonly GameObject _go;
        private readonly MeshFilter _filter;
        private readonly MeshCollider _collider;
        private readonly Mesh _mesh;

        public Chunk(Vector3Int coord, Transform parent, Material material, float voxelSize, int chunkSize)
        {
            Coord = coord;

            _go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
            _go.transform.SetParent(parent, false);
            // Position the chunk at its origin corner in the block's local space.
            _go.transform.localPosition = (Vector3)coord * (chunkSize * voxelSize);

            _filter = _go.AddComponent<MeshFilter>();
            var renderer = _go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            _collider = _go.AddComponent<MeshCollider>();

            _mesh = new Mesh { name = $"ChunkMesh_{coord.x}_{coord.y}_{coord.z}" };
            _mesh.MarkDynamic(); // rebuilt frequently as the player forges
            _filter.sharedMesh = _mesh;

            IsDirty = true; // needs an initial mesh build
        }

        public void MarkDirty() => IsDirty = true;

        /// <summary>
        /// Uploads freshly generated geometry to the mesh and collider. The lists
        /// are owned by the caller (ChunkManager) and reused across chunks.
        /// </summary>
        public void ApplyMesh(List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
        {
            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetTriangles(triangles, 0);
            _mesh.SetNormals(normals); // gradient-based smooth normals from the generator (Oturum 6)
            _mesh.RecalculateBounds();

            // The MeshCollider must be re-assigned to re-cook collision data.
            // Skip empty meshes so we don't cook an invalid collision mesh.
            _collider.sharedMesh = null;
            if (vertices.Count > 0)
                _collider.sharedMesh = _mesh;

            IsDirty = false;
        }

        public void Dispose()
        {
            if (_mesh != null) Object.Destroy(_mesh);
            if (_go != null) Object.Destroy(_go);
        }
    }
}
