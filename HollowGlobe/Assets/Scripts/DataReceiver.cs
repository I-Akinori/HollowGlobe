using System.Collections.Generic;
using HollowGlobe.Controller;
using OscJack;
using UnityEngine;

namespace HollowGlobe
{
    public class DataReceiver : MonoBehaviour
    {
        OscServer server;
        [SerializeField] private GameObject parent;
        [SerializeField] private Material material;
        [SerializeField] private int port = 9000;
        [SerializeField] private int maxCount = 10;
        private List<LineData> queue = new List<LineData>();
        private List<GameObject> objects = new List<GameObject>();
        void Start()
        {
            server = new OscServer(port);
            server.MessageDispatcher.AddCallback(
                "/line",
                OnReceive
            );
        }

        void Update()
        {
            if (queue.Count > 0)
            {
                GenerateLine(queue[0]);
                queue.RemoveAt(0);
            }
            
            if (objects.Count > maxCount)
            {
                var mos = objects[0].GetComponent<MoveOnSphere>();
                mos.ReadyToDestroy();
                if (mos.readyToDestroy)
                {
                    var obj = objects[0];
                    objects.RemoveAt(0);
                    Destroy(obj);
                }
            }
        }

        void OnReceive(string address, OscDataHandle data)
        {
            string json = data.GetElementAsString(0);
            var line = JsonUtility.FromJson<LineData>(json);
            queue.Add(line);
        }

        void OnDestroy()
        {
            server?.Dispose();
        }
        
        void GenerateLineTransform(LineData data)
        {
            var obj = new GameObject();
            var lm = obj.AddComponent<LineMover>();
            lm.Initialize(data, material);
        }
        void GenerateLine(LineData data)
        {
            var obj = new GameObject();
            obj.transform.SetParent(parent.transform, false);
            obj.layer = LayerMask.NameToLayer("VirtualSpace");
            var mos = obj.AddComponent<MoveOnSphere>();
            mos.Initialize(data, material);
            objects.Add(obj);
            
            // var line = obj.AddComponent<LineRenderer>();
            // line.useWorldSpace = false;
            // line.material = new Material(material);
            // line.material.SetColor("_Color", data.color);
            // line.startWidth = 0.01f;
            // line.endWidth = 0.01f;
            // line.positionCount = data.positions.Count;
            // line.SetPositions(data.positions.ToArray());
        }
    }
}