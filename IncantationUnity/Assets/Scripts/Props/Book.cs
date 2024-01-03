using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Book : Prop
{
	public AudioClip openSound;
	public AudioClip closeSound;

	public bool IsOpen()
	{
		return gameObject.activeInHierarchy;
	}

	public void Toggle()
	{
		if (IsOpen())
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void Open()
	{
		if (IsOpen())
		{
			return;
		}
		gameObject.SetActive(true);
		SFXManager.Play(openSound, MixerGroup.Book, transform.position);
	}

	public void Close()
	{
		if (!IsOpen())
		{
			return;
		}
		gameObject.SetActive(false);
		SFXManager.Play(closeSound, MixerGroup.Book, transform.position);
	}
}
