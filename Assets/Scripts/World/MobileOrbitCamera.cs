using UnityEngine;
using UnityEngine.EventSystems;

namespace Riverforge
{
    public sealed class MobileOrbitCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(0f, 1.5f, 0f);
        public float distance = 37f;
        public float yaw = -35f;
        public float pitch = 33f;
        private float _lastPinch;

        public void Initialize()
        {
            Apply();
        }

        private void Update()
        {
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved && !OverUI(t.fingerId))
                {
                    yaw += t.deltaPosition.x * .12f;
                    pitch = Mathf.Clamp(pitch - t.deltaPosition.y * .10f, 18f, 65f);
                }
            }
            else if (Input.touchCount >= 2)
            {
                var a = Input.GetTouch(0); var b = Input.GetTouch(1);
                float d = Vector2.Distance(a.position, b.position);
                if (_lastPinch > 0f) distance = Mathf.Clamp(distance - (d - _lastPinch) * .025f, 18f, 60f);
                _lastPinch = d;
            }
            else
            {
                _lastPinch = 0f;
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    yaw += Input.GetAxis("Mouse X") * 4f;
                    pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 3f, 18f, 65f);
                }
                distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 2.2f, 18f, 60f);
#endif
            }
            Apply();
        }

        private bool OverUI(int fingerId) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
        private void Apply()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target - rot * Vector3.forward * distance;
            transform.rotation = rot;
        }
    }
}
