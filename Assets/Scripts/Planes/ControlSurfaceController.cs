using UnityEngine;

namespace Planes {
    public class ControlSurfaceController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private Transform _model = null;
        [SerializeField] private ControlSurfaceType _type = ControlSurfaceType.Aileron;
        [SerializeField] private bool _invert = false;

        public ControlSurfaceType Type => _type;
        public bool Invert => _invert;

        [Header("Settings")]
        [SerializeField, Range(0f, 90f)] private float _maxPosDeflection = 45f;
        [SerializeField, Range(0f, 90f)] private float _maxNegDeflection = 45f;
        [SerializeField, Range(0f, 90f)] private float _maxTrim = 10f;
        [SerializeField, Min(0f)] private float _trimSpeed = 1f;

        public float MaxPosDeflection => _maxPosDeflection;
        public float MaxNegDeflection => _maxNegDeflection;
        public float TrimSpeed => _trimSpeed * Time.deltaTime;

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField, Min(0f)] private float _gizmoDeflectionLength = 1f;
        [SerializeField] private Color _gizmoDeflectionColor = Color.white;
        [SerializeField] private Color _gizmoSurfColor = Color.white;
        [SerializeField] private bool _gizmoShowSurf = true;
#endif

        [Header("Runtime")]
        [SerializeField] private float _deflection = 0f;
        [SerializeField] private float _trim = 0f;

        public float Deflection {
            get => _deflection;
            set => _deflection = Mathf.Clamp(value, -_maxNegDeflection, _maxPosDeflection) + _trim;
        }

        public float RawDeflection => _deflection - _trim; // without trim effect

        public float Trim {
            get => _trim;
            set => _trim = Mathf.Clamp(value, -_maxTrim, _maxTrim);
        }

        private Vector3 _rotAxis;

        private void Awake() {
            if (!_model) Debug.LogWarning("ControlSurfaceController: no model assigned");

            if (_type == ControlSurfaceType.Rudder) _rotAxis = Vector3.up * (_invert ? -1 : 1);
            else _rotAxis = Vector3.right * (_invert ? -1 : 1);
        }

        private void Update() {
            transform.localRotation = Quaternion.Euler(_rotAxis * _deflection);

            // Sync model transform
            _model.SetPositionAndRotation(transform.position, transform.rotation);
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
        private void OnDrawGizmosSelected() {
            Gizmos.color = _gizmoDeflectionColor;
            if (_type == ControlSurfaceType.Rudder) {
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (_maxPosDeflection - _deflection), transform.up) * -transform.forward * _gizmoDeflectionLength);
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (-_maxNegDeflection - _deflection), transform.up) * -transform.forward * _gizmoDeflectionLength);
            } else {
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (_maxPosDeflection - _deflection), transform.right) * -transform.forward * _gizmoDeflectionLength);
                Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis((_invert ? -1 : 1) * (-_maxNegDeflection - _deflection), transform.right) * -transform.forward * _gizmoDeflectionLength);
            }

            if (!_model || !_gizmoShowSurf) return;
            Bounds bounds = _model.GetComponent<MeshRenderer>().bounds;
            Gizmos.color = _gizmoSurfColor;
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2f);
        }
    }
#endif
}