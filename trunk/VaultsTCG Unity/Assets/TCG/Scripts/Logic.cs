using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


// for multiplayer only
public class Logic : MonoBehaviour
{

	public static bool IsMultiplayer = true;

	public static bool IsFirstPlayer = false;
	public static bool CheckedFirst = false;

    
	public static int Player1CardsInHand = 0;
	public static int Player2CardsInHand = 0;

    public static PhotonView ScenePhotonView;

	public static readonly string SceneNameMainMenu = "MainMenuScene";
	public static readonly string SceneNameGame = "GameScene";

    // Use this for initialization
    public void Start()
    {
      if (MainMenu.IsMulti) {
						ScenePhotonView = this.GetComponent<PhotonView> ();
						ScenePhotonView.RPC ("SendEnemyName", PhotonTargets.Others, PhotonNetwork.playerName);
						ScenePhotonView.RPC ("FirstPlayerCanPlay", PhotonTargets.Others);
						
						Debug.Log ("sending name to 1st player");
				}
	}

	public void Awake()
	{
		IsMultiplayer = MainMenu.IsMulti;


		if (IsMultiplayer) { 
						// in case we started this demo with the wrong scene being active, simply load the menu scene
						if (!PhotonNetwork.connected)
						{
							Application.LoadLevel(LobbyMenu.SceneNameMenu);
							return;
						}
						//Debug.Log("checking if isfirstplayer");
						if (PhotonNetwork.playerList.Length == 1) {
								Debug.Log ("isfirstplayer");
								IsFirstPlayer = true;
						}
			else { 	
			}
						
			CheckedFirst = true;
		}
	}

    public void OnJoinedRoom()
    {
       
    }

	void OnGUI()
	{
		if (IsMultiplayer) {
						if (GUILayout.Button ("Return to Lobby")) {
								PhotonNetwork.LeaveRoom ();  // we will load the menu level when we successfully left the room
								Application.LoadLevel(SceneNameMainMenu);
						}
		
						if (PhotonNetwork.connectionStateDetailed == PeerState.Joined) {
								
								//Debug.Log("playerID: " + PhotonNetwork.player.ID);
								
								if (Logic.IsFirstPlayer)
										GUI.Label (new Rect (440, 2, 200, 20), PhotonNetwork.connectionStateDetailed.ToString () + " as Player1");
								else
										GUI.Label (new Rect (440, 2, 200, 20), PhotonNetwork.connectionStateDetailed.ToString () + " as Player2");
						
						} else
								GUI.Label (new Rect (440, 2, 200, 20), PhotonNetwork.connectionStateDetailed.ToString ());
				}
		else if (GUILayout.Button ("Return to Main Menu")) {
			playerDeck.pD.LoadSavedDeck();
			Application.LoadLevel(SceneNameMainMenu);
		}
		}

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
	
	//	PhotonNetwork.LoadLevel(SceneNameGame); //rand matchmaker

	    Debug.Log("OnPhotonPlayerConnected: " + player);

      

        if (PhotonNetwork.isMasterClient)
        {
			Logic.ScenePhotonView.RPC("SendEnemyName", PhotonTargets.Others, PhotonNetwork.playerName);
			Debug.Log ("sending name to 2d player");
			ScenePhotonView.RPC("UpdateEnemyCardsInHand", PhotonTargets.Others, Player.cards_in_hand.Count); //sending player1 hand cards to the new player
          
        }
    }





	[RPC]
	public void UpdateEnemyCardsInHand(int numcards)
	{	
		Enemy.CardsInHand = numcards;
		Debug.Log("Updated enemy hand count");
	}

	[RPC]
	public void UpdateEnemyCardsInDeck(int numcards)
	{	
		Enemy.NumberOfCardsInDeck = numcards;
		Debug.Log("Updated enemy cards in deck");

	}

	public GameObject FindSlotByID(string targetstring) //ex. 5280003  : 5(always) 2(enemy zone) 8(zone id) 000 (always) 3 (slot number) 
	{
		Debug.Log ("trying to find slot: "+targetstring);
		string[] stringSeparators = new string[] {"000"};

		bool playerszone = false;
		if (targetstring [1] == '2') 
						playerszone = true; 

		targetstring = targetstring.Substring (2); //ex. 80003


		int zoneid = System.Int32.Parse(targetstring.Split(stringSeparators, System.StringSplitOptions.None)[0]);	// ex. 8 - zone id
		int slotnumber = System.Int32.Parse(targetstring.Split(stringSeparators, System.StringSplitOptions.None)[1]); // ex. 3 - slot number in zone

		foreach (Zone foundzone in playerDeck.pD.zones) 
						if (foundzone.zone_id == zoneid && foundzone.BelongsToPlayer == playerszone) {
						Debug.Log("found zone with id "+zoneid);
								foreach (Transform child in foundzone.transform){
										Debug.Log("found child");
										if (child.GetComponent<Slot> ())
											{	Debug.Log("found slot number:"+child.GetComponent<Slot> ().number_in_zone+" need number: "+slotnumber);
													if (child.GetComponent<Slot> ().number_in_zone == slotnumber)
														{
															Debug.Log("found slot");
															return child.gameObject;
														}
											}
										}
						}
		return null;
	}

	[RPC]
	public void SendTargets(string targetsdata)
	{	
		Debug.Log ("received targets data: "+targetsdata);
		string[] targets = targetsdata.Split(',');

		Enemy.targets.Clear ();
		int target_id;
		foreach (string targetstring in targets)
		{
			if (targetstring!="")
			{
					target_id = System.Int32.Parse(targetstring);

					if (targetstring.StartsWith("5")) Enemy.targets.Add ( FindSlotByID(targetstring));
					else if (target_id>2) Enemy.targets.Add ( FindCardByID(target_id).gameObject ); //enemy targets some card
					else if (target_id==2) Enemy.targets.Add (  GameObject.FindWithTag ("Player") as GameObject ); //enemy targets our player
					else if (target_id==1) Enemy.targets.Add (  GameObject.FindWithTag ("Enemy") as GameObject ); //enemy targets self
			}
		}

		Debug.Log ("the enemy has send us their targets");
		if (Enemy.targets.Count>0) Debug.Log ("first enemy target:" + Enemy.targets[0].name);
		Enemy.NeedTarget = 0;
	}

	[RPC]
	public void SendUpgradeCreature(int Index)
	{	
		
		card temp_card = playerDeck.pD.MakeCard(Index, true);

		Enemy.targets [0].GetComponent<card> ().Grow (temp_card);
	}

	[RPC]
	public void SendPlayedHandCard(int Index, int id)
	{	

		card temp_card = playerDeck.pD.MakeCard(Index, true);
		temp_card.GetComponent<card>().id_ingame = id;
		

		Enemy.cards_in_hand.Add (temp_card.GetComponent<card> ());

		temp_card.GetComponent<card>().PlayEnemyHandCardMultiplayer();
	}

	[RPC]
	public void SendEffect(int effect_number, int creature_id)
	{	
		card creaturecard = FindCardByID (creature_id);

		//creaturecard.Tap ();
		EffectManager.DoEffect (true, creaturecard, effect_number, Enemy.targets);
	}

	[RPC]
	public void SendPlayedCard(int id)
	{	Debug.Log("received enemy played card, id ingame: " + id);
	
		FindEnemyCardByID(id).PlayEnemyCardMultiplayer();

		
	}

	//[RPC]
	//public void SendTarget(int id)
	//{	
	//	Debug.Log("received enemy target");
	//	FindCardByID(id).AssignEnemyTargetMultiplayer();
		
		
	//}

	[RPC]
	public void SendEnemyName(string enemyname)
	{	
		Debug.Log ("someone joined, received his name");
		Enemy.EnemyName = enemyname;
	
		
		
	}


	[RPC]
	public void SendEndTurn()
	{	
		if (Player.Turn==1) playerDeck.pD.TheSecondPlayerCanPlay = true;
		Player.PlayersTurn = true; //it's the player's turn now
		Player.NewTurn ();
			
	}

	[RPC]
	public void FirstPlayerCanPlay()
	{	
		playerDeck.pD.TheFirstPlayerCanPlay = true;

		
	}
	
	public card FindEnemyCardByID(int id)
	{
		

		foreach (card enemycard in Enemy.cards_in_game) 
		{
			if (enemycard.id_ingame == id) {

				Debug.Log("found played card by id, card name: " + enemycard.Name);
				return enemycard;
			}
			
		}
		Debug.Log("can't find that id!");
		return null;
	}

	public static card FindCardByID(int id)
	{
		
		foreach (card enemycard in Enemy.cards_in_game) 
		{
			if (enemycard.id_ingame == id) {

				Debug.Log("found card by id");
				return enemycard;
			}
			
		}
		foreach (card playercard in Player.cards_in_game) 
		{
			if (playercard.id_ingame == id) {
				
				Debug.Log("found card by id");
				return playercard;
			}
			
		}
		Debug.Log("can't find that id!");
		return null;
	}



    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerDisconnected: " + player);

        if (PhotonNetwork.isMasterClient)
        {
           
        }
    }

    public void OnMasterClientSwitched()
    {
        Debug.Log("OnMasterClientSwitched");
    }
}
