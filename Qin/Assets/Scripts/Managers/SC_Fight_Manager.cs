using System.Collections.Generic;
using UnityEngine;
using static SC_Global;
using static SC_Character;
using System.Collections;
using UnityEngine.UI;

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

        // Show Fight Panel
        uiManager.HideWeapons();

        uiManager.cancelAction = DoNothing;        

        SC_Character attacked = attackingCharacter.AttackTarget.Character;
        SC_Construction targetConstruction = attackingCharacter.AttackTarget.Construction;

        uiManager.fightPanel.attackerName.text = attackingCharacter.characterName;
        uiManager.fightPanel.attackedName.text = targetConstruction?.Name ?? attacked?.characterName ?? "Qin";

        int currentAttackedHealth = targetConstruction?.Health ?? attacked?.Health ?? SC_Qin.Energy;

        uiManager.fightPanel.attackerHealth.text = attackingCharacter.Health.ToString();
        uiManager.fightPanel.attackedHealth.text = currentAttackedHealth.ToString();

        uiManager.fightPanel.attackerSlider.Set(attackingCharacter.Tile.Construction?.Health ?? attackingCharacter.Health, attackingCharacter.Tile.Construction?.maxHealth ?? attackingCharacter.MaxHealth);
        uiManager.fightPanel.attackedSlider.Set(currentAttackedHealth, targetConstruction?.maxHealth ?? attacked?.MaxHealth ?? SC_Qin.Qin.energyToWin);

        float y = Mathf.Min(attackingCharacter.transform.position.y, attackingCharacter.AttackTarget.transform.position.y);
        float x = Mathf.Lerp(attackingCharacter.transform.position.x, attackingCharacter.AttackTarget.transform.position.x, .5f);        

        uiManager.fightPanel.panel.transform.position = new Vector3(x, y, 0);

        uiManager.fightPanel.panel.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, uiManager.fightPanel.panel.GetComponent<RectTransform>().sizeDelta.y);

        uiManager.fightPanel.panel.SetActive(true);

        Vector3 distance = attackingCharacter.AttackTarget.transform.position - attackingCharacter.transform.position;

        Vector3 pos = Vector3.zero;

        if (distance.x >= 0) {

            if (distance.y >= 0) {

                if (distance.x >= distance.y)
                    pos = Vector3.right;
                else
                    pos = Vector3.up;

            } else {

                if (distance.x >= -distance.y)
                    pos = Vector3.right;
                else
                    pos = Vector3.down;

            }

        } else {

            if (distance.y >= 0) {

                if (-distance.x >= distance.y)
                    pos = Vector3.left;
                else
                    pos = Vector3.up;

            } else {

                if (-distance.x >= -distance.y)
                    pos = Vector3.left;
                else
                    pos = Vector3.down;

            }

        }

        // Attacking character attacks
        StartCoroutine(FightAnim(attackingCharacter, pos * .5f * SC_Game_Manager.Instance.CurrentMapPrefab.TileSize, true));        

    }

    IEnumerator FightAnim(SC_Character c, Vector3 travel, bool attacking, bool killed = false) {

        #region Character who is currently attacking moves
        Vector3 basePos = c.transform.position;

        yield return new WaitForSeconds(fightDelay);

        float timer = 0;

        while (timer < animTime) {

            timer += Time.deltaTime;

            c.transform.position = Vector3.Lerp(basePos, basePos + travel, Mathf.Min(timer, animTime) / animTime);

            yield return new WaitForEndOfFrame();

        }

        yield return new WaitForSeconds(fightDelay);
        #endregion

        bool counter = attackingCharacter != c;

        SC_Character attacked = counter ? attackingCharacter : attackingCharacter.AttackTarget.Character;

        #region If the current character is attacking
        if (attacking) {

            SC_Construction attackedConstru = counter ? attackingCharacter.Tile.Construction : attackingCharacter.AttackTarget.Construction;

            float baseValue = attackedConstru?.Health ?? attacked?.Health ?? SC_Qin.Energy; 

            float endValue = 0;

            if (attacked) {

                CharacterAttack(c, attacked);

                endValue = attackedConstru?.Health ?? attacked.Health;

            } else if (attackedConstru) {

                HitConstruction(c, attackedConstru);

                endValue = attackedConstru.Health;

            } else {

                SC_Qin.ChangeEnergy(-c.BaseDamage);

                endValue = SC_Qin.Energy;

            }

            timer = 0;

            while (timer < healthBarAnimTime) {

                timer += Time.deltaTime;

                (counter ? uiManager.fightPanel.attackerSlider : uiManager.fightPanel.attackedSlider).Set(Mathf.Lerp(baseValue, endValue, Mathf.Min(timer, healthBarAnimTime) / healthBarAnimTime), attackedConstru?.maxHealth ?? attacked?.MaxHealth ?? SC_Qin.Qin.energyToWin);

                yield return new WaitForEndOfFrame();

            }

            StartCoroutine(FightAnim(c, -travel, false, endValue <= 0));
            #endregion

        } else {

            #region Counter attack, don't counter counter attack
            if ((attacked != null) && !counter && attacked.GetActiveWeapon().Range(attacked).In(AttackRange) && !killed) {

                StartCoroutine(FightAnim(attacked, travel, true));

            } else {

                SC_Player.localPlayer.Busy = false;

                uiManager.fightPanel.panel.SetActive(false);

                FinishCharacterAction();

                SC_Cursor.SetLock(false);

            }
            #endregion

        }

    }

    bool CharacterAttack(SC_Character attacker, SC_Character attacked) {

        bool killed = false;

        if (attacked.Tile.GreatWall)
            killed = HitConstruction(attacker, attacked.Tile.Construction);
        else {

            killed = attacked.Hit(CalcDamage(attacker, attacked), false);
            attacker.CriticalAmount = (attacker.CriticalAmount >= CharactersVariables.critTrigger) ? 0 : Mathf.Min((attacker.CriticalAmount + attacker.Technique), CharactersVariables.critTrigger);
            attacked.DodgeAmount = (attacked.DodgeAmount >= CharactersVariables.dodgeTrigger) ? 0 : Mathf.Min((attacked.DodgeAmount + attacked.Reflexes), CharactersVariables.dodgeTrigger);

        }

        if (attacker.Hero && killed)
            IncreaseRelationships(attacker.Hero);

        uiManager.TryRefreshInfos(attacker.gameObject, attacker.GetType());

        if(!killed)
            uiManager.TryRefreshInfos(attacked.gameObject, attacked.GetType());

        return killed;

    }

    bool HitConstruction(SC_Character attacker, SC_Construction construction) {

        construction.Health -= Mathf.CeilToInt(attacker.BaseDamage / (attacker != attackingCharacter ? CharactersVariables.counterFactor : 1));

        construction.Lifebar.UpdateGraph(construction.Health, construction.maxHealth);

        if (construction.Health <= 0) {

            construction.DestroyConstruction();
            return true;

        } else
            return false;
            // uiManager.TryRefreshInfos(construction.gameObject, construction.GetType());          
    
    }

    public int CalcDamage (SC_Character attacker, SC_Character attacked) {

        int damages = attacker.BaseDamage;

        //damages = Mathf.CeilToInt(damages * attacker.GetActiveWeapon().ShiFuMiModifier(attacked.GetActiveWeapon()));

        if (attacker.Hero)
            damages += Mathf.CeilToInt(damages * RelationBoost(attacker.Hero));

        if (attacker.Hero && attacked.Hero)
            damages = Mathf.CeilToInt(damages * RelationMalus(attacker.Hero, attacked.Hero));

        if (attacker.CriticalAmount == CharactersVariables.critTrigger)
            damages = Mathf.CeilToInt(damages * CharactersVariables.critMultiplier);

        if (attacker.Hero?.Berserk ?? false)
            damages = Mathf.CeilToInt(damages * CharactersVariables.berserkDamageMultiplier);

        if (attacked.DodgeAmount == CharactersVariables.dodgeTrigger)
            damages = Mathf.FloorToInt(damages * ((100 - CharactersVariables.dodgeReductionPercentage) / 100));

        int armor = attacked.Armor;
        int resistance = attacked.Resistance;

        if (attacked.Hero) {

            float relationBoost = RelationBoost(attacked.Hero);
            armor += Mathf.CeilToInt(armor * relationBoost);
            resistance += Mathf.CeilToInt(resistance * relationBoost);

        }

        damages -= (attacker.GetActiveWeapon().physical) ? armor : resistance;

        if (attacker != attackingCharacter)
            damages = Mathf.CeilToInt(damages / CharactersVariables.counterFactor);

        return Mathf.Max(0, damages);

    }

    float RelationMalus (SC_Hero target, SC_Hero opponent) {

        int value;
        target.Relationships.TryGetValue(opponent.characterName, out value);

        return 1 - CharactersVariables.relationValues.GetValue("boost", value);

    }

    float RelationBoost (SC_Hero target) {

        float boost = 0;

        foreach (SC_Hero hero in TileManager.HeroesInRange(target)) {

            int value;
            target.Relationships.TryGetValue(hero.characterName, out value);

            boost += CharactersVariables.relationValues.GetValue("boost", value);

        }

        return boost;

    }

    public SC_Hero CheckHeroSaved (SC_Hero toSave, bool alreadySaved) {

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

    }

    void IncreaseRelationships (SC_Hero killer) {

        List<SC_Hero> heroesInRange = TileManager.HeroesInRange(killer);

        foreach (SC_Hero hero in heroesInRange) {

            killer.Relationships[hero.characterName] += Mathf.CeilToInt(CharactersVariables.killRelationValue / heroesInRange.Count);
            hero.Relationships[killer.characterName] += Mathf.CeilToInt(CharactersVariables.killRelationValue / heroesInRange.Count);

        }

    }    

}
