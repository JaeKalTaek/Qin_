using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using static SC_EditorTile;
using static SC_Global;
using System.Reflection;

public class SC_Game_Manager : NetworkBehaviour {

    public SC_MapEditorScript playMapPrefab;

    public SC_MapEditorScript prepMapPrefab;

    public bool prep;

    public SC_MapEditorScript CurrentMapPrefab { get; set; }

    public SC_Common_Characters_Variables CommonCharactersVariables { get; set; }

    public SC_CommonQinVariables CommonQinVariables { get; set; }

    public static SC_Game_Manager Instance { get; set; }  

    public int Turn { get; set; }

    public bool Qin { get { return Turn % 3 == 0; } }

    public bool QinTurnStarting { get; set; }

	public SC_Hero LastHeroDead { get; set; }

    public string CurrentConstru { get; set; }

	public Vector3 CurrentWorkshopPos { get; set; }

    public SC_Castle CurrentCastle { get; set; }

	public SC_Player Player { get; set; }

	SC_UI_Manager uiManager;

	SC_Tile_Manager tileManager;

    public static float TileSize;

    public bool ServerStarted { get; set; }

    #region Setup
    private void Awake () {

        Instance = this;

        uiManager = FindObjectOfType<SC_UI_Manager>();
        uiManager.connectingPanel.SetActive(true);        

        SC_Village.number = 0;

        SC_Castle.castles = new bool[6];

        SC_Demon.demons = new SC_Demon[6];

        SC_Tile.filters = Resources.LoadAll<Sprite>("Sprites/Tiles/Filters");

        CurrentMapPrefab = prep ? prepMapPrefab : playMapPrefab;

    }

    void Start() {

        TileSize = CurrentMapPrefab.TileSize;

        CommonCharactersVariables = Resources.Load<SC_Common_Characters_Variables>("Prefabs/Characters/P_Common_Characters_Variables");

        CommonQinVariables = Resources.Load<SC_CommonQinVariables>("Prefabs/P_CommonQinVariables");

        Turn = 1;

        StartCoroutine("WaitForPlayer");

        StartCoroutine("WaitForServer");		

    }

    IEnumerator WaitForPlayer() {

        while (!Player)
            yield return new WaitForEndOfFrame();

        uiManager.SetupUI(Player.Qin);

    }

    IEnumerator WaitForServer() {

        while (!ServerStarted)
            yield return new WaitForEndOfFrame();

        if (isServer) {

            GenerateMap();
            SetupTileManager();

        }

    }

	void GenerateMap() {

        foreach (Transform child in CurrentMapPrefab.transform) {

            SC_EditorTile eTile = child.GetComponent<SC_EditorTile>();

            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/P_BaseTile"), child.position, Quaternion.identity, GameObject.Find("Tiles").transform);

            bool[] borders = new bool[4];

            for (int i = 0; i < eTile.transform.GetChild(2).childCount; i++)
                borders[i] = eTile.transform.GetChild(2).GetChild(i).GetComponent<SpriteRenderer>().sprite;

            go.GetComponent<SC_Tile>().infos = new TileInfos(
                eTile.tileType.ToString(),
                Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + eTile.tileType).Length),
                (int)eTile.riverSprite,
                (int)eTile.region,
                borders
            );

            NetworkServer.Spawn(go);

        }

	}

	void SetupTileManager() {

		GameObject tm = Instantiate (Resources.Load<GameObject>("Prefabs/P_Tile_Manager"));
		SC_Tile_Manager stm = tm.GetComponent<SC_Tile_Manager> ();

        stm.xSize = CurrentMapPrefab.SizeMapX;
        stm.ySize = CurrentMapPrefab.SizeMapY;	

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

		foreach (Transform child in CurrentMapPrefab.transform) {

			SC_EditorTile eTile = child.GetComponent<SC_EditorTile> ();

            if (eTile.construction != ConstructionType.None) {

                GameObject constructionPrefab = TryLoadConstruction(eTile.construction.ToString());

                GameObject go = Instantiate(constructionPrefab, eTile.transform.position, Quaternion.identity);

                if ((eTile.construction == ConstructionType.Castle) && !prep)
                    go.GetComponent<SC_Castle>().CastleType = eTile.castleType.ToString();

                NetworkServer.Spawn (go);                

			}

            if ((eTile.soldier != SoldierType.None) || eTile.Qin || (eTile.Hero != HeroType.None)) {

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

    public void Load() {

        foreach (SC_Castle castle in FindObjectsOfType<SC_Castle>())
            if (!Player.Qin)
                castle.Setup();

        foreach (SC_Tile t in tileManager.ChangingTiles)
            t.SetupTile();

        uiManager.loadingPanel.SetActive(false);

        prep = false;

    }
    #endregion

    #region Next Turn
    // Called by UI
    public void NextTurn () {

        Player.CmdNextTurn();

    }

    public void NextTurnFunction() {    

	    Turn++;

        SC_Cursor.SetLock(false);

        // tileManager.RemoveAllFilters();        

        SC_Character.attackingCharacter = null;

        /*foreach (SC_Convoy convoy in FindObjectsOfType<SC_Convoy>())
			convoy.MoveConvoy ();*/

        CurrentConstru = "Bastion";

        if (Qin) {

            if(SC_Pump.pumps != null)
                foreach (SC_Pump p in SC_Pump.pumps)
                 p.Drain();

            foreach(SC_Demon d in SC_Demon.demons)
                if(d && d.Alive != -1)
                    d.TryRespawn();

            SC_Qin.ChangeEnergy(SC_Qin.Qin.regenPerVillage * SC_Village.number);

            QinTurnStarting = true;

            if (Player.Qin) {

                Player.Busy = true;                

                tileManager.DisplayConstructableTiles(CurrentConstru);

            }

		}

        foreach (SC_Character character in FindObjectsOfType<SC_Character>()) {

            character.Tile.Character = character;

            if (character.Hero) {

                character.Hero.Regen();

                character.Hero.IncreaseRelationships(CommonCharactersVariables.relationGains.finishTurn);

                /*if (Qin) {

                    if (character.Hero.PowerUsed)
                        character.Hero.PowerBacklash++;

                    if (character.Hero.PowerBacklash >= 2)
                        character.Hero.DestroyCharacter();

                } else {

                    character.Hero.BerserkTurn = false;

                }*/

            }

            character.UnTired();

            character.CanMove = character.Qin == Qin;

        }

        uiManager.NextTurn ();
        
    }
    #endregion

    #region Methods called by UI  
    public void SetAttackWeapon (bool usedActiveWeapon) {

        Player.CmdHeroAttack(usedActiveWeapon);

    }

    public void CancelLastConstruction () {

        uiManager.cancelAction = DoNothing;

        Player.CmdCancelLastConstru();

    }

    /*public void UseHeroPower () {

        SC_Hero hero = GameObject.Find(GameObject.Find("PowerHero").GetComponentInChildren<Text>().name).GetComponent<SC_Hero>();
        hero.PowerUsed = true;

        GameObject.Find("PowerHero").SetActive(false);

        print("Implement Power");

    }*/

    public void DestroyProductionBuilding () {

        Player.CmdDestroyProductionBuilding();

    }

    public void UnselectCharacter () {        

        tileManager.RemoveAllFilters();

        SC_Tile t = SC_Character.characterToMove.Tile;        

        SC_Character.characterToMove = null;

        t.OnCursorEnter();

        uiManager.cancelAction = DoNothing;

    }

    public void ResetMovement () {

        uiManager.HidePreviewFight();

        SC_Player.localPlayer.CmdResetMovement();

    }

    public void Concede () {

        Player.CmdShowVictory(!Player.Qin);

    }
    #endregion

    #region Construction
    public void SoldierConstruct(string c) {

        uiManager.constructPanel.SetActive(false);

        uiManager.cancelAction = DoNothing;

        tileManager.RemoveAllFilters();

        Player.CmdSetConstru(c);

        Player.CmdConstructAt(SC_Character.attackingCharacter.transform.position.x.I(), SC_Character.attackingCharacter.transform.position.y.I());

        Player.Busy = false;

        SC_Cursor.SetLock(false);

    }

    public void ConstructAt (int x, int y) {

        //tileManager.RemoveAllFilters();

        SC_Tile tile = tileManager.GetTileAt(x, y);

        bool qinConstru = !tile.Soldier || QinTurnStarting;

        if (tile.Soldier) {

            if (!tile.Ruin) {

                uiManager.HideInfosIfActive(tile.Soldier.gameObject);

                if (QinTurnStarting) {

                    SC_Construction.lastConstruSoldier = tile.Soldier;

                    tile.Soldier.gameObject.SetActive(false);

                    SC_Qin.ChangeEnergy(tile.Soldier.sacrificeValue);

                    tile.Character = null;

                } else {

                    tile.Soldier.DestroyCharacter();

                }

            } else {

                FinishAction(true);

            }

        }

        tile.Ruin?.DestroyConstruction();

        if (isServer) {

            GameObject go = Resources.Load<GameObject>("Prefabs/Constructions/P_" + CurrentConstru);
            if(!go)
                go = Resources.Load<GameObject>("Prefabs/Constructions/Production/P_" + CurrentConstru);

            go = Instantiate(go, tile.transform.position, Quaternion.identity);

            NetworkServer.Spawn(go);

            if(qinConstru)
                Player.CmdSetLastConstru(go);

            Player.CmdFinishConstruction(qinConstru);

        }

    }

    public void FinishConstruction (bool qinConstru) {

        tileManager.RemoveAllFilters();

        if (QinTurnStarting) {

            Player.Busy = false;

        } else {

            if (qinConstru) {

                SC_Qin.ChangeEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

                Player.CmdChangeQinEnergyOnClient(-SC_Qin.GetConstruCost(CurrentConstru), false);

                uiManager.UpdateCreationPanel(uiManager.qinConstrus);

                if (CanCreateConstruct(CurrentConstru))
                    tileManager.DisplayConstructableTiles(CurrentConstru);

            } else {

                Player.CmdChangeQinEnergy(-SC_Qin.GetConstruCost(CurrentConstru));

            }

        }

        if (qinConstru)
            uiManager.cancelAction = CancelLastConstruction;

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

        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Soldiers/P_BaseSoldier"));

        go.GetComponent<SC_Soldier>().characterPath = "Prefabs/Characters/Soldiers/Basic/P_" + soldierName;

        go.transform.SetPos(pos);

        NetworkServer.Spawn(go);

        Player.CmdSetupNewSoldier(go);

    }

    public void CreateDemon () {

        if (!SC_UI_Manager.clickSecurity && (SC_Qin.Energy > CurrentCastle.DemonCost)) {

            if (isServer)
                CreateDemonFunction();
            else
                Player.CmdCreateDemon(CurrentCastle.gameObject);

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

        SC_Qin.ChangeEnergy(CurrentCastle.sacrificeValue);

        SC_Demon demon = SC_Demon.demons[CurrentCastle.Tile.Region];

        foreach (FieldInfo fI in demon.baseStats.GetType().GetFields()) {

            float percent = 1f + (GetCurrentCastleSacrificeValue() / 100);

            fI.SetValue(demon.baseStats, Mathf.CeilToInt(((int)fI.GetValue(demon.baseStats)) * percent));

        }

        demon.Health = demon.MaxHealth;

        demon.UpdateHealth();

        demon.Linked = false;

        castle.DestroyConstruction();

    }
    #endregion

    public void FinishAction() {

        if (SC_Cursor.Instance.Locked)
            FinishAction(false);
        else
            SC_Character.FinishCharacterAction();

    }

    public void FinishAction (bool sync) {

        SC_Cursor.SetLock(false);

        if (sync)
            Player.CmdFinishCharacterAction();
        else
            SC_Character.FinishCharacterAction();

        uiManager.characterActionsPanel.SetActive(false);

        uiManager.cancelAction = DoNothing;

        Player.Busy = false;

    }

    public static float GetCurrentCastleSacrificeValue() {

        return Instance.CommonQinVariables.castleSacrifice.GetValue(Mathf.CeilToInt(((float)Instance.CurrentCastle.Health / Instance.CurrentCastle.maxHealth) * 100));

    }

}
