using UnityEngine;
using System.Collections;
using System.Linq; 
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class playerDeck : MonoBehaviour   {

	public static playerDeck pD ; //instance of this singleton

	string[,] cardsInfo;

	public List<Zone> zones;

	int nexttokenid;

	[HideInInspector] 
	public List<int> Deck;
	[HideInInspector] 
	public List<int> SavedDeck = new List<int>();
	[HideInInspector] 
	public List<int> Collection;
	 
	//this setting is parsed from the database:



	[HideInInspector] 
	public  Slot[,] Grid = new Slot[7,7];
	[HideInInspector] 
	public  int first_or_second;
	[HideInInspector] 
	public bool TheSecondPlayerCanPlay=false;
	[HideInInspector] 
	public bool TheFirstPlayerCanPlay=false;

	public  AudioClip Healed;
	public  AudioClip HitBySpell;
	public  AudioClip Hit;
	public  AudioClip TakesCard;
	
	public Transform healfx;
	public Transform firefx;
	public Transform manafx;


	public TextAsset offlineDeck;
	public TextAsset offlineCollection;

	public TextAsset database;


	public  Sprite cardback;

	public  Sprite secretart;

	public  Sprite victory;
	public  Sprite defeat;


	public Material slot_regular;
	public Material slot_highlighted;
	public Material slot_mouseover;

	public Object choosecardprefab;

	public GameObject choosecard_bg;

	Transform templateTransform;

	void Awake()
		
	{

		DontDestroyOnLoad(gameObject);
		
		Debug.Log("awaking playerdeck");
		if (playerDeck.pD == null)	playerDeck.pD = this;	//singleton
		
		templateTransform = CardTemplate.Instance.transform;
		
	}



	public void SaveDeckBeforePlaying()
	{
		SavedDeck.Clear();
		foreach (int foundcard in Deck)
			SavedDeck.Add(foundcard);
	}

	public void LoadSavedDeck()
	{
		Deck.Clear();
		foreach (int foundcard in SavedDeck)
			Deck.Add(foundcard);
	}

	
	public  void LoadPlayerDeckOffline()
	{
	
		string deckstring;

		
		deckstring = pD.offlineDeck.text;
		Debug.Log("loading offline deck");
			
		pD.Deck = LoadDeck(deckstring);
			
		deckstring = pD.offlineCollection.text;
			
		pD.Collection = LoadDeck(deckstring);


	}



	public    List<int> LoadDeck (string deckstring)
	{
		List<int> tempdeck = new List<int>();
		string[] temparray = deckstring.Split(","[0]);
		for (int i = 0; i < temparray.Length; i++)
		{
			if (temparray[i].Length>0) tempdeck.Add(System.Int32.Parse(temparray[i]));
		}
		return tempdeck;
	}

	 public  void ShuffleDeck(List<int> DeckToShuffle)
	{
		for (int i = 0; i < DeckToShuffle.Count; i++) {
			int temp = DeckToShuffle[i];
			int randomIndex = Random.Range(i, DeckToShuffle.Count);
			DeckToShuffle[i] = DeckToShuffle[randomIndex];
			DeckToShuffle[randomIndex] = temp;
		}
	}





	void Start () {
	
	}


	// Update is called once per frame
	void Update () {
	

	}





	public card MakeCard (int Index, bool AI=false)
	{
		GameObject new_card_obj = new GameObject();
		
		card new_card = new_card_obj.AddComponent("card") as card; //adding card script

		new_card_obj.AddComponent("AudioSource");
			
			
		GameObject.FindWithTag ("Player").SendMessage("TakesCardSFX");

		new_card.Index = Index;
	

		LoadCardStats(new_card);

		new_card.GetComponent<AudioSource>().clip = new_card.sfxEntry; //play entry sound, works

		return new_card;
	}


	 public  void PlaceCreatureInGame(card creaturecard, bool ForEnemy=false) //utility 
	{
		if (ForEnemy == false) {

			Player.AddCreature(creaturecard);   

		} else 
			
		{ 				//it's the enemy who is placing a creature
						
			Enemy.AddCreature(creaturecard); 

		}
		

		creaturecard.id_ingame = System.Int32.Parse("4"+nexttokenid.ToString());
		nexttokenid++;
				
	}



	  public void LoadCardStats(card temp_card)
	{
		int Index = temp_card.Index;
		Debug.Log ("index:"+Index);

		//Debug.Log ("name:"+cardsDB[Index]["name"].ToString());
		DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == Index).SingleOrDefault();
		if (dbcard == null) Debug.LogWarning("card not found in the new db!");
		temp_card.gameObject.name =  dbcard.name; //for easier debugging with inspector, otherwise there will be a ton of NewGameObjects 

		temp_card.Name = dbcard.name; 

		//Debug.Log ("type:"+cardsDB[Index]["type"].ToString());
		temp_card.Type = dbcard.type;
		temp_card.Subtype = dbcard.subtype;
		temp_card.CardColor = dbcard.color;

		if (MainMenu.TCGMaker.core.UseManaColors) {

			temp_card.Cost = dbcard.cost;

		} else { //gonna ensure every cost is colorless
			ManaColor colorless = MainMenu.TCGMaker.core.colors [0];

			temp_card.Cost = new List<ManaColor>();

			foreach (ManaColor foundcolor in dbcard.cost)
			temp_card.Cost.Add(colorless);
		}

		temp_card.Art = dbcard.art;

		temp_card.DiscardCost = dbcard.discardcost;

//		Debug.Log("gonna update effects");
		
		
		foreach (Effect effect in dbcard.effects){
//			Debug.Log("added effect");
			temp_card.Effects.Add(effect);
		}
					
//		Debug.Log ("updated effects on card "+temp_card.Name);
		if (!temp_card.abilities ) temp_card.abilities = temp_card.gameObject.AddComponent ("abilities") as abilities;
				
		temp_card.sfxEntry =  dbcard.sfxentry;
		temp_card.sfxMove0 =  dbcard.sfxmove0;
		temp_card.sfxMove1 =  dbcard.sfxmove1;
		temp_card.sfxAbility0 =  dbcard.sfxability0;

		if (temp_card.IsACreature()) { //if its a creature

			if (temp_card.buffs == null) temp_card.buffs = new List<Buff>();
		
			temp_card.Level =  dbcard.level;
			temp_card.GrowID =  dbcard.growid;

			temp_card.Hero =  dbcard.hero;
			temp_card.Ranged =  dbcard.ranged;

			//default keyword abilities:
			temp_card.takes_no_combat_dmg = dbcard.takes_no_combat_dmg;
			temp_card.deals_no_combat_dmg = dbcard.deals_no_combat_dmg;
			temp_card.no_first_turn_sickness = dbcard.no_first_turn_sickness;

			temp_card.cant_attack = dbcard.cant_attack;
			temp_card.extramovement = dbcard.extramovement;

			temp_card.free_attack = dbcard.free_attack;
			temp_card.less_dmg_from_ranged = dbcard.less_dmg_from_ranged;
			temp_card.no_dmg_from_ranged = dbcard.no_dmg_from_ranged;
			temp_card.takes_no_spell_dmg = dbcard.takes_no_spell_dmg;
			//


			temp_card.CreatureStartingOffense = temp_card.CreatureOffense = dbcard.offense;
			temp_card.CreatureStartingDefense = temp_card.CreatureDefense = dbcard.defense;

			temp_card.CustomInts.Clear();	//otherwise creature upgrades won't work
			temp_card.CustomStrings.Clear();

			foreach (CustomInt c_int in dbcard.CustomInts)
			{
				temp_card.CustomInts.Add(c_int.h_name, c_int.value);

			}

			foreach (CustomString c_string in dbcard.CustomStrings)
			{
				temp_card.CustomStrings.Add(c_string.h_name, c_string.value);
				
			}

			//if (cardsDB [Index] ["defender"].ToString() == "1") temp_card.GetComponent<card> ().CanAttack = false;
			//if (cardsDB [Index] ["doubledmg"].ToString() == "1") temp_card.GetComponent<card> ().DoubleDamage = true;
			//if (cardsDB [Index] ["takeshalfdmg"].ToString() == "1") temp_card.GetComponent<card> ().TakesHalfDamage = true;

		} 
	


	}

	GameObject AddCardComponent(string component_name, card parent)
	{
		GameObject template_obj = templateTransform.Find(component_name).gameObject;
		GameObject new_obj = (GameObject)Instantiate (template_obj);
		new_obj.name = template_obj.name; //Don't change it, it can be used later
		
		AssignParentWithLocalPos(new_obj, parent.gameObject);

		return new_obj;
	}

	public void AssignParentWithLocalPos(GameObject child, GameObject parent)
	{
		float slot_scale = 1;
		if (parent.transform.parent != null) //if the card is in a slot
			slot_scale = parent.transform.parent.localScale.x;

		child.transform.localScale = new Vector2 (slot_scale*child.transform.localScale.x * parent.transform.localScale.x, slot_scale*child.transform.localScale.y * parent.transform.localScale.y);
		Vector3 localpos = child.transform.position;

		child.transform.position =  parent.transform.position; 
		child.transform.parent = parent.transform; //this will make our art follow the card
		child.transform.localPosition = localpos;
	}

	
	void AddStringStat(card card_to_update, string statname, Transform templateTransform)
	{
		if (CardTemplate.Instance.transform.Find (statname + "3DText")) { //example: specialpower3DText
			if (card_to_update.CustomStrings.ContainsKey (statname)) {
				GameObject cstat = (GameObject)Instantiate (templateTransform.Find (statname + "3DText").gameObject);
				cstat.name = statname + "3DText"; //Don't change it, it's used later
				cstat.GetComponent<TextMesh> ().text = card_to_update.CustomStrings [statname]; //custom stat value
				AssignParentWithLocalPos (cstat, card_to_update.gameObject);
			} else
				Debug.LogWarning ("no such custom stat found: " + statname);
		} 
		else	Debug.LogWarning ("no template found for stat: " + statname);
	}

	void AddIntStat(card card_to_update, string statname, Transform templateTransform)
	{
		if (templateTransform.Find (statname + "3DText")) { //example: specialpower3DText
						if (card_to_update.CustomInts.ContainsKey (statname)) {
				GameObject cstat = (GameObject)Instantiate (templateTransform.Find (statname + "3DText").gameObject);
								cstat.name = statname + "3DText"; //Don't change it, it's used later
								cstat.GetComponent<TextMesh> ().text = card_to_update.CustomInts [statname].ToString (); //custom stat value
								AssignParentWithLocalPos (cstat, card_to_update.gameObject);
						} else
								Debug.LogWarning ("no such custom stat found: " + statname);
				} 
		else	Debug.LogWarning ("no template found for stat: " + statname);
	}

	static Texture2D SpriteToTexture(Sprite sprite)
		
	{
		Texture2D croppedTexture = new Texture2D ((int)sprite.rect.width, (int)sprite.rect.height);
		
		Color[] pixels = sprite.texture.GetPixels ((int)sprite.textureRect.x, 
		                                           (int)sprite.textureRect.y, 
		                                           (int)sprite.rect.width, 
		                                           (int)sprite.rect.height);
		croppedTexture.SetPixels (pixels);
		croppedTexture.Apply ();
		return croppedTexture;
	}



	public void AddArtAndText(card card_to_update, bool nocollider=false) 
	{


		DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == card_to_update.Index).SingleOrDefault();

		GameObject cardobj = card_to_update.gameObject;
		if (!nocollider && !card_to_update.collider2D) {
						card_to_update.gameObject.AddComponent ("BoxCollider2D");  
						BoxCollider2D collider = card_to_update.GetComponent<BoxCollider2D> () as BoxCollider2D;
						
						collider.size = new Vector3 (MainMenu.ColliderWidth, MainMenu.ColliderHeight, 0f); // this defines the clickable area size of each card
				}

		if (!card_to_update.GetComponent<SpriteRenderer>()) card_to_update.gameObject.AddComponent("SpriteRenderer");
		
					
		GameObject card_art = AddCardComponent("CardArt", card_to_update);
					
		card_art.GetComponent<SpriteRenderer>().sprite = dbcard.art ; 

	
		if (templateTransform.Find(card_to_update.CardColor.name+"Frame"))
			card_to_update.GetComponent<SpriteRenderer> ().sprite = templateTransform.Find(card_to_update.CardColor.name+"Frame").GetComponent<SpriteRenderer> ().sprite;

		else Debug.LogError("Could not find a frame for color: "+card_to_update.CardColor.name + " ! Check that you have a "+card_to_update.CardColor.name+"Frame attached to the Card Template in MainMenuScene");


		card_to_update.renderer.sortingOrder = 2;


		foreach (string c_stat in card_to_update.CustomInts.Keys)	//adding custom stats:
						AddIntStat(card_to_update, c_stat, templateTransform);

		foreach (string c_stat in card_to_update.CustomStrings.Keys)
						AddStringStat(card_to_update, c_stat, templateTransform);


		GameObject card_name = AddCardComponent("Name3DText", card_to_update);

		card_name.GetComponent<TextMesh> ().text = card_to_update.Name.ToString (); //creature offense


		string cardtext = dbcard.text;
		if (cardtext != "") 
			{
				GameObject card_description = AddCardComponent("Description3DText", card_to_update);
				
				card_description.GetComponent<TextMesh> ().text = TextWrap(cardtext, 30);  //card description
						
			} 
			
		if (card_to_update.Type != 0) //if it's not a keeper, add mana text 
			{
				if (MainMenu.TCGMaker.core.UseManaColors)
				{
					int i = 0;
					foreach (ManaColor foundcolor in card_to_update.Cost)
					{
						
						if (!foundcolor.icon) Debug.LogWarning("No icon found for mana color: "+foundcolor.name);
						else {
						GameObject mana_icon = AddCardComponent("ManaIcon", card_to_update);
						
						Sprite thisicon = foundcolor.icon;
						mana_icon.GetComponent<SpriteRenderer>().sprite = thisicon; //mana cost
						
						Vector3 pos = mana_icon.transform.localPosition;
						pos.x -= i * thisicon.bounds.size.x * mana_icon.transform.localScale.x * 1.1f; //by default, the distance between mana icons is 10% of an icon's width
						mana_icon.transform.localPosition = pos;
						}

						i++;
					}
				}
				else 
				{	
					GameObject card_mana = AddCardComponent("Mana3DText", card_to_update);
					
					card_mana.GetComponent<TextMesh> ().text = card_to_update.Cost.Count.ToString (); //mana cost
					
					GameObject card_cost_icon = AddCardComponent("CostIcon", card_to_update);
						
				}
		}

		if (card_to_update.IsACreature())
		{
			//the atk/def numbers that will be displayed on top of the card:
			GameObject card_atk = AddCardComponent("Offense3DText", card_to_update);
			GameObject card_atk_icon = AddCardComponent("OffenseIcon", card_to_update);			
			card_atk.GetComponent<TextMesh> ().text = card_to_update.CreatureOffense.ToString ();  //creature offense

			if (!MainMenu.TCGMaker.core.OptionOneCombatStatForCreatures)
				{
					GameObject card_def = AddCardComponent("Defense3DText", card_to_update);
					GameObject card_def_icon = AddCardComponent("DefenseIcon", card_to_update);		
					card_def.GetComponent<TextMesh> ().text = card_to_update.CreatureDefense.ToString (); //creature offense
						
				}
		}

		foreach (Transform child in card_to_update.transform)
			child.renderer.enabled = true;
		
	}

public static string TextWrap(string originaltext, int chars_in_line)
	{
		string output = "";
		string[] words = originaltext.Split(' ');
		int line = 0;
		foreach (string word in words)
		{
			if ((line + word.Length + 1 ) <= chars_in_line ) 
			{
				output += " " + word;
				line += word.Length + 1;
			}
			else { 
				output += "\n" + word;
				line = word.Length;
			}
		}
		return output;
	}
}
