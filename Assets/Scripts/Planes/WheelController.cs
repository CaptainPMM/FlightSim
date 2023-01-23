using UnityEngine;

namespace Planes {
    [RequireComponent(typeof(WheelCollider))]
    public class WheelController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private Transform _wheelModel = null;
        [SerializeField] private WheelFunction _wheelFunction = WheelFunction.None;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float _maxBrake = 1f;
        [SerializeField, Range(0f, 90f)] private float _maxSteerAngle = 90f;
        [SerializeField, Min(0f)] private float _maxTorque = 50f;
        [SerializeField] private PropController _attachedProp = null;
        [SerializeField, Min(0)] private int _startRollPropRPM = 525;

        public WheelFunction WheelFunction => _wheelFunction;
        public float MaxBrake => _maxBrake;
        public float MaxSteerAngle => _maxSteerAngle;
        public float MaxTorque => _maxTorque;

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float _brake = 0f;
        [SerializeField, Range(-90f, 90f)] private float _steerAngle = 0f;
        [SerializeField, Min(0f)] private float _torque = 0f;

        public float Brake {
            get => _brake;
            set {
                _brake = Mathf.Clamp(value, 0f, _maxBrake);
                _wc.brakeTorque = _brake;
            }
        }

        public float SteerAngle {
            get => _steerAngle;
            set {
                _steerAngle = Mathf.Clamp(value, -_maxSteerAngle, _maxSteerAngle);
                _wc.steerAngle = _steerAngle;
            }
        }

        public float Torque {
            get => _torque;
            set {
                _torque = Mathf.Clamp(value, 0f, _maxTorque);
                _wc.motorTorque = _torque;
            }
        }

        private WheelCollider _wc;
        private Vector3 _pos;
        private Quaternion _rot;

        private void Awake() {
            _wc = GetComponent<WheelCollider>();
            if (!_wheelModel) Debug.LogWarning("WheelController: no wheel model assigned");
            if (!_attachedProp) Debug.LogWarning("WheelController: no attached prop assigned");
        }

        private void Start() {
            UpdateValues();
        }

        private void Update() {
            // Sync model transform
            _wc.GetWorldPose(out _pos, out _rot);
            _wheelModel.SetPositionAndRotation(_pos, _rot);

            // Parking brake effect (fixes start rolling stuck bug)
            if (_wc.isGrounded && _wc.rpm < 1f && _torque <= 0.1f) {
                if (_attachedProp.RPM >= _startRollPropRPM) Torque = 0.01f;
                else Torque = 0f;
            }
        }

        private void UpdateValues() {
            Brake = _brake;
            SteerAngle = _steerAngle;
            Torque = _torque;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (Application.isPlaying && _wc) UpdateValues();
        }
    }
#endif
}