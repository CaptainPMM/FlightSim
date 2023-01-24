using UnityEngine;
using Utils;

namespace Planes.Aerodynamics {
    [RequireComponent(typeof(PropController))]
    public class PropTorque : MonoBehaviour, IDebugDraw {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane = null;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float _strength = 1f;

        [Header("Settings/Gizmos")]
        [SerializeField] private float _gizmoLength = 1f;

        private Rigidbody _rb;
        private PropController _prop;
        private Vector3 _torqueVector;

        private void Awake() {
            if (!_plane) Debug.LogWarning($"PropTorque: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();

            _prop = GetComponent<PropController>();
        }

        private void FixedUpdate() {
            _torqueVector = (_prop.ClockwiseRotation ? transform.forward : -transform.forward) * _prop.RPM * _strength;
            _rb.AddTorque(_torqueVector, ForceMode.Force);
        }

        public bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public void DebugDraw() {
            DebugDrawer.Line(name + " torque", transform.position, transform.position + _plane.transform.right * _torqueVector.magnitude * (_prop.ClockwiseRotation ? -1 : 1) * _gizmoLength, Color.green);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _plane.transform.right * _torqueVector.magnitude * (_prop.ClockwiseRotation ? -1 : 1) * _gizmoLength);
        }
#endif
    }
}