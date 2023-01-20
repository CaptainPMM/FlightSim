using UnityEngine;
using Utils;

namespace Planes.Aerodynamics {
    public class AerodynamicEffector : MonoBehaviour, IDebugDraw {
        public const float AIR_DENSITY_SL = 1.2250f; // kg/m^3 (standard sea level)
        public const float AIR_DENSITY_LAPSE_RATE_DEFAULT = 0.0354f; // 𝚫 air density / 1000ft

        [Header("Setup")]
        [SerializeField] private PlaneController _plane;

        [Header("Settings/Lift")]
        [SerializeField, Min(0f)] private float _airDensitySL = AIR_DENSITY_SL; // kg/m^3
        [SerializeField, Min(0f)] private float _airDensityLapseRate = AIR_DENSITY_LAPSE_RATE_DEFAULT; // 𝚫 air density / 1000ft
        [SerializeField, Min(0f)] private float _airDensity = AIR_DENSITY_SL; // [d] kg/m^3 (standard sea level)
        [SerializeField, Min(0f)] private float _wingAreaX = 1f; // m -> X * Z => [s] in m^2
        [SerializeField, Min(0f)] private float _wingAreaZ = 1f; // m -> X * Z => [s] in m^2
        [SerializeField] private float _baseLift = 1f; // newtons
        [SerializeField] private AnimationCurve _AOALiftFactor = AnimationCurve.Linear(-20f, -1f, 20f, 1f); // base lift * f(AOA) = [CL] lift coefficient

        [Header("Settings/Drag")]
        [SerializeField] private AnimationCurve _TASInducedDragFactor = AnimationCurve.Linear(0f, 1f, 200f, 0.1f);

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField, Min(0f)] private float _gizmoLocationSize = 0.2f;
        [SerializeField] private bool _gizmoShowWingArea = true;
        [SerializeField] private Vector3 _gizmoWingAreaOffset = Vector3.zero;
        [SerializeField] private bool _gizmoWingAreaSwizzle = false;
        [SerializeField] private bool _gizmoShowExtendedChordLine = false;
        [SerializeField, Min(0f)] private float _gizmoExtendedChordLineLength = 1f;
#endif
        [SerializeField, Min(0f)] private float _gizmoAOALength = 1f;
        [SerializeField, Min(0f)] private float _gizmoLiftLength = 1f;

        private Rigidbody _rb;

        private float _ALT;

        private Vector3 _oldPos;
        private Vector3 _velocity;
        private float _speed;
        private float _TAS;

        private Vector3 _wingChordLine;
        private Vector3 _relativeWind;
        private float _AOA;

        private Vector3 _airflow;

        private float _liftForce;
        private Vector3 _liftDirection;
        private Vector3 _liftVector;

        public float LiftForce => _liftForce;
        public Vector3 LiftDirection => _liftDirection;
        public Vector3 LiftVector => _liftVector;

        public Vector3 LiftWithoutDragVector => _liftForce * Vector3.Cross(_relativeWind, transform.right).normalized;
        public Vector3 InducedDragVector => _liftVector - LiftWithoutDragVector;

        private void Awake() {
            if (!_plane) Debug.LogWarning("AerodynamicEffector: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
        }

        private void Start() {
            _oldPos = transform.position;
        }

        private void FixedUpdate() {
            UpdateAirDensity();
            UpdateVelocity();
            UpdateAOA();
            UpdateLift();
            ApplyForces();
        }

        private void UpdateAirDensity() {
            _ALT = transform.position.y * 3.28084f; // m to feet conversion

            if (_airDensityLapseRate == 0f) return;
            _airDensity = _airDensitySL - _airDensityLapseRate * _ALT / 1000f;
        }

        private void UpdateVelocity() {
            _velocity = (transform.position - _oldPos) / Time.fixedDeltaTime;
            _oldPos = transform.position;

            _speed = _velocity.magnitude;
            _TAS = _speed * 1.94384f; // m/s to knots conversion
        }

        private void UpdateAOA() {
            _wingChordLine = transform.forward;
            _relativeWind = Vector3.ProjectOnPlane(_velocity, transform.right).normalized;
            _AOA = Vector3.SignedAngle(_wingChordLine, _relativeWind, transform.right);
        }

        private void UpdateLift() {
            // Induced drag -> tilt lift vector backwards based on TAS e.g. |_ => /
            _airflow = Vector3.Lerp(_relativeWind, _wingChordLine, _TASInducedDragFactor.Evaluate(_TAS));

            _liftForce = 0.5f * _airDensity * Mathf.Pow(_speed, 2f) * (_wingAreaX * _wingAreaZ) * _baseLift * _AOALiftFactor.Evaluate(_AOA); // L = (1/2) * d * v^2 * s * CL
            _liftDirection = Vector3.Cross(_airflow, transform.right).normalized; // perpendicular to airflow
            _liftVector = _liftForce * _liftDirection;
        }

        private void ApplyForces() {
            _rb.AddForceAtPosition(_liftVector, transform.position, ForceMode.Force);
        }

        public Vector3 GetLiftWithoutDrag() => _liftForce * Vector3.Cross(_relativeWind, transform.right).normalized;

        public bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public void DebugDraw() {
            DebugDrawer.Line(name + " wingChordLine", transform.position, transform.position + _wingChordLine * _gizmoAOALength, Color.white);
            DebugDrawer.Line(name + " relativeWind", transform.position, transform.position + _relativeWind * _gizmoAOALength, Color.blue);
            DebugDrawer.Line(name + " airflow", transform.position, transform.position + _airflow * _gizmoAOALength, Color.red);

            Vector3 liftWithoutDragPos = transform.position + LiftWithoutDragVector * _gizmoLiftLength;
            DebugDrawer.Line(name + " liftWithoutDrag", transform.position, liftWithoutDragPos, Color.white);
            DebugDrawer.Line(name + " inducedDrag", liftWithoutDragPos, liftWithoutDragPos + InducedDragVector * _gizmoLiftLength, Color.red);
            DebugDrawer.Line(name + " lift", transform.position, transform.position + _liftVector * _gizmoLiftLength, Color.cyan);
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
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _airflow * _gizmoAOALength);

            Vector3 liftWithoutDragPos = transform.position + LiftWithoutDragVector * _gizmoLiftLength;
            Gizmos.DrawLine(liftWithoutDragPos, liftWithoutDragPos + InducedDragVector * _gizmoLiftLength);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, liftWithoutDragPos);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _liftVector * _gizmoLiftLength);
        }
#endif
    }
}