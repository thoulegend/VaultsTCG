
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq; 

[System.Serializable]
public class DBZone  {

	public DBZone(DBZone original)	//copying
	{
		Name = original.Name;
		RotateDegrees = original.RotateDegrees;
		
		UseSlots = original.UseSlots;
		StackAllInOneSlot = original.StackAllInOneSlot;
		PlayerCanChooseSlot = original.PlayerCanChooseSlot;
		
		PlayerInvisible = original.PlayerInvisible;
		EnemyInvisible = original.EnemyInvisible;
		DrawAtTheStartOfGame = original.DrawAtTheStartOfGame;
		EnemyFaceDown = original.EnemyFaceDown;
		PlayerFaceDown = original.PlayerFaceDown;

		Default = original.Default;
		Shared = original.Shared;
	}

	public DBZone(string c_name="")
	{
		Name = c_name;
	}

	public string Name;

	public float RotateDegrees = 0;
	
	public bool UseSlots = false;
	public bool StackAllInOneSlot = false;
	public bool PlayerCanChooseSlot = true;

	public bool PlayerInvisible = false;
	public bool EnemyInvisible = false;
	public int DrawAtTheStartOfGame = 0;
	public bool EnemyFaceDown = false;
	public bool PlayerFaceDown = false;
		
	public bool Default = false; 
	public bool Shared = false;

}

public class Zone : MonoBehaviour  {
	//using a custom inspector so no need to hide anything from inspector
	public DBZone dbzone;

	// for newly drawn cards ids:
	private static int last_id;

	public int zone_id;

	public bool Shared {
		get { return dbzone.Shared; }
	}

	public float RotateDegrees {
		get { return dbzone.RotateDegrees; }
	}
	
	public bool UseSlots {
		get { return dbzone.UseSlots; }
	}

	public bool StackAllInOneSlot {
		get { return dbzone.StackAllInOneSlot; }
	}

	public bool PlayerCanChooseSlot  {
		get { return dbzone.PlayerCanChooseSlot; }
	}
	


	public int DrawAtTheStartOfGame  {
		get { return dbzone.DrawAtTheStartOfGame; }
	}



	public bool BelongsToPlayer = true;

	public bool FaceDown {
		get { if (BelongsToPlayer) return dbzone.PlayerFaceDown;
					else return dbzone.EnemyFaceDown;
		}
	}

	public bool Invisible  {
		get {if (BelongsToPlayer)   return dbzone.PlayerInvisible;
			else return dbzone.EnemyInvisible;
		}
	}
	public List<card> cards;

	public Transform slot_to_use;

	public bool PlayerIsChoosingASlot = false;	//using this in addition to a target type because we need to know which zone to highlight

	public static Texture2D hl_mouseover_texture;

	public static Texture2D hl_all_texture;

	public  float ZoneDefaultScale = 1f; //we assume that all cards are the same size 
	 
	public  float ZoneScale = 1f; 

	float minspacing = 0.4f;

	public void Awake(){

		if (!UseSlots)	// disabling the slots' colliders if we don't use slots for this zone
		{ Debug.Log("disabling colliders:"+dbzone.Name);
			foreach (Transform child in transform)
								if (child.GetComponent<Slot> ())
									child.collider.enabled = false;
		}
		//calculate the scale required to fit ONE card in the zone
		
		float realHeight = MainMenu.ColliderHeight;
		float realWidth = MainMenu.ColliderWidth;
		
		bool arrangevertical = false;
		
		if (RotateDegrees == 90 || RotateDegrees == -90) { // only supporting these rotations for now
			realWidth = realHeight;
			realHeight = MainMenu.ColliderWidth;
		}
		
		if (collider.bounds.size.y > collider.bounds.size.x)	arrangevertical = true;
		
		if (arrangevertical) ZoneDefaultScale = collider.bounds.size.x / realWidth ;
		else 	ZoneDefaultScale = collider.bounds.size.y / realHeight ;

		ZoneScale = ZoneDefaultScale;
//		Debug.Log ("ZoneDefaultScale:"+ZoneScale);
		}

	public void RecalculateScale(){	//calculating the scale needed to fit ALL the cards in the zone
		//Debug.Log ("recalculating scale");
		float realHeight = MainMenu.ColliderHeight;
		float realWidth = MainMenu.ColliderWidth;
		
		bool arrangevertical = false;
		
		if (RotateDegrees == 90 || RotateDegrees == -90) { // only supporting these rotations for now
			realWidth = realHeight;
			realHeight = MainMenu.ColliderWidth;
		}

		
		if (collider.bounds.size.y > collider.bounds.size.x)	arrangevertical = true;

		if (arrangevertical) ZoneScale = collider.bounds.size.y / (realHeight*cards.Count + minspacing*(cards.Count-1)) ;
		else 	ZoneScale = collider.bounds.size.x / (realWidth*cards.Count + (realWidth*0.65f)*(cards.Count-1)) ; 

//		Debug.Log ("realwidth:"+realWidth+"collider.bounds.size.x:"+collider.bounds.size.x+"cards.Count:"+cards.Count);

		if (ZoneDefaultScale < ZoneScale)
						ZoneScale = ZoneDefaultScale;
		//Debug.Log ("ZoneScale:"+ZoneScale);

		foreach (card foundcard in cards)
						foundcard.transform.localScale = new Vector3 (ZoneScale, ZoneScale, 1f);



	}

	void Start () {	


		hl_all_texture = hl_mouseover_texture = new Texture2D(1, 1);
		
		hl_all_texture.SetPixel(0,0,new Color(Color.cyan.r, Color.cyan.g, Color.green.b, 0.2f));
		hl_mouseover_texture.SetPixel(0,0,new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.35f));

		hl_all_texture.Apply();
		hl_mouseover_texture.Apply();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool CanPlace(bool AI=false)
	{
		if (!UseSlots) return true;
			else if (FirstAvailableSlot (AI)) return true;
		return false;
	}

	public Transform FirstAvailableSlot(bool AI=false, Slot initialslot = null)
	{

		foreach (Transform child in transform) 
		{
			Slot foundslot = child.GetComponent<Slot>();
			if (foundslot != false)
			{

				if (dbzone.Name == "Grid" && initialslot == null ) //inital placement
				{
					if (foundslot.HeroIsAdjacent(AI) && child.childCount == 0) return child; //initial placement
				}
				else if (dbzone.Name == "Grid" && foundslot.IsAdjacent(initialslot) && child.childCount == 0) return child; //movement
						else if (child.childCount == 0 || StackAllInOneSlot) return child; //if there are no cards in this slot or we stack the cards
			}
		}
		return null;
	}

	public void AddCard(card card_to_add)	//when the card enters the zone, rotate and resize it
	{
	

		Debug.Log ("adding card:"+card_to_add.Name +"to zone: "+dbzone.Name+", Belongs to player: " +  card_to_add.ControlledByPlayer);
		//if (card_to_add.collider != null ) card_to_add.collider.enabled = false;
		if (card_to_add.IsZoomed) card_to_add.UnZoom();


		if (card_to_add.faceDown && !FaceDown) { //it it was face down and the zone has cards face up, make the card face up again 
			card_to_add.FaceUp();
		}

		if (!Invisible && !card_to_add.GetComponent<SpriteRenderer> ()) //if no sprite renderer was added, time to add it now 
			playerDeck.pD.AddArtAndText(card_to_add);

		cards.Add(card_to_add);
		if (FaceDown && card_to_add.renderer) {
					card_to_add.FaceDown();
				}

	
		 if (UseSlots) 
		{
			

			if (PlayerCanChooseSlot) 
			{
				if (slot_to_use!=null) { Debug.Log("slot was chosen before"); ScaleAndPlaceIntoSlot(card_to_add); }

					else if (card_to_add.ControlledByPlayer) //player, grid or no grid
						 StartCoroutine(ChooseSlotAndPlace(card_to_add));
							
							
								
								else { //enemy
					
									if (card_to_add.transform.parent!=null && card_to_add.transform.parent.IsChildOf(transform))  
										 slot_to_use = FirstAvailableSlot(card_to_add.transform.parent.GetComponent<Slot>()); //movement
												else slot_to_use = FirstAvailableSlot(true); //initial placement
										
									ScaleAndPlaceIntoSlot(card_to_add);
									}
			}

			else if (StackAllInOneSlot)
			{
				slot_to_use = FirstAvailableSlot();
				int FrameSortingOrder;
				int ArtSortingOrder;

//				Debug.Log("cards stacked in slot already:" + FirstAvailableSlot().childCount );

				FrameSortingOrder = (slot_to_use.childCount+1)*2;
				ArtSortingOrder = (slot_to_use.childCount+1)*2 - 1;

				card_to_add.renderer.sortingOrder = FrameSortingOrder;
				foreach (Transform child in card_to_add.transform) child.renderer.sortingOrder = FrameSortingOrder;
				card_to_add.transform.Find ("CardArt").renderer.sortingOrder = ArtSortingOrder;

				foreach (Transform child in slot_to_use) child.renderer.collider2D.enabled = false; // disabling the collider of the underneath cards so they don't cause glitches OnMouseOver etc
				ScaleAndPlaceIntoSlot(card_to_add);
			}

			else	{ //normal slot, choosing disabled
			
			slot_to_use = FirstAvailableSlot();
			ScaleAndPlaceIntoSlot(card_to_add);
			
			}


		}
		else //not using slots
		{
			card_to_add.transform.Rotate(0, 0, RotateDegrees); 
			RecalculateScale();
//			Debug.Log ("gonna set scale to:"+ZoneScale);
			card_to_add.transform.localScale = new Vector3 (ZoneScale, ZoneScale, 1f);

			RearrangeCards();

			TriggerOnEnter(card_to_add);

		}

		foreach (card foundcard in Player.cards_in_game)
			foundcard.checked_for_highlight = false;
		
		foreach (card foundcard in Player.cards_in_hand)
			foundcard.checked_for_highlight = false;
	}


	public IEnumerator ChooseSlotAndPlace(card card_to_place) //not used for initial creature placement 
	{	
		Player.ActionCancelled = false;	
		bool for_movement = false;
		if (card_to_place.transform.parent!=null) for_movement = (card_to_place.transform.parent.IsChildOf(transform)); //if the card is already in this zone

		//card_to_place.Hide ();
				
		Player.targets.Clear ();
		Player.NeedTarget = 400;
		PlayerIsChoosingASlot = true;
		if (MainMenu.TCGMaker.core.UseGrid) 
			{
				if (for_movement) HighlightSlotsForMovement(card_to_place.transform.parent.GetComponent<Slot>());
						else HighlightAvailableSlots(); //initial placement
			}

		while (PlayerIsChoosingASlot && !Player.ActionCancelled)
				yield return new WaitForSeconds (0.2f);

		if (!Player.ActionCancelled) { 
						Debug.Log("player has chosen a slot!");
						if (for_movement) card_to_place.MovedThisTurn++;
						
						slot_to_use = Player.targets[0].transform;
						
						if (MainMenu.IsMulti) Player.SendTargets ();
						
						ScaleAndPlaceIntoSlot (card_to_place);
				}
		else {
			PlayerIsChoosingASlot = false;
			RemoveHighlightedSlots(); //placement got cancelled
		}


		 //card_to_place.Show ();
	}

	IEnumerator CheckIfCanPlaceInZone(card card_to_place) //used for initial creature placement
	{	
		Debug.Log ("checking if can place in zone: "+name);
		Player.ActionCancelled = false;	
		
		if (!FirstAvailableSlot ())
		{
			Player.Warning = "You don't have enough slots available!";
			Player.ActionCancelled = true;
		} 
		else if (UseSlots) {	
			Debug.Log ("The zone uses slots");

			if (PlayerCanChooseSlot) {	//if there is an available slot
				
				//card_to_place.Hide ();
				
				Player.targets.Clear ();
				Player.NeedTarget = 400;
				PlayerIsChoosingASlot = true;

				if (MainMenu.TCGMaker.core.UseGrid) 
				{
					if (card_to_place.transform.parent==null) HighlightAvailableSlots();
						else if (card_to_place.transform.parent.IsChildOf(transform))	//if the card is already in this zone
							HighlightSlotsForMovement(card_to_place.transform.parent.GetComponent<Slot>());
								else HighlightAvailableSlots();
				}

				while (PlayerIsChoosingASlot && !Player.ActionCancelled)
					yield return new WaitForSeconds (0.2f);
				if (!Player.ActionCancelled) 
				{
					slot_to_use = Player.targets[0].transform;
					if (MainMenu.IsMulti) Player.SendTargets ();
				}
				else {
					PlayerIsChoosingASlot = false;
					RemoveHighlightedSlots(); //placement got cancelled
				}
			
				//card_to_place.Show ();
			}
			
			
		}
		Player.WaitForCheck = false;
	}

	void ScaleAndPlaceIntoSlot(card card_to_add) {
		if (slot_to_use != null) 
		{
			if (card_to_add.transform.parent != null) //if the card already was in some slot, enable that slot's collider again
				card_to_add.transform.parent.gameObject.layer = 0; //default
			else TriggerOnEnter(card_to_add);				

			if (card_to_add.IsZoomed) card_to_add.UnZoom();
			float realHeight = MainMenu.ColliderHeight;
			float realWidth = MainMenu.ColliderWidth;
			
			float ScaleForSlot = slot_to_use.collider.bounds.size.y / realHeight / slot_to_use.transform.localScale.y;
			if (ScaleForSlot > slot_to_use.collider.bounds.size.x / realWidth) ScaleForSlot = slot_to_use.collider.bounds.size.x / realWidth; //if we need to scale more to fit the width than to fit the height
//			Debug.Log("gonna apply scale for slot: "+ ScaleForSlot);

			card_to_add.transform.position = slot_to_use.position;
			card_to_add.transform.parent = slot_to_use; //putting the card into the first available slot
			card_to_add.transform.localScale = new Vector3 (ScaleForSlot, ScaleForSlot, 1f);

			slot_to_use.gameObject.layer = 2; //ignore raycast 



			if (card_to_add.Hero)
				if (Player.cards_in_game.Contains(card_to_add))
					GameObject.FindWithTag("Player").GetComponent<Player>().hero_slot = slot_to_use.GetComponent<Slot>();
				else GameObject.FindWithTag("Enemy").GetComponent<Enemy>().hero_slot = slot_to_use.GetComponent<Slot>();

			slot_to_use = null;
		}
//		Debug.Log("local scale: "+card_to_add.transform.localScale);
	

	}

	void TriggerOnEnter(card card_to_add)
	{   if (Player.CreaturesZone == this && card_to_add.ControlledByPlayer)	card_to_add.abilities.OnEnter();  // if this is creatures' zone
			else if (Enemy.CreaturesZone == this) card_to_add.abilities.OnEnter(true); //true indicates it's the enemy, not the player, who is playing this creature
	}



	void OnGUI()
	{
		if (PlayerIsChoosingASlot) //if player needs to choose a slot 
		{
			if (!MainMenu.TCGMaker.core.UseGrid)  HighlightAvailableSlots();

			GUI.Label (new Rect (400, 5, 200, 30), "Choose a slot");
		}
	}

	public void RemoveHighlightedSlots()
	{
		foreach (Transform foundslot in transform)
		{
			if (foundslot.GetComponent<MeshRenderer>()) foundslot.renderer.material = playerDeck.pD.slot_regular;
								
			foundslot.GetComponent<Slot>().highlighted = false;

				
			
		}
	}

	void HighlightAvailableSlots()
	{
//		Debug.Log("highlighting available slots");
		foreach (Transform child in transform)  {
			if (child.GetComponent<Slot>() != false) 
				if (child.childCount == 0 || StackAllInOneSlot) //if the slot is empty or the cards can be stacked in it
					{ //if the slot is available, highlight it
						

						if (MainMenu.TCGMaker.core.UseGrid) //highlighting grid slots
						{
							if (child.GetComponent<Slot>().HeroIsAdjacent())
							
								child.GetComponent<Slot>().Highlight();

						}
						else child.GetComponent<Slot>().Highlight();				

					}
		}
	}

	public void HighlightSlotsForMovement(Slot current_slot)
	{
		Debug.Log("highlighting available slots for movement");

		foreach (Transform child in transform)  {
			if (child.GetComponent<Slot>() != false) 
				if (child.childCount == 0 && child.GetComponent<Slot>().IsAdjacent(current_slot) ) //if the slot is empty and adjacent
			
						child.GetComponent<Slot>().Highlight();
		}
	}

	public void RemoveCard(card card_to_remove)	//set the rotation and scale back to normal
	{
	
//		Debug.Log("removing card from zone:" + name);

		cards.Remove(card_to_remove);

		if (UseSlots) //emptying the parent slot and making its collider enabled again
		{
			if (card_to_remove.IsTurned && !MainMenu.TCGMaker.core.UseGrid) 
				card_to_remove.transform.parent.Rotate(0, 0, -MainMenu.TCGMaker.core.OptionTurnDegrees);  //unturn the slot

			card_to_remove.transform.parent.gameObject.layer = 0; //default

			card_to_remove.transform.parent = null;
		}
		else {
		

			card_to_remove.transform.Rotate(0, 0, -RotateDegrees); 
			
			card_to_remove.transform.localScale = new Vector3 (1f, 1f, 1f);

			if (ZoneScale < ZoneDefaultScale) RecalculateScale (); //since we have more space now
			RearrangeCards();
		
		}
	}

	public static bool OppositeSlotHasCard(card ourcard)
	{
		Zone thiszone;
		Slot thisslot;
		if (!ourcard.transform.parent.parent.GetComponent<Zone> ()) 			
				Debug.LogWarning ("the card is not in a zone - can't find the opposite slot");

		else {	//if this card is in a zone
			thisslot = ourcard.transform.parent.GetComponent<Slot>();
			thiszone = thisslot.transform.parent.GetComponent<Zone>();

			foreach (Zone foundzone in playerDeck.pD.zones) 
				if (foundzone.zone_id == thiszone.zone_id && foundzone.BelongsToPlayer != thiszone.BelongsToPlayer )	//if this is the opposite zone
						foreach (Transform child in foundzone.transform)
							if (child.GetComponent<Slot>()) 
								if (thisslot.number_in_zone == child.GetComponent<Slot>().number_in_zone) 	//if this is the opposite slot
									if (child.GetComponent<Slot>().transform.childCount > 0) 
										return true;
		}

		return false;
	}

	public static void MoveToZone(card ourcard, int zoneid, bool players_zone = true)
	{
		foreach (Zone foundzone in playerDeck.pD.zones) 
			if (foundzone.zone_id == zoneid && foundzone.BelongsToPlayer == players_zone)
				foundzone.AddCard(ourcard);
	}

	public IEnumerator DrawCards(int param)
	{
		Debug.Log ("gonna draw "+param+" cards");
		for (int i=0; i<param; i++) {
			
			DrawCard();
			yield return new WaitForSeconds(0.5f);
			
		}

		
		
	}

	 public  void DrawCard() {
		//Debug.Log("drawing a card");
		//draw a card

		if (BelongsToPlayer)
		{
			if (playerDeck.pD.Deck.Count <= 0)	return;			//no cards to draw
			if (Player.HandZone == this && Player.cards_in_hand.Count == MainMenu.TCGMaker.core.MaxHandSize) { Player.GraveyardZone.DrawCard(); return; }	
		}

		else {
			if (Enemy.Deck.Count <= 0)	return;			//no cards to draw
			if (Enemy.HandZone == this && Enemy.cards_in_hand.Count == MainMenu.TCGMaker.core.MaxHandSize) { Enemy.GraveyardZone.DrawCard(); return; }	
		}

				GameObject new_card_obj = new GameObject ();

				new_card_obj.AddComponent ("AudioSource");

				card new_card = new_card_obj.AddComponent ("card") as card; //adding card script

									
				if (BelongsToPlayer) {

						GameObject.FindWithTag ("Player").SendMessage ("TakesCardSFX");

						new_card.Index = playerDeck.pD.Deck.ElementAt (0);
						
						playerDeck.pD.Deck.RemoveAt (0);
					
				} else {
						new_card.Index = Enemy.Deck.ElementAt (0);
						Enemy.Deck.RemoveAt (0);
					}

			
				//Debug.Log ("index:" + new_card.Index);
				playerDeck.pD.LoadCardStats (new_card); //the card is still invisible at this point
				
				if (BelongsToPlayer) {
		
						//new_card_obj.GetComponent<AudioSource> ().clip = new_card.sfxEntry;	//play card entry audio

						new_card.ControlledByPlayer = true;

				} else {
						Enemy.CardsInHand += 1;	//drawing a card for the enemy
						Enemy.NumberOfCardsInDeck--;
				}
					
							
		AddCard (new_card);

		if (MainMenu.IsMulti) {	//making the card ids and sending the updated hand and deck card count
			
			new_card.id_ingame = System.Int32.Parse(playerDeck.pD.first_or_second.ToString() + last_id.ToString()); //makes card ids like 1<cardid>, 2<cardid> depending on whether the player is first or second (so they dont have the same ids)
			last_id++;
			
			Debug.Log("ismulti");
			Logic.ScenePhotonView.RPC("UpdateEnemyCardsInHand", PhotonTargets.Others, Player.cards_in_hand.Count); //sending hand cards to the other player
			Logic.ScenePhotonView.RPC("UpdateEnemyCardsInDeck", PhotonTargets.Others, playerDeck.pD.Deck.Count); //sending hand cards to the other player
			}
		}
	public void RearrangeCards() {
		//Debug.Log ("rearranging cards, zone" + gameObject.name);
		//float spacing = 0.5f; //we don't try to fill all the space in the zone, this is the max distance we will use between cards


		//float freespace = 0f;

		bool arrangevertical = false; 

//		float DegreesInRadians = RotateDegrees * Mathf.Deg2Rad;


		if (collider.bounds.size.y > collider.bounds.size.x)
		
		arrangevertical = true;

		float realHeight = MainMenu.ColliderHeight * ZoneScale;
		float realWidth = MainMenu.ColliderWidth * ZoneScale;

		float spacing = realWidth *0.65f;

		if ((RotateDegrees > 45 && RotateDegrees < 135) || (RotateDegrees < -45 && RotateDegrees > -135)) { 
			realWidth = realHeight;
			realHeight = MainMenu.ColliderWidth * ZoneScale;
		}


		//if (arrangevertical) freespace = (collider.bounds.size.y - (cards.Count * realHeight));
		//else freespace = (collider.bounds.size.x - (cards.Count * realWidth));


		float startX = 0f;
		float startY = 0f;
		startX = collider.bounds.min.x;
		startY = collider.bounds.max.y;
		//collider.enabled = false;
		startX += realWidth / 2; //card width/2
		startY -= realHeight / 2; //card height/2
	

		int i = 0;
		foreach (card foundcard in cards) {
		//	Debug.Log("arranging card:"+foundcard.Name+"localscalex:"+foundcard.transform.localScale.x);
			if (foundcard.IsZoomed) { foundcard.IsZoomed = false; }

			//foundcard.transform.position = new Vector3 (-5.79f + i * density, -1.9f, 0f); //don't change z
			if (arrangevertical)
				foundcard.transform.position = new Vector3 (startX, startY - i * (realHeight + spacing), 0f); //don't change z
			else
				foundcard.transform.position = new Vector3 (startX + i * (realWidth + spacing), startY, 0f); 
		i++;
//			Debug.Log("finished arranging card:"+foundcard.Name);
				}
		}

	public static Vector2 ScreenToGUIPoint (Vector2 screenPoint) //because GUIUtility.ScreenToGUIPoint doesn't seem to work
	{
		Vector2 guiPoint = screenPoint;
		guiPoint.y = Screen.height-guiPoint.y;
		return guiPoint;
	}

	public static Rect GUIRectWithObject(GameObject go)
	{
		Vector3 cen = go.collider.bounds.center;
		Vector3 ext = go.collider.bounds.extents;
		Vector2[] extentPoints = new Vector2[8]
		{
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z))),
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z))),
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z))),
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z))),
			
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z))),
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z))),
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z))),
			ScreenToGUIPoint(Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z)))
		};
		
		Vector2 min = extentPoints[0];
		Vector2 max = extentPoints[0];
		
		foreach(Vector2 v in extentPoints)
		{
			min = Vector2.Min(min, v);
			max = Vector2.Max(max, v);
		}
		
		return new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
	}
}
