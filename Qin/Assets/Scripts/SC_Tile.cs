using System;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;
using static SC_Character;
using static SC_EditorTile;
using System.Collections.Generic;

[Serializable]
public class SC_Tile : NetworkBehaviour {

    public TDisplay CurrentDisplay { get; set; }

    [Header("Tile Variables")]
    [Tooltip("Movement cost to walk on this tile")]
    public int baseCost;

    public int Cost { get; set; }

    [Tooltip("Combat modifiers for this tile")]
    public SC_CombatModifiers combatModifers;

    public SC_CombatModifiers CombatModifiers { get { return Construction?.combatModifers ?? combatModifers; } }

    [Tooltip("Offset of the sprite")]
    public Vector2 spriteOffset;

    [SyncVar]
    public TileInfos infos;

    public int Region { get { return infos.region; } }    

    public bool CanAttack { get { return CurrentDisplay == TDisplay.Attack && !Empty; } }

	public bool CanCharacterAttack(SC_Character c) {

        if (Character)
            return c.Qin != Character.Qin;
        else if (Construction)
            return !c.Qin && AttackableContru;
        else if (Qin)
            return !c.Qin;
        else
            return true;

    }

    public bool Constructable {
        get /*(bool ignoreChara)*/ {

            return /*(!Character || ignoreChara) &&*/ !Hero && !Construction && RegionValid;

        }
    }

    bool RegionValid { get { return (Region != -1) && SC_Castle.castles[Region]; } }

    public SC_Construction Construction { get; set; }

    public SC_Construction AttackableContru { get { return (Construction?.maxHealth != 0) ? Construction : null; } }

    public SC_Village Village { get { return Construction as SC_Village; } }

    public SC_Castle Castle { get { return Construction as SC_Castle; } }

    public bool GreatWall { get { return Construction?.GreatWall ?? false; } }

    public SC_Pit Pit { get { return Construction as SC_Pit; } }

    public SC_DrainingStele DrainingStele { get { return Construction?.DrainingStele; } }

    public bool ProductionBuilding { get { return Construction?.production ?? false; } }

    public SC_Ruin Ruin { get { return Construction?.Ruin; } }

    public SC_Character Character { get; set; }

    public SC_Hero Hero { get { return Character as SC_Hero; } }

    public SC_Demon Demon { get { return Character as SC_Demon; } }

    public SC_Soldier Soldier { get { return Character as SC_Soldier; } }

    public SC_Qin Qin { get; set; }

    public bool Empty { get { return !Construction && !Character && !Qin; } }

    public bool Palace { get { return name.Contains("Palace"); } }

    public bool CursorOn { get; set; }

    // Used for PathFinder
    public SC_Tile Parent { get; set; }

	static SC_Game_Manager GameManager { get { return SC_Game_Manager.Instance; } }

	static SC_Tile_Manager TileManager { get { return SC_Tile_Manager.Instance; } }

    static SC_UI_Manager UIManager { get { return SC_UI_Manager.Instance; } }

    static SC_Fight_Manager FightManager { get { return SC_Fight_Manager.Instance; } }

    SpriteRenderer filter;

    public static bool CanChangeFilters { get { return (!activeCharacter || (activeCharacter.Qin != SC_Player.localPlayer.Qin)) && !SC_Player.localPlayer.Busy; } }

    public List<DemonAura> DemonAuras { get; set; }

    public static Sprite[] filters;

    void Awake () {

        DemonAuras = new List<DemonAura>();

    }

    public override void OnStartClient () {

        base.OnStartClient();

        CurrentDisplay = TDisplay.None;

        SetupTile();

        if (infos.region != -1) {

            for (int i = 0; i < transform.GetChild(2).childCount; i++)
                transform.GetChild(2).GetChild(i).gameObject.SetActive(infos.borders[i] && (i % 2 == 0));

        }        

    }

    public void SetupTile() {

        SC_Tile t = Resources.Load<SC_Tile>("Prefabs/Tiles/P_" + infos.type);

        baseCost = t.baseCost;
        Cost = baseCost;

        combatModifers = t.combatModifers;

        string s = infos.type == "Changing" ? "Changing" : infos.type + "/" + (infos.type == "River" ? (RiverSprite)infos.riverSprite + "" : infos.sprite + "");

        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tiles/" + s);

        transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (infos.type == "Mountain") ? -(transform.position.x.I() + transform.position.y.I()) : -Size;

        transform.GetChild(0).transform.localPosition = new Vector3(t.spriteOffset.x, t.spriteOffset.y);

    }

    void Start() {

        if (GameManager) {

            if (transform.position.x.I() == (GameManager.CurrentMapPrefab.SizeMapX - 1) && transform.position.y.I() == (GameManager.CurrentMapPrefab.SizeMapY - 1)) {

                if (!isServer)
                    GameManager.StartCoroutine("FinishConnecting");

            }

        }

        filter = transform.GetChild(1).GetComponent<SpriteRenderer>();

        if(UIManager)
            transform.parent = UIManager.tilesT;        

    }

    public void CursorClick() {

        SC_Sound_Manager.Instance.OnButtonClick();

        if (SC_UI_Manager.CanInteract && SC_Player.localPlayer.Turn) {

            if (CurrentDisplay == TDisplay.Construct) {

                SC_Player.localPlayer.CmdConstructAt(transform.position.x.I(), transform.position.y.I(), false);

            } else if (CurrentDisplay == TDisplay.Movement) {

                UIManager.TryDoAction(() => { StartMovement(gameObject); });                

            } else if (CanAttack && (SC_Hero.StaminaCost != SC_Hero.EStaminaCost.TooHigh)) {

                SC_UI_Manager.Instance.TryDoAction(() => {

                    activeCharacter.AttackTarget = this;

                    if (MovingCharacter) {

                        if (UIManager.previewFightPanel.activeSelf)
                            SC_Player.localPlayer.CmdSetChainAttack();

                        StartMovement((SC_Arrow.path?[SC_Arrow.path.Count - 1] ?? activeCharacter.Tile).gameObject);

                    } else {

                        SC_Cursor.SetLock(true);

                        TileManager.RemoveAllFilters();

                        activeCharacter.StartAttack();

                    }
                });                                

            } else if (CurrentDisplay == TDisplay.Sacrifice) {

                SC_Player.localPlayer.CmdChangeQinEnergy(Soldier.sacrificeValue);

                RemoveDisplay();

                Character.CanBeSelected = false;

                SC_Player.localPlayer.CmdDestroyCharacter(Character.gameObject);

            } /*else if (CurrentDisplay == TDisplay.Resurrection) {

                uiManager.EndQinAction("qinPower");

                SC_Qin.UsePower(transform.position);

            }*/

            else if (CurrentDisplay == TDisplay.None && !SC_Player.localPlayer.Busy && !activeCharacter) {

                if (Character && (Character.Qin == SC_Player.localPlayer.Qin))
                    Character.TrySelecting();
                else if (Pit && SC_Player.localPlayer.Qin)
                    Pit.SelectPit();
                else if (Castle && !Character && SC_Player.localPlayer.Qin) {
                    if (!SC_Demon.demons[Region])
                        UIManager.CreateDemon(Castle);
                    else if (SC_Demon.demons[Region].Alive == -1)
                        UIManager.DisplaySacrificeCastlePanel(Castle);
                } else
                    UIManager.ActivateMenu(UIManager.playerActionsPanel);

            }

        } else if (SC_UI_Manager.CanInteract && CurrentDisplay == TDisplay.None) {

            UIManager.ActivateMenu(UIManager.playerActionsPanel);

        }

    }

    public void OnCursorEnter() {

        CursorOn = true;

        if (CanAttack && !MovingCharacter)
            UIManager.PreviewFight(activeCharacter.Tile);
        else if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();

        SC_Arrow.CursorMoved(this);

        if (!UIManager.previewFightPanel.activeSelf) { 

            Character?.ShowInfos();
            Qin?.ShowInfos();

        } else {

            UIManager.HideInfosIfActive(activeCharacter.gameObject);

        }

        if (!Character && SC_Player.localPlayer.Turn)
            activeCharacter?.ShowInfos();

        this.ShowInfos();

    }

    public void OnCursorExit() {

        CursorOn = false;

        UIManager.HidePreviewFight();

        if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();

        UIManager.HideInfos(CanChangeFilters);

    }

    public void SetFilter(TDisplay displayFilter, bool preview = false) {

        if (displayFilter == TDisplay.None) {

            filter.enabled = false;

        } else {

            filter.color = new Color(1, 1, 1, preview ? .7f : 1);

            filter.sprite = filters[(int)Enum.Parse(typeof(TDisplay), displayFilter.ToString())];

            filter.enabled = true;

        }

	}

	public void RemoveDisplay() {

        CurrentDisplay = TDisplay.None;

        filter.enabled = false;

	}

    public void ChangeDisplay(TDisplay d) {

        CurrentDisplay = d;

        SetFilter(d);

    }

    public void ChangeDisplay(TDisplay d, bool p) {

        if (p)
            SetFilter(d, true);
        else
            ChangeDisplay(d);

    }

    public void TryAddAura (string demon, SC_CombatModifiers aura) {

        bool notHere = true;

        foreach (DemonAura dA in DemonAuras)
            if (dA.demon == demon)
                notHere = false;

        if (notHere)
            DemonAuras.Add(new DemonAura(demon, aura));

    }   
    
    public int DemonsModifier (string id, bool qin) {

        int modif = 0;

        foreach (DemonAura dA in DemonAuras)
            modif += (int)dA.aura.GetType().GetField(id).GetValue(dA.aura) * (qin ? 1 : -1);

        return modif;

    }

}
