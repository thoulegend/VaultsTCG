using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EffectToDo {

	public bool AI;
	public card effectcard;
	public int effect_number;
	public List <GameObject> targets = new List<GameObject>();
}


public class EffectManager : MonoBehaviour {
	static List <GameObject> ourtargets = new List<GameObject>();
	static GameObject ourtarget;
	static GameObject secondtarget;
	static public EffectManager instance; 

	public static List<EffectToDo> Stack = new List<EffectToDo>();



	// Use this for initialization
	void Awake()
	{
		instance = this;
		}
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Player.CanDoStack:"+Player.CanDoStack);
		//try to execute the first effect from stack
		if (Stack.Count > 0 && !Player.EffectInProcess && Player.CanDoStack) {

			// here an option to counter the spell/ cast an instant will be added
			Player.EffectInProcess = true;
			Debug.Log ("starting to do the last added effect from the stack");
			EffectToDo lastadded = new EffectToDo();

			lastadded.AI = Stack[Stack.Count-1].AI;
			lastadded.effectcard = Stack[Stack.Count-1].effectcard;
			lastadded.effect_number = Stack[Stack.Count-1].effect_number;
			lastadded.targets =  Stack[Stack.Count-1].targets;
		
			Stack.Remove(Stack[Stack.Count-1]);	

			DoEffect (lastadded.AI, lastadded.effectcard, lastadded.effect_number, lastadded.targets);

						
				}
	}

	public static void AddToStack(bool AI, card effectcard, int effect_number)
	{
		EffectToDo effect_to_add = new EffectToDo();

		effect_to_add.AI = AI;
		effect_to_add.effectcard = effectcard;
		effect_to_add.effect_number = effect_number;
		if (AI)  foreach (GameObject target in  Enemy.targets) effect_to_add.targets.Add(target); 
		else foreach (GameObject target in  Player.targets) effect_to_add.targets.Add(target); 

		Stack.Add (effect_to_add);
		Debug.Log ("added to stack effect from card:" + effectcard.Name + ", for enemy: "+AI);
	}



	public static void DoEffect(bool AI, card effectcard, int effect_number, List<GameObject> effecttargets)
	{
		if (Stack.Count == 0) 
			
		{ 		
			Player.CanDoStack = false;

			Debug.Log("gonna play audio: ");
			GameObject.FindWithTag("Player").audio.PlayOneShot(effectcard.sfxAbility0);
			if (effectcard.Type == 2 ) effectcard.StartCoroutine("SpellAfterEffects", AI); //if it's a spell 
			else if ( effectcard.Type == 4 ) effectcard.StartCoroutine("SecretAfterEffects", AI); // or a secret
			else Player.SpellInProcess = false; //some ability
				
		}

		Debug.Log("DoEffect start, effect_number: "+effect_number+", effects on card: "+effectcard.Effects.Count);


		int Param0=0;
		int param0type = effectcard.Effects[effect_number].param0type;

		int effect = effectcard.Effects[effect_number].type;

		if (effectcard.Type == 1 && effectcard.Effects[effect_number].trigger == 1) //if it's a creature's activated ability
			effectcard.Turn ();

		Debug.Log ("gonna do effect type: " + effect);
	
		if (param0type == 1)	//number of allies
			if (effectcard.ControlledByPlayer) Param0 = Player.player_creatures.Count();
				else Param0 = Enemy.enemy_creatures.Count();
		
		else if (param0type == 2) //number of allies destroyed this turn
			if (effectcard.ControlledByPlayer) Param0 = Player.AlliesDestroyedThisTurn;
				else Param0 = Enemy.AlliesDestroyedThisTurn;

		else Param0 = effectcard.Effects [effect_number].param0;
				

		bool EOT = false;
		int BuffDebuffType = 0;
		//int card_index = effectcard.Index;

	

		Debug.Log ("param0:" + Param0);

		ourtargets = effecttargets;

		switch (effect) { // check the index of the 1st effect and call the appropriate function
		case 0: 	//heal  
			Debug.Log ("heal");
			Heal (Param0); 
			break;
		case 1:		// damage 
			Debug.Log ("damage");
			if (Player.player_creatures.Contains(effectcard) || Enemy.enemy_creatures.Contains(effectcard)) Damage (Param0, effectcard.id_ingame);
			else Damage (Param0, effectcard.id_ingame);
			Debug.Log(" effectcard:"+effectcard.Name + "idingame:" +effectcard.id_ingame);
			break;
		case 2:		//draw card(s)
			Debug.Log ("draw card(s)");
			DrawCard (AI, Param0);
			break;
		case 4: //place a card in your keeper zone (from deck, graveyard, etc)
			
			Debug.Log ("place card in keeper zone");
			
			PutTargetCardInKeeperZone(AI);
			
			
			break;		
		case 5: //place a card in your hand (from deck, graveyard, etc)

			Debug.Log ("place card in hand");

			PutTargetCardInHand(AI);
			
		
			break;
		case 6: //fight between two creatures
			Debug.Log ("brawl");
			Brawl();
			break;
	
		case 8: //untap 
			Debug.Log ("untapattackingtarget");
			UntapTarget();
			break;
		case 9: //kill
			Debug.Log ("destroy");
			 DestroyCreature();
			break;
		case 10: //debuff.  If the EOT boolean is true, this effect only lasts until end of turn.

			EOT = effectcard.Effects[effect_number].eot;

			Debug.Log ("debuff");
			BuffDebuffType = effectcard.Effects[effect_number].bufftype;
			DoBuff(false, Param0, BuffDebuffType, EOT, effectcard);
			break;

		case 11: //buff 

			EOT = effectcard.Effects[effect_number].eot;

			Debug.Log ("buff");
			BuffDebuffType = effectcard.Effects[effect_number].bufftype;
			DoBuff(true, Param0, BuffDebuffType, EOT, effectcard); //we pass effect to let Buff know what type of buff we want
			break;
		case 12: //place creature, param is card index

			Debug.Log ("place creature");
			PlaceCreature(AI, Param0, effectcard); 
			break;
		case 13: //place target creature in game under your control
			
			Debug.Log ("place creature");
			PlaceCreature(AI); 
			break;
		case 15: //gain mana
			
			Debug.Log ("gain mana");
			GainMana(AI, Param0); 
			break;
		default:
			Debug.Log ("effect:"+effect);
			Debug.Log ("there's no such effect in DB!");
			break;
		}

		Player.EffectInProcess = false;



	


	}



	public static void PlaceCreature(bool AI, int Index = -1, card effectcard = null)
	{
		Debug.Log ("gonna place creature now");
		card newcreature;
		if (Index != -1)
		{
			newcreature = playerDeck.pD.MakeCard (Index);
			if (MainMenu.TCGMaker.core.UseGrid) Player.CreaturesZone.slot_to_use = effectcard.transform.parent.GetComponent<Slot>().RandomEmptyAdjacentSlot(); //grid is used by both so we can use just Player.CreaturesZone
			playerDeck.pD.PlaceCreatureInGame (newcreature, AI);
		}
			else 

		{

					
		foreach (GameObject target in ourtargets) {
			card target_card = target.GetComponent<card>();
			

				if (Player.cards_in_hand.Contains(target_card))  Player.RemoveHandCard(target_card); 
					else if (Enemy.cards_in_hand.Contains(target_card))  Enemy.RemoveHandCard(target_card);  

			playerDeck.pD.PlaceCreatureInGame (target.GetComponent<card>(), AI);

			
				if (Player.cards_in_graveyard.Contains(target_card))  { 
					target_card.PlayFX(playerDeck.pD.healfx);
					target_card.RemoveFromGraveyard(); }
				else if (Enemy.cards_in_graveyard.Contains(target_card))  { 
					target_card.PlayFX(playerDeck.pD.healfx);
					target_card.RemoveFromGraveyard(true); }

				}	
		}
	}


	public static void GainMana(bool AI, int amount)
	{
		if (AI)
			GameObject.FindWithTag ("Enemy").SendMessage("GainsMana",amount);
				else
			GameObject.FindWithTag ("Player").SendMessage("GainsMana",amount);
		
	}


	public static void PutTargetCardInKeeperZone(bool AI=false)
	{
		card targetcard;
		foreach (GameObject target in ourtargets)
		{
			targetcard = target.GetComponent<card>();
			if (Player.cards_in_graveyard.Contains(targetcard))   Player.cards_in_graveyard.Remove(targetcard);  
			else if (Enemy.cards_in_graveyard.Contains(targetcard))  Enemy.cards_in_graveyard.Remove(targetcard);
			//here will be checks if the card was from enemy creatures, keepers, own creatures, etc
			if (!AI) {
				
				Player.AddKeeper(targetcard);
				targetcard.renderer.sortingOrder = 0;
				targetcard.ControlledByPlayer = true;
				GameObject.FindWithTag ("Player").SendMessage("TakesCardSFX");
			}
			else 
			{
				GameObject.FindWithTag ("Enemy").SendMessage("TakesCardSFX");
								
				Enemy.AddKeeper(targetcard);	//AI
			}
		}
	}

	public static void PutTargetCardInHand(bool AI=false)
	{
			
		card targetcard;
		foreach (GameObject target in ourtargets)
		{
			targetcard = target.GetComponent<card>();

			if (Player.cards_in_graveyard.Contains(targetcard))   Player.cards_in_graveyard.Remove(targetcard);  
				else if (Enemy.cards_in_graveyard.Contains(targetcard))  Enemy.cards_in_graveyard.Remove(targetcard);
					else if (Player.player_creatures.Contains(targetcard)) Player.RemoveCreature(targetcard);
						else if (Enemy.enemy_creatures.Contains(targetcard))  Enemy.RemoveCreature(targetcard);

			//here will be checks if the card was from enemy creatures, keepers, own creatures, etc
			if (!AI) {

				Player.AddHandCard(targetcard);
				//targetcard.renderer.sortingOrder = 0;
				targetcard.ControlledByPlayer = true;

					}
			else 
			{
				Enemy.AddHandCard(targetcard);	
			}
		}
					
	}



	public static void DrawCard(bool AI, int param) // 
	{

		instance.StartCoroutine (DrawCards(param, AI));
	}
	
	static IEnumerator DrawCards(int param, bool AI=false)
	{
		Zone zone_to_place_cards;
		if (AI) zone_to_place_cards = Enemy.HandZone;
			else zone_to_place_cards = Player.HandZone;

		Debug.Log ("param"+param);
		for (int i=0; i<param; i++) {

			zone_to_place_cards.DrawCard();
			Debug.Log ("i"+i);
			
		}
		yield return new WaitForSeconds(1f);

		
	}

	public static void Heal(int param) // heal 
	{

		foreach (GameObject target in ourtargets)
		{
			Debug.Log("target name:"+target.name);
			target.SendMessage ("IsHealed", param);
		}
	}
	

	public static void Damage(int param, int cardid=-1) // damage 
	{
	
		foreach (GameObject target in ourtargets)
		{
			Debug.Log("target name: "+ target.name);
			target.SendMessage ("IsHitBySpell",new Vector3(param, 0, cardid));
		}
		Debug.Log ("done effect");

	}



	public static void DoBuff(bool positive, int param, int BuffType, bool EOT=false, card effectcard=null) 
	{

		card card_to_buff;
		foreach (GameObject target in ourtargets)
		{
			card_to_buff = target.GetComponent<card>();	//only cards can be buffed for now (not players)

			card_to_buff.AddBuff(positive, param, BuffType, EOT, effectcard);

		}

	}



	public static void Brawl() 
	{
		ourtargets[0].SendMessage ("IsHitBySpell", new Vector3(ourtargets[1].GetComponent<card> ().CreatureOffense, 1, -1));
		ourtargets[1].SendMessage("IsHitBySpell", new Vector3(ourtargets[0].GetComponent<card> ().CreatureOffense, 1, -1));
		Debug.Log ("done effect");
	}

	public static void UntapTarget()
	{
		foreach (GameObject target in ourtargets)
			if (target.GetComponent<card>().IsTurned) target.GetComponent<card>().UnTurn();
		Debug.Log ("done effect");

	}

	public static void DestroyCreature()
	{
		foreach (GameObject target in ourtargets)
			target.GetComponent<card>().Kill();

		Debug.Log ("done effect");



	}

	//Utility methods:

	public static List<GameObject> CreaturesInGame(bool alsoHeroes = false)
	{
		List<GameObject> output = new List<GameObject>();

		foreach (card foundcreature in Player.player_creatures)
						if (!foundcreature.Hero || alsoHeroes) output.Add (foundcreature.gameObject);

		foreach (card foundcreature in Enemy.enemy_creatures)
						if (!foundcreature.Hero || alsoHeroes) output.Add (foundcreature.gameObject);
		return output;

	}

	public static List<GameObject> RandomCreatures(int need_creatures, List<card> cardslist)
	{
		List<GameObject> output = new List<GameObject>();
		
		if (cardslist.Count <= need_creatures) //just all all of them
		{ 
			foreach (card foundcard in cardslist) output.Add(foundcard.gameObject);
			
		}
		else if (need_creatures > 0){
			output.Add(RandomCard(cardslist));
			for (int i = 1; i < need_creatures ; i++){
				
				GameObject tempcreature = RandomCard(cardslist);
				if (!output.Contains(tempcreature))
				{
					output.Add (tempcreature);
				}
				else i--;
			}
		}
		return output;
	}

	public static List<GameObject> RandomGameObjects(int need_objects, List<GameObject> gameobjlist)
	{
		List<GameObject> output = new List<GameObject>();
		
		if (gameobjlist.Count <= need_objects) //just all all of them
		{ 
			foreach (GameObject foundobj in gameobjlist) output.Add(foundobj);
			
		}
		else if (need_objects > 0){
			output.Add(RandomGameObject(gameobjlist));
			for (int i = 1; i < need_objects ; i++){
				
				GameObject tempobj = RandomGameObject(gameobjlist);
				if (!output.Contains(tempobj))
				{
					output.Add (tempobj);
				}
				else i--;
			}
		}
		return output;
	}


	public static List<card> TurnedCreatures(List<card> creatureslist) //Make a list of all tapped creatures in this list
	{

		List<card> output = new List<card>();
		foreach (card foundcreature in creatureslist)
		{
			if (foundcreature.IsTurned == true)
			{
				output.Add (foundcreature);
			}
		}
		return output;
	}


	public static int RandomCardIdFromIntList (List<int> cardlist, int type = -1)
	{
		List<int> foundcards = new List<int>();
		if (type != -1)
		foreach (int foundcard in cardlist) {
			DbCard dbcard = MainMenu.TCGMaker.cards.Where (x => x.id == foundcard).SingleOrDefault ();
			
			if (dbcard != null && dbcard.type == type)
				foundcards.Add (foundcard);
		}
		else
			foundcards = cardlist;
		
		return foundcards[Random.Range(0,foundcards.Count)];

	}

	public static GameObject RandomCardFromIntList (List<int> cardlist, int type = -1)
	{
		List<int> foundcards = new List<int>();
		if (type != -1)
						foreach (int foundcard in cardlist) {
								DbCard dbcard = MainMenu.TCGMaker.cards.Where (x => x.id == foundcard).SingleOrDefault ();
				
					if (dbcard != null && dbcard.type == type)
										foundcards.Add (foundcard);
						}
				else
						foundcards = cardlist;

		int randomcard = foundcards[Random.Range(0,foundcards.Count)];
		//add removal from deck here?

		return playerDeck.pD.MakeCard(randomcard).gameObject;
	}

	public static GameObject RandomGameObject(List<GameObject> objlist)
	{
		return objlist[Random.Range(0,objlist.Count)];
	}

	public static GameObject RandomCard(List<card> cardlist, int type=-1)
	{
		List<card> foundcards = new List<card>();
		if (type != -1) {
						foreach (card foundcard in cardlist) {
								if (foundcard.Type == type)
										foundcards.Add (foundcard);
						}
				} else
						foundcards = cardlist;

		if (foundcards.Count > 0)
						return foundcards [Random.Range (0, foundcards.Count)].gameObject;
				else
						return null;
	}

	public static GameObject HighestAttackCreature(List<card> creatureslist, bool alsoHeroes = false)
	{
		int highestAttackValue = -1; // this will store the highest attack value of player creatures on the field
		foreach (card foundcreature in creatureslist)
		{
			if (foundcreature.CreatureOffense > highestAttackValue && (alsoHeroes || !foundcreature.Hero))
			    highestAttackValue = foundcreature.CreatureOffense;
		}
		List<card> biggestCreatures = new List<card>(); // holds every player creature that has the highest attack value
			    foreach (card foundcreature in creatureslist)
		{
				if (foundcreature.CreatureOffense == highestAttackValue) biggestCreatures.Add (foundcreature);
		}
		// Now that we have a list of the biggest creatures, choose a random one.
		//return biggestCreatures[Random.Range (0, biggestCreatures.Count)].gameObject;
		return biggestCreatures[0].gameObject;
	}

}
