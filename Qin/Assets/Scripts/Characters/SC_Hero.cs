using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Hero : SC_Character {

    [HideInInspector]
    [SyncVar]
    public HeroDeck deck;

    [HideInInspector]
    [SyncVar]
    public bool clone;

    SC_Hero original;

    public SC_Tile StartingTile { get; set; }

    public Dictionary<string, int> Relationships { get; set; }
	public List<string> RelationshipKeys { get; set; }

    public string RelationGainBlocked { get; set; }

	public bool Berserk { get; set; }

    [Tooltip("Characteristics")]
    public bool male;

    #region Stamina system   
    public int StaminaCostAugmentation { get; set; }

    public bool BaseActionDone { get { return ActionCount >= 0; } }

    public int ActionCount { get; set; }

    public int ActionCost { get { return BaseActionDone ? CommonVariables.baseStaminaActionCost + CommonVariables.staminaActionAdditionalCost * (ActionCount / StaminaCostAugmentation) : 0; } }

    public int MovementPoints { get; set; }

    public bool BaseMovementDone => MovementCount >= 0;

    public int MovementCount { get; set; }

    public int MovementCost (int distance) { return BaseMovementDone ? distance * (CommonVariables.baseStaminaMovementCost + CommonVariables.staminaMovementAdditionalCost * (MovementCount / StaminaCostAugmentation)) : 0; }
    #endregion

    public bool ReadyToRegen { get; set; }

    public int DrainingSteleSlow { get; set; }

    public static List<SC_Hero> heroes;

    public bool TrapUsed { get; set; }

    public bool Dead { get; set; }

    public int HumansFateDuration { get; set; }

    public override bool IsInvulnerable => HumansFateDuration > 0;

    public override void OnStartClient () {

        base.OnStartClient();

        weapons = new List<SC_Weapon> ();

        foreach (string s in Hero.deck.weapons)
            weapons.Add (Resources.Load<SC_Weapon> ("Prefabs/Characters/Components/Weapons/P_" + s));

        male = loadedCharacter.Hero.male;

        if (heroes == null || (heroes.Count >= 6 && !clone))
            heroes = new List<SC_Hero>();
        
        heroes.Add(this);

        if (heroes.Count == 6)
            SetupHeroesRelationships();

        StaminaCostAugmentation = CommonVariables.staminaCostsAugmentation;

        if (clone) {

            foreach (SC_Hero h in heroes)
                if (characterName == h.characterName && !h.clone)
                    original = h;

            SetBerserk (original.Berserk);

        }

        MovementPoints = clone ? original.MovementPoints : loadedCharacter.baseStats.movement;       

    }

    protected override void Start () {

        base.Start();

        if (!clone)
            StartingTile = Tile;
        else {

            StartingTile = original.StartingTile;

            if (SC_Cursor.Tile == Tile)
                Tile.OnCursorEnter ();

        }

        transform.parent = uiManager.heroesT;

        ActionCount = clone ? original.ActionCount : -1;

        MovementCount = clone ? original.MovementCount : -1;

    }

    public static void SetupHeroesRelationships() {

        foreach (SC_Hero hero in heroes) {

            hero.Relationships = new Dictionary<string, int>();
            hero.RelationshipKeys = new List<string>();

            foreach (SC_Hero hero2 in heroes) {

                if (hero != hero2) {

                    hero.Relationships.Add(hero2.characterName, 0);
                    hero.RelationshipKeys.Add(hero2.characterName);

                }

            }

        }

    }

    public override void TrySelecting () {

		if (CanBeSelected)
            base.TrySelecting();

	}

    public void Heal (int amount) {

        Health = Mathf.Min (Health + amount, MaxHealth);

    }

	public void Regen() {

        if (Tile.Village || Tile.Pit) {

            if (ReadyToRegen)
                Heal (gameManager.CommonCharactersVariables.prodConstructionRegen);
            else
                ReadyToRegen = true;

        }

        UpdateHealth();

	}

	public void SetBerserk (bool b) {

        if ((b && !Berserk) || (!b && Berserk))  {

            foreach (string s in new string[] { "Strength", "Chi", "Armor", "Resistance", "Preparation", "Anticipation", "Movement", "Range" }) {

                int bonus = (b ? 1 : -1) * Mathf.Max (1, (int) ((s == "Range" ? 1 : (int) baseStats.GetType ().GetField (s.ToLower ()).GetValue (baseStats)) * (gameManager.CommonCharactersVariables.berserkStatsBonusPercentage / 100f) + .5f));

                GetType ().GetProperty (s + "Modifiers").SetValue (this, (int) GetType ().GetProperty (s + "Modifiers").GetValue (this) + bonus);

            }

            if (b)
                StaminaCostAugmentation *= CommonVariables.berserkStaminaThresholdMultiplier;
            else
                StaminaCostAugmentation /= CommonVariables.berserkStaminaThresholdMultiplier;

            Berserk = b;

        }         

    }

	public override void DestroyCharacter() {

        if (!Dead) {

            Dead = true;

            base.DestroyCharacter ();

            if (!Qin) {

                gameObject.SetActive (false);

                if (!TrapUsed && !clone) {

                    TrapUsed = true;

                    SC_HeroTraps.hero = this;

                    typeof (SC_HeroTraps).GetMethod (deck.trap).Invoke (SC_HeroTraps.Instance, null);

                }

                if (Health <= 0) {

                    if (!clone) {

                        SC_Game_Manager.LastHeroDead = this;

                        Tile.Grave = Instantiate (Resources.Load<GameObject> ("Prefabs/P_Grave"));

                        Tile.Grave.transform.SetPos (Tile.transform.position, 3);

                        List<SC_Hero> clones = new List<SC_Hero> (heroes);

                        foreach (SC_Hero h in clones)
                            if (h != this && h.characterName == characterName)
                                h.DestroyCharacter ();                        
                                              
                        SC_Qin.ChangeEnergy (SC_Qin.Qin.energyWhenHeroDies);

                        SC_Sound_Manager.Instance.AugmentPart ();

                    }

                    heroes.Remove (this);

                    if (heroes.Count <= 0)
                        uiManager.ShowVictory (true);

                }

            } else {

                Destroy (gameObject);

            }

        }

	}

    public void IncreaseRelationships (int amount) {

        if (!Qin) {

            List<string> differentHeroesInRange = new List<string> ();

            foreach (SC_Hero hero in TileManager.HeroesInRange (this))
                if (hero.characterName != characterName && !differentHeroesInRange.Contains (hero.characterName) && !hero.Qin)
                    differentHeroesInRange.Add (hero.characterName);

            List<string> alreadyGained = new List<string> ();

            foreach (SC_Hero hero in TileManager.HeroesInRange (this)) {

                if (hero.characterName != characterName && hero.characterName != RelationGainBlocked && !alreadyGained.Contains (hero.characterName) && !hero.Qin) {

                    alreadyGained.Add (hero.characterName);

                    Instantiate (Resources.Load<GameObject> ("Prefabs/UI/P_RelationshipGainFeedback"), transform.position + Vector3.up * SC_Game_Manager.TileSize, Quaternion.identity);

                    Instantiate (Resources.Load<GameObject> ("Prefabs/UI/P_RelationshipGainFeedback"), hero.transform.position + Vector3.up * SC_Game_Manager.TileSize, Quaternion.identity);

                    Relationships[hero.characterName] += (int) (((float) amount / differentHeroesInRange.Count) + .5f);

                }

            }

            RelationGainBlocked = "";

        }

    }

    #region Stamina system
    public enum EStaminaCost { NotNeeded, TooHigh, WillDie, Enough }

    public static EStaminaCost StaminaCost { get; set; }

    public static void SetStaminaCost (int[] cost) {

        StaminaCost = cost.Sum() <= 0 ? EStaminaCost.NotNeeded : ((activeCharacter.Health > cost.Sum()) ? EStaminaCost.Enough : ((cost.Length == 1 || cost[1] == 0 || activeCharacter.Health > cost[0]) ? EStaminaCost.WillDie : EStaminaCost.TooHigh));

        uiManager.DisplayStaminaCost(cost.Sum());        

    }
    #endregion

    public static void SendHeroesInfos () {

        SC_UI_Manager UI = SC_UI_Manager.Instance;

        HeroDeck[] decks = new HeroDeck[6];

        int count = 0;
        
        foreach (SC_Tile t in TileManager.tiles) {

            if (t.DeployedHero) {

                SC_HeroDeck heroDeck = null;

                foreach (SC_HeroDeck hD in UI.heroPreparationUI.heroDecks)
                    if (hD.Hero.Renderer.sprite == t.DeployedHero.SpriteR.sprite)
                        heroDeck = hD;

                List<string> weapons = new List<string> ();

                foreach (SC_PreparationSlot weapon in heroDeck.Weapons)
                    if (!weapon.IsDefault)
                        weapons.Add (weapon.Renderer.sprite.name);

                decks[count] = new HeroDeck (

                    t.transform.position,

                    heroDeck.Hero.Renderer.sprite.name,

                    heroDeck.Trap.Renderer.sprite.name,

                    weapons.ToArray ()

                );

                count++;

            }

        }

        SC_Player.localPlayer.CmdSendHeroesInfos (decks);

    }

}
