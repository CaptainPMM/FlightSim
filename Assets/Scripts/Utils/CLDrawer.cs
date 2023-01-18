using UnityEngine;
using Planes.Aerodynamics;

namespace Utils {
    public class CLDrawer : MonoBehaviour {
        [SerializeField, Min(0f)] private float radius = 0.1f;

        private AerodynamicEffector[] _liftEffectors;

        private float _totalLiftForce;
        private Vector3 _totalWeightedLiftPosition;
        private Vector3 _cl;

        private void Start() {
            _liftEffectors = GetComponentsInChildren<AerodynamicEffector>();
        }

        private void OnDrawGizmosSelected() {
            if (!Application.isPlaying) return;

            _totalLiftForce = 0f;
            _totalWeightedLiftPosition = Vector3.zero;
            foreach (AerodynamicEffector l in _liftEffectors) {
                _totalLiftForce += l.LiftVector.sqrMagnitude;
                _totalWeightedLiftPosition += l.transform.position * l.LiftVector.sqrMagnitude;
            }
            _cl = _totalWeightedLiftPosition / _totalLiftForce;

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_cl, radius);
        }
    }
}