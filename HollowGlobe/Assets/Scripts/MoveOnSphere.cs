using System.Collections.Generic;

namespace HollowGlobe.Controller
{
    using UnityEngine;

    public class MoveOnSphere : MonoBehaviour
    {
        private LineRenderer line;
        private List<Vector3> points;
        private List<Vector3> velocities;
        private List<float> lengths;
        private float lifeTime;
        private float maxLifeTime = 100;
        public float speed = 1.0f;
        public Vector3 forward;
        private float birthTime = 0f;
        private Color color;
        private float hue;
        private float destroyTime = 5f;
        public bool readyToDestroy = false;

        void Awake()
        {
            this.transform.position = Vector3.back;
            transform.position = transform.position.normalized;
            var angle = Random.Range(0, Mathf.PI * 2);
            forward = Vector3.up;
            birthTime = Time.time;
        }

        void Update()
        {
            var dt = Time.deltaTime;
            lifeTime -= dt;
            if (lifeTime <= 0)
            {
                lifeTime = 0;
                return;
            }
            var lifeRatio = lifeTime / maxLifeTime;
            
            // 絵全体の更新
            var p = transform.position;
            var up = p.normalized;
            var right = Vector3.Cross(forward, up).normalized;
            var forwardTangent = Vector3.Cross(up, right).normalized;
            var turn = 0.01f * (Mathf.PerlinNoise1D(Time.time + birthTime * 100) - 0.5f);
            var go = 1f;
            var dir = turn * right + go * forwardTangent;
            dir = dir - up * Vector3.Dot(dir, up);
            var mag = dir.magnitude;
            if (mag < 1e-6f) return;
            dir /= mag;

            var vt = dir * (speed * lifeRatio);              // 接平面速度
            p = (p + vt * dt).normalized;          // 移動→球面へ戻す
            forward = (vt - up * Vector3.Dot(vt, up)).normalized; // 念のため接平面に
            if (forward.sqrMagnitude > 1e-8f)
            {
                transform.rotation = Quaternion.LookRotation(forward, up) * Quaternion.AngleAxis(90, Vector3.right);
            }
            transform.position = p * (lifeRatio * 0.5f + 0.5f);
            transform.localScale = Vector3.one;// * lifeRatio;
            
            // LineRendererの更新
            color = Color.HSVToRGB(hue, Mathf.Lerp(0.1f, 1f, lifeRatio), 
                Mathf.Lerp((Mathf.Sin(birthTime) + 1) * 0.5f, 1f, lifeRatio));
            color.a = Mathf.Clamp01((maxLifeTime - lifeTime) / 4f);
            line.material.SetColor("_Color", color);
            TransformLine();
            SetPositions();
        }
        
        public void Initialize(LineData data, Material material)
        {
            line = this.gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.material = new Material(material);
            color = data.color;
            Color.RGBToHSV(color, out hue, out _, out _);
            color.a = 0.0f;
            line.material.SetColor("_Color", color);
            line.startWidth = 0.01f;
            line.endWidth = 0.01f;
            line.positionCount = data.positions.Count;
            points = data.positions;
            velocities = new List<Vector3>();
            lengths = new List<float>();
            lifeTime = maxLifeTime;
        
            for (var i = 0; i < points.Count; i++)
            {
                //points[i] *= 1.0f;
                //points[i] += new Vector3(0, 2, -5);
            }
            for (var i = 0; i < points.Count - 1; i++)
            {
                lengths.Add((points[i] - points[i + 1]).magnitude);
            }
            points.ForEach(p => velocities.Add(Vector3.zero));
            SetPositions();
        }

        private void TransformLine()
        {
            var lifeRatio = Mathf.Lerp(3f, 1f,lifeTime / maxLifeTime);
            // 中点になろうとする効果
            for (var i = 1; i < points.Count - 1; i++)
            {
                var prevPos = points[i - 1];
                var currentPos = points[i];
                var nextPos = points[i + 1];
                velocities[i] += ((prevPos + nextPos) * 0.5f - currentPos) * 0.01f;
            }
            // 長さ保持効果
            for (var i = 0; i < points.Count; i++)
            {
                if (i > 0)
                {
                    var d01 = points[i - 1] - points[i];
                    velocities[i] += (d01.magnitude - lengths[i - 1] * lifeRatio) * d01.normalized * 0.01f;
                }
                if (i < velocities.Count - 1)
                {
                    var d01 = points[i] - points[i + 1];
                    velocities[i] += (d01.magnitude - lengths[i] * lifeRatio) * d01.normalized * 0.01f;
                }
            }
                
            for (var i = 0; i < velocities.Count; i++)
            {
                velocities[i] = Mathf.Min(velocities[i].magnitude, 0.02f) * velocities[i].normalized;
                points[i] += velocities[i] * Time.deltaTime;
            }
        }
        // 球の中心からの距離を1にしてセット
        // LineRendererの座標系では原点は(0, 0, lifeRatio)
        private void SetPositions()
        {
            var lifeRatio = lifeTime / maxLifeTime * 0.5f + 0.5f;
            var modified = new List<Vector3>();
            foreach (var p in points)
            {
                var c = Vector3.forward * lifeRatio;
                var d = (p - c);
                modified.Add(d.normalized * lifeRatio + c);
            }
            line.SetPositions(modified.ToArray());
        }

        public void ReadyToDestroy()
        {
            destroyTime -= Time.deltaTime;
            if (destroyTime <= 0)
            {
                readyToDestroy = true;
            }
            color.a = destroyTime * 0.2f;
            line.material.SetColor("_Color", color);
        }
        void OnDestroy()
        {
            if (line.material != null)
            {
                Destroy(line.material);
                line.material = null;
            }
        }
    }
}