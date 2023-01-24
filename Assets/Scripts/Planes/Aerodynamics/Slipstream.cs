using UnityEngine;
using Utils;

namespace Planes.Aerodynamics {
    [RequireComponent(typeof(ControlSurfaceController))]
    public class Slipstream : MonoBehaviour, IDebugDraw {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane = null;
        [SerializeField] private PropController _prop = null;

        [Header("Settings")]
        [SerializeField] private Vector3 _forceOffset = Vector3.zero;
        [SerializeField, Min(0f)] private float _strength = 1f;

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField] private float _gizmoLength = 1f;
        [SerializeField] private float _gizmoLocationSize = 0.2f;
#endif

        private Rigidbody _rb;
        private Vector3 _forcePos;
        private Vector3 _forceVector;

        private void Awake() {
            if (!_plane) Debug.LogWarning($"Slipstream: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
            if (!_prop) Debug.LogWarning($"Slipstream: no prop assigned");
        }

        private void Update() {
            // In Update for Gizmos
            _forcePos = transform.position + transform.TransformVector(_forceOffset);
            _forceVector = (_prop.ClockwiseRotation ? _plane.transform.right : -_plane.transform.right) * _prop.RPM * _strength;
        }

        private void FixedUpdate() {
            _rb.AddForceAtPosition(_forceVector, _forcePos, ForceMode.Force);
        }

        public bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public void DebugDraw() {
            DebugDrawer.Line(name + " slipstream", _forcePos, _forcePos + _forceVector * _gizmoLength, Color.green);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_forcePos, _forcePos + _forceVector * _gizmoLength);
            Gizmos.DrawSphere(transform.position + transform.TransformVector(_forceOffset), _gizmoLocationSize); // works in editor time as well
        }
#endif
    }
}