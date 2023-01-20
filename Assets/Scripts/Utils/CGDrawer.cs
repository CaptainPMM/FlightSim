using UnityEngine;

namespace Utils {
    [RequireComponent(typeof(Rigidbody))]
    public class CGDrawer : MonoBehaviour, IDebugDraw {
#if UNITY_EDITOR
        [SerializeField, Min(0f)] private float radius = 0.1f;
#endif

        private Rigidbody _rb;

        private void Start() {
            _rb = GetComponent<Rigidbody>();
        }

        public bool DebugDrawActive => enabled && gameObject.activeInHierarchy;
        public void DebugDraw() {
            DebugDrawer.Pos("CG", _rb.worldCenterOfMass, Color.magenta, 1.75f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, radius);
        }
#endif
    }
}