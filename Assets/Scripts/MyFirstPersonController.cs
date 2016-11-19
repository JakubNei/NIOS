using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MyFirstPersonController : MonoBehaviour
{
	public Camera cam;
	public CharacterController characterController;

	public Vector3 moveDir = Vector3.zero;

	public float stickToGroundForce = 10;
	public float gravityMultiplier = 1;

	public float xSensitivity = 50;
	public float ySensitivity = 50;

	public float yMinimum = -90;
	public float yMaximum = 90;

	public float moveSpeed = 5;

	public bool clampVerticalRotation = true;
	public bool smoothRotation = true;
	public float smothRotationSpeed = 5f;

	public bool cursorIsLocked = true;

	// Use this for initialization
	void Start()
	{
		if (!characterController) characterController = GetComponent<CharacterController>();
		if (!cam) cam = Camera.main;
	}


	// Update is called once per frame
	void Update()
	{
		float xRot = Input.GetAxis("Mouse X") * xSensitivity;
		float yRot = Input.GetAxis("Mouse Y") * ySensitivity;


		var currentCharRot = this.transform.localRotation;
		var currentCamRot = cam.transform.localRotation;

		var newCharRot = currentCharRot * Quaternion.Euler(0f, xRot, 0f);
		var newCamRot = currentCamRot * Quaternion.Euler(-yRot, 0f, 0f);

		if (clampVerticalRotation)
			newCamRot = ClampRotationAroundXAxis(newCamRot);

		if (smoothRotation)
		{
			newCharRot = Quaternion.Slerp(currentCharRot, newCharRot, smothRotationSpeed * Time.deltaTime);
			newCamRot = Quaternion.Slerp(currentCamRot, newCamRot, smothRotationSpeed * Time.deltaTime);
		}

		this.transform.localRotation = newCharRot;
		cam.transform.localRotation = newCamRot;


		/*
		if (Input.GetKeyUp(KeyCode.Escape))
			cursorIsLocked = false;
		else if (Input.GetMouseButtonUp(0))
			cursorIsLocked = true;
			*/

		if (cursorIsLocked)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else if (!cursorIsLocked)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
		angleX = Mathf.Clamp(angleX, yMinimum, yMaximum);
		q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

	void FixedUpdate()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = transform.forward * vertical + transform.right * horizontal;

		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
						   characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
		desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

		moveDir.x = desiredMove.x * moveSpeed;
		moveDir.z = desiredMove.z * moveSpeed;


		if (characterController.isGrounded)
		{
			moveDir.y = -stickToGroundForce;
		}
		else
		{
			moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
		}

		characterController.Move(moveDir * Time.fixedDeltaTime);
	}



}
