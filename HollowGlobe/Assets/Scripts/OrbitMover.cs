using UnityEngine;

namespace HollowGlobe
{
    public class OrbitMover : MonoBehaviour
    {
        private Vector3 basePosition = new Vector3(1f, 0f, 0f);
        private Vector3 center = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.basePosition = new Vector3(0f, 0f, 1f);
        }

        // Update is called once per frame
        void Update()
        {
            float pitch = 20f * Mathf.Deg2Rad * Time.deltaTime; // ピッチ回転量
            float yaw   = 30f * Mathf.Deg2Rad * Time.deltaTime; // ヨー回転量

            rotation = rotation * Quaternion.AngleAxis(Mathf.Rad2Deg * pitch, Vector3.right);
            rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * yaw, Vector3.up) * rotation;

            var position = center + rotation * basePosition;
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
