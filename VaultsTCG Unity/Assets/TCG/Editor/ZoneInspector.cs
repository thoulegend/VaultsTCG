using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Zone))]
 class ZoneInspector : Editor 
{

	public override void OnInspectorGUI()
	{
				Zone myTarget = (Zone)target;

				DBZone selected = myTarget.dbzone;
				
				foreach (DBZone foundzone in MainMenu.TCGMaker.core.zones)
						if (GUILayout.Button (foundzone.Name)) {
								myTarget.dbzone = foundzone;
								Debug.Log("found zone: "+foundzone.Name+" , use slots:"+foundzone.UseSlots);
								if (foundzone.Name == "Grid")	myTarget.name = "Zone - Grid"; //gameobject's name
								else	myTarget.name = "Zone - " + "player " + foundzone.Name.ToLower();
						}
				foreach (DBZone foundzone in MainMenu.TCGMaker.core.enemy_zones)
						if (GUILayout.Button (foundzone.Name)) {
								myTarget.dbzone = foundzone;
								myTarget.name = "Zone - " + foundzone.Name.ToLower(); //gameobject's name
								
		}

		GUILayout.Label ("Selected zone: "+selected.Name);
	}
		
}