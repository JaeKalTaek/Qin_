using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using static SC_Global;
using static SC_Player;
using static SC_Character;
using TMPro;
using Prototype.NetworkLobby;

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
    public GameObject playerActionsPanel, optionsPanel, soundPanel, concedePanel;
    public GameObject endTurnButton;
    public Toggle healthBarsToggle;
    public TileTooltip tileTooltip;
    public Slider musicVolume;

    [Header("Fight UI")]
    public GameObject previewFightPanel;
    public PreviewFightValues attackerPreviewFight, attackedPreviewFight;
    public FightPanel fightPanel;
    public TextMeshProUGUI combatFeedbackText;

    [Header("Colors")]
    public Color maxHealthColor;
    public Color minHealthColor;

    [Header("Characters")]
    public CharacterTooltip characterTooltip;
    public CharacterDetails characterDetails;
    public GameObject characterActionsPanel;
    public GameObject attackButton, destroyConstruButton, buildConstruButton;    

    [Header("Heroes")]
    public RelationshipDetails[] relationshipsDetails;
    public GameObject weaponChoicePanel, weaponChoice1, weaponChoice2;
    // public GameObject usePower;

    [Header("Qin")]
    public Text qinEnergy;
    public GameObject construct, constructPanel, endQinConstru, cancelSoldierConstru;
    public SC_QinCancelConstruButton cancelQinConstru;
    public Transform qinConstrus, soldierConstrus;
    public CreationTooltip construTooltip, soldierTooltip;
    public Transform qinPower;
    public GameObject sacrifice, endSacrifice;
    public GameObject pitPanel;
    public CreateDemonPanel createDemonPanel;
    public SacrificeCastlePanel sacrificeCastlePanel;

    [Header("Transforms")]
    public Transform tilesT;
    public Transform bordersT, soldiersT, heroesT, demonsT, wallsT, bastionsT, castlesT, pitsT, ruinsT, villagesT, drainingStelesT;
    #endregion

    #region Variables
    public GameObject CurrentChara { get; set; }

    public GameObject CurrentTile { get; set; }

    static SC_Game_Manager GameManager { get { return SC_Game_Manager.Instance; } }

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

    public static SC_UI_Manager Instance { get; set; }

    public static Vector2 Size { get { return Instance.GetComponent<RectTransform>().sizeDelta; } }

    public static bool CanInteract { get { return (!EventSystem.current.IsPointerOverGameObject() || !Cursor.visible) && !GameManager.prep; } }

    public float clickSecurityDuration;

    public static bool clickSecurity;

    GameObject grid;

    public Action backAction;

    Selectable previouslySelected;
    #endregion

    #region Setup
    private void Awake() {

        Instance = this;

        backAction = DoNothing;

    }

    public void SetupUI(bool qin) {

        fightManager = SC_Fight_Manager.Instance;

        if (GameManager.prep) {

            qinPreparationPanel.SetActive(qin);

            heroesPreparationPanel.SetActive(!qin);

            preparationPanel.SetActive(true);

        } else
            gamePanel.SetActive(true);

        // Setup Grid
        SpriteRenderer gridRenderer = Instantiate(Resources.Load<GameObject>("Prefabs/UI/P_Grid").GetComponent<SpriteRenderer>());
        Vector3 size = new Vector3(XSize, YSize, 1) * GameManager.CurrentMapPrefab.TileSize;
        gridRenderer.size = new Vector2(size.x, size.y);
        grid = gridRenderer.gameObject;
        grid.transform.position = Vector3.Scale((size - Vector3.one * GameManager.CurrentMapPrefab.TileSize) / 2f, new Vector3(1, 1, 0));        

    }
    #endregion

    #region Preparation Phase
    public void SetReady () {

        bool canSetReady = true;

        if(localPlayer.Qin)
            foreach (SC_Castle castle in FindObjectsOfType<SC_Castle>())
                if (castle.CastleType == null)             
                    canSetReady = false;

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

        go.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/Castle/Roofs/" + castleType);

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
        optionsPanel.SetActive(false);
        soundPanel.SetActive(false);
        concedePanel.SetActive(false);
        characterDetails.panel.SetActive(false);

        backAction = DoNothing;

        //usePower.SetActive (!gameManager.Qin && !SC_Player.localPlayer.Qin);        

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

        characterTooltip.icon.sprite = character.Sprite.sprite;

        characterTooltip.name.text = character.characterName;

        characterTooltip.healthLabel.text = "HP";

        characterTooltip.health.Set(character.Health, character.MaxHealth);

        characterTooltip.health.GetComponentInChildren<Text>().text = character.Health + " / " + character.MaxHealth;

        characterTooltip.crit.Set(character.CriticalAmount, GameManager.CommonCharactersVariables.critTrigger, false);

        characterTooltip.crit.GetComponentInChildren<Text>().text = character.CriticalAmount + " / " + GameManager.CommonCharactersVariables.critTrigger;

        characterTooltip.dodge.Set(character.DodgeAmount, GameManager.CommonCharactersVariables.dodgeTrigger, false);

        characterTooltip.dodge.GetComponentInChildren<Text>().text = character.DodgeAmount + " / " + GameManager.CommonCharactersVariables.dodgeTrigger;

        characterTooltip.critContainer.SetActive(true);

        characterTooltip.dodgeContainer.SetActive(true);

        characterTooltip.panel.SetActive(true);

        if (SC_Tile.CanChangeFilters)
            TileManager.DisplayMovementAndAttack(character, true);

    }

    #region Characters Details
    public void DisplayCharacterDetails(SC_Character c) {

        DisplayCharacterDetails(true);

        ShowCharacterInfos(c);

        foreach (Transform t in characterDetails.stats.GetChild(0))
            if(t.gameObject.activeSelf)
                t.GetChild(1).GetComponent<Text>().text = GetStat(c, t.name);

        for (int i = 0; i < characterDetails.weapons.GetChild(0).childCount; i++)
            characterDetails.weapons.GetChild(0).GetChild(i).GetComponent<Text>().text = (i == 0) ? c.GetActiveWeapon().weaponName : (i == 1) ? (c.Hero?.GetWeapon(false).weaponName) : "";

        if(c.Hero) {

            for (int i = 0; i < c.Hero.RelationshipKeys.Count; i++) {

                int v;

                c.Hero.Relationships.TryGetValue(c.Hero.RelationshipKeys[i], out v);

                relationshipsDetails[i].icon.sprite = Resources.Load<SC_Hero>("Prefabs/Characters/Heroes/P_" + c.Hero.RelationshipKeys[i].Replace(" ", "_")).GetComponentInChildren<SpriteRenderer>().sprite;

                relationshipsDetails[i].boostValue.text = "+" + GameManager.CommonCharactersVariables.relationValues.GetValue("boost", v) + "% atk/def";

                relationshipsDetails[i].boostValue.gameObject.SetActive(true);

                relationshipsDetails[i].relationValue.text = v.ToString();

                relationshipsDetails[i].relationValue.gameObject.SetActive(false);

                relationshipsDetails[i].link.sizeDelta = new Vector2(GameManager.CommonCharactersVariables.relationValues.GetValue("link", v), relationshipsDetails[i].link.sizeDelta.y);

            }

            characterDetails.relationshipsPanel.GetChild(6).GetComponent<Image>().sprite = c.Sprite.sprite;

        }

        characterDetails.relationshipsPanel.gameObject.SetActive(c.Hero);
        // characterDetails.soldierPanel.SetActive(c.Soldier);        

    }

    public void ToggleRelationshipValueType (int id) {

        relationshipsDetails[id].boostValue.gameObject.SetActive(!relationshipsDetails[id].boostValue.gameObject.activeSelf);

        relationshipsDetails[id].relationValue.gameObject.SetActive(!relationshipsDetails[id].relationValue.gameObject.activeSelf);

    }

    void DisplayCharacterDetails(bool b) {

        SC_Cursor.SetLock(b);

        backAction = b ? (Action)(() => DisplayCharacterDetails(false)) : DoNothing;

        characterDetails.panel.SetActive(b);

        turnIndicator.transform.parent.gameObject.SetActive(!b);

        // tileTooltip.panel.SetActive(!b);

    }

    string GetStat(SC_Character c, string s) {

        int stat = (int)c.GetType().GetProperty(s).GetValue(c);

        int baseStat = (int)c.baseStats.GetType().GetField(s.ToLower()).GetValue(c.baseStats);

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

        bool movingDemon = characterToMove?.Demon && localPlayer.Qin;

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

        if (SC_Tile.CanChangeFilters && construction.DrainingStele) {

            TileManager.DisplayedDrainingStele = construction.DrainingStele;

            foreach (SC_Tile tile in TileManager.GetRange(construction.transform.position, construction.DrainingStele.range))
                tile.SetFilter(TDisplay.Attack);

        }

	}

	void ShowQinInfos() {

        CurrentChara = SC_Qin.Qin.gameObject;

        characterTooltip.icon.sprite = SC_Qin.Qin.GetComponentInChildren<SpriteRenderer>().sprite;

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

        SC_Construction attackerConstru = activeCharacter.Tile.AttackableContru;

        activeCharacter.Hero?.SetWeapon(activeWeapon);        

        attackerPreviewFight.name.text = activeCharacter.characterName + (attackerConstru ? " on " + (attackerConstru as SC_Castle ? "Castle" : attackerConstru.Name) : "");

        SC_Character attacked = activeCharacter.AttackTarget.Character;
        SC_Construction attackedConstru = activeCharacter.AttackTarget.AttackableContru;

        attackedPreviewFight.crit.gameObject.SetActive(attacked);

        attackedPreviewFight.dodge.gameObject.SetActive(attacked && !attackedConstru);

        if (activeCharacter.AttackTarget.Qin) {

            attackedPreviewFight.name.text = "Qin";

            attackedPreviewFight.health.Set(SC_Qin.Energy - activeCharacter.BaseDamage, SC_Qin.Energy, SC_Qin.Energy);

            NonCharacterAttackPreview();

        } else if (attacked) {

            attackedPreviewFight.name.text = attacked.characterName + (attackedConstru ? " on " + (attackedConstru as SC_Castle ? "Castle" : attackedConstru.Name) : "");
         
            PreviewCharacterAttack(attacked, activeCharacter, PreviewCharacterAttack(activeCharacter, attacked) || !attacked.GetActiveWeapon().Range(attacked).In(fightManager.AttackRange));

        } else {

            SC_Construction c = activeCharacter.AttackTarget.AttackableContru;

            attackedPreviewFight.name.text = c.Name;

            attackedPreviewFight.health.Set(Mathf.Max(0, c.Health - activeCharacter.BaseDamage), c.Health, c.maxHealth);

            NonCharacterAttackPreview();

        }

        activeCharacter.Hero?.SetWeapon(activeWeapon);

        previewFightPanel.SetActive(true);

    }

    void NonCharacterAttackPreview() {

        attackerPreviewFight.health.Set(activeCharacter.Health, activeCharacter.Health, activeCharacter.MaxHealth);

        //attackedPreviewFight.constructionHealth.gameObject.SetActive(false);

        // attackedPreviewFight.crit.gameObject.SetActive(false);

        // attackedPreviewFight.dodge.gameObject.SetActive(false);

        attackerPreviewFight.crit.Set(activeCharacter.CriticalAmount, Mathf.Min(activeCharacter.CriticalAmount + activeCharacter.Technique, GameManager.CommonCharactersVariables.critTrigger), GameManager.CommonCharactersVariables.critTrigger);

        attackerPreviewFight.dodge.Set(activeCharacter.DodgeAmount, activeCharacter.DodgeAmount, GameManager.CommonCharactersVariables.dodgeTrigger);
        
    }

    bool PreviewCharacterAttack(SC_Character attacker, SC_Character attacked, bool cantCounter = false) {

        bool attackedKilled = false;

        SC_Construction c = attacked.Tile.AttackableContru;

        print(c);

        int bD = attacker.BaseDamage;

        PreviewFightValues attackedPF = attacker != activeCharacter ? attackerPreviewFight : attackedPreviewFight;

        int dT = GameManager.CommonCharactersVariables.dodgeTrigger;
        int cT = GameManager.CommonCharactersVariables.critTrigger;

        if (c) {

            int healthLeft = c.Health - (cantCounter ? 0 : bD);

            attackedKilled = healthLeft <= 0;

            attackedPF.health.Set(Mathf.Max(0, healthLeft), c.Health, c.maxHealth);

            /*attackedPF.constructionName.text = c.Name;

            attackedPF.constructionHealth.Set(Mathf.Max(0, healthLeft), c.Health, c.maxHealth);

            attackedPF.constructionHealth.gameObject.SetActive(true);*/

            attackedPF.dodge.Set(attacked.DodgeAmount, attacked.DodgeAmount, dT);

        } else {

            int healthLeft = attacked.Health - (cantCounter ? 0 : fightManager.CalcDamage(attacker, attacked));
            
            attackedKilled = healthLeft <= 0;

            attackedPF.health.Set(Mathf.Max(0, healthLeft), attacked.Health, attacked.MaxHealth);            

            attackedPF.dodge.Set(attacked.DodgeAmount, Mathf.Min(attacked.DodgeAmount + (cantCounter ? 0 : attacked.Reflexes), dT), dT);
          
        }

        attackedPF.crit.Set(attacked.CriticalAmount, Mathf.Min(attacked.CriticalAmount + (attackedKilled || !attacked.GetActiveWeapon().Range(attacked).In(fightManager.AttackRange) ? 0 : attacked.Technique), cT), cT);

        return attackedKilled;

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
    public void ShowHideWeapon (bool firstWeapon, bool active) {

        (firstWeapon ? weaponChoice1 : weaponChoice2).SetActive(active);

        (firstWeapon ? weaponChoice1 : weaponChoice2).GetComponentInChildren<Text>().text = activeCharacter.Hero.GetWeapon(firstWeapon).weaponName;

    }

    public void ChooseWeapon () {        

        ShowHideWeapon(true, activeCharacter.Hero.weapon1.Range(activeCharacter.Hero).In(fightManager.AttackRange));

        ShowHideWeapon(false, activeCharacter.Hero.weapon2.Range(activeCharacter.Hero).In(fightManager.AttackRange));

        weaponChoicePanel.SetActive(true);

        ForceSelect(weaponChoicePanel.GetComponentInChildren<Button>().gameObject);

        backAction = ResetAttackChoice;

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
    public void StartQinAction() {

        localPlayer.Busy = true;

        TileManager.RemoveAllFilters();

        playerActionsPanel.SetActive(false);

        backAction = EndQinAction;

    }

    public void EndQinAction() {

        TileManager.RemoveAllFilters();

        StartCoroutine(ClickSafety(() => { SC_Cursor.SetLock(false); }));

        backAction = DoNothing;

        constructPanel.SetActive(false);

        endSacrifice.SetActive(false);

        pitPanel.SetActive(false);

        createDemonPanel.panel.SetActive(false);

        sacrificeCastlePanel.panel.SetActive(false);

        localPlayer.Busy = false;        

    }

    // Called by UI
    public void DisplaySacrifices () {

        StartQinAction();

        endSacrifice.SetActive(true);

        TileManager.DisplaySacrifices();

        StartCoroutine(ClickSafety(() => { SC_Cursor.SetLock(false); }));

    }

    // Called by UI
    /*public void DisplayResurrection () {

        if (!SC_Player.localPlayer.Busy && gameManager.LastHeroDead && (SC_Qin.Energy > SC_Qin.Qin.powerCost)) {

            StartQinAction("qinPower");

            TileManager.DisplayResurrection();

        }

    }*/

    void DisplayActionPanel() {

        SC_Cursor.SetLock(true);

        clickSecurity = true;

        StartCoroutine(ClickSafety(() => { clickSecurity = false; }));

        StartQinAction();

    }

    public void CreateDemon(SC_Castle castle) {

        GameManager.CurrentCastle = castle;

        DisplayActionPanel();

        createDemonPanel.name.text = castle.CastleType + " Demon";

        createDemonPanel.create.image.sprite = Resources.Load<Sprite>("Sprites/Characters/Demons/" + castle.CastleType);

        createDemonPanel.cost.text = "Cost : " + castle.DemonCost + " vital energy";        

        createDemonPanel.panel.SetActive(true);

        createDemonPanel.create.Select();

    }

    public void DisplaySacrificeCastlePanel (SC_Castle castle) {

        GameManager.CurrentCastle = castle;

        DisplayActionPanel();

        float value = SC_Game_Manager.GetCurrentCastleSacrificeValue();

        bool can = value >= 0;

        if (can) {

            sacrificeCastlePanel.type.text = castle.CastleType + " Demon buff : ";

            sacrificeCastlePanel.buff.text = (value == 0) ? "None" : "+" + value + "% stats";

            backAction = EndQinAction;

        }       

        sacrificeCastlePanel.panel.SetActive(true);

        (can ? sacrificeCastlePanel.canPanel : sacrificeCastlePanel.cantPanel).SetActive(true);

        (can ? sacrificeCastlePanel.cantPanel : sacrificeCastlePanel.canPanel).SetActive(false);

        (can ? sacrificeCastlePanel.yes : sacrificeCastlePanel.close).Select();

    }

    #region Construction
    public void DisplayConstructPanel(bool qin) {

        constructPanel.SetActive(true);

        qinConstrus.gameObject.SetActive(qin);
        soldierConstrus.gameObject.SetActive(!qin);

        endQinConstru.SetActive(qin);

        cancelQinConstru.gameObject.SetActive(qin);
        if(qin)
            cancelQinConstru.SetCanClick(false);

        cancelSoldierConstru.SetActive(!qin);

        UpdateCreationPanel(qin ? qinConstrus : soldierConstrus, true);

    }

    public void UpdateCreationPanel (Transform t, bool open = false) {

        foreach (SC_UI_Creation c in t.GetComponentsInChildren<SC_UI_Creation>())
            c.SetCanClick();

        if (open) {

            foreach (SC_UI_Creation c in GetComponentsInChildren<SC_UI_Creation>())
                c.OnDeselect(new BaseEventData(EventSystem.current));

            ForceSelect(t.GetComponentInChildren<Button>().gameObject);

        }

    }

    // Called by UI
    public void DisplayQinConstructPanel() {

        StartQinAction();

        DisplayConstructPanel(true);

        SelectConstruct();

    }

    // Called by UI
    public void DisplaySoldiersConstructPanel () {

        characterActionsPanel.SetActive(false);

        TileManager.RemoveAllFilters();

        DisplayConstructPanel(false);

        backAction = CancelAction;

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

        backAction = EndQinAction;

    }    

    // Called by UI
    public void DisplayConstructableTiles(string c) {

        previouslySelected = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

        backAction = EndQinAction;

        EventSystem.current.sendNavigationEvents = false;

        StartCoroutine(ClickSafety(() => { SC_Cursor.SetLock(false); }));

        localPlayer.CmdSetConstru(c);

        TileManager.RemoveAllFilters();

        TileManager.DisplayConstructableTiles(c);

    }
    #endregion

    #region Pit
    public void DisplayPitPanel() {

        DisplayActionPanel();

        pitPanel.SetActive(true);

        UpdateCreationPanel(pitPanel.transform.GetChild(1), true);

    }    

    public void PitCreateSoldier (string s) {

        if (!clickSecurity) {            

            localPlayer.CmdCreateSoldier(GameManager.CurrentPitPos, s);

            EndQinAction();

        }

    }
    #endregion

    #endregion

    #region Both Players  
    void Update () {

        if (Input.GetButtonDown("Cancel"))
            backAction();
        else if (Input.GetButtonDown("DisplayDetails") && CurrentChara?.GetComponent<SC_Character>() && !SC_Cursor.Instance.Locked)
            DisplayCharacterDetails(CurrentChara.GetComponent<SC_Character>());

        if(draggedCastle)
            draggedCastle.transform.position = WorldMousePos;

        if (victoryPanel.activeSelf && Input.anyKeyDown) {

            StopCoroutine(ForceMainMenuReturn());

            LobbyManager.s_Singleton.StopClientClbk();

        }

    }

    public void SetGridVisiblity (bool on) {

        grid.SetActive(on);

    }

    void ActivatePlayerMenu() {

        if (localPlayer.Qin) {

            construct.SetActive(localPlayer.Turn);
            sacrifice.SetActive(localPlayer.Turn);

            if (GameManager.QinTurnStarting)
                localPlayer.CmdSetQinTurnStarting(false);

        }

        endTurnButton.SetActive(localPlayer.Turn);

        backAction = () => {

            SC_Cursor.SetLock(false);

            playerActionsPanel.SetActive(false);

        };

    }

    public void ActivateMenu (GameObject menu) {

        SC_Cursor.SetLock(true);

        if (menu == playerActionsPanel)
            ActivatePlayerMenu();

        menu.SetActive(true);

        ForceSelect(menu.transform.GetChild(menu.transform.childCount - 1).gameObject);

    }

    //Caled by UI
    public void DisplaySubMenu (GameObject parentPanel, GameObject subPanel, Action addToBack) {

        parentPanel.SetActive(false);

        ActivateMenu(subPanel);

        backAction = () => {

            subPanel.SetActive(false);

            addToBack();

        };

    }

    // Called by UI
    public void ActivateOptionsMenu () {

        DisplaySubMenu(playerActionsPanel, optionsPanel, () => { ActivateMenu(playerActionsPanel); });

    }

    // Called by UI
    public void ActivateSoundMenu () {

        DisplaySubMenu(optionsPanel, soundPanel, () => { ActivateOptionsMenu(); });

    }

    // Called by UI
    public void DisplayConcedePanel () {

        DisplaySubMenu(optionsPanel, concedePanel, () => { ActivateOptionsMenu(); });

    }

    // Called by UI
    public void ToggleHealth () {

        foreach (SC_Lifebar lifebar in FindObjectsOfType<SC_Lifebar>())
            lifebar.Toggle();

    }

    public void ShowVictory (bool qinWon) {

        SC_Sound_Manager.Instance.GameOver();

        victoryPanel.GetComponentInChildren<Text>().text = (qinWon ? "Qin" : "The Heroes") + " won the war !";

        victoryPanel.SetActive(true);

        StartCoroutine(ForceMainMenuReturn());

    }

    IEnumerator ForceMainMenuReturn () {

        yield return new WaitForSeconds(5f);

        LobbyManager.s_Singleton.StopClientClbk();

    }

    public void Attack() {

        SC_Cursor.SetLock(false);

        characterActionsPanel.SetActive(false);

        backAction = CancelAction;

        TileManager.CheckAttack();

    }

    public void Back () {

        backAction();

    }

    void CancelAction() {

        constructPanel.SetActive(false);

        TileManager.RemoveAllFilters();

        ActivateMenu(characterActionsPanel);

        backAction = GameManager.ResetMovement;

        TileManager.PreviewAttack();

        previewFightPanel.SetActive(false);

    }    
    #endregion

    #region Utility
    IEnumerator ClickSafety (Action a) {

        yield return new WaitForSeconds(clickSecurityDuration);

        a();

    }
    #endregion

}
