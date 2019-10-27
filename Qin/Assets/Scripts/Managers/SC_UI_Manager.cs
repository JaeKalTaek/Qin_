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
using static SC_Hero;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;

public class SC_UI_Manager : MonoBehaviour {

    #region UI Elements
    [Header("Preparation")]
    public GameObject connectingPanel, preparationPanel;    
    public GameObject otherPlayerReady;
    public Color readyColor, notReadyColor;
    public HeroesPreparationUI heroPreparationUI;
    public QinPreparationUI qinPreprationUI;

    [Header("Game")]
    public GameObject gamePanel;
    public GameObject loadingPanel, victoryPanel;
    public Text turnIndicator;    
    public GameObject playerActionsPanel, optionsPanel, soundPanel, concedePanel;
    public GameObject endTurnButton;
    public Toggle healthBarsToggle;
    public TileTooltip tileTooltip;
    public Slider musicVolume;
    public NextTurnUI nextTurnUI;

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
    public GameObject characterActionsPanel, weaponChoicePanel;
    public GameObject attackButton, destroyConstruButton, buildConstruButton;    

    [Header("Heroes")]
    public RelationshipDetails[] relationshipsDetails;
    public HeroTooltip heroTooltip;
    public SC_UI_Stamina[] staminaUsage;
    public WarningStaminaDeathPanel warningStaminaDeathPanel;
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

    [Header ("Transparency Focus")]
    [Tooltip("List of menus that can become transparent when camera needs to focus \"behind them\"")]
    public List<GameObject> transparencyFocusMenus;

    [Tooltip ("Transparency to set the menus affected to")]
    [Range (0, 1)]
    public float transparency;
    #endregion

    #region Variables
    public Dictionary<GameObject, Vector3> staticUIs;

    public GameObject CurrentChara { get; set; }

    public GameObject CurrentTile { get; set; }

    static SC_Game_Manager GameManager { get { return SC_Game_Manager.Instance; } }

    public SC_Tile_Manager TileManager { get; set; }

    static SC_Fight_Manager fightManager;

    public static SC_UI_Manager Instance { get; set; }

    public static Vector2 Size { get { return Instance.GetComponent<RectTransform>().sizeDelta; } }

    public static bool CanInteract { get { return (!EventSystem.current.IsPointerOverGameObject() || !Cursor.visible) && !GameManager.prep; } }

    [Header("Other variables")]
    public float clickSecurityDuration;

    public static bool clickSecurity;

    GameObject grid;

    public Action backAction;

    Selectable previouslySelected;

    public int CurrentMaxSlotsCount { get; set; }
    #endregion

    #region Setup
    private void Awake() {

        CurrentMaxSlotsCount = 6;

        Instance = this;

        backAction = DoNothing;

        staticUIs = new Dictionary<GameObject, Vector3> ();

    }

    public void SetupUI(bool qin) {

        fightManager = SC_Fight_Manager.Instance;

        if (GameManager.prep) {

            qinPreprationUI.panel.SetActive(qin);

            heroPreparationUI.panel.SetActive(!qin);

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
    public int PreparationPhase { get; set; }

    public void ToggleReady () {

        bool canSetReady = true;

        if (localPlayer.Qin)
            foreach (SC_Castle castle in FindObjectsOfType<SC_Castle> ())
                canSetReady &= castle.CastleType != null;

        if (canSetReady) {

            localPlayer.Ready ^= true;

            localPlayer.CmdReady(localPlayer.Ready, localPlayer.Qin);

        }

    }

    public void ToggleReady (bool r) {

        otherPlayerReady.GetComponent<Image>().color = r ? readyColor : notReadyColor;

        otherPlayerReady.GetComponentInChildren<Text> ().text = "Other Player is " + (r ? "" : "not ") + " ready";

    }

    public void Load() {

        loadingPanel.SetActive(true);

        preparationPanel.SetActive(false);

        gamePanel.SetActive(true);

    }

    #region Heroes
    int herosPreparationSlotsCount;

    public int HeroesPreparationSlotsCount {

        get { return herosPreparationSlotsCount; }

        set {

            herosPreparationSlotsCount = value;

            heroPreparationUI.preparationSlotsCount.text = value + "/" + CurrentMaxSlotsCount;

            bool b = true;

            if (PreparationPhase != (int)EHeroPreparationElement.Weapon)
                b = value == CurrentMaxSlotsCount;
            else {

                foreach (SC_HeroDeck heroDeck in heroPreparationUI.heroDecks)
                    b &= !heroDeck.Weapons[0].IsDefault;

                b &= value <= CurrentMaxSlotsCount;

            }

            heroPreparationUI.continueButton.interactable = b;

        }

    }    

    public void HeroPreparationContinue () {

        PreparationPhase++;

        switch (PreparationPhase - 1) {

            case 0:           

                CurrentMaxSlotsCount = GameManager.CommonCharactersVariables.maxTotalWeaponsCount;

                herosPreparationSlotsCount = 0;

                foreach (SC_HeroDeck heroDeck in heroPreparationUI.heroDecks) {

                    foreach (SC_PreparationSlot s in heroDeck.Weapons) {

                        s.gameObject.SetActive (true);

                        herosPreparationSlotsCount += s.IsDefault ? 0 : 1;                    

                    }

                }

                HeroesPreparationSlotsCount = herosPreparationSlotsCount;

                heroPreparationUI.heroesPool.SetActive (false);
                heroPreparationUI.weaponsPool.SetActive (true);

                heroPreparationUI.returnButton.gameObject.SetActive (true);

                break;

            case 1:

                CurrentMaxSlotsCount = 6;

                herosPreparationSlotsCount = 0;                

                foreach (SC_HeroDeck heroDeck in heroPreparationUI.heroDecks) {

                    heroDeck.Trap.gameObject.SetActive (true);

                    herosPreparationSlotsCount += heroDeck.Trap.IsDefault ? 0 : 1;

                }

                HeroesPreparationSlotsCount = herosPreparationSlotsCount;

                heroPreparationUI.weaponsPool.SetActive (false);
                heroPreparationUI.trapsPool.SetActive (true);

                break;

            case 2:

                heroPreparationUI.pool.SetActive (false);
                heroPreparationUI.returnButton2.gameObject.SetActive (true);
                heroPreparationUI.confirmButton.gameObject.SetActive (true);
                
                if (!FindObjectOfType <SC_DeploymentHero> ())
                    for (int i = 0; i < heroPreparationUI.heroDecks.Count; i++)
                        Instantiate (Resources.Load<SC_DeploymentHero> ("Prefabs/Characters/Heroes/P_DeploymentHero"), TileManager.DeploymentTiles [i].transform.position, Quaternion.identity).SpriteR.sprite = heroPreparationUI.heroDecks [i].Hero.Renderer.sprite;

                break;

            case 3:

                heroPreparationUI.returnButton2.gameObject.SetActive (false);
                heroPreparationUI.confirmButton.gameObject.SetActive (false);
                heroPreparationUI.cancelButton.gameObject.SetActive (true);

                ToggleReady ();

                break;

        }        

    }

    public void HeroPreparationReturn () {

        PreparationPhase--;

        switch (PreparationPhase + 1) {

            case 1:

                CurrentMaxSlotsCount = 6;

                HeroesPreparationSlotsCount = 0;

                foreach (SC_HeroDeck heroDeck in heroPreparationUI.heroDecks)
                    HeroesPreparationSlotsCount += heroDeck.Hero.IsDefault ? 0 : 1;

                heroPreparationUI.heroesPool.SetActive (true);
                heroPreparationUI.returnButton.gameObject.SetActive (false);
                heroPreparationUI.weaponsPool.SetActive (false);

                heroPreparationUI.returnButton.gameObject.SetActive (false);

                break;

            case 2:

                CurrentMaxSlotsCount = GameManager.CommonCharactersVariables.maxTotalWeaponsCount;

                HeroesPreparationSlotsCount = 0;

                foreach (SC_HeroDeck heroDeck in heroPreparationUI.heroDecks)
                    foreach (SC_PreparationSlot s in heroDeck.Weapons)
                        HeroesPreparationSlotsCount += s.IsDefault ? 0 : 1;

                heroPreparationUI.weaponsPool.SetActive (true);
                heroPreparationUI.trapsPool.SetActive (false);

                break;

            case 3:

                heroPreparationUI.pool.SetActive (true);
                heroPreparationUI.returnButton2.gameObject.SetActive (false);
                heroPreparationUI.confirmButton.gameObject.SetActive (false);

                //SC_Cursor.SetLock (true);

                break;

            case 4:

                heroPreparationUI.returnButton2.gameObject.SetActive (true);
                heroPreparationUI.confirmButton.gameObject.SetActive (true);
                heroPreparationUI.cancelButton.gameObject.SetActive (false);

                ToggleReady ();                

                break;

        }        

    }
    #endregion

    #region Qin
    int qinPreparationSlotsCount;

    public int QinPreparationSlotsCount {

        get { return qinPreparationSlotsCount; }

        set {

            qinPreparationSlotsCount = value;

            qinPreprationUI.preparationSlotsCount.text = value + "/" + CurrentMaxSlotsCount;            

            qinPreprationUI.continueButton.interactable = value == CurrentMaxSlotsCount;

        }

    }

    public void QinPreparationContinue () {

        PreparationPhase++;

        switch (PreparationPhase - 1) {

            case 0:

                qinPreparationSlotsCount = 0;

                foreach (SC_CastleDeck castleDeck in qinPreprationUI.castleDecks) {

                    castleDeck.Trap.gameObject.SetActive (true);

                    qinPreparationSlotsCount += castleDeck.Trap.IsDefault ? 0 : 1;                    

                }

                QinPreparationSlotsCount = qinPreparationSlotsCount;

                qinPreprationUI.castlesPool.SetActive (false);
                qinPreprationUI.trapsPool.SetActive (true);

                qinPreprationUI.returnButton.gameObject.SetActive (true);

                break;

            case 1:

                CurrentMaxSlotsCount = 1;

                qinPreprationUI.curseSlot.gameObject.SetActive (true);

                qinPreparationSlotsCount = qinPreprationUI.curseSlot.IsDefault ? 0 : 1;

                QinPreparationSlotsCount = qinPreparationSlotsCount;

                qinPreprationUI.trapsPool.SetActive (false);
                qinPreprationUI.cursesPool.SetActive (true);

                break;

            case 2:

                qinPreprationUI.cursesPool.SetActive (false);
                qinPreprationUI.soldiersPool.SetActive (true);
                qinPreprationUI.preparationSlotsCount.gameObject.SetActive (false);
                qinPreprationUI.continueButton.GetComponentInChildren<Text> ().text = "Confirm";

                break;

            case 3:

                qinPreprationUI.continueButton.gameObject.SetActive (false);
                qinPreprationUI.returnButton.gameObject.SetActive (false);
                qinPreprationUI.cancelButton.gameObject.SetActive (true);

                ToggleReady ();

                break;

        }

    }

    public void QinPreparationReturn () {

        PreparationPhase--;

        switch (PreparationPhase + 1) {

            case 1:

                qinPreparationSlotsCount = 0;

                foreach (SC_CastleDeck castleDeck in qinPreprationUI.castleDecks)
                    qinPreparationSlotsCount += castleDeck.Castle.IsDefault ? 0 : 1;

                QinPreparationSlotsCount = qinPreparationSlotsCount;

                qinPreprationUI.castlesPool.SetActive (true);
                qinPreprationUI.trapsPool.SetActive (false);

                qinPreprationUI.returnButton.gameObject.SetActive (false);

                break;

            case 2:

                CurrentMaxSlotsCount = 6;

                foreach (SC_CastleDeck castleDeck in qinPreprationUI.castleDecks)
                    qinPreparationSlotsCount += castleDeck.Trap.IsDefault ? 0 : 1;                

                QinPreparationSlotsCount = qinPreparationSlotsCount;

                qinPreprationUI.trapsPool.SetActive (true);
                qinPreprationUI.cursesPool.SetActive (false);

                break;

            case 3:

                CurrentMaxSlotsCount = 1;

                qinPreparationSlotsCount = qinPreprationUI.curseSlot.IsDefault ? 0 : 1;

                qinPreprationUI.cursesPool.SetActive (true);
                qinPreprationUI.soldiersPool.SetActive (false);
                qinPreprationUI.preparationSlotsCount.gameObject.SetActive (true);
                qinPreprationUI.continueButton.GetComponentInChildren<Text> ().text = "Continue";

                break;

            case 4:

                qinPreprationUI.continueButton.gameObject.SetActive (true);
                qinPreprationUI.returnButton.gameObject.SetActive (true);
                qinPreprationUI.cancelButton.gameObject.SetActive (false);

                ToggleReady ();

                break;

        }

    }
    #endregion
    #endregion

    #region Next Turn 
    public void NextTurn() {

        playerActionsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        soundPanel.SetActive(false);
        concedePanel.SetActive(false);
        characterDetails.panel.SetActive(false);

        backAction = DoNothing;

        SwapTurnIndicators(false);

        turnIndicator.text = (GameManager.QinTurn ? "Qin" : "Coalition") + "'s Turn";

        // usePower.SetActive (!gameManager.Qin && !SC_Player.localPlayer.Qin);        

        nextTurnUI.text.text = turnIndicator.text;

        DOTween.Sequence ().Append (nextTurnUI.panel.DOFade (1, 1)).Append (nextTurnUI.panel.DOFade (0, 1));

        DOTween.Sequence ().Append (nextTurnUI.text.DOFade (1, 1)).Append (nextTurnUI.text.DOFade (0, 1).OnComplete (GameManager.StartNextTurn));

    }   
     
    public void SwapTurnIndicators(bool b) {

        turnIndicator.transform.parent.gameObject.SetActive(b);

        nextTurnUI.panel.gameObject.SetActive(!b);

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
            HideInfos(!endSacrifice.activeSelf);

    }

	public void HideInfos(bool removeFilters) {

        if (removeFilters)
            TileManager.RemoveAllFilters(true);

        characterTooltip.panel.SetActive(false);

        tileTooltip.panel.SetActive(false);

        CurrentChara = null;

	}

    public void TryRefreshInfos(GameObject g, Type t) {

        g.GetComponent<SC_Character> ()?.UpdateStats ();

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

        if(character.Hero) {

            heroTooltip.movementCost.text = character.Hero.MovementCost(1).ToString();

            heroTooltip.movementPoints.text = character.Hero.MovementPoints + "/" + character.Movement;

        }

        heroTooltip.panel.SetActive(character.Hero);

        characterTooltip.prep.Set(character.PreparationCharge, character.Preparation, ColorMode.Default);

        characterTooltip.prep.GetComponentInChildren<Text>().text = character.PreparationCharge + " / " + character.Preparation;

        characterTooltip.anticip.Set(character.AnticipationCharge, character.Anticipation, ColorMode.Default);

        characterTooltip.anticip.GetComponentInChildren<Text>().text = character.AnticipationCharge + " / " + character.Anticipation;

        characterTooltip.prepContainer.SetActive(true);

        characterTooltip.anticipContainer.SetActive(true);

        characterTooltip.panel.SetActive(true);

        if (SC_Tile.CanChangeFilters)
            TileManager.DisplayMovementAndAttack(character, true);

    }

    #region Characters Details
    public void DisplayCharacterDetails(SC_Character c) {        

        ShowCharacterInfos (c);

        foreach (Transform t in characterDetails.stats.GetChild(0))
            if(t.gameObject.activeSelf)
                t.GetChild(1).GetComponent<Text>().text = GetStat(c, t.name);

        for (int i = 0; i < Mathf.Max (characterDetails.weapons.childCount, c.weapons.Count) ; i++) {

            if (i < c.weapons.Count) {

                if (i < characterDetails.weapons.childCount) {

                    characterDetails.weapons.GetChild (i).GetComponent<Text> ().text = c.weapons [i].weaponName;
                    characterDetails.weapons.GetChild (i).gameObject.SetActive (true);

                } else
                    Instantiate (Resources.Load<GameObject> ("Prefabs/UI/UI_Weapon"), characterDetails.weapons).GetComponent<Text> ().text = c.weapons[i].weaponName;

            } else {

                characterDetails.weapons.GetChild (i).gameObject.SetActive (false);

            }

        }

        if (c.Hero) {

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

        DisplayCharacterDetails (true);

    }

    public void ToggleRelationshipValueType (int id) {

        relationshipsDetails[id].boostValue.gameObject.SetActive(!relationshipsDetails[id].boostValue.gameObject.activeSelf);

        relationshipsDetails[id].relationValue.gameObject.SetActive(!relationshipsDetails[id].relationValue.gameObject.activeSelf);

    }

    Action previousBackAction;

    public void DisplayCharacterDetails(bool b) {

        SC_Cursor.SetLock(b);

        if(b)
            previousBackAction = backAction;

        backAction = b ? () => DisplayCharacterDetails(false) : previousBackAction;

        characterDetails.panel.SetActive(b);

        turnIndicator.transform.parent.gameObject.SetActive(!b);

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
            tileTooltip.shields.gameObject.SetActive(false);

        }

        bool movingDemon = activeCharacter?.Demon && localPlayer.Qin;

        tileTooltip.power.text = t.CombatModifiers.strength + (movingDemon ? 0 : t.DemonsModifier("strength", localPlayer.Qin)) + "";
        tileTooltip.defense.text = t.CombatModifiers.armor + (movingDemon ? 0 : t.DemonsModifier("armor", localPlayer.Qin)) + "";
        tileTooltip.preparation.text = t.CombatModifiers.preparation + (movingDemon ? 0 : t.DemonsModifier("preparation", localPlayer.Qin)) + "";
        tileTooltip.anticipation.text = t.CombatModifiers.anticipation + (movingDemon ? 0 : t.DemonsModifier("anticipation", localPlayer.Qin)) + "";
        tileTooltip.range.text = t.CombatModifiers.range + (movingDemon ? 0 : t.DemonsModifier("range", localPlayer.Qin)) + "";
        tileTooltip.movement.text = t.CombatModifiers.movement + (movingDemon ? 0 : t.DemonsModifier("movement", localPlayer.Qin)) + "";

        tileTooltip.subPanel.ReCalculate ();

        tileTooltip.panel.SetActive(true);

    }

	void ShowConstructionsInfos(SC_Construction construction) {

        tileTooltip.name.text = construction.Name;

        if (construction.GreatWall) {

            tileTooltip.shields.Set(construction.Health);

        } else if (construction.Health > 0) {

            tileTooltip.health.Set(construction.Health, construction.maxHealth);
            tileTooltip.health.gameObject.SetActive(true);

        }

        if (SC_Tile.CanChangeFilters && construction.DrainingStele) {

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

        heroTooltip.panel.SetActive(false);

        characterTooltip.prepContainer.SetActive(false);

        characterTooltip.anticipContainer.SetActive(false);

        characterTooltip.panel.SetActive(true);

    }
    #endregion

    #region Fight related
    // Also called by UI
    public void SelectWeaponPreviewFight (int index) {

        activeCharacter.Hero.SetActiveWeapon (index);

        PreviewFight();

        activeCharacter.Hero.SetActiveWeapon (index);

    }

    public void PreviewFight (SC_Tile attackingFrom) {

        if (activeCharacter.Hero?.CanAttackWithWeapons(attackingFrom).Count == 1)
            SelectWeaponPreviewFight(activeCharacter.Hero.CanAttackWithWeapons(attackingFrom)[0]);
        else if (!activeCharacter.Hero)
            PreviewFight();

    }

    public void PreviewFight () {

        if (StaminaCost != EStaminaCost.TooHigh) {

            SC_Construction attackerConstru =  activeCharacter.CombatTile.AttackableContru;

            attackerPreviewFight.name.text = activeCharacter.characterName + (attackerConstru ? " on " + (attackerConstru as SC_Castle ? "Castle" : attackerConstru.Name) : "");

            SC_Character attacked = SC_Cursor.Tile.Character;
            SC_Construction attackedConstru = SC_Cursor.Tile.AttackableContru;

            attackedPreviewFight.prep.gameObject.SetActive(attacked);

            attackedPreviewFight.anticip.gameObject.SetActive(attacked && !attackedConstru);

            if (SC_Cursor.Tile.Qin) {

                attackedPreviewFight.name.text = "Qin";

                attackedPreviewFight.health.Set(0, SC_Qin.Energy, SC_Qin.Energy);

                attackedPreviewFight.shields.Set(0);

                NonCharacterAttackPreview();

            } else if (attacked) {

                attackedPreviewFight.name.text = attacked.characterName + (attackedConstru ? " on " + (attackedConstru as SC_Castle ? "Castle" : attackedConstru.Name) : "");

                PreviewCharacterAttack(attacked, activeCharacter, PreviewCharacterAttack(activeCharacter, attacked) || !attacked.GetActiveWeapon().Range(attacked).In(SC_Fight_Manager.AttackRange));

            } else {

                attackedPreviewFight.name.text = attackedConstru.Name;

                attackedPreviewFight.health.Set(fightManager.CalcDamage(activeCharacter, attackedConstru), attackedConstru.Health, attackedConstru.maxHealth);

                attackedPreviewFight.health.NewGauge.gameObject.SetActive(!attackedConstru.GreatWall);
                attackedPreviewFight.health.PrevGauge.gameObject.SetActive(!attackedConstru.GreatWall);

                attackedPreviewFight.shields.Set(attackedConstru.GreatWall?.Health ?? 0, true);
                    
                NonCharacterAttackPreview();

            }

            previewFightPanel.SetActive(true);

        }

    }

    void NonCharacterAttackPreview() {

        attackerPreviewFight.health.Set(activeCharacter.Health, activeCharacter.Health, activeCharacter.MaxHealth);

        attackerPreviewFight.shields.Set(activeCharacter.Tile.GreatWall?.Health ?? 0);

        attackerPreviewFight.prep.Set(activeCharacter.PreparationCharge, Mathf.Min (activeCharacter.PreparationCharge + 1, activeCharacter.Preparation), activeCharacter.Preparation);

        attackerPreviewFight.anticip.Set(activeCharacter.AnticipationCharge, activeCharacter.AnticipationCharge, activeCharacter.Anticipation);
        
    }

    bool PreviewCharacterAttack(SC_Character attacker, SC_Character attacked, bool cantHit = false) {

        bool attackedKilled = false;

        SC_Construction c = attacked.CombatTile.AttackableContru;

        PreviewFightValues attackedPF = attacker != activeCharacter ? attackerPreviewFight : attackedPreviewFight;

        int healthLeft = attacked.Health - (cantHit ? 0 : fightManager.CalcDamage(attacker, attacked));
            
        attackedKilled = healthLeft <= 0;

        attackedPF.health.Set(c ? attacked.Health : Mathf.Max(0, healthLeft), attacked.Health, attacked.MaxHealth);

        if(c)
            attackedPF.health.Values.text = (cantHit ? "" : fightManager.CalcDamage(attacker, c) + " <= ") + c.Health;

        attackedPF.shields.Set(c?.Health ?? 0, !cantHit);  

        attackedPF.anticip.Set(attacked.AnticipationCharge, c ? attacked.AnticipationCharge : Mathf.Min(attacked.AnticipationCharge + (cantHit || attackedKilled ? 0 : 1), attacked.Anticipation), attacked.Anticipation, attacked.Anticiping && !c);

        attackedPF.prep.Set(attacked.PreparationCharge, Mathf.Min(attacked.PreparationCharge + (attackedKilled || !attacked.GetActiveWeapon().Range(attacked).In(SC_Fight_Manager.AttackRange) ? 0 : 1), attacked.Preparation), attacked.Preparation, attacked.Prepared);

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
    public void ChooseWeapon () {

        foreach (Transform t in weaponChoicePanel.transform) {

            t.gameObject.SetActive (false);

            Destroy (t.gameObject);

        }

        foreach (SC_Weapon w in activeCharacter.weapons) {

            if (w.Range (activeCharacter).In (SC_Fight_Manager.AttackRange)) {

                GameObject weaponChoice = Instantiate (Resources.Load<GameObject> ("Prefabs/UI/WeaponChoice"), weaponChoicePanel.transform);

                weaponChoice.GetComponentInChildren<Text> ().text = w.weaponName;

                weaponChoice.GetComponent<Button> ().onClick.AddListener (() => { GameManager.ActiveCharacterAttack (w.Index); });        

                weaponChoice.GetComponent<EventTrigger> ().triggers.Add (CreateEventTriggerEntry (EventTriggerType.PointerEnter, (eventData) => { SelectWeaponPreviewFight (w.Index); }));

                weaponChoice.GetComponent<EventTrigger> ().triggers.Add (CreateEventTriggerEntry (EventTriggerType.PointerExit, (eventData) => { HidePreviewFight (); }));

            }

        }

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
        previewFightPanel.SetActive(false);

    }
    #endregion

    #region Stamina system
    public void DisplayStaminaActionCost (bool show) {

        activeCharacter.Hero?.SetStaminaCost(show ? activeCharacter.Hero.ActionCost : 0);        

    }

    public void DisplayStaminaCost (int cost) {

        if (CurrentChara == activeCharacter.gameObject)
            staminaUsage[0].SetStaminaCost(cost);

        staminaUsage[1].SetStaminaCost(cost);

    }

    public void TryDoAction(UnityAction action) {

        if (StaminaCost == EStaminaCost.WillDie) {         

            warningStaminaDeathPanel.yes.onClick.RemoveAllListeners();

            warningStaminaDeathPanel.yes.onClick.AddListener(() => { warningStaminaDeathPanel.panel.SetActive(false); } );

            warningStaminaDeathPanel.yes.onClick.AddListener(action);

            warningStaminaDeathPanel.no.onClick.RemoveAllListeners();

            bool b = !SC_Cursor.Instance.Locked;

            warningStaminaDeathPanel.no.onClick.AddListener(() => HideStaminaWarningPanel(b));

            warningStaminaDeathPanel.panel.SetActive(true);

            if (previewFightPanel.activeSelf)
                localPlayer.CmdSetChainAttack(true);

            SC_Cursor.SetLock(true);

        } else {

            action();

        }

    }

    public void HideStaminaWarningPanel(bool unlock) {

        localPlayer.CmdSetChainAttack(false);

        SC_Cursor.SetLock(!unlock);

        warningStaminaDeathPanel.panel.SetActive(false);
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

        foreach (KeyValuePair<GameObject, Vector3> entry in staticUIs)
            entry.Key.transform.position = entry.Value;

        if (Input.GetButtonDown("Cancel"))
            backAction ();
        else if (Input.GetButtonDown("DisplayDetails")) {

            if (CurrentChara?.GetComponent<SC_Character>() && !SC_Cursor.Instance.Locked)
                DisplayCharacterDetails (CurrentChara.GetComponent<SC_Character>());
            else if (characterDetails.panel.activeSelf)
                backAction ();

        }

        if (victoryPanel.activeSelf && Input.anyKeyDown) {

            StopCoroutine (ForceMainMenuReturn ());

            LobbyManager.s_Singleton.StopClientClbk ();

        }

        if (Cursor.visible)
            SetMenuTransparencyAt (Camera.main.ScreenToWorldPoint (Input.mousePosition), false);

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

        SetMenuTransparency (menu, false);

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

        TryDoAction(() => {

            DisplayStaminaActionCost(false);

            SC_Cursor.SetLock(false);

            characterActionsPanel.SetActive(false);

            backAction = CancelAction;

            TileManager.CheckAttack();

        });

    }

    public void Back () {

        backAction();

    }

    void CancelAction() {

        DisplayStaminaActionCost(false);

        constructPanel.SetActive(false);

        TileManager.RemoveAllFilters();

        ActivateMenu(characterActionsPanel);

        backAction = GameManager.ResetMovement;

        TileManager.PreviewAttack();

        previewFightPanel.SetActive(false);

    }    
    #endregion

    #region Utility   
    public bool IsFullScreenMenuOn { get { return characterDetails.panel.activeSelf; } }

    public GraphicRaycaster GR { get { return GetComponent<GraphicRaycaster> (); } }

    public void FocusOn (Vector3 pos) {

        if (IsFullScreenMenuOn) {

            backAction = DoNothing;

            DisplayCharacterDetails (false);

        }

    }

    public void SetMenuTransparencyAt (Vector3 pos, bool transparent) {

        PointerEventData pED = new PointerEventData (EventSystem.current) { position = WorldMousePos };

        List<RaycastResult> results = new List<RaycastResult> ();

        GR.Raycast (pED, results);

        foreach (RaycastResult r in results)
            SetMenuTransparency (r.gameObject, transparent);

    }

    void SetMenuTransparency (GameObject menu, bool transparent) {

        if (transparencyFocusMenus.Contains (menu))
            foreach (MaskableGraphic g in menu.GetComponentsInChildren<MaskableGraphic> ())
                g.DOFade (transparent ? transparency : 1, 0);

    }

    IEnumerator ClickSafety (Action a) {

        yield return new WaitForSeconds(clickSecurityDuration);

        a();

    }

    EventTrigger.Entry CreateEventTriggerEntry (EventTriggerType type, UnityAction <BaseEventData> action) {

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener (action);
        return entry;

    }
    #endregion

}
