using UnityEngine;
using Forge.Core;

namespace Forge
{
    /// <summary>
    /// Single-MonoBehaviour facade for the forge block. Owns the density field and
    /// the chunk grid, and turns player input into deformations. Everything below
    /// this class is plain C# (VoxelData / Chunk / ChunkManager / Marching Cubes);
    /// this is the only piece that touches Unity's component lifecycle and input.
    ///
    /// Scene setup: put this on an empty GameObject, assign a Material, ensure a
    /// camera is tagged MainCamera (or assign one). The chunk GameObjects are
    /// created as children at runtime. The block grows from the GameObject origin
    /// along +X/+Y/+Z; its size in meters is (cells * voxelSize) per axis.
    ///
    /// Input note: input is read through small helpers that compile against either
    /// the new Input System package or the legacy Input Manager, so this works
    /// regardless of the project's Active Input Handling setting.
    /// </summary>
    public sealed class ForgeBlock : MonoBehaviour
    {
        [Header("Block dimensions (in cells)")]
        [SerializeField] private int cellsX = 32;
        [SerializeField] private int cellsY = 16;
        [SerializeField] private int cellsZ = 32;

        [Tooltip("World size of one voxel cell, in meters.")]
        [SerializeField] private float voxelSize = 0.05f;

        [Header("Rendering")]
        [SerializeField] private Material blockMaterial;

        [Header("Deformation brush")]
        [Tooltip("Brush radius in meters.")]
        [SerializeField] private float brushRadius = 0.06f;
        [Tooltip("Density removed at the brush center per application (0..1).")]
        [SerializeField] private float brushStrength = 0.5f;

        [Header("Input")]
        [Tooltip("Camera used for mouse picking. Falls back to Camera.main.")]
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private float maxRayDistance = 100f;

        private VoxelData _voxels;
        private ChunkManager _chunks;

        private void Start()
        {
            if (raycastCamera == null) raycastCamera = Camera.main;

            // VoxelData is sized in SAMPLE POINTS (cells + 1) per axis.
            _voxels = new VoxelData(cellsX + 1, cellsY + 1, cellsZ + 1, voxelSize);
            _voxels.FillSolid();

            _chunks = new ChunkManager(_voxels, transform, blockMaterial);
            _chunks.BuildAll();
        }

        private void Update()
        {
            // Hold left mouse to forge. Each frame the brush carves a soft dent at
            // the cursor's hit point and only the touched chunks are rebuilt.
            if (!IsForgeButtonHeld() || raycastCamera == null) return;

            Ray ray = raycastCamera.ScreenPointToRay(PointerPosition());
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
                DeformAt(hit.point);
        }

        // --- Input abstraction ------------------------------------------------
        // These compile against whichever input backend the project is configured
        // for (new Input System package, legacy Input Manager, or both).

        private static bool IsForgeButtonHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        private static Vector3 PointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            return mouse != null ? (Vector3)mouse.position.ReadValue() : Vector3.zero;
#else
            return Input.mousePosition;
#endif
        }

        /// <summary>
        /// Applies a deformation at a WORLD-space point and rebuilds the affected
        /// chunks. This is the single entry point for all input sources: mouse
        /// today, a VR hammer collision (Hafta 2) tomorrow — the trigger changes,
        /// this rule stays.
        /// </summary>
        public void DeformAt(Vector3 worldPoint)
        {
            // InverseTransformPoint maps world -> the block's unscaled local space
            // (meters), which is exactly the space VoxelData and the chunk meshes
            // live in, regardless of how this GameObject is moved/rotated/scaled.
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

            _chunks.Deform(localPoint, brushRadius, brushStrength);
            _chunks.RegenerateDirty();
        }

        private void OnDestroy()
        {
            _chunks?.Dispose();
        }

        // Draws the block's bounds in the editor so it can be placed before play.
        private void OnDrawGizmosSelected()
        {
            Vector3 size = new Vector3(cellsX, cellsY, cellsZ) * voxelSize;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 1f);
            Gizmos.DrawWireCube(size * 0.5f, size);
        }
    }
}
