using UnityEngine;

public class Transformer : MonoBehaviour
{
    public void UpdateTransform(Vector3 pos)
    {
        this.transform.position = pos;
    }
}
