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
    public GameObject connectingPanel, preparationPanel;
    public GameObject qinPreparationPanel, heroesPreparationPanel;
    public GameObject readyButton, otherPlayerReady;
    public Color readyColor, notReadyColor;

    [Header("Game")]
    public GameObject gamePanel;
    public GameObject loadingPanel, victoryPanel;
    public Text turnIndicator;    
    public GameObject playerActionsPanel;
    public GameObject toggleHealthBarsButton, endTurnButton;
    public TileTooltip tileTooltip;

    [Header("Preview Fight")]
    public GameObject previewFightPanel;
    public CharacterFightPreview attackerPreviewFight, attackedPreviewFight;

    [Header("Characters")]
    public CharacterTooltip characterTooltip;
    public CharacterDetails characterDetails;
    public GameObject characterActionsPanel;
    public GameObject attackButton, destroyConstruButton, buildConstruButton;    

    [Header("Heroes")]
    public GameObject relationshipPanel;
    public GameObject weaponChoicePanel, weaponChoice1, weaponChoice2;
    // public GameObject usePower;

    [Header("Qin")]
    public Text qinEnergy;
    public GameObject construct, constructPanel, endQinConstru, cancelSoldierConstru;
    public Transform qinConstrus, soldierConstrus;
    public CreationTooltip construTooltip, soldierTooltip;
    public Transform qinPower;
    public GameObject sacrifice, endSacrifice;
    public GameObject workshopPanel;

    [Header("Transforms")]
    public Transform tilesT;
    public Transform bordersT, soldiersT, heroesT, demonsT, wallsT, bastionsT, castlesT, workshopsT, ruinsT, villagesT;
    #endregion

    #region Variables
    public GameObject CurrentChara { get; set; }

    public GameObject CurrentTile { get; set; }

    static SC_Game_Manager gameManager;

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

    public static SC_UI_Manager Instance { get; set; }

    public static bool CanInteract { get {

            return (!EventSystem.current.IsPointerOverGameObject() || !Cursor.visible) && !gameManager.prep;

    } }

    public float clickSecurityDuration;

    public static bool clickSecurity;

    GameObject grid;

    public Action returnAction, cancelAction;    

    public bool LifeBarsOn { get { return toggleHealthBarsButton.name == "On"; } }

    Selectable previouslySelected;
    #endregion

    #region Setup
    private void Awake() {

        Instance = this;

        returnAction = DoNothing;

    }

    public void SetupUI(bool qin) {       

        gameManager = SC_Game_Manager.Instance;

        fightManager = SC_Fight_Manager.Instance;

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

        cancelAction = DoNothing;

        turnIndicator.text = gameManager.Qin ? "Qin's Turn" : (gameManager.Turn % 3 == 1 ? "1st" : "2nd") + " Coalition's Turn";

	}
    #endregion

    #region Infos
    public void ShowInfos(GameObject g, Type t) {        

        if (t.IsSubclassOf(typeof(SC_Character)))
            ShowCharacterInfos(g.GetComponent<SC_Character>());
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

        characterTooltip.panel.SetActive(false);

        tileTooltip.panel.SetActive(false);

        CurrentChara = null;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        if((CurrentChara == g) || (CurrentTile == g))
            ShowInfos(g, t);

    }

	void ShowCharacterInfos(SC_Character character) {

        CurrentChara = character.gameObject;

        characterTooltip.icon.sprite = character.GetComponent<SpriteRenderer>().sprite;

        characterTooltip.name.text = character.characterName;

        characterTooltip.healthLabel.text = "Health";

        characterTooltip.health.Set(character.Health, character.maxHealth);

        characterTooltip.health.GetComponentInChildren<Text>().text = character.Health + " / " + character.maxHealth;

        characterTooltip.crit.Set(character.CriticalAmount, gameManager.CommonCharactersVariables.critTrigger);

        characterTooltip.crit.GetComponentInChildren<Text>().text = character.CriticalAmount + " / " + gameManager.CommonCharactersVariables.critTrigger;

        characterTooltip.dodge.Set(character.DodgeAmount, gameManager.CommonCharactersVariables.dodgeTrigger);

        characterTooltip.dodge.GetComponentInChildren<Text>().text = character.DodgeAmount + " / " + gameManager.CommonCharactersVariables.dodgeTrigger;

        characterTooltip.critContainer.SetActive(true);

        characterTooltip.dodgeContainer.SetActive(true);

        characterTooltip.panel.SetActive(true);

        if (SC_Tile.CanChangeFilters)
            TileManager.DisplayMovementAndAttack(character, true);

    }

    #region Characters Details
    public void DisplayCharacterDetails(SC_Character c) {

        ShowCharacterInfos(c);

        foreach (Transform t in characterDetails.stats)
            if(t.gameObject.activeSelf)
                t.GetChild(1).GetComponent<Text>().text = GetStat(c, t.name);

        for (int i = 0; i < characterDetails.weapons.childCount; i++)
            characterDetails.weapons.GetChild(i).GetComponent<Text>().text = (i == 0) ? c.GetActiveWeapon().weaponName : (i == 1) ? (c.Hero?.GetWeapon(false).weaponName) : "";

        if(c.Hero) {

            for (int i = 0; i < c.Hero.RelationshipKeys.Count; i++) {

                int v;

                c.Hero.Relationships.TryGetValue(c.Hero.RelationshipKeys[i], out v);

                Transform t = characterDetails.relationshipsPanel.GetChild(i);

                t.GetChild(0).GetComponent<Image>().sprite = Resources.Load<SC_Hero>("Prefabs/Characters/Heroes/P_" + c.Hero.RelationshipKeys[i].Replace(" ", "_")).GetComponent<SpriteRenderer>().sprite;

                t.GetChild(1).GetComponent<Text>().text = v.ToString();                

                (t.GetChild(2) as RectTransform).sizeDelta = new Vector2(gameManager.CommonCharactersVariables.relationValues.GetValue("link", v), (t.GetChild(2) as RectTransform).sizeDelta.y);

            }

            characterDetails.relationshipsPanel.GetChild(5).GetComponent<Image>().sprite = c.GetComponent<SpriteRenderer>().sprite;

        }

        characterDetails.relationshipsPanel.gameObject.SetActive(c.Hero);
        characterDetails.soldierPanel.SetActive(c.Soldier);

        DisplayCharacterDetails(true);

    }

    void DisplayCharacterDetails(bool b) {

        SC_Cursor.Instance.Locked = b;

        returnAction = b ? (Action)(() => DisplayCharacterDetails(false)) : DoNothing;

        characterDetails.panel.SetActive(b);

        turnIndicator.gameObject.SetActive(!b);

        qinEnergy.gameObject.SetActive(!b);

        tileTooltip.panel.SetActive(!b);

    }

    string GetStat(SC_Character c, string s) {

        int stat = (int)c.GetType().GetProperty(s).GetValue(c);

        int baseStat = (int)c.GetType().GetField("base" + s).GetValue(c);

        int modifiers = stat - baseStat;

        return stat + (modifiers == 0 ? "" : (" (" + baseStat + " " + (modifiers > 0 ? "+" : "-") + " " + Mathf.Abs(modifiers) + ")"));

    }
    #endregion

    void ShowTileTooltip (SC_Tile t) {

        CurrentTile = t.gameObject;

        if (t.Construction)
            ShowConstructionsInfos(t.Construction);
        else {

            tileTooltip.name.text = t.infos.type;
            tileTooltip.health.gameObject.SetActive(false);

        }

        bool movingDemon = SC_Character.characterToMove?.Demon;

        bool q = t.Character?.Qin ?? localPlayer.Qin;

        tileTooltip.power.text = t.CombatModifiers.strength + (movingDemon ? 0 : t.DemonsModifier("strength", q)) + "";
        tileTooltip.defense.text = t.CombatModifiers.armor + (movingDemon ? 0 : t.DemonsModifier("armor", q)) + "";
        tileTooltip.technique.text = t.CombatModifiers.technique + (movingDemon ? 0 : t.DemonsModifier("technique", q)) + "";
        tileTooltip.reflexes.text = t.CombatModifiers.reflexes + (movingDemon ? 0 : t.DemonsModifier("reflexes", q)) + "";
        tileTooltip.range.text = t.CombatModifiers.range + (movingDemon ? 0 : t.DemonsModifier("range", q)) + "";
        tileTooltip.movement.text = t.CombatModifiers.movement + (movingDemon ? 0 : t.DemonsModifier("movement", q)) + "";

        tileTooltip.panel.SetActive(true);

    }

	void ShowConstructionsInfos(SC_Construction construction) {

        tileTooltip.name.text = construction.Name;

        if (construction.Health > 0) {

            tileTooltip.health.Set(construction.Health, construction.maxHealth);
            tileTooltip.health.gameObject.SetActive(true);

        }

        if (SC_Tile.CanChangeFilters && construction.Pump) {

            TileManager.DisplayedPump = construction.Pump;

            foreach (SC_Tile tile in TileManager.GetRange(construction.transform.position, construction.Pump.range))
                tile.SetFilter(TDisplay.Attack);

        }

	}

	void ShowQinInfos() {

        CurrentChara = SC_Qin.Qin.gameObject;

        characterTooltip.icon.sprite = SC_Qin.Qin.GetComponent<SpriteRenderer>().sprite;

        characterTooltip.name.text = "Qin";

        characterTooltip.healthLabel.text = "Energy";

        characterTooltip.health.Set(SC_Qin.Energy, SC_Qin.Qin.energyToWin);

        characterTooltip.health.GetComponentInChildren<Text>().text = SC_Qin.Energy + " / " + SC_Qin.Qin.energyToWin;

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

            attackedPreviewFight.crit.gameObject.SetActive(true);

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

        attackedPF.dodge.gameObject.SetActive(true);

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

    public void ChooseWeapon (SC_Hero h) {

        weaponChoicePanel.SetActive(true);

        if (h.weapon1.Range(h).In(fightManager.AttackRange))
            ShowWeapon(h.GetWeapon(true), true);

        if (h.weapon2.Range(h).In(fightManager.AttackRange))
            ShowWeapon(h.GetWeapon(false), false);

        cancelAction = ResetAttackChoice;

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

            localPlayer.CmdSetQinTurnStarting(false);            

            TileManager.RemoveAllFilters();

            playerActionsPanel.SetActive(false);

            cancelAction = DoNothing;

        }

    }

    public void EndQinAction(string action) {

        TileManager.RemoveAllFilters();

        StartCoroutine(ClickSafety(() => { SC_Cursor.SetLock(false); }));

        if (action == "construct")
            cancelAction = DoNothing;

        constructPanel.SetActive(false);

        endSacrifice.SetActive(false);

        workshopPanel.SetActive(false);

        localPlayer.Busy = false;

        returnAction = DoNothing;

    }

    // Called by UI
    public void DisplaySacrifices () {

        if (!localPlayer.Busy) {

            StartQinAction("sacrifice");

            endSacrifice.SetActive(true);

            TileManager.DisplaySacrifices();

            returnAction = () => EndQinAction("sacrifice");

            StartCoroutine(ClickSafety(() => { SC_Cursor.SetLock(false); }));

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
    public void DisplayConstructPanel(bool qin) {        

        constructPanel.SetActive(true);

        qinConstrus.gameObject.SetActive(qin);
        soldierConstrus.gameObject.SetActive(!qin);

        endQinConstru.SetActive(qin);
        cancelSoldierConstru.SetActive(!qin);

        UpdateCreationPanel(qin ? qinConstrus : soldierConstrus, true);

    }

    public void UpdateCreationPanel (Transform t, bool open = false) {

        foreach (SC_UI_Creation c in t.GetComponentsInChildren<SC_UI_Creation>())
            c.SetCanClick();

        if (open)
            ForceSelect(t.GetComponentInChildren<Button>().gameObject);

    }

    // Called by UI
    public void DisplayQinConstructPanel() {

        StartQinAction("construct");

        DisplayConstructPanel(true);

        SelectConstruct();

    }

    // Called by UI
    public void DisplaySoldiersConstructPanel () {

        characterActionsPanel.SetActive(false);

        TileManager.RemoveAllFilters();

        DisplayConstructPanel(false);

        cancelAction = CancelAction;

    }

    public void UpdateCreationTooltip(string[] values, bool constru) {

        CreationTooltip cT = constru ? construTooltip : soldierTooltip;

        cT.name.text = values[0];

        cT.cost.text = values[1];

        cT.desc.text = values[2];

    }

    public void SelectConstruct () {

        TileManager.RemoveAllFilters();

        SC_Cursor.SetLock(true);

        EventSystem.current.sendNavigationEvents = true;

        previouslySelected?.Select();

        returnAction = DoNothing;

    }    

    // Called by UI
    public void DisplayConstructableTiles(string c) {

        previouslySelected = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

        returnAction = SelectConstruct;

        EventSystem.current.sendNavigationEvents = false;

        StartCoroutine(ClickSafety(() => { SC_Cursor.SetLock(false); }));

        localPlayer.CmdSetConstru(c);

        TileManager.RemoveAllFilters();

        TileManager.DisplayConstructableTiles(c);

    }
    #endregion

    #region Workshop
    public void DisplayWorkshopPanel() {

        SC_Cursor.SetLock(true);        

        clickSecurity = true;

        StartCoroutine(ClickSafety(() => { clickSecurity = false; }));

        StartQinAction("workshop");

        workshopPanel.SetActive(true);

        UpdateCreationPanel(workshopPanel.transform.GetChild(1), true);

        cancelAction = () => { EndQinAction("workshop"); };

    }    

    public void WorkshopCreateSoldier (string s) {

        if (!clickSecurity) {            

            localPlayer.CmdCreateSoldier(gameManager.CurrentWorkshopPos, s);

            EndQinAction("workshop");

        }

    }
    #endregion

    #endregion

    #region Both Players  
    void Update () {

        if (Input.GetButtonDown("ToggleGrid"))
            grid.SetActive(!grid.activeSelf);
        else if (Input.GetButtonDown("Cancel"))
            cancelAction();
        else if (Input.GetButtonDown("Return"))
            returnAction();
        else if (Input.GetButtonDown("DisplayDetails") && CurrentChara?.GetComponent<SC_Character>())
            DisplayCharacterDetails(CurrentChara.GetComponent<SC_Character>());

        if (draggedCastle)
            draggedCastle.transform.SetPos(WorldMousePos);

    }

    public void ActivateMenu (bool playerMenu) {

        SC_Cursor.SetLock(true);

        if (playerMenu) {

            if (localPlayer.Qin) {

                construct.SetActive(localPlayer.Turn);
                sacrifice.SetActive(localPlayer.Turn);                

                if (gameManager.QinTurnStarting)
                    localPlayer.CmdSetQinTurnStarting(false);

            }

            endTurnButton.SetActive(localPlayer.Turn);

            cancelAction = DoNothing;

            returnAction = () => {

                SC_Cursor.SetLock(false);

                playerActionsPanel.SetActive(false);
                
            };

        }

        GameObject menu = playerMenu ? playerActionsPanel : characterActionsPanel;

        #region Set Menu Pos
        RectTransform Rect = menu.GetComponent<RectTransform>();

        Vector3 currentTileViewportPos = Camera.main.WorldToViewportPoint(SC_Cursor.Instance.transform.position);

        int offset = currentTileViewportPos.x < 0.5 ? 1 : -1;

        Rect.anchorMin = new Vector3(currentTileViewportPos.x + (offset * (0.1f + (0.05f * (1 / (Mathf.Pow(Camera.main.orthographicSize, Camera.main.orthographicSize / 4)))))), currentTileViewportPos.y, currentTileViewportPos.z);
        Rect.anchorMax = Rect.anchorMin;
        #endregion

        menu.SetActive(true);

        ForceSelect(menu.transform.Find((playerMenu ? "EndTurn" : "Cancel") + "Button").gameObject);

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

        SC_Cursor.SetLock(false);

        characterActionsPanel.SetActive(false);

        cancelAction = CancelAction;

        TileManager.CheckAttack();

    }

    public void Cancel () {

        cancelAction();

    }

    void CancelAction() {

        constructPanel.SetActive(false);

        TileManager.RemoveAllFilters();

        ActivateMenu(false);

        cancelAction = gameManager.ResetMovement;

        TileManager.PreviewAttack();

        previewFightPanel.SetActive(false);

    }

    public void Wait() {

        SC_Cursor.SetLock(false);

        localPlayer.CmdWait();

        characterActionsPanel.SetActive(false);

        cancelAction = DoNothing;

        localPlayer.Busy = false;

    }

    public void Return() {

        returnAction();

    }
    #endregion

    #region Utility
    IEnumerator ClickSafety (Action a) {

        yield return new WaitForSeconds(clickSecurityDuration);

        a();

    }
    #endregion

}
