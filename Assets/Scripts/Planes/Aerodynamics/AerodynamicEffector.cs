using UnityEngine;
using Utils;

namespace Planes.Aerodynamics {
    public class AerodynamicEffector : MonoBehaviour, IDebugDraw {
        public const float AIR_DENSITY_SL = 1.2250f; // kg/m^3 (standard sea level)
        public const float AIR_DENSITY_LAPSE_RATE_DEFAULT = 0.0354f; // 𝚫 air density / 1000ft

        [Header("Setup")]
        [SerializeField] protected PlaneController _plane = null;
        [SerializeField] protected bool _includeInCL = true;

        public bool IncludeInCL => _includeInCL;

        [Header("Settings/Lift")]
        [SerializeField, Min(0f)] protected float _airDensitySL = AIR_DENSITY_SL; // kg/m^3
        [SerializeField, Min(0f)] protected float _airDensityLapseRate = AIR_DENSITY_LAPSE_RATE_DEFAULT; // 𝚫 air density / 1000ft
        [SerializeField, Min(0f)] protected float _airDensity = AIR_DENSITY_SL; // [d] kg/m^3 (standard sea level)
        [SerializeField, Min(0f)] protected float _wingAreaX = 1f; // m -> X * Z => [s] in m^2
        [SerializeField, Min(0f)] protected float _wingAreaZ = 1f; // m -> X * Z => [s] in m^2
        [SerializeField] protected float _baseLift = 1f; // newtons
        [SerializeField] protected AnimationCurve _AOALiftFactor = AnimationCurve.Linear(-20f, -1f, 20f, 1f); // base lift * f(AOA) = [CL] lift coefficient

        [Header("Settings/Drag")]
        [SerializeField] protected AnimationCurve _TASInducedDragFactor = AnimationCurve.Linear(0f, 1f, 200f, 0.1f);

        [Header("Settings/Ground Effect")]
        [SerializeField] protected bool _useGroundEffect = true;
        [SerializeField] protected LayerMask _groundLayers;
        [SerializeField] protected AnimationCurve _groundEffectAGLLiftFactor = AnimationCurve.Linear(0f, 1.5f, 32f, 1f);
        [SerializeField] protected AnimationCurve _groundEffectAGLDragFactor = AnimationCurve.Linear(0f, 0.15f, 32f, 1f);

#if UNITY_EDITOR
        [Header("Settings/Gizmos")]
        [SerializeField, Min(0f)] protected float _gizmoLocationSize = 0.2f;
        [SerializeField] protected bool _gizmoShowWingArea = true;
        [SerializeField] protected Vector3 _gizmoWingAreaOffset = Vector3.zero;
        [SerializeField] protected WingAreaSwizzle _gizmoWingAreaSwizzle = WingAreaSwizzle.XZ;
        [SerializeField] protected bool _gizmoShowExtendedChordLine = false;
        [SerializeField, Min(0f)] protected float _gizmoExtendedChordLineLength = 1f;
#endif
        [SerializeField, Min(0f)] protected float _gizmoAOALength = 1f;
        [SerializeField, Min(0f)] protected float _gizmoLiftLength = 1f;

        protected Rigidbody _rb;

        protected float _MSL;
        protected float _AGL;

        protected Vector3 _oldPos;
        protected Vector3 _velocity;
        protected float _speed;
        protected float _TAS;

        protected Vector3 _wingChordLine;
        protected Vector3 _relativeWind;
        protected float _AOA;

        protected Vector3 _airflow;

        protected float _liftForce;
        protected Vector3 _liftDirection;
        protected Vector3 _liftVector;

        public float LiftForce => _liftForce;
        public Vector3 LiftDirection => _liftDirection;
        public Vector3 LiftVector => _liftVector;

        protected Vector3 _liftWithoutDragVector;

        public Vector3 LiftWithoutDragVector => _liftWithoutDragVector;
        public Vector3 InducedDragVector => _liftVector - _liftWithoutDragVector;

        protected virtual void Awake() {
            if (!_plane) Debug.LogWarning($"{this.GetType().Name}: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
        }

        protected virtual void Start() {
            _oldPos = transform.position;
        }

        protected virtual void FixedUpdate() {
            UpdateAltitudes();
            UpdateAirDensity();
            UpdateVelocity();
            UpdateAOA();
            UpdateLift();
            ApplyForces();
        }

        protected virtual void UpdateAltitudes() {
            _MSL = transform.position.y * 3.28084f; // m to feet conversion

            if (Physics.Raycast(transform.position, -_liftDirection, out RaycastHit hit, Mathf.Infinity, _groundLayers)) {
                _AGL = Vector3.Distance(hit.point, transform.position) * 3.28084f; // m to feet conversion
            } else _AGL = _MSL;
        }

        protected virtual void UpdateAirDensity() {
            if (_airDensityLapseRate == 0f) return;
            _airDensity = _airDensitySL - _airDensityLapseRate * _MSL / 1000f;
        }

        protected virtual void UpdateVelocity() {
            _velocity = (transform.position - _oldPos) / Time.fixedDeltaTime;
            _oldPos = transform.position;

            _speed = _velocity.magnitude;
            _TAS = _speed * 1.94384f; // m/s to knots conversion
        }

        protected virtual void UpdateAOA() {
            _wingChordLine = transform.forward;
            _relativeWind = Vector3.ProjectOnPlane(_velocity, transform.right).normalized;
            _AOA = Vector3.SignedAngle(_wingChordLine, _relativeWind, transform.right);
        }

        protected virtual void UpdateLift() {
            // Induced drag -> tilt lift vector backwards based on TAS e.g. |_ => / [AND ground effect?]
            _airflow = Vector3.Lerp(_relativeWind, _wingChordLine, _TASInducedDragFactor.Evaluate(_TAS) * (_useGroundEffect ? _groundEffectAGLDragFactor.Evaluate(_AGL) : 1f));

            _liftForce = 0.5f * _airDensity * Mathf.Pow(_speed, 2f) * (_wingAreaX * _wingAreaZ) * _baseLift * _AOALiftFactor.Evaluate(_AOA); // L = (1/2) * d * v^2 * s * CL
            if (_useGroundEffect) _liftForce *= _groundEffectAGLLiftFactor.Evaluate(_AGL); // [ground effect?]
            _liftDirection = Vector3.Cross(_airflow, transform.right).normalized; // perpendicular to airflow
            _liftVector = _liftForce * _liftDirection;

            _liftWithoutDragVector = _liftForce * Vector3.Cross(_relativeWind, transform.right).normalized;
        }

        protected virtual void ApplyForces() {
            _rb.AddForceAtPosition(_liftVector, transform.position, ForceMode.Force);
        }

        public virtual bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public virtual void DebugDraw() {
            DebugDrawer.Line(name + " wingChordLine", transform.position, transform.position + _wingChordLine * _gizmoAOALength, Color.white);
            DebugDrawer.Line(name + " relativeWind", transform.position, transform.position + _relativeWind * _gizmoAOALength, Color.blue);
            DebugDrawer.Line(name + " airflow", transform.position, transform.position + _airflow * _gizmoAOALength, Color.red);

            Vector3 liftWithoutDragPos = transform.position + _liftWithoutDragVector * _gizmoLiftLength;
            DebugDrawer.Line(name + " liftWithoutDrag", transform.position, liftWithoutDragPos, Color.white);
            DebugDrawer.Line(name + " inducedDrag", liftWithoutDragPos, liftWithoutDragPos + InducedDragVector * _gizmoLiftLength, Color.red);
            DebugDrawer.Line(name + " lift", transform.position, transform.position + _liftVector * _gizmoLiftLength, Color.cyan);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, _gizmoLocationSize);
            if (_gizmoShowWingArea) {
                switch (_gizmoWingAreaSwizzle) {
                    case WingAreaSwizzle.XZ:
                        Gizmos.DrawWireCube(transform.position + _gizmoWingAreaOffset, new Vector3(_wingAreaX, 0f, _wingAreaZ));
                        break;
                    case WingAreaSwizzle.YZ:
                        Gizmos.DrawWireCube(transform.position + _gizmoWingAreaOffset, new Vector3(0f, _wingAreaX, _wingAreaZ));
                        break;
                    case WingAreaSwizzle.XY:
                        Gizmos.DrawWireCube(transform.position + _gizmoWingAreaOffset, new Vector3(_wingAreaX, _wingAreaZ, 0f));
                        break;
                }
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

            Vector3 liftWithoutDragPos = transform.position + _liftWithoutDragVector * _gizmoLiftLength;
            Gizmos.DrawLine(liftWithoutDragPos, liftWithoutDragPos + InducedDragVector * _gizmoLiftLength);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, liftWithoutDragPos);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _liftVector * _gizmoLiftLength);
        }

        protected enum WingAreaSwizzle : byte {
            XZ,
            YZ,
            XY
        }
#endif
    }
}