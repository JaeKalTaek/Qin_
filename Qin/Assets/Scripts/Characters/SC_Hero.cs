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

    //Relationships	
    public Dictionary<string, int> Relationships { get; set; }
	public List<string> RelationshipKeys { get; set; }

    public string RelationGainBlocked { get; set; }

	//bool saved;

	//Berserk	
	/*public bool Berserk { get; set; }
    public bool BerserkTurn { get; set; }*/

    [Tooltip("Characteristics")]
    public bool male;

    //power	
    /*public bool PowerUsed { get; set; }
	public int PowerBacklash { get; set; }

    [Tooltip("Color applied when the character is berserker")]
    public Color berserkColor;*/

    #region Stamina system   
    public bool BaseActionDone { get { return ActionCount >= 0; } }

    public int ActionCount { get; set; }

    public int ActionCost { get { return BaseActionDone ? gameManager.CommonCharactersVariables.baseStaminaActionCost + gameManager.CommonCharactersVariables.staminaActionAdditionalCost * (ActionCount / gameManager.CommonCharactersVariables.staminaCostsAugmentation) : 0; } }

    public int MovementPoints { get; set; }

    public bool BaseMovementDone { get { return MovementCount >= 0; } }

    public int MovementCount { get; set; }

    public int MovementCost (int distance) { return BaseMovementDone ? distance * (gameManager.CommonCharactersVariables.baseStaminaMovementCost + gameManager.CommonCharactersVariables.staminaMovementAdditionalCost * (MovementCount / gameManager.CommonCharactersVariables.staminaCostsAugmentation)) : 0; }
    #endregion

    public bool ReadyToRegen { get; set; }

    public int DrainingSteleSlow { get; set; }

    public static List<SC_Hero> heroes;

    public bool TrapUsed { get; set; }

    public bool Dead { get; set; }

    public override void OnStartClient () {

        base.OnStartClient();

        weapons = new List<SC_Weapon> ();

        foreach (string s in Hero.deck.weapons)
            weapons.Add (Resources.Load<SC_Weapon> ("Prefabs/Characters/Components/Weapons/P_" + s));

        male = loadedCharacter.Hero.male;

        // berserkColor = loadedCharacter.Hero.berserkColor;

        if (heroes == null || (heroes.Count >= 6 && !clone))
            heroes = new List<SC_Hero>();
        
        heroes.Add(this);

        if (heroes.Count == 6)
            SetupHeroesRelationships();

        if (clone) {

            foreach (SC_Hero h in heroes)
                if (characterName == h.characterName && !h.clone)
                    original = h;

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

        ActionCount = -1;

        MovementCount = -1;

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

		if (CanBeSelected /*|| (Berserk && !BerserkTurn)*/)
            base.TrySelecting();

	}

    /*public static void Attack(bool usedActiveWeapon) {

        activeCharacter.Hero.SetWeapon(usedActiveWeapon);

        fightManager.Attack();

    }*/ 

    public void Heal (int amount) {

        Health = Mathf.Min (Health + amount, MaxHealth);

    }

	public void Regen() {

        // Health = Mathf.Min(Health + gameManager.CommonCharactersVariables.staminaRegen, MaxHealth);

        if (Tile.Village || Tile.Pit) {

            if (ReadyToRegen)
                Heal (gameManager.CommonCharactersVariables.prodConstructionRegen);
            else
                ReadyToRegen = true;

        }

        UpdateHealth();

	}

	/*public override bool Hit(int damages, bool saving) {

		bool dead = false;

		base.Hit(damages, saving);

		if (Health <= 0) {

			if (saving) {

                Health = gameManager.CommonCharactersVariables.savedHealthAmount;
				Berserk = true;
				BerserkTurn = true;

				GetComponent<SpriteRenderer> ().color = berserkColor;

			} else {

				SC_Hero saver = fightManager.CheckHeroSaved (this, saved);

				if (saver) {

					saver.Hit (damages, true);
					saved = true;
					Health += damages;

				} else {

					DestroyCharacter();
					dead = true;

				}

			}

		} else if (Health <= gameManager.CommonCharactersVariables.berserkTriggerHealth) {

			CanMove = !gameManager.Qin;

			BerserkTurn = true;

			if(!Berserk)
                GetComponent<SpriteRenderer>().color = berserkColor;

            Berserk = true;

		}

        if (!dead)
            UpdateHealth();

        return dead;

	}

	public override void Tire() {

		if (!Berserk || BerserkTurn) base.Tire ();

	}

	public override void UnTired() {

		if (Berserk)
			GetComponent<SpriteRenderer> ().color = berserkColor;
		else
			base.UnTired ();

	}*/

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

                        List<SC_Hero> heroesCopy = new List<SC_Hero> (heroes);

                        foreach (SC_Hero h in heroesCopy) {

                            if (h != this && h.characterName == characterName) {

                                heroes.Remove (h);

                                h.DestroyCharacter ();

                            }

                        }

                        SC_Qin.ChangeEnergy (SC_Qin.Qin.energyWhenHeroDies);

                        SC_Sound_Manager.Instance.AugmentPart ();

                    }

                    heroes.Remove (this);

                    if (heroes.Count <= 0)
                        uiManager.ShowVictory (true);

                    // gameManager.LastHeroDead = this;        

                    /*foreach (SC_Hero hero in heroes) {

                        int value = 0;
                        Relationships.TryGetValue (hero.characterName, out value);

                        if (value >= gameManager.CommonCharactersVariables.berserkTriggerRelation) {

                            hero.Berserk = true;
                            hero.BerserkTurn = true;
                            hero.CanMove = !gameManager.Qin;

                            hero.GetComponent<Renderer> ().material.color = Color.cyan;

                        }

                    }*/

                }

            } else {

                Destroy (gameObject);

            }

        }

	}

    public void IncreaseRelationships (int amount) {

        List<string> differentHeroesInRange = new List<string> ();

        foreach (SC_Hero hero in TileManager.HeroesInRange (this))
            if (hero.characterName != characterName && !differentHeroesInRange.Contains (hero.characterName))
                differentHeroesInRange.Add (hero.characterName);

        List<string> alreadyGained = new List<string> ();

        foreach (SC_Hero hero in TileManager.HeroesInRange (this)) {

            if (hero.characterName != characterName && hero.characterName != RelationGainBlocked && !alreadyGained.Contains (hero.characterName)) {

                alreadyGained.Add (hero.characterName);

                Instantiate (Resources.Load<GameObject> ("Prefabs/UI/P_RelationshipGainFeedback"), transform.position + Vector3.up * SC_Game_Manager.TileSize, Quaternion.identity);

                Instantiate (Resources.Load<GameObject> ("Prefabs/UI/P_RelationshipGainFeedback"), hero.transform.position + Vector3.up * SC_Game_Manager.TileSize, Quaternion.identity);

                Relationships[hero.characterName] += (int) (((float) amount / differentHeroesInRange.Count) + .5f);

            }

        }

        RelationGainBlocked = "";

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
