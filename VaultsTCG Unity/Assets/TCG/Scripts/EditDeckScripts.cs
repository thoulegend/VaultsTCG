using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EditDeckScripts : MonoBehaviour {
	private int cardsindeck;

	public  GUISkin customskin = null;

	public static int CardsInARow = 15;

	public static int PageDeck = 0;
	public static int PageCollection = 0;

	public static float card_scale = 1f;
	public static float offensedefense_scale = 0.056f;
	public static Vector3 FirstCardColliderSize = new Vector3 (1.15f, 1.9f, 0f);
	public static Vector3 OtherCardsColliderSize = new Vector3 (0.6f, 1.9f, 0f);
	public static Vector2 OtherCardsColliderCenter = new Vector2 (0.25f, 0);
	//collider.size = new Vector3 (1.4f, 1.9f, 0f);
	public  Texture2D editdeck_bg;


	public static List<card> cards_in_deck = new List<card>(); 
	public static List<card> cards_in_collection = new List<card>(); 

	public static List <int> collection= new List<int>();
	public static List <int> collection_amount= new List<int>();

	public static List <int> deck = new List<int>();
	public static List <int> deck_amount= new List<int>();

	public static List <Vector3> guivector = new List<Vector3>();
	public static List <string> guiamount = new List<string>();

	public static List<card> cards_filtered_out = new List<card>(); 

	public static card itemBeingDragged;

	public static bool TableMode = false;

	public static string searchname="";
	public static bool FilterOn = false;


	public static bool Zoomed = false;
	public static bool CardsNeedArrange = false;
	private bool CanExitNow = false;


	public static void MoveCard(card card_to_move)
	{
		if (card_to_move.IsZoomed) { Debug.Log ("starting unzoom after moving"); Player.CanUnzoom = true; card_to_move.UnZoom(); }
		//move the card from collection to deck or from deck to collection 
		if (card_to_move.InCollection) 
		{
			Debug.Log("mouse down collection, card:"+card_to_move);
			
			
			card foundcard = EditDeckScripts.cards_in_deck.Find(x => x.Index == card_to_move.Index);
			
			if (foundcard) { Debug.Log("found, adding amount"); foundcard.Amount++;}
			else 
			{ 	//Debug.Log("cloning");

				GameObject clone = (GameObject)Instantiate(card_to_move.gameObject);

				EditDeckScripts.cards_in_deck.Add(clone.GetComponent<card>()); //make a copy of this card to display in deck
				//foundcard = EditDeckScripts.cards_in_deck.Find(x => x.Index == Index);
				clone.GetComponent<card>().Amount=1;
			}
			card_to_move.Amount--;
			
			
		}
		else 
		{
			Debug.Log("moving card index: "+ card_to_move.Index.ToString());
			EditDeckScripts.cards_in_collection.Find(x => x.Index == card_to_move.Index).Amount++;
			Debug.Log("amount in coll: "+ 	EditDeckScripts.cards_in_collection.Find(x => x.Index == card_to_move.Index).Amount);
			card_to_move.Amount--;
			
			
			
			if (EditDeckScripts.FilterOn)
			{
				if (card_to_move.Name.ToString().ToLower().Contains(EditDeckScripts.searchname.ToLower())) { EditDeckScripts.cards_in_collection.Add(card_to_move); }
				else { 
					EditDeckScripts.cards_filtered_out.Add(card_to_move); 
					card_to_move.GetComponent<SpriteRenderer>().enabled = false; 
				}
			}
			
		}
		
		
		EditDeckScripts.ArrangeCards();
		
	}



	// Use this for initialization
	void Start () {
		Debug.Log("startind editdeckscripts");


		

		Camera.main.orthographicSize = Screen.height / 2.0f /100f;
		camera.orthographicSize = Screen.height / 2.0f /100f;

		cards_in_deck = new List<card>(); 
		cards_in_collection = new List<card>(); 

		LoadDeck();
		LoadCollection();
		ArrangeCards();
		Debug.Log("deck count editdeck"+ playerDeck.pD.Deck.Count.ToString());
	}
	
	private  string PlayerDeckToString()
	{
		string output="";
		foreach (int card in playerDeck.pD.Deck)
		{
			output+=card.ToString()+",";
		}
		
		return output;
	}


	 void DoUpdateDeck()
	{
		Debug.Log("updating");

		WWWForm form = new WWWForm();
		
		form.AddField("userid", MainMenu.userid);
		form.AddField("deckstring", PlayerDeckToString());
		Debug.Log("deckstring: "+PlayerDeckToString());
		
		
		WWW w = new WWW(MainMenu.url_update_deck, form);
		
		StartCoroutine(UpdateDeck(w));

	}
	
	 IEnumerator UpdateDeck( WWW w)
	{
		Debug.Log("ienum updating");
		yield return w;
		if (w.error == null) {
			Debug.Log("no error");
						Debug.Log ("updated deck on server");
			CanExitNow = true;
				} else {
			Debug.Log("error");
						Debug.Log ("ERROR:" + w.error + "\n");
				}
		Debug.Log("wtext: "+w.text);
		Debug.Log("ended updating");

	}


	void OnGUI()
		
	{
		GUI.skin = customskin;

	
		// 							displaying the deck
		
		if (CardsNeedArrange)

		{ 
			guivector.Clear();
			guiamount.Clear();

			BoxCollider2D collider;

			Debug.Log("PageDeck " + PageDeck);
			int cardsdisplayed = 0;
			int i = 0;
			int StartIndex = PageDeck*CardsInARow;
			for ( i = StartIndex; cardsdisplayed<(StartIndex+CardsInARow); i++)
			
			{
			if (i>=cards_in_deck.Count) break;
				if (cards_in_deck[i].Amount<=0) { foreach (Transform child in cards_in_deck[i].transform)	child.gameObject.renderer.enabled = false; //hiding the offense/defense text
				}
			else //showing the card

				{	foreach (Transform child in cards_in_deck[i].transform)	child.gameObject.renderer.enabled = true; //showing the offense/defense text 
					Debug.Log("name deck card: "+ cards_in_deck[i].Name );
					cards_in_deck[i].InCollection = false;
					cards_in_deck[i].renderer.enabled = true;

					if (MainMenu.TCGMaker.core.OptionCardFrameIsSeparateImage) 
					{
					foreach (Transform child in cards_in_deck[i].transform) child.renderer.sortingOrder = 50-(cardsdisplayed*2)+1; 
					cards_in_deck[i].transform.Find ("CardArt").renderer.sortingOrder = 50-(cardsdisplayed*2); 
					cards_in_deck[i].renderer.sortingOrder = 50-(cardsdisplayed*2)+1; // this ensures the cards will overlap each other in the right order

					
				
					}
					else
					{
						cards_in_deck[i].renderer.sortingOrder = 50-(cardsdisplayed*2); 
						foreach (Transform child in cards_in_deck[i].transform) child.renderer.sortingOrder = 50-(cardsdisplayed*2)+1; 
					}


					collider = cards_in_deck[i].GetComponent<BoxCollider2D> () as BoxCollider2D;
					collider.enabled = true;

					cards_in_deck[i].transform.position = new Vector3 (-5.53f + cardsdisplayed * 0.6f, -0.8f, 0f); //don't change z

					if (cards_in_deck[i].Amount>1)
					{
						//Debug.Log("amount " + cards_in_deck[i].Amount.ToString());
						guivector.Add(Camera.main.WorldToScreenPoint(cards_in_deck[i].transform.position));
						guiamount.Add( cards_in_deck[i].Amount.ToString());



					}
									
					if (i==0) { //the first card is fully showing and needs a bigger collider
					
					collider.size = FirstCardColliderSize;
					collider.center =new Vector2(0,0);
										}
				cardsdisplayed++;
			} 
		}	
		
		//						 now displaying the collection
		if (TableMode) {}
		else 
		{
			cardsdisplayed = 0;
			StartIndex = PageCollection*CardsInARow;
			Debug.Log("PageCollection " + PageCollection);
			Debug.Log("cards_in_collection.Count " + cards_in_collection.Count);
			Debug.Log("StartIndex " + StartIndex);
			Debug.Log("StartIndex+CardsInARow :" + (StartIndex+CardsInARow).ToString());
				for ( i = StartIndex; cardsdisplayed<(StartIndex+CardsInARow); i++)
			{

				if (i>=cards_in_collection.Count) break;
					Debug.Log("collection card  " + 	cards_in_collection[i].GetComponent<card>().Index);
					if (cards_in_collection[i].GetComponent<card>().Amount<=0) {  foreach (Transform child in cards_in_collection[i].transform)	child.gameObject.renderer.enabled = false; //hiding the offense/defense text 
					}  
				else if (cards_in_collection[i].GetComponent<card>().FilteredOut) {}
				else {
						foreach (Transform child in cards_in_collection[i].transform)	child.gameObject.renderer.enabled = true; //showing the offense/defense text 
						//Debug.Log("collection card  " + 	cards_incollection[i].GetComponent<card>().Index);
						cards_in_collection[i].GetComponent<card>().InCollection = true;
						cards_in_collection[i].GetComponent<SpriteRenderer>().enabled = true;

						if (MainMenu.TCGMaker.core.OptionCardFrameIsSeparateImage) 
						{

							cards_in_collection[i].renderer.sortingOrder = 50-(cardsdisplayed*2)+1; // this ensures the cards will overlap each other in the right order
							
							foreach (Transform child in cards_in_collection[i].transform) child.renderer.sortingOrder = 50-(cardsdisplayed*2)+1; 
							cards_in_collection[i].transform.Find ("CardArt").renderer.sortingOrder = 50-(cardsdisplayed*2); 
						}
						else
						{
							cards_in_collection[i].renderer.sortingOrder = 50-(cardsdisplayed*2); 
							foreach (Transform child in cards_in_collection[i].transform) child.renderer.sortingOrder = 50-(cardsdisplayed*2)+1; 
						}

						collider = cards_in_collection[i].GetComponent<BoxCollider2D> () as BoxCollider2D;
						collider.enabled = true;

						cards_in_collection[i].transform.position = new Vector3 (-5.53f + cardsdisplayed * 0.6f, 1.65f, 0f);		//don't change z
					
						if ((cards_in_collection[i].Amount>1)&&(!TableMode)) //table mode uses a separate display of amounts
						{
							//Debug.Log("amount " + cards_in_collection[i].Amount.ToString());
							guivector.Add(Camera.main.WorldToScreenPoint(cards_in_collection[i].transform.position));
							guiamount.Add( cards_in_collection[i].Amount.ToString());

							
							
						}
					
						if (i==0) { //the first card is fully showing and needs a bigger collider
						
						collider.size = FirstCardColliderSize;
						collider.center =new Vector2(0,0);
						
											} 
					
					cardsdisplayed++;
					}
				}
			
			
			}

			CardsNeedArrange = false; //finished arranging cards

		}





		if (TableMode) 
		{
			int row = 0;
			int rows_in_column = 7;
			int column = 0;
			foreach (card foundcard in cards_in_collection)

			{	
				if ((foundcard.FilteredOut)||(foundcard.Amount<=0)) {}
				else
				{


				//Debug.Log(foundcard.GetComponent<card>().Name);	
				if ((row!=0)&&(row%rows_in_column == 0))  { column++; row = 0; }  //if we should start a new column, start it and reset rows
			
					string amounttext = "";
				if (foundcard.Amount>1)
				{
						amounttext = " x"+foundcard.Amount;
						//GUI.Label(new Rect(100+200*column,110+22*row,200,20),foundcard.Amount.ToString());
						//guivector.Add(new Vector3(45+200*column, Screen.height-(110+22*row), 0));
						//guiamount.Add(foundcard.Amount.ToString());
						
						
				}
				if (GUI.Button(new Rect(35+200*column,110+22*row,200,20), foundcard.GetComponent<card>().Name.ToString()+amounttext, "tablemodecard" ))  //just the names of our cards, without images
				{
					foundcard.OnMouseDown(); 
				
					break;
				}

				row++;
				}
			}
		}

		//if (!Zoomed)//displaying amount numbers 
		//{
			for (int i=0; i<guivector.Count; i++)
			{
			Vector3 p = guivector[i];
			GUI.Label(new Rect(p.x+15,Screen.height-p.y+50,200,30), "x"+guiamount[i], "amount");
			}
		//}

	
	
		GUI.Label(new Rect(10,4, 228, 30), "EDIT DECK", "title");

		if (GUI.Button(new Rect(275, 555, 100, 50), "SAVE", "nobg"))
		{
			playerDeck.pD.Deck.Clear(); //deleting the old deck

	
			foreach (card foundcard in cards_in_deck) 
			{
				if (foundcard.Amount>0)
				{
					for (int i = 0; i<foundcard.Amount; i++)
					{	 playerDeck.pD.Deck.Add(foundcard.Index); 
						Debug.Log("adding card:"+ foundcard.Index.ToString());
					} //adding cards to the new deck
				
				}
			}

			DoUpdateDeck();
			//Debug.Log("deck count editdeck"+ playerDeck.Deck.Count.ToString());
	

			//MainMenu.IsMulti = false;
			//Application.LoadLevel(MainMenu.SceneNameGame);
		}

		if (GUI.Button(new Rect(475,555, 100, 50), "CANCEL", "nobg"))
		{ Application.LoadLevel(MainMenu.SceneNameMainMenu);}
	
		if (GUI.Button(new Rect(120 ,40, 150, 50), "Table View"))
		{
			if (TableMode) {
				TableMode = false; 

				foreach (card foundcard in cards_in_collection)
				{
					foreach (Transform child in foundcard.transform)	child.gameObject.renderer.enabled = true; //showing the offense/defense text
					foundcard.GetComponent<SpriteRenderer>().enabled = true; 
				}
				ArrangeCards();
			}
				else
			{

				TableMode = true; 
				ArrangeCards();
			foreach (card foundcard in cards_in_collection)
					{
						foreach (Transform child in foundcard.transform)	child.gameObject.renderer.enabled = false; //hiding the offense/defense text
						foundcard.GetComponent<SpriteRenderer>().enabled = false; 
					}
			}
		}
		GUI.Label(new Rect(7,46, 228, 30), "COLLECTION");
		GUI.Label(new Rect(7,306, 228, 30), "DECK");
		GUI.Label(new Rect(275,46, 228, 30), "SEARCH BY NAME");

		searchname = GUI.TextField(new Rect(430,46, 228, 30), searchname);
		if (GUI.Button(new Rect(660,40, 80, 50), "GO"))
		{ UnFilterCollection();
			FilterCollection();
			FilterOn = true;
		}
		if (GUI.Button(new Rect(820 ,40, 180, 50), "CLEAR FILTERS"))
		{ UnFilterCollection();
			ArrangeCards();
			FilterOn = false;
		}

		if (GUI.Button(new Rect(984 ,160, 36, 50), ">"))
		{ 
			int cardstodisplay = 0;
			foreach (card foundcard in  cards_in_collection)
			{
				if ((foundcard.Amount>0)&&(foundcard.FilteredOut==false)) cardstodisplay++;
			}
			int MaxPage = Mathf.CeilToInt(cardstodisplay/CardsInARow); //rounding up
			if (PageCollection < MaxPage)
			{
				PageCollection++; //going to the next page
				ArrangeCards();
			}
		}

		if (GUI.Button(new Rect(-5 ,160, 36, 50), "<"))
		{ 
			if (PageCollection>0) {
				PageCollection--; //going to the previous page
			ArrangeCards();
			}
		}


		if (GUI.Button(new Rect(984 ,400, 36, 50), ">"))
		{ 
			int cardstodisplay = 0;
			foreach (card foundcard in  cards_in_deck)
			{
				if ((foundcard.Amount>0)&&(foundcard.FilteredOut==false)) cardstodisplay++;
			}
			int MaxPage = Mathf.CeilToInt(cardstodisplay/CardsInARow); //rounding up

			if (PageDeck < MaxPage)
			{
				PageDeck++; //going to the next page
				ArrangeCards();
			}
		}

		if (GUI.Button(new Rect(-5 ,400, 36, 50), "<"))
		{ 
			if (PageDeck>0)
			{ PageDeck--;//going to the previous page
			ArrangeCards();
			}
		}


		if ( CanExitNow) Application.LoadLevel(MainMenu.SceneNameMainMenu);//go back to main menu after saving deck

		camera.Render(); //rendering zoomed cards (if any) on top of the gui

	}

	void UnFilterCollection()
	{

		foreach (card foundcard in cards_in_collection) 
		{
			foundcard.GetComponent<card>().FilteredOut = false;
		}
	
	}

	void FilterCollection()
	{
		Debug.Log("need to find: "+searchname);
		for (int i = 0; i < cards_in_collection.Count; i++)  //note how you can't do that with foreach cycling
		{
	
			if (cards_in_collection[i].GetComponent<card>().Name.ToString().ToLower().Contains(searchname.ToLower())) {  //testing card name against our search, case-independent

			} 
				else {
				cards_in_collection[i].GetComponent<card>().FilteredOut = true;
				ArrangeCards();
					}
		}

			
	}

	void Update()
	{
	
	}

	void LoadCollection()
	{
		collection.Clear ();
		cards_in_collection.Clear ();
		collection_amount.Clear ();

		int i=0;
		foreach (int foundcardID in playerDeck.pD.Collection) {

			if (collection.Contains(foundcardID)) {
				i =	collection.IndexOf(foundcardID);
				collection_amount[i]++;
			}
			else
			{
				Debug.Log("adding to collection id: "+foundcardID);
				collection.Add(foundcardID);
				collection_amount.Add(1);
			}
				}


		foreach (int foundcardID in playerDeck.pD.Deck) {

				i =	collection.IndexOf(foundcardID);
				collection_amount[i]--;  //removing the deck cards from visible colllection
		}


		i = 0;

		foreach (int foundcardID in collection) {
		GameObject new_card_obj = new GameObject ();
		card new_card = new_card_obj.AddComponent("card") as card; 
		new_card.Amount = collection_amount[i];
		new_card.Index = foundcardID;
		new_card.InCollection = true;
		DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == new_card.Index).SingleOrDefault();
		if (dbcard == null) Debug.LogWarning("card not found in the new db!");
		
		new_card.Name = dbcard.name; 
		
		new_card_obj.name = dbcard.name;  //for debugging with inspector
		new_card.Type = dbcard.type;
		new_card.CardColor = dbcard.color;
		
		if (new_card.Type!=0) new_card.Cost = dbcard.cost;
	
		new_card_obj.AddComponent ("BoxCollider2D");  //enemy hand cards dont need a collider yet since they are not visible
		BoxCollider2D collider = new_card.GetComponent<BoxCollider2D> () as BoxCollider2D;
		
		collider.size = OtherCardsColliderSize; // its smaller than the card itself, because the cards are covering each other
		collider.center = OtherCardsColliderCenter;
		
		
			if (new_card.Type == 1) //if it's a creature
			{
				new_card.CreatureOffense = dbcard.offense;
				new_card.CreatureDefense = dbcard.defense;


			}
		playerDeck.pD.AddArtAndText(new_card, true);
		new_card.transform.localScale = new Vector3(card_scale, card_scale, card_scale);
		cardsindeck++;
		cards_in_collection.Add(new_card);
		i++;
		}

	}



	void LoadDeck()
	{
		deck.Clear ();
		cards_in_deck.Clear ();
		deck_amount.Clear ();
		int i=0;
		foreach (int foundcardID in playerDeck.pD.Deck) {
			
			if (deck.Contains(foundcardID)) {
				i =	deck.IndexOf(foundcardID);
				deck_amount[i]++;
			}
			else
			{
				
				deck.Add(foundcardID);
				deck_amount.Add(1);
			}
		}
		i = 0;
		foreach (int foundcardID in deck) {
		
			
			GameObject new_card_obj = new GameObject ();
			card new_card = new_card_obj.AddComponent("card") as card; 

			new_card.Amount = deck_amount[i];
			new_card.Index = foundcardID;
			DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == new_card.Index).SingleOrDefault();
			if (dbcard == null) Debug.LogWarning("card not found in the new db!");
			
			new_card.Name = dbcard.name; 
			
			new_card_obj.name = dbcard.name;  //for debugging with inspector
			new_card.Type = dbcard.type; 
			new_card.CardColor = dbcard.color;

			if (new_card.Type!=0) new_card.Cost = dbcard.cost;

			new_card_obj.AddComponent ("BoxCollider2D");  

			BoxCollider2D collider = new_card.GetComponent<BoxCollider2D> () as BoxCollider2D;
			collider.size = OtherCardsColliderSize; // its smaller than the card itself, because the cards are covering each other
			collider.center = OtherCardsColliderCenter;

			new_card.transform.localScale = new Vector3(card_scale, card_scale, card_scale);
			if (new_card.Type == 1) //if it's a creature
			{
				new_card.CreatureOffense = dbcard.offense;
				new_card.CreatureDefense = dbcard.defense;
			}

			playerDeck.pD.AddArtAndText(new_card, true);
			cardsindeck++;
			cards_in_deck.Add(new_card);

		
			i++;
		}
	}

	public static void HideAllCards()
	{
		foreach (card foundcard in cards_in_deck)
		{
			foreach (Transform child in foundcard.transform)	child.gameObject.renderer.enabled = false; //hiding the offense/defense text
			foundcard.GetComponent<SpriteRenderer>().enabled = false;
			BoxCollider2D collider = foundcard.GetComponent<BoxCollider2D> () as BoxCollider2D;
			collider.enabled = false;
		}

		if (TableMode) {}
		else 
		{
			foreach (card foundcard in cards_in_collection)
			{
				foreach (Transform child in foundcard.transform)	child.gameObject.renderer.enabled = false; //hiding the offense/defense text
				foundcard.GetComponent<SpriteRenderer>().enabled = false;
				BoxCollider2D collider = foundcard.GetComponent<BoxCollider2D> () as BoxCollider2D;
				collider.enabled = false;
			}
		}
	}

	public static void ArrangeCards()
	{
		HideAllCards();

		CardsNeedArrange = true;


	}




}
