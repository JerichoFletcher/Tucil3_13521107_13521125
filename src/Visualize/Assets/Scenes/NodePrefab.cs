using UnityEngine;
using TMPro;

public class NodePrefab : MonoBehaviour {
    public TMP_Text labelNama;

    public Rect Bound { get; set; }
    public Rect Range { get; set; }

    public void Set(string name, float x, float y) {
        labelNama.text = name;
        float
            bx = (x - Range.x) / Range.width,
            by = (y - Range.y) / Range.height,
            wx = Bound.x + bx * Bound.width,
            wy = Bound.y + by * Bound.height;
        transform.position = new Vector3(wx, wy, transform.position.z);
    }
}
