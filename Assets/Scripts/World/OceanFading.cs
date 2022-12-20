using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace World {
    [RequireComponent(typeof(WaterSurface))]
    public class OceanFading : MonoBehaviour {
        private WaterSurface _waterSurface;
        private Camera _cam;

        [SerializeField] private MeshRenderer _simpleWaterMesh = null;
        [SerializeField] private Material _simpleWaterOpaqueMat = null;
        [SerializeField] private Material _simpleWaterTranspMat = null;
        [SerializeField] private float _transitionStartHeight = 500f;
        [SerializeField] private float _transitionEndHeight = 1000f;

        private float _currCamHeight = 0f;
        private Color _simpleWaterColor;

        private void Awake() {
            _waterSurface = GetComponent<WaterSurface>();
            _cam = Camera.main;
            if (!_cam) Debug.LogWarning("OceanFading: no camera found");
            if (!_simpleWaterMesh) Debug.LogWarning("OceanFading: no simpleWaterMesh assigned");
            else _simpleWaterMesh.gameObject.SetActive(false);
            if (!_simpleWaterOpaqueMat) Debug.LogWarning("OceanFading: no opaque water mat assigned");
            if (!_simpleWaterTranspMat) Debug.LogWarning("OceanFading: no transparent water mat assigned");
            else _simpleWaterMesh.material = _simpleWaterTranspMat;
            _simpleWaterColor = _simpleWaterMesh.material.color;
        }

        private void Update() {
            _currCamHeight = _cam.transform.position.y;

            if (_waterSurface.enabled && _currCamHeight > _transitionEndHeight) {
                _waterSurface.enabled = false;
                _simpleWaterMesh.material = _simpleWaterOpaqueMat;
            } else if (!_waterSurface.enabled && _currCamHeight < _transitionEndHeight) {
                _waterSurface.enabled = true;
                _simpleWaterMesh.material = _simpleWaterTranspMat;
            }

            if (!_simpleWaterMesh.gameObject.activeSelf && _currCamHeight > _transitionStartHeight) _simpleWaterMesh.gameObject.SetActive(true);
            else if (_simpleWaterMesh.gameObject.activeSelf && _currCamHeight < _transitionStartHeight) _simpleWaterMesh.gameObject.SetActive(false);

            if (_currCamHeight > _transitionStartHeight && _currCamHeight < _transitionEndHeight) {
                _simpleWaterColor.a = Mathf.InverseLerp(_transitionStartHeight, _transitionEndHeight, _currCamHeight);
                _simpleWaterMesh.material.color = _simpleWaterColor;
            }
        }
    }
}