using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {


	void Awake()	{
	


		int i;
		bool foundname;	
		foreach (Zone foundzone in Object.FindObjectsOfType(typeof(Zone)) as Zone[]) {
						
						foundname = false;
						foreach (DBZone dbz in MainMenu.TCGMaker.core.zones)
							if (dbz.Name == foundzone.dbzone.Name) {
														foundzone.dbzone = dbz; 
														foundname = true; 
														foundzone.zone_id = MainMenu.TCGMaker.core.zones.IndexOf(dbz);
														break;
													}
						
						if (!foundname) foreach (DBZone dbz_e in MainMenu.TCGMaker.core.enemy_zones)
								if (dbz_e.Name == foundzone.dbzone.Name) {
								foundzone.dbzone = dbz_e;
								foundname = true;
								foundzone.BelongsToPlayer = false;
								foundzone.zone_id = MainMenu.TCGMaker.core.enemy_zones.IndexOf(dbz_e);
								break;
							}
						
						if (foundname)
							{
								foundzone.Awake ();
								playerDeck.pD.zones.Add(foundzone); 
								i = 0;
								
								foreach (Transform child in foundzone.transform) //looping through zone slots
									if (child.GetComponent<Slot>()) 
									{
										child.GetComponent<Slot>().number_in_zone = i;
										i++;

										if (foundzone.dbzone.Name == "Grid")
											{
												
												playerDeck.pD.Grid[child.GetComponent<Slot>().row, child.GetComponent<Slot>().column] = child.GetComponent<Slot>();
											}
										
									}
								//Debug.Log("found zone: "+foundzone.dbzone.Name+" , use slots:"+foundzone.dbzone.UseSlots);

								switch (foundzone.dbzone.Name)
								{
									case "Creatures":
									Player.CreaturesZone = foundzone;
									break;
									case "Enemy creatures":
										Enemy.CreaturesZone = foundzone;
										break;
									case "Hand":
										Player.HandZone = foundzone;
										break;
									case "Enemy hand":
										Enemy.HandZone = foundzone;
										break;
									case "Keepers":
										Player.KeepersZone = foundzone;
										break;
									case "Enemy keepers":
										Enemy.KeepersZone = foundzone;
										break;
									case "Graveyard":
										Player.GraveyardZone = foundzone;
										break;
									case "Enemy graveyard":
										Enemy.GraveyardZone = foundzone;
										break;
								}
							//player and enemy zones should have the same id
					if (MainMenu.TCGMaker.core.enemy_zones.Contains(foundzone.dbzone)) foundzone.zone_id = MainMenu.TCGMaker.core.enemy_zones.IndexOf(foundzone.dbzone);
					    else if (MainMenu.TCGMaker.core.zones.Contains(foundzone.dbzone)) foundzone.zone_id = MainMenu.TCGMaker.core.zones.IndexOf(foundzone.dbzone);
					        
			}
			else Debug.LogWarning("zone not found: "+foundzone.name);
		}


		playerDeck.pD.TheSecondPlayerCanPlay = false;
		playerDeck.pD.TheFirstPlayerCanPlay = false;


		Enemy.StartGame();	
		Player.StartGame();	

		if (MainMenu.TCGMaker.core.UseGrid){
			Debug.Log("using grid");
			
			
			Player.CreaturesZone = GameObject.Find("Zone - Grid").GetComponent<Zone>();
			Enemy.CreaturesZone = GameObject.Find("Zone - Grid").GetComponent<Zone>();
			
			//adding heroes:
			
			Player.CreaturesZone.slot_to_use = GameObject.FindWithTag("Player").GetComponent<Player>().hero_slot.transform;
			Player.AddCreature(playerDeck.pD.MakeCard(0));
			Enemy.CreaturesZone.slot_to_use = GameObject.FindWithTag("Enemy").GetComponent<Enemy>().hero_slot.transform;
			Enemy.AddCreature(playerDeck.pD.MakeCard(1, true));
		}
		//Debug.Log("deck count playerdeck"+ Deck.Count.ToString());
		//TextAsset enemydeck = Resources.Load("enemydeck") as TextAsset;

		playerDeck.pD.SaveDeckBeforePlaying();

		Enemy.Deck = playerDeck.pD.LoadDeck(playerDeck.pD.offlineDeck.text);
		Enemy.NumberOfCardsInDeck = Enemy.Deck.Count;
		
		playerDeck.pD.ShuffleDeck(playerDeck.pD.Deck);
		playerDeck.pD.ShuffleDeck(Enemy.Deck);
		
		
		
		
		StartCoroutine(StartGame());
		}

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
	
	}

	


	IEnumerator StartGame()
		
	{		
		// drawing cards for each player
		//float time_between_cards = 0.5f;
		
		Debug.Log("ISMULTI: "+ MainMenu.IsMulti);
		if (MainMenu.IsMulti) { 
		
			//while (RndMatchmaker.Joined==false)	yield return new WaitForSeconds (0.1f); //don't draw cards until you are connected to the room	
			while (Logic.CheckedFirst==false)	yield return new WaitForSeconds (0.1f); 	
			if (Logic.IsFirstPlayer) {
				
				Debug.Log("isfirstplayer");
				playerDeck.pD.first_or_second = 1;
				
				Player.PlayersTurn = true;
				
				while (playerDeck.pD.TheFirstPlayerCanPlay==false)	yield return new WaitForSeconds (0.4f); //don't draw cards until it's your turn
			
			

				Player.NewTurn(); //first turn
				//drawing cards for player:
				foreach (Zone foundzone in Object.FindObjectsOfType(typeof(Zone)) as Zone[]) 
					if (foundzone.DrawAtTheStartOfGame > 0 && foundzone.BelongsToPlayer) foundzone.StartCoroutine("DrawCards", foundzone.DrawAtTheStartOfGame);
				
			} else { 
				
				//the player is the second player
		
				playerDeck.pD.first_or_second = 2;
				Enemy.NewTurn ();
				//Player.Turn = 1;
				
				while (playerDeck.pD.TheSecondPlayerCanPlay==false)	yield return new WaitForSeconds (0.4f); //don't draw cards until it's your turn


				//drawing cards for player:
				foreach (Zone foundzone in Object.FindObjectsOfType(typeof(Zone)) as Zone[]) 
					if (foundzone.DrawAtTheStartOfGame > 0 && foundzone.BelongsToPlayer) foundzone.StartCoroutine("DrawCards", foundzone.DrawAtTheStartOfGame);
			}
			
			
			
		} else { 		//single player vs AI
			
			playerDeck.pD.first_or_second = 1;
		
			//draw cards for all

			foreach (Zone foundzone in Object.FindObjectsOfType(typeof(Zone)) as Zone[]) 
				if (foundzone.DrawAtTheStartOfGame > 0 ) foundzone.StartCoroutine("DrawCards", foundzone.DrawAtTheStartOfGame);

			Player.NewTurn();
			
			Debug.Log ("activating player's turn");
			Player.PlayersTurn = true;
		}
		
	}

	public static void RemoveAllEOTBuffsAndDebuffs()
	{
		//Debug.Log ("gonna remove EOT buffs and debuffs");
		foreach (card foundcard in Player.cards_in_game) 
			if (foundcard.Type==1) //if it's a creature
				foundcard.RemoveEOTBuffsAndDebuffs();

		
		foreach (card foundcard in Enemy.cards_in_game) 
			if (foundcard.Type==1) //if it's a creature
				foundcard.RemoveEOTBuffsAndDebuffs();
				
	}
}
