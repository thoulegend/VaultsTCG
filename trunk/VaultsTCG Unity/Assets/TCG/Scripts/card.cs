using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class card : MonoBehaviour {

	public static AudioClip Hit;
	public static AudioClip HitBySpell;
	public static AudioClip Healed;

	public AudioClip sfxMove0;
	public AudioClip sfxMove1;

	public AudioClip sfxEntry; // sound effect to be played when card is cast
	public AudioClip sfxAbility0; // sound effect for creature ability 0

	public List<Effect> Effects = new List<Effect>();  

	public bool Secret = false;
	public bool faceDown = false;

	//public bool SpellUnresolved = false;

	public Sprite Art;
	public int Index = 0;
	public List<ManaColor> Cost;
	public int Level = 0;
	public string GrowID = "";

	public int DiscardCost = 0; // a card can require a number of cards to be discarded as additional cost

	public  int CostInCurrency = 0; //this is for buying cards from the shop, it doesn't affect gameplay

	public Dictionary<string, int> CustomInts = new Dictionary<string, int>() ; 
	public Dictionary<string, string> CustomStrings = new Dictionary<string, string>() ;

	public GameObject highlight = null;

	public int Type = 0;
	public int Subtype = 0;

	public ManaColor CardColor;

	public int CreatureOffense = 0;
	public int CreatureDefense = 0;
	public int Defense
	{
		get {
			if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) return CreatureOffense;
			else return CreatureDefense;

		}
		protected set {
			if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) CreatureOffense = value;
			else CreatureDefense = value;
			
		}
	}

	public abilities abilities = null;
	public List<Buff> buffs;

	public int CreatureStartingOffense = 0;
	public int CreatureStartingDefense = 0;

	public int StartingDefense
	{
		get {
			if (MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) return CreatureStartingOffense;
			else return CreatureStartingDefense;
			
		}
	
	}

	public int CritChance = 0;
	public float CritDamageMultiplier = 2.5f;

	public bool FirstTurnInGame = true;

	public bool IsTurned = false;
	public bool ControlledByPlayer = false;

	public bool DoubleDamage = false;
	public bool TakesHalfDamage = false;

	public Transform healfx;
	public Transform firefx;
	 
	public bool Dead = false;
	public bool ShowedByEnemy = false;

	//default keyword abilities:
	public bool takes_no_combat_dmg = false;
	public bool deals_no_combat_dmg = false;
	public bool no_first_turn_sickness = false;
	public bool cant_attack = false;
	public bool free_attack = false;
	public bool takes_no_spell_dmg = false;
	
	public bool extramovement = false;
	public bool less_dmg_from_ranged = false;
	public bool no_dmg_from_ranged = false;
	//default keyword abilities

	public int AttackedThisTurn = 0;

	//Z-s
	public bool Ranged = false;
	public int MovedThisTurn = 0;
	//Z-s

	public static float ZoomHeight; 

	public float Zoom; 

	public static float ZoomEditDeckMode = 1.7f;
	public string Name="";


	public int id_ingame;

	public bool Hero = false;

	//variables for edit deck mode:
	public int Amount;
	public bool FilteredOut = false;
	public bool InCollection = false;


	public bool IsZoomed=false;
	bool IsRotatedForZoom=false;
	bool IsMovedForZoom=false;

	public bool isDragged = false;

	public bool checked_for_highlight = false;

	public static bool WaitABit=false;
	//for zoom/unzoom:
	float old_y;
	int old_sortingorder;

	// Seconds the mouse is hovering over a card
	float mouseHoverSeconds = 0;
	float mouseHoverZoomTime = 0.5f; // amount of time of mousehover before showing full card

	static List<int> NoTargetEffects = new List<int> {2, 12, 15};

	public Slot slot
	{
		get
		{	if (transform.parent != null)
			return transform.parent.GetComponent<Slot>();

			return null;
		}
	}
	 
	// Use this for initialization
	void Start () {


		//foreach (CustomStat customstat in TCGMakerData.stats.CustomInts) //populating custom stats
			//CustomInts.Add(customstat.m_name, )

		Hit = playerDeck.pD.Hit;
		HitBySpell = playerDeck.pD.HitBySpell;
		Healed = playerDeck.pD.Healed;



	}

	void AddHighlight()
	{
		//Debug.Log ("adding highlight to card: "+Name);
		//highlight = (GameObject) Instantiate(Resources.Load("HandCardHighlight"), transform.position, Quaternion.Euler(90, 0, 0));
		highlight = (GameObject) Instantiate(CardTemplate.Instance.transform.Find("Highlight").gameObject);
		//highlight.transform.parent = transform;

		playerDeck.pD.AssignParentWithLocalPos (highlight, gameObject);
		//if (transform.parent) highlight.localScale =  


		//float adjustment = 1f;
		//highlight.transform.localScale =  new Vector3 (adjustment*transform.localScale.x, adjustment*transform.localScale.y, adjustment*transform.localScale.z);
	}

	bool IsPlayable()
	{

		if (!Player.PlayersTurn) return false;

		if (Player.cards_in_hand.Contains (this)) {
						if (Type == 0 && Player.KeepersPlayedThisTurn == 0) //keeper
								return true;

						if (Type == 1 && CanPayCosts (true)  ) 
							if (Level < 1) return true;	//creature
									else if (HasUpgradableCreature(Player.player_creatures)) return true; //creature upgrade

						

						if (Type > 1 && CanPayCosts (true) && ValidSpell (false)) //spells, enchantments, etc
								return true;
				} 
		else if (Player.player_creatures.Contains (this))
				if (CanAttack ())
						return true;

		return false;

	}


	void Update()
	{
		if (!checked_for_highlight && highlight) 
		{
			if (!IsPlayable()) Destroy (highlight);
		}
		else if (!checked_for_highlight && !Player.SpellInProcess && !Player.EffectInProcess && IsPlayable() )
			AddHighlight();

		checked_for_highlight = true;

		if (IsACreature())   
		
			 if (Defense<=0 && !Dead ) 	
			{
				Kill();  //if it has just been killed

				if (Hero)
					if (ControlledByPlayer) Player.HeroIsDead = true;
						else Enemy.HeroIsDead = true;
			}

	}

	public void RemoveEOTBuffsAndDebuffs ()
	{
		List<Buff> buffs_to_remove = new List<Buff>();

		foreach (Buff foundbuff in buffs)
						if (foundbuff.EOT)
							buffs_to_remove.Add (foundbuff);

		foreach (Buff foundEOTbuff in buffs_to_remove)
			RemoveBuff(foundEOTbuff);
				
	}

	
	public void RemoveBuff (Buff buff_to_remove)
	{
		bool positive = buff_to_remove.positive;
		int param = buff_to_remove.param;

		//OFFENSE BUFFS:
		switch (buff_to_remove.type) { 
		case Buff.SET_ATTACK_TO: 	
			CreatureOffense = CreatureStartingOffense;
			break;
		case Buff.RAISE_ATTACK_BY:
			if (!positive) param = -param;
			CreatureOffense -= param;
			break;
		case Buff.MULTIPLY_ATTACK_BY:
			if (!positive) param = 1/param;
			CreatureOffense /= param;
			break;
		//DEFENSE BUFFS:
		case Buff.SET_DEFENSE_TO: 	
			CreatureDefense = CreatureStartingDefense;
			break;
		case Buff.RAISE_DEFENSE_BY:
			if (!positive) param = -param;
			CreatureDefense -= param;
			break;
		case Buff.MULTIPLY_DEFENSE_BY:
			if (!positive) param = 1/param;
			CreatureDefense /= param;
			break;
		//MISC BUFFS:
		case Buff.SET_CRIT_CHANCE_TO: 	
			CritChance = 0;
			break;
		case Buff.ASSIGN_ABILITY:
			if (!positive) ReturnAbilities();
				else AssignSpecialAbility(param, false);
			break;
			}
		buffs.Remove (buff_to_remove);
	}

	public void ReturnAbilities()
	{
		DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == Index).SingleOrDefault();
		Effects = dbcard.effects;

		takes_no_combat_dmg = dbcard.takes_no_combat_dmg;
		deals_no_combat_dmg = dbcard.deals_no_combat_dmg;
		no_first_turn_sickness = dbcard.no_first_turn_sickness;
		cant_attack = dbcard.cant_attack;
		free_attack = dbcard.free_attack;
		takes_no_spell_dmg = dbcard.takes_no_spell_dmg;
		extramovement = dbcard.extramovement;
		less_dmg_from_ranged = dbcard.less_dmg_from_ranged;
		no_dmg_from_ranged = dbcard.no_dmg_from_ranged;
		
		if (transform.Find("Description3DText")!=null)
			transform.Find("Description3DText").GetComponent<TextMesh>().text = playerDeck.TextWrap(dbcard.text, 30); //clear the card text
	}

	public void RemoveAllAbilities()
	{
		Effects.Clear();

		takes_no_combat_dmg = false;
		deals_no_combat_dmg = false;
		no_first_turn_sickness = false;
		cant_attack = false;
		free_attack = false;
		takes_no_spell_dmg = false;
		extramovement = false;
		less_dmg_from_ranged = false;
		no_dmg_from_ranged = false;

		if (transform.Find("Description3DText")!=null)
			transform.Find("Description3DText").GetComponent<TextMesh>().text = ""; //clear the card text
	}

	public void AddBuff(bool positive, int param, int BuffType, bool EOT=false, card effectcard=null)
	{
			Debug.Log ("creature is being buffed for:" + param +", buff is positive: "+positive);
			Debug.Log ("BuffType" + BuffType);
			Buff newbuff = new Buff();
			
			switch (BuffType) {
			//OFFENSE BUFFS:
				case Buff.SET_ATTACK_TO: 	
						CreatureOffense = param;
						break;
				case Buff.RAISE_ATTACK_BY:
						if (!positive) param = -param;
						CreatureOffense += param;
						break;
				case Buff.MULTIPLY_ATTACK_BY:
						if (!positive) param = 1/param;
						CreatureOffense *= param;
						break;
			//DEFENSE BUFFS:
				case Buff.SET_DEFENSE_TO: 	
						CreatureDefense = param;
						break;
				case Buff.RAISE_DEFENSE_BY:
						if (!positive) param = -param;
						CreatureDefense += param;
						break;
				case Buff.MULTIPLY_DEFENSE_BY:
						if (!positive) param = 1/param;
						CreatureDefense *= param;
						break;
			//MISC BUFFS:
				case Buff.SET_CRIT_CHANCE_TO: 	
						CritChance = param;
						break;
				case Buff.ASSIGN_ABILITY: 	
						if (!positive) RemoveAllAbilities();
							else AssignSpecialAbility(param);
						break;
				}
			UpdateCreatureAtkDefLabels();
				
			
			newbuff.type = BuffType;
			newbuff.positive = positive;
			newbuff.param = param;
			newbuff.EOT = EOT;
						
			if (effectcard.Type == 3 )newbuff.enchantmentcard = effectcard; // if it's an enchantment
			buffs.Add(newbuff);
	}

	public void AssignSpecialAbility(int ability_code, bool setTrue = true)
	{

		switch (ability_code)	{
			case Buff.DEALS_NO_COMBAT_DMG:
				deals_no_combat_dmg = setTrue;
				break;
			case Buff.TAKES_NO_COMBAT_DMG:
				takes_no_combat_dmg = setTrue;
				break;
			case Buff.CANT_ATTACK:
				cant_attack = setTrue;
				break;
			case Buff.EXTRA_MOVEMENT:
				extramovement = setTrue;
				break;
			case Buff.FIRST_ATTACK_DOESNT_TURN:
				free_attack = setTrue;
				break;
			case Buff.TAKES_NO_DMG_FROM_SPELLS:
				takes_no_spell_dmg = setTrue;
				break;
			case Buff.NO_FIRST_TURN_SICKNESS:
				no_first_turn_sickness = setTrue;
				break;
		}
	}

	public void UpdateCreatureAtkDefLabels()
	{
//		Debug.Log("starting off: " + CreatureStartingOffense + "off:" + CreatureOffense );

		transform.Find("Offense3DText").GetComponent<TextMesh>().text = CreatureOffense.ToString();

		if ( CreatureStartingOffense == CreatureOffense ) transform.Find("Offense3DText").renderer.material.SetColor ("_Color", Color.white); 
		else if ( CreatureStartingOffense < CreatureOffense ) transform.Find("Offense3DText").renderer.material.SetColor ("_Color", Color.blue); 
		else if  ( CreatureStartingOffense > CreatureOffense ) transform.Find("Offense3DText").renderer.material.SetColor ("_Color", Color.red); 

		if (!MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures) {
						transform.Find ("Defense3DText").GetComponent<TextMesh> ().text = CreatureDefense.ToString ();
						if (StartingDefense == Defense)
								transform.Find ("Defense3DText").renderer.material.SetColor ("_Color", Color.white);
						else if (StartingDefense < Defense)
								transform.Find ("Defense3DText").renderer.material.SetColor ("_Color", Color.blue);
						else if (StartingDefense > Defense)
								transform.Find ("Defense3DText").renderer.material.SetColor ("_Color", Color.red); 
				}
	}



	public void Kill()
	{
		Debug.Log ("killing card:"+Name);

		if (ControlledByPlayer) {
						Player.TriggerCardAbilities(abilities.ON_FRIENDLY_DIES);	//triggering creature abilities and secrets
						Player.AlliesDestroyedThisTurn++;
						Player.RemoveCreature(this);
				}
		else  {
			Enemy.TriggerCardAbilities(abilities.ON_FRIENDLY_DIES);	//triggering creature abilities and secrets
			Enemy.AlliesDestroyedThisTurn++;
			Enemy.RemoveCreature(this);
		}


		if (MainMenu.TCGMaker.core.OptionGraveyard)
						MoveToGraveyard ();
				else 
						Destroy (gameObject);
				
	}

	void MoveToGraveyard()
	{
		//if (IsZoomed)UnZoom ();
		if (IsTurned) UnTurn();
		Dead = true;


		Debug.Log ("moving to graveyard:" + Name );

		Zone graveyard;
		
		if (ControlledByPlayer) 	{ ControlledByPlayer = false; graveyard = Player.GraveyardZone; }
		else graveyard = Enemy.GraveyardZone;			
		
		graveyard.AddCard (this);

		if (Type == 1) { //if it's a creature
			for (int i = 0; i < buffs.Count; i++)	//removing all buffs, note that "foreach" can't be used here
				RemoveBuff(buffs[i]);

			CreatureOffense = CreatureStartingOffense; //removing all effects and damage from the atk/def labels

			transform.Find("Offense3DText").GetComponent<TextMesh>().text = CreatureOffense.ToString();

			if (!MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures)
			{
				CreatureDefense = CreatureStartingDefense; 
				transform.Find("Defense3DText").GetComponent<TextMesh>().text = CreatureDefense.ToString();
			}

			UpdateCreatureAtkDefLabels(); //to make text white again
		}


		Destroy (highlight);
	}

	public void RemoveFromGraveyard(bool AI=false)
	{
	
		Dead = false;
		
		Zone graveyard;

		if (AI) 	graveyard = Enemy.GraveyardZone;
		else {
			ControlledByPlayer = true;
			graveyard = Player.GraveyardZone;
		}
		
			
		graveyard.RemoveCard (this);
	}

	void OnMouseOver () 
	{

		if (MainMenu.TCGMaker.core.UseGrid && Player.NeedTarget == 1) //player needs to choose a target for an attack
		{
			//Debug.Log("mouseover for attack");
			if (Enemy.cards_in_game.Contains(this))
				if (Player.AttackerCreature.slot.IsAdjacent(slot)) slot.Highlight();
					else if (Player.AttackerCreature.Ranged)
						{
							Debug.Log("target valid if in line");
							List<Slot> foundpath = Player.AttackerCreature.slot.IsInALine(slot);
							//Debug.Log("foundpath: "+foundpath);
							if (foundpath != null)
								foreach (Slot foundslot in  Player.AttackerCreature.slot.IsInALine(slot)) //if the target is in a line to our ranged creature, highlight the path to target
									foundslot.Highlight();

						}


		}

		mouseHoverSeconds += Time.deltaTime;
		//Debug.Log("onmouseover: "+ Name +", for time: "+mouseHoverSeconds);
		if (ShowedByEnemy)
			return;
		
		if (IsZoomed == false && mouseHoverSeconds >= mouseHoverZoomTime) 
		{
			ZoomCard ();
		}
		

		 if (Input.GetMouseButtonDown (1)) {
			Debug.Log("right click");

			if (Player.cards_in_game.Contains(this)) { 
			
				Debug.Log("displaying menu"); 
				abilities.DisplayMenu = true;


			} 
		}
		
		else if (Input.GetMouseButtonDown (2)) {

		}
	}

	void OnMouseExit() 
	{

		if (MainMenu.TCGMaker.core.UseGrid && slot != null && !slot.zone.PlayerIsChoosingASlot)
			Player.CreaturesZone.RemoveHighlightedSlots();

		if (ShowedByEnemy)
			return;
		if (IsZoomed == true) { 
				//UnZoom();
			Debug.Log("OnMouseExit starting to unzoom if we're allowed");
			UnZoom();
		}
	}


	
	
	void ZoomCard()
	{
		Debug.Log ("zooming card:" + Name);
		if (Application.loadedLevelName == MainMenu.SceneNameEditDeck) {
						
						Player.CanUnzoom = false;
						StartCoroutine(WaitBeforeUnZoom());
						BoxCollider2D collider = GetComponent<BoxCollider2D> () as BoxCollider2D;
						collider.size = EditDeckScripts.FirstCardColliderSize;
						collider.center = new Vector2 (0, 0);
						foreach (Transform child in transform)
								child.gameObject.layer = 8;  //the offense/defense text 
						gameObject.layer = 8; //zoomedcards layer that appears on top of gui in editdeck mode
				} else { 		// game scene

		
			BoxCollider2D thiscollider = GetComponent<BoxCollider2D> () as BoxCollider2D;
			if (transform.parent !=null) Zoom = ZoomHeight / (thiscollider.size.y * transform.localScale.y * transform.parent.localScale.y);
				else Zoom = ZoomHeight / (thiscollider.size.y * transform.localScale.y); //all cards should be the same size when zoomed, no matter their slot/zone size
			Debug.Log ("zoomheight:"+ZoomHeight +"collider height:"+(thiscollider.size.y * transform.localScale.y));

			if (Player.cards_in_game.Contains (this)) {
								if (Type == 4) RevealSecretCard(); //secret 

								foreach (card foundcard in Player.cards_in_game)
										foundcard.collider2D.enabled = false;
									}
				
			else if (Player.cards_in_hand.Contains(this)) foreach (card foundcard in Player.cards_in_hand) foundcard.collider2D.enabled = false;
			else if (Enemy.cards_in_game.Contains(this)) foreach (card foundcard in Enemy.cards_in_game) foundcard.collider2D.enabled = false;
			//GetComponent<SpriteRenderer>().sprite = playerDeck.pD.cardsImages[Index];

			//if (transform.position.y <= 0f) {IsMovedForZoom = true; old_y = transform.position.y; transform.position=new Vector3 (transform.position.x, 0f, transform.position.z);}
			if (transform.position.y <= -2.7f) {IsMovedForZoom = true; old_y = transform.position.y; transform.position=new Vector3 (transform.position.x, -2.3f, transform.position.z);}
			if (transform.position.y >= 4f) {IsMovedForZoom = true; old_y = transform.position.y; transform.position=new Vector3 (transform.position.x, 3f, transform.position.z);}
		}

			
		if (IsTurned == true && !MainMenu.TCGMaker.core.UseGrid) { 
						
			if (transform.parent)  transform.parent.Rotate(0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees); //unturning the slot
				else transform.Rotate(0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
				
			IsRotatedForZoom = true; 
		}


		Vector3 theScale = transform.localScale;
		if (Application.loadedLevelName == MainMenu.SceneNameEditDeck) {
			EditDeckScripts.Zoomed = true;
			theScale.x *= ZoomEditDeckMode; //make  bigger
			theScale.y *= ZoomEditDeckMode; //make bigger
		} else {
			theScale.x *= Zoom; //make  bigger
			theScale.y *= Zoom; //make bigger

			if (Player.cards_in_game.Contains(this)) foreach (card foundcard in Player.cards_in_game) foundcard.collider2D.enabled = true;
			else if (Player.cards_in_hand.Contains(this)) foreach (card foundcard in Player.cards_in_hand) foundcard.collider2D.enabled = true;
			else if (Enemy.cards_in_game.Contains(this)) foreach (card foundcard in Enemy.cards_in_game) foundcard.collider2D.enabled = true;
		}
		transform.localScale = theScale;


		old_sortingorder = GetComponent<SpriteRenderer> ().sortingOrder ;
	

		if (MainMenu.TCGMaker.core.OptionCardFrameIsSeparateImage) 
		{	 

			GetComponent<SpriteRenderer> ().sortingOrder = 101; 
			foreach (Transform child in transform) child.renderer.sortingOrder = 101; 
			transform.Find ("CardArt").renderer.sortingOrder = 100; //make zoomed card appear on top of all the cards
			if (highlight) highlight.renderer.sortingOrder = 100; //make zoomed card appear on top of all the cards

		}
		else 
		{
			GetComponent<SpriteRenderer> ().sortingOrder = 100; //make zoomed card appear on top of all the cards
			foreach (Transform child in transform) child.renderer.sortingOrder = 101; //the card's icons should still be on top of it
		}
		
		IsZoomed = true;
		
	}

	public IEnumerator WaitBeforeUnZoom()
	{
		yield return new WaitForSeconds (0.1f);
		Player.CanUnzoom = true;
	}

	public void UnZoom()
	{
		//while (Player.CanUnzoom==false) yield return new WaitForSeconds (0.2f);
		if (Player.CanUnzoom) {
		
			Debug.Log("unzooming card:" + Name);

			if (Player.cards_in_game.Contains (this) && Type == 4) HideSecretCard(); //secret 

			gameObject.layer = 0; //default layer
			foreach (Transform child in transform)	child.gameObject.layer = 0;  //the offense/defense text 
			if (IsRotatedForZoom == true) {
				if (transform.parent && !MainMenu.TCGMaker.core.UseGrid)  transform.parent.Rotate(0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees); 
				else transform.Rotate(0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees);
			}
			IsRotatedForZoom = false;
			
			if (IsMovedForZoom == true) {
				transform.position = new Vector3 (transform.position.x, old_y, transform.position.z);
			}
			IsMovedForZoom = false;
			
			Vector3 theScale = transform.localScale;
			if (Application.loadedLevelName == MainMenu.SceneNameEditDeck) {
				BoxCollider2D collider = GetComponent<BoxCollider2D> () as BoxCollider2D;
				collider.size = EditDeckScripts.OtherCardsColliderSize; // its smaller than the card itself, because the cards are covering each other
				collider.center = EditDeckScripts.OtherCardsColliderCenter;
				EditDeckScripts.Zoomed = false;
				theScale.x /= ZoomEditDeckMode; 
				theScale.y /= ZoomEditDeckMode; 
			} else {
				Debug.Log("scaling back after zoom");
				theScale.x /= Zoom; 
				theScale.y /= Zoom; 
				
			}

			if (MainMenu.TCGMaker.core.OptionCardFrameIsSeparateImage) 
				{	 
					
					foreach (Transform child in transform) child.renderer.sortingOrder = old_sortingorder + 1; //the card's icons should still be on top of it
					GetComponent<SpriteRenderer> ().sortingOrder = old_sortingorder + 1;
					transform.Find ("CardArt").renderer.sortingOrder = old_sortingorder; 
				if (highlight) highlight.renderer.sortingOrder = old_sortingorder; 
				}
			else 
				{
					GetComponent<SpriteRenderer> ().sortingOrder = old_sortingorder;
					foreach (Transform child in transform) child.renderer.sortingOrder = old_sortingorder + 1; //the card's icons should still be on top of it
				}

			transform.localScale = theScale;


			IsZoomed = false;
			ShowedByEnemy = false;
			mouseHoverSeconds = 0; // reset mouse hover seconds
			//yield return new WaitForSeconds (0.1f);
		} 
		//else {
		//	yield return 0f;
		//	Debug.Log ("can't unzoom yet");
		//}
	}

	public bool IsACreature() //utility
	{
		if (Type == 1) return true;
		else return false;
	}


	public bool IsACreatureOrHeroInGame()
	{
		if (Enemy.enemy_creatures.Contains(this) || Player.player_creatures.Contains (this)) return true;
		else return false;
	}

	public bool IsACreatureInGame()
	{
		if (!Hero && (Enemy.enemy_creatures.Contains(this) || Player.player_creatures.Contains (this))) return true;
		    else return false;
	}

	void BadTarget()
	{
		Debug.Log ("bad target");
		Player.Warning = "This is not a valid target for this spell";
	}

	public void OnMouseDown() {
		if (CustomStrings.ContainsKey("flavor text"))
		Debug.Log ("flavor text: "+CustomStrings["flavor text"]);

		if (Application.loadedLevelName == MainMenu.SceneNameEditDeck)  //edit deck mode
						EditDeckScripts.MoveCard (this);
	
		else if (Application.loadedLevelName == MainMenu.SceneNameMainMenu) //it's main menu, player wants to buy a promo card
		{
			if (CostInCurrency <= Currency.PlayerCurrency) //if the player can afford this card
			{

			Currency.DoBuyCard(Index);
			Currency.GetCurrency(); //updating the display of player's balance
			Debug.Log("bought a card!");
			MainMenu.message = "Bought a card!";
			MainMenu.CollectionNeedsUpdate = true;
			}
			else { Debug.Log("can't afford"); MainMenu.message = "You don't have enough silver"; }  
		}
		else //		GAME MODE
		{
		
		
			Debug.Log("card mousedown, needtarget: "+Player.NeedTarget );
			switch (Player.NeedTarget) { 
			case 1: 	//needs a target for creature attack
			
					if (Enemy.enemy_creatures.Contains(this)) // if it's an enemy creature 
					    {
							if (MainMenu.TCGMaker.core.UseGrid) //using grid
							{
								Slot attacker_slot = Player.AttackerCreature.slot;
								
							if (attacker_slot.IsAdjacent(slot) || (Player.AttackerCreature.Ranged && attacker_slot.IsInALine(slot).Count > 0) ) AssignTarget();
										else Player.Warning = "This is not a valid target for attacking"; 
							}
					   
						else AssignTarget(); 
					}	
					else 	//not an enemy creature
					{
						Debug.Log ("bad target");
						Player.Warning = "This is not a valid target for attacking";
					}


			break;

		
			case 2: //target enemy player or creature (enemy player gets handled in Enemy.cs)
				if (Enemy.enemy_creatures.Contains(this) ) AssignTarget(); // if it's an enemy creature 

				else BadTarget();
			break;

				//needs a target from opened deck/graveyard: (see effects help.txt)
			case 3:
				if (Player.temp_cards.Contains(this)) { // if it's a card from opened deck
					AssignTarget(gameObject);
					
					Player.temp_cards.Remove(this);
					
					playerDeck.pD.Deck.Remove(Index);

					Destroy(GameObject.Find("ChooseCardText"));
					
					foreach (card foundcard in Player.temp_cards) Destroy(foundcard.gameObject);	 
				
				}
				else BadTarget();
				break;
			case 50:
			case 51:
				if (Player.temp_cards.Contains(this)) { // if it's a card from opened graveyard
					AssignTarget (Player.cards_in_graveyard.Find(x => x.Index == Index).gameObject);

					Destroy(GameObject.Find("ChooseCardText"));
				
					foreach (card foundcard in Player.temp_cards) Destroy(foundcard.gameObject);	 
				
				}
				else BadTarget();
				break;
	

			case 4: //two creatures in game
				if ( IsACreatureInGame() ) { 

					if (Player.targets.Count>0)	{
						if (Player.targets[0] != gameObject) AssignTargets(2);
						else BadTarget();
					}

					else AssignTargets(2);
				}
				else BadTarget();
			break;
			
			case 5: //needs any target creature ( not a hero)
				if (IsACreatureInGame() && !Hero) 		AssignTarget();

				else BadTarget();
			break;
			
			case 6: //needs any target creature that has attacked this turn
				if (IsACreatureInGame() && this.AttackedThisTurn > 0) AssignTarget();
						
					else BadTarget();

			break;
			case 40: //needs a target ally
				if (Player.player_creatures.Contains(this) && IsACreatureOrHeroInGame()) AssignTarget();
				
				else BadTarget();
				
				break;
			case 41: //needs a target enemy creature or hero
				if (Enemy.enemy_creatures.Contains(this) && IsACreatureOrHeroInGame()) AssignTarget();
				
				else BadTarget();
				
				break;
			case 99:  //needs a player's creature to upgrade
				if (IsACreature() && Level == Player.CurrentTargetParam && GrowID == Player.CurrentTargetParamString && Player.player_creatures.Contains (this)) AssignTarget();
				else Player.Warning = "You need a level "+Player.CurrentTargetParam+" "+ Player.CurrentTargetParamString +" target for this upgrade"; //you need a level 1 Orc target for this upgrade
				
				
				break;
			case 7:  //needs a player's creature of certain type (for now only hero)
				if (Hero && Player.player_creatures.Contains (this)) AssignTarget();
					else Player.Warning = "You need a hero target for this spell";
					

			break;
			
			case 8:
			case 9:
					//needs a player creature, needtarget=8 can also take player as a target but this gets handled by Player.OnMouseDown
				if (IsACreature() && Player.player_creatures.Contains (this)) AssignTarget();
					else BadTarget();
			break;		
			
			case 21:
				//needs a card in hand to discard
				if (Player.cards_in_hand.Contains (this)) AssignTarget();
				else BadTarget();
			break;		
			case 30:
				//needs a creature with attack <= param 
				if (IsACreatureInGame() && CreatureOffense <= Player.CurrentTargetParam) AssignTarget();
				else BadTarget();
			break;
			case 31:
				//needs a creature with cost <= param 
				if (IsACreatureInGame() && Cost.Count <= Player.CurrentTargetParam) AssignTarget();
				else BadTarget();
				break;	
			default:
			{
				if (Player.NeedTarget >0 ) return;	// don't start playing a card or attacking if we are are waiting to the player to choose some target
				
				if (Player.PlayersTurn == false) {	Debug.Log ("not your turn!");	return; } //do nothing if it's not player's turn
				
				if ((ControlledByPlayer == true)&&(Player.GameEnded==false)) 
				{
					if (IsZoomed == true) { UnZoom(); }

					if (IsACreatureOrHeroInGame() && MainMenu.TCGMaker.core.UseGrid && !abilities.DisplayMenu)
					{
						if (IsTurned)Player.Warning = "This creature cannot move because it has commited this turn";
							else if (MovedThisTurn>1) Player.Warning = "This creature can't move any more this turn";
								else if (MovedThisTurn>0 && !extramovement) Player.Warning = "This creature has already moved this turn";
									else if (FirstTurnSickness()) Player.Warning = "A creature cannot move on its first turn";
										else  
											{	
												
												Player.CreaturesZone.StartCoroutine("ChooseSlotAndPlace", this);
												
											}

					}
					else PlayCard ();
				}

			}
			break;
			}



		} // if game mode
		
	}



	void AssignTarget(GameObject targetgameobject = null)
	{
		if (targetgameobject == null)
						targetgameobject = this.gameObject;
		Player.targets.Add(targetgameobject);
		Debug.Log ("First target of player: " + Player.targets [0]);
		Player.NeedTarget = 0;

	}

	void AssignTargets(int targets_needed)	//if need two+ targets
	{

			Player.targets.Add(gameObject);
		
		if (Player.targets.Count == targets_needed) Player.NeedTarget = 0;
		
	}

	public void Turn()
	{
		if (!MainMenu.TCGMaker.core.UseGrid) {
			if (transform.parent)
				transform.parent.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees);
			else
				transform.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees); 
		}

		IsTurned = true;
		checked_for_highlight = false;
	}

	public void UnTurn()
	{
		if (!MainMenu.TCGMaker.core.UseGrid) {
			if (transform.parent)
				transform.parent.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
			else
				transform.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees); 
		}
		IsTurned = false;
	}

	void SendCard()
	{
		Logic.ScenePhotonView.RPC("SendPlayedCard", PhotonTargets.Others, id_ingame); //sending played card to the other player
	}


	void SendEffect(int effect_number)
	{
		Logic.ScenePhotonView.RPC("SendEffect", PhotonTargets.Others, effect_number, id_ingame); //sending played card to the other player
	}

	public void SendHandCard()
	{
		Debug.Log ("sending played hand card:" + Name);
		Logic.ScenePhotonView.RPC("SendPlayedHandCard", PhotonTargets.Others, Index, id_ingame); //sending played card to the other player
	}

	public void SendUpgradeCreature()
	{
		Debug.Log ("sending creature upgrade:" + Name);
		Logic.ScenePhotonView.RPC("SendUpgradeCreature", PhotonTargets.Others, Index); //sending played card to the other player
	}

	public void PlayEnemyCardMultiplayer()
	{

		if (Type == 0) 	TurnKeeperForMana(true); //if it's a keeper

		else if (IsACreature ()) CreatureAttack(true);

	}

	public void PlayEnemyHandCardMultiplayer()
	{
		
				if (Type == 0)
						FromHandKeeper (false);
				else if (Type == 1)
						StartCoroutine(FromHandCreature (true));
				else if (Type == 2)
						FromHandSpell (true);
				else if (Type == 3 || Type == 4)
						StartCoroutine(FromHandEnchantment (true));
		
	}

	public void TurnKeeperForMana(bool AI=false)
	{
		Debug.Log("tapping keeper for mana:"+CardColor.name);
		Turn();
		ManaColor mana_to_gain;

		if (MainMenu.TCGMaker.core.UseManaColors)
			mana_to_gain = CardColor;
		else
			mana_to_gain = MainMenu.TCGMaker.core.colors [0]; //colorless

		if (AI)	Enemy.mana.Add(mana_to_gain);
			else 
		{
			Player.mana.Add(mana_to_gain);
			if (MainMenu.IsMulti) SendCard(); //sending played card to the other player
		}


	}

	bool CanPayCosts(bool potential_mana=false)
	{
		if (CanPayDiscardCost ())
		{
			if (potential_mana && CanPayManaCost (potential_mana)) return true; //if the player could get the required mana by tapping keepers
			if (CanPayManaCost()) return true; //if the player already has the required mana in their mana pool

		}
						
		return false;
	}

	bool CanPayDiscardCost()
	{
		if (DiscardCost <= (Player.cards_in_hand.Count + 1))
						return true;
		return false;
	}

	bool CanPayManaCost(bool potential_mana=false)
	{

		if (MainMenu.TCGMaker.core.UseManaColors)
				{
				List<ManaColor> temp = new List<ManaColor>();
				
				if (potential_mana) {
									foreach (card foundkeeper in Player.keepers_in_game) 
										if (!foundkeeper.IsTurned)	temp.Add(foundkeeper.CardColor);
									}
				foreach (ManaColor foundmana in Player.mana) 
					temp.Add(foundmana);


				int need_colorless = 0;
				foreach (ManaColor foundcolor in Cost)
				{
					if (foundcolor.name != "colorless")
					{
						ManaColor can_pay = temp.Where(x => x.name == foundcolor.name).FirstOrDefault();
						if (can_pay != null)	temp.Remove(can_pay);
						else return false;
					}
					else need_colorless++;
				}

				if (need_colorless <= temp.Count)return true;

				}
		else if (potential_mana)
			{
				int unturned_keepers = 0;
				foreach (card foundkeeper in Player.keepers_in_game) 
					if (!foundkeeper.IsTurned) unturned_keepers++;

				if (Cost.Count <= (Player.mana.Count + unturned_keepers)) return true;
			}

		else if (Cost.Count <= Player.mana.Count) return true;

		return false;
		
	}



	public void PlayCard()
	{	

		Debug.Log ("trying to play: "+Name );


	if (Type == 0) {//if the card is a keeper 
		if (Player.cards_in_hand.Contains(this)) { // and is in the player's hand, move it to keepers zone if we can
			
			
			if (Player.KeepersPlayedThisTurn > 0) 
					Player.Warning = "You've already played a keeper this turn!";
			
				else if (!Player.KeepersZone.CanPlace())
					Player.Warning = "You don't have enough slots to place a keeper!";
			else {
					FromHandKeeper ();
					if (MainMenu.IsMulti)  SendHandCard(); //sending played card to the other player
				} 	
		}
		
			else if (Player.keepers_in_game.Contains(this) && !IsTurned) //if the keeper is in the keepers zone and not tapped, tap it for mana
				TurnKeeperForMana();
		
	}
	
		else if (Player.cards_in_hand.Contains(this)) { //if the card is in player's hand and is not a keeper
		Debug.Log("Cost"+Cost);
		if (CanPayManaCost() && CanPayDiscardCost()) { // if the player can pay the card's cost
				 
				if (Type == 3  && !Player.CreaturesZone.CanPlace())
					Player.Warning = "You don't have enough slots to place an enchantment!";
				else if (Type == 4  && !Player.CreaturesZone.CanPlace())
					Player.Warning = "You don't have enough slots to place a secret!";
				else StartCoroutine(PayAdditionalCostAndPlay());
		}
			else Player.Warning = "You don't have enough mana";
	}
		// the card is a creature in game
		else if (IsACreatureOrHeroInGame())
			{ 
				TryToAttack();
			}
}

	public bool FirstTurnSickness()
	{
		if (MainMenu.TCGMaker.core.OptionFirstTurnSickness)
			if (FirstTurnInGame && !no_first_turn_sickness) return true;
		return false;
	}

	public void TryToAttack()
	{
		if (!IsTurned && !FirstTurnSickness() && !cant_attack) CreatureAttack();
				else if (cant_attack) Player.Warning = "This creature cannot attack";
					else if (FirstTurnSickness()) Player.Warning = "A creature can't attack the first turn it is in game";
						else if (IsTurned) Player.Warning = "A tapped creature can't attack";
	}

	bool CanAttack()
	{
			
		if (!IsTurned && !FirstTurnSickness() && !cant_attack) return true;
		return false;
	
	}

	public void Discard(bool AI = false) //discarding from hand
	{

		if (AI) Enemy.RemoveHandCard(this);
				else Player.RemoveHandCard(this);

		if (MainMenu.TCGMaker.core.OptionGraveyard)	
		{
			//if (AI)		playerDeck.pD.AddArtAndText (gameObject); //first we need to add some image to display on the card in graveyard
			MoveToGraveyard();
		}
		else 	Destroy (gameObject);
	}

	IEnumerator PayAdditionalCostAndPlay()
	{		
		if (DiscardCost > 0 && ValidSpell()) 
		{
			Player.ActionCancelled = false;
			Player.targets.Clear();
			Debug.Log("this card has an additional discard cost");
						for (int i = 0; i < DiscardCost; i++) 
								{
									Player.NeedTarget = 21; // a card in hand to discard
									
									while (Player.NeedTarget > 0) 		yield return new WaitForSeconds (0.1f);
									if (Player.ActionCancelled) { Debug.Log("action cancelled"); return false; }
								}
			foreach (GameObject target in Player.targets) //discard
					target.GetComponent<card>().Discard();

		}

		// the discard cost is paid, now play the card:

		if (IsACreature()) 	StartCoroutine(FromHandCreature()); 
			
		else if (Type == 2) FromHandSpell();  // is a spell
		
		else if (Type == 3 || Type == 4)  StartCoroutine(FromHandEnchantment());  //enchantment or secret
	

	}

	public void FaceDown()
	{
		foreach (Transform child in transform) child.renderer.enabled = false;
		GetComponent<SpriteRenderer> ().sprite = playerDeck.pD.cardback;
		faceDown = true;
	}


	public void FaceUp()
	{
		foreach (Transform child in transform)	child.renderer.enabled = true;

		Transform templateTransform = CardTemplate.Instance.transform;

		if (templateTransform.Find(CardColor.name+"Frame"))
			GetComponent<SpriteRenderer> ().sprite = templateTransform.Find(CardColor.name+"Frame").GetComponent<SpriteRenderer> ().sprite;

		faceDown = false;
	}

	public void Hide()
	{
		renderer.enabled = false;

		foreach (Transform child in transform)
			child.renderer.enabled = false;
		
	}

	public void Show()
	{
		renderer.enabled = true;
		
		foreach (Transform child in transform)
			child.renderer.enabled = true;
		
	}

	public void HideSecretCard()
	{
		transform.Find ("CardArt").GetComponent<SpriteRenderer>().sprite = playerDeck.pD.secretart;

		foreach (Transform child in transform)
			if (child.name!="CardArt") child.renderer.enabled = false;
		 
	}


	public void RevealSecretCard()
	{
		transform.Find ("CardArt").GetComponent<SpriteRenderer>().sprite = Art ;
		foreach (Transform child in transform)
						child.renderer.enabled = true;

	}

	public bool HasUpgradableCreature(List<card> creatureslist)
	{
		foreach (card foundcard in creatureslist) {
			Debug.Log("foundcard, growid:"+GrowID+"level:"+foundcard.Level);
						if (foundcard.GrowID == GrowID && foundcard.Level == Level-1)
								return true;
				}
		return false;
	}

	public void Grow(card upgrade, bool AI=false)
	{
		//Debug.Log ("growing, iszoomed:"+IsZoomed);
		Index = upgrade.Index;

		//if (IsZoomed) UnZoom();
		collider2D.enabled = false;

		foreach (Transform child in transform) Destroy(child.gameObject); //destroying additional gameobjects for art, card name, description text, etc
		playerDeck.pD.LoadCardStats(this);

		if (IsTurned)	//temporary unturn
			if (!MainMenu.TCGMaker.core.UseGrid) {
				if (transform.parent)
					transform.parent.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);
				else
					transform.Rotate (0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees); 
			}

		playerDeck.pD.AddArtAndText (this);

		if (AI)
						Enemy.RemoveHandCard (upgrade);
				else
						Player.RemoveHandCard (upgrade);


		Destroy (upgrade.gameObject);
		collider2D.enabled = true;

		if (IsTurned)	//return the turned rotation
			if (!MainMenu.TCGMaker.core.UseGrid) {
				if (transform.parent)
					transform.parent.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees);
				else
					transform.Rotate (0, 0, MainMenu.TCGMaker.core.OptionTurnDegrees); 
			}
	}

	IEnumerator WaitForTargetAndGrow()
	{
		Player.ActionCancelled = false;
		Player.targets.Clear ();
		Player.NeedTarget = 99;
		Player.CurrentTargetParam = Level-1;
		Player.CurrentTargetParamString = GrowID;
		while (Player.NeedTarget > 0)	yield return new WaitForSeconds (0.2f);

		if (!Player.ActionCancelled)
		{
			PayManaCost();

			card oldcard = Player.targets[0].GetComponent<card>();
			if (MainMenu.IsMulti) {
				Player.SendTargets();
				SendUpgradeCreature();
			}
			oldcard.Grow(this);
		}
	}

	public IEnumerator FromHandCreature(bool AI=false) {
		
		Debug.Log("playing a creature");

		if (AI == false) {	// if it's the player who is playing a creature

													
						if (GrowID!="" && Level > 0 ) // a creature upgrade
						{
							if (HasUpgradableCreature(Player.player_creatures)) 
																	
								StartCoroutine(WaitForTargetAndGrow());
														
							else Player.Warning = "You don't have a creature to upgrade with this card";
						}
						else // a regular creature
						{	
							
							Zone creaturezone = Player.CreaturesZone;
							
							Player.WaitForCheck = true;
							
							creaturezone.StartCoroutine("CheckIfCanPlaceInZone",this);				
							
							while (Player.WaitForCheck) yield return new WaitForSeconds(0.15f);			
							if (!Player.ActionCancelled) 	//if we can place a card in this zone and the player hasn't cancelled
										{	
											if (MainMenu.IsMulti)  SendHandCard(); 
											
											PayManaCost();
											Player.RemoveHandCard(this); //always remove before adding
											Player.TriggerCardAbilities(abilities.ON_ENTER_CARDSUBTYPE, Subtype);	//before adding this card so it can't trigger itself					
											Player.AddCreature(this);
																	
										}
																	
						} //regular creature end
						
				} else 
		
			{ 				//it's the enemy who is playing a creature

				
				PayManaCost(AI);
			
				Enemy.RemoveHandCard(this); //always remove before adding
				
				if (GrowID!="" && Level > 0 ) //if this is a creature upgrade
					{
						Enemy.ChooseTargetForUpgrade(this);
						card oldcard = Enemy.targets[0].GetComponent<card>();
						
						oldcard.Grow(this, AI);
					}
				else 
			

				Enemy.AddCreature(this);
			
		}

	
	}



	public void FromHandKeeper(bool ForPlayer=true) 	{ 

		Debug.Log("FromHandKeeper, ForPlayer:"+ForPlayer);
			if (ForPlayer)
	{
			Player.KeepersPlayedThisTurn +=1;

			Player.RemoveHandCard(this);

			Player.AddKeeper(this);

		

	
	}
	else //it's the enemy who plays the keeper
	{
			Debug.Log("playskeeper");
			Debug.Log("Index:"+Index);
		

			Enemy.RemoveHandCard(this); 
			Enemy.AddKeeper(this);
	
	}
	
		abilities.TriggerAbility (abilities.ON_ENTER, !ForPlayer);

	}


	IEnumerator WaitForMultiplayerTargetAndDoEffect(int effect_number)
	{
		Debug.Log ("waiting for enemy to send target ienum...");
				

		while (Enemy.NeedTarget==100)	 yield return 0.3f;  //waiting for player to choose a target
		
		//EffectManager.DoEffect (true, this, effect_number);
		EffectManager.AddToStack(true, this, effect_number);
	}


	IEnumerator WaitForTargetAndDoEffect(int Target, int TargetParam, int effect_number)
	{
		Debug.Log ("WaitForTargetAndDoEffect start");
		Player.targets.Clear (); //clear previous targets

		Player.ActionCancelled = false;
		Player.AttackerCreature = this; //for some cards like spectralist
		Player.CurrentTargetParam = TargetParam;
		Player.NeedTarget = Target; 

		if (Target == 3) Player.OpenIntListToChooseCard(playerDeck.pD.Deck); //need target:  card from deck
		else if (Target == 50) 	Player.OpenListToChooseCard (Player.cards_in_graveyard, 2);	//need target:  spell from graveyard
		else if (Target == 51) Player.OpenListToChooseCard (Player.cards_in_graveyard, 1);	//need target: creature from graveyard


		while (Player.NeedTarget>0)	 yield return 0.5f;  //waiting for player to choose a target
				


		if (!Player.ActionCancelled) {
						if (MainMenu.IsMulti) SendTargetsAndEffect(effect_number);
											
						//EffectManager.DoEffect (false, this, effect_number);
						EffectManager.AddToStack(false, this, effect_number);
									}
		else  //effect and got cancelled
		{
			Debug.Log("choosing target... spell got cancelled");
			if (Type == 2 ) {} //if it's a spell, just cancel 

			else  if (Effects[effect_number].trigger == 1) //same for activated abilities
			{}

			else if (effect_number == (Effects.Count-1)) //abilities that are triggered and can't be prevented. If it was the last effect, finish the ability
			{
				if ( Type == 4 ) SecretAfterEffects(); //secret
				else Player.SpellInProcess = false; 
			}
		}


		}


	public void SendTargetsAndEffect(int effect_number)
	{

			Debug.Log ("sending targets and effect, card: "+ Name);
			Player.SendTargets ();

		if (IsACreature())	
			{
				if (Effects[effect_number].trigger == 1 )	// if this was an activated ability. Other abilities are handled on multiplayer opponent's side automatically
					 SendEffect(effect_number); 
			}

			else if (Type!=3 && Type!=4) SendHandCard (); // send info about this play to the opponent, secrets and enchantments are handles automatically
			

	}

	bool ChooseAutomaticTargetsAndDoEffect(int effect_number, bool AI=false) //don't change from effect_number (to Effect for ex.) because of multiplayer
	{

		bool TargetIsAutomatic = true;
		Effect effect = Effects[effect_number];
		Debug.Log("trying to choose automatic target, effect target:"+effect.target+"AI:"+AI);

		if (NoTargetEffects.Contains(effect.type))
			 {
			Debug.Log("no target effect");
				if (MainMenu.IsMulti){
					if (IsACreature() && effect.trigger != 0)
						SendEffect (effect_number);	//a creature's ability but not its activated ability
				
					else if (Type == 2)	SendHandCard (); // send info about this play to the opponent
				}
			}
			

		else switch (effect.target) { 
				
					
				case 12: 	// all friendly creatures
						if (AI)
								Enemy.targets = Enemy.Creatures ();
						else
								Player.targets = Player.Creatures ();
				
						break;
				case 13:	//all creatures
						if (AI) Enemy.targets = EffectManager.CreaturesInGame();
								
						else 	Player.targets = EffectManager.CreaturesInGame();
								break;
				case 14:	//all enemy creatures
						if (AI) Enemy.targets = Player.Creatures ();
					
					else 	Player.targets = Enemy.Creatures ();
					break;
				case 16:	//all enemy creatures or heroes
					if (AI) 
							{
								Enemy.targets.Clear();
								foreach (card foundcard in Player.player_creatures) Enemy.targets.Add(foundcard.gameObject);
							}		
					else 	foreach (card foundcard in Enemy.enemy_creatures) Player.targets.Add(foundcard.gameObject);
					break;
				case 200:	//random enemy creature
						if (AI)
							{	Enemy.targets.Clear();
								Enemy.targets.Add(Player.RandomCreature());
							}
							
						else 
								Player.targets.Add(Enemy.RandomCreature());
							
						
						break;
				case 201:	//param0-param1 random enemy creatures
						int number_from = effect.targetparam0; 
						int number_to = effect.targetparam1; 
						int number_of_creatures = Random.Range (number_from, number_to + 1); //+1 because upper int is not included
			
						if (AI)
								Enemy.targets = EffectManager.RandomCreatures (number_of_creatures, Player.player_creatures);
						else
								Player.targets = EffectManager.RandomCreatures (number_of_creatures, Enemy.enemy_creatures);
				
						break;
				case 202: //random enemy creature with cost < x
						if (AI) 				
							{	Enemy.targets.Clear();
								Enemy.targets.Add(Player.RandomCreatureWithCostEqualOrLowerThan(effect.targetparam0));
							}
								
						else	Player.targets.Add(Enemy.RandomCreatureWithCostEqualOrLowerThan(effect.targetparam0));
								
						break;
				case 203: //random enemy creature or hero
					if (AI) 				
					{	Enemy.targets.Clear();
						Enemy.targets.Add(Player.RandomAlly());
					}
					
					else	Player.targets.Add(Enemy.RandomAlly());
					
					break;
				case 230: //random ally
					if (AI){
								Enemy.targets.Clear();		
								Enemy.targets.Add(Enemy.RandomAlly());
							}
							
						else	Player.targets.Add(Player.RandomAlly());
							
						break;
				case 261: //random X creatures in game (no heroes)
					if (AI)
						Enemy.targets = EffectManager.RandomGameObjects(effect.targetparam0, EffectManager.CreaturesInGame());
					else
						Player.targets = EffectManager.RandomGameObjects(effect.targetparam0, EffectManager.CreaturesInGame());
					break;
				case 300: //random creature in hand
					if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_hand, 1)); //second param is type
							}
					else
						Player.targets.Add(EffectManager.RandomCard(Player.cards_in_hand, 1));
					break;
				case 301: //random creature in deck
			if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCardFromIntList(Enemy.Deck));
					}
					else
						Player.targets.Add(EffectManager.RandomCardFromIntList(playerDeck.pD.Deck));
					break;
				case 302: //random creature from graveyard
				if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_graveyard, 1)); //second param is type
						}
					else
						Player.targets.Add(EffectManager.RandomCard(Player.cards_in_graveyard, 1));
					break;
				case 303: //random spell from graveyard
					if (AI){
						Enemy.targets.Clear();
						Enemy.targets.Add(EffectManager.RandomCard(Enemy.cards_in_graveyard, 2)); //second param is type
					}
					else
						Player.targets.Add(EffectManager.RandomCard(Player.cards_in_graveyard, 2));
					break;
				case 304: //random creature in enemy deck
						if (AI){
						Enemy.targets.Clear();	
						Enemy.targets.Add(EffectManager.RandomCardFromIntList(playerDeck.pD.Deck));
						}
					else
						Player.targets.Add(EffectManager.RandomCardFromIntList(Enemy.Deck));
					break;
				case 10: //  current player
		
						if (AI)
								{
									Enemy.targets.Clear();
									Enemy.targets.Add (GameObject.FindWithTag ("Enemy"));
								}
						else
								
									Player.targets.Add (GameObject.FindWithTag ("Player"));
								
				
						break;
				case 11: //the target is the enemy
						if (AI)
						{
							Enemy.targets.Clear();
							Enemy.targets.Add (GameObject.FindWithTag ("Player"));
						}
						else
						
							Player.targets.Add (GameObject.FindWithTag ("Enemy"));
						
						
						break;
				case 15: //this creature (for creature abilities)

						if (AI)
								{
									Enemy.targets.Clear();
									Enemy.targets.Add (gameObject);
								}
						else				
									Player.targets.Add (gameObject);
								
								break;
				case 60: //top card from deck
					
					if (AI)
					{
						Enemy.targets.Clear();
						Enemy.targets.Add (playerDeck.pD.MakeCard(Enemy.Deck[0]).gameObject);
						Enemy.Deck.RemoveAt(0);
					}
					else
					{
						Player.targets.Clear();
						Player.targets.Add (playerDeck.pD.MakeCard(playerDeck.pD.Deck[0]).gameObject);
						playerDeck.pD.Deck.RemoveAt(0);
					}
					break;
	
				default:
						TargetIsAutomatic = false;
						break;
				}
		
	if (TargetIsAutomatic)
			{
				//if (!AI && MainMenu.IsMulti && RandomTargetsList.Contains(Target)) Player.SendTargets(); 
				if (!AI && MainMenu.IsMulti) SendTargetsAndEffect(effect_number); //if not an enchantment
				//EffectManager.DoEffect (AI, this, effect_number);
				EffectManager.AddToStack(AI, this, effect_number);
				return true;
			}
	return false;
	
	}


	public void ApplyEffect(int effect_number, bool AI=false)
	{
		Player.targets.Clear();
		if (!MainMenu.IsMulti) Enemy.targets.Clear();
		Debug.Log ("effect number: " + effect_number);	

		int Target = 0;
		int TargetParam = 0;

		Target = Effects[effect_number].target;
		TargetParam = Effects[effect_number].targetparam0;

		int effect = Effects[effect_number].type;
		Debug.Log ("EnemyNeedTarget:" + Enemy.NeedTarget);

		if (MainMenu.IsMulti && Enemy.NeedTarget == 100) StartCoroutine(WaitForMultiplayerTargetAndDoEffect (effect_number)); //for creature entry abilities only

		else if (ChooseAutomaticTargetsAndDoEffect(effect_number, AI) == false) { //if the target needs to be chosen
					
				if (AI) {
							Debug.Log("Effect Target:" + Target);
								
							 if (!MainMenu.IsMulti) {	//AI enemy, it needs to choose a target
										Enemy.NeedTarget = Target;
										Enemy.CurrentTargetParam = TargetParam;
										Enemy.ChooseTarget (effect);
													}		
							 EffectManager.AddToStack(AI, this, effect_number);
						} 
	
			else StartCoroutine (WaitForTargetAndDoEffect (Target, TargetParam, effect_number));	//player needs to choose a target

			}

	}
	

	public void SecretAfterEffects(bool AI=false) //secrets can have only 1 effect for now
	{
		//while (SpellUnresolved) {
		//	yield return new WaitForSeconds (0.5f);
		//}

		RevealSecretCard ();

		if (AI)		Enemy.RemoveEnchantment(this);
		else 	Player.RemoveEnchantment(this);

		Debug.Log ("after effects");
		
		if (MainMenu.TCGMaker.core.OptionGraveyard) MoveToGraveyard();
		else Destroy (gameObject);
		
		Player.SpellInProcess = false;
		
		
	}
	
	public void SpellAfterEffects(bool AI=false)
	{
		Debug.Log ("starting spell aftereffects");
		//while (SpellUnresolved) {
		//	yield return new WaitForSeconds (0.5f);
		//}

			if (AI) {
						if (IsZoomed)
								UnZoom ();
					

						Enemy.TriggerCardAbilities(abilities.ON_SPELL);
						Player.TriggerCardAbilities(abilities.ON_OPPONENT_SPELL);
					} 
			else {

			PayManaCost();
			Player.RemoveHandCard(this);

			Player.TriggerCardAbilities(abilities.ON_SPELL);
			Enemy.TriggerCardAbilities(abilities.ON_OPPONENT_SPELL);

				}
	
			
			
		if (MainMenu.TCGMaker.core.OptionGraveyard) MoveToGraveyard();
			else Destroy (gameObject);

			Player.SpellInProcess = false;


	}


	bool ValidSpell(bool warning = true)	//validating abilities that can't be cancelled, mostly for random abilities 
	{


		int target;
		foreach (Effect foundeffect in Effects)
			 {
				target = foundeffect.target;
				//Debug.Log ("doing ValidSpell, target:"+target);

				if (target == 7 && Effects.Count == 1) { //if target is a creature of certain type (for now only hero type)

								if (!Player.HasAHero ()) {
										if (warning) Player.Warning = "This card needs a hero for its target";
										return false;
								}
					}
			else if (target == 203 || target == 16) //random enemy creature or hero
			{
				if (!Enemy.HasACreature() && !Enemy.HasAHero()){
					if (warning) Player.Warning = "There are no enemies to target with this spell";
					return false;
				}
			}
			else if (target == 202) { //random enemy creature with cost <= x
							
				if (!Enemy.RandomCreatureWithCostEqualOrLowerThan(foundeffect.targetparam0)) {
					if (warning) Player.Warning = "There are no creatures to target with this spell";
					return false;
				}
			}
				else if (target == 200 || target == 201 || target == 14 ){ //random enemy creature (no heroes)
				
								if (!Enemy.HasACreature ()) {
										if (warning) Player.Warning = "There are no enemy creatures to target with this spell";
										return false;
									}
					}
				else if (target == 230) { //random ally
								if (!Player.HasACreature () && !Player.HasAHero()) {
									if (warning) Player.Warning = "There are no allies to target with this spell";
									return false;
								}
				}
				else if (target == 261) //random x creatures
				{
					if (EffectManager.CreaturesInGame().Count < foundeffect.targetparam0) {
					if (warning) Player.Warning = "There are not enough creatures to target with this spell";
						return false;
					}
				}
				else if (target == 13) { //from all creatures
								if (!Player.HasACreature () && !Enemy.HasACreature()) {
								if (warning) Player.Warning = "There are no creatures to target with this spell";
									return false;
								}
				}
				else if (target == 302) {//random creature from graveyard
							if (EffectManager.RandomCard(Player.cards_in_graveyard, 1) == null) {
								if (warning) Player.Warning = "You have no creatures in the graveyard to target";
								return false;
							}
				}
				else if (target == 303) //random spell from graveyard
				if (EffectManager.RandomCard(Player.cards_in_graveyard, 2) == null) {
					if (warning) Player.Warning = "You have no spells in the graveyard to target";
					return false;
				} 
						
				}
		return true;
	}

	public void PayManaCost (bool AI=false)
	{
		Debug.Log("paying mana cost for card: "+Name+" AI:"+AI);

		int need_colorless = 0;

		List<ManaColor> list_to_use;

		if (AI) list_to_use = Enemy.mana;
			else list_to_use = Player.mana;

		foreach (ManaColor foundcolor in Cost)
		if (foundcolor.name!="colorless"){
			 list_to_use.Remove(list_to_use.Where(x => x.name == foundcolor.name).First());
				
		}
		else need_colorless++;

		if (AI) while (need_colorless > 0)
		{
			list_to_use.RemoveAt(0);
			need_colorless--;
		}
		else
		{
			foreach (card foundcard in Player.cards_in_hand)
				foundcard.checked_for_highlight = false;
		}

	}

	public IEnumerator FromHandEnchantment(bool AI=false)	
	{

		if (Type == 4) 	HideSecretCard (); //secret


		if (AI) {
			PayManaCost(AI);
			Enemy.RemoveHandCard(this);
			Enemy.AddEnchantment(this);

				} 
		else { //player

			Zone enchzone = Player.CreaturesZone;
			
			Player.WaitForCheck = true;
			
			enchzone.StartCoroutine("CheckIfCanPlaceInZone",this);				
			
			while (Player.WaitForCheck) yield return new WaitForSeconds(0.15f);			
			if (!Player.ActionCancelled) 	//if we can place a card in this zone and the player hasn't cancelled
			{	
				if (MainMenu.IsMulti)  SendHandCard(); 
				PayManaCost();
				Player.RemoveHandCard(this);
				Player.AddEnchantment(this);

			}
				}

		abilities.TriggerAbility (abilities.ON_ENTER, AI);

	}

	public void FromHandSpell(bool AI=false)	{
		Player.ActionCancelled = false;
	
		if (AI == true) {	//enemy 

			PayManaCost(AI);
			Enemy.cards_in_hand.Remove (this);
			Enemy.CardsInHand -= 1;
			//SpellUnresolved = true;

			StartCoroutine(ShowCardAndWait(true));
				}
		else {			// player


				if (ValidSpell()) 
				{
					//SpellUnresolved = true;
					if (Effects.Count == 0) SpellAfterEffects(); //if the spell has no effects for some reason
						else {
								for(int i = 0; i< Effects.Count; i++) ApplyEffect(i, AI);
									
								Player.CanDoStack = true;
								Debug.Log("Player.CanDoStack:"+Player.CanDoStack);
							}
				}
				 
			else { 
					Debug.Log ("can't cast this spell");
					Player.ActionCancelled = true; 
				}
			}
				
		
	}

	IEnumerator ShowCardAndWait(bool AI)
	{	
		Debug.Log("starting ShowCardAndWait");

		Player.SpellInProcess = true;

		transform.position = new Vector3 (0f, 0f, 0f); //show the card to the player
	
		if (faceDown)	FaceUp ();
		if (!renderer) playerDeck.pD.AddArtAndText (this); 
		Debug.Log ("art scale:" + transform.Find ("CardArt").localScale.y);
		ZoomCard ();
		ShowedByEnemy = true;
	
		yield return new WaitForSeconds (1.3f);

		if (Effects.Count == 0) SpellAfterEffects (AI);	//if the spell has no effect for some reason
						
			else {
					for (int i = 0; i< Effects.Count; i++) ApplyEffect (i, AI);
				
					Player.CanDoStack = true;
				}

	}


	IEnumerator AttackTarget()
	{

		while (Player.NeedTarget>0) yield return 0.5f; //waiting for player to choose a target
		if (MainMenu.IsMulti && !Player.ActionCancelled) { //sending info about this play to the enemy
			Player.SendTargets();
			SendCard();

		} 

		if(!Player.ActionCancelled) {

			Player.targets[0].SendMessage("IsAttacked", this);
			abilities.TriggerAbility (abilities.ON_ATTACK); //should be after sendmessage

			if (AttackedThisTurn>0 || !free_attack) Turn();
			AttackedThisTurn++;

		}
	}



	public void CreatureAttack(bool AI=false)
	{
				Debug.Log ("gonna attack");
				Player.ActionCancelled = false;
				GameObject ourtarget;
				if (AI == false) {

						Player.targets.Clear();
						Player.NeedTarget = 1;
						Player.AttackerCreature = this;
						StartCoroutine (AttackTarget ());
						
					
				} else {		//enemy
					
						if (MainMenu.IsMulti || MainMenu.TCGMaker.core.UseGrid) { 

								Debug.Log ("enemy is attacking with " + Name + ", target:" + Enemy.targets[0].name);
								ourtarget = Enemy.targets[0];
											}	 
							else
								{												//AI
								Debug.Log ("AI is attacking");
								
								ourtarget = Enemy.ChooseTargetForAttacking (); 
								}
				
						
						ourtarget.SendMessage ("IsAttacked", this);
						abilities.TriggerAbility (abilities.ON_ATTACK, true); //should be after sendmessage

						if (AttackedThisTurn>0 || !free_attack) Turn();
						AttackedThisTurn++;
						Enemy.targets.Clear ();
				}


		}

	 public bool IsCriticalStrike()
	{
		if (CritChance > 0)
			{	Debug.Log ("CritChance>0" + CritChance);
				float rnd =  (Random.Range(1,100));
			Debug.Log ("rnd:" + rnd);
				if (rnd < CritChance) {Debug.Log ("critical strike!"); return true; }
		}
		return false;
	}

	bool noDamage(card attacker, card target)
	{
		if (attacker.deals_no_combat_dmg || target.takes_no_combat_dmg || (attacker.Ranged && target.no_dmg_from_ranged)) return true;

		return false;
	}

	public void PlayFX(Transform particle)
	{
		Transform newobj = (Transform)Instantiate(particle, transform.position, transform.rotation); 
		newobj.renderer.sortingOrder = 99; //particle should be on top of the card
	}

	public void IsAttacked (card Attacker) //invokes when this card (probably a creature) is attacked
	{
		if (Player.player_creatures.Contains(this)) Player.TriggerCardAbilities(abilities.ON_FRIENDLY_ISATTACKED);
			else Enemy.TriggerCardAbilities(abilities.ON_FRIENDLY_ISATTACKED);

	
		Debug.Log ("a creature "+Name+" is attacked, processing damage..");
		if (Attacker.Ranged) {	
								PlayFX(playerDeck.pD.firefx);
								audio.PlayOneShot(HitBySpell);
							}
			else 	audio.PlayOneShot (Hit);
				
		renderer.material.color = Color.red;
		Invoke ("RestoreColor", 0.3f); //we make the avatar red for 0.3 seconda


		int DamageToCreature; 

		if (noDamage(Attacker, this)) DamageToCreature = 0;
			else 
		{
			DamageToCreature = Attacker.CreatureOffense;

			if (Attacker.IsCriticalStrike ()) {
							Player.Warning = "Critical strike!"; //you could change it to some fx
							DamageToCreature = (int)(Attacker.CreatureOffense * Attacker.CritDamageMultiplier);
											}

			if (Attacker.DoubleDamage) DamageToCreature = DamageToCreature * 2;

			if (TakesHalfDamage) 	DamageToCreature = (int)(DamageToCreature / 2);

			if (Attacker.Ranged && less_dmg_from_ranged) DamageToCreature--;

			bool DoNoDamage = false;

			if (MainMenu.TCGMaker.core.OptionKillOrDoNothing) 
					{
						if (Defense >= DamageToCreature) DoNoDamage = true; 
					}

			if (DoNoDamage)	DamageToCreature = 0;
				else Defense -= DamageToCreature;

			Debug.Log("dealt "+DamageToCreature+" damage, attacker offense:"+Attacker.CreatureOffense);
		}

		if (MainMenu.TCGMaker.core.OptionRetaliate && !noDamage(this, Attacker)) {
						// the attacked creature deals damage back to the attacker:
			if (!MainMenu.TCGMaker.core.UseGrid || Ranged || slot.IsAdjacent(Attacker.slot) )
							{
								int DamageToAttacker = CreatureOffense;	//default damage

								if (IsCriticalStrike ())
												DamageToAttacker = (int)(CreatureOffense * CritDamageMultiplier);

								if (DoubleDamage)
												DamageToAttacker = DamageToAttacker * 2; 

								if (Attacker.TakesHalfDamage)
												DamageToAttacker = (int)(DamageToAttacker / 2);

								if (Ranged && Attacker.less_dmg_from_ranged) DamageToAttacker--;
										
								Attacker.Defense -= DamageToAttacker;
							}
		
				}


		Player.CreatureStatsNeedUpdating = true;

		if (Defense <=0 ) {	//the attack target died

			if (Player.player_creatures.Contains(Attacker) || Player.cards_in_graveyard.Contains(Attacker))		Attacker.abilities.TriggerAbility (abilities.ON_KILL);

			else Attacker.abilities.TriggerAbility (abilities.ON_KILL, true);
				}

		if (Attacker.Defense <=0 ) {
			if (Player.player_creatures.Contains(this) || Player.cards_in_graveyard.Contains(this))		abilities.TriggerAbility (abilities.ON_KILL);
			else abilities.TriggerAbility (abilities.ON_KILL, true);
				}
	
	}



	
	public void IsHealed (int param)
	{
		Debug.Log ("healing");

		Debug.Log ("CreatureStartingDefense: "+StartingDefense);
		Debug.Log ("param: "+param);
		if (StartingDefense < Defense + param ) Defense = StartingDefense; //we can't raise creature's health above its starting health by healing
			else Defense += param;
		 
		PlayFX(playerDeck.pD.healfx); 
		audio.PlayOneShot(Healed);
		//renderer.material.color = Color.green;
		//Invoke("RestoreColor", 0.3f); //we make our enemy's avatar change color for 0.3 seconda
		Player.CreatureStatsNeedUpdating = true;
	}

	public void IsHitBySpell (Vector3 param) //second param: 0=fire , 1=normal dmg
	{
		Debug.Log ("creature is hit by spell");
		int amount = (int)param.x;
		int damagetype = (int)param.y;
		int cardid = (int)param.z;

		if (damagetype == 0)	//fire
		{
			PlayFX(playerDeck.pD.firefx); 
			audio.PlayOneShot(HitBySpell);
		}
		if (damagetype == 1)	//physical
		{
			audio.PlayOneShot (Hit);
			renderer.material.color = Color.red;
			Invoke ("RestoreColor", 0.3f); //we make the avatar red for 0.3 seconda
		}
		if (!takes_no_spell_dmg) StartCoroutine (IsDealtSpellDamage (amount, cardid));
	}



	IEnumerator IsDealtSpellDamage (int amount, int cardid)
	{
		Debug.Log ("creature is dealt damage:"+amount);
		yield return new WaitForSeconds(0.8f);

		Defense -= amount;
		Player.CreatureStatsNeedUpdating = true;

		if (Defense <=0 && cardid != -1)
		{
			card effectcard = Logic.FindCardByID(cardid);

			if (effectcard.Type == 1) 
				{ 
					Debug.Log ("triggering OnKill on creature:" +effectcard.Name); 
					if (Player.player_creatures.Contains(effectcard) || Player.cards_in_graveyard.Contains(effectcard))		effectcard.abilities.TriggerAbility (abilities.ON_KILL);
						else effectcard.abilities.TriggerAbility (abilities.ON_KILL, true);
				}//if it was a creature ability and the target died, trigger OnKill on the creature that used the ability
		}


	}


	public void RestoreColor()
	{
		renderer.material.color = Color.white; //this actually doesn't paint the avatar white, but restores it to its original colors
	}
}
