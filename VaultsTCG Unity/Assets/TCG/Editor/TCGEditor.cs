using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;




public class TCGEditor : EditorWindow  {
	enum sections {coresettings, cardsdb, combat, stats};
	sections current_section = sections.coresettings;

	static CardsDB cards_db = new CardsDB();

	Vector2 scrollPos = Vector2.zero;

	private string stat_name = "";
	private string type_name = "";

	private const string SettingsAssetFile = "Assets/TCG/Resources/TCGData.asset";
	private static DBZone viewed_zone;



	private bool doRepaint = false;
	private string color_name;

	private int rows = 3, columns = 3;
	private float offset = 0.2f;

	//	private List<NodeComponent> comps_to_add = new List<NodeComponent>();

	public static TCGData currentSettings;
	public static TCGData unchangedSettings;
	//public static TCGData currentSettings_unchanged;

	#region Unity MenuItem
	[MenuItem("Window/TCG Maker")]


	public static void OpenTCGEditor()
	{
		TCGEditor E = EditorWindow.CreateInstance<TCGEditor>();
		E.Show();
	}
	#endregion Unity MenuItem
	public static void ReLoadCurrentSettings()
	{
		Debug.Log ("starting to reload");
		// warns developers if there are more than one settings files in resources folders. first will be used.
		UnityEngine.Object[] settingFiles = Resources.LoadAll("TCGData", typeof(TCGData)); //finding all files with that name
		if (settingFiles != null && settingFiles.Length > 0)
		{
			Debug.Log ("found some settings");
			unchangedSettings = (TCGData)settingFiles[0];
			currentSettings = (TCGData)Instantiate(unchangedSettings);
			if (Current.cards != null)cards_db.cards = Current.cards;

			Debug.Log("current.cards count: "+  Current.cards.Count);
			if (settingFiles.Length > 1)
			{
				Debug.LogWarning("more than one settings file");
			}

		}

	}




	public static TCGData Current
	{
		get
		{
			if (currentSettings == null)
			{
							
				// try to load settings from file
				ReLoadCurrentSettings();
				
				// if still not loaded, create one
				if (currentSettings == null)
				{
				

					unchangedSettings = (TCGData)ScriptableObject.CreateInstance("TCGData");
					currentSettings = (TCGData)Instantiate(unchangedSettings);
					if (currentSettings != null)
					{
						AssetDatabase.CreateAsset(unchangedSettings, SettingsAssetFile);
						Debug.Log("no file found, creating new file");
					}
					else {}
				}
//				AddCustomStatsToCombat();
			}

			if (currentSettings.core.zones != null && currentSettings.core.zones.Count == 0)currentSettings.core.AddDefaultZones();
			if (currentSettings.core.colors != null && currentSettings.core.colors.Count == 0)currentSettings.core.AddDefaultColors();

			return currentSettings;
		}
		
		protected set
		{
			currentSettings = value;
		}
	}


	void OnGUI(){

		//Debug.Log("id: " + GUIUtility.keyboardControl);
		if  (Event.current.keyCode == KeyCode.Return) { GUIUtility.keyboardControl = 0; Event.current.Use(); }//lose keyboard focus
//		Current.test = EditorGUILayout.IntField (Current.test, GUILayout.Width (50));
		GUILayout.BeginVertical (); //main
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button("Core Settings")) current_section = sections.coresettings; 
		else if (GUILayout.Button("Card Stats")) current_section = sections.stats;
		//else if (GUILayout.Button("Combat Rules")) current_section = sections.combat;
		else if (GUILayout.Button("Cards Database")) { 
			MainMenu.currentsettings = Current;
			cards_db.UpdateCreatures(cards_db.cards);
			current_section = sections.cardsdb; 
		}
		GUILayout.EndHorizontal ();
	
		if  (Event.current.keyCode == KeyCode.F4) Current.core.UseGrid = true; 

		switch (current_section)
		{
		case sections.cardsdb : 
			cards_db.DrawGUI();
			break;
		case sections.coresettings : 
			DrawCoreOptions(Current.core);
			break;
		case sections.combat :
//			DrawCombat(Current.combat);
			break;
		case sections.stats :
			DrawStats(Current.stats);
			break;
		}

		GUILayout.BeginHorizontal ();
		if (GUILayout.Button("Save Game Settings")) SaveTCGData(); 
		if (GUILayout.Button("Reload Game Settings")) LoadTCGData(); 
		GUILayout.EndHorizontal ();

		GUILayout.EndScrollView();
		GUILayout.EndVertical ();
	}

	void LoadTCGData(){

		currentSettings = (TCGData)Instantiate(unchangedSettings); //works
//		AddCustomStatsToCombat();
		if (currentSettings.cards != null) cards_db.cards = currentSettings.cards;
			else cards_db.cards = new List<DbCard>();

		bool found = false;

		if (viewed_zone != null) {
						foreach (DBZone dbz in Current.core.zones) {
								//Debug.Log("dbz name:"+dbz.Name+"viewed name:"+viewed_zone.Name);
								if (dbz.Name == viewed_zone.Name) {
										found = true;
										viewed_zone = dbz;
										break;
								} 
							}
						if (!found)	viewed_zone = null;
				}
		cards_db.viewed_card = null;

	}

	void SaveTCGData() {



		Current.core.enemy_zones.Clear ();
		foreach (DBZone foundzone in Current.core.zones) {
						if (!foundzone.Shared) {
						DBZone newzone = new DBZone(foundzone);
						newzone.Name = "Enemy " + newzone.Name.ToLower();
						Current.core.enemy_zones.Add (newzone);
						}
				}
		//CombatCodeGenerator.combat_rules = Current.combat;
		//CombatCodeGenerator.WriteCodeFile();

		Current.cards = cards_db.cards;

		unchangedSettings = currentSettings;	
	
		AssetDatabase.CreateAsset(currentSettings, SettingsAssetFile); //trying to rewrite file

		currentSettings = (TCGData)Instantiate(unchangedSettings);  //we need this
//		AddCustomStatsToCombat();
		Debug.Log("unchanged test"+unchangedSettings.test);



	}





	public void DrawCoreOptions(CoreOptions o)
	{	
		EditorGUIUtility.labelWidth = 250;

		o.OptionStartingLife = EditorGUILayout.IntField ("Starting life:", o.OptionStartingLife, GUILayout.MaxWidth(400));

		o.MaxHandSize = EditorGUILayout.IntField ("Maximum hand size:", o.MaxHandSize, GUILayout.MaxWidth(400));

		o.OptionFirstTurnSickness = EditorGUILayout.Toggle ("First turn sickness:", o.OptionFirstTurnSickness);

		o.OptionGraveyard = EditorGUILayout.Toggle ("Graveyard:",o.OptionGraveyard);

		o.OptionManaDoesntReset = EditorGUILayout.Toggle ("Mana doesn't reset each turn:", o.OptionManaDoesntReset);


		o.OptionManaAutoIncrementsEachTurn = EditorGUILayout.Toggle ("Mana autoincrements each turn:", o.OptionManaAutoIncrementsEachTurn);

		if (o.OptionManaAutoIncrementsEachTurn) o.OptionManaMaxIncrement = EditorGUILayout.IntField ("Max mana gain in a turn:", o.OptionManaMaxIncrement, GUILayout.MaxWidth(400));




		o.OptionCantAttackPlayerThatHasHeroes = EditorGUILayout.Toggle ("Can't attack a player if he has a hero:", o.OptionCantAttackPlayerThatHasHeroes);

		o.OptionGameLostIfHeroDead = EditorGUILayout.Toggle ("Lose condition: if the player's hero is dead", o.OptionGameLostIfHeroDead);

		o.OptionRetaliate = EditorGUILayout.Toggle ("Defending creature deals damage back:", o.OptionRetaliate);
		if (!o.OptionRetaliate) {
			o.OptionKillOrDoNothing = EditorGUILayout.Toggle ("Kill opponent or deal no damage:", o.OptionKillOrDoNothing);
		}
	


		o.OptionOneCombatStatForCreatures = EditorGUILayout.Toggle ("Attack and defense are a single stat:",o.OptionOneCombatStatForCreatures);




		o.OptionGameMusic = EditorGUILayout.Toggle ("Play music in game:", o.OptionGameMusic);

		o.OptionPlayerTurnPopup = EditorGUILayout.Toggle ("Display \"your turn\" popup:", o.OptionPlayerTurnPopup);


		o.OptionTurnDegrees = EditorGUILayout.FloatField("Turned cards' angle:", o.OptionTurnDegrees, GUILayout.MaxWidth(400));

		o.UseCardColors = EditorGUILayout.Toggle ("Use card colors", o.UseCardColors);


		if (o.UseCardColors) {
						o.UseManaColors = EditorGUILayout.Toggle ("Use colors in mana costs", o.UseManaColors);

						GUI.skin.label.fontStyle = FontStyle.Bold;
						GUILayout.BeginVertical ("box");
						GUILayout.Label ("Mana colors", GUILayout.Width (90));
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Name", GUILayout.Width (150));
						GUILayout.Label ("Icon", GUILayout.Width (200));
						GUILayout.EndHorizontal ();
						GUI.skin.label.fontStyle = FontStyle.Normal;

						int i = 0;
						ManaColor color_to_delete = null;

						foreach (ManaColor mcolor in o.colors) {

								GUILayout.BeginHorizontal ();
								GUILayout.Label (mcolor.name, GUILayout.Width (150));

								Sprite m_icon = EditorGUILayout.ObjectField (mcolor.icon, typeof(Sprite), false, GUILayout.Width (200)) as Sprite;

								if (m_icon != null) {
										if (!mcolor.icon_texture || m_icon != mcolor.icon)
												mcolor.icon_texture = MainMenu.SpriteToTexture (m_icon);  //prepare a new texture to display

										GUILayout.Label (mcolor.icon_texture, GUILayout.Width (50), GUILayout.Height (50));
								}
								mcolor.icon = m_icon;

								if (!mcolor.Default && GUILayout.Button ("X", GUILayout.Width (30)))
										color_to_delete = mcolor; //it's the way to do it because otherwise we'll get "collection was modified" error
								GUILayout.EndHorizontal ();
								i++;
						}

						if (color_to_delete != null) { //removing this color from all cards' and effects' costs
								o.colors.Remove (color_to_delete);

								foreach (DbCard foundcard in Current.cards) {
										if (foundcard.cost.Contains (color_to_delete))
												foundcard.cost.RemoveAll (obj => obj == color_to_delete);

										foreach (Effect foundeffect in foundcard.effects)
												if (foundeffect.cost.Contains (color_to_delete))
														foundeffect.cost.RemoveAll (obj => obj == color_to_delete);
								}
						}

						GUILayout.BeginHorizontal ();
						color_name = EditorGUILayout.TextField ("Add new color:", color_name, GUILayout.MaxWidth (500));
			
						if (GUILayout.Button ("Add", GUILayout.Width (70))) {
								o.colors.Add (new ManaColor (color_name));
								color_name = "";
						}
						GUILayout.EndHorizontal ();
						GUILayout.EndVertical (); //box end
				} else
			o.UseManaColors = false;

		//GUILayout.BeginVertical ("box");
		DrawZones (o);
		//GUILayout.EndVertical ();



	}



	public void DrawZones(CoreOptions c)
	{

		if (c.zones == null) c.zones = new List<DBZone>();
		List<DBZone> zones = c.zones;

		GUILayout.BeginHorizontal("box");
		GUILayout.BeginVertical(); //1st column: list of all zones
		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.Label("Zones", GUILayout.Width(90));
		

		if (zones.Count > 0)	//displaying the zones' table
		{
			GUI.skin.label.alignment = TextAnchor.MiddleCenter; 
			GUILayout.BeginHorizontal();
			
			GUILayout.Label("name",GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

			GUILayout.EndHorizontal();
			GUI.skin.label.fontStyle = FontStyle.Normal;
			
			for (int i = 0; i < zones.Count; i++)
			{
				DBZone currentzone = zones[i];

				if (currentzone.Name!="Grid")
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button(currentzone.Name,GUILayout.MinWidth(150), GUILayout.MaxWidth(300)))  viewed_zone = currentzone; 

										
						if (!currentzone.Default && GUILayout.Button("Delete",GUILayout.Width(100)))
								zones.RemoveAt(i);	//deleting 
						GUILayout.EndHorizontal();
					}
			}

		}
		
		if (GUILayout.Button("new zone",GUILayout.Width(100))    ) 
		{	DBZone newzone = new DBZone("New zone");

			zones.Add(newzone);
			viewed_zone = zones[zones.Count-1];

			//DBZone enemyzone = new DBZone(newzone.Name);
			//c.enemy_zones.Add(enemyzone);

		}

		GUILayout.EndVertical();
		
		GUILayout.BeginVertical();
		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.Label("Edit zone");
		GUI.skin.label.fontStyle = FontStyle.Normal;
		


		GUILayout.BeginVertical("box");


		if (viewed_zone != null)	//displaying the detailed info
		{

			GUILayout.BeginVertical();
			
			if (ViewZone(viewed_zone))
				zones.Remove(viewed_zone);	//deleting card from db
			
			GUILayout.EndVertical();
		}


		GUILayout.EndVertical();

	
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		if (currentSettings.core.UseGrid) 
			{
				currentSettings.core.UseGrid = EditorGUILayout.Toggle("Use grid", currentSettings.core.UseGrid);

				rows = EditorGUILayout.IntField("rows:", rows);
				columns = EditorGUILayout.IntField("columns:", columns);
				offset = EditorGUILayout.FloatField("distance between cells:", offset);

				if (GUILayout.Button("Create hex grid",GUILayout.Width(100))    ) 
					CreateHexGrid(rows, columns, offset);
			}
	}

	public void CreateHexGrid(int rows, int columns, float offset)
	{
		//i should add finding the right scene here

		GameObject gridzone = GameObject.Find("Zone - Grid");

		//removing the old grid:


		float hexheight = 0f;
		float hexwidth = 0f;
		float xOff = 0f;
		float yOff = 0f;

		for (int i=0; i<rows; i++)
		{
			for (int j=0; j<columns; j++)
			{
			
					GameObject new_hex = (GameObject)Instantiate(Resources.Load("Hex"));
					new_hex.transform.parent = gridzone.transform;
					new_hex.name = "Hex " + i + ", "+ j ;
					new_hex.GetComponent<Slot>().row = i;
					new_hex.GetComponent<Slot>().column = j;

					if (hexheight == 0f) { 
					  hexheight = new_hex.collider.bounds.size.y;
					  hexwidth = new_hex.collider.bounds.size.x;
					  yOff = hexheight/2; // half of the hex height
					  xOff = hexwidth*3/4; // 3/4 of the hex width
					}
					
				// Compute position on Y 
				
				float yPos = j * (yOff * 2 + offset);
				
				if (i % 2 != 0) {   // if the current line is not even
					yPos += yOff;     // extra offset of half the width on x axis
				}
				
				// Compute position on X 
				
				float xPos = i * (xOff + offset);

				new_hex.transform.position = new Vector3(xPos,   yPos,     -0.1f);

		}
	}

	}

	public bool ViewZone(DBZone z)
	{

		if (z.Default)	GUILayout.Label ("Name: " + z.Name );
			else z.Name = EditorGUILayout.TextField("Name:", z.Name, GUILayout.MaxWidth(500));

		GUILayout.BeginHorizontal();

		z.UseSlots = EditorGUILayout.Toggle ("Uses slots:",z.UseSlots);
		if (z.UseSlots) {
						GUILayout.Label ("Player can choose slot:");
						z.PlayerCanChooseSlot = EditorGUILayout.Toggle (z.PlayerCanChooseSlot);
						GUILayout.Label ("Stack all cards in one slot:");
						z.StackAllInOneSlot = EditorGUILayout.Toggle (z.StackAllInOneSlot);
		}
		GUILayout.EndHorizontal();

		z.RotateDegrees = EditorGUILayout.FloatField ("Rotate cards, degrees:", z.RotateDegrees);

		z.DrawAtTheStartOfGame = EditorGUILayout.IntField ("Draw cards at the start of the game:", z.DrawAtTheStartOfGame);

		GUILayout.BeginHorizontal();
		z.EnemyInvisible = EditorGUILayout.Toggle ("Invisible (enemy):",z.EnemyInvisible);
		z.PlayerInvisible = EditorGUILayout.Toggle ("Invisible (player):",z.PlayerInvisible);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();

		z.EnemyFaceDown = EditorGUILayout.Toggle ("Face down (enemy):", z.EnemyFaceDown);


		z.PlayerFaceDown = EditorGUILayout.Toggle ("Face down (player):", z.PlayerFaceDown);
		GUILayout.EndHorizontal();

		if (!z.Default && GUILayout.Button ("Delete zone", GUILayout.Width (100)))
			return true;




				
		return false;
	}

	public void DrawStats(Stats s) {
		s.scrollPos = GUILayout.BeginScrollView(s.scrollPos);
		GUILayout.BeginVertical("box");
		
		GUILayout.Label("List of custom int stats:", GUILayout.Width(250));
		foreach (CustomStat customstat in s.CustomInts)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(customstat.h_name, GUILayout.Width(100));
			if (GUILayout.Button ("X",  GUILayout.Width(50))) { 
				
				s.CustomInts.Remove(customstat); 
				break;
			}
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Label ("List of custom string stats:", GUILayout.Width (250));
		foreach (CustomStat customstat in s.CustomStrings)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(customstat.h_name, GUILayout.Width(100));
			if (GUILayout.Button ("X", GUILayout.Width(50))) { s.CustomStrings.Remove(customstat); break;}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		stat_name = EditorGUILayout.TextField("New stat name:", stat_name, GUILayout.MaxWidth(500));
		
		if (GUILayout.Button ("Add int stat")) {
			s.CustomInts.Add(new CustomStat(stat_name));
			stat_name = "";
		}
		
		if (GUILayout.Button ("Add text stat")) {
			s.CustomStrings.Add(new CustomStat(stat_name));
			stat_name = "";
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical("box");
		GUILayout.Label ("Creature types:", GUILayout.Width (250));
		foreach (CardType customtype in s.CardTypes)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(customtype.name, GUILayout.Width(100));
			if (GUILayout.Button ("X", GUILayout.Width(50))) { s.CardTypes.Remove(customtype); break;}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		type_name = EditorGUILayout.TextField("New creature type:", type_name, GUILayout.MaxWidth(500));

		if (GUILayout.Button ("Add creature type")) {
			s.CardTypes.Add(new CardType(type_name));
			type_name = "";
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
		
	}
	void Awake()
	{
		if (cards_db == null) cards_db = new CardsDB();

	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (doRepaint)
		{
			Repaint ();
		}
	}
}
