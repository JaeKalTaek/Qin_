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
    public GameObject endTurn;
    public GameObject victoryPanel;
    public GameObject playerActionsPanel;
    public GameObject toggleHealthBarsButton;
    public Button cancelButton;
    public TileTooltip tileTooltip;

    [Header("Preview Fight")]
    public GameObject previewFightPanel;
    public CharacterFightPreview attackerPreviewFight, attackedPreviewFight;

    [Header("Characters")]
    public CharacterTooltip characterTooltip;
    public GameObject statsPanel;
    public GameObject characterActionsPanel;
    public GameObject attackButton;
    public GameObject destroyConstruButton;
    public GameObject buildConstruButton;    

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
    public GameObject CurrentChara { get; set; }

    public GameObject CurrentTile { get; set; }

    static SC_Game_Manager gameManager;

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

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

        if (qin) {

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

        if (t == typeof(SC_Hero))
            ShowHeroInfos(g.GetComponent<SC_Hero>());
        else if (t.IsSubclassOf(typeof(SC_BaseQinChara)))
            ShowBaseQinCharaInfos(g.GetComponent<SC_BaseQinChara>());
        else if (t == typeof(SC_Qin))
            ShowQinInfos();
        else if (t == typeof(SC_Tile))
            ShowTileTooltip(g.GetComponent<SC_Tile>());
        else
            print("ERRROR");

    }

    public void HideInfosIfActive(GameObject g) {

        if (CurrentChara == g)
            HideInfos(true);

    }

	public void HideInfos(bool removeFilters) {

        if (removeFilters)
            TileManager.RemoveAllFilters(true);

        /*statsPanel.SetActive (false);
		relationshipPanel.SetActive (false);*/

        characterTooltip.panel.SetActive(false);

        tileTooltip.panel.SetActive(false);

        buildingInfosPanel.SetActive (false);
		qinPanel.SetActive (false);

        CurrentChara = null;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        if((CurrentChara == g) || (CurrentTile == g))
            ShowInfos(g, t);

    }

	void ShowCharacterInfos(SC_Character character) {

        /*statsPanel.SetActive (true);

		SetText("Name", character.characterName);
		SetText("Health", "Health : " + character.Health + " / " + character.maxHealth);
		SetText("Strength", " Strength : " + GetStat(character, "Strength"));
		SetText("Armor", " Armor : " + GetStat(character, "Armor"));
		SetText("Chi", " Chi : " + GetStat(character, "Chi"));
		SetText("Resistance", " Resistance : " + GetStat(character, "Resistance"));
		SetText("Technique", " Technique : " + GetStat(character, "Technique") + ", Crit : " + character.CriticalAmount + "/" + gameManager.CommonCharactersVariables.critTrigger);
		SetText("Reflexes", " Reflexes : " + GetStat(character, "Reflexes") + ", Dodge : " + character.DodgeAmount + "/" + gameManager.CommonCharactersVariables.dodgeTrigger);
        SetText("Movement", " Movement : " + GetStat(character, "Movement"));
		SetText("WeaponsTitle", " Weapons :");*/

        CurrentChara = character.gameObject;

        characterTooltip.icon.sprite = character.GetComponent<SpriteRenderer>().sprite;

        characterTooltip.name.text = character.characterName;

        characterTooltip.healthLabel.text = "Health";

        characterTooltip.health.Set(character.Health, character.maxHealth);

        characterTooltip.crit.Set(character.CriticalAmount, gameManager.CommonCharactersVariables.critTrigger);

        characterTooltip.dodge.Set(character.DodgeAmount, gameManager.CommonCharactersVariables.dodgeTrigger);

        characterTooltip.critContainer.SetActive(true);

        characterTooltip.dodgeContainer.SetActive(true);

        characterTooltip.panel.SetActive(true);

        if (SC_Tile.CanChangeFilters)
            TileManager.DisplayMovementAndAttack(character, true);

    }

    /*string GetStat(SC_Character chara, string id) {        

        int stat = (int)chara.GetType().GetProperty(id).GetValue(chara);

        int baseStat = (int)chara.GetType().GetField("base" + id).GetValue(chara);

        int modifiers = stat - baseStat;

        return stat + (modifiers == 0 ? "" : (" (" + baseStat + " " + (modifiers > 0 ? "+" : "-") + " " + Mathf.Abs(modifiers) + ")"));

    }*/

	void ShowHeroInfos(SC_Hero hero) {

		ShowCharacterInfos (hero);

		/*relationshipPanel.SetActive (true);

		SetText("Weapon 1", "  - " + hero.GetWeapon(true).weaponName + " (E)");
		SetText("Weapon 2", "  - " + hero.GetWeapon(false).weaponName);

		for (int i = 0; i < hero.RelationshipKeys.Count; i++) {

			int value;
			hero.Relationships.TryGetValue(hero.RelationshipKeys [i], out value);
			GameObject.Find ("Relation_" + (i + 1)).GetComponent<Text> ().text = "  " + hero.RelationshipKeys [i] + " : " + value;

		}*/


	}

	void ShowBaseQinCharaInfos (SC_BaseQinChara baseQinChara) {

		ShowCharacterInfos (baseQinChara);

		/*SetText("Weapon 1", "  - " + baseQinChara.weapon.weaponName);
		SetText("Weapon 2", "");*/

	}     

    void ShowTileTooltip(SC_Tile t) {

        CurrentTile = t.gameObject;

        if (t.Construction)
            ShowConstructionsInfos(t.Construction);
        else {

            tileTooltip.name.text = t.infos.type;
            tileTooltip.health.gameObject.SetActive(false);

        }

        bool movingDemon = SC_Character.characterToMove?.Demon;

        tileTooltip.power.text = t.CombatModifiers.strength + (movingDemon ? 0 : t.DemonsModifier("strength", localPlayer.Qin)) + "";
        tileTooltip.defense.text = t.CombatModifiers.armor + (movingDemon ? 0 : t.DemonsModifier("armor", localPlayer.Qin)) + "";
        tileTooltip.technique.text = t.CombatModifiers.technique + (movingDemon ? 0 : t.DemonsModifier("technique", localPlayer.Qin)) + "";
        tileTooltip.reflexes.text = t.CombatModifiers.reflexes + (movingDemon ? 0 : t.DemonsModifier("reflexes", localPlayer.Qin)) + "";
        tileTooltip.range.text = t.CombatModifiers.range + (movingDemon ? 0 : t.DemonsModifier("range", localPlayer.Qin)) + "";
        tileTooltip.movement.text = t.CombatModifiers.movement + (movingDemon ? 0 : t.DemonsModifier("movement", localPlayer.Qin)) + "";

        tileTooltip.panel.SetActive(true);

    }

	void ShowConstructionsInfos(SC_Construction construction) {

        tileTooltip.name.text = construction.Name;

        if (construction.Health > 0) {

            tileTooltip.health.Set(construction.Health, construction.maxHealth);
            tileTooltip.health.gameObject.SetActive(true);

        }

		/*buildingInfosPanel.SetActive (true);

		SetText("BuildingName", construction.Name);
		SetText("BuildingHealth", construction.Health != 0 ? "Health : " + construction.Health + " / " + construction.maxHealth : "");*/

        if (SC_Tile.CanChangeFilters && construction.Pump) {

            TileManager.DisplayedPump = construction.Pump;

            foreach (SC_Tile tile in TileManager.GetRange(construction.transform.position, construction.Pump.range))
                tile.SetFilter(TDisplay.Attack);

        }

	}

	void ShowQinInfos() {

        /*qinPanel.SetActive (true);

		SetText("QinEnergy", SC_Qin.Energy + "");*/

        CurrentChara = SC_Qin.Qin.gameObject;

        characterTooltip.icon.sprite = SC_Qin.Qin.GetComponent<SpriteRenderer>().sprite;

        characterTooltip.name.text = "Qin";

        characterTooltip.healthLabel.text = "Energy";

        characterTooltip.health.Set(SC_Qin.Energy, SC_Qin.Qin.energyToWin);

        characterTooltip.critContainer.SetActive(false);

        characterTooltip.dodgeContainer.SetActive(false);

        characterTooltip.panel.SetActive(true);

    }
    #endregion

    #region Fight related
    // Also called by UI
    public void PreviewFight (bool activeWeapon) {

        SC_Character attacker = SC_Character.attackingCharacter;

        attacker.Hero?.SetWeapon(activeWeapon);        

        attackerPreviewFight.name.text = attacker.characterName;

        attackerPreviewFight.constructionHealth.gameObject.SetActive(false);
        attackedPreviewFight.constructionHealth.gameObject.SetActive(false);

        SC_Character a = attacker.AttackTarget.Character;

        int cT = gameManager.CommonCharactersVariables.critTrigger;

        if (attacker.AttackTarget.Qin) {

            attackedPreviewFight.name.text = "Qin";

            attackedPreviewFight.health.Set(SC_Qin.Energy - attacker.BaseDamage, SC_Qin.Energy, SC_Qin.Energy);

            NonCharacterAttackPreview();

        } else if (a) {

            attackedPreviewFight.name.text = a.characterName;

            if (!PreviewCharacterAttack(attacker, a, false) && a.GetActiveWeapon().Range(a).In(fightManager.AttackRange))
                PreviewCharacterAttack(a, attacker, true);

            attackedPreviewFight.crit.Set(a.CriticalAmount, Mathf.Min(a.CriticalAmount + a.Technique, cT), cT);

        } else {

            SC_Construction c = attacker.AttackTarget.Construction;

            attackedPreviewFight.name.text = c.Name;

            attackedPreviewFight.health.Set(Mathf.Max(0, c.Health - attacker.BaseDamage), c.Health, c.maxHealth);

            NonCharacterAttackPreview();

        }

        attackerPreviewFight.crit.Set(attacker.CriticalAmount, Mathf.Min(attacker.CriticalAmount + attacker.Technique, cT), cT);

        attacker.Hero?.SetWeapon(activeWeapon);



        previewFightPanel.SetActive(true);

    }

    void NonCharacterAttackPreview() {

        attackedPreviewFight.constructionHealth.gameObject.SetActive(false);

        attackedPreviewFight.crit.gameObject.SetActive(false);

        attackedPreviewFight.dodge.gameObject.SetActive(false);

        attackerPreviewFight.dodge.Set(SC_Character.attackingCharacter.DodgeAmount, SC_Character.attackingCharacter.DodgeAmount, gameManager.CommonCharactersVariables.dodgeTrigger);

    }

    bool PreviewCharacterAttack(SC_Character attacker, SC_Character attacked, bool counter) {

        bool killed = false;

        SC_Construction c = attacked.Tile.Construction;

        int bD = attacker.BaseDamage;

        CharacterFightPreview attackedPF = counter ? attackerPreviewFight : attackedPreviewFight;

        int dT = gameManager.CommonCharactersVariables.dodgeTrigger;

        if (c && c.GreatWall) {

            killed = c.Health - bD <= 0;

            attackedPF.health.Set(killed ? 0 : attacked.Health, attacked.Health, attacked.maxHealth);

            attackedPF.constructionName.text = c.Name;

            attackedPF.constructionHealth.Set(Mathf.Max(0, c.Health - bD), c.Health, c.maxHealth);

            attackedPF.constructionHealth.gameObject.SetActive(true);

            attackedPF.dodge.Set(attacked.DodgeAmount, attacked.DodgeAmount, dT);

        } else {

            int lifeLeft = attacked.Health - fightManager.CalcDamages(attacker, attacked, counter);

            killed = lifeLeft <= 0;

            attackedPF.health.Set(Mathf.Max(0, lifeLeft), attacked.Health, attacked.maxHealth);            

            attackedPF.dodge.Set(attacked.DodgeAmount, Mathf.Min(attacked.DodgeAmount + attacked.Technique, dT), dT);

        }

        return killed;

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

        (first ? weaponChoice1 : weaponChoice2).GetComponentInChildren<Text>().text = weapon.weaponName;

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

    public void ActivateMenu (bool playerMenu) {

        GameObject menu = playerMenu ? playerActionsPanel : characterActionsPanel;

        RectTransform Rect = menu.GetComponent<RectTransform>();

        Vector3 currentTileViewportPos = Camera.main.WorldToViewportPoint(TileManager.GetTileAt(SC_Cursor.Instance.gameObject).transform.position);

        int offset = currentTileViewportPos.x < 0.5 ? 1 : -1;

        Rect.anchorMin = new Vector3(currentTileViewportPos.x + (offset * (0.1f + (0.05f * (1 / (Mathf.Pow(Camera.main.orthographicSize, Camera.main.orthographicSize / 4)))))), currentTileViewportPos.y, currentTileViewportPos.z);
        Rect.anchorMax = Rect.anchorMin;

        menu.SetActive(true);

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

}
