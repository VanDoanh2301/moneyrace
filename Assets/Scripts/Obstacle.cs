
using UnityEngine;

public class Obstacle : MonoBehaviour
{
	public float zPos = 16.7f;

	public MeshRenderer meshRenderer;

	private float obstacleSpeed;

	private float amplitude;

	private float cosSpeed;

	private float angle;

	private float startX;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Bottom"))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void InitOBstacle(float _obstacleSpeed, float _amplitude, float _cosSpeed, Material _material)
	{
		obstacleSpeed = _obstacleSpeed;
		amplitude = _amplitude;
		cosSpeed = _cosSpeed;
		Vector3 position = base.transform.position;
		startX = position.x;
		meshRenderer.material = _material;
	}

	private void Update()
	{
		Transform transform = base.transform;
		float x = startX + Mathf.Cos(angle) * amplitude;
		Vector3 position = base.transform.position;
		transform.position = new Vector3(x, position.y - obstacleSpeed * Time.deltaTime, zPos);
		angle += Time.deltaTime * cosSpeed;
	}
}
