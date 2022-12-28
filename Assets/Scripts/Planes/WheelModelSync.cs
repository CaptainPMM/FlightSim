using UnityEngine;

namespace Planes {
    public class WheelModelSync : MonoBehaviour {
        [SerializeField] private WheelCollider _wheelCollider;

        private Vector3 _pos;
        private Quaternion _rot;

        private void Awake() {
            if (!_wheelCollider) Debug.LogWarning("WheelModelSync: no wheel collider assigned");
        }

        private void Update() {
            _wheelCollider.GetWorldPose(out _pos, out _rot);
            transform.SetPositionAndRotation(_pos, _rot);
        }
    }
}