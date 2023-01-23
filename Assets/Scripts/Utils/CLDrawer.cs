using System.Linq;
using UnityEngine;
using Planes.Aerodynamics;

namespace Utils {
    public class CLDrawer : MonoBehaviour, IDebugDraw {
#if UNITY_EDITOR
        [SerializeField, Min(0f)] private float radius = 0.1f;
#endif

        private AerodynamicEffector[] _liftEffectors;

        private float _totalLiftForce;
        private Vector3 _totalWeightedLiftPosition;
        private Vector3 _cl;

        private void Start() {
            _liftEffectors = GetComponentsInChildren<AerodynamicEffector>().Where(e => e.IncludeInCL).ToArray();
        }

        private bool UpdateCL() {
            _totalLiftForce = 0f;
            _totalWeightedLiftPosition = Vector3.zero;
            foreach (AerodynamicEffector l in _liftEffectors) {
                _totalLiftForce += l.LiftVector.sqrMagnitude;
                _totalWeightedLiftPosition += l.transform.position * l.LiftVector.sqrMagnitude;
            }
            if (_totalLiftForce > 0f) {
                _cl = _totalWeightedLiftPosition / _totalLiftForce;
                return true;
            } else return false;
        }

        public bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public void DebugDraw() {
            if (UpdateCL()) DebugDrawer.Pos("CL", _cl, Color.blue, 1.75f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (!Application.isPlaying) return;

            if (UpdateCL()) {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_cl, radius);
            }
        }
#endif
    }
}