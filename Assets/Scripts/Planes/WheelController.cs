using UnityEngine;

namespace Planes {
    [RequireComponent(typeof(WheelCollider))]
    public class WheelController : MonoBehaviour {
        [SerializeField] private float _torque = 0f;
        [SerializeField, Min(0f)] private float _brake = 0f;
        [SerializeField, Range(-90f, 90f)] private float _steerAngle = 0f;

        public float Torque {
            get => _torque;
            set {
                _torque = value;
                _wc.motorTorque = value;
            }
        }

        public float Brake {
            get => _brake;
            set {
                _brake = value;
                _wc.brakeTorque = value;
            }
        }

        public float SteerAngle {
            get => _steerAngle;
            set {
                _steerAngle = value;
                _wc.steerAngle = value;
            }
        }

        private WheelCollider _wc;

        private void Awake() {
            _wc = GetComponent<WheelCollider>();
        }

        private void Start() {
            UpdateValues();
        }

        private void UpdateValues() {
            _wc.motorTorque = _torque;
            _wc.brakeTorque = _brake;
            _wc.steerAngle = _steerAngle;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (Application.isPlaying && _wc) UpdateValues();
        }
    }
#endif
}