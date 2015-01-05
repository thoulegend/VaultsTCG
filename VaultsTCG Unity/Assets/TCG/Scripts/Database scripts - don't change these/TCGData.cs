using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class TCGData : ScriptableObject {

	public int test = 2;

	public CoreOptions core;
	 


	public Stats stats;

	public List<DbCard> cards;

	//public Combat combat;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
