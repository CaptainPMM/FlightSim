using UnityEngine;

namespace Planes.Aerodynamics {
    public class AerodynamicEffector : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane;

        [Header("Settings")]
        [SerializeField] private float _baseLift = 1f;
        [SerializeField] private AnimationCurve _AOALiftFactor = AnimationCurve.Linear(-20f, 0f, 20f, 0f);
        [SerializeField, Min(0f)] private float _baseInducedDrag = 0.1f;
        [SerializeField] private AnimationCurve _IASInducedDragFactor = AnimationCurve.Linear(0f, 1f, 200f, 0.1f);

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField, Min(0f)] private float _gizmoLocationSize = 0.2f;
        [SerializeField] private bool _gizmoShowExtendedChordLine = false;
        [SerializeField, Min(0f)] private float _gizmoExtendedChordLineLength = 1f;
        [SerializeField, Min(0f)] private float _gizmoAOALength = 1f;
        [SerializeField, Min(0f)] private float _gizmoLiftLength = 1f;
#endif

        private Rigidbody _rb;

        private Vector3 _oldPos;
        private Vector3 _velocity;
        private float _speed;
        private float _IAS;

        private Vector3 _wingChordLine;
        private Vector3 _relativeWind;
        private float _AOA;

        private Vector3 _lift;

        private void Awake() {
            if (!_plane) Debug.LogWarning("AerodynamicEffector: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
        }

        private void Start() {
            _oldPos = transform.position;
        }

        private void FixedUpdate() {
            UpdateVelocity();
            UpdateAOA();
            UpdateLift();
            ApplyForces();
        }

        private void UpdateVelocity() {
            _velocity = transform.position - _oldPos;
            _oldPos = transform.position;

            _speed = _velocity.magnitude;
            _IAS = _speed * 1.94384f; // m/s to knots conversion
        }

        private void UpdateAOA() {
            _wingChordLine = transform.forward;
            _relativeWind = _velocity.normalized;
            _AOA = Vector3.SignedAngle(_wingChordLine, _relativeWind, transform.right);
        }

        private void UpdateLift() {
            _lift = transform.up * _speed * _baseLift * _AOALiftFactor.Evaluate(_AOA);
        }

        private void ApplyForces() {
            _rb.AddForceAtPosition(_lift, transform.position, ForceMode.Force);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, _gizmoLocationSize);

            if (_gizmoShowExtendedChordLine) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + transform.forward * _gizmoExtendedChordLineLength, transform.position - transform.forward * _gizmoExtendedChordLineLength);
            }

            if (!Application.isPlaying) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _relativeWind * _gizmoAOALength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _wingChordLine * _gizmoAOALength);

            Gizmos.DrawLine(transform.position, transform.position + _lift * _gizmoLiftLength);
        }
#endif
    }
}