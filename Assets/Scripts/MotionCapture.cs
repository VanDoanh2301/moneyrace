using UnityEngine;

public enum Motions { None, Tap, Up, Down, Left, Right };
public class MotionCapture : MonoBehaviour
{
    [SerializeField]
    private float minSwipeLength = 200f;

    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;

    private Motions currentMotion = Motions.None;

    public Motions CurrentMotion { get { Motions tmp = currentMotion; currentMotion = Motions.None; return tmp; } }

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        catchMotions();
    }

    private void catchMotions()
    {
        // Editor / Standalone: mouse. Device build: touch.
        // (Trước đây #if UNITY_ANDROID khiến Editor với target Android không nhận chuột.)
#if UNITY_EDITOR || UNITY_STANDALONE
        CatchMouse();
#else
        CatchTouch();
#endif
    }

    private void CatchTouch()
    {
        if (Input.touches.Length == 0)
            return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            firstPressPos = new Vector2(t.position.x, t.position.y);
        }

        if (t.phase == TouchPhase.Ended)
        {
            ApplySwipe(new Vector2(t.position.x, t.position.y));
        }
    }

    private void CatchMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ApplySwipe(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        }
    }

    private void ApplySwipe(Vector2 endPos)
    {
        secondPressPos = endPos;
        currentSwipe = secondPressPos - firstPressPos;

        if (currentSwipe.magnitude < minSwipeLength)
        {
            currentMotion = Motions.Tap;
            return;
        }

        currentSwipe.Normalize();

        if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            currentMotion = Motions.Up;
        else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            currentMotion = Motions.Down;
        else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            currentMotion = Motions.Left;
        else if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            currentMotion = Motions.Right;
    }

    public void resetMotion()
    {
        currentMotion = Motions.None;
    }
}
