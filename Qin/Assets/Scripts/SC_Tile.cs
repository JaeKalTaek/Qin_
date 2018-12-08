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

    [SyncVar]
    public TileInfos infos;

    public int Region { get { return infos.region; } }    

	public bool CanCharacterAttack(SC_Character c) {

        if (Character)
            return c.Qin != Character.Qin;
        else if (Construction)
            return !c.Qin && (Construction.GreatWall || Pump);
        else if (Qin)
            return !c.Qin;
        else
            return true;

    }

    public bool Constructable (bool ignoreChara) {

        return (!Character || ignoreChara) && !Construction && RegionValid;

    }

    bool RegionValid { get { return (Region != -1) && SC_Castle.castles[Region]; } }

    public SC_Construction Construction { get; set; }

    public SC_Village Village { get { return Construction as SC_Village; } }

    public SC_Bastion Bastion { get { return Construction as SC_Bastion; } }

    public SC_Castle Castle { get { return Construction as SC_Castle; } }

    public bool GreatWall { get { return Construction?.GreatWall ?? false; } }

    public SC_Workshop Workshop { get { return Construction as SC_Workshop; } }

    public SC_Pump Pump { get { return Construction?.Pump; } }

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

	static SC_Game_Manager gameManager;

	static SC_Tile_Manager tileManager;

    static SC_UI_Manager uiManager;

    static SC_Fight_Manager fightManager;

    SpriteRenderer filter;

    public static bool CanChangeFilters { get { return (!characterToMove || (characterToMove.Qin != SC_Player.localPlayer.Qin)) && !SC_Player.localPlayer.Busy; } }

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

            for (int i = 0; i < transform.GetChild(1).childCount; i++)
                transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().sprite = infos.borders[i] ? Resources.Load<Sprite>("Sprites/RegionBorders/" + (Region)infos.region) : null;

        }        

    }

    public void SetupTile() {

        SC_Tile t = Resources.Load<SC_Tile>("Prefabs/Tiles/P_" + infos.type);

        baseCost = t.baseCost;
        Cost = baseCost;

        combatModifers = t.combatModifers;

        string s = infos.type == "Changing" ? "Changing" : infos.type + "/" + (infos.type == "River" ? (RiverSprite)infos.riverSprite + "" : infos.sprite + "");

        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tiles/" + s);

    }

    void Start() {

        if(!gameManager)
            gameManager = SC_Game_Manager.Instance;

        if(!tileManager)
            tileManager = SC_Tile_Manager.Instance;

        if(!uiManager)
            uiManager = SC_UI_Manager.Instance;

        if (!fightManager)
            fightManager = SC_Fight_Manager.Instance;

        if (transform.position.x.I() == (gameManager.CurrentMapPrefab.SizeMapX - 1) && transform.position.y.I() == (gameManager.CurrentMapPrefab.SizeMapY - 1) && !isServer)
            gameManager.StartCoroutine("FinishConnecting");

        filter = transform.GetChild(0).GetComponent<SpriteRenderer>();

        transform.parent = uiManager.tilesT;        

    }

    public void CursorClick() {

        if (SC_UI_Manager.CanInteract && SC_Player.localPlayer.Turn) {

            if (CurrentDisplay == TDisplay.Construct) {

                SC_Player.localPlayer.CmdConstructAt(transform.position.x.I(), transform.position.y.I());

            } else if (CurrentDisplay == TDisplay.Movement) {

                uiManager.cancelAction = DoNothing;             

                SC_Player.localPlayer.Busy = true;

                SC_Player.localPlayer.CmdMoveCharacterTo(transform.position.x.I(), transform.position.y.I());

            } else if (CurrentDisplay == TDisplay.Attack && !Empty) {

                tileManager.RemoveAllFilters();

                fightManager.AttackRange = SC_Tile_Manager.TileDistance(attackingCharacter.transform.position, this);

                SC_Player.localPlayer.CmdPrepareForAttack(fightManager.AttackRange, gameObject, !SC_Player.localPlayer.Qin);

                if (attackingCharacter.Hero)
                    uiManager.ChooseWeapon();
                else
                    SC_Player.localPlayer.CmdAttack();

            } else if (CurrentDisplay == TDisplay.Sacrifice) {

                SC_Player.localPlayer.CmdChangeQinEnergy(Soldier.sacrificeValue);

                RemoveDisplay();

                Character.CanMove = false;

                SC_Player.localPlayer.CmdDestroyCharacter(Character.gameObject);

            } /*else if (CurrentDisplay == TDisplay.Resurrection) {

                uiManager.EndQinAction("qinPower");

                SC_Qin.UsePower(transform.position);

            }*/

            if (CurrentDisplay == TDisplay.None && !SC_Player.localPlayer.Busy) {

                if (Character && (Character.Qin == SC_Player.localPlayer.Qin))
                    Character.TryCheckMovements();
                else if (SC_Player.localPlayer.Qin) {
                    if (Workshop)
                        Workshop.SelectWorkshop();
                    else if (Castle && !Character)
                        uiManager.CastleMenu();
                } else
                    uiManager.ActivateMenu(true);

            }

        } else if (SC_UI_Manager.CanInteract && CurrentDisplay == TDisplay.None) {

            uiManager.ActivateMenu(true);

        }

    }

    public void OnCursorEnter() {

        CursorOn = true;

        if (CurrentDisplay == TDisplay.Attack)
            Hero?.PreviewFight();
        else if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();
        else {

            Character?.ShowInfos();
            Qin?.ShowInfos();            

        }

        this.ShowInfos();

    }

    public void OnCursorExit() {

        CursorOn = false;

        if (CurrentDisplay == TDisplay.Attack && Hero)
            uiManager.HidePreviewFight();
        else if (CurrentDisplay == TDisplay.Sacrifice)
            Soldier.ToggleDisplaySacrificeValue();

        uiManager.HideInfos(CanChangeFilters);

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
