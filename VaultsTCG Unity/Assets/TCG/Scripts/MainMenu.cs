
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class MainMenu : MonoBehaviour
{
	public  bool PlayOffline = true;

	public static int userid;

	public static TCGData currentsettings;
	public static TCGData TCGMaker
	{
		get
		{
			if (currentsettings == null)
				currentsettings =  (TCGData)Resources.Load("TCGData", typeof(TCGData));

			return currentsettings;
		}
	}

	public static bool DownloadedPlayerDeck = false;
	public static bool DownloadedPlayerCollection = false;
	public static bool FirstLoadMenu = true;

	public static bool IsMulti = true;
//    private Vector2 scrollPos = Vector2.zero;

//    private bool connectFailed = false;

	public static GUIStyle mystyle;

	public static bool CollectionNeedsUpdate = false;

	public static readonly string SceneNameMainMenu = "MainMenuScene";

    public static readonly string SceneNameMenu = "LobbyScene";

    public static readonly string SceneNameGame = "GameScene";

	public static readonly string SceneNameEditDeck = "EditDeckScene";

	public static string wwwtext="test";

	public static string username = "";
	private string pswd = ""; 
	private string repass = ""; 
	private string email = ""; 
	private string url = "http://losange-vision.com/registration.php"; 
	private string url_login = "http://losange-vision.com/login.php"; 
	private string url_latest_cards = "http://losange-vision.com/latestcards.php"; 
	private string url_player_deck = "http://losange-vision.com/playerdecks.php"; 
	private string url_player_collection = "http://losange-vision.com/playercollection.php"; 
	public static string url_update_deck = "http://losange-vision.com/updatedeck.php"; 



	private Hashtable[] promocards; 
	public static List <string> promo_prices= new List<string>();
	public static List <Vector3> promo_vector= new List<Vector3>();

	public static string deckstring, collectionstring;

	public static bool LoggedIn = false;

	private bool register = false;

	//public static bool DeckAndCollectionFromOnlineDB = false; //this will change if the user logs in

	public static string message;

	public static float ColliderWidth, ColliderHeight;


    public void Awake()
    {
	
		CardTemplate instance = CardTemplate.Instance; 

    }


	public void Start()
	{
		if (FirstLoadMenu) {
			Debug.Log ("firstload");
						

			playerDeck.pD.LoadPlayerDeckOffline ();
						
				} else {

			if (LoggedIn) 
			{
				Currency.GetCurrency();
				DoGetLatestCards();
				DoGetPlayerDeck();
				DoGetPlayerCollection();
			}
				}
		Debug.Log ("is logged in = " + LoggedIn);
		FirstLoadMenu = false;
	}

	public void Update()
	{
		if (CollectionNeedsUpdate) DoGetPlayerCollection();
			CollectionNeedsUpdate = false;
	}


	public static Texture2D SpriteToTexture(Sprite sprite)
		
	{
		Texture2D croppedTexture = new Texture2D ((int)sprite.rect.width, (int)sprite.rect.height);
		
		Color[] pixels = sprite.texture.GetPixels (0, 
		                                           0, 
		                                           (int)sprite.rect.width, 
		                                           (int)sprite.rect.height);
		
		croppedTexture.SetPixels (pixels);
		croppedTexture.Apply ();
		return croppedTexture;
		
	}

	static public Hashtable[] ParsePromocards (string cardsstring)
	{
		
		string[] lines = cardsstring.Split("\n"[0]); 
		string[] linearray;
		// finds the number of cards
		
		Hashtable[] output = new Hashtable[lines.Length-1];
		for (int i = 0; i < (lines.Length-1); i++)
		{

			output[i]=new Hashtable();
			linearray = lines[i].Split(","[0]);
			output[i]["id"] = linearray[0];
			Debug.Log("linearray[0]"+ linearray[0]);
			Debug.Log("linearray[1]"+ linearray[1]);
			output[i]["cost"] = linearray[1]; 

		}
		return output;
	}

	public void DoGetPlayerCollection()
	{
		WWWForm form = new WWWForm();
		
		form.AddField("userid", userid);
		WWW w = new WWW(url_player_collection, form);
		Debug.Log("downloading collection for user id: "+ userid);
		StartCoroutine(GetPlayerCollection(w));
		
		
	}
	
	IEnumerator GetPlayerCollection( WWW w)
	{
		yield return w;
		if (w.error ==null)
		{
			collectionstring = w.text;
			Debug.Log("downloaded collection: "+ w.text);
			
			DownloadedPlayerCollection = true;
			
			playerDeck.pD.Collection = playerDeck.pD.LoadDeck(collectionstring);
		}
		else message +="ERROR:" +w.error + "\n";
		Debug.Log(message);
	}

	public void DoGetPlayerDeck()
	{
		WWWForm form = new WWWForm();
		
		form.AddField("userid", userid);
		WWW w = new WWW(url_player_deck, form);
		Debug.Log("downloading deck for user id: "+ userid);
		StartCoroutine(GetPlayerDeck(w));
		
		
	}

	IEnumerator GetPlayerDeck( WWW w)
	{
		yield return w;
		if (w.error ==null)
		{
			deckstring = w.text;
			Debug.Log("downloaded deck: "+ w.text);
			
			DownloadedPlayerDeck = true;

			playerDeck.pD.Deck =  playerDeck.pD.LoadDeck(deckstring);
			
		
		}
		else message +="ERROR:" +w.error + "\n";
		Debug.Log(message);
	}


	public void DoGetLatestCards()
	{
		
		WWW w = new WWW(url_latest_cards);
		
		StartCoroutine(GetLatestCards(w));
		

	}


	IEnumerator GetLatestCards( WWW w)
	{
		yield return w;
		if (w.error ==null)
		{
			promo_prices.Clear();
			promo_vector.Clear();

			promocards = ParsePromocards(w.text);
			int Index;
			int i=0;
			foreach (Hashtable foundcard in promocards)
			{
			
			Index = System.Int32.Parse(foundcard["id"].ToString());

			Debug.Log("got promo card index:" + Index);
			GameObject promo_card_obj = new GameObject ();
			card promo_card = promo_card_obj.AddComponent("card") as card; 
			promo_card.Index = Index;
			DbCard dbcard = MainMenu.TCGMaker.cards.Where(x => x.id == Index).SingleOrDefault();
			if (dbcard == null) Debug.LogWarning("card not found in the new db!");

			promo_card.Type = dbcard.type;
			promo_card.Cost = dbcard.cost;
			promo_card.CardColor = dbcard.color;

				if (promo_card.IsACreature()) {
					promo_card.CreatureOffense = dbcard.offense; 
					promo_card.CreatureDefense = dbcard.defense; 
				}

			promo_card.CostInCurrency = System.Int32.Parse(foundcard["cost"].ToString());
			
			playerDeck.pD.AddArtAndText(promo_card);

			promo_card.transform.position = new Vector3 (3.79f + 2.5f*i, -0.9f, 0f); 

			promo_prices.Add(foundcard["cost"].ToString());
			promo_vector.Add(Camera.main.WorldToScreenPoint(promo_card.transform.position));

			i++;
			}

		}
		else message +="ERROR:" +w.error + "\n";
		Debug.Log(message);
	}


	public void DoRegister()
	{
		WWWForm form = new WWWForm();

		form.AddField("username", username);
		form.AddField("password", pswd);
		form.AddField("email", email);
	

		WWW w = new WWW(url, form);

		StartCoroutine(RegisterPlayer(w));

	}

	IEnumerator RegisterPlayer( WWW w)
	{
		yield return w;
		if (w.error ==null)
		{
			message +=w.text;
	
		}
		else message +="ERROR:" +w.error + "\n";
		Debug.Log(message);
	}

	public void DoLogin()
	{
		WWWForm form = new WWWForm(); 
		
		form.AddField("username", username);
		form.AddField("password", pswd);
		
		WWW w = new WWW(url_login, form);
		
		StartCoroutine(Login(w));
	}



	 IEnumerator Login (WWW w)
	{
		yield return w;
		if (w.error ==null)
		{
			if (w.text.Contains("login-SUCCESS")) {
				userid = System.Int32.Parse(Regex.Match(w.text,"(?<=login-SUCCESS)[0-9]+").ToString());
				message ="Logged in!";
				LoggedIn = true;
				Currency.GetCurrency();
				DoGetLatestCards();
			//	DeckAndCollectionFromOnlineDB = true;
				DoGetPlayerDeck();
				DoGetPlayerCollection();

			}
			else	message +=w.text;
		}
		else message +="ERROR:" +w.error + "\n";
	}
    public void OnGUI()
    {
		if (LoggedIn) {
			GUI.Label (new Rect (750, 250, 200, 20), "Latest cards (click to buy)");
			for (int i=0; i<promo_vector.Count; i++)
			{
				Vector3 p = promo_vector[i];
				GUI.Label(new Rect(p.x-30,Screen.height-p.y+70,200,30), "Price: " +promo_prices[i]);
			}

		}
		//GUILayout.Label("deck count" + playerDeck.Deck.Count.ToString());



		GUI.Label(new Rect(Screen.width / 2 -30, ((Screen.height - 350) / 2)+300, 600, 150), Currency.messagecurrency);
        GUI.skin.box.fontStyle = FontStyle.Bold;
        GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300), "TCG Maker 1.3 Demo");
        GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 250));

        GUILayout.Space(50);

		GUILayout.BeginHorizontal();

		if (message!="") GUILayout.Box(message);
		GUILayout.EndHorizontal();
	
		if (register)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Username");
			username = GUILayout.TextField(username);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Password");
			pswd = GUILayout.TextField(pswd);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Email");
			email = GUILayout.TextField(email);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Repeat Password");
			repass = GUILayout.TextField(repass);
			GUILayout.EndHorizontal();

		

			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Back")) register=false;
			
			if (GUILayout.Button("Register"))
			{
				message ="";
				if (username=="" || pswd=="" || repass=="" || email=="") message+="Please enter all the fields \n";
				else if (pswd==repass) DoRegister();       			  // Registration
				else message+="Your password does not match \n";
			}
			GUILayout.EndHorizontal();
			
		}
		else if ((LoggedIn)||(PlayOffline==true)) {
		

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Single Game", GUILayout.Width(200)))
			{
				
				IsMulti = false;
				Application.LoadLevel(SceneNameGame);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			

			if (!PlayOffline)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Multiplayer Game", GUILayout.Width(200)))
			{
				IsMulti = true;
				Application.LoadLevel(SceneNameMenu);
			}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Change deck", GUILayout.Width(200)))
			{
				
				Application.LoadLevel(SceneNameEditDeck);
				
				
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("User:");

			username = GUILayout.TextField(username,GUILayout.Width(150));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Password:");
			//GUILayout.FlexibleSpace();
			pswd = GUILayout.PasswordField(pswd, "*"[0], GUILayout.Width(150));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Login")) {
					if (username=="" || pswd=="") message+="Please enter all the fields \n";
					else DoLogin();           // Login
				
				}
			
			if (GUILayout.Button("Register"))	{ register=true; } 
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			}

       







        GUILayout.EndArea();
    }


}
