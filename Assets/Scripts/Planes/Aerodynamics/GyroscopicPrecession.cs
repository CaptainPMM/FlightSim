using UnityEngine;
using Utils;

namespace Planes.Aerodynamics {
    [RequireComponent(typeof(PropController))]
    public class GyroscopicPrecession : MonoBehaviour, IDebugDraw {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane = null;
        [SerializeField] private Transform _samplePoint = null;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float _propRadius = 1f;
        [SerializeField, Min(0f)] private float _strength = 1f;

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField] private float _gizmoLength = 1f;
#endif

        private Rigidbody _rb;
        private PropController _prop;

        private Vector3 _oldFwd;
        private float _angVelocity;

        private Vector3 _forcePos;
        private Vector3 _forceVector;

        private void Awake() {
            if (!_plane) Debug.LogWarning($"GyroscopicPrecession: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();

            _prop = GetComponent<PropController>();
        }

        private void Start() {
            _oldFwd = _samplePoint.forward;
        }

        private void Update() {
            // In Update for Gizmos
            _forcePos = _prop.transform.position + (_prop.ClockwiseRotation ? _samplePoint.right : -_samplePoint.right) * _propRadius;
        }

        private void FixedUpdate() {
            _angVelocity = Vector3.SignedAngle(_oldFwd, _samplePoint.forward, _samplePoint.right);
            _oldFwd = _samplePoint.forward;

            _forceVector = _prop.transform.forward * _angVelocity * _prop.RPM * _strength;

            _rb.AddForceAtPosition(_forceVector, _forcePos, ForceMode.Force);
        }

        public bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public void DebugDraw() {
            DebugDrawer.Line(name + " precession", _forcePos, _forcePos + _forceVector * _gizmoLength, Color.green);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_forcePos, _forcePos + _forceVector * _gizmoLength);
        }
#endif
    }
}