using UnityEngine;
using UnityEngine.Networking;

public class SC_Castle : SC_Bastion {

    [Header("Castles variables")]
    [Tooltip("Energy gained by Qin when he sacrifices this castle")]
    public int sacrificeValue;

    public string CastleType { get; set; }

    public static bool[] castles;

    public int DemonCost { get { return Resources.Load<SC_Demon>("Prefabs/Characters/Demons/P_" + CastleType + "Demon").cost; } }

    public SpriteRenderer Roof { get { return transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>(); } }

    protected override void Start () {

        if (!SC_Game_Manager.Instance.prep)
            Setup();

        base.Start();

        castles[Tile.Region] = true;

        transform.parent = uiManager.castlesT;

        Roof.sortingOrder = GetComponentInChildren<SpriteRenderer>().sortingOrder;

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

    public void Setup () {

        Name = CastleType + " Castle";

        Roof.sprite = Resources.Load<Sprite>("Sprites/Constructions/Castle/Roofs/" + CastleType);

    }

    public override void DestroyConstruction (bool playSound) {

        base.DestroyConstruction(playSound);

        SC_Sound_Manager.Instance.AugmentPart();

        tileManager.UpdateNeighborWallGraph(Tile);

        foreach (SC_Tile t in tileManager.regions[Tile.Region])
            t.Ruin?.DestroyConstruction(false);

        castles[Tile.Region] = false;

        if (Health <= 0)
            SC_Demon.demons[Tile.Region]?.DestroyCharacter();

        bool victory = true;

        foreach (bool b in castles)
            if (b)
                victory = false;

        if (victory)
            uiManager.ShowVictory(false);
        

    }

}
