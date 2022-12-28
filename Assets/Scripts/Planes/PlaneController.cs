using System.Collections.Generic;
using UnityEngine;
using World.Lights;

namespace Planes {
    public class PlaneController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private List<LightsController> _lights = new();

        private Dictionary<World.Lights.LightType, LightsController> _lightsByType;

        private void Awake() {
            // Init _lightsByType dictionary
            _lightsByType = new();
            foreach (LightsController lc in _lights) {
                if (_lightsByType.TryAdd(lc.LightType, lc) == false) Debug.LogWarning("PlanceController: duplicate light type found: " + lc.LightType);
            }
        }

        private void Update() {
            ReadInputs();
        }

        private void ReadInputs() {
            ReadLightInputs();
        }

        private void ReadLightInputs() {
            if (Input.GetKeyDown(KeyCode.N)) _lightsByType[World.Lights.LightType.Nav].Toggle();
            if (Input.GetKeyDown(KeyCode.B)) _lightsByType[World.Lights.LightType.Beacon].Toggle();
            if (Input.GetKeyDown(KeyCode.V)) _lightsByType[World.Lights.LightType.Strobe].Toggle();
            if (Input.GetKeyDown(KeyCode.X)) _lightsByType[World.Lights.LightType.Taxi].Toggle();
            if (Input.GetKeyDown(KeyCode.C)) _lightsByType[World.Lights.LightType.Landing].Toggle();
        }
    }
}