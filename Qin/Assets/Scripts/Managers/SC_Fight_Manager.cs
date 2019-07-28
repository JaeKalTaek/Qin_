using UnityEngine;
using static SC_Global;
using static SC_Character;
using System.Collections;

public class SC_Fight_Manager : MonoBehaviour {

    public static int AttackRange { get { return SC_Tile_Manager.TileDistance(SC_Arrow.path?[SC_Arrow.path.Count - 1] ?? activeCharacter.Tile, activeCharacter.AttackTarget ?? SC_Cursor.Tile); } }

    SC_UI_Manager uiManager;

    public SC_Tile_Manager TileManager { get; set; }

    SC_Common_Characters_Variables CharactersVariables { get { return SC_Game_Manager.Instance.CommonCharactersVariables; } }

    public static SC_Fight_Manager Instance;

    [Header("Fight Animation Variables")]
    [Tooltip("Cooldown between fight panel appearing and first attack anim")]
    public float fightDelay;

    [Tooltip("Time for the character to move towards its target")]
    public float animTime;

    [Tooltip("Time for the health bar to get to its final value")]
    public float healthBarAnimTime;

    void Awake () {

        Instance = this;

    }

    void Start () {

        uiManager = SC_UI_Manager.Instance;

    }

    public void Attack () {        

        uiManager.HideWeapons();

        if(SC_Player.localPlayer.Turn)
            uiManager.backAction = DoNothing;

        #region Setup Fight Panel
        #region Setup Values
        SC_Character attacked = activeCharacter.AttackTarget.Character;
        SC_Construction targetConstruction = activeCharacter.AttackTarget.AttackableContru;

        uiManager.fightPanel.attackerName.text = activeCharacter.Tile.AttackableContru?.Name ?? activeCharacter.characterName;
        uiManager.fightPanel.attackedName.text = targetConstruction?.Name ?? attacked?.characterName ?? "Qin";

        int currentAttackedHealth = targetConstruction?.Health ?? attacked?.Health ?? SC_Qin.Energy;

        uiManager.fightPanel.attackerHealth.text = (activeCharacter.Tile.AttackableContru?.Health ?? activeCharacter.Health).ToString();
        uiManager.fightPanel.attackedHealth.text = currentAttackedHealth.ToString();

        uiManager.fightPanel.attackerSlider.Set(activeCharacter.Tile.AttackableContru?.Health ?? activeCharacter.Health, activeCharacter.Tile.AttackableContru?.maxHealth ?? activeCharacter.MaxHealth);
        uiManager.fightPanel.attackedSlider.Set(currentAttackedHealth, targetConstruction?.maxHealth ?? attacked?.MaxHealth ?? SC_Qin.Qin.energyToWin);

        float y = Mathf.Min(activeCharacter.transform.position.y, activeCharacter.AttackTarget.transform.position.y);
        float x = Mathf.Lerp(activeCharacter.transform.position.x, activeCharacter.AttackTarget.transform.position.x, .5f);
        #endregion

        #region Setup pos
        uiManager.fightPanel.panel.transform.position = new Vector3(x, y, 0);

        uiManager.fightPanel.panel.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, uiManager.fightPanel.panel.GetComponent<RectTransform>().sizeDelta.y);
        #endregion

        uiManager.fightPanel.panel.SetActive(true);
        #endregion

        #region Setup attack direction
        Vector3 distance = activeCharacter.AttackTarget.transform.position - activeCharacter.transform.position;

        Vector3 travel = (Mathf.Abs(distance.x) >= Mathf.Abs(distance.y) ? Vector3.right * Mathf.Sign(distance.x) : Vector3.up * Mathf.Sign(distance.y));
        #endregion

        // Attacking character attacks
        StartCoroutine(FightAnim(activeCharacter, travel * .5f * SC_Game_Manager.Instance.CurrentMapPrefab.TileSize, true));        

    }

    IEnumerator FightAnim(SC_Character c, Vector3 travel, bool attacking, bool killed = false) {

        #region Setting up variables
        bool counter = activeCharacter != c;

        SC_Character attacked = counter ? activeCharacter : activeCharacter.AttackTarget.Character;

        SC_Construction attackedConstru = counter ? activeCharacter.Tile.AttackableContru : activeCharacter.AttackTarget.AttackableContru;
        #endregion

        if(attacking)
            SC_Sound_Manager.Instance.Hit(c, attacked, attackedConstru);

        #region Current character moves
        Vector3 basePos = c.transform.position;

        yield return new WaitForSeconds(fightDelay);

        float timer = 0;

        while (timer < animTime) {

            timer += Time.deltaTime;

            c.transform.position = Vector3.Lerp(basePos, basePos + travel, Mathf.Min(timer, animTime) / animTime);

            yield return new WaitForEndOfFrame();

        }

        #region Critical strike feedback
        if(attacking && SC_Player.localPlayer.Turn && (c.CriticalAmount >= SC_Game_Manager.Instance.CommonCharactersVariables.critTrigger) && attacked && (attacked.DodgeAmount < SC_Game_Manager.Instance.CommonCharactersVariables.dodgeTrigger))
            SC_Camera.Instance.StartCoroutine("Screenshake");
        #endregion

        yield return new WaitForSeconds(fightDelay);

        uiManager.combatFeedbackText.gameObject.SetActive(false);
        #endregion        

        #region If the current character is attacking
        if (attacking) {           

            float baseValue = attackedConstru?.Health ?? attacked?.Health ?? SC_Qin.Energy;

            float endValue = Mathf.Max(0, attackedConstru ? CalcDamage(c, attackedConstru) : (attacked ? attacked.Health - CalcDamage(c, attacked) : 0));

            #region Text Feedback
            string feedbackText = "";

            if (c.CriticalAmount >= SC_Game_Manager.Instance.CommonCharactersVariables.critTrigger && !attackedConstru)
                feedbackText += "Crit!";

            if (baseValue == endValue)
                feedbackText += ((feedbackText != "" ? "\n" : "") + (attacked && attacked.DodgeAmount >= SC_Game_Manager.Instance.CommonCharactersVariables.dodgeTrigger ? "Dodged!" :  "No Damage!"));

            if(feedbackText != "") {

                uiManager.combatFeedbackText.text = feedbackText;

                uiManager.combatFeedbackText.transform.position = c.transform.position + (travel.y > 0 ? Vector3.down : Vector3.up) + (Vector3.right * (travel.x / 2));

                uiManager.combatFeedbackText.gameObject.SetActive(true);

            }                
            #endregion

            timer = 0;           

            while (timer < healthBarAnimTime) {

                timer += Time.deltaTime;

                float HealthValue = Mathf.Lerp(baseValue, endValue, Mathf.Min(timer, healthBarAnimTime) / healthBarAnimTime);

                (counter ? uiManager.fightPanel.attackerSlider : uiManager.fightPanel.attackedSlider).Set(HealthValue, attackedConstru?.maxHealth ?? attacked?.MaxHealth ?? SC_Qin.Qin.energyToWin);

                (counter ? uiManager.fightPanel.attackerHealth : uiManager.fightPanel.attackedHealth).text = ((int)(HealthValue + .5f)).ToString();               

                yield return new WaitForEndOfFrame();

            }

            StartCoroutine(FightAnim(c, -travel, false, endValue <= 0));
            #endregion

        #region Else, the current character has finished his return
        } else {
            
            if (SC_Player.localPlayer.isServer)
                SC_Player.localPlayer.CmdApplyDamage(counter);

            #region Counter attack
            if (attacked && !counter && attacked.GetActiveWeapon().Range(attacked).In(AttackRange) && !killed) {

                StartCoroutine(FightAnim(attacked, travel, true));

            } else {

                uiManager.fightPanel.panel.SetActive(false);

                SC_Player.localPlayer.CmdSynchroFinishAction();

            }
            #endregion

        }
        #endregion

    }

    public void CharacterAttack(SC_Character attacker, SC_Character attacked) {

        bool killed = attacked.Hit(CalcDamage(attacker, attacked));      
        
        attacked.DodgeAmount = (attacked.DodgeAmount >= CharactersVariables.dodgeTrigger) ? 0 : Mathf.Min((attacked.DodgeAmount + attacked.Reflexes), CharactersVariables.dodgeTrigger);

        if (attacker.Hero) {

            if(killed)
                attacker.Hero.IncreaseRelationships(CharactersVariables.relationGains.kill);

            if (attacker != activeCharacter)
                attacker.Hero.IncreaseRelationships(CharactersVariables.relationGains.counterAttack);

        }

        uiManager.TryRefreshInfos(attacker.gameObject, attacker.GetType());

        if(!killed)
            uiManager.TryRefreshInfos(attacked.gameObject, attacked.GetType());

    }

    public void HitConstruction(SC_Character attacker, SC_Construction construction) {

        construction.Health = CalcDamage(attacker, construction); // CalcAttack(attacker);

        construction.Lifebar.UpdateGraph(construction.Health, construction.maxHealth);

        if (construction.Health <= 0)
            construction.DestroyConstruction(true);
    
    }

    public int CalcAttack(SC_Character attacker) {

        int damages = attacker.GetActiveWeapon().physical ? attacker.Strength : attacker.Chi;

        //damages = Mathf.CeilToInt(damages * attacker.GetActiveWeapon().ShiFuMiModifier(attacked.GetActiveWeapon()));

        if (attacker.Hero)
            damages += (int)(damages * RelationBoost(attacker.Hero) + .5f);

        /*if (attacker.Hero && attacked.Hero)
            damages = Mathf.CeilToInt(damages * RelationMalus(attacker.Hero, attacked.Hero));*/

        if (attacker.CriticalAmount == CharactersVariables.critTrigger)
            damages = (int)(damages * CharactersVariables.critMultiplier + .5f);

        /*if (attacker.Hero?.Berserk ?? false)
            damages = Mathf.CeilToInt(damages * CharactersVariables.berserkDamageMultiplier);*/

        if (attacker != activeCharacter)
            damages = (int)((damages / CharactersVariables.counterFactor) + .5f);


        return Mathf.Max(0, damages);

    }

    public int CalcDamage (SC_Character attacker, SC_Construction constructionAttacked) {

        return Mathf.Max(0, constructionAttacked.Health - (constructionAttacked.GreatWall ? 1 : CalcAttack(attacker)));

    }

    public int CalcDamage (SC_Character attacker, SC_Character attacked) {

        int damages = CalcAttack(attacker);

        if (attacked.DodgeAmount == CharactersVariables.dodgeTrigger)
            damages = (int)(damages * ((100 - CharactersVariables.dodgeReductionPercentage) / 100f) + .5f);

        int armor = attacked.Armor;
        int resistance = attacked.Resistance;

        if (attacked.Hero) {

            float relationBoost = RelationBoost(attacked.Hero);
            armor += (int)(armor * relationBoost + .5f);
            resistance += (int)(resistance * relationBoost + .5f);

        }

        damages -= (attacker.GetActiveWeapon().physical) ? armor : resistance;

        return Mathf.Max(0, damages);

    }

    /*float RelationMalus (SC_Hero target, SC_Hero opponent) {

        int value;
        target.Relationships.TryGetValue(opponent.characterName, out value);

        return 1 - (CharactersVariables.relationValues.GetValue("boost", value) / 100);

    }*/

    float RelationBoost (SC_Hero target) {

        float boost = 0;

        foreach (SC_Hero hero in TileManager.HeroesInRange(target)) {

            int value;
            target.Relationships.TryGetValue(hero.characterName, out value);

            boost += CharactersVariables.relationValues.GetValue("boost", value);

        }

        return boost / 100f;

    }

    /*public SC_Hero CheckHeroSaved (SC_Hero toSave, bool alreadySaved) {

        SC_Hero saver = null;

        if (!alreadySaved) {

            foreach (SC_Hero hero in FindObjectsOfType<SC_Hero>()) {

                if (!hero.Qin) {

                    int value = 0;
                    toSave.Relationships.TryGetValue(hero.characterName, out value);

                    int currentValue = -1;
                    if (saver)
                        toSave.Relationships.TryGetValue(saver.characterName, out currentValue);

                    if ((value >= CharactersVariables.saveTriggerRelation) && (value > currentValue))
                        saver = hero;

                }

            }

            SC_Tile nearestTile = TileManager.GetUnoccupiedNeighbor(toSave);

            if (saver && nearestTile) {

                saver.Tile.Character = null;

                saver.transform.SetPos(TileManager.GetUnoccupiedNeighbor(toSave).transform);

                saver.Tile.Character = saver;

            } else {

                saver = null;

            }

        }

        return saver;

    }*/

    public void ApplyDamage (bool counter) {

        SC_Character attacker = counter ? activeCharacter.AttackTarget.Character : activeCharacter;

        SC_Character attacked = counter ? activeCharacter : activeCharacter.AttackTarget.Character;        

        SC_Construction attackedConstru = counter ? activeCharacter.Tile.AttackableContru : activeCharacter.AttackTarget.AttackableContru;

        if (attackedConstru)
            HitConstruction(attacker, attackedConstru);
        else if (attacked)
            CharacterAttack(attacker, attacked);
        else if (activeCharacter.AttackTarget.Qin)
            SC_Qin.ChangeEnergy(-SC_Qin.Energy);
        else
            print("Attack target error");

        attacker.CriticalAmount = (attacker.CriticalAmount >= CharactersVariables.critTrigger) && !attackedConstru ? 0 : Mathf.Min((attacker.CriticalAmount + attacker.Technique), CharactersVariables.critTrigger);

    }

}
