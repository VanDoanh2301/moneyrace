using UnityEngine;

public class DecorativeParticleSpawner : MonoBehaviour
{
    public GameObject particlePrefab;
    public float spawnInterval = 0.6f;
    public float minY = -5f;
    public float maxY = 6f;
    public Color[] palette;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnParticle();
        }
    }

    void SpawnParticle()
    {
        Camera cam = GameManager_jump.Instance.cam;
        Vector3 screenSize = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        float x = cam.transform.position.x + screenSize.x + Random.Range(0.5f, 2f);
        float y = Random.Range(minY, maxY);

        GameObject particle = Instantiate(particlePrefab, new Vector3(x, y, 0), Quaternion.identity);

        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        Color color = palette[Random.Range(0, palette.Length)];
        color.a = Random.Range(0.15f, 0.4f);
        sr.color = color;

        float scale = Random.Range(0.05f, 0.2f);
        particle.transform.localScale = new Vector3(scale, scale, 1f);

        DecorativeParticle dp = particle.GetComponent<DecorativeParticle>();
        dp.velocity = new Vector2(Random.Range(0.3f, 0.9f), Random.Range(-0.15f, 0.15f));
    }
}
