using System.Collections.Generic;
using HollowGlobe;
using UnityEngine;

public class LineMover : MonoBehaviour
{
    private LineRenderer line;
    private List<Vector3> points;
    private List<Vector3> velocities;
    private List<float> lengths;
    private float lifeTime;
    private float maxLifeTime = 100;

    public void Initialize(LineData data, Material material)
    {
        line = this.gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.material = new Material(material);
        line.material.SetColor("_Color", data.color);
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.positionCount = data.positions.Count;
        line.SetPositions(data.positions.ToArray());
        points = data.positions;
        velocities = new List<Vector3>();
        lengths = new List<float>();
        lifeTime = maxLifeTime;
        
        for (var i = 0; i < points.Count; i++)
        {
            points[i] *= 1.0f;
            points[i] += new Vector3(0, 2, -5);
        }
        for (var i = 0; i < points.Count - 1; i++)
        {
            lengths.Add((points[i] - points[i + 1]).magnitude);
        }
        points.ForEach(p => velocities.Add(Vector3.up));
    } 

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            lifeTime = 0;
            return;
        }
        line.SetPositions(points.ToArray());
        var lifeRatio = lifeTime / maxLifeTime;
        var radius = 2.0f * lifeRatio;
        
        // whole
        var center = Vector3.zero;
        var wholeVel = Vector3.zero;
        for (var i = 0; i < points.Count; i++)
        {
            center += points[i];
            wholeVel += velocities[i];
        }
        center /= points.Count;
        wholeVel /= points.Count;
        {
            var localPos = center - Vector3.zero;
            var radForce = localPos * (radius - localPos.magnitude);
            var nor = localPos.normalized;
            var dir = wholeVel.normalized;
            var tan = Vector3.Cross(dir, nor);
            var angle = Mathf.PerlinNoise1D(Time.time * 1.0f) - 0.5f;
            angle *= Mathf.PI;
            var swayForce = dir * Mathf.Cos(angle) + tan * Mathf.Sin(angle);
            var waveForce = nor * Mathf.PerlinNoise1D(100 - Time.time * 1.0f);
            wholeVel += (radForce * 5.0f + swayForce * 5.0f 
                                         + waveForce * 0.0f) * Time.deltaTime;
        }
        // individual
        for (var i = 0; i < velocities.Count; i++)
        {
            var localPos = points[i] - Vector3.zero;
            var radForce = localPos * (radius - localPos.magnitude);
            var nor = localPos.normalized;
            var dir = velocities[i].normalized;
            var tan = Vector3.Cross(dir, nor);
            var angle = Mathf.PerlinNoise1D(Time.time * 1.0f) - 0.5f;
            angle *= Mathf.PI;
            var swayForce = dir * Mathf.Cos(angle) + tan * Mathf.Sin(angle);
            var waveForce = nor * Mathf.PerlinNoise1D(100 - Time.time * 1.0f);
            velocities[i] += (radForce * 5.0f + swayForce * 5.0f 
                                                   + waveForce * 0.0f) * Time.deltaTime;
            velocities[i] += wholeVel * 0.001f;
            if (i > 0)
            {
                var d01 = points[i - 1] - points[i];
                //velocities[i] += 80f * (d01.magnitude - lengths[i - 1] * lifeRatio) * d01.normalized * Time.deltaTime;
            }
            if (i < velocities.Count - 1)
            {
                var d01 = points[i] - points[i + 1];
                //velocities[i] += 80f * (d01.magnitude - lengths[i] * lifeRatio) * d01.normalized * Time.deltaTime;
            }
            velocities[i] /= velocities[i].magnitude;
            velocities[i] *= lifeRatio;
        }
        for (var i = 0; i < velocities.Count; i++)
        {
            points[i] += velocities[i] * Time.deltaTime;
        }
    }
    
}
