using UnityEngine;

namespace Planes {
    public class PropController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane = null;
        [SerializeField] private Transform _model = null;
        [SerializeField] private Transform _forceOrigin = null;
        [SerializeField, Min(0)] private int _idleRPM = 500;
        [SerializeField, Min(0)] private int _maxRPM = 2000;
        [SerializeField, Min(0f)] private float _RPMChangeRate = 200f;
        [SerializeField, Min(0f), Tooltip("in inch")] private float _propDiameter = 70f; // inches
        [SerializeField, Min(0f), Tooltip("in inch")] private float _propPitch = 4f; // inches
        [SerializeField] private AnimationCurve _RPMThrustFactor = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public int IdleRPM => _idleRPM;
        public int MaxRPM => _maxRPM;

#if UNITY_EDITOR
        [Header("Setup/Gizmos")]
        [SerializeField, Min(0f)] private float _gizmoThrustLength = 0.1f;
#endif

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float _RPM = 0f;
        [SerializeField, Min(0)] private int _targetRPM = 0;

        public float RPM => _RPM;

        public int TargetRPM {
            get => _targetRPM;
            set {
                if (!EngineOn) {
                    Debug.LogWarning("PropController: cannot set target RPM while the engine is off");
                    return;
                }
                _targetRPM = Mathf.Clamp(value, _idleRPM, _maxRPM);
            }
        }

        public bool EngineOn => _RPM >= _idleRPM;

        private Rigidbody _rb = null;
        private float _force = 0f;
        private Vector3 _forceVector = Vector3.zero;
        private Vector3 _oldPos = Vector3.zero;

        private void Awake() {
            if (!_plane) Debug.LogWarning("PropController: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
            if (!_model) Debug.LogWarning("PropController: no model assigned");
            if (!_forceOrigin) Debug.LogWarning("PropController: no force origin assigned");
        }

        private void Start() {
            _oldPos = _forceOrigin.transform.position;
        }

        private void Update() {
            _RPM = Mathf.MoveTowards(_RPM, _targetRPM, _RPMChangeRate * Time.deltaTime);

            if (_RPM > 0f) {
                _model.Rotate(0f, 0f, (_RPM / 60f) * Time.deltaTime * 360f, Space.Self);
            }
        }

        private void FixedUpdate() {
            if (_RPM > 0f) {
                _force = (float)(0.00000004392399m * (decimal)_RPM * (decimal)(Mathf.Pow(_propDiameter, 3.5f) / Mathf.Sqrt(_propPitch)) * (0.000423333m * (decimal)_RPM * (decimal)_propPitch - (decimal)Vector3.Distance(_oldPos, _forceOrigin.transform.position)))
                         * 1.3f
                         * _RPMThrustFactor.Evaluate(Mathf.InverseLerp(0f, _maxRPM, _RPM));

                _forceVector = _forceOrigin.rotation * new Vector3(0f, 0f, _force);
                _rb.AddForceAtPosition(_forceVector, _forceOrigin.position, ForceMode.Force);
            }

            _oldPos = _forceOrigin.transform.position;
        }

        [ContextMenu("Start Engine")]
        public void StartEngine() {
            if (EngineOn) {
                Debug.LogWarning("PropController: engine is already running");
                return;
            }
            _targetRPM = _idleRPM;
        }

        [ContextMenu("Stop Engine")]
        public void StopEngine() {
            if (!EngineOn) {
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
            Gizmos.DrawLine(_forceOrigin.position, _forceOrigin.position + _forceVector * _gizmoThrustLength);
        }
#endif
    }
}