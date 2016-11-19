
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public GameObject cursor;


	MyFirstPersonController fpc;

	void Start()
	{
		fpc = GetComponent<MyFirstPersonController>();
	}

	void Update()
	{
		var cam = Camera.main.transform;
		RaycastHit hitInfo;
		if (fpc.enabled && Physics.Raycast(cam.transform.position + cam.forward, cam.forward, out hitInfo))
		{
			cursor.SetActive(true);
			cursor.transform.position = hitInfo.point;

			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				var p = hitInfo.transform.root.gameObject.GetComponent<IPlayerTouched>();
				if (p != null)
					p.OnTouched(this);
				else
					Debug.Log(hitInfo.transform.root.gameObject.name + " does not have IPlayerTouched component");
			}

		}
		else
		{
			cursor.SetActive(false);
		}
	}

	public void InputEnabled(bool enabled)
	{		
		fpc.enabled = enabled;
	}

}