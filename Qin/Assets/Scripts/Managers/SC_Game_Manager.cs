using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using static SC_EditorTile;
using static SC_Global;
using System.Reflection;
using QinCurses;
using System.Collections.Generic;

public class SC_Game_Manager : NetworkBehaviour {

    public SC_MapEditorScript mapPrefab;

    public SC_Common_Characters_Variables CommonCharactersVariables { get; set; }

    public SC_CommonQinVariables CommonQinVariables { get; set; }

    public static SC_Game_Manager Instance { get; set; }  

    public bool QinTurn { get; set; }

    public bool QinTurnStarting { get; set; }

	public static SC_Hero LastHeroDead { get; set; }

    public string CurrentConstru { get; set; }

	public Vector3 CurrentPitPos { get; set; }

    public SC_Castle CurrentCastle { get; set; }

	public SC_Player Player { get; set; }

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;

    public bool ServerStarted { get; set; }

    public bool FocusOn { get; set; }

    public int ScorchedRegion { get; set; }

    public bool? AdditionalTurn { get; set; }

    public static bool otherGM;

    public List<string> elementLayers;

    public bool PrepPhase { get { return uiManager.preparationPanel.activeSelf; } }

    #region Setup
    private void Awake () {

        GameObject bg = Instantiate (Resources.Load<GameObject> ("Prefabs/P_Background"));
        bg.GetComponent<SpriteRenderer> ().size = new Vector2 (mapPrefab.SizeMapX + 10, mapPrefab.SizeMapY + 10);
        bg.transform.position = new Vector3 ((mapPrefab.SizeMapX - 1) / 2f, (mapPrefab.SizeMapY - 1) / 2f, 0);

        ScorchedRegion = -1;

        FocusOn = true;

        Instance = this;

        uiManager = FindObjectOfType<SC_UI_Manager>();    

        SC_Village.number = 0;

        SC_Castle.castles = new bool[6];

        SC_Demon.demons = new SC_Demon[6];

        SC_Tile.filters = Resources.LoadAll<Sprite>("Sprites/Tiles/Filters");

        CommonCharactersVariables = Resources.Load<SC_Common_Characters_Variables>("Prefabs/Characters/P_Common_Characters_Variables");

        CommonQinVariables = Resources.Load<SC_CommonQinVariables>("Prefabs/P_CommonQinVariables");

        QinTurn = false;

    }

    public override void OnStartServer () {

        ServerStarted = true;

        StartCoroutine("WaitForPlayer");

    }

    IEnumerator WaitForPlayer () {

        while (!Player || !otherGM)
            yield return null;

        GenerateMap();
        SetupTileManager();

    }

    void GenerateMap() {

        foreach (Transform child in mapPrefab.transform) {

            SC_EditorTile eTile = child.GetComponent<SC_EditorTile>();

            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/P_BaseTile"), child.position, Quaternion.identity, GameObject.Find("Tiles").transform);

            bool[] borders = new bool[4];

            for (int i = 0; i < eTile.transform.GetChild(2).childCount; i++)
                borders[i] = eTile.transform.GetChild(2).GetChild(i).GetComponent<SpriteRenderer>().sprite;

            go.GetComponent<SC_Tile>().infos = new TileInfos(
                eTile.tileType.ToString(),
                eTile.heroDeployTile,
                (int)eTile.region,
                borders
            );

            NetworkServer.Spawn(go);

        }

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (Resources.Load<GameObject>("Prefabs/P_Tile_Manager"));

        tm.GetComponent<SC_Tile_Manager> ().qinIsServerForStart = Player.Qin;

		NetworkServer.Spawn (tm);

	}

	public void FinishSetup() {

        tileManager = SC_Tile_Manager.Instance;

		if (isServer)
			GenerateElements ();

	}

    public GameObject TryLoadConstruction (string c) {

        return TryLoadConstruction("", c) ?? TryLoadConstruction("Production/", c) ?? TryLoadConstruction("Special/", c);

    }

    GameObject TryLoadConstruction(string p, string c) {

        return Resources.Load<GameObject>("Prefabs/Constructions/" + p + "P_" + c);

    }

	void GenerateElements() {

		foreach (Transform child in mapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

            if (eTile.construction != ConstructionType.None) {

                GameObject constructionPrefab = TryLoadConstruction(eTile.construction.ToString());

                GameObject go = Instantiate(constructionPrefab, eTile.transform.position, Quaternion.identity);

                NetworkServer.Spawn (go);                

			}

            if ((eTile.soldier != SoldierType.None) || eTile.Qin) {

                string basePath = "Prefabs/Characters/" + (eTile.soldier != SoldierType.None ? "Soldiers/P_BaseSoldier" : (eTile.Qin ? "P_Qin" : "Heroes/P_BaseHero"));

                GameObject go = Instantiate(Resources.Load<GameObject>(basePath), eTile.transform.position, Quaternion.identity);

                if(!eTile.Qin)
                    go.GetComponent<SC_Character>().characterPath = "Prefabs/Characters/" + (eTile.soldier != SoldierType.None ? "Soldiers/Basic/P_" + eTile.soldier : "Heroes/P_" + eTile.Hero);

                NetworkServer.Spawn(go);

            }

        }

	}    

    public IEnumerator FinishConnecting() {

        while(!Player)
            yield return null;

        Player.CmdFinishConnecting();

    }

    public IEnumerator Load () {

        foreach (SC_DeploymentHero h in FindObjectsOfType<SC_DeploymentHero> ())
            Destroy (h.gameObject);

        yield return new WaitForSeconds (.5f);

        uiManager.loadingPanel.SetActive(false);

        SC_Cursor.SetLock (false);

    }
    #endregion

    #region Next Turn
    // Called by UI
    public void NextTurn () {

        Player.CmdNextTurn();

    }

    public void NextTurnFunction() {

        if (AdditionalTurn != null && AdditionalTurn == QinTurn)
            AdditionalTurn = null;
        else
            QinTurn ^= true;

        SC_Cursor.SetLock(true);

        if(SC_Character.activeCharacter) SC_Character.activeCharacter.Moving = false;

        SC_Character.activeCharacter = null;

        /*foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();*/

        CurrentConstru = "Bastion";        

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

            if (!character.Tile.Character) {

                print("Tile at : " + character.transform.position + " had no character");

                character.Tile.Character = character;

            }

            if (character.Hero) {

                character.Hero.MovementPoints = character.Hero.Movement;

                character.Hero.MovementCount = -1;

                character.Hero.ActionCount = -1;

                if (!QinTurn) {

                    character.Hero.Regen ();

                    if (character.Tile.Region == ScorchedRegion && !character.Qin)
                        character.Hit (SC_CastleTraps.Instance.scorchedDamage);

                    if (character.Hero.Isolated)
                        ((SC_Isolation) SC_Qin.Curse).ApplyDamage (character.Hero);

                } else {                  
                    
                    character.Hero.IncreaseRelationships (CommonCharactersVariables.relationGains.finishTurn);

                    if (character.Hero.HumansFateDuration > 0) {

                        character.Hero.HumansFateDuration--;

                        if (character.Hero.HumansFateDuration <= 0)
                            character.Hit (character.Health);

                    }

                }

            }            

            character.CanBeSelected = character.Stunned <= 0 ? character.Qin == QinTurn : false;

            if (character.Stunned <= 0)
                character.SetTired (false);
            else
                character.Stunned--;                               

        }

        if (QinTurn) {

            if (SC_DrainingStele.drainingSteles != null)
                foreach (SC_DrainingStele p in SC_DrainingStele.drainingSteles)
                    p.Drain ();

            foreach (SC_Demon d in SC_Demon.demons)
                if (d && d.Alive != -1)
                    d.TryRespawn ();

            SC_Qin.ChangeEnergy (SC_Qin.Qin.regenPerVillage * SC_Village.number);

            QinTurnStarting = true;

        }

        uiManager.NextTurn();

    }

    public void StartNextTurn() {

        // uiManager.SwapTurnIndicators (true);

        SC_Cursor.SetLock (false);

        if ((!CheckHeroTrapActivated ()) && Player.Qin && Player.Turn) {

            Player.Busy = true;

            tileManager.DisplayConstructableTiles(CurrentConstru);

        }

    }
    #endregion

    #region Methods called by UI  
    public void ActiveCharacterAttack (int weaponIndex) {

        Player.CmdAttack (SC_Character.activeCharacter.AttackTarget.gameObject, weaponIndex);

    }

    public void CancelLastConstruction () {

        if (QinTurnStarting)
            uiManager.backAction = DoNothing;
        else
            uiManager.cancelQinConstru.SetCanClick(false);

        Player.CmdCancelLastConstru();

    }

    /*public void UseHeroPower () {

        SC_Hero hero = GameObject.Find(GameObject.Find("PowerHero").GetComponentInChildren<Text>().name).GetComponent<SC_Hero>();
        hero.PowerUsed = true;

        GameObject.Find("PowerHero").SetActive(false);

    }*/

    public void DestroyProductionBuilding () {           

        SC_UI_Manager.Instance.TryDoAction(() => { Player.CmdDestroyProductionBuilding(); });

        uiManager.DisplayStaminaActionCost(false);

    }

    public void UnselectCharacter () {

        uiManager.HidePreviewFight();

        uiManager.HideInfosIfActive (SC_Character.activeCharacter.gameObject);

        SC_Arrow.DestroyArrow();

        tileManager.RemoveAllFilters();

        SC_Character.activeCharacter.Moving = false;

        SC_Hero.SetStaminaCost (new int[] { -1 });

        SC_Character.activeCharacter = null;

        SC_Cursor.Tile.OnCursorEnter();        

        uiManager.backAction = DoNothing;

    }

    public void ResetMovement () {

        uiManager.HidePreviewFight();

        SC_Player.localPlayer.CmdResetMovement();

    }

    public void ActivateCurse () {

        SC_Qin.Curse.Activate (true);

    }

    public void Concede () {

        Player.CmdShowVictory(!Player.Qin);

    }
    #endregion

    #region Construction
    public void SoldierConstruct(string c) {

        uiManager.constructPanel.SetActive(false);

        uiManager.backAction = DoNothing;

        tileManager.RemoveAllFilters();

        Player.CmdSetConstru(c);

        Player.CmdConstructAt(SC_Character.activeCharacter.transform.position.x.I(), SC_Character.activeCharacter.transform.position.y.I(), true);

    }

    public void ConstructAt (int x, int y, bool soldier) {

        SC_Tile tile = tileManager.GetTileAt(x, y);

        /*bool qinConstru = !tile.Soldier || QinTurnStarting;

        if (tile.Soldier) {

            if (!tile.Ruin) {

                if (!QinTurnStarting) {

                    uiManager.HideInfosIfActive(tile.Soldier.gameObject);

                    if (QinTurnStarting) {

                        SC_Construction.lastConstruSoldier = tile.Soldier;

                        tile.Soldier.gameObject.SetActive(false);

                        SC_Qin.ChangeEnergy(tile.Soldier.sacrificeValue);

                        tile.Character = null;

                    } else {

                    tile.Soldier.DestroyCharacter();

                     }

                }

            } else {

                SC_Character.FinishCharacterAction();

            }

        }*/

        if (soldier) {

            if (tile.Ruin)
                SC_Character.FinishCharacterAction();
            else
                tile.Soldier.DestroyCharacter();

        }

        tile.Ruin?.DestroyConstruction(false);

        SC_Sound_Manager.Instance.OnConstruct();

        TryFocusOn (tile.transform.position);

        if (isServer) {

            GameObject go = Resources.Load<GameObject>("Prefabs/Constructions/P_" + CurrentConstru);
            if(!go)
                go = Resources.Load<GameObject>("Prefabs/Constructions/Production/P_" + CurrentConstru);

            go = Instantiate(go, tile.transform.position, Quaternion.identity);

            NetworkServer.Spawn(go);

            if(!soldier)
                Player.CmdSetLastConstru(go);

            Player.CmdFinishConstruction(!soldier);

        }

    }

    public void FinishConstruction (bool qinConstru) {

        SC_Construction.lastConstru.Tile.Construction = SC_Construction.lastConstru;

        SC_Construction.lastConstru.Tile.RemoveDisplay ();

        if (!QinTurnStarting) {

            if (qinConstru) {

                SC_Qin.ChangeEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

                Player.CmdChangeQinEnergyOnClient(-SC_Qin.GetConstruCost(CurrentConstru), false);

                uiManager.UpdateCreationPanel(uiManager.qinConstrus);

                if (CanCreateConstruct (CurrentConstru))
                    tileManager.DisplayConstructableTiles (CurrentConstru);
                else
                    tileManager.RemoveAllFilters ();

                uiManager.backAction = uiManager.SelectConstruct;

                uiManager.cancelQinConstru.SetCanClick(true);

            } else if (SC_Cursor.Tile.Soldier) {

                Player.CmdChangeQinEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

                FinishAction();

            }

        }        

        if (QinTurnStarting) {

            FinishAction();

            uiManager.backAction = CancelLastConstruction;

        }

    }
    #endregion

    #region Players Actions  
    /*public void SpawnConvoy(Vector3 pos) {

		if (pos.x >= 0) {

			if (tileManager.GetTileAt(pos).IsEmpty()) {
			
				SpawnConvoy (pos + new Vector3 (-1, 0, 0));

			} else {

				GameObject go = Instantiate (convoyPrefab, GameObject.Find ("Convoys").transform);
				go.transform.position = pos;

			}

		}

	}*/           

    public void CreateSoldier(Vector3 pos, string soldierName) {

        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Soldiers/P_BaseSoldier"), pos, Quaternion.identity);

        go.GetComponent<SC_Soldier>().characterPath = "Prefabs/Characters/Soldiers/Basic/P_" + soldierName;

        NetworkServer.Spawn(go);

        Player.CmdSetupNewSoldier(go);

    }

    public void CreateDemon () {

        if (!SC_UI_Manager.clickSecurity && (SC_Qin.Energy > CurrentCastle.DemonCost)) {

            if (isServer)
                CreateDemonFunction();
            else
                Player.CmdCreateDemon(CurrentCastle.gameObject);

            Player.CmdTryFocus (CurrentCastle.transform.position);

            uiManager.EndQinAction();

        }

    }

    public void CreateDemonFunction () {

        Player.CmdChangeQinEnergy(-CurrentCastle.DemonCost);

        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Demons/P_BaseDemon"), CurrentCastle.transform.position, Quaternion.identity);

        go.GetComponent<SC_Character>().characterPath = "Prefabs/Characters/Demons/P_" + CurrentCastle.CastleType + "Demon";

        NetworkServer.Spawn(go);

    }

    public void SacrificeCastle () {

        Player.CmdSacrificeCastle(CurrentCastle.gameObject);

        uiManager.EndQinAction();

    }

    public void SacrificeCastle (SC_Castle castle) {
        
        TryFocusOn (castle.transform.position);

        CurrentCastle = castle;

        SC_Qin.ChangeEnergy(CurrentCastle.sacrificeValue);

        SC_Demon demon = SC_Demon.demons[CurrentCastle.Tile.Region];

        float percent = 1f + (GetCurrentCastleSacrificeValue() / 100);

        foreach (FieldInfo fI in demon.baseStats.GetType().GetFields())                     
            fI.SetValue(demon.baseStats, Mathf.CeilToInt(((int)fI.GetValue(demon.baseStats)) * percent));

        demon.Health = demon.MaxHealth;

        demon.UpdateHealth();

        demon.Linked = false;

        castle.DestroyConstruction(true);

        CheckHeroTrapActivated ();

    }
    #endregion

    public void ToggleFocus () {

        FocusOn ^= true;

    }

    public void TryFocusOn (Vector3 pos) {

        uiManager.SetMenuTransparencyAt (pos, true);

        if (FocusOn && !Player.Turn && (uiManager.IsFullScreenMenuOn || SC_Camera.Instance.ShouldFocus(pos))) {

            uiManager.FocusOn (pos);

            SC_Cursor.FocusOn (pos);

            SC_Camera.Instance.FocusOn (pos);

        }

    }

    // Called by UI
    public void Wait () {

        Player.CmdFinishAction();

    }

    public void FinishAction (bool wait = false) {

        tileManager.RemoveAllFilters();

        if (SC_Character.activeCharacter)
            SC_Character.FinishCharacterAction(wait);

        if (Player.Turn) {

            SC_Cursor.SetLock(false);

            uiManager.characterActionsPanel.SetActive(false);

            uiManager.backAction = DoNothing;

            Player.Busy = false;

        }

        CheckHeroTrapActivated ();

    }

    public bool CheckHeroTrapActivated () {

        if (SC_HeroTraps.actions.Count > 0) {

            SC_HeroTraps.actions[0].Invoke ();

            return true;

        } else
            return false;

    }

    public static float GetCurrentCastleSacrificeValue() {

        return Instance.CommonQinVariables.castleSacrifice.GetValue((int)(((float)Instance.CurrentCastle.Health / Instance.CurrentCastle.maxHealth) * 100 + .5f));

    }

}
