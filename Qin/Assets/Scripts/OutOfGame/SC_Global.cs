using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class SC_Global {

    public enum TDisplay { None = -1, Attack = 0, Sacrifice = 1, Construct = 2, Deploy = 3, Movement = 4 }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    public static Vector3 WorldMousePos { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }

    public static int XSize { get { return SC_Game_Manager.Instance.CurrentMapPrefab.SizeMapX; } }

    public static int YSize { get { return SC_Game_Manager.Instance.CurrentMapPrefab.SizeMapY; } }

    public static int Size { get { return XSize + YSize; } }

    public static Vector2 UISize { get { return SC_UI_Manager.Instance.GetComponent<RectTransform> ().sizeDelta; } }

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

        [Tooltip("Preparation Modifier")]
        public int preparation;

        [Tooltip("Anticipation Modifier")]
        public int anticipation;

        [Tooltip("Range Modifier")]
        public int range;

        [Tooltip("Movement Modifier")]
        public int movement;

    }

    public static void DoNothing() { }

    public struct TileInfos {

        public string type;

        public bool heroDeploy;

        public int sprite;

        public int riverSprite;

        public int region;

        public bool[] borders;

        public TileInfos(string t, bool d, int s, int rS, int r, bool[] b) {

            type = t;

            heroDeploy = d;

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

        public GameObject panel, prepContainer, anticipContainer;

        public Image icon;

        public Text name, healthLabel;

        public Slider health, prep, anticip;

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

        public SC_FightValue health, prep, anticip;

        public SC_ShieldBar shields;

    }

    [Serializable]
    public class TileTooltip {

        public GameObject panel;

        public SC_Scroll_Menu subPanel;

        public Slider health;

        public SC_ShieldBar shields;

        public Text name, power, defense, preparation, anticipation, range, movement;

    }

    [Serializable]
    public class FightPanel {

        public GameObject panel;

        public Slider attackerSlider, attackedSlider;

        public SC_ShieldBar attackerShield, attackedShield;

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

    [Serializable]
    public struct HeroesPreparationUI {

        public GameObject panel, decks, pool;

        public List<SC_HeroDeck> heroDecks;

        public GameObject heroesPool, weaponsPool, trapsPool;

        public TextMeshProUGUI preparationSlotsCount;

        public Button continueButton, returnButton, returnButton2, confirmButton, cancelButton;

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

        public int maxHealth, strength, chi, armor, resistance, preparation, anticipation, movement;

        public BaseCharacterStats (BaseCharacterStats other) {

            maxHealth = other.maxHealth;
            strength = other.strength;
            chi = other.chi;
            armor = other.armor;
            resistance = other.resistance;
            preparation = other.preparation;
            anticipation = other.anticipation;
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

    public enum EPreparationElement { Hero, Weapon, Trap, Deployment, Confirmation }

}
