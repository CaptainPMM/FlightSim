using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.Lights {
    public class LightsController : MonoBehaviour {
        private const float TIMER_UPDATE_INTERVAL = 2f;

        [Header("Setup")]
        [SerializeField] private LightType _lightType = LightType.Undefined;
        public LightType LightType => _lightType;

        [SerializeField] private List<Light> _lights = new();

        [Header("Setup/Timer")]
        [SerializeField] private bool _enableTimer = false;
        [SerializeField, Range(12f, 23.99f)] private float _enableTime = 17.00f;
        [SerializeField, Range(0f, 11.99f)] private float _disableTime = 07.00f;

        [Header("Runtime")]
        [SerializeField] private bool _lightsEnabled = false;
        public bool LightsEnabled {
            get => _lightsEnabled;
            set {
                _lightsEnabled = value;
                UpdateLights();
            }
        }

        private void Start() {
            UpdateLights();
            if (_enableTimer) StartCoroutine(TimerRoutine());
        }

        public void Enable() {
            LightsEnabled = true;
        }

        public void Disable() {
            LightsEnabled = false;
        }

        public void Toggle() {
            LightsEnabled = !LightsEnabled;
        }

        private void UpdateLights() {
            _lights.ForEach(l => l.gameObject.SetActive(_lightsEnabled));
        }

        private IEnumerator TimerRoutine() {
            if (!DayNightCycle.Main) {
                Debug.LogWarning("LightsController: timer is enabled but there is no DayNightCycle main time source instance");
                yield break;
            }

            var waiter = new WaitForSeconds(TIMER_UPDATE_INTERVAL);
            while (true) {
                if (!_lightsEnabled) {
                    if (DayNightCycle.Main.CurrTime > _enableTime || DayNightCycle.Main.CurrTime < _disableTime) LightsEnabled = true;
                } else if (DayNightCycle.Main.CurrTime > _disableTime && DayNightCycle.Main.CurrTime < _enableTime) LightsEnabled = false;
                yield return waiter;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            UpdateLights();
        }
#endif
    }
}