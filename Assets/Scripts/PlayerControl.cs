
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractionEvent
{
	public PlayerControl player;
}

public class InputControlContext
{
	public bool IsActive;
	public event Action OnExit;
	public void IsExiting()
	{
		OnExit.Raise();
	}
}

public class PlayerControl : MonoBehaviour
{
	public GameObject cursor;

	Stack<InputControlContext> inputStack = new Stack<InputControlContext>();

	MyFirstPersonController fpc;

	void Start()
	{
		fpc = GetComponent<MyFirstPersonController>();

		OverrideInput();
		OverrideInput();
	}

	public InputControlContext OverrideInput()
	{
		var other = new InputControlContext();
		other.IsActive = true;
		if (inputStack.Count > 0)
			inputStack.Peek().IsActive = false;
		inputStack.Push(other);
		return other;
	}

	void LateUpdate()
	{
		if (inputStack.Count > 1 && Input.GetKeyDown(KeyCode.Escape))
		{
			var e = inputStack.Pop();
			e.IsExiting();
			if (inputStack.Count > 0)
				inputStack.Peek().IsActive = true;
		}


		if (inputStack.Count == 1)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				OverrideInput();
			}

		}
		
		if (inputStack.Count == 2)
		{
			fpc.enabled = true;

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			var cam = Camera.main.transform;
			RaycastHit hitInfo;
			if (fpc.enabled && Physics.Raycast(cam.transform.position + cam.forward, cam.forward, out hitInfo))
			{
				cursor.SetActive(true);
				cursor.transform.position = hitInfo.point;

				if (Input.GetKeyDown(KeyCode.Mouse0))
				{
					var p = hitInfo.transform.GetComponentInChildren<IInteraction>();
					if(p == null)
						p = hitInfo.transform.root.GetComponentInChildren<IInteraction>();

					if (p != null)
					{
						var data = new InteractionEvent();
						data.player = this;
						p.OnTouched(data);
					}
					else
						Debug.Log(hitInfo.transform.root.gameObject.name + " does not have IPlayerTouched component");
				}

			}
			else
			{
				cursor.SetActive(false);
			}

		}
		else
		{
			fpc.enabled = false;
			cursor.SetActive(false);
		}
	}

}