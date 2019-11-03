using UnityEngine;
using static SC_Global;

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
            Setup ();
        else
            CastleType = "";

        base.Start();

        castles[Tile.Region] = true;

        transform.parent = uiManager.castlesT;

        Roof.sortingOrder = GetComponentInChildren<SpriteRenderer>().sortingOrder;

    }

    public void SetCastle (string type) {

        CastleType = type;

        Setup ();

        uiManager.TryRefreshInfos (gameObject, typeof (SC_Castle));

        foreach (SC_Tile t in tileManager.ChangingTiles) {

            if (t.Region == Tile.Region) {

                t.infos.type = CastleType == "" ? "Changing" : CastleType;

                t.infos.riverSprite = (int) SC_EditorTile.RiverSprite.Small_Alone;

                t.SetupTile ();

            }

        }

    }

    public void Setup () {

        Name = CastleType + " Castle";

        Roof.sprite = CastleType == "" ? null : Resources.Load<Sprite>("Sprites/Constructions/Castle/Roofs/" + CastleType);

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

    void OnMouseOver () {

        if (Input.GetMouseButtonDown (1) && uiManager.PreparationPhase == (int)EQinPreparationElement.Castles && CastleType != "") {

            SC_UI_Manager.Instance.QinPreparationSlotsCount--;            

            SC_PreparationElement.GiveBackElement ((int) EQinPreparationElement.Castles, CastleType + "Castle");

            GetPrepCastle (this).Renderer.sprite = GetPrepCastle (this).DefaultSprite;

            SetCastle ("");

        }

    }   

}
