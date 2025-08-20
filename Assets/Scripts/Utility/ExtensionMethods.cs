using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static List<T> GetAllWithinRange<T>(Vector3 worldPos, float radius) where T : Component
    {
        var results = new List<T>();
        var cols = Physics.OverlapSphere(worldPos, radius);
        foreach (var c in cols)
        {
            var comp = c.GetComponentInParent<T>();
            if (comp != null && !results.Contains(comp))
                results.Add(comp);
        }
        return results;
    }

    public static T GetClosest<T>(Vector3 position, float maxDistance) where T : MonoBehaviour
    {
        T closest = null;
        float closestDistance = maxDistance;
        var objs = GameObject.FindObjectsOfType<T>();
        foreach (var obj in objs)
        {
            float d = Vector3.Distance(obj.transform.position, position);
            if (d < closestDistance)
            {
                closest = obj;
                closestDistance = d;
            }
        }
        return closest;
    }

    public static Vector3 GetMouseWorldPosition()
    {
        var cam = Camera.main;
        if (cam == null) return Vector3.zero;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return hit.point;

        Plane ground = new Plane(Vector3.up, Vector3.zero);
        return ground.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }
}
