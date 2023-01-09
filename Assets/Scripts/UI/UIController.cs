using UnityEngine;

namespace UI {
    [RequireComponent(typeof(Canvas))]
    public class UIController : MonoBehaviour {
        private Canvas _canvas = null;

        private void Awake() {
            _canvas = GetComponent<Canvas>();
        }

        private void Update() {
            ReadInputs();
        }

        private void ReadInputs() {
            if (Input.GetKeyDown(KeyCode.Alpha0)) _canvas.enabled = !_canvas.enabled;
        }
    }
}