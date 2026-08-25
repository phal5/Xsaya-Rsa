using UnityEngine;

namespace Art.Shaders
{
    /// <summary>
    /// Disables frustum culling by expanding mesh bounds so deformed/twisted vertices
    /// remain visible regardless of the original mesh bounding box.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public class DisableFrustumCulling : MonoBehaviour
    {
        [Tooltip("Expanded bounding box size to prevent frustum culling during vertex deformation")]
        public Vector3 expandedSize = new Vector3(100000f, 100000f, 100000f);

        private MeshFilter _meshFilter;
        private Mesh _instancedMesh;

        private void OnEnable()
        {
            ApplyExpandedBounds();
        }

        private void Start()
        {
            ApplyExpandedBounds();
        }

        private void ApplyExpandedBounds()
        {
            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            if (_meshFilter != null)
            {
                if (Application.isPlaying)
                {
                    if (_instancedMesh == null)
                    {
                        _instancedMesh = _meshFilter.mesh;
                    }
                    if (_instancedMesh != null)
                    {
                        _instancedMesh.bounds = new Bounds(Vector3.zero, expandedSize);
                    }
                }
                else
                {
                    if (_meshFilter.sharedMesh != null)
                    {
                        // In edit mode, clone mesh to avoid modifying asset file permanently
                        if (_instancedMesh == null)
                        {
                            _instancedMesh = Instantiate(_meshFilter.sharedMesh);
                            _instancedMesh.name = _meshFilter.sharedMesh.name + "_NoCull";
                            _meshFilter.sharedMesh = _instancedMesh;
                        }
                        _instancedMesh.bounds = new Bounds(Vector3.zero, expandedSize);
                    }
                }
            }

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.localBounds = new Bounds(Vector3.zero, expandedSize);
            }
        }

        private void OnDisable()
        {
            // Restore or clean up if needed
        }
    }
}
