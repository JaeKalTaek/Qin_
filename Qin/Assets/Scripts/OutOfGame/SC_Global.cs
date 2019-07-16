using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class SC_Global {

    public enum TDisplay { None = -1, Attack = 0, Sacrifice = 1, Construct = 2, Movement = 3 }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    public static Vector3 WorldMousePos { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }

    public static int XSize { get { return SC_Game_Manager.Instance.CurrentMapPrefab.SizeMapX; } }

    public static int YSize { get { return SC_Game_Manager.Instance.CurrentMapPrefab.SizeMapY; } }

    public static int Size { get { return XSize + YSize; } }

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

    public static void DoNothing() { }

    public struct TileInfos {

        public string type;

        public int sprite;

        public int riverSprite;

        public int region;

        public bool[] borders;

        public TileInfos(string t, int s, int rS, int r, bool[] b) {

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

        public DemonAura(string d, SC_CombatModifiers a) {

            demon = d;

            aura = a;

        }

    }

    public enum ColorMode { Default, Health, Param }

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
    public class RelationshipDetails {

        public Image icon;

        public Text boostValue, relationValue;

        public RectTransform link;

    }

    [Serializable]
    public class PreviewFightValues {

        public Text name;

        public SC_FightValue health, crit, dodge;

    }

    [Serializable]
    public class TileTooltip {

        public GameObject panel;

        public Slider health;

        public Text name, power, defense, technique, reflexes, range, movement;

    }

    [Serializable]
    public class FightPanel {

        public GameObject panel;

        public Slider attackerSlider, attackedSlider;

        public Text attackerName, attackedName, attackerHealth, attackedHealth;

    }

    [Serializable]
    public class CreationTooltip {

        public Text name, cost, desc;

    }

    [Serializable]
    public class CreateDemonPanel {

        public GameObject panel;

        public Text name, cost;

        public Button create;

    }

    [Serializable]
    public class SacrificeCastlePanel {

        public GameObject panel, canPanel, cantPanel;

        public Text type, buff;

        public Button yes, close;

    }

    [Serializable]
    public struct WarningStaminaDeathPanel {

        public GameObject panel;

        public Button yes, no;        

    }

    [Serializable]
    public struct HeroTooltip {

        public GameObject panel;

        public Text movementCost, movementPoints, actionCost;

    }

    [Serializable]
    public struct NextTurnUI {

        public Image panel;

        public TextMeshProUGUI text;

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

    [Serializable]
    public class BaseCharacterStats {

        public int maxHealth, strength, chi, armor, resistance, technique, reflexes, movement;

        public BaseCharacterStats (BaseCharacterStats other) {

            maxHealth = other.maxHealth;
            strength = other.strength;
            chi = other.chi;
            armor = other.armor;
            resistance = other.resistance;
            technique = other.technique;
            reflexes = other.reflexes;
            movement = other.movement;

        }

    }   
    
    [Serializable]
    public class ThresholdValues {

        public int[] thresholds;

        public Values[] values;

        [Serializable]
        public class Values {

            public float[] values;

        }

        public float GetValue (int threshold) {

            return GetValue(0, threshold);

        }

        public float GetValue (int index, int threshold) {

            float v = 0;

            for (int i = 0; i < thresholds.Length; i++)
                if (threshold >= thresholds[i])
                    v = values[index].values[i];

            return v;

        }

    }    

}
