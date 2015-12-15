using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	#region Public Variables
	public float Speed;
	public float JumpSpeed;
	public float FallSpeed;
    public bool Paused = true;
    #endregion

    //public GameObject textCanvas;

    //DisplayText textDisplay;
    [SerializeField]
    Vector3 RotationVelocity;
	#region Private Variables
	Rigidbody rigid;
	Vector3 movement;
	int floorMask;
	float camRayLength = 150f;
    Vector3 absoluteRotation;
    Vector3 mouseDelta;
	#endregion


	// Use this for initialization
	void Start () {
		floorMask = LayerMask.GetMask ("Floor");
		rigid = GetComponent<Rigidbody> ();
        //textDisplay = textCanvas.GetComponent<DisplayText> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
        mouseDelta.x = Input.GetAxisRaw("Mouse X");
        mouseDelta.y = Input.GetAxisRaw("Mouse Y");

        Cursor.visible = Paused;
        if (!Paused)
        {
            Move(h, v);
            Turn();
        }
	}

	void Move(float h, float v)
	{
		movement.Set(h,0,v);
        Quaternion rot = Quaternion.EulerRotation(0, absoluteRotation.y, 0);
        movement = rot*movement;
		movement = movement.normalized * Speed * Time.deltaTime;
		if (Input.GetKey ("space"))
			movement += new Vector3 (0, JumpSpeed, 0) * Time.deltaTime;
		else if (Input.GetKey ("left shift"))
			//movement *= 3;
			movement -= new Vector3 (0, FallSpeed, 0) * Time.deltaTime;
		rigid.MovePosition (movement + transform.position);

	}

	void Turn()
	{
        absoluteRotation.x = Mathf.Clamp(absoluteRotation.x + (mouseDelta.y) * RotationVelocity.x, -Mathf.PI / 2.0f, Mathf.PI / 2.0f);
        absoluteRotation.y += (mouseDelta.x) * RotationVelocity.y;
        while (absoluteRotation.y > 2 * Mathf.PI)
            absoluteRotation.y -= 2 * Mathf.PI;
        while (absoluteRotation.y < 0)
            absoluteRotation.y += 2 * Mathf.PI;

        Quaternion rotation = Quaternion.EulerRotation(-absoluteRotation.x,absoluteRotation.y,0);
        rigid.MoveRotation(rotation);
	}
  
}
