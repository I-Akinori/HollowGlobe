namespace Controller
{
    using System.Collections.Generic;
using System.Linq;
using HollowGlobe;
using UnityEngine;
using OscJack;
using TMPro;

public class UIHider : MonoBehaviour
{
    private bool isDrawing = false;
    [SerializeField] private float intervaltime = 0.1f;
    [SerializeField] private int maxCount = 10;
    private float time = 0;
    private int count = 0;
    
    [SerializeField] private TMP_InputField text;
    
    void Start()
    {
        time = 0f;
        count = 0;
    }
    
    // Update is called once per frame
    void Update()
    {
        // 連続タップでUI非表示
        if (!text.gameObject.activeSelf)
        {
            return;
        }
        time += Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            count++;
            time = 0;
        }

        if (time >= intervaltime)
        {
            count = 0;
        }

        if (count >= maxCount)
        {
            text.gameObject.SetActive(false);
        }
    }
}

}