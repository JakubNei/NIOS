using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class PressableButton : MonoBehaviour, IPlayerTouched
{

	public UnityEvent onPressed;

	public void OnTouched(PlayerControl player)
	{
		onPressed.Invoke();
	}

}
