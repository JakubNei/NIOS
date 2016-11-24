using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class PressableButton : MonoBehaviour, IInteraction
{

	public UnityEvent onPressed;

	public void OnTouched(InteractionEvent data)
	{
		onPressed.Invoke();
	}

}
