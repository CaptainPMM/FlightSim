using System.Collections.Generic;
using UnityEngine;
using World.Lights;
using Utils;

namespace Planes {
    [RequireComponent(typeof(Rigidbody), typeof(CGDrawer))]
    public class PlaneController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private List<WheelController> _wheels = new();
        [SerializeField] private List<LightsController> _lights = new();

        private Dictionary<WheelFunction, List<WheelController>> _wheelsByFunction;
        private Dictionary<World.Lights.LightType, LightsController> _lightsByType;

        private void Awake() {
            // Init dictionaries
            _wheelsByFunction = new();
            foreach (WheelController wc in _wheels) {
                foreach (WheelFunction wf in System.Enum.GetValues(typeof(WheelFunction))) {
                    if ((wc.WheelFunction & wf) > 0) {
                        if (!_wheelsByFunction.ContainsKey(wf)) _wheelsByFunction.Add(wf, new());
                        _wheelsByFunction[wf].Add(wc);
                    }
                }
            }

            _lightsByType = new();
            foreach (LightsController lc in _lights) {
                if (_lightsByType.TryAdd(lc.LightType, lc) == false) Debug.LogWarning("PlanceController: duplicate light type found: " + lc.LightType);
            }
        }

        private void Update() {
            ReadInputs();
        }

        private void ReadInputs() {
            ReadWheelInputs();
            ReadLightInputs();
        }

        private void ReadWheelInputs() {
            _wheelsByFunction[WheelFunction.Brake].ForEach(wc => wc.Brake = Input.GetAxis("Brake") * wc.MaxBrake);
            _wheelsByFunction[WheelFunction.Steer].ForEach(wc => wc.SteerAngle = Input.GetAxis("Yaw") * wc.MaxSteerAngle);
            _wheelsByFunction[WheelFunction.Torque].ForEach(wc => wc.Torque += Input.GetAxis("WheelTorque") * wc.MaxTorque);
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