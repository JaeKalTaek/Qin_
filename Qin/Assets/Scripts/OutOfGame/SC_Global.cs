using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public static void DoNothing () { }

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

        public GameObject panel, critContainer, dodgeContainer;

        public Image icon;

        public Text name, healthLabel;

        public Slider health, crit, dodge;

    }

    [Serializable]
    public class CharacterDetails {

        public GameObject panel, soldierPanel;

        public Transform stats, weapons, relationshipsPanel;

    }

    [Serializable]
    public class CharacterFightPreview {

        public Text name, constructionName;

        public SC_FightValue health, constructionHealth, crit, dodge;

    }

    [Serializable]
    public class TileTooltip {

        public GameObject panel;

        public Slider health;

        public Text name, power, defense, technique, reflexes, range, movement;

    }

    [Serializable]
    public class CreationTooltip {

        public Text name, cost, desc;

    }

    public static bool CanCreateConstruct(string c) {

        return (SC_Qin.GetConstruCost(c) < SC_Qin.Energy) && (SC_Tile_Manager.Instance.GetConstructableTiles(c).Count > 0);

    }

    public static bool CanCreateSoldier (string s) {

        return Resources.Load<SC_Soldier>("Prefabs/Characters/Soldiers/Basic/P_" + s).cost < SC_Qin.Energy;

    }

    public static void ForceSelect(GameObject g) {

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(g);

    }

}
