﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class SC_Global {

    public enum TDisplay { None = -1, Attack, Sacrifice, Construct, Deploy, Movement, QinCurse }

    public enum ShiFuMi { Rock, Paper, Scissors, Special }

    public static Vector3 WorldMousePos { get { return Camera.main.ScreenToWorldPoint (Input.mousePosition); } }

    public static SC_Tile TileUnderMouse { get { return SC_Tile_Manager.Instance.GetTileAt (WorldMousePos, true); } }

    public static int XSize { get { return SC_Game_Manager.Instance.mapPrefab.SizeMapX; } }

    public static int YSize { get { return SC_Game_Manager.Instance.mapPrefab.SizeMapY; } }

    public static int Size { get { return XSize + YSize; } }

    public static Vector2 UISize { get { return SC_UI_Manager.Instance.GetComponent<RectTransform> ().sizeDelta; } }

    [Serializable]
    public struct SC_CombatModifiers {

        [Header ("Combat Modifiers")]
        [Tooltip ("Strength Modifier")]
        public int strength;

        [Tooltip ("Chi Modifier")]
        public int chi;

        [Tooltip ("Armor Modifier")]
        public int armor;

        [Tooltip ("resistance Modifier")]
        public int resistance;

        [Tooltip ("Preparation Modifier")]
        public int preparation;

        [Tooltip ("Anticipation Modifier")]
        public int anticipation;

        [Tooltip ("Range Modifier")]
        public int range;

        [Tooltip ("Movement Modifier")]
        public int movement;

    }

    public static void DoNothing () { }

    public struct TileInfos {

        public string type;

        public bool heroDeploy;

        public int region;

        public bool[] borders;

        public TileInfos (string t, bool d, int r, bool[] b) {

            type = t;

            heroDeploy = d;

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

        public TrapPanel heroTrapPanel;

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
    public class CastlePanel {

        public GameObject panel;

        public Text demonName, demonCost;

        public Button createDemonButton;

    }

    [Serializable]
    public class SacrificeCastlePanel {

        public GameObject panel, canPanel, cantPanel;

        public Text type, buff;

        public Button yes, close;

    }

    [Serializable]
    public struct TrapPanel {

        public GameObject panel;

        public EventTrigger trap;

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

        public TextMeshProUGUI poolLabel, preparationSlotsCount;

        public Button continueButton, returnButton, cancelButton;

    }

    public struct HeroDeck {

        public Vector3 pos;

        public string hero, trap;

        public string[] weapons;

        public HeroDeck (Vector3 p, string h, string t, string[] w) {

            pos = p;

            hero = h;

            trap = t;

            weapons = w;

        }

    }

    [Serializable]
    public struct QinPreparationUI {

        public GameObject panel, decks, pool;

        public List<SC_CastleDeck> castleDecks;

        public SC_QinPreparationSlot curseSlot;

        public GameObject castlesPool, trapsPool, cursesPool, soldiersPool;

        public TextMeshProUGUI poolLabel, preparationSlotsCount, curseCost;

        public Button continueButton, returnButton, cancelButton;

    }

    public struct CastleDeck {

        public string castle, trap;

        public CastleDeck (string c, string t) {

            castle = c;

            trap = t;
        }

    }

    public struct SoldierInfos {

        public Vector3 pos;

        public string name;

        public SoldierInfos (Vector3 p, string n) {

            pos = p;

            name = n;
        }

    }

    public static SC_QinPreparationSlot GetPrepCastle (SC_Castle c) {

        return SC_UI_Manager.Instance.qinPreprationUI.castleDecks[c.Tile.Region].Castle;

    }

    public static bool IsPrepCastle (int e) {

        return e == (int) EQinPreparationElement.Castles && SC_Player.localPlayer.Qin;

    }

    public static bool CanCreateConstruct (string c) {

        return (SC_Qin.GetConstruCost (c) < SC_Qin.Energy) && (SC_Tile_Manager.Instance.GetConstructableTiles (c).Count > 0);

    }

    public static bool CanCreateSoldier (string s) {

        return Resources.Load<SC_Soldier> ("Prefabs/Characters/Soldiers/Basic/P_" + s).cost < SC_Qin.Energy;

    }

    public static void ForceSelect (GameObject g) {

        EventSystem.current.SetSelectedGameObject (null);
        EventSystem.current.SetSelectedGameObject (g);

    }

    public static string GetRandomSprite (string path) {

        return path + UnityEngine.Random.Range (0, Resources.LoadAll<Sprite> (path).Length).ToString ();

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

            return GetValue (0, threshold);

        }

        public float GetValue (int index, int threshold) {

            float v = 0;

            for (int i = 0; i < thresholds.Length; i++)
                if (threshold >= thresholds[i])
                    v = values[index].values[i];

            return v;

        }

    }

    public static T GetObjectUnderMouse<T> () where T : MonoBehaviour {

        T t = null;

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

        RaycastHit2D[] hit = Physics2D.GetRayIntersectionAll (ray, Mathf.Infinity);

        foreach (RaycastHit2D r in hit)
            t = r.transform.gameObject.GetComponent<T> () ?? t;

        return t;

    }

    [Serializable]
    public struct Tooltip {

        public string id, text;

    }

    public enum EHeroPreparationElement { Hero, Weapon, Trap, Deployment, Confirmation }

    public enum EQinPreparationElement { Castles, Trap, Curse, Soldiers, Confirmation }

}

