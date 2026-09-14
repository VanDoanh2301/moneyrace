using UnityEngine;

/// <summary>
/// Catmull path wanderer
/// This script is used to move the circles in the main menu scene
/// the path should has at least 4 points
/// </summary>
public class CatmullWanderer : MonoBehaviour
{
    public Bounds MovementBounds;
    Vector3[] points;
    public float Speed = 0.1f;
    public void Start()
    {
        points = new Vector3[4];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = GetRandomPointInBounds(MovementBounds);
        }
        points[1] = transform.position;
        CalculateTravellingTime();

    }

    float t = 0;
    float currentCurveLength = 1;
    float travelingTime = 0;
    public void Update()
    {
        Vector3 p0 = points[0];
        Vector3 p1 = points[1];
        Vector3 p2 = points[2];
        Vector3 p3 = points[3];
        if (t + Time.deltaTime * travelingTime >= 1)
        {
            t = 0;
            AddNewPoint();
            CalculateTravellingTime();
            transform.position = GetCatmullRomPosition(t, p1, p2, p3, points[3]);

        }
        else
        {
            t += Time.deltaTime * travelingTime;
            transform.position = GetCatmullRomPosition(t, p0, p1, p2, p3);
        }

    }
    void CalculateTravellingTime()
    {
        //approximation
        currentCurveLength = Vector3.Distance(points[1], points[2]);
        travelingTime = Speed / currentCurveLength;
    }
    void AddNewPoint()
    {
        for (int i = 1; i < points.Length; i++)
            points[i - 1] = points[i];
        points[points.Length - 1] = GetRandomPointInBounds(MovementBounds);
    }
    public Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        float minX = bounds.center.x - bounds.extents.x / 2;
        float maxX = bounds.center.x + bounds.extents.x / 2;
        float minY = bounds.center.y - bounds.extents.y / 2;
        float maxY = bounds.center.y + bounds.extents.y / 2;
        return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
    }
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }

    #region Display Curve
#if UNITY_EDITOR
    [Header("Editor Only")]
    [SerializeField] bool showPath;
    [SerializeField] bool showWanderingRegion;
    void OnDrawGizmos()
    {
        if (showPath)
        {
            Gizmos.color = Color.white;
            if (points != null && points.Length > 3)
            {
                //Draw the Catmull-Rom spline between the points
                for (int i = 0; i < points.Length - 1; i++)
                {
                    //Cant draw between the endpoints
                    //Neither do we need to draw from the second to the last endpoint
                    //...if we are not making a looping line
                    //if ((i == 0 || i == points.Length - 2 || i == points.Length - 1) && !isLooping)
                    //{
                    //    continue;
                    //}

                    DisplayCatmullRomSpline(i);
                }
                Gizmos.color = Color.red;
                DisplayCatmullRomSpline(1);
            }
            Gizmos.color = Color.white;
        }
        if (showWanderingRegion)
        {
            Gizmos.color = new Color(1, 0, 0, 0.25f);
            Gizmos.DrawCube(MovementBounds.center, MovementBounds.extents);
        }
    }

    //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    void DisplayCatmullRomSpline(int pos)
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = points[ClampListPos(pos - 1)];
        Vector3 p1 = points[pos];
        Vector3 p2 = points[ClampListPos(pos + 1)];
        Vector3 p3 = points[ClampListPos(pos + 2)];

        //The start position of the line
        Vector3 lastPos = p1;

        //The spline's resolution
        //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = 0.02f;

        //How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            //Draw this line segment
            Gizmos.DrawLine(lastPos, newPos);
            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    //Clamp the list positions to allow looping
    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = points.Length - 1;
        }

        if (pos > points.Length)
        {
            pos = 1;
        }
        else if (pos > points.Length - 1)
        {
            pos = 0;
        }

        return pos;
    }
#endif
    #endregion
}
