using UnityEngine;
using System.Collections;
using UnityEditor;

public class SceneScripts : MonoBehaviour {


	
	
	

		[DrawGizmo(GizmoType.NotSelected | GizmoType.Pickable)]
		
		private static void Draw(GameObject gObj, GizmoType gizmoType)	//drawing an outline for all slots and zones
			
		{
				GUIStyle style = new GUIStyle();
				
				style.alignment = TextAnchor.MiddleCenter;
				style.fontStyle = FontStyle.Bold;
				if (gObj.GetComponent<Zone> () != null) {
						//if (!gObj.GetComponent<Zone> ().UseSlots) {
								style.normal.textColor = Color.yellow;
								Handles.Label(gObj.collider.bounds.center, gObj.name, style);
								Bounds bounds = gObj.collider.bounds;
					
								Gizmos.color = Color.yellow;
					
								Gizmos.DrawWireCube (bounds.center, bounds.size);
					
						//}
				
				} else if (gObj.GetComponent<Slot> () != null) {
						if (gObj.transform.parent != null)
						if (gObj.transform.parent.GetComponent<Zone> () != null)
						//if (gObj.transform.parent.GetComponent<Zone> ().UseSlots) {
								style.normal.textColor = Color.cyan;
								Handles.Label(gObj.collider.bounds.center, gObj.name, style);
								Bounds bounds = gObj.collider.bounds;
						
								Gizmos.color = Color.cyan;
						
								Gizmos.DrawWireCube (bounds.center, bounds.size);
						//}
				}
			
			
		}	

		
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
