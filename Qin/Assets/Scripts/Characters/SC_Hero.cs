using System.Collections.Generic;
using UnityEngine;

public class SC_Hero : SC_Character {

	//Relationships	
	public Dictionary<string, int> Relationships { get; set; }
	public List<string> RelationshipKeys { get; set; }

	bool saved;

	//Berserk	
	public bool Berserk { get; set; }
    public bool BerserkTurn { get; set; }

	//Weapons
    [Header("Heroes Variables")]
    [Tooltip("Weapons of this hero")]
	public SC_Weapon weapon1, weapon2;

	//power	
	public bool PowerUsed { get; set; }
	public int PowerBacklash { get; set; }

    [Tooltip("Color applied when the character is berserker")]
    public Color berserkColor;

    public bool ReadyToRegen { get; set; }

    public int PumpSlow { get; set; }

    public static List<SC_Hero> heroes;

    public override void OnStartClient () {

        base.OnStartClient();

        weapon1 = loadedCharacter.Hero.weapon1;

        weapon2 = loadedCharacter.Hero.weapon2;

        berserkColor = loadedCharacter.Hero.berserkColor;

        if (heroes == null)
            heroes = new List<SC_Hero>();

        heroes.Add(this);

        if (heroes.Count == 6)
            SetupHeroesRelationships();

    }

    protected override void Start () {

        base.Start();

        transform.parent = uiManager.heroesT;

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

    public override void TryCheckMovements () {

		if (CanMove || (Berserk && !BerserkTurn))
            base.TryCheckMovements();

	}

	public void PreviewFight() {

		if (!attackingCharacter.Hero && SC_UI_Manager.CanInteract && SC_Player.localPlayer.Turn) {

            attackingCharacter.AttackTarget = Tile;

            fightManager.AttackRange = SC_Tile_Manager.TileDistance(attackingCharacter.transform.position, Tile);

            uiManager.PreviewFight(true);

		}

	}

    public static void Attack(bool usedActiveWeapon) {

        ((SC_Hero)attackingCharacter).SetWeapon(usedActiveWeapon);

        fightManager.Attack();

    }    

	public void Regen() {

		if (Tile.Village) {

            if (ReadyToRegen) {

                Health = Mathf.Min(Health + gameManager.CommonCharactersVariables.villageRegen, MaxHealth);
                UpdateHealth();

            } else {

                ReadyToRegen = true;

            }

        }

	}

	public override bool Hit(int damages, bool saving) {

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

	}

	public override void DestroyCharacter() {

		base.DestroyCharacter();

		SC_Qin.ChangeEnergy (SC_Qin.Qin.energyWhenHeroDies);

		gameManager.LastHeroDead = this;        

		foreach (SC_Hero hero in heroes) {

			int value = 0;
			Relationships.TryGetValue (hero.characterName, out value);

			if (value >= gameManager.CommonCharactersVariables.berserkTriggerRelation) {

				hero.Berserk = true;
				hero.BerserkTurn = true;
				hero.CanMove = !gameManager.Qin;

				hero.GetComponent<Renderer> ().material.color = Color.cyan;

			}

		}

		gameObject.SetActive (false);

        heroes.Remove(this);


        if (heroes.Count <= 0)
			uiManager.ShowVictory (true);

	}

	public SC_Weapon GetWeapon(bool active) {

		return active ? weapon1 : weapon2;

	}

	public void SetWeapon(bool activeWeaponUsed) {

		if(!activeWeaponUsed) {

			SC_Weapon temp = GetWeapon(true);
			weapon1 = weapon2;
			weapon2 = temp;

		}

	}

    public override Vector2 GetRange (SC_Tile t = null) {

        if (!t)
            t = Tile;

        return new Vector2(Mathf.Min(weapon1.minRange, weapon2.minRange), Mathf.Max(weapon1.MaxRange(this, t), weapon2.MaxRange(this, t)));

    }

}
