using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOptions : MonoBehaviour  {

	public static GameOptions instance ; //instance of this singleton




	public void Awake()
		
	{
		
				DontDestroyOnLoad (gameObject);
		

				if (instance == null)
						instance = this;	//singleton

		}	


}
