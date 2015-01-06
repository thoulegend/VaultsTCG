using UnityEngine;
using System.Collections;
public class CardTemplate : MonoBehaviourSingleton<CardTemplate>{




	// Use this for initialization
	void Start () {
		if (GetComponent<BoxCollider2D>())
		{
			MainMenu.ColliderWidth = GetComponent<BoxCollider2D>().size.x;
			MainMenu.ColliderHeight = GetComponent<BoxCollider2D>().size.y;
		}
		else if (GetComponent<BoxCollider>()){
			MainMenu.ColliderWidth = GetComponent<BoxCollider>().size.x;
			MainMenu.ColliderHeight = GetComponent<BoxCollider>().size.y;
		}

		gameObject.SetActive(false);
		//if (renderer) renderer.enabled = false;	//hiding our card template
		//foreach (Transform child in transform)
		//	child.renderer.enabled = false;
		
		card.ZoomHeight = MainMenu.ColliderHeight * 2.75f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
