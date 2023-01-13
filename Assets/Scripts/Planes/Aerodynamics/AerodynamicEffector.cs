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
        [SerializeField] private bool _gizmoShowExtendedChordLine = false;
        [SerializeField, Min(0f)] private float _gizmoExtendedChordLineLength = 1f;
        [SerializeField, Min(0f)] private float _gizmoAOALength = 1f;
#endif

        private Rigidbody _rb;

        private Vector3 wingChordLine;
        private Vector3 relativeWind;

        private void Awake() {
            if (!_plane) Debug.LogWarning("AerodynamicEffector: no plane assigned");
            else _rb = _plane.GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            wingChordLine = transform.forward;
            relativeWind = Quaternion.Euler(0f, transform.eulerAngles.y, -transform.eulerAngles.z) * Vector3.forward; // TODO
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (_gizmoShowExtendedChordLine) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + transform.forward * _gizmoExtendedChordLineLength, transform.position - transform.forward * _gizmoExtendedChordLineLength);
            }

            if (!Application.isPlaying) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + wingChordLine * _gizmoAOALength);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + relativeWind * _gizmoAOALength);
        }
#endif
    }
}