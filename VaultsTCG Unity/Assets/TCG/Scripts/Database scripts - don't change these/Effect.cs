using UnityEngine;

using System.Collections;

using System.Collections.Generic;

[System.Serializable]
public class Effect
{
	public bool creatureability = false;
	public bool hastrigger = false;
	
	public int type=-1;
	public string name="";
	
	public int target=-1;

	public int param0type = 0;

	public int param0=-1;
	public int param1=-1;
	
	public int targetparam0=-1;
	public int targetparam1=-1;

	public int triggerparam0=-1;

	public int bufftype=-1;
	public bool eot = false;
	
	public int trigger=-1;
	
	public List<ManaColor> cost = new List<ManaColor>();
	public int discardcost=-1;
	
	
	
	public Effect(bool e_hastrigger = false, bool e_creatureability = false) {
		creatureability = e_creatureability;
		hastrigger = e_hastrigger;
	}
}