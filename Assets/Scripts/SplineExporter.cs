using UnityEngine;
using UnityEngine.Splines; // Make sure you have the Splines package
using System.IO;
using System.Text;

public class SplineExporter : MonoBehaviour
{
    [Tooltip("Drag your Spline component here")]
    public SplineContainer splineContainer;
    
    [Tooltip("How many points to sample? 600 was your last file's count.")]
    public int numberOfPoints = 600;

    // This adds a right-click menu to the script in the Inspector
    [ContextMenu("Export to CSV")]
    void Export()
    {
        if (splineContainer == null)
        {
            Debug.LogError("Spline Container is not set!");
            return;
        }

        // We only care about the first spline in the container
        if (splineContainer.Splines.Count == 0)
        {
            Debug.LogError("No splines found in the container!");
            return;
        }
        Spline spline = splineContainer.Splines[0];

        // We will build the CSV file as one big string
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < numberOfPoints; i++)
        {
            // 't' is a value from 0 (start) to 1 (end)
            float t = (float)i / (float)numberOfPoints;

            // Get the LOCAL position on the spline
            // We use local (EvaluatePosition) because it's already
            // relative to the PortoTrack origin.
            Vector3 localPoint = spline.EvaluatePosition(t);

            // Write the "x,z" coordinates (since y is "up")
            sb.AppendLine($"{localPoint.x},{localPoint.z}");
        }

        // Define the path
        // Application.dataPath points to your "Assets" folder
        string path = Path.Combine(Application.dataPath, "Resources", "track_centerline.csv");

        // Make sure the "Resources" folder exists
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        // Write the file
        File.WriteAllText(path, sb.ToString());

        Debug.Log($"SUCCESS: Exported {numberOfPoints} points to {path}");
    }
}