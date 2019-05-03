using UnityEngine;
using static SC_Global;
using static SC_Character;
using System.Collections;

public class SC_Fight_Manager : MonoBehaviour {

    public int AttackRange { get; set; }

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

        yield return new WaitForSeconds(fightDelay);

        uiManager.combatFeedbackText.gameObject.SetActive(false);
        #endregion        

        #region If the current character is attacking
        if (attacking) {           

            float baseValue = attackedConstru?.Health ?? attacked?.Health ?? SC_Qin.Energy;

            float endValue = Mathf.Max(0, (attacked && !attackedConstru) ? attacked.Health - CalcDamage(c, attacked) : (attackedConstru?.Health ?? SC_Qin.Energy) - CalcAttack(c));

            #region Text Feedback
            string feedbackText = "";

            if (c.CriticalAmount >= SC_Game_Manager.Instance.CommonCharactersVariables.critTrigger)
                feedbackText += "Crit!";

            if (baseValue == endValue)
                feedbackText += ((feedbackText != "" ? "\n" : "") + "No Damage!");

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

                // print(attackedConstru?.maxHealth ?? attacked?.MaxHealth ?? SC_Qin.Qin.energyToWin);

                (counter ? uiManager.fightPanel.attackerHealth : uiManager.fightPanel.attackedHealth).text = Mathf.RoundToInt(HealthValue).ToString();               

                yield return new WaitForEndOfFrame();

            }

            // print("Killed : " + (endValue <= 0));

            StartCoroutine(FightAnim(c, -travel, false, endValue <= 0));
            #endregion

        #region Else, the current character has finished his return
        } else {

            #region Applying damage
            if (SC_Player.localPlayer.isServer)
                SC_Player.localPlayer.CmdApplyDamage(counter);

            /*if (attacked)
                CharacterAttack(c, attacked);
            else if (attackedConstru)
                HitConstruction(c, attackedConstru);
            else
                SC_Qin.ChangeEnergy(-c.BaseDamage);*/
            #endregion

            // Augment attacker's crit amount
            // c.CriticalAmount = (c.CriticalAmount >= CharactersVariables.critTrigger) ? 0 : Mathf.Min((c.CriticalAmount + c.Technique), CharactersVariables.critTrigger);

            #region Counter attack
            if (attacked && !counter && attacked.GetActiveWeapon().Range(attacked).In(AttackRange) && !killed) {

                StartCoroutine(FightAnim(attacked, travel, true));

            } else {

                uiManager.fightPanel.panel.SetActive(false);

                if (SC_Player.localPlayer.isServer)
                    SC_Player.localPlayer.CmdFinishAction();

                //SC_Game_Manager.Instance.FinishAction();

            }
            #endregion

        }
        #endregion

    }

    public void CharacterAttack(SC_Character attacker, SC_Character attacked) {

        bool killed = false;

        if (attacked.Tile.AttackableContru)
            killed = HitConstruction(attacker, attacked.Tile.AttackableContru);
        else {

            killed = attacked.Hit(CalcDamage(attacker, attacked));            
            attacked.DodgeAmount = (attacked.DodgeAmount >= CharactersVariables.dodgeTrigger) ? 0 : Mathf.Min((attacked.DodgeAmount + attacked.Reflexes), CharactersVariables.dodgeTrigger);

        }

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

    public bool HitConstruction(SC_Character attacker, SC_Construction construction) {

        construction.Health -= Mathf.RoundToInt(CalcAttack(attacker));

        construction.Lifebar.UpdateGraph(construction.Health, construction.maxHealth);

        if (construction.Health <= 0) {

            construction.DestroyConstruction(true);
            return true;

        } else
            return false;
            // uiManager.TryRefreshInfos(construction.gameObject, construction.GetType());          
    
    }

    public int CalcAttack(SC_Character attacker) {

        int damages = attacker.GetActiveWeapon().physical ? attacker.Strength : attacker.Chi;

        //damages = Mathf.CeilToInt(damages * attacker.GetActiveWeapon().ShiFuMiModifier(attacked.GetActiveWeapon()));

        if (attacker.Hero)
            damages += Mathf.RoundToInt(damages * RelationBoost(attacker.Hero));

        /*if (attacker.Hero && attacked.Hero)
            damages = Mathf.CeilToInt(damages * RelationMalus(attacker.Hero, attacked.Hero));*/

        if (attacker.CriticalAmount == CharactersVariables.critTrigger)
            damages = Mathf.RoundToInt(damages * CharactersVariables.critMultiplier);

        /*if (attacker.Hero?.Berserk ?? false)
            damages = Mathf.CeilToInt(damages * CharactersVariables.berserkDamageMultiplier);*/

        if (attacker != activeCharacter)
            damages = Mathf.RoundToInt(damages / CharactersVariables.counterFactor);

        return Mathf.Max(0, damages);

    }

    public int CalcDamage (SC_Character attacker, SC_Character attacked) {

        int damages = CalcAttack(attacker);

        if (attacked.DodgeAmount == CharactersVariables.dodgeTrigger)
            damages = Mathf.RoundToInt(damages * ((100 - CharactersVariables.dodgeReductionPercentage) / 100f));

        int armor = attacked.Armor;
        int resistance = attacked.Resistance;

        if (attacked.Hero) {

            float relationBoost = RelationBoost(attacked.Hero);
            armor += Mathf.RoundToInt(armor * relationBoost);
            resistance += Mathf.RoundToInt(resistance * relationBoost);

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

        // print("Attacker : " + attacker?.characterName + "\nAttacked : " + (attacked?.characterName ?? attackedConstru?.name ?? "Qin"));

        if (attacked)
            CharacterAttack(attacker, attacked);
        else if (attackedConstru)
            HitConstruction(attacker, attackedConstru);
        else
            SC_Qin.ChangeEnergy(-CalcAttack(attacker));

        attacker.CriticalAmount = (attacker.CriticalAmount >= CharactersVariables.critTrigger) ? 0 : Mathf.Min((attacker.CriticalAmount + attacker.Technique), CharactersVariables.critTrigger);

    }

}
