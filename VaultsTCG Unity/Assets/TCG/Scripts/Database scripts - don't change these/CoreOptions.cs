using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ManaColor
{
	//public int id;
	public string name;
	public Sprite icon;
	public Texture icon_texture;

	public bool Default = false;

	public ManaColor(string m_name="")
	{
		name = m_name;

	}
}

[System.Serializable]
public class CoreOptions   {
	// starting stats:
	//public  int OptionStartingCards = 6; assigned through zone system now
	public  int OptionStartingLife = 20;
	
	// game mechanics options:
	public   List<DBZone> zones = new List<DBZone>(); 
	public   List<DBZone> enemy_zones = new List<DBZone>(); 

	public  bool OptionFirstTurnSickness = true;
	public  bool OptionGraveyard = true;

	public  int MaxHandSize = 7;

	public  bool OptionManaDoesntReset = false;
	public  bool OptionManaAutoIncrementsEachTurn = false;
	public  int OptionManaMaxIncrement = 10; //used only when OptionManaAutoIncrementsEachTurn is set to 'true', mana gained each turn will stop incrementing when it reaches this amount. 

	public bool UseCardColors = false; 
	public bool UseManaColors = false; //mana colors are edited in database file in Card Editor

	public List<ManaColor> colors = new List<ManaColor>();



	// combat options:
	public  bool OptionCantAttackPlayerThatHasHeroes = false;	//hero is a creature with hero=1   attribute in database xml
	public  bool OptionGameLostIfHeroDead = false;


	public bool OptionRetaliate = true; //with this set to 'true' a creature that gets attacked does its combat damage back to the attacker
	public bool OptionOneCombatStatForCreatures = false;  //instead of attack and defense, just one "power" stat
	public bool OptionKillOrDoNothing = false; //with this set to 'true' a creature either kills the opposing one or does no damage. Doesn't work with Option Retaliate currently
	
	
	// cosmetic options:
	public  bool OptionGameMusic = false;
	public  bool OptionPlayerTurnPopup = true;
	public  float OptionTurnDegrees = -90; //degrees for when a card is turned (for mana or attacking)
	
	//card art and text generation:

	public  bool OptionCardFrameIsSeparateImage = true;

	public bool UseGrid = false;

	public void AddDefaultColors()
	{
		ManaColor colorless = new ManaColor("colorless");
		colorless.Default = true;
		colors.Add(colorless);
	}

	public void AddDefaultZones()
	{

		DBZone newzone = new DBZone ("Hand");
		newzone.DrawAtTheStartOfGame = 6;
		zones.Add (newzone);
		newzone = new DBZone ("Keepers");
		zones.Add (newzone);
		newzone = new DBZone ("Creatures");
		zones.Add (newzone);
		newzone = new DBZone ("Graveyard");
		newzone.UseSlots = newzone.StackAllInOneSlot = true;
		zones.Add (newzone);

		foreach (DBZone zone in zones) {
						zone.Default = true;
						DBZone enemyzone = new DBZone(zone);
						enemyzone.Name = "Enemy " + enemyzone.Name.ToLower();
						enemy_zones.Add (enemyzone);
				}
		newzone = new DBZone ("Grid");
		newzone.UseSlots = true;
		newzone.PlayerCanChooseSlot = true;
		newzone.Shared = true;
		zones.Add (newzone);
	}


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
