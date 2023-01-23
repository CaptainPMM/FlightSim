using UnityEngine;
using Utils;

namespace Planes.Aerodynamics {
    public class PropEffector : AerodynamicEffector {
        public static bool EnableGizmos = false;

        [Header("Settings/Prop/Gizmos")]
        [SerializeField] private bool _gizmosForceEnable = false;

        private PropController _prop;
        private float _radius;

        private float _rotationalSpeed;
        private Vector3 _rotationalDirection;
        private Vector3 _rotationalVector;

        private float _forwardSpeed;
        private Vector3 _forwardDirection;
        private Vector3 _forwardVector;

        protected override void Awake() {
            base.Awake();
            _prop = transform.parent.GetComponent<PropController>();
            if (!_prop) Debug.LogWarning("PropEffector: effector should be a child of a PropController component");
        }

        protected override void Start() {
            base.Start();
            _radius = Vector3.Distance(transform.position, _prop.transform.position);
        }

        protected override void UpdateVelocity() {
            _rotationalSpeed = (_prop.RPM / 60f) * 2f * Mathf.PI * _radius;
            _rotationalDirection = Vector3.ProjectOnPlane(transform.forward, _prop.transform.forward).normalized;
            _rotationalVector = _rotationalSpeed * _rotationalDirection;

            _forwardVector = Vector3.Project((transform.position - _oldPos) / Time.fixedDeltaTime, _plane.Velocity);
            _forwardDirection = _forwardVector.normalized;
            _forwardSpeed = _forwardVector.magnitude;

            _velocity = _rotationalVector + _forwardVector;
            _oldPos = transform.position;

            _speed = _rotationalSpeed + _forwardSpeed;
            _TAS = _speed * 1.94384f; // m/s to knots conversion
        }

        public override void DebugDraw() {
            if (EnableGizmos || _gizmosForceEnable) {
                base.DebugDraw();
                DebugDrawer.Line(name + " rotVector", transform.position, transform.position + _rotationalVector * _gizmoLiftLength, Color.magenta);
                DebugDrawer.Line(name + " fwdVector", transform.position, transform.position + _forwardVector * _gizmoLiftLength, Color.blue);
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected() {
            if (EnableGizmos || _gizmosForceEnable) {
                base.OnDrawGizmosSelected();
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + _rotationalVector * _gizmoLiftLength);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + _forwardVector * _gizmoLiftLength);
            }
        }
#endif
    }
}