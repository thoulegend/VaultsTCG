using UnityEngine;
using System.Collections;

public class VictoryDefeat : MonoBehaviour {
	Sprite victoryordefeat;
	public int VictoryCurrency = 50;
	public int DefeatCurrency = 20;
	// Use this for initialization
	void Start () {
		this.tag = "VictoryDefeat";
	}
	

	void EndOfGame () {
		if (Player.Lost) {

			Currency.DoAssignCurrency(Currency.PlayerCurrency+DefeatCurrency); 
			victoryordefeat = playerDeck.pD.defeat;
			GetComponent<SpriteRenderer> ().sprite = victoryordefeat;
				}
		else if (Enemy.Lost)
		{
			Currency.DoAssignCurrency(Currency.PlayerCurrency+VictoryCurrency); 
			victoryordefeat = playerDeck.pD.victory;
			GetComponent<SpriteRenderer> ().sprite = victoryordefeat;
		}
		renderer.sortingOrder = 100;

	}

	void OnGUI()
	{
		Rect windowRect = new Rect(400,300,300,90);
		if (Player.Life <= 0)  windowRect = GUI.Window(0, windowRect, DoMyWindow, "You've received " + DefeatCurrency +  " silver!"); 
		if (Enemy.Life <= 0)  windowRect = GUI.Window(0, windowRect, DoMyWindow, "You've received " + VictoryCurrency +  " silver!"); 	
		//Rect victoryDefeatBox = new Rect (Screen.width * 0.5f, Screen.height * 0.5f, 370, 324);
		//if (Enemy.Lost)
					//	GUI.DrawTexture (victoryDefeatBox, (Texture)Resources.Load ("Victory1"));
		//if (Player.Lost)
			//GUI.DrawTexture (victoryDefeatBox, (Texture)Resources.Load ("Defeat1"));
	}



	void DoMyWindow(int windowID) {
		//if (GUILayout.Button("Back to main menu"))  Application.LoadLevel(MainMenu.SceneNameMainMenu);
			
		
	}

	





}