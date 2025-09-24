using UnityEngine;/*

public class LineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float rayDistance = 100f;

    public void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        // Cast the Ray
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, hit.origin);
            lineRenderer.SetPosition(1, hit.point);
        }

        // Hide Ray
        lineRenderer.enabled = false;
    }
}*/
