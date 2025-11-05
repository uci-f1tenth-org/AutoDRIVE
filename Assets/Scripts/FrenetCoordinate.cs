using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FrenetCoordinate : MonoBehaviour
{
    public float Frenet_S = 0f;
    public float Frenet_D = 0f;
    public float normalizedFrenet_S = 0f;
    public float normalizedFrenet_D = 0f;
    private float totalPathLength = 0f;
    public float halfTrackWidth = 0.625f; // Half the width of the track for normalization
    private List<Vector2> waypoints = new List<Vector2>();


    void Start()
    {
        TextAsset lineCoordinateFile = Resources.Load<TextAsset>("track_centerline");

        string[] lines = lineCoordinateFile.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 2)
            {
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                waypoints.Add(new Vector2(x, y));
            }
        }

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            totalPathLength += Vector2.Distance(waypoints[i], waypoints[i + 1]);
        }
    }

    void FixedUpdate()
    {
        Vector2 carPosition = new Vector2(transform.position.x, transform.position.z);
        float minDistance = float.MaxValue;
        int closestSegmentIndex = 0;
        float accumulated_s = 0f;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector2 p1 = waypoints[i];
            Vector2 p2 = waypoints[i + 1];
            float segmentLength = Vector2.Distance(p1, p2);

            // Find the projection of the car onto the line segment
            Vector2 segmentVector = p2 - p1;
            Vector2 carToP1 = carPosition - p1;
            float t = Vector2.Dot(carToP1, segmentVector) / segmentVector.sqrMagnitude;

            Vector2 closestPointOnSegment;
            if (t < 0.0f)
            {
                closestPointOnSegment = p1; // Clamped to start
            }
            else if (t > 1.0f)
            {
                closestPointOnSegment = p2; // Clamped to end
            }
            else
            {
                closestPointOnSegment = p1 + t * segmentVector; // Projection is on the segment
            }

            float distance = Vector2.Distance(carPosition, closestPointOnSegment);

            // If this is the new closest segment
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSegmentIndex = i;

                // --- Calculate 'd' (Lateral Distance) ---
                Frenet_D = minDistance;

                // Determine the sign of 'd' (left or right)
                // Use a 2D cross-product equivalent
                float cross_z = (p2.x - p1.x) * (carPosition.y - p1.y) - (p2.y - p1.y) * (carPosition.x - p1.x);
                if (cross_z < 0)
                {
                    Frenet_D *= -1; // Car is to the right
                }

                // --- Calculate 's' (Longitudinal Distance) ---
                float s_on_segment = Vector2.Distance(p1, closestPointOnSegment);
                Frenet_S = accumulated_s + s_on_segment;
            }

            // Add this segment's length to the total 's' accumulator
            accumulated_s += segmentLength;
        }
        if (totalPathLength > 0)
        {
            normalizedFrenet_S = Frenet_S / totalPathLength;
        }
        if (halfTrackWidth > 0)
        {
            normalizedFrenet_D = Frenet_D / halfTrackWidth;
            normalizedFrenet_D = Mathf.Clamp(normalizedFrenet_D, -1f, 1f);
        }
    }
}
