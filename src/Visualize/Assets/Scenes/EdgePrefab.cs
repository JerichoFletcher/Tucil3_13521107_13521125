using UnityEngine;

public class EdgePrefab : MonoBehaviour {
    public LineRenderer line;

    public Rect Bound { get; set; }
    public Rect Range { get; set; }

    public void SetPosition(float x1, float y1, float x2, float y2) {
        float
            bx1 = (x1 - Range.x) / Range.width,
            by1 = (y1 - Range.y) / Range.height,
            bx2 = (x2 - Range.x) / Range.width,
            by2 = (y2 - Range.y) / Range.height,
            wx1 = Bound.x + bx1 * Bound.width,
            wy1 = Bound.y + by1 * Bound.height,
            wx2 = Bound.x + bx2 * Bound.width,
            wy2 = Bound.y + by2 * Bound.height;
        line.SetPosition(0, new Vector3(wx1, wy1, 0));
        line.SetPosition(1, new Vector3(wx2, wy2, 0));
    }

    public void SetColor(Color color) {
        line.startColor = color;
        line.endColor = color;
    }
}
