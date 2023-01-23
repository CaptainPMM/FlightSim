using UnityEngine;

namespace Planes.Aerodynamics {
    public class PropTorque : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane = null;
        [SerializeField] private PropController _prop = null;
        [SerializeField] private bool _propClockwiseRot = true;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float _strength = 1f;

        private Rigidbody _rb;

        private void Awake() {
            if (!_plane) Debug.LogWarning($"PropTorque: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
            if (!_prop) Debug.LogWarning($"PropTorque: no prop assigned");
        }

        private void FixedUpdate() {
            _rb.AddTorque((_propClockwiseRot ? _prop.transform.forward : -_prop.transform.forward) * _prop.RPM * _strength, ForceMode.Force);
        }
    }
}