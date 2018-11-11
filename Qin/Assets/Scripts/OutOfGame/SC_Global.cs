using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SC_Global {

    public enum TDisplay { None = -1, Attack = 0, Sacrifice = 1, Construct = 2, Movement = 3 }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    public enum Actions { Attack, Inventory, Wait, Build, Sacrifice, Destroy, EndTurn, Concede, Options, Cancel }

    public static Vector3 WorldMousePos { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }

    [Serializable]
    public struct SC_CombatModifiers {

        [Header("Combat Modifiers")]
        [Tooltip("Strength Modifier")]
        public int strength;

        [Tooltip("Chi Modifier")]
        public int chi;

        [Tooltip("Armor Modifier")]
        public int armor;

        [Tooltip("resistance Modifier")]
        public int resistance;

        [Tooltip("Technique Modifier")]
        public int technique;

        [Tooltip("Reflexes Modifier")]
        public int reflexes;

        [Tooltip("Range Modifier")]
        public int range;

        [Tooltip("Movement Modifier")]
        public int movement;

    }

    public static List<Actions> ActionsUpdate(Actions action, List<Actions> actions, bool add)
    {
        if(add)
            if(!actions.Contains(action))
                actions.Add(action);
        else
            if (actions.Contains(action))
                actions.Remove(action);

        return actions;
    }

    public struct TileInfos {

        public string type;

        public int sprite;

        public int riverSprite;

        public int region;

        public bool[] borders;

        public TileInfos (string t, int s, int rS, int r, bool[] b) {

            type = t;

            sprite = s;

            riverSprite = rS;

            region = r;

            borders = b;

        }

    }

    public struct DemonAura {

        public string demon;

        public SC_CombatModifiers aura;

        public DemonAura (string d, SC_CombatModifiers a) {

            demon = d;

            aura = a;

        }

    }

    [Serializable]
    public class CharacterTooltip {

        public GameObject panel;

        public Image icon;

        public Text name;

        public Slider health, crit, dodge;

    }

    [Serializable]
    public class CharacterFightPreview {

        public Text name, constructionName;

        public SC_FightValue health, constructionHealth, crit, dodge;

    }

}
