using UnityEngine;

namespace World {
    public class TestRunway : MonoBehaviour {
        [SerializeField] private bool _enableTestRunway = false;
        public bool EnableTestRunway {
            get => _enableTestRunway;
            set {
                _enableTestRunway = value;
                foreach (Transform child in transform) child.gameObject.SetActive(_enableTestRunway);
            }
        }

        private void Start() {
            EnableTestRunway = _enableTestRunway;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha7)) EnableTestRunway = !_enableTestRunway;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            EnableTestRunway = _enableTestRunway;
        }
#endif
    }
}