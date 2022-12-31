using UnityEngine;

namespace Planes {
    [RequireComponent(typeof(WheelCollider))]
    public class WheelController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private Transform _wheelModel = null;
        [SerializeField] private WheelFunction _wheelFunction = WheelFunction.None;
        [SerializeField, Min(0f)] private float _maxBrake = 1f;
        [SerializeField, Range(0f, 90f)] private float _maxSteerAngle = 90f;
        [SerializeField, Min(0f)] private float _maxTorque = 50f;

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
                _brake = Mathf.Max(0f, value);
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
                _torque = Mathf.Max(0f, value);
                _wc.motorTorque = _torque;
            }
        }

        private WheelCollider _wc;
        private Vector3 _pos;
        private Quaternion _rot;

        private void Awake() {
            _wc = GetComponent<WheelCollider>();
            if (!_wheelModel) Debug.LogWarning("WheelController: no wheel model assigned");
        }

        private void Start() {
            UpdateValues();
        }

        private void Update() {
            _wc.GetWorldPose(out _pos, out _rot);
            _wheelModel.SetPositionAndRotation(_pos, _rot);
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