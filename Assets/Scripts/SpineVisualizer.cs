using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SplineVisualizer : MonoBehaviour
{
    private List<Vector3> waypoints = new List<Vector3>();

    void OnDrawGizmos()
    {
        // Load waypoints if we haven't
        if (waypoints.Count == 0)
        {
            TextAsset lineCoordinateFile = Resources.Load<TextAsset>("track_centerline");
            if (lineCoordinateFile == null)
            {
                Debug.LogError("Cannot find 'track_centerline.csv' in Resources folder!");
                return;
            }

            string[] lines = lineCoordinateFile.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                if (values.Length >= 2)
                {
                    float.TryParse(values[0], out float x);
                    float.TryParse(values[1], out float y);
                    // Using Y for vertical, so CSV y-coord is our Z-coord
                    waypoints.Add(new Vector3(x, 0, y)); 
                }
            }
        }

        // Draw the spline
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            // Convert local spline points to world points
            Vector3 worldP1 = transform.TransformPoint(waypoints[i]);
            Vector3 worldP2 = transform.TransformPoint(waypoints[i + 1]);
            
            // Draw the line in world space
            Gizmos.DrawLine(worldP1, worldP2);
        }
    }
}