using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public GameObject book;

	private void Update()
	{
		bool book_input = Input.GetKeyDown(KeyCode.Tab);
		if (book_input)
		{
			book.SetActive(!book.activeInHierarchy);
		}
	}
}
