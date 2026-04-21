using UnityEngine;

namespace HollowGlobe
{
    public class OrbitMover : MonoBehaviour
    {
        private Vector3 basePosition = new Vector3(1f, 0f, 0f);
        private Vector3 center = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        private float lifeTime;
        private float maxLifeTime = 300;
        private Vector2 direction = Vector2.zero;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.basePosition = new Vector3(0f, 0f, 1f);
            this.lifeTime = this.maxLifeTime;
            this.direction = new Vector2(10f, 10f);
        }

        // Update is called once per frame
        void Update()
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                lifeTime = 0;
                Destroy(this.gameObject);
                return;
            }
            var lifeRatio = lifeTime / this.maxLifeTime;
            
            var pitch = this.direction.y * Mathf.Deg2Rad * Time.deltaTime; // ピッチ回転量
            var yaw   = this.direction.x * Mathf.Deg2Rad * Time.deltaTime; // ヨー回転量
            pitch *= lifeRatio;
            yaw *= lifeRatio;

            rotation = rotation * Quaternion.AngleAxis(Mathf.Rad2Deg * pitch, Vector3.right);
            rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * yaw, Vector3.up) * rotation;

            var position = center + rotation * basePosition * lifeRatio;
            Debug.DrawLine(this.transform.position, position, Color.white, 100);
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one * lifeRatio;
        }
    }
}
