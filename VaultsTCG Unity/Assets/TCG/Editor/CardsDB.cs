using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class CardsDB 
{
	// used for displaying and editing effects:
	static int[] typeints =new int[]{0, 1, 2, 4, 5, 6, 8, 9, 10, 11, 12, 13, 15};
	static string[] typenames =new string[]{"Heal", "Damage", "Draw card(s)", "Put a card (from somewhere) in your keeper zone", "Put a card (from somewhere) in your hand", "Two creatures fight", "Untap", "Destroy", 
		"Debuff", "Buff", "Put a new creature in game under your control", "Put a creature (can be enemy's or from deck, etc) in game under your control", "Gain mana"};
	
	static Dictionary<int, string> param0text = new Dictionary<int, string>() {	{0, "Amount of heal"}, {1, "Amount of damage"}, {2, "Number of cards"}, {10, "Amount"}, {11, "Amount"}, {12, "Creature"}, {15, "Amount of mana"} };
	
	static Dictionary<int, string> targetparam0text = new Dictionary<int, string>() {	{201, "x"}, {202, "x"}, {261, "x"}, {30, "X"} , {31, "X"}  };
	
	static Dictionary<int, string> targetparam1text = new Dictionary<int, string>() {	{201, "y"} };
		
	static Dictionary<int, string> typecardtext = new Dictionary<int, string>() {	{0, "Heal <t> for <p0>"}, {1, "Deal <p0> damage to <t>"}, {2, "Draw <p0> cards"}, {4, "Put <t> in your keeper zone"}, {5, "Put <t> in your hand"}, {6, "Two target creatures deal their attack damage to each other"}, {8, "Untap <t>"}, {9, "Destroy <t>"},   {12, "Put a <p0> in game under your control"}, {13, "Put <t> in game under your control"}, {15, "Gain <p0> mana"} };
	static Dictionary<int, string> buffcardtext = new Dictionary<int, string>() { {1, "Set <t>'s attack to <p0>"}, {2, "<t> gets +<p0>/0"}, {3, "Multiply <t>'s attack by <p0>"},
		{7, "Set <t>'s attack to <p0>"}, {8, "<t> gets 0/+<p0>"}, {9, "Multiply <t>'s defense by <p0>"}, {13, "Set <t>'s critical strike chance to <p0>"}, {20, "<t> gains \"<p0>\""}  };
	static Dictionary<int, string> debuffcardtext = new Dictionary<int, string>() { {1, "Set <t>'s attack to <p0>"}, {2, "<t> gets -<p0>/0"}, {3, "Divide <t>'s attack by <p0>"}, 
		{7, "Set <t>'s defense to <p0>"}, {8, "<t> gets 0/-<p0>"}, {9, "Divide <t>'s defense by <p0>"}, {20, "Remove all abilities from <t>"}  };
		
	static int[] triggerints =new int[]{ 0, 1, 2, 3, 20, 21, 22, 23, 30, 31, 50};
	static string[] triggernames =new string[]{"When this card enters game", "Activated from the menu", "When this creature attacks", "When this creature kills an enemy", 
		"When this player casts a spell", "When the opponent casts a spell", "When a friendly creature dies", 
		"When a friendly creature is attacked", "At the start of your turn", "At the end of your turn", "When you play a <type> creature"};
	
	static int[] targetints =new int[]{ 2, 3, 4, 5, 6, 8, 9, 10, 11, 12, 13, 14, 15, 16, 21, 30, 31, 40, 41, 50, 51, 60, 200, 201, 202, 203, 230, 261, 300, 301, 302, 303, 304};
	static string[] targetnames =new string[]{"Target enemy player or creature", "Target card from your deck", "Two target creatures in game", "Target creature", 
		"Target creature that has attacked this turn", "Current player or their creature", "Target friendly creature", "The current player", "The enemy player", 
		"All friendly creatures", "All creatures", "All enemy creatures", "this creature (for creature abilities only)", "All enemy creatures or heroes", "Target card in hand", "Target creature with attack <x> or less", "Target creature with cost <x> or less", 
		"Target ally", "Target enemy creature or hero", "Target spell from graveyard", "Target creature from graveyard", "The top card from your deck", "A random enemy creature", "<x>-<y> random enemy creatures", "A random enemy creature with cost <x> or less",
		"A random enemy creature or hero", "A random ally", "Random <x> creatures", "A random creature from your hand", "A copy of random creature from your deck", "A random creature from your graveyard", "A random spell from your graveyard",
	"A copy of random creature from enemy deck"};
	
	static int[] buffints =new int[]{ 1, 2, 3, 7, 8, 9, 13, 20 };
	static string[] buffnames =new string[]{"Set attack to...", "Raise attack by...", "Multiply attack by...", "Set defense to...", "Raise defense by...", "Multiply defense by...", "Set critical strike chance to...", "Grant a special ability"};

	
	static int[] debuffints =new int[]{ 1, 2, 3, 7, 8, 9, 20 };
	static string[] debuffnames =new string[]{"Set attack to...", "Lower attack by...", "Divide attack by...", "Set defense to...", "Lower defense by...", "Divide defense by...", "Remove all abilities"};

	static List<int> notarget_effects = new List<int>(){ 2, 12, 15 };

	//for cards:
	static int[] cardtypeints =new int[]{0, 1, 2, 3, 4 };
	static string[] cardtypenames =new string[]{"Keeper", "Creature", "Spell", "Enchantment", "Secret"};
	//

	static int[] keywordints =new int[]{ 0, 1, 2, 3, 4, 5, 101 };
	static string[] keywordnames =new string[]{"No first turn sickness", "Can't attack", "Takes no combat damage", "Deals no combat damage", "Takes no damage from spells", "First attack each turn doesn't cause to turn", "Can move 2 slots instead of one"};

	static int[] paramtypeints =new int[]{0, 1, 2};
	static string[] paramtypenames =new string[]{"Value", "Number of player creatures", "Number of player creatures killed this turn"};

	public List<DbCard> cards;

	public DbCard viewed_card = null;

	int color_int;
	int mana_type;
	int e_mana_type;
	int cost_int;
	int e_cost_int;

	string new_attribute_name = "";
	Vector2 scrollPos = Vector2.zero;
	Vector2 scrollPosFirst = Vector2.zero;
	string fileName = "";
	Texture2D imagespath = null;

	public static Sprite[] cardimages;

	static int[] creatureints;
	static string[] creaturenames;

	static int[] colorints;
	static string[] colornames;

	static int[] subtypeints;
	static string[] subtypenames;

	static int[] subtypeintsnone;
	static string[] subtypenamesnone;

	//public bool UseManaColors = false;

	public int tempsubtype;

	public void Awake()
	{

	}

	public bool DrawEffect(Effect e){
		
		int type = e.type;
		int target = e.target;
		
		GUILayout.BeginHorizontal ();
		
		if (e.creatureability) {
			GUILayout.Label ("Ability triggers... ");
			e.trigger = EditorGUILayout.IntPopup (e.trigger, triggernames, triggerints, GUILayout.MaxWidth (350));
			
		} else if (e.hastrigger) { //enchantments and keepers with trigger
			GUILayout.Label("Ability triggers... ");
			e.trigger = EditorGUILayout.IntPopup(e.trigger, triggernames, triggerints, GUILayout.MaxWidth(350));
		}
		else e.trigger = -1; 

		if (e.trigger == 1) { //if this ability activates from a menu
			GUILayout.Label("Cost: ");
			if (MainMenu.TCGMaker.core.UseManaColors) 
			{

				if (e.cost.Count == 0) GUILayout.Label("<none>");
				ManaColor color_to_delete = null;
				foreach (ManaColor foundcolor in e.cost) 
				{
					GUILayout.BeginHorizontal ();
					
					if (foundcolor.icon) //if there is an icon, display it
					{
						if (!foundcolor.icon_texture) foundcolor.icon_texture = SpriteToTexture(foundcolor.icon);
						GUILayout.Label (foundcolor.icon_texture, GUILayout.Width(25), GUILayout.Height(25)); 
					}
					else GUILayout.Label(foundcolor.name);
					
					if (GUILayout.Button("X")) color_to_delete = foundcolor; 
					
					GUILayout.EndHorizontal ();
				}
				if (color_to_delete != null) e.cost.Remove(color_to_delete);
				
				GUILayout.BeginHorizontal ();
				e_mana_type = EditorGUILayout.IntPopup(e_mana_type, colornames, colorints);
				if (GUILayout.Button("Add cost")) e.cost.Add(MainMenu.TCGMaker.core.colors[e_mana_type]); 
				GUILayout.EndHorizontal ();
				
			} 	
				else 
			{
				e_cost_int = e.cost.Count;
				e_cost_int = EditorGUILayout.IntField ("Cost: ", e_cost_int);
				if (e.cost.Count != e_cost_int)
					{
						e.cost.Clear();
						for (int i=0; i < e_cost_int; i++)
							e.cost.Add(MainMenu.TCGMaker.core.colors[0]); //add 1 colorless mana to cost
					}
			}
					
			GUILayout.Label ("Cards to discard:");
			e.discardcost = EditorGUILayout.IntField (e.discardcost, GUILayout.Width (50));
		
		} //activated abilities cost end
		else if (e.trigger == 50) //if trigger requires a creature type param
		{
			GUILayout.Label ("Type:");
			e.triggerparam0 = EditorGUILayout.IntPopup (e.triggerparam0, subtypenames, subtypeints, GUILayout.MaxWidth (350));
		}

		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();

		GUILayout.Label("Effect:");
		e.type = EditorGUILayout.IntPopup(e.type, typenames, typeints, GUILayout.MaxWidth(300));
		
		if (type == 10 || type == 11) { //displaying "until end of turn" option for buff and debuffs
		
			GUILayout.Label("Until EOT:");
			e.eot = EditorGUILayout.Toggle (e.eot, GUILayout.Width (50));

		}
		
		if (type == 10) //debuff	
		{
			GUILayout.Label("Debuff type:");
			e.bufftype = EditorGUILayout.IntPopup ( e.bufftype, debuffnames, debuffints, GUILayout.MaxWidth (300));
		}
		else if (type == 11) //buff	
		{
			GUILayout.Label("Buff type:");
			e.bufftype = EditorGUILayout.IntPopup ( e.bufftype, buffnames, buffints, GUILayout.MaxWidth (300));

			if (e.bufftype == 20) //assign ability buff
				e.param0 = EditorGUILayout.IntPopup ("Special ability:", e.param0, keywordnames, keywordints, GUILayout.Width (300));



				
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();

		//parameters:
		if (type == 12)	//place a new creature in game 
		{
			
			e.param0 = EditorGUILayout.IntPopup (param0text [type], e.param0, creaturenames, creatureints, GUILayout.Width (300));
		}
	
		else if (param0text.ContainsKey(type) && e.bufftype != 20 ) 
		{
			e.param0type = EditorGUILayout.IntPopup ("Parameter type:", e.param0type, paramtypenames, paramtypeints, GUILayout.Width (300));

			if (e.param0type == 0) //number 
				{
					GUILayout.Label(param0text[type]+":");
					e.param0 = EditorGUILayout.IntField (e.param0, GUILayout.Width(50));
				}
		}
		else if (!param0text.ContainsKey(type)) e.param0type=0; //important

	

		if (!notarget_effects.Contains(type)) { 
			GUILayout.Label("Target:");
			e.target = EditorGUILayout.IntPopup(e.target, targetnames, targetints, GUILayout.MaxWidth(340));
		}
		
		
		if (!notarget_effects.Contains(type) && targetparam0text.ContainsKey(target)) 
		{

			GUILayout.Label(targetparam0text[target]+":");
			e.targetparam0 = EditorGUILayout.IntField (e.targetparam0);
		}
		
		if (!notarget_effects.Contains(type) && targetparam1text.ContainsKey(target)) 
		{
			GUILayout.Label(targetparam1text[target]+":");
			e.targetparam1 = EditorGUILayout.IntField (e.targetparam1);
		}
		
		GUILayout.EndHorizontal();
		
		return GUILayout.Button("x",GUILayout.Width(20));
		
		
	}
	
	public void GenerateCardText(DbCard c)
	{
		string text = "";
		string temptext = "";
		string targettext = "";
		string param0text = "";
		int type = c.type;
		int discardcost = c.discardcost;
		string name = c.name;

		foreach (Effect effect in c.effects)
		{
			// adding trigger text:
			if (effect.trigger == 0) text += "When " + name + " enters game, "; //"on enter" abilities
			else if (effect.trigger == 3) text += "When " + name + " kills an enemy, "; //"on kill" abilities
			else if (effect.trigger == 2) text += "When " + name + " attacks, "; //"on attack" abilities
			else if (effect.trigger == 1) text += "Ability: ";	//activated abilities
			else if (effect.trigger >= 20) //triggers that are not specifir to this creature (when opponnent plays a spell, etc)
			{
				for (int i=0; i < triggerints.Length; i++)
				{
					if (triggerints[i] == effect.trigger) 
					{ 
						text += triggernames[i];
						text +=", ";
					}
				}
			}

			if (effect.trigger == 50) //replacing <type> with chosen creature type 
				for (int i=0; i < subtypeints.Length; i++)
			{
				if (subtypeints[i] == effect.triggerparam0) 
				{ 
					text = text.Replace("<type>", subtypenames[i]);

				}
			}

			//additing additional cost text:
			if (discardcost == 1 && type == 2) text+="Discard a card: "; //if it's a spell
			else if (discardcost > 0 && type == 2) text+="Discard "+discardcost+" cards: ";
			
			if (discardcost == 1 && type == 1) text += "As an additional cost to play "+name+ ", discard a card"; // if it's a creature
			else if (discardcost > 0 && type == 1) text += "As an additional cost to play "+name+ ", discard "+discardcost+" cards";
			
			if (effect.discardcost == 1) text+="Discard a card: ";
			else if (effect.discardcost > 0) text+="Discard "+effect.discardcost+" cards: ";
			
			
			// adding target text:
			if (effect.target == 15) targettext = "it"; //this creature 
				else for (int i=0; i < targetints.Length; i++)
				{
					if (targetints[i] == effect.target)
					{
						targettext = targetnames[i];
						targettext = targettext.Replace("<x>", effect.targetparam0.ToString());
						targettext = targettext.Replace("<y>", effect.targetparam1.ToString());
						targettext = targettext.First().ToString().ToLower() + targettext.Substring(1); //making first letter lowercase
					}
				}
			
			
			if (effect.type == 10) temptext = debuffcardtext[effect.bufftype].Replace("<t>", targettext);	//debuffs
			else if (effect.type == 11) temptext = buffcardtext[effect.bufftype].Replace("<t>", targettext);	 //buffs
			else temptext = typecardtext[effect.type].Replace("<t>", targettext);	//adding target text such as "target player or creature"
			
			
			if (effect.type == 12) //put a new creature in game
			{
				for (int i=0; i < creatureints.Count(); i++)
				{
					if (creatureints[i] == effect.param0) param0text = creaturenames[i];
					
				}
			}
			else if (effect.bufftype == 20) //assign ability
				for (int i=0; i < keywordints.Count(); i++)
				{
					if (keywordints[i] == effect.param0) param0text = keywordnames[i];
				
				}
			else if (effect.param0type == 0)
			{
			 param0text = effect.param0.ToString();
			}

			else {//special parameters
				param0text = "X";
				if (effect.param0type == 1) //number of player allies
					temptext += ", where X is the number of allies";
				else if (effect.param0type == 2) //number of player allies destroyed this turn
					temptext += ", where X is the number of allies destroyed this turn";
			}
			temptext = temptext.Replace("<p0>", param0text);	//adding param0 text, it is currently either a number or a creature

			if (text!="") temptext = temptext.First().ToString().ToLower() + temptext.Substring(1); //making first letter lowercase
			
			text += temptext;
			
			if (effect.type == 10 || effect.type == 11)
				if (effect.eot) text += " until end of turn";
			
			
			text += "\n";
			text = text.First().ToString().ToUpper() + text.Substring(1); //making first character uppercase
			c.text = text;

		}
		
	}


	public  bool ViewCard(DbCard c){
		
		int type = c.type;


		c.name = EditorGUILayout.TextField("Name:", c.name, GUILayout.MaxWidth(500));
		
		GUILayout.BeginHorizontal();
		int temptype = EditorGUILayout.IntPopup("Type: ", c.type, cardtypenames, cardtypeints, GUILayout.Width(300));

		if (temptype != c.type) //if the type has been changed
						c.effects.Clear ();
		c.type = temptype;
		if (c.type == 1) 
		{
			tempsubtype = c.subtype+1; //-1 becomes 0
			tempsubtype = EditorGUILayout.IntPopup("Subtype: ", tempsubtype, subtypenamesnone, subtypeintsnone, GUILayout.Width(300));
			c.subtype = tempsubtype-1; //0 ("none") becomes -1
		}
		GUILayout.EndHorizontal();

		if (colornames != null) //we allow to choose a color even if we don't use mana colors: for aestetic purposes: card frames, etc

		{	
			if (c.color == null) c.color = MainMenu.TCGMaker.core.colors[0];
			color_int = 0;
			for (int i=0; i<colornames.Length; i++)
				if (colornames[i] == c.color.name) color_int = colorints[i];
			//if (MainMenu.TCGMaker.core.colors.Contains(c.color)) color_int = MainMenu.TCGMaker.core.colors.IndexOf(c.color);


			color_int = EditorGUILayout.IntPopup("Color: ", color_int, colornames, colorints, GUILayout.Width(300));
			c.color = MainMenu.TCGMaker.core.colors[color_int];

		}
		
		foreach (CustomStat customstat in MainMenu.TCGMaker.stats.CustomInts)
		{
			//we can't serialize dictionaries so going to use linq..
			CustomInt intstat = c.CustomInts.Where(x => x.h_name == customstat.h_name).SingleOrDefault();

			if (intstat == null)
			{
				intstat = new CustomInt(customstat.h_name);
				c.CustomInts.Add(intstat);
			}

			intstat.value = EditorGUILayout.IntField (customstat.h_name+": ", intstat.value);
		}
		
		foreach (CustomStat customstat in MainMenu.TCGMaker.stats.CustomStrings)
		{
			//we can't serialize dictionaries so going to use linq..
			CustomString stringstat = c.CustomStrings.Where(x => x.h_name == customstat.h_name).SingleOrDefault();
			
			if (stringstat == null)
			{
				stringstat = new CustomString(customstat.h_name);
				c.CustomStrings.Add(stringstat);
			}
			
			stringstat.value = EditorGUILayout.TextField (customstat.h_name+": ", stringstat.value);
		}

		
		if (type != 0) {
			GUILayout.BeginHorizontal ();

			if (MainMenu.TCGMaker.core.UseManaColors) 
			{
				GUILayout.Label("Cost: ");
				if (c.cost.Count == 0) GUILayout.Label("<none>");
				ManaColor color_to_delete = null;
				foreach (ManaColor foundcolor in c.cost) 
				{
					GUILayout.BeginHorizontal ();

						if (foundcolor.icon) //if there is an icon, display it
						{
						if (!foundcolor.icon_texture) foundcolor.icon_texture = SpriteToTexture(foundcolor.icon);
							GUILayout.Label (foundcolor.icon_texture, GUILayout.Width(25), GUILayout.Height(25)); 
						}
						else GUILayout.Label(foundcolor.name);

					if (GUILayout.Button("X")) color_to_delete = foundcolor; 
					
					GUILayout.EndHorizontal ();
				}
				if (color_to_delete != null) c.cost.Remove(color_to_delete);

				GUILayout.BeginHorizontal ();
				mana_type = EditorGUILayout.IntPopup(mana_type, colornames, colorints);
				if (GUILayout.Button("Add cost")) c.cost.Add(MainMenu.TCGMaker.core.colors[mana_type]); 
				GUILayout.EndHorizontal ();
				
			} 
			else {
					cost_int = c.cost.Count;
					cost_int = EditorGUILayout.IntField ("Cost: ", cost_int);
					if (c.cost.Count != cost_int)
						{
							c.cost.Clear();
							for (int i=0; i< cost_int; i++)
							c.cost.Add(MainMenu.TCGMaker.core.colors[0]); //add 1 colorless mana to cost
						}
				}



			bool has_discard_cost =  (c.discardcost > 0)? true : false;

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label("Has a discard cost: ");
			has_discard_cost = EditorGUILayout.Toggle(has_discard_cost);
			
			if (has_discard_cost && c.discardcost == -1) c.discardcost = 1;
			if (!has_discard_cost) c.discardcost = -1;
			
			if (c.discardcost > 0) c.discardcost = EditorGUILayout.IntField ("Cards to discard: ", c.discardcost);
			else c.discardcost = -1;
			GUILayout.EndHorizontal ();
		}

		
		if (type == 1) {	//if it's a creature

			c.hero = EditorGUILayout.Toggle("Is a hero:", c.hero);
			c.ranged = EditorGUILayout.Toggle("Ranged:", c.ranged);
			GUILayout.BeginHorizontal ();
			c.offense = EditorGUILayout.IntField ("Offense: ", c.offense);
			c.defense = EditorGUILayout.IntField ("Defense: ", c.defense);
			GUILayout.EndHorizontal ();

			GUI.skin.label.fontStyle = FontStyle.Bold;
			GUILayout.BeginHorizontal ();
			GUILayout.Label("Specials:");
			GUILayout.EndHorizontal();
			GUI.skin.label.fontStyle = FontStyle.Normal;

			GUILayout.BeginVertical ("box",GUILayout.Width(600));
			GUILayout.BeginHorizontal ();

			c.cant_attack = EditorGUILayout.Toggle("Can't attack:", c.cant_attack);
			c.takes_no_combat_dmg = EditorGUILayout.Toggle("Takes no combat damage:", c.takes_no_combat_dmg);
			c.deals_no_combat_dmg = EditorGUILayout.Toggle("Deals no combat damage:", c.deals_no_combat_dmg);
		
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();

			c.no_first_turn_sickness = EditorGUILayout.Toggle("No first turn sickness:", c.no_first_turn_sickness);
			if (MainMenu.TCGMaker.core.UseGrid) c.extramovement = EditorGUILayout.Toggle("Extra movement:", c.extramovement);
			c.free_attack = EditorGUILayout.Toggle("First attack doesn't turn:", c.free_attack);

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			
			c.less_dmg_from_ranged = EditorGUILayout.Toggle("Less damage from ranged:", c.less_dmg_from_ranged);
			c.no_dmg_from_ranged = EditorGUILayout.Toggle("No damage from ranged:", c.no_dmg_from_ranged);
			c.takes_no_spell_dmg = EditorGUILayout.Toggle("No damage from spells:", c.takes_no_spell_dmg);

			GUILayout.EndHorizontal ();

			GUILayout.EndVertical();

			GUILayout.BeginHorizontal ();
			c.growid = EditorGUILayout.TextField("Upgrade ID:", c.growid, GUILayout.MaxWidth(500));
			if (c.growid != "") c.level = EditorGUILayout.IntField ("Level: ", c.level);

			GUILayout.EndHorizontal ();
			
			foreach (Effect foundeffect in c.effects)
				foundeffect.creatureability = true;
		} 
		else if (type == 3 || type == 4 || type == 0) {
			foreach (Effect foundeffect in c.effects) foundeffect.hastrigger = true; //enchantments, keepers and secrets
		}
		
		GUILayout.BeginHorizontal();
		
		c.art = EditorGUILayout.ObjectField ("Art:",c.art, typeof(Sprite), false) as Sprite;

		GUILayout.EndHorizontal();
	
		if (c.art != null) 
						GUILayout.Label (SpriteToTexture (c.art));
	
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Text:");
		
		c.text = EditorGUILayout.TextArea(c.text);
		if (c.effects.Count > 0 && GUILayout.Button ("Generate", GUILayout.Width (100))) GenerateCardText(c);
		
		GUILayout.EndHorizontal();

		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.BeginHorizontal ();
		GUILayout.Label("Sounds:");
		GUILayout.EndHorizontal();
		GUI.skin.label.fontStyle = FontStyle.Normal;

		GUILayout.BeginVertical ("box");
	

		GUILayout.BeginHorizontal();
		
		c.sfxentry = EditorGUILayout.ObjectField ("Entry:",c.sfxentry, typeof(AudioClip), false) as AudioClip;
		c.sfxability0 = EditorGUILayout.ObjectField ("Ability:",c.sfxability0, typeof(AudioClip), false) as AudioClip;


		GUILayout.EndHorizontal();

		if (MainMenu.TCGMaker.core.UseGrid) {
			GUILayout.BeginHorizontal();
			c.sfxmove1 = EditorGUILayout.ObjectField ("Move 2:",c.sfxmove1, typeof(AudioClip), false) as AudioClip;
			c.sfxmove0 = EditorGUILayout.ObjectField ("Move 1:",c.sfxmove0, typeof(AudioClip), false) as AudioClip;
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		if (c.effects.Count > 0 ) { 
			GUI.skin.label.fontStyle = FontStyle.Bold;
			GUILayout.BeginHorizontal ();
			if (type == 1)  GUILayout.Label("Abilities:");
			else GUILayout.Label("Effects:");
			GUILayout.EndHorizontal();
			GUI.skin.label.fontStyle = FontStyle.Normal;
		}
		
		Effect effect_to_remove = null;
		foreach (Effect effect in c.effects) {
			GUILayout.BeginVertical ("box");
			
			if (DrawEffect(effect)) effect_to_remove = effect; 		//it's the way to do it because otherwise we'll get "collection was modified" error
			GUILayout.EndVertical ();
		}
		if (effect_to_remove != null) c.effects.Remove(effect_to_remove);	
		
		
		GUILayout.BeginVertical ();
		//GUILayout.Label(typestring,GUILayout.Width(150));
		if (type == 1 && GUILayout.Button ("Add ability", GUILayout.Width (100))) //creature
			c.effects.Add (new Effect (false, true));
		
		
		else if (type == 2 && GUILayout.Button ("Add effect", GUILayout.Width (100)))	//spell
			c.effects.Add (new Effect ());
		
		else if ((type == 3 || type == 4 || type == 0 ) && GUILayout.Button ("Add effect", GUILayout.Width (100))) //enchantment/secret/keeper
			c.effects.Add (new Effect (true));
		
		if (GUILayout.Button ("Delete card", GUILayout.Width (100)))
			return true;
		GUILayout.EndVertical ();
		
		return false;
	} //
	
	public bool DrawCard(DbCard c ){
		
		string typestring="";
		
		for (int i = 0; i < cardtypeints.Length; i++) 
		{
			if (c.type == cardtypeints[i]) typestring = cardtypenames[i];
		}
		
		GUILayout.Label(typestring,GUILayout.Width(100));
		
		return GUILayout.Button("Delete",GUILayout.Width(100));
		
	} 
	
	static Texture2D SpriteToTexture(Sprite sprite)
		
	{
		Texture2D croppedTexture = new Texture2D ((int)sprite.rect.width, (int)sprite.rect.height);
		
		Color[] pixels = sprite.texture.GetPixels ((int)sprite.textureRect.x, 
		                                           (int)sprite.textureRect.y, 
		                                           (int)sprite.rect.width, 
		                                           (int)sprite.rect.height);
		//Debug.Log ("pixels size:"+pixels.Length);
		//Debug.Log ("width:"+sprite.rect.width + "height:"+sprite.rect.height );
		croppedTexture.SetPixels (pixels);
		croppedTexture.Apply ();
		return croppedTexture;
		
	}

	public void UpdateCreatures(List<DbCard> cardlist)
	{
		int i=0;
		List<int> creatureintslist = new List<int>();
		List<string> creaturenameslist = new List<string>();
		foreach (DbCard foundcard in cardlist)
		{
		

			if (foundcard.type == 1)  {
				creatureintslist.Add(i);
				creaturenameslist.Add(foundcard.name);
			}
			i++;
		}

		creatureints = new int[creatureintslist.Count];
		creaturenames = new string[creaturenameslist.Count];
		
		for (i=0; i<creatureintslist.Count; i++)
		{
			creatureints[i] = creatureintslist.ElementAt(i);
			creaturenames[i] = creaturenameslist.ElementAt(i);
		}
		////
		Debug.Log("updating, color count: "+ MainMenu.TCGMaker.core.colors.Count);
		colorints = new int[MainMenu.TCGMaker.core.colors.Count];
		colornames = new string[MainMenu.TCGMaker.core.colors.Count];
		
		for (i=0; i < MainMenu.TCGMaker.core.colors.Count; i++)
		{
			colorints[i] = i;
			colornames[i] = MainMenu.TCGMaker.core.colors[i].name;
		}
		
		Debug.Log("updating, subtype count: "+ MainMenu.TCGMaker.stats.CardTypes.Count);
		subtypeints = new int[MainMenu.TCGMaker.stats.CardTypes.Count];
		subtypenames = new string[MainMenu.TCGMaker.stats.CardTypes.Count];

		subtypeintsnone = new int[MainMenu.TCGMaker.stats.CardTypes.Count+1];
		subtypenamesnone = new string[MainMenu.TCGMaker.stats.CardTypes.Count+1];

		subtypeintsnone[0] = 0;
		subtypenamesnone[0] = "<none>";
		for (i=0; i < MainMenu.TCGMaker.stats.CardTypes.Count; i++)
		{
			subtypeints[i] = i;
			subtypenames[i] = MainMenu.TCGMaker.stats.CardTypes[i].name;
			subtypeintsnone[i+1] = i+1;
			subtypenamesnone[i+1] = MainMenu.TCGMaker.stats.CardTypes[i].name;
		}


	}



	void WriteEffect(StreamWriter writer, Effect effect, int e_number)
	{
		if (effect.type!=-1) 	writer.Write ("effect"+e_number+"={0} ", effect.type);  //ex. effect0=
		if (effect.trigger!=-1) 	writer.Write ("trigger"+e_number+"={0} ", effect.trigger);
//		if (effect.cost!=-1) 	writer.Write ("cost"+e_number+"={0} ", effect.cost);
		if (effect.discardcost!=-1) 	writer.Write ("cost"+e_number+"={0} ", effect.discardcost);
		if (effect.target!=-1) 	writer.Write ("target"+e_number+"={0} ", effect.target);  
		if (effect.targetparam0!=-1) 	writer.Write ("target"+e_number+"param0={0} ", effect.targetparam0);  
		if (effect.targetparam1!=-1) 	writer.Write ("target"+e_number+"param1={0} ", effect.targetparam1);  
		if (effect.param0!=-1) 	writer.Write ("param"+e_number+"_0={0} ", effect.param0);  
		if (effect.param1!=-1) 	writer.Write ("param"+e_number+"_1={0} ", effect.param1);  
		if (effect.bufftype!=-1) 	writer.Write ("bufftype"+e_number+"={0} ", effect.bufftype);  
		if (effect.eot!=false) 	writer.Write ("eot"+e_number+"=1 "); 

	}

	void WriteCard(StreamWriter writer, DbCard dbcard)
	{
		writer.Write ("<card ");
		Debug.Log ("dbcard name:" + dbcard.name);
		if (dbcard.id!=-1) 	writer.Write ("id={0} ", dbcard.id);
		if (dbcard.art!=null) 	writer.Write ("art={0} ", dbcard.art.ToString()); 
		if (dbcard.name!="") 	writer.Write ("name=\"{0}\" ", dbcard.name); 
		if (dbcard.type!=-1) 	writer.Write ("type={0} ", dbcard.type);
//		if (dbcard.color!=-1) 	writer.Write ("color={0} ", dbcard.color);

//		foreach (string m_name in dbcard.CustomInts.Keys)
//			writer.Write ("ci_{0}={1}", m_name, dbcard.CustomInts[m_name]);

//		foreach (string m_name in dbcard.CustomStrings.Keys)
//			writer.Write ("cs_{0}={1}", m_name, dbcard.CustomStrings[m_name]);

//		if (dbcard.cost!=-1) 	writer.Write ("cost={0} ", dbcard.cost); 
		if (dbcard.discardcost!=-1) 	writer.Write ("discardcost={0} ", dbcard.discardcost); 
		if (dbcard.text!="") 	writer.Write ("text=\"{0}\" ", dbcard.text); 
		
		if (dbcard.type == 1) 
				{
					if (dbcard.level!=-1) 	writer.Write ("level={0} ", dbcard.level);
					if (dbcard.growid!="") 	writer.Write ("growid=\"{0}\" ", dbcard.growid); 
					if (dbcard.offense!=-1) 	writer.Write ("attack={0} ", dbcard.offense); 
					if (dbcard.defense!=-1) 	writer.Write ("defense={0} ", dbcard.defense); 
				}


		int i = 0;
		foreach (Effect effect in dbcard.effects) 
							{
								WriteEffect(writer,effect, i);
								i++; 
							}
				


		
			
		writer.Write ("/> \n");	//end of line
	}



	List<DbCard> ReadFile(string fileName)
	{
		StreamReader sr = new StreamReader(fileName);

		string fileContents = sr.ReadToEnd();
		sr.Close();

		List<DbCard> output = new List<DbCard>();

		int eot;
		string[] stringSeparators = new string[] {"<card"};
	
		string[] lines = fileContents.Split(stringSeparators, System.StringSplitOptions.None);
		Debug.Log("lines length:"+lines.Length);



		int i = 0;
	

	
		string line = "";
		int j;
		for (j = 1; j < lines.Count(); j++) {

			line = lines[j];
			if (Regex.Match(line, "(?<=name=\")[a-zA-Z0-9 ]+").ToString()!="" || Regex.Match(line, "(?<=type=)[0-9]+").ToString()!="" || Regex.Match(line, "(?<=art=\")[a-zA-Z0-9 ]+").ToString()!="") 
			{
			DbCard newcard = new DbCard();
			newcard.effects = new List<Effect>();
			if (Regex.Match(line, "(?<=name=\")[a-zA-Z0-9 ]+").ToString()!="") newcard.name = Regex.Match(line, "(?<=name=\")[a-zA-Z0-9 ]+").ToString();
			if (Regex.Match(line, "(?<=text=\")[-a-zA-Z0-9,/:\n ]+").ToString()!="") newcard.text = Regex.Match(line, "(?<=text=\")[-a-zA-Z0-9,/:\n ]+").ToString();
			

			if (Regex.Match(line, "(?<=type=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=type=)[0-9]+").ToString(), out newcard.type);
//			if (Regex.Match(line, "(?<=color=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=color=)[0-9]+").ToString(), out newcard.color);
//			if (Regex.Match(line, "(?<=art=\")[a-zA-Z0-9 ]+").ToString()!="") newcard.art = Regex.Match(line, "(?<=art=\")[a-zA-Z0-9 ]+").ToString();
			newcard.id = output.Count;
//			if (Regex.Match(line, "(?<=cost=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=cost=)[0-9]+").ToString(), out newcard.cost);
			if (Regex.Match(line, "(?<=discardcost=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=discardcost=)[0-9]+").ToString(), out newcard.discardcost);
			Debug.Log (line);
			output.Add(newcard);

			if (newcard.type == 1) {	//if it's a creature
				if (Regex.Match(line, "(?<=growid=\")[a-zA-Z0-9 ]+").ToString()!="") newcard.growid = Regex.Match(line, "(?<=growid=\")[a-zA-Z0-9 ]+").ToString();
				if (Regex.Match(line, "(?<=level=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=level=)[0-9]+").ToString(), out newcard.level);
				if (Regex.Match(line, "(?<=attack=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=attack=)[0-9]+").ToString(), out newcard.offense);
				if (Regex.Match(line, "(?<=defense=)[0-9]+").ToString()!="") System.Int32.TryParse(Regex.Match(line, "(?<=defense=)[0-9]+").ToString(), out newcard.defense);

		
			}
			
	
			for (i = 0; i<10; i++)
			{	Effect foundeffect = new Effect(newcard.type == 1);
					eot = -1;
				if (Regex.Match(line, "(?<=effect"+i+"=)[0-9]+").ToString() !=""  )
				{
					System.Int32.TryParse(Regex.Match(line, "(?<=effect"+i+"=)[0-9]+").ToString(), out foundeffect.type);
					Debug.Log("eot:" +foundeffect.eot);
					if (Regex.Match(line, "(?<=param"+i+"_0=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=param"+i+"_0=)[0-9]+").ToString(), out foundeffect.param0);
					if (Regex.Match(line, "(?<=param"+i+"_1=)[0-9]+").ToString() != "")  System.Int32.TryParse(Regex.Match(line, "(?<=param"+i+"_1=)[0-9]+").ToString(), out foundeffect.param1);
//					if (Regex.Match(line, "(?<=cost"+i+"=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=cost"+i+"=)[0-9]+").ToString(), out foundeffect.cost);
					if (Regex.Match(line, "(?<=discardcost"+i+"=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=discardcost"+i+"=)[0-9]+").ToString(), out foundeffect.discardcost);
					if (Regex.Match(line, "(?<=target"+i+"=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=target"+i+"=)[0-9]+").ToString(), out foundeffect.target);
					if (Regex.Match(line, "(?<=target"+i+"param0=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=target"+i+"param0=)[0-9]+").ToString(), out foundeffect.targetparam0);
					if (Regex.Match(line, "(?<=target"+i+"param1=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=target"+i+"param1=)[0-9]+").ToString(), out foundeffect.targetparam1);
					if (Regex.Match(line, "(?<=bufftype"+i+"=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=bufftype"+i+"=)[0-9]+").ToString(), out foundeffect.bufftype);
					if (Regex.Match(line, "(?<=eot"+i+"=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=eot"+i+"=)[0-9]+").ToString(), out eot);
					if (eot == 1) foundeffect.eot = true;
					if (Regex.Match(line, "(?<=trigger"+i+"=)[0-9]+").ToString() != "") System.Int32.TryParse(Regex.Match(line, "(?<=trigger"+i+"=)[0-9]+").ToString(), out foundeffect.trigger);
				
					Debug.Log("eot after parse:" +foundeffect.eot);
					newcard.effects.Add(foundeffect);
				}
				else break; //if there's no next effect, stop reading effects
			}
			}
			}


		return output;
	}
	
	void WriteFile()
	{
		Debug.Log("SaveFile");
		string path = Application.dataPath; // <project folder>/Assets
		string name = "db.xml";
		if (File.Exists(fileName))
		{
			FileInfo FI = new FileInfo(fileName);
			path = FI.DirectoryName;
			name = FI.Name;
		}
		string tmp = EditorUtility.SaveFilePanel("Save new card database",path,name,"xml");

		if (tmp == "")
			return;
		fileName = tmp;

		StreamWriter writer = File.CreateText(fileName);

		foreach (string colorname in colornames)
				{

				writer.Write ("<color ");
				writer.Write ("name=\"{0}\" ", colorname);
				writer.Write (" /> \n");

				}
		foreach (DbCard dbcard in cards)
			{
			WriteCard(writer, dbcard);
			}
		writer.Close();

	}

	void OpenFile()
	{
		string tmp = EditorUtility.OpenFilePanel("Select card database file",fileName,"xml");
		if (tmp != "")
		{
			fileName = tmp;
			if (System.IO.File.Exists(fileName))
				cards = ReadFile(fileName);
			UpdateCreatures(cards);


		}
	}


	public void DrawGUI()
	{
		if (cards == null) cards = new List<DbCard>();
	
		GUILayout.BeginHorizontal();
	
		GUILayout.BeginVertical(); //1st column
		scrollPosFirst = GUILayout.BeginScrollView(scrollPosFirst);

		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.Label("Cards database");

		GUILayout.BeginHorizontal("box"); //small box for load file

	
		if (GUILayout.Button("load from xml (optional)",GUILayout.Width(200))
		    ) 
			OpenFile();


		GUI.enabled = true;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();


		if (cards.Count > 0)	//displaying the cards' table
		{
			GUI.skin.label.alignment = TextAnchor.MiddleCenter; 
			GUILayout.BeginHorizontal();
	
			GUILayout.Label("name",GUILayout.MinWidth(150), GUILayout.MaxWidth(300));
			GUILayout.Label("type",GUILayout.Width(100));
			GUILayout.Label("",GUILayout.Width(100));

			GUILayout.EndHorizontal();
			GUI.skin.label.fontStyle = FontStyle.Normal;

			for (int i = 0; i < cards.Count; i++)
			{
				DbCard currentcard = cards[i];
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(currentcard.name,GUILayout.MinWidth(150), GUILayout.MaxWidth(300))) { 
					GUIUtility.keyboardControl = 0; //lose keyboard focus
					viewed_card = currentcard; 
				}
				if (DrawCard(currentcard))
					cards.RemoveAt(i);	//deleting card from db
				GUILayout.EndHorizontal();
			}
				
					
		}

		if (GUILayout.Button("new card",GUILayout.Width(100))    ) 
		{	DbCard newcard = new DbCard();
			newcard.id = cards.Count;
			cards.Add(newcard);
			UpdateCreatures(cards);
			viewed_card = newcard;
		}
	
		GUILayout.EndScrollView();
		GUILayout.EndVertical(); //1st column end


		GUILayout.BeginVertical(); //2d column
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.Label("Card view");
		GUI.skin.label.fontStyle = FontStyle.Normal;

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("File:",GUILayout.Width(30));
		GUILayout.TextField(fileName);
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical("box",GUILayout.Width(600));

		if (viewed_card != null)	//displaying the card's detailed info
		{
				GUILayout.BeginVertical();

				if (ViewCard(viewed_card))
					cards.Remove(viewed_card);	//deleting card from db
				
				GUILayout.BeginVertical();
		}

		GUILayout.EndVertical(); //detailed card box

		GUILayout.EndScrollView();
		GUILayout.EndVertical(); //2d column: detailed card info

		GUILayout.EndHorizontal();


	}

}