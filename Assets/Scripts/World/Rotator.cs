using UnityEngine;

namespace World {
    public class Rotator : MonoBehaviour {
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _rotation;

        private void Update() {
            _transform.Rotate(_rotation * Time.deltaTime, Space.Self);
        }
    }
}