using UnityEngine;

public class MathStuff : MonoBehaviour
{
    public static Vector3[] GetRadialSpread(int count, Vector3 axis)
    {
        if (count <= 0) return new Vector3[0];

        var dirs = new Vector3[count];
        float step = 360f / count;

        Vector3 startDir = Vector3.forward;
        Vector3.OrthoNormalize(ref axis, ref startDir);

        for (int i = 0; i < count; i++)
        {
            dirs[i] = Quaternion.AngleAxis(step * i, axis) * startDir;
        }

        return dirs;
    }

    public static Vector3[] GetRadialSpread(int count, Vector3 startDir, Vector3 axis)
    {
        if (count <= 0) return new Vector3[0];

        var dirs = new Vector3[count];
        float step = 360f / count;

        for (int i = 0; i < count; i++)
            dirs[i] = Quaternion.AngleAxis(step * i, axis) * startDir;

        return dirs;
    }
}
