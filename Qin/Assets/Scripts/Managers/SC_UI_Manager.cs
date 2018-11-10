using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using static SC_Global;
using static SC_Player;

public class SC_UI_Manager : MonoBehaviour {

    #region UI Elements
    [Header("Preparation")]
    public GameObject connectingPanel;
    public GameObject preparationPanel;
    public GameObject qinPreparationPanel;
    public GameObject heroesPreparationPanel;
    public GameObject readyButton;
    public GameObject otherPlayerReady;
    public Color readyColor, notReadyColor;

    [Header("Game")]
    public GameObject gamePanel;
    public GameObject loadingPanel;
    public Text turnIndicator;
    public GameObject previewFightPanel;
    public GameObject endTurn;
    public GameObject victoryPanel;
    public GameObject playerActionsPanel;
    public GameObject toggleHealthBarsButton; 

    [Header("Characters")]
    public GameObject statsPanel;
    public GameObject characterActionsPanel;
    public GameObject attackButton;
    public GameObject destroyConstruButton;
    public GameObject buildConstruButton;
    public Button cancelButton;

    [Header("Heroes")]
    public GameObject relationshipPanel;
    public GameObject weaponChoicePanel;
    public GameObject weaponChoice1;
    public GameObject weaponChoice2;
    public GameObject usePower;

    [Header("Constructions")]
    public GameObject buildingInfosPanel;

    [Header("Qin")]
    public Text energyText;
    public GameObject qinPanel;
    public GameObject construct;
    public Transform constructPanel;
    public Transform soldierConstructPanel;
    public Transform qinPower;
    public GameObject sacrifice;
    public GameObject endSacrifice;
    public GameObject workshopPanel;

    [Header("Transforms")]
    public Transform tilesT;
    public Transform bordersT;
    public Transform soldiersT;
    public Transform heroesT;
    public Transform demonsT;
    public Transform wallsT;
    public Transform bastionsT;
    public Transform castlesT;
    public Transform workshopsT;
    public Transform ruinsT;
    public Transform villagesT;
    #endregion

    #region Variables
    public GameObject CurrentGameObject { get; set; }

    static SC_Game_Manager gameManager;

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

    public SC_Menu_Manager MenuManager { get; set; }

    public static SC_UI_Manager Instance { get; set; }

    public static bool CanInteract { get {

            return localPlayer.Turn && (!EventSystem.current.IsPointerOverGameObject() || !Cursor.visible) && !gameManager.prep;

        } }

    public float clickSecurityDuration;

    bool clickSecurity;

    public SC_Soldier[] basicSoldiers;

    SC_Construction[] qinConstructions;

    public SC_Construction[] SoldiersConstructions { get; set; }

    GameObject grid;

   public bool LifeBarsOn { get { return toggleHealthBarsButton.name == "On"; } }
    #endregion

    #region Setup
    private void Awake() {

        Instance = this;

    }

    public void SetupUI(bool qin) {       

        gameManager = SC_Game_Manager.Instance;

        fightManager = SC_Fight_Manager.Instance;

        MenuManager = SC_Menu_Manager.Instance;

        basicSoldiers = Resources.LoadAll<SC_Soldier>("Prefabs/Characters/Soldiers/Basic");

        for (int i = 0; i < workshopPanel.transform.GetChild(1).childCount; i++) {

            Transform soldier = workshopPanel.transform.GetChild(1).GetChild(i);

            if (i < basicSoldiers.Length) {

                soldier.GetChild(0).GetComponentInChildren<Text>().text = basicSoldiers[i].characterName;
                soldier.GetChild(1).GetComponentInChildren<Text>().text = basicSoldiers[i].cost.ToString();

            } else {

                Destroy(soldier.gameObject);

            }

        }

        qinConstructions = Resources.LoadAll<SC_Construction>("Prefabs/Constructions");

        SoldiersConstructions = Resources.LoadAll<SC_Construction>("Prefabs/Constructions/Production");

        SetupConstructPanel(true, constructPanel);

        SetupConstructPanel(false, soldierConstructPanel);

        if (gameManager.Qin) {

            construct.SetActive(true);
            sacrifice.SetActive(true);

        }

        if (gameManager.prep) {

            qinPreparationPanel.SetActive(qin);

            heroesPreparationPanel.SetActive(!qin);

            preparationPanel.SetActive(true);

        } else
            gamePanel.SetActive(true);

        // Setup Grid
        SpriteRenderer gridRenderer = Instantiate(Resources.Load<GameObject>("Prefabs/UI/P_Grid").GetComponent<SpriteRenderer>());
        Vector3 size = new Vector3(gameManager.CurrentMapPrefab.SizeMapX, gameManager.CurrentMapPrefab.SizeMapY, 1) * gameManager.CurrentMapPrefab.TileSize;
        gridRenderer.size = new Vector2(size.x, size.y);
        grid = gridRenderer.gameObject;
        grid.transform.position = (size - Vector3.one * gameManager.CurrentMapPrefab.TileSize) / 2f;        

    }

    void SetupConstructPanel(bool qin, Transform panel) {

        SC_Construction[] constructions = qin ? qinConstructions : SoldiersConstructions;

        for (int i = qin ? 1 : 0; i < panel.childCount; i++) {

            int index = qin ? i - 1 : i;

            Transform construction = panel.GetChild(i);

            if (index < constructions.Length) {

                construction.GetChild(0).GetComponentInChildren<Text>().text = constructions[index].Name;
                construction.GetChild(1).GetComponentInChildren<Text>().text = constructions[index].cost.ToString();

            } else {

                construction.gameObject.SetActive(false);

            }

        }

    }
    #endregion

    #region Preparation Phase
    public void SetReady () {

        bool canSetReady = true;

        if(localPlayer.Qin) {

            foreach (SC_Castle castle in FindObjectsOfType<SC_Castle>())
                if (castle.CastleType == null)             
                    canSetReady = false;

        }

        if (canSetReady) {

            localPlayer.Ready ^= true;

            SetReady(readyButton, localPlayer.Ready);

            localPlayer.CmdReady(localPlayer.Ready, localPlayer.Qin);

        }

    }

    public void SetReady (GameObject g, bool r) {

        g.GetComponent<Image>().color = r ? readyColor : notReadyColor;

        g.GetComponentInChildren<Text>().text = ((g == readyButton) ? "" : "Other Player ") + (r ? "Ready" : "Not Ready");

    }

    GameObject draggedCastle;

    public void StartDragCastle(string castleType) {

        GameObject go = Resources.Load<GameObject>("Prefabs/UI/P_Drag&DropCastle");

        go.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/Castles/" + castleType);

        draggedCastle = Instantiate(go, new Vector3(WorldMousePos.x, WorldMousePos.y, -.54f) , Quaternion.identity);

        draggedCastle.name = castleType;

    }

    public void DropCastle() {

        TileManager.GetTileAt(WorldMousePos)?.Castle?.SetCastle(draggedCastle.name);

        Destroy(draggedCastle);

    }

    public void Load() {

        loadingPanel.SetActive(true);

        preparationPanel.SetActive(false);

        gamePanel.SetActive(true);

    }
    #endregion

    #region Next Turn 
    public void NextTurn() {

        playerActionsPanel.SetActive(false);

        //usePower.SetActive (!gameManager.Qin && !SC_Player.localPlayer.Qin);

        cancelButton.gameObject.SetActive(false);

        turnIndicator.text = gameManager.Qin ? "Qin's Turn" : (gameManager.Turn % 3 == 1 ? "1st" : "2nd") + " Coalition's Turn";

	}
    #endregion

    #region Buttons
    public void SetCancelButton (Action a) {

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(delegate { a(); });
        cancelButton.gameObject.SetActive(true);

    }
    #endregion

    #region Infos
    public void ShowInfos(GameObject g, Type t) {

        CurrentGameObject = g;

        if (t == typeof(SC_Hero))
            ShowHeroInfos(g.GetComponent<SC_Hero>());
        else if (t.IsSubclassOf(typeof(SC_BaseQinChara)))
            ShowBaseQinCharaInfos(g.GetComponent<SC_BaseQinChara>());
        else if (t.IsSubclassOf(typeof(SC_Construction)))
            ShowConstructionsInfos(g.GetComponent<SC_Construction>());
        else if (t == typeof(SC_Qin))
            ShowQinInfos();
        else if (t == typeof(SC_Ruin)) {

            buildingInfosPanel.SetActive(true);

            SetText("BuildingName", "Ruin");
            SetText("BuildingHealth", "");

        } else
            print("ERRROR");

	}

    public void HideInfosIfActive(GameObject g) {

        if (CurrentGameObject == g)
            HideInfos(true);

    }

	public void HideInfos(bool removeFilters) {

        if (removeFilters)
            TileManager.RemoveAllFilters(true);

		statsPanel.SetActive (false);
		relationshipPanel.SetActive (false);
		buildingInfosPanel.SetActive (false);
		qinPanel.SetActive (false);

        CurrentGameObject = null;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        if(CurrentGameObject == g) {

            CurrentGameObject = null;

            ShowInfos(g, t);

        }

    }

	void ShowCharacterInfos(SC_Character character) {

		statsPanel.SetActive (true);

		SetText("Name", character.characterName);
		SetText("Health", "Health : " + character.Health + " / " + character.maxHealth);
		SetText("Strength", " Strength : " + GetStat(character, "Strength"));
		SetText("Armor", " Armor : " + GetStat(character, "Armor"));
		SetText("Chi", " Chi : " + GetStat(character, "Chi"));
		SetText("Resistance", " Resistance : " + GetStat(character, "Resistance"));
		SetText("Technique", " Technique : " + GetStat(character, "Technique") + ", Crit : " + character.CriticalAmount + "/" + gameManager.CommonCharactersVariables.critTrigger);
		SetText("Reflexes", " Reflexes : " + GetStat(character, "Reflexes") + ", Dodge : " + character.DodgeAmount + "/" + gameManager.CommonCharactersVariables.dodgeTrigger);
        SetText("Movement", " Movement : " + GetStat(character, "Movement"));
		SetText("WeaponsTitle", " Weapons :");

        if (SC_Tile.CanChangeFilters)
            TileManager.DisplayMovementAndAttack(character, true);

    }

    string GetStat(SC_Character chara, string id) {        

        int stat = (int)chara.GetType().GetProperty(id).GetValue(chara);

        int baseStat = (int)chara.GetType().GetField("base" + id).GetValue(chara);

        int modifiers = stat - baseStat;

        return stat + (modifiers == 0 ? "" : (" (" + baseStat + " " + (modifiers > 0 ? "+" : "-") + " " + Mathf.Abs(modifiers) + ")"));

    }

	void ShowHeroInfos(SC_Hero hero) {

		ShowCharacterInfos (hero);

		relationshipPanel.SetActive (true);

		SetText("Weapon 1", "  - " + hero.GetWeapon(true).weaponName + " (E)");
		SetText("Weapon 2", "  - " + hero.GetWeapon(false).weaponName);

		for (int i = 0; i < hero.RelationshipKeys.Count; i++) {

			int value;
			hero.Relationships.TryGetValue(hero.RelationshipKeys [i], out value);
			GameObject.Find ("Relation_" + (i + 1)).GetComponent<Text> ().text = "  " + hero.RelationshipKeys [i] + " : " + value;

		}


	}

	void ShowBaseQinCharaInfos (SC_BaseQinChara baseQinChara) {

		ShowCharacterInfos (baseQinChara);

		SetText("Weapon 1", "  - " + baseQinChara.weapon.weaponName);
		SetText("Weapon 2", "");

	}     

	void ShowConstructionsInfos(SC_Construction construction) {

		buildingInfosPanel.SetActive (true);

		SetText("BuildingName", construction.Name);
		SetText("BuildingHealth", construction.Health != 0 ? "Health : " + construction.Health + " / " + construction.maxHealth : "");

        if (SC_Tile.CanChangeFilters && construction.Pump) {

            TileManager.DisplayedPump = construction.Pump;

            foreach (SC_Tile tile in TileManager.GetRange(construction.transform.position, construction.Pump.range))
                tile.SetFilter(TDisplay.Attack);

        }

	}

	void ShowQinInfos() {

		qinPanel.SetActive (true);

		SetText("QinEnergy", SC_Qin.Energy + "");

	}
    #endregion

    #region Fight related
    // Also called by UI
    public void PreviewFight (bool activeWeapon) {

        SC_Character attacker = SC_Character.attackingCharacter;

        attacker.Hero?.SetWeapon(activeWeapon);

        previewFightPanel.SetActive(true);

        SetText("AttackerName", attacker.characterName);

        SetText("AttackerWeapon", attacker.GetActiveWeapon().weaponName);

        /*SetText ("AttackerCrit", attacker.CriticalAmount.ToString ());

		SetText ("AttackerDodge", attacker.DodgeAmount.ToString ());*/

        int attackedDamages = 0;

        int attackerDamages = attacker.BaseDamage;

        string attackedName = "";

        int attackedHP = 0;

        string attackedWeapon = "";

        /*string attackedCrit = "";

		string attackedDodge = "";*/

        if (attacker.AttackTarget.Character && !attacker.AttackTarget.GreatWall) {

            SC_Character attacked = attacker.AttackTarget.Character;

            attackedName = attacked.characterName;

            attackedWeapon = attacked.GetActiveWeapon().weaponName;

            attackerDamages = fightManager.CalcDamages(attacker, attacked, false);

            if (!attacker.Tile.GreatWall && attacked.GetActiveWeapon().Range(attacked).In(fightManager.AttackRange))
                attackedDamages = fightManager.CalcDamages(attacked, attacker, true);

            attackedHP = attacked.Health - attackerDamages;

            /*attackedCrit = attacked.CriticalHit.ToString ();

			attackedDodge = attacked.DodgeHit.ToString ();*/

        } else {

            int attackedType = attacker.AttackTarget.Construction ? 0 : attacker.AttackTarget.Qin ? 1 : 2;

            attackedName = (attackedType == 0) ? attacker.AttackTarget.Construction.Name : (attackedType == 1) ? "Qin" : "";

            int attackedHealth = (attackedType == 0) ? attacker.AttackTarget.Construction.Health : (attackedType == 1) ? SC_Qin.Energy : 0;

            if (attackedType != 2)
                attackedHP = attackedHealth - attackerDamages;

        }

        SetText("AttackerHP", (Mathf.Max(attacker.Health - attackedDamages, 0)).ToString());

        SetText("AttackedName", attackedName);

        SetText("AttackedHP", Mathf.Max(attackedHP, 0).ToString());

        SetText("AttackerDamages", attackerDamages.ToString());
        SetText("AttackedDamages", attackedDamages.ToString());

        SetText("AttackedWeapon", attackedWeapon);

        /*SetText("AttackedCrit", attackedCrit);
		SetText("AttackedDodge", attackedDodge);*/

        attacker.Hero?.SetWeapon(activeWeapon);

    }

    // Also called by UI
    public void HidePreviewFight () {

        previewFightPanel.SetActive(false);

    }  
    #endregion

    #region Heroes
    /*public void ShowHeroPower(bool show, string heroName) {

		usePower.SetActive (!show);

		if (show)
			usePower.GetComponentInChildren<Text> ().name = heroName;

	}*/

    #region Weapons
    public void ShowWeapon (SC_Weapon weapon, bool first) {

        if (first)
            weaponChoice1.SetActive(true);
        else
            weaponChoice2.SetActive(true);

        SetText("Weapon Choice " + (first ? "1" : "2") + " Text", weapon.weaponName);

    }

    public void ResetAttackChoice () {

        HideWeapons();

        Attack();

    }

    public void HideWeapons () {

        weaponChoicePanel.SetActive(false);
        weaponChoice1.SetActive(false);
        weaponChoice2.SetActive(false);
        previewFightPanel.SetActive(false);

    }
    #endregion
    #endregion

    #region Qin
    #region Actions
    public void StartQinAction(string action) {

        if (!localPlayer.Busy) {

            localPlayer.Busy = true;

            constructPanel.gameObject.SetActive(action == "construct");

            endSacrifice.SetActive(action == "sacrifice");

            workshopPanel.SetActive(action == "workshop");

            localPlayer.CmdSetQinTurnStarting(false);

            cancelButton.gameObject.SetActive(false);

            TileManager.RemoveAllFilters();

            playerActionsPanel.SetActive(false);

        }

    }

    public void EndQinAction(string action) {

        if (action != "workshop")
            SC_Tile_Manager.Instance.RemoveAllFilters();

        if (action == "construct")
            cancelButton.gameObject.SetActive(false);

        constructPanel.gameObject.SetActive(false);

        endSacrifice.SetActive(false);

        workshopPanel.SetActive(false);

        localPlayer.Busy = false;

    }

    // Called by UI
    public void DisplaySacrifices () {

        if (!localPlayer.Busy) {

            StartQinAction("sacrifice");

            TileManager.DisplaySacrifices();

        }

    }

    // Called by UI
    /*public void DisplayResurrection () {

        if (!SC_Player.localPlayer.Busy && gameManager.LastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            StartQinAction("qinPower");

            TileManager.DisplayResurrection();

        }

    }*/
    #endregion

    #region Building
    public void UpdateQinConstructPanel () {

        for (int i = 1; i < constructPanel.childCount; i++)
            constructPanel.GetChild(i).GetComponentInChildren<Button>().interactable = (SC_Qin.GetConstruCost(qinConstructions[i].Name) < SC_Qin.Energy) && (TileManager.GetConstructableTiles(qinConstructions[i].Name == "Wall").Count > 0);

    }

    // Called by UI
    public void DisplayQinConstructPanel() {        

        UpdateQinConstructPanel();

        StartQinAction("construct");                  

    }

    // Called by UI
    public void DisplaySoldiersConstructPanel () {

        characterActionsPanel.SetActive(false);

        for (int i = 0; i < soldierConstructPanel.childCount; i++)
            soldierConstructPanel.GetChild(i).GetComponentInChildren<Button>().interactable = (SC_Qin.GetConstruCost(SoldiersConstructions[i].Name) < SC_Qin.Energy) && (TileManager.GetConstructableTiles(SoldiersConstructions[i].Name == "Wall").Count > 0);

        TileManager.RemoveAllFilters();

        soldierConstructPanel.gameObject.SetActive(true);

        SetCancelButton(CancelAction);

    }

    // Called by UI
    public void DisplayConstructableTiles(int id) {

        localPlayer.CmdSetConstru(qinConstructions[id].Name);

        TileManager.RemoveAllFilters();

        TileManager.DisplayConstructableTiles(qinConstructions[id].Name == "Wall");

    }
    #endregion

    #region Workshop
    public void DisplayWorkshopPanel() {

        Transform uiSoldiers = workshopPanel.transform.GetChild(1);

        for (int i = 0; i < uiSoldiers.childCount; i++)
            uiSoldiers.GetChild(i).GetComponentInChildren<Button>().interactable = basicSoldiers[i].cost < SC_Qin.Energy;

        StartCoroutine(ClickSafety());

        StartQinAction("workshop");     

    }

    IEnumerator ClickSafety() {

        clickSecurity = true;

        yield return new WaitForSeconds(clickSecurityDuration);

        clickSecurity = false;


    }

    public void WorkshopCreateSoldier (int id) {

        if (!clickSecurity) {            

            localPlayer.CmdCreateSoldier(gameManager.CurrentWorkshopPos, basicSoldiers[id].characterName);

            EndQinAction("workshop");

        }

    }
    #endregion

    #endregion

    #region Both Players  
    void Update () {

        if(Input.GetButtonDown("ToggleGrid"))
            grid.SetActive(!grid.activeSelf);

        if(draggedCastle)
            draggedCastle.transform.SetPos(WorldMousePos);

        if (cancelButton.isActiveAndEnabled && Input.GetButtonDown("Cancel"))
            cancelButton.onClick.Invoke();

    }

    // Called by UI
    public void ToggleHealth () {

        toggleHealthBarsButton.name = toggleHealthBarsButton.name == "On" ? "Off" : "On";

        foreach (SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Toggle();

    }

    public void ShowVictory (bool qinWon) {

        victoryPanel.GetComponentInChildren<Text>().text = (qinWon ? "Qin" : "The Heroes") + " won the war !";

        victoryPanel.SetActive(true);

    }

    public void Attack() {

        SC_Cursor.Instance.Locked = false;

        characterActionsPanel.SetActive(false);

        SetCancelButton(CancelAction);

        TileManager.CheckAttack();

    }

    void CancelAction() {

        SC_Cursor.Instance.Locked = true;

        soldierConstructPanel.gameObject.SetActive(false);

        TileManager.RemoveAllFilters();

        characterActionsPanel.SetActive(true);

        SetCancelButton(gameManager.ResetMovement);

        TileManager.PreviewAttack();

        previewFightPanel.SetActive(false);

    }

    public void Wait() {

        SC_Cursor.Instance.Locked = false;

        localPlayer.CmdWait();

        characterActionsPanel.SetActive(false);

        cancelButton.gameObject.SetActive(false);

        localPlayer.Busy = false;

    }
    #endregion

    #region Utility functions
    void SetText (string id, string text) {

        GameObject.Find(id).GetComponent<Text>().text = text;

    }
    #endregion
}
