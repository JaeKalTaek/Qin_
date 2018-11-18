using System.Collections.Generic;
using UnityEngine;
using static SC_Global;

public class SC_Fight_Manager : MonoBehaviour {

    public int AttackRange { get; set; }

    SC_UI_Manager uiManager;

    public SC_Tile_Manager TileManager { get; set; }

    SC_Common_Characters_Variables CharactersVariables { get { return SC_Game_Manager.Instance.CommonCharactersVariables; } }

    public static SC_Fight_Manager Instance;

    void Awake () {

        Instance = this;

    }

    void Start () {

        uiManager = SC_UI_Manager.Instance;

    }

    public void Attack () {

        SC_Player.localPlayer.Busy = false;

        uiManager.HideWeapons();

        uiManager.cancelAction = DoNothing;

        SC_Character attacker = SC_Character.attackingCharacter;

        attacker.Tire();

        SC_Character attacked = attacker.AttackTarget.Character;
        SC_Construction targetConstruction = attacker.AttackTarget.Construction;
        SC_Construction currentConstruction = attacker.Tile.Construction;

        if (attacked) {

            bool killed = CharacterAttack(attacker, attacked, targetConstruction, false);

            if (!killed && attacked.GetActiveWeapon().Range(attacked).In(AttackRange))
                CharacterAttack(attacked, attacker, currentConstruction, true);

        } else if (targetConstruction) {

            HitConstruction(attacker, targetConstruction, false);

        } else if (attacker.AttackTarget.Qin) {

            SC_Qin.ChangeEnergy(-attacker.BaseDamage);

        }        

        SC_Character.Wait();

    }

    bool CharacterAttack(SC_Character attacker, SC_Character attacked, SC_Construction attackedConstru, bool counter) {

        bool killed = false;

        if (attacked.Tile.GreatWall)
            HitConstruction(attacker, attackedConstru, counter);
        else
            killed = attacked.Hit(CalcDamages(attacker, attacked, counter), false);

        attacker.CriticalAmount = (attacker.CriticalAmount >= CharactersVariables.critTrigger) ? 0 : Mathf.Min((attacker.CriticalAmount + attacker.Technique), CharactersVariables.critTrigger);

        if (!attacked.Tile.GreatWall)
            attacked.DodgeAmount = (attacked.DodgeAmount >= CharactersVariables.dodgeTrigger) ? 0 : Mathf.Min((attacked.DodgeAmount + attacked.Reflexes), CharactersVariables.dodgeTrigger);

        if (attacker.Hero && killed)
            IncreaseRelationships(attacker.Hero);

        uiManager.TryRefreshInfos(attacker.gameObject, attacker.GetType());

        if(!killed)
            uiManager.TryRefreshInfos(attacked.gameObject, attacked.GetType());

        return killed;

    }

    void HitConstruction(SC_Character attacker, SC_Construction construction, bool counter) {

        construction.Health -= Mathf.CeilToInt(attacker.BaseDamage / (counter ? CharactersVariables.counterFactor : 1));

        construction.Lifebar.UpdateGraph(construction.Health, construction.maxHealth);

        if (construction.Health <= 0)
            construction.DestroyConstruction();
        else
            uiManager.TryRefreshInfos(construction.gameObject, construction.GetType());

    }

    public int CalcDamages (SC_Character attacker, SC_Character attacked, bool counter) {

        int damages = attacker.BaseDamage;

        //damages = Mathf.CeilToInt(damages * attacker.GetActiveWeapon().ShiFuMiModifier(attacked.GetActiveWeapon()));

        if (attacker.Hero)
            damages += Mathf.CeilToInt(damages * RelationBoost(attacker.Hero));

        if (attacker.Hero && attacked.Hero && attacked.Qin)
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

        if (counter)
            damages = Mathf.CeilToInt(damages / CharactersVariables.counterFactor);

        return Mathf.Max(0, damages);

    }

    float RelationMalus (SC_Hero target, SC_Hero opponent) {

        int value;
        target.Relationships.TryGetValue(opponent.characterName, out value);

        return 1 - RelationValue(value);

    }

    float RelationBoost (SC_Hero target) {

        float boost = 0;

        foreach (SC_Hero hero in TileManager.HeroesInRange(target)) {

            int value;
            target.Relationships.TryGetValue(hero.characterName, out value);

            boost += RelationValue(value);

        }

        return boost;

    }

    float RelationValue (int value) {

        float v = 0;

        for (int i = 0; i < CharactersVariables.relationBoostValues.relations.Length; i++)
            if (value >= CharactersVariables.relationBoostValues.relations[i])
                v = CharactersVariables.relationBoostValues.values[i];

        return v;

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
