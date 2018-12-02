using UnityEngine;
using UnityEngine.Networking;

public class SC_Castle : SC_Construction {

    [Header("Castles variables")]
    [Tooltip("Energy gained by Qin when he sacrifices this castle")]
    public int sacrificeValue;

    [HideInInspector]
    [SyncVar]
    public string CastleType;

    public static bool[] castles;

    protected override void Start () {

        if (!SC_Game_Manager.Instance.prep)
            Setup();

        base.Start();

        castles[Tile.Region] = true;

        transform.parent = uiManager.castlesT;

    }

    public void SetCastle (string type) {

        SC_Player.localPlayer.CmdChangeCastleType(gameObject, type, Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Tiles/" + type).Length));

    }

    public void SetCastle (string type, int sprite) {

        CastleType = type;

        if(SC_Player.localPlayer.Qin)
            Setup();

        foreach (SC_Tile t in tileManager.ChangingTiles) {

            if (t.Region == Tile.Region) {

                t.GetComponent<SC_Tile>().infos.type = CastleType;

                t.GetComponent<SC_Tile>().infos.sprite = sprite;

            }

        }

    }

    public void Setup() {

        Name = CastleType + " Castle";

        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/Castles/" + CastleType);

    }

    public override void DestroyConstruction () {

        base.DestroyConstruction();

        tileManager.UpdateNeighborWallGraph(Tile);

        foreach (SC_Tile t in tileManager.regions[Tile.Region])
            t.Ruin?.DestroyConstruction();

        castles[Tile.Region] = false;

        SC_Demon.demons[Tile.Region].DestroyCharacter();

        bool victory = true;

        foreach (bool b in castles)
            if (b)
                victory = false;

        if (victory)
            uiManager.ShowVictory(false);
        

    }

}
