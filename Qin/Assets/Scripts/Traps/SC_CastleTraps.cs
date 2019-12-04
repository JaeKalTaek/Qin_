using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Character;

public class SC_CastleTraps : MonoBehaviour {

    public static SC_CastleTraps Instance { get; set; }

    SC_Game_Manager GameManager { get { return SC_Game_Manager.Instance; } }

    SC_Tile_Manager TileManager { get { return SC_Tile_Manager.Instance; } }

    int Region { get { return GameManager.CurrentCastle.Tile.Region; } }

    SC_Hero Destroyer { get { return (activeCharacter?.gameObject.activeSelf ?? false) ? activeCharacter.Hero : null; } }

    void Awake () {

        Instance = this;

    }

    #region Stasis
    [Header ("Stasis")]
    [Tooltip ("Stasis radius")]
    public int stasisRadius;

    [Tooltip ("Stasis stun duration")]
    public int stasisStunDuration;

    public void Stasis () {

        foreach (SC_Tile t in TileManager.GetRange (GameManager.CurrentCastle.transform.position, stasisRadius))
            t.Hero?.Stun (stasisStunDuration);

    }
    #endregion

    #region Labyrinth
    public void Labyrinth () {


        if (GameManager.isServer)
            foreach (SC_Tile t in TileManager.tiles)
                if (t.Region == Region && t.infos.type != "Plain" && t.Constructable)
                    NetworkServer.Spawn (Instantiate (Resources.Load<GameObject> ("Prefabs/Constructions/P_Wall"), t.transform.position, Quaternion.identity));

    }
    #endregion

    #region Reinforcements
    [Header ("Reinforcements")]
    [Tooltip ("Reinforcements soldier")]
    public string reinforcementsSoldier;

    public void Reinforcements () {

        if (GameManager.isServer && Destroyer) {

            foreach (SC_Tile t in TileManager.tiles) {

                if (!t.Character && !t.DrainingStele && !t.Qin &&!t.Grave && TileManager.NearTiles (t, Destroyer.Tile)) {

                    GameObject go = Instantiate (Resources.Load<GameObject> ("Prefabs/Characters/Soldiers/P_BaseSoldier"), t.transform.position, Quaternion.identity);

                    go.GetComponent<SC_Soldier> ().characterPath = "Prefabs/Characters/Soldiers/Basic/P_" + reinforcementsSoldier;

                    NetworkServer.Spawn (go);

                }

            }

        }

    }
    #endregion

    #region Time Walk
    public void TimeWalk () {

        Destroyer?.SetCharacterPos (Destroyer.StartingTile);

    }
    #endregion

    #region Disillusion
    public void Disillusion () {

        if (Destroyer) {

            List<SC_Hero> bestFriends = new List<SC_Hero> ();

            foreach (SC_Hero hero in SC_Hero.heroes) {

                if (Destroyer.characterName != hero.characterName) {

                    if (bestFriends.Count == 0)
                        bestFriends.Add (hero);
                    else {

                        int currentValue = Destroyer.Relationships[bestFriends[0].characterName];

                        int value = Destroyer.Relationships[hero.characterName];

                        if (value >= currentValue) {

                            if (value > currentValue)
                                bestFriends.Clear ();

                            bestFriends.Add (hero);

                        }

                    }

                }

            }

            SC_Hero closest = bestFriends[0];

            if (bestFriends.Count > 1) {                

                foreach (SC_Hero h in bestFriends)
                    if (SC_Tile_Manager.TileDistance (Destroyer.Tile, h.Tile) < SC_Tile_Manager.TileDistance (Destroyer.Tile, closest.Tile))
                        closest = h;

            }

            Destroyer.RelationGainBlocked = closest.characterName;
            closest.RelationGainBlocked = Destroyer.characterName;

            Destroyer.Relationships[closest.characterName] = 0;
            closest.Relationships[Destroyer.characterName] = 0;

        }

    }
    #endregion

    #region Retribution
    [Header ("Retribution")]
    [Tooltip ("Retribution damage")]
    public int retributionDamage;

    public void Retribution () {

        Destroyer?.Hit (retributionDamage);

    }
    #endregion

    #region Scorched Earth
    [Header ("Scorched Earth")]
    [Tooltip ("Scorched Earth damage")]
    public int scorchedDamage;

    public void ScorchedEarth () {

        GameManager.ScorchedRegion = Region;

    }
    #endregion

}
