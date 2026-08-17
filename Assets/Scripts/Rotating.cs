using System;
using UnityEngine;

public class Rotating : MonoBehaviour
{
	public float speed = 100f;
	private Transform myTransfrom;
	private Rigidbody2D rb2d;
	private Vector3 startPos;
	private Vector3 endPos;
    Rigidbody m_Rigidbody;
    bool down;
    Vector3 m_EulerAngleVelocity;

    //  Joystick joystick;
    private void Start()
	{
        m_EulerAngleVelocity = new Vector3(0, 0, 1);

        //	joystick = GameObject.Find("Floating").GetComponent<FloatingJoystick>();
        //this.rb2d = base.GetComponent<Rigidbody2D>();
        m_Rigidbody = GetComponent<Rigidbody>();
        down = false;

        this.myTransfrom = base.transform;
	}

    
    private void FixedUpdate()
	{

		//base.transform.Rotate(0f, 0f, joystick.Horizontal * this.speed * Time.deltaTime);
	 	if (Input.GetMouseButtonDown(0))
		{
			this.startPos = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
		}
		if (Input.GetMouseButton(0))
		{
			this.endPos = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
			Vector3 vector = this.endPos - this.startPos;
			float num = Mathf.Atan2(vector.x, vector.y) * 57.29578f;

            Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity *num* Time.deltaTime);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);

           // base.transform.Rotate(0f, 0f, num * this.speed * Time.deltaTime);
			this.startPos = this.endPos;
		} 
	}

	 void RotateLeft () {
         Quaternion theRotation = transform.localRotation;
         theRotation.z *= 270;
         transform.localRotation = theRotation;
     }
}
