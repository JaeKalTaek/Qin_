using UnityEngine;
using UnityEngine.Networking;
using static SC_Character;

public class SC_CastleTraps : MonoBehaviour {

    public static SC_CastleTraps Instance { get; set; }

    SC_Game_Manager GameManager { get { return SC_Game_Manager.Instance; } }

    SC_Tile_Manager TileManager { get { return SC_Tile_Manager.Instance; } }

    int Region { get { return GameManager.CurrentCastle.Tile.Region; } }

    void Awake () {

        Instance = this;

    }

    #region Stasis
    [Header ("Stasis")]
    [Tooltip ("Stasis radius")]
    public int stasisRadius;

    public void Stasis () {

        foreach (SC_Tile t in TileManager.GetRange (GameManager.CurrentCastle.transform.position, stasisRadius))
            if (t.Hero)
                t.Hero.Stunned = true;

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

        if (GameManager.isServer && activeCharacter) {

            foreach (SC_Tile t in TileManager.tiles) {

                if (!t.Character && !t.DrainingStele && !t.Qin) {

                    int dist = SC_Tile_Manager.TileDistance (t, activeCharacter.Tile);

                    if (dist == 1 || (dist == 2 && t.transform.position.x != activeCharacter.transform.position.x && t.transform.position.y != activeCharacter.transform.position.y)) {

                        GameObject go = Instantiate (Resources.Load<GameObject> ("Prefabs/Characters/Soldiers/P_BaseSoldier"), t.transform.position, Quaternion.identity);

                        go.GetComponent<SC_Soldier> ().characterPath = "Prefabs/Characters/Soldiers/Basic/P_" + reinforcementsSoldier;

                        NetworkServer.Spawn (go);

                    }

                }

            }

        }

    }
    #endregion

    #region Time Walk
    public void TimeWalk () {

        activeCharacter?.RollbackCharacterPos (activeCharacter.Hero.StartingTile);

    }
    #endregion

    #region Disillusion
    [Header ("Disillusion")]
    [Tooltip ("Disillusion push distance")]
    public int disillusionPushDistance;

    public void Disillusion () {

        

    }
    #endregion

    #region Retribution
    [Header ("Retribution")]
    [Tooltip ("Retribution damage")]
    public int retributionDamage;

    public void Retribution () {

        activeCharacter?.Hit (retributionDamage);

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
