using UnityEngine;

public class DecorativeParticle : MonoBehaviour
{
    public Vector2 velocity;

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    void OnBecameInvisible()
    {
        if (GameManager_jump.Instance.cam != null)
            if (transform.position.x < GameManager_jump.Instance.cam.transform.position.x - 5)
                Destroy(gameObject);
    }
}
