using UnityEngine;

namespace Planes {
    public class PropController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private Transform _model = null;

        [Header("Settings")]
        [SerializeField, Min(0)] private int _idleRPM = 500;
        [SerializeField, Min(0)] private int _maxRPM = 2000;
        [SerializeField, Min(0f)] private float _RPMChangeRate = 200f;

        public int IdleRPM => _idleRPM;
        public int MaxRPM => _maxRPM;

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

        private float _deltaRot;

        private void Awake() {
            if (!_model) Debug.LogWarning("PropController: no model assigned");
        }

        private void Update() {
            _RPM = Mathf.MoveTowards(_RPM, _targetRPM, _RPMChangeRate * Time.deltaTime);

            if (_RPM > 0f) {
                _deltaRot = (_RPM / 60f) * Time.deltaTime * 360f;
                transform.Rotate(0f, 0f, _deltaRot, Space.Self);
                _model.Rotate(0f, 0f, _deltaRot, Space.Self);
            }
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
#endif
    }
}