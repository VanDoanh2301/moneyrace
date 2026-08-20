using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int index;
    public bool hit;
    public NeonColorPair colorPair;

    void OnBecameInvisible()
    {
        if (GameManager_jump.Instance.cam != null)
            if (transform.position.x < GameManager_jump.Instance.cam.transform.position.x || transform.position.y < -10)
                Destroy(gameObject);
    }
}
