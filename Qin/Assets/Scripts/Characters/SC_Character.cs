using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public abstract class SC_Character : NetworkBehaviour {

    [Header("Character variables")]
    [Tooltip("Name of this character")]
    public string characterName;

    [Tooltip("Offset of the sprite of this character")]
    public Vector2 spriteOffset;

    #region Stats  
    public BaseCharacterStats baseStats;

    public int MaxHealth { get { return baseStats.maxHealth; } }
    public int Health { get; set; }
    public SC_Lifebar Lifebar { get; set; }

    public int StrengthModifiers { get; set; }
    public int Strength { get { return Mathf.Max(0, baseStats.strength + StrengthModifiers + Tile.CombatModifiers.strength + DemonsModifier("strength")); } }

    public int ChiModifiers { get; set; }
    public int Chi { get { return Mathf.Max(0, baseStats.chi + ChiModifiers + Tile.CombatModifiers.chi + DemonsModifier("chi")); } }

    public int ArmorModifiers { get; set; }
    public int Armor { get { return baseStats.armor + ArmorModifiers + Tile.CombatModifiers.armor + DemonsModifier("armor"); } }

    public int ResistanceModifiers { get; set; }
    public int Resistance { get { return baseStats.resistance + ResistanceModifiers + Tile.CombatModifiers.resistance + DemonsModifier("resistance"); } }

    public int TechniqueModifiers { get; set; }
    public int Technique { get { return Mathf.Max(0, baseStats.technique + TechniqueModifiers + Tile.CombatModifiers.technique + DemonsModifier("technique")); } }
    public int CriticalAmount { get; set; }

    public int ReflexesModifiers { get; set; }
    public int Reflexes { get { return Mathf.Max(0, baseStats.reflexes + ReflexesModifiers + Tile.CombatModifiers.reflexes + DemonsModifier("reflexes")); } }
    public int DodgeAmount { get; set; }

    [Tooltip("Time for a character to walk one tile of distance")]
    public float moveDuration;
    public int MovementModifiers { get; set; }
    public int Movement { get { return Mathf.Max(0, baseStats.movement + MovementModifiers - (Hero?.DrainingSteleSlow ?? 0) + Tile.CombatModifiers.movement + DemonsModifier("movement")); } }
    public bool CanBeSelected { get; set; }    

    public int RangeModifiers { get; set; }      
    #endregion

    [Tooltip("Color applied when the character is tired")]
    public Color tiredColor = new Color(.15f, .15f, .15f);

    public Color BaseColor { get; set; }	

    public bool Qin { get; set; }

    public SC_Hero Hero { get { return this as SC_Hero; } }

    public SC_BaseQinChara BaseQinChara { get { return this as SC_BaseQinChara; } }

    public SC_Soldier Soldier { get { return this as SC_Soldier; } }

    public SC_Demon Demon { get { return this as SC_Demon; } }

    public SC_Tile AttackTarget { get; set; }

    public static bool ChainAttack { get; set; }

    public SC_Tile LastPos { get; set; }

    #region Managers
    protected static SC_Tile_Manager tileManager;

	protected static SC_Game_Manager gameManager;

	protected static SC_UI_Manager uiManager;

    protected static SC_Fight_Manager fightManager;
    #endregion

    List<SC_Tile> path;

    public bool Moving { get; set; }

    public static SC_Character activeCharacter;

    public static bool MovingCharacter { get { return activeCharacter?.Moving ?? false; } }

    public SC_Tile Tile { get { return tileManager.GetTileAt(gameObject); } }

    [HideInInspector]
    [SyncVar]
    public string characterPath;    

    protected SC_Character loadedCharacter;

    public SpriteRenderer Sprite { get; set; }

    protected virtual void Awake() {

        if (!gameManager)
            gameManager = SC_Game_Manager.Instance;

        Qin = !Hero;

        Sprite = GetComponentInChildren<SpriteRenderer>();

        BaseColor = Sprite.color;

        CanBeSelected = Qin == gameManager.QinTurn;        

    }

    public override void OnStartClient () {

        loadedCharacter = Resources.Load<SC_Character>(characterPath);      

        characterName = loadedCharacter.characterName;

        baseStats = new BaseCharacterStats(loadedCharacter.baseStats);

        moveDuration = loadedCharacter.moveDuration;

        Health = baseStats.maxHealth;

        Lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
        Lifebar.transform.position += new Vector3(0, -.44f, 0);        

        tiredColor = loadedCharacter.tiredColor;

        Sprite.sprite = loadedCharacter.GetComponentInChildren<SpriteRenderer>().sprite;
        Sprite.transform.localPosition += new Vector3(loadedCharacter.spriteOffset.x, loadedCharacter.spriteOffset.y, 0);

    }

    protected virtual void Start() {

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;               	

		LastPos = Tile;

        LastPos.Character = this;

        transform.SetPos(transform.position, 0);        

    }

    public static bool CanCharacterDoAction (int cost) { 

        return !activeCharacter.Hero || !activeCharacter.Hero.BaseActionDone || (activeCharacter.Health >= cost);

    }

    #region Movement
    public virtual void TrySelecting () {

        activeCharacter = this;

        Moving = true;

        tileManager.CheckMovements(this);

        uiManager.backAction = gameManager.UnselectCharacter;

    }

    public static void StartMovement (GameObject target) {

        activeCharacter.Hero?.SetStaminaCost(-1);

        SC_Cursor.SetLock(true);

        Destroy(SC_Arrow.arrow);

        uiManager.backAction = DoNothing;

        SC_Player.localPlayer.Busy = true;

        tileManager.RemoveAllFilters();

        SC_Player.localPlayer.CmdMoveCharacterTo(activeCharacter.gameObject, target);

    }

    public void MoveTo(SC_Tile target) {

        activeCharacter = this;

        LastPos = Tile;

        path = tileManager.PathFinder(LastPos, target);

        if(path == null)
            FinishMovement(false);
        else
            StartCoroutine(Move());

    }    

	IEnumerator Move() {

        SC_Sound_Manager.Instance.SetFootsteps(true);

        int pathIndex = 1;

        float movementTimer = 0;

        Sprite.sortingOrder = Size;

        Vector3 currentStart = transform.position;

        Vector3 currentEnd = new Vector3(path[1].transform.position.x, path[1].transform.position.y, 0);

        while (pathIndex < path.Count) {

            movementTimer = Mathf.Min(movementTimer + Time.deltaTime, moveDuration);

            transform.position = Vector3.Lerp(currentStart, currentEnd, movementTimer/moveDuration);

            if (movementTimer == moveDuration) {

                pathIndex++;

                if(pathIndex < path.Count) {

                    movementTimer = 0;

                    currentStart = transform.position;

                    currentEnd = new Vector3(path[pathIndex].transform.position.x, path[pathIndex].transform.position.y, 0);

                }

            }

			yield return new WaitForEndOfFrame();

		}

        SC_Sound_Manager.Instance.SetFootsteps(false);

        FinishMovement(true);

    }

    void FinishMovement(bool moved) {

        SC_Tile target = moved ? path[path.Count - 1] : LastPos;

        if(moved) {

            transform.SetPos(target.transform.position);

            LastPos.Character = null;

            target.Character = this;            

        }        

        Moving = false;

        bool canAttack = false;

        foreach (SC_Tile tile in tileManager.GetAttackTiles())
            if (!tile.Empty && tile.CanCharacterAttack(this))
                canAttack = true;

        uiManager.attackButton.SetActive(canAttack);

        if (Hero) {

            // CanMove = (Hero.Berserk && !Hero.BerserkTurn);

            uiManager.destroyConstruButton.SetActive(!SC_Player.localPlayer.Qin && (target.ProductionBuilding || target.Ruin));

            if (moved) {

                SC_DrainingStele.UpdateHeroSlow(Hero);

                Hero.ReadyToRegen = false;

                if (Hero.BaseActionDone)
                    Hero.Hit(path.Count - 1);

            }

        } else if (Soldier) {

            uiManager.buildConstruButton.SetActive(SC_Player.localPlayer.Qin && (target.Ruin || (Soldier.Builder && !target.Construction)));

        } else if (Demon && moved) {

            uiManager.buildConstruButton.SetActive(false);

            Demon.RemoveAura(false, LastPos);

            Demon.AddAura(false);

        }

        if (activeCharacter.gameObject.activeSelf) {

            if (moved) {

                uiManager.TryRefreshInfos(gameObject, GetType());

                SC_Tile t = uiManager.CurrentTile.GetComponent<SC_Tile>();

                if (t && t.CursorOn)
                    uiManager.TryRefreshInfos(t.gameObject, t.GetType());

            }

            if (ChainAttack) {

                ChainAttack = false;

                if (SC_Player.localPlayer.Turn)
                    StartAttack();

            } else if (SC_Player.localPlayer.Turn) {

                if (SC_Cursor.Tile.CanCharacterAttack(this)) {

                    uiManager.ChooseWeapon();

                    uiManager.backAction = () => {

                        ResetMovementFunction();

                        uiManager.HideWeapons();

                    };

                } else {

                    tileManager.PreviewAttack();

                    uiManager.ActivateMenu(uiManager.characterActionsPanel);

                    uiManager.backAction = gameManager.ResetMovement;

                }

            }

        } else {

            activeCharacter = null;

            if (SC_Player.localPlayer.Turn)
                gameManager.FinishAction();

        }

    }

    public void ResetMovementFunction () {

        uiManager.characterActionsPanel.SetActive(false);

        tileManager.RemoveAllFilters();

        if (Hero?.BaseActionDone ?? false) {

            Hero.Health += SC_Tile_Manager.TileDistance(Tile, LastPos);

            Hero.UpdateHealth();

        }

        Tile.Character = null;

        transform.SetPos(LastPos.transform.position);

        LastPos.Character = this;

        CanBeSelected = true;

        if (Hero) {

            SC_DrainingStele.UpdateHeroSlow(Hero);            

        }  else if (Demon) {

            Demon.RemoveAura(true, LastPos);

            Demon.AddAura(true);

        }

        if (SC_Player.localPlayer.Turn) {            

            Moving = true;         

            uiManager.backAction = gameManager.UnselectCharacter;

            tileManager.CheckMovements(this);

            SC_Player.localPlayer.Busy = false;

            SC_Cursor.SetLock(false);

        }                

    }
    #endregion

    public void StartAttack () {

        // SC_Player.localPlayer.CmdPrepareForAttack(AttackTarget.gameObject);

        if (Hero) {

            if (Hero.CanAttackWithWeapons(Tile).Count == 1)
                SC_Player.localPlayer.CmdHeroAttack(AttackTarget.gameObject, Hero.CanAttackWithWeapons(Tile)[0]);
            else
                uiManager.ChooseWeapon();

        } else
            SC_Player.localPlayer.CmdAttack(AttackTarget.gameObject);

    }

    public static void FinishCharacterAction() {

        if (activeCharacter.Hero) {

            if (activeCharacter.AttackTarget && activeCharacter.Hero.BaseActionDone)
                activeCharacter.Hit(gameManager.CommonCharactersVariables.staminaActionCost);

            if (activeCharacter.gameObject.activeSelf) {

                activeCharacter.Hero.IncreaseRelationships(gameManager.CommonCharactersVariables.relationGains.action);

                SC_Sound_Manager.Instance.SetTempo();

                // activeCharacter.Hero.BerserkTurn = activeCharacter.Hero.Berserk;

                activeCharacter.Hero.SetStaminaCost(-1);

                activeCharacter.Hero.BaseActionDone = true;

            }

        } else if (activeCharacter.BaseQinChara) {

            activeCharacter.CanBeSelected = false;

            activeCharacter.Tire();

        }

        activeCharacter.AttackTarget = null;

        activeCharacter = null;

    }   

	public virtual void DestroyCharacter() {

        uiManager.HideInfosIfActive(gameObject);

        Tile.Character = null;

	}

	public SC_Weapon GetActiveWeapon() {

		return Hero?.GetWeapon(true) ?? BaseQinChara.weapon;

	}	

	public virtual bool Hit(int damages) {

		Health -= damages;

        if (Health <= 0)
            DestroyCharacter();
        else
            UpdateHealth();

        return (Health <= 0);

	}

    public void UpdateHealth() {

        Lifebar.UpdateGraph(Health, MaxHealth);
        uiManager.TryRefreshInfos(gameObject, GetType());

    }	

	public virtual void Tire() {

		Sprite.color = tiredColor;

	}

	public virtual void UnTired() {

        Sprite.color = BaseColor;

	}

    public abstract Vector2 GetRange (SC_Tile t = null);

    public int Range (SC_Tile t) {

        return t.CombatModifiers.range + RangeModifiers + DemonsModifier("range", t);

    }

    int DemonsModifier(string id, SC_Tile t = null) {

        return Demon ? 0 : (t ?? Tile).DemonsModifier(id, Qin);

    }

    public virtual bool CanCharacterGoThrough (SC_Tile t) {

        if (t.Character)
            return Qin == t.Character.Qin;
        else if (t.Construction)
            return (Qin || !t.Construction.GreatWall) && !t.DrainingStele;
        else if (t.Qin)
            return Qin;
        else
            return true;

    }

    public virtual bool CanCharacterSetOn (SC_Tile t) {

        if ((t.Character && (t.Character != this)) || t.Qin)
            return false;
        else if (t.Construction)
            return (Qin || !t.Construction.GreatWall) && !t.DrainingStele;
        else
            return true;

    }

}
