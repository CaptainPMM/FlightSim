using UnityEngine;
using Planes.Aerodynamics;

namespace Utils {
    public class CLDrawer : MonoBehaviour {
        [SerializeField, Min(0f)] private float radius = 0.1f;

        private AerodynamicEffector[] _liftEffectors;

        private Vector3 _cl;
        private Vector3 _totalLiftVector;
        private float _totalLift;

        private void Start() {
            _liftEffectors = GetComponentsInChildren<AerodynamicEffector>();
        }

        private void OnDrawGizmosSelected() {
            if (!Application.isPlaying) return;

            _totalLiftVector = Vector3.zero;
            _totalLift = 0f;
            foreach (AerodynamicEffector l in _liftEffectors) {
                _totalLiftVector += l.transform.position * l.Lift.sqrMagnitude;
                _totalLift += l.Lift.sqrMagnitude;
            }
            _cl = _totalLiftVector / _totalLift;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_cl, radius);
        }
    }
}