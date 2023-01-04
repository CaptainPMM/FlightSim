using UnityEngine;

namespace Planes {
    public class PropController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane = null;
        [SerializeField] private Transform _model = null;
        [SerializeField] private Transform _forceOrigin = null;
        [SerializeField, Min(0f)] private int _idleRPM = 500;
        [SerializeField, Min(0f)] private int _maxRPM = 2000;
        [SerializeField, Min(0f)] private float _rpmChangeRate = 200f;
        [SerializeField] private AnimationCurve _RPMThrustCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float _RPM = 0f;
        [SerializeField, Min(0f)] private int _targetRPM = 0;

        public float RPM => _RPM;

        public int TargetRPM {
            get => _targetRPM;
            set {
                if (_RPM < _idleRPM) {
                    Debug.LogWarning("PropController: cannot set target RPM while the engine is off");
                    return;
                }
                _targetRPM = Mathf.Clamp(value, _idleRPM, _maxRPM);
            }
        }

        private Rigidbody _rb = null;
        private Vector3 _forceVector = Vector3.zero;

        private void Awake() {
            if (!_plane) Debug.LogWarning("PropController: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
            if (!_model) Debug.LogWarning("PropController: no model assigned");
            if (!_forceOrigin) Debug.LogWarning("PropController: no force origin assigned");
        }

        private void Update() {
            _RPM = Mathf.MoveTowards(_RPM, _targetRPM, _rpmChangeRate * Time.deltaTime);

            if (_RPM > 0f) {
                _model.Rotate(0f, 0f, (_RPM / 60f) * Time.deltaTime * 360f, Space.Self);
            }
        }

        private void FixedUpdate() {
            if (_RPM > 0f) {
                _forceVector = _plane.transform.rotation * new Vector3(0f, 0f, _RPM * _RPMThrustCurve.Evaluate(Mathf.InverseLerp(0f, _maxRPM, _RPM)));
                _rb.AddForceAtPosition(_forceVector, _forceOrigin.position, ForceMode.Force);
            }
        }

        [ContextMenu("Start Engine")]
        public void StartEngine() {
            if (_RPM >= _idleRPM) {
                Debug.LogWarning("PropController: engine is already running");
                return;
            }
            _targetRPM = _idleRPM;
        }

        [ContextMenu("Stop Engine")]
        public void StopEngine() {
            if (_RPM < _idleRPM) {
                Debug.LogWarning("PropController: engine is already off");
                return;
            }
            _targetRPM = 0;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            _RPM = Mathf.Clamp(_RPM, 0f, _maxRPM);
            _targetRPM = Mathf.Clamp(_targetRPM, 0, _maxRPM);
        }

        private void OnDrawGizmosSelected() {
            if (!Application.isPlaying) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_forceOrigin.position, _forceOrigin.position + _forceVector);
        }
#endif
    }
}