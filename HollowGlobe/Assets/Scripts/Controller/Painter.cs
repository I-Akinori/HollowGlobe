using System.Collections.Generic;
using System.Linq;
using HollowGlobe;
using UnityEngine;
using OscJack;
using TMPro;

public class Painter : MonoBehaviour
{
    private bool isDrawing = false;
    private List<LineRenderer> lines;
    private Dictionary<LineRenderer, float> deletingLines;
    private LineRenderer currentLine;
    private List<Vector3> points;
    private int maxCount = 48;
    private Color lineColor;
    private float deleteTime = 1f;
    [SerializeField] private Material material;
    [SerializeField] private float sampleRatio = 0.1f;
    [SerializeField] private float time = 0;
    [SerializeField] private string ip = "127.0.0.1";
    [SerializeField] private int port = 9000;
    [SerializeField] private TextMeshProUGUI text;
    
    void Start()
    {
        lines = new List<LineRenderer>();
        deletingLines = new Dictionary<LineRenderer, float>();
        points = new List<Vector3>();
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdateLine();
        UpdateDeletingLine();
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            currentLine = GenerateLine();
            time = 0;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            SendData();
            
            points.Clear();
            deletingLines[currentLine] = deleteTime;
            currentLine = null;
            time = 0;
        }
        if (!isDrawing)
        {
            return;
        }

        time += Time.deltaTime;
        if (time < sampleRatio || points.Count >= maxCount)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out var hit,10.0f))
        {
            if (points.Count > 0 && (points[^1] - hit.point).magnitude < 0.02f)
            {
                return;
            }
            var p = new Vector3(hit.point.x, hit.point.y, 0);
            points.Add(p);
        }
        time = 0;
    }

    LineRenderer GenerateLine()
    {
        var obj = new GameObject();
        obj.transform.position = Vector3.back;
        obj.transform.parent = transform;
        var line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.material = new Material(material);
        lineColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), 1.0f, 1.0f);
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.material.SetColor("_Color", lineColor);
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        lines.Add(line);
        return line;
    }
    void DeleteLine(LineRenderer line)
    {
        lines.Remove(line);
        Destroy(line.gameObject);
        Destroy(line);
    }
    void UpdateLine()
    {
        if (currentLine == null)
        {
            return;
        }

        currentLine.positionCount = points.Count;
        currentLine.SetPositions(points.ToArray());
    }
    void UpdateDeletingLine()
    {
        var list = new List<LineRenderer>();
        var keys = deletingLines.Keys.ToList();
        foreach (var key in keys)
        {
            deletingLines[key] -= Time.deltaTime;
            if (deletingLines[key] <= 0)
            {
                list.Add(key);
                continue;
            }

            var col = key.startColor;
            col.a = deletingLines[key] / deleteTime;
            key.material.SetColor("_Color", col);
        }

        foreach (var line in list)
        {
            deletingLines.Remove(line);
            DeleteLine(line);
        }
    }

    void SendData()
    {
        var ip = "127.0.0.1";
        ip = text.text.Replace("\u200b", "");
        using (var client = new OscClient(ip, port))
        {
            var state = new LineData
            {
                positions = points,
                color = lineColor
            };

            string json = JsonUtility.ToJson(state);
            client.Send("/line", json);
        }
    }
}
