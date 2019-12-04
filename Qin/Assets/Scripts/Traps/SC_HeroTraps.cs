using System;
using System.Collections.Generic;
using UnityEngine;
using static SC_Global;

public class SC_HeroTraps : MonoBehaviour {

    public static SC_HeroTraps Instance { get; set; }

    public static SC_Hero hero;

    public static List<Action> actions;

    void Awake () {

        actions = new List<Action> ();

        Instance = this;

    }

    void Raze (SC_Tile tile) {

        if (tile.Character?.Qin ?? false)
            tile.Character.DestroyCharacter ();

        tile.Construction?.DestroyConstruction (true);

        tile.infos.type = "Plain";
        tile.SetupTile ();

    }

    #region Heavenly Beams 
    public void HeavenlyBeams () {

        foreach (SC_Tile t in SC_Tile_Manager.Instance.tiles)
            if (t.transform.position.x == hero.Tile.transform.position.x || t.transform.position.y == hero.Tile.transform.position.y)
                Raze (t);

    }
    #endregion

    #region Darkest Night
    [Header ("Darkest Night")]
    [Tooltip ("Darkest Night's radius")]
    public int darkestNightRadius;

    public void DarkestNight () {

        if (SC_Player.localPlayer.Qin)
            foreach (SC_Tile t in SC_Tile_Manager.Instance.GetRange (hero.transform.position, darkestNightRadius))
                t.CreateFog (true);

    }
    #endregion

    #region Earth's Wrath
    [Header ("Earth's Wrath")]
    [Tooltip ("Earth's Wraths radius")]
    public int earthsWrathRadius;

    public void EarthsWrath () {

        foreach (SC_Tile t in SC_Tile_Manager.Instance.GetRange (hero.Tile.transform.position, earthsWrathRadius))
            Raze (t);

    }
    #endregion

    #region After Images
    [Header ("After Images")]
    [Tooltip ("Percent of health the killed hero resuscitates with")]
    [Range(1, 100)]
    public int afterImagesResurrectionHealthPercent;

    [Tooltip ("Number of after images")]
    public int afterImagesCount;

    [Tooltip ("After Images radius")]
    public int afterImagesRadius;

    public static Stack<SpriteRenderer> afterImagesPlacedHeroes;

    public void AfterImages () {

        hero.Health = Mathf.CeilToInt (hero.MaxHealth * (afterImagesResurrectionHealthPercent / 100f));

        actions.Add (AfterImagesActivation);

    }

    void AfterImagesActivation () {

        actions.Remove (AfterImagesActivation);        

        SC_Player.localPlayer.Busy = true;

        if (!SC_Player.localPlayer.Qin) {

            afterImagesPlacedHeroes = new Stack<SpriteRenderer> ();

            SC_Tile_Manager.Instance.RemoveAllFilters ();

            SC_Cursor.SetLock (false);

            foreach (SC_Tile t in SC_Tile_Manager.Instance.GetRange (hero.transform.position, afterImagesRadius))
                if (hero.CanCharacterSetOn (t) && t.baseCost < 100)
                    t.ChangeDisplay (TDisplay.Deploy);

            SC_UI_Manager.Instance.DisplayInfo ("Place your hero");

        } else
            SC_UI_Manager.Instance.DisplayInfo ("Waiting for your opponent");     

    }

    public static void AfterImagesCreateSprite (Vector3 pos) {

        SC_UI_Manager.Instance.DisplayInfo ("Place a clone");

        afterImagesPlacedHeroes.Push (new GameObject ("After Images Sprite").AddComponent<SpriteRenderer> ());

        afterImagesPlacedHeroes.Peek ().transform.position = pos;

        afterImagesPlacedHeroes.Peek ().sprite = hero.Sprite.sprite;

        bool canDeploy = false;

        foreach (SC_Tile t in SC_Tile_Manager.Instance.GetRange (hero.transform.position, Instance.afterImagesRadius))
            canDeploy |= t.CurrentDisplay == TDisplay.Deploy;

        SC_UI_Manager.Instance.backAction = (canDeploy && afterImagesPlacedHeroes.Count < 3) ? (Action) AfterImagesReturn : DoNothing;

        if (afterImagesPlacedHeroes.Count == 3 || !canDeploy) {

            SC_UI_Manager.Instance.DisplayInfo ("");

            SC_Tile_Manager.Instance.RemoveAllFilters (true);

            List<Vector3> positions = new List<Vector3> ();

            foreach (SpriteRenderer s in afterImagesPlacedHeroes) {

                positions.Add (s.transform.position);

                Destroy (s.gameObject);

            }

            SC_Player.localPlayer.CmdSpawnAfterImages (positions.ToArray());

        }

    }

    public static void AfterImagesReturn () {

        SC_Tile_Manager.Instance.GetTileAt (afterImagesPlacedHeroes.Peek ().gameObject).ChangeDisplay (TDisplay.Deploy);

        Destroy (afterImagesPlacedHeroes.Peek ().gameObject);

        afterImagesPlacedHeroes.Pop ();

        if (afterImagesPlacedHeroes.Count <= 0) {

            SC_UI_Manager.Instance.DisplayInfo ("Place your hero");

            SC_UI_Manager.Instance.backAction = DoNothing;

        }

    }
    #endregion

    #region Heaven's Light
    [Header ("Heaven's Light")]
    [Tooltip ("Heaven's Light's radius")]
    public int heavensLightRadius;

    [Tooltip ("Heaven's Light's heal")]
    public int heavensLightHeal;

    public void HeavensLight () {

        foreach (SC_Hero h in SC_Hero.heroes)
            if (h != hero && SC_Tile_Manager.TileDistance (h.Tile, hero.Tile) <= heavensLightRadius)
                h.Heal (heavensLightHeal);                
               
    }
    #endregion

    #region Rallying Cry
    public void RallyingCry () {

        SC_Game_Manager.Instance.AdditionalTurn = false;

    }
    #endregion

    #region Human's Fate
    [Header ("Human's Fate")]
    [Tooltip ("Human's Fate's duration")]
    public int humansFateDuration;

    public void HumansFate () {

        hero.Dead = false;

        hero.Tile.Character = hero;

        hero.Tile.UpdateFog ();

        hero.gameObject.SetActive (true);

        hero.Health = hero.PreviousHealth;        

        hero.HumansFateDuration = humansFateDuration;

        hero.SetBerserk (true);

    }
    #endregion

    #region Legacy
    [Header ("Legacy")]
    [Tooltip ("Legacy values")]
    public ThresholdValues legacyValues;

    public void Legacy () {

        foreach (SC_Hero h in SC_Hero.heroes) {

            if (h != hero) {

                h.StrengthModifiers += (int)legacyValues.GetValue (hero.Relationships[h.characterName]);
                h.ChiModifiers += (int) legacyValues.GetValue (hero.Relationships[h.characterName]);

            }

        }

    }
    #endregion

}
