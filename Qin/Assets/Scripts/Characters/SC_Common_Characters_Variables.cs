﻿using System;
using UnityEngine;

public class SC_Common_Characters_Variables : MonoBehaviour {

    [Header("Variables common to all characters")]
    [Tooltip("Damage is multiplied by this amount when landing a critical hit")]
    public float critMultiplier;

    [Tooltip("Percentage of damage reduced when a character dodges")]
    public float dodgeReductionPercentage;

    [Tooltip("Damage is divided by this amount if this is a counter-attack")]
    public float counterFactor;

    [Tooltip("Damage is multiplied by this amount if the character is winning the ShiFuMi")]
    public float shiFuMiAvantage;

    [Tooltip("Damage is divided by this amount if the character is losing the ShiFuMi")]
    public float shiFuMiDisavantage;

    [Header("Variables common to all heroes")]
    [Tooltip("Health regenerated by a hero at the start of a turn if he's on a village")]
    public int prodConstructionRegen;

    [Tooltip ("Maximum number of weapons that can be given among the heroes")]
    public int maxTotalWeaponsCount;

    [Header("Stamina System")]
    [Tooltip("Stamina regenerated by a hero at the start of a turn")]
    public int staminaRegen;

    [Tooltip("Stamina cost for each tile walked using stamina")]
    public int baseStaminaMovementCost;

    [Tooltip("Additional cost added for each tile walked while using stamina each staminaCostAugmentation action")]
    public int staminaMovementAdditionalCost;

    [Tooltip("Stamina cost for an additional action (attack or building destruction)")]
    public int baseStaminaActionCost;

    [Tooltip("Additional cost added for an additional action (attack or building destruction) each staminaCostAugmentation action")]
    public int staminaActionAdditionalCost;

    [Tooltip("The stamina costs of the heroes' actions are augmented after this number of actions done")]
    public int staminaCostsAugmentation;

    /*[Tooltip("Amount of health kept by a hero who was saved")]
    public int savedHealthAmount;

    [Tooltip("Amount of health under which a hero becomes berserk")]
    public int berserkTriggerHealth;

    [Tooltip("Amount of relation value required between two heroes for one to go berserk when the other dies")]
    public int berserkTriggerRelation;

    [Tooltip("Damage are multiplied by that amount when the hero is berserker")]
    public float berserkDamageMultiplier;*/

    [Header("Relations")]
    [Tooltip("Relation points gained by heroes")]
    public RelationGains relationGains;

    [Tooltip("Values linked to the relationship thresholds")]
    public RelationshipValues relationValues;

    /*[Tooltip("Amount of relation value required between two heroes for one to save the other when he's about to die")]
    public int saveTriggerRelation;*/  

    [Serializable]
    public class RelationshipValues {

        [Tooltip("Relationship threshold values")]
        public SC_Global.ThresholdValues relationships;

        public float GetValue(string id, int r) {

            return relationships.GetValue(id == "boost" ? 0 : 1, r);

        }

    }

    [Serializable]
    public class RelationGains {

        [Header("Heroes Relation Gain Values")]
        [Tooltip("Amount of relation value gained by heroes when one of them performs an action while nearby at least one other")]
        public int action;

        [Tooltip("Amount of relation value gained by heroes when one of them is nearby at least one other when the turn finishes")]
        public int finishTurn;

        [Tooltip("Amount of relation value gained by heroes when one of them performs a counter attack while nearby at least one other")]
        public int counterAttack;

        [Tooltip("Amount of relation value gained by heroes when one of them kills a unit while nearby at least one other")]
        public int kill;

    }

}
