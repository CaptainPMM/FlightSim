using UnityEngine;

namespace Planes.Aerodynamics {
    public class AerodynamicEffector : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private PlaneController _plane;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float _airDensity = 1.225f; // [d] kg/m^3 (standard sea level)
        [SerializeField, Min(0f)] private float _wingAreaX = 1f; // m -> X * Z => [s] in m^2
        [SerializeField, Min(0f)] private float _wingAreaZ = 1f; // m -> X * Z => [s] in m^2
        [SerializeField] private float _baseLift = 1f; // newtons
        [SerializeField] private AnimationCurve _AOALiftFactor = AnimationCurve.Linear(-20f, 0f, 20f, 0f); // base lift * f(AOA) = [CL] lift coefficient

        // [SerializeField, Min(0f)] private float _baseInducedDrag = 0.1f;
        // [SerializeField] private AnimationCurve _IASInducedDragFactor = AnimationCurve.Linear(0f, 1f, 200f, 0.1f);

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField, Min(0f)] private float _gizmoLocationSize = 0.2f;
        [SerializeField] private bool _gizmoShowWingArea = true;
        [SerializeField] private Vector3 _gizmoWingAreaOffset = Vector3.zero;
        [SerializeField] private bool _gizmoWingAreaSwizzle = false;
        [SerializeField] private bool _gizmoShowExtendedChordLine = false;
        [SerializeField, Min(0f)] private float _gizmoExtendedChordLineLength = 1f;
        [SerializeField, Min(0f)] private float _gizmoAOALength = 1f;
        [SerializeField, Min(0f)] private float _gizmoLiftLength = 1f;
#endif

        private Rigidbody _rb;

        private Vector3 _oldPos;
        private Vector3 _velocity;
        private float _speed;
        // private float _IAS;

        private Vector3 _wingChordLine;
        private Vector3 _relativeWind;
        private float _AOA;

        private Vector3 _lift;
        public Vector3 Lift => _lift;

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
            // _IAS = _speed * 1.94384f; // m/s to knots conversion
        }

        private void UpdateAOA() {
            _wingChordLine = transform.forward;
            _relativeWind = _velocity.normalized;
            _AOA = Vector3.SignedAngle(_wingChordLine, _relativeWind, transform.right);
        }

        private void UpdateLift() {
            _lift = transform.up *                                                                                                      // lift direction vector *
                    0.5f * _airDensity * Mathf.Pow(_speed, 2f) * (_wingAreaX * _wingAreaZ) * _baseLift * _AOALiftFactor.Evaluate(_AOA); // lift force (L = (1/2) * d * v^2 * s * CL)
        }

        private void ApplyForces() {
            _rb.AddForceAtPosition(_lift, transform.position, ForceMode.Force);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, _gizmoLocationSize);
            if (_gizmoShowWingArea) {
                if (_gizmoWingAreaSwizzle)
                    Gizmos.DrawWireCube(transform.position + _gizmoWingAreaOffset, new Vector3(0f, _wingAreaX, _wingAreaZ));
                else
                    Gizmos.DrawWireCube(transform.position + _gizmoWingAreaOffset, new Vector3(_wingAreaX, 0f, _wingAreaZ));
            }

            if (_gizmoShowExtendedChordLine) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + transform.forward * _gizmoExtendedChordLineLength, transform.position - transform.forward * _gizmoExtendedChordLineLength);
            }

            if (!Application.isPlaying) return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + _wingChordLine * _gizmoAOALength);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _relativeWind * _gizmoAOALength);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _lift * _gizmoLiftLength);

            // Gizmos.color = Color.yellow;  TODO Draw induced drag...
        }
#endif
    }
}