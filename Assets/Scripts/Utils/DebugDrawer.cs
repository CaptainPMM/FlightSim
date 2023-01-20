using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public class DebugDrawer : MonoBehaviour {
        public static DebugDrawer Inst { get; private set; }

        public const float DEFAULT_LINE_WIDTH = 0.02f;
        public const float DEFAULT_POS_SIZE = 0.5f;

        [Header("Setup")]
        [SerializeField] private Material _drawingMaterial;

        [field: Header("Runtime")]
        [field: SerializeField]
        public bool Enable { get; set; } = true;

        private IDebugDraw[] _drawings;
        private Dictionary<string, DrawingInfo> _activeDrawings = new();
        private List<string> _inactiveDrawings = new();

        private void Awake() {
            Inst = this;
            _drawings = FindObjectsOfType<MonoBehaviour>(true).OfType<IDebugDraw>().ToArray();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Alpha5)) Enable = !Enable;

            if (!Enable) return;
            foreach (IDebugDraw d in _drawings) if (d.DebugDrawActive) d.DebugDraw();
        }

        private void LateUpdate() {
            if (_activeDrawings.Count == 0) return;

            _inactiveDrawings.Clear();
            foreach (var kv in _activeDrawings) {
                if (!kv.Value.active) {
                    Destroy(_activeDrawings[kv.Key].drawing.gameObject);
                    _inactiveDrawings.Add(kv.Key);
                } else _activeDrawings[kv.Key].active = false;
            }
            _inactiveDrawings.ForEach(i => _activeDrawings.Remove(i));
        }

        public static void Line(string uniqueName, Vector3 origin, Vector3 target, Color color, float width = DEFAULT_LINE_WIDTH) {
            if (!Inst.Enable) return;

            LineRenderer line;
            if (Inst._activeDrawings.TryGetValue(uniqueName, out DrawingInfo drawingInfo)) {
                line = drawingInfo.drawing as LineRenderer;
                Inst._activeDrawings[uniqueName].active = true;
            } else {
                line = new GameObject("[DBG] <Line> #" + uniqueName, typeof(LineRenderer)).GetComponent<LineRenderer>();
                line.transform.parent = Inst.transform;

                line.positionCount = 2;
                line.material = Inst._drawingMaterial;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                Inst._activeDrawings.Add(uniqueName, new DrawingInfo(line));
            }

            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.SetPosition(0, origin);
            line.SetPosition(1, target);
        }

        public static void Pos(string uniqueName, Vector3 position, Color color, float size = DEFAULT_POS_SIZE, float width = DEFAULT_LINE_WIDTH) {
            if (!Inst.Enable) return;

            LineRenderer line;
            if (Inst._activeDrawings.TryGetValue(uniqueName, out DrawingInfo drawingInfo)) {
                line = drawingInfo.drawing as LineRenderer;
                Inst._activeDrawings[uniqueName].active = true;
            } else {
                line = new GameObject("[DBG] <Pos> #" + uniqueName, typeof(LineRenderer)).GetComponent<LineRenderer>();
                line.transform.parent = Inst.transform;

                line.positionCount = 8;
                line.material = Inst._drawingMaterial;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                Inst._activeDrawings.Add(uniqueName, new DrawingInfo(line));
            }

            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.SetPositions(new Vector3[] {
                new Vector3(position.x + size * 0.5f, position.y, position.z),
                new Vector3(position.x - size * 0.5f, position.y, position.z),
                new Vector3(position.x, position.y, position.z),
                new Vector3(position.x, position.y + size * 0.5f, position.z),
                new Vector3(position.x, position.y - size * 0.5f, position.z),
                new Vector3(position.x, position.y, position.z),
                new Vector3(position.x, position.y, position.z + size * 0.5f),
                new Vector3(position.x, position.y, position.z - size * 0.5f)
            });
        }

        private class DrawingInfo {
            public bool active;
            public Renderer drawing;

            public DrawingInfo(Renderer drawing) {
                active = true;
                this.drawing = drawing;
            }
        }
    }

    public interface IDebugDraw {
        public bool DebugDrawActive { get; }
        public void DebugDraw();
    }
}