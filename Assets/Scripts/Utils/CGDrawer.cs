using UnityEngine;

namespace Utils {
    [RequireComponent(typeof(Rigidbody))]
    public class CGDrawer : MonoBehaviour {
        [SerializeField, Min(0f)] private float radius = 0.1f;

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, radius);
        }
    }
}