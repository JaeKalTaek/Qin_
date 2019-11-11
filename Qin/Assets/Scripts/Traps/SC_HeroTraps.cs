using UnityEngine;

public class SC_HeroTraps : MonoBehaviour {

    public static SC_HeroTraps Instance { get; set; }

    public static SC_Hero hero;

    void Awake () {

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

    #region Earth's Wrath
    [Header ("Earth's Wrath")]
    [Tooltip ("Earth's Wraths radius")]
    public int earthsWrathRadius;

    public void EarthsWrath () {

        foreach (SC_Tile t in SC_Tile_Manager.Instance.GetRange (hero.Tile.transform.position, earthsWrathRadius))
            Raze (t);

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

    #region Legacy
    [Header ("Legacy")]
    [Tooltip ("Legacy values")]
    public SC_Global.ThresholdValues legacyValues;

    public void Legacy () {

        foreach (SC_Hero h in SC_Hero.heroes) {

            if (h != hero) {

                int relationship;
                hero.Relationships.TryGetValue (h.characterName, out relationship);

                h.StrengthModifiers += (int)legacyValues.GetValue (relationship);
                h.ChiModifiers += (int) legacyValues.GetValue (relationship);

            }

        }

    }
    #endregion

}
