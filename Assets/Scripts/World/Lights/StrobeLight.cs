using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Lights {
    [RequireComponent(typeof(Light))]
    public class StrobeLight : MonoBehaviour {
        private const float START_DELAY_MAX = 4f;

        private Light _light;

        [SerializeField] private GameObject _startDelayCtx = null;
        [SerializeField, Min(0f)] private float _interval = 1f;
        [SerializeField, Min(0f)] private float _duration = 0.15f;
        [Tooltip("Should be less than interval")]
        [SerializeField, Min(0f)] private float _offset = 0f;

        private static Dictionary<GameObject, float> _startDelays = new();
        private float _startDelay;

        private Action UpdateFunc;

        private void Awake() {
            _light = GetComponent<Light>();
            if (_offset >= _interval) Debug.LogWarning("StrobeLight: offset should be less than interval");
        }

        private void Start() {
            _light.enabled = false;

            if (_startDelayCtx) {
                if (_startDelays.TryGetValue(_startDelayCtx, out float startDelay)) _startDelay = startDelay;
                else {
                    _startDelay = UnityEngine.Random.Range(0f, START_DELAY_MAX);
                    _startDelays.Add(_startDelayCtx, _startDelay);
                }
            } else _startDelay = UnityEngine.Random.Range(0f, START_DELAY_MAX);

            UpdateFunc = WaitForStart;
        }

        private void Update() {
            UpdateFunc.Invoke();
        }

        private void WaitForStart() {
            if (Time.time > _startDelay) {
                UpdateFunc = StrobeRoutine;
            }
        }

        private void StrobeRoutine() {
            if (!_light.enabled) {
                if (Mod.Calc(Time.time - _offset, _interval) < _duration) _light.enabled = true;
            } else {
                if (Mod.Calc(Time.time - _offset, _interval) > _duration) _light.enabled = false;
            }
        }
    }
}