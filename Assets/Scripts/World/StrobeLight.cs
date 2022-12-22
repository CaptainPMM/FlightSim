using System.Collections;
using UnityEngine;

namespace World {
    [RequireComponent(typeof(Light))]
    public class StrobeLight : MonoBehaviour {
        private Light _light;

        [SerializeField, Min(0f)] private float _offset = 0f;
        [SerializeField, Min(0f)] private float _interval = 0.85f;
        [SerializeField, Min(0f)] private float _duration = 0.15f;

        private void Awake() {
            _light = GetComponent<Light>();
        }

        private void Start() {
            StartCoroutine(StrobeRoutine());
        }

        private IEnumerator StrobeRoutine() {
            yield return new WaitForSeconds(_offset);

            while (true) {
                yield return new WaitForSeconds(_interval);
                _light.enabled = true;
                yield return new WaitForSeconds(_duration);
                _light.enabled = false;
            }
        }
    }
}