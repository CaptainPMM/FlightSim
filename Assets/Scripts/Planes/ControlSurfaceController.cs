using UnityEngine;

namespace Planes {
    public class ControlSurfaceController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private Transform _model = null;
        [SerializeField] private ControlSurfaceType _type = ControlSurfaceType.Aileron;
        [SerializeField] private bool _invert = false;
        [SerializeField, Range(0f, 90f)] private float _maxPosDeflection = 45f;
        [SerializeField, Range(0f, 90f)] private float _maxNegDeflection = 45f;

        public ControlSurfaceType Type => _type;
        public bool Invert => _invert;
        public float MaxPosDeflection => _maxPosDeflection;
        public float MaxNegDeflection => _maxNegDeflection;

        [Header("Setup/Gizmos")]
        [SerializeField, Min(0f)] private float _gizmoDeflectionLength = 1f;
        [SerializeField] private Color _gizmoDeflectionColor = Color.white;
        [SerializeField] private Color _gizmoSurfColor = Color.red;

        [Header("Runtime")]
        [SerializeField] private float _deflection = 0f;

        public float Deflection {
            get => _deflection;
            set {
                _deflection = Mathf.Clamp(value, -_maxNegDeflection, _maxPosDeflection);
                if (_type == ControlSurfaceType.Rudder) transform.localRotation = Quaternion.Euler(0f, _invert ? -_deflection : _deflection, 0f);
                else transform.localRotation = Quaternion.Euler(_invert ? -_deflection : _deflection, 0f, 0f);

                // Sync model transform
                _model.SetPositionAndRotation(transform.position, transform.rotation);
            }
        }

        private void Awake() {
            if (!_model) Debug.LogWarning("ControlSurfaceController: no model assigned");
        }

        private void Start() {
            UpdateValues();
        }

        private void UpdateValues() {
            if (!_model) return;
            Deflection = _deflection;
        }

        [ContextMenu("Set pos to model")]
        private void SetPosToModel() {
            if (!_model) return;
            transform.SetPositionAndRotation(_model.transform.position, _model.transform.rotation);
        }

        public float GetAxisLerpDeflection(float t) {
            return Mathf.Lerp(-_maxNegDeflection, _maxPosDeflection, Mathf.InverseLerp(-1f, 1f, t));
        }

        public void AxisLerpDeflection(float t) {
            Deflection = GetAxisLerpDeflection(t);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            SetPosToModel();
            UpdateValues();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = _gizmoDeflectionColor;
            if (_type == ControlSurfaceType.Rudder) {
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (_maxPosDeflection - _deflection), transform.up) * -transform.forward * _gizmoDeflectionLength);
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (-_maxNegDeflection - _deflection), transform.up) * -transform.forward * _gizmoDeflectionLength);
            } else {
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (_maxPosDeflection - _deflection), transform.right) * -transform.forward * _gizmoDeflectionLength);
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (-_maxNegDeflection - _deflection), transform.right) * -transform.forward * _gizmoDeflectionLength);
            }

            if (!_model) return;
            Bounds bounds = _model.GetComponent<MeshRenderer>().bounds;
            Gizmos.color = _gizmoSurfColor;
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2f);
        }
    }
#endif
}