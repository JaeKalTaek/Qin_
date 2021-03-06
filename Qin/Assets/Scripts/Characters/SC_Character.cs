﻿using System.Collections;
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

    public SC_Tile CombatTile { get { return this == activeCharacter ? SC_Arrow.path?[SC_Arrow.path.Count - 1] ?? Tile : Tile; } }

    public int StrengthModifiers { get; set; }
    public int Strength { get { return Mathf.Max(0, baseStats.strength + StrengthModifiers + CombatTile.CombatModifiers.strength + DemonsModifier("strength")); } }
     
    public int ChiModifiers { get; set; }
    public int Chi { get { return Mathf.Max(0, baseStats.chi + ChiModifiers + CombatTile.CombatModifiers.chi + DemonsModifier("chi")); } }

    public int ArmorModifiers { get; set; }
    public int Armor { get { return baseStats.armor + ArmorModifiers + CombatTile.CombatModifiers.armor + DemonsModifier("armor"); } }

    public int ResistanceModifiers { get; set; }
    public int Resistance { get { return baseStats.resistance + ResistanceModifiers + CombatTile.CombatModifiers.resistance + DemonsModifier("resistance"); } }

    public int PreparationModifiers { get; set; }
    public int Preparation { get { return Mathf.Max(1, baseStats.preparation - PreparationModifiers - CombatTile.CombatModifiers.preparation - DemonsModifier("preparation")); } }
    public int PreparationCharge { get; set; }
    public bool Prepared { get { return PreparationCharge >= Preparation; } }

    public int AnticipationModifiers { get; set; }
    public int Anticipation { get { return Mathf.Max(1, baseStats.anticipation - AnticipationModifiers - CombatTile.CombatModifiers.anticipation - DemonsModifier("anticipation")); } }
    public int AnticipationCharge { get; set; }
    public bool Anticipating { get { return AnticipationCharge >= Anticipation; } }

    [Tooltip ("Weapons of this character")]
    public List<SC_Weapon> weapons;

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

    public bool RealHero { get { return (!Hero?.Qin) ?? false; } }

    public SC_BaseQinChara BaseQinChara { get { return this as SC_BaseQinChara; } }

    public SC_Soldier Soldier { get { return this as SC_Soldier; } }

    public SC_Demon Demon { get { return this as SC_Demon; } }

    public SC_Tile AttackTarget { get; set; }

    public static bool ChainAttack { get; set; }

    public SC_Tile LastPos { get; set; }

    public int Stunned { get; set; }

    #region Managers
    protected static SC_Tile_Manager TileManager { get { return SC_Tile_Manager.Instance; } }

	protected static SC_Game_Manager gameManager;

	protected static SC_UI_Manager uiManager;

    protected static SC_Fight_Manager fightManager;

    protected SC_Common_Characters_Variables CommonVariables { get { return gameManager.CommonCharactersVariables; } }
    #endregion

    List<SC_Tile> path;

    public bool Moving { get; set; }

    public static SC_Character activeCharacter;

    public static bool MovingCharacter { get { return activeCharacter?.Moving ?? false; } }

    public SC_Tile Tile { get { return TileManager.GetTileAt(gameObject); } }

    [HideInInspector]
    [SyncVar]
    public string characterPath;    

    protected SC_Character loadedCharacter;

    public SpriteRenderer Sprite { get; set; }

    public int PreviousHealth { get; set; }

    public virtual bool IsInvulnerable { get { return false; } }

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

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;               	

		Tile.Character = this;

        Tile.UpdateFog ();

        transform.SetPos(transform.position, "Character");        

    }

    #region Movement
    public virtual void TrySelecting () {

        activeCharacter = this;

        Moving = true;

        TileManager.CheckMovements(this);

        uiManager.backAction = gameManager.UnselectCharacter;

    }

    public static void StartMovement (GameObject target) {

        SC_Hero.SetStaminaCost (new int[] { -1 });

        SC_Cursor.SetLock(true);

        SC_Arrow.DestroyArrow();

        uiManager.backAction = DoNothing;

        SC_Player.localPlayer.Busy = true;

        TileManager.RemoveAllFilters();

        SC_Player.localPlayer.CmdMoveCharacterTo(activeCharacter.gameObject, target);

    }

    public void MoveTo(SC_Tile target) {

        if (gameObject == uiManager.CurrentChara)
            TileManager.RemoveAllFilters (true);

        activeCharacter = this;

        LastPos = Tile;

        path = TileManager.PathFinder(LastPos, target);

        if (path == null)
            FinishMovement (false);
        else {

            Tile.Character = null;

            Tile.UpdateFog ();

            StartCoroutine (Move ());

        }

    }    

	IEnumerator Move() {

        gameManager.TryFocusOn (transform.position);

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

            target.Character = this;

            target.UpdateFog ();

        }        

        Moving = false;

        bool canAttack = false;

        foreach (SC_Tile tile in TileManager.GetAttackTiles())
            if (!tile.Empty && tile.CanCharacterAttack(this))
                canAttack = true;

        uiManager.attackButton.SetActive(canAttack);

        if (Hero) {

            uiManager.destroyConstruButton.SetActive(RealHero && (target.ProductionBuilding || target.Ruin));

            if (moved) {

                if (RealHero)
                    SC_DrainingStele.UpdateHeroSlow(Hero);

                Hero.ReadyToRegen = false;

                Hero.Hit(Hero.MovementCost(path.Count - 1), true);

                Hero.MovementPoints = (Hero.MovementPoints > path.Count - 1) ? Hero.MovementPoints - (path.Count - 1) : Hero.Movement;

                Hero.MovementCount += Hero.MovementPoints == Hero.Movement ? 1 : 0;                

            }

        } else if (Soldier) {

            uiManager.buildConstruButton.SetActive(SC_Player.localPlayer.Qin && (target.Ruin || (Soldier.Builder && !target.Construction)));

        } else if (Demon && moved) {

            uiManager.buildConstruButton.SetActive(false);

            Demon.RemoveAura(LastPos);

            Demon.AddAura();

        }

        if (activeCharacter) {

            if (moved) {

                this.TryRefreshInfos ();

                SC_Tile t = uiManager.CurrentTile?.GetComponent<SC_Tile> ();

                if (t && t.CursorOn)
                    t.TryRefreshInfos ();

            }

            if (ChainAttack) {

                ChainAttack = false;

                if (SC_Player.localPlayer.Turn)
                    StartAttack ();

            } else if (SC_Player.localPlayer.Turn) {

                if (SC_Cursor.Tile.CanCharacterAttack (this)) {

                    uiManager.ChooseWeapon ();

                    uiManager.backAction = () => {

                        ResetMovementFunction ();

                        uiManager.HideWeapons ();

                    };

                } else {

                    TileManager.PreviewAttack ();

                    uiManager.ActivateMenu (uiManager.characterActionsPanel);

                    uiManager.backAction = gameManager.ResetMovement;

                }

            }

        } else {

            if (SC_Player.localPlayer.Turn)
                gameManager.FinishAction ();
            else
                gameManager.CheckHeroTrapActivated ();

        }

    }

    public void SetCharacterPos (SC_Tile newPos) {        

        Tile.Character = null;

        LastPos = Tile;

        transform.SetPos (newPos.transform.position);

        if (RealHero)
            SC_DrainingStele.UpdateHeroSlow (Hero);

        newPos.Character = this;

        newPos.UpdateFog ();

    }

    public void ResetMovementFunction () {

        uiManager.characterActionsPanel.SetActive(false);

        if (Hero && Tile != LastPos) {            

            if (Hero.BaseMovementDone && Hero.MovementPoints == Hero.Movement) {

                Hero.MovementCount--;

                Hero.MovementPoints = SC_Tile_Manager.TileDistance(Tile, LastPos);

            } else
                Hero.MovementPoints += SC_Tile_Manager.TileDistance(Tile, LastPos);

            if (Hero.BaseMovementDone) {

                Hero.Health += Hero.MovementCost(SC_Tile_Manager.TileDistance(Tile, LastPos));

                Hero.UpdateHealth();

            }

        }

        TileManager.RemoveAllFilters ();

        Demon?.RemoveAura ();        

        SetCharacterPos (LastPos);

        LastPos.UpdateFog ();

        Demon?.AddAura ();

        CanBeSelected = true;

        this.TryRefreshInfos ();

        if (SC_Player.localPlayer.Turn) {            

            Moving = true;         

            uiManager.backAction = gameManager.UnselectCharacter;

            TileManager.CheckMovements(this);

            SC_Player.localPlayer.Busy = false;

            SC_Cursor.SetLock(false);

        }                

    }
    #endregion

    public bool CanCounterAttack (bool killed, int range) {

        return GetRange ().In (range) && !killed && !Tired;

    }

    public void StartAttack () {

        if (CanAttackWithWeapons (Tile).Count == 1)
            SC_Player.localPlayer.CmdAttack (AttackTarget.gameObject, CanAttackWithWeapons (Tile)[0]);
        else
            uiManager.ChooseWeapon ();

    }

    public static void FinishCharacterAction (bool wait = false) {

        activeCharacter.CapStats ();

        if (activeCharacter.Hero) {

            activeCharacter.Hero.ActionCount += wait ? 0 : 1;

            if (activeCharacter.AttackTarget) {

                activeCharacter.AttackTarget = null;

                activeCharacter.Hit (activeCharacter.Hero.ActionCost, true);

            }

            if (activeCharacter) {

                activeCharacter.AttackTarget = null;

                if (!wait)
                    activeCharacter.Hero.IncreaseRelationships (gameManager.CommonCharactersVariables.relationGains.action);

                SC_Sound_Manager.Instance.SetTempo ();              

            }

            SC_Hero.SetStaminaCost (new int[] { -1 });

        } else {

            activeCharacter.AttackTarget = null;

            activeCharacter.SetTired (true);

            activeCharacter.Demon?.CapStatsInAura ();

        }

        activeCharacter = null;

    }   

    public virtual void Stun (int duration) {

        SetTired (true);

        Stunned += duration + (Qin == gameManager.QinTurn ? 1 : 0);

    }

	public virtual void DestroyCharacter() {

        if (activeCharacter == this)
            activeCharacter = null;

        uiManager.HideInfosIfActive(gameObject);

        Tile.Character = null;

        Tile.UpdateFog ();

    }

	public SC_Weapon GetActiveWeapon() {

        return weapons[0];

	}	

	public virtual bool Hit (int damages, bool stamina = false) {        

        if (Health >= 0 && (!IsInvulnerable || stamina)) {

            PreviousHealth = Health;

            Health -= damages;

            if (Health <= 0)
                DestroyCharacter ();
            else
                UpdateHealth ();

        }

        return (Health <= 0);

	}

    public void UpdateHealth() {

        Lifebar.UpdateGraph(Health, MaxHealth);
        this.TryRefreshInfos ();

    }	

    public bool Tired { get; set; }

    public virtual void SetTired (bool tired) {

        if (tired)
            CanBeSelected = false;

        Tired = tired;        

        Sprite.color = Tired ? tiredColor : BaseColor;

    }

    public Vector2 GetRange (SC_Tile t = null) {

        float min = float.MaxValue;
        float max = 0;

        foreach (SC_Weapon w in weapons) {

            min = Mathf.Min (min, w.minRange);
            max = Mathf.Max (max, w.Range (this, t).y);

        }

        return new Vector2 (min, max);

    }

    public void SetActiveWeapon (int index) {

        if (index == 0) return;

        SC_Weapon temp = weapons[0];
        weapons[0] = weapons[index];
        weapons[index] = temp;

    }

    public List<int> CanAttackWithWeapons (SC_Tile from) {

        List<int> indexes = new List<int> ();

        foreach (SC_Weapon w in weapons)
            if (w.Range (this, from).In (SC_Tile_Manager.TileDistance (from, SC_Cursor.Tile)))
                indexes.Add (weapons.IndexOf (w));

        return indexes;

    }

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
            return (Qin || t.Construction.Health == 0);
        else if (t.Qin)
            return Qin;
        else
            return true;

    }

    public virtual bool CanCharacterSetOn (SC_Tile t) {

        if (t.Grave)
            return false;
        else if ((t.Character && (t.Character != this)) || t.Qin)
            return false;
        else if (t.Construction)
            return (Qin || !t.Construction.GreatWall) && !t.DrainingStele;
        else
            return true;

    }

    public int GetCurrentPropertyValue (string p) {

        bool wasActive = this == activeCharacter;

        if (wasActive)
            activeCharacter = null;

        int value = (int)GetType ().GetProperty (p).GetValue (this);

        if (wasActive)
            activeCharacter = this;

        return value;

    }

    public void CapStats () {

        AnticipationCharge = Mathf.Min (AnticipationCharge, Anticipation);

        PreparationCharge = Mathf.Min (PreparationCharge, Preparation);

    }

}
