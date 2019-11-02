using System.Collections.Generic;
using UnityEngine;

public class SC_EditorTile : MonoBehaviour {

	[Header("Editor Tile Variables")]
    [Tooltip("Type of this tile")]
    public TileType tileType;

    [Tooltip("Sprite of the river on this tile")]
    public RiverSprite riverSprite;

    [Tooltip("Region to which this tile belongs")]
    public Region region = Region.None;

    public Region PrevRegion { get; set; }

    public static List<SC_EditorTile>[] regions;

    [Header("Construction on this tile")]
    [Tooltip("Type of construction on this tile")]
    public ConstructionType construction;

    [Tooltip("Type of the castle on this tile")]
    public CastleType castleType;

    [Header("Character on this tile")]
    [Tooltip("Type of Soldier on this tile")]
    public SoldierType soldier;
    public SoldierType PrevSoldier { get; set; }

    [Tooltip("Is Qin on this tile ?")]
    public bool Qin;
    public bool PrevQin { get; set; }

    [Tooltip("Type of Hero on this tile")]
    public HeroType Hero;
    public HeroType PrevHero { get; set; }

    [Tooltip ("Is this tile a possible deployment tile for heroes ?")]
    public bool heroDeployTile;
    public bool PrevHeroDeployTile { get; set; }

    public static int heroesDeployTilesCount;

    public static SC_EditorTile currentQinTile;

    public static List<HeroTile> heroesOnTiles = new List<HeroTile>();    

    public bool IsRiver { get { return tileType == TileType.River; } }

    public bool IsChanging { get { return tileType == TileType.Changing; } }

    static SC_MapEditorScript map;

    public static SC_EditorTile GetHeroTile (HeroType h) {

        SC_EditorTile tile = null;

        foreach (HeroTile hT in heroesOnTiles)
            if (hT.hero == h)
                tile = hT.tile;

        return tile;

    }

    public enum TileType {

        Plain, Forest, Mountain, Palace, River, Desert, Snow, Changing

    }

    public enum Region {

        None = -1, Zhao, Wei, Chu, Qi, Yan, Han

    }

    public enum RiverSprite {

        Horizontal, Vertical, Bottom, Left, Top, Right, LeftBottom, LeftTop, RightBottom, RightTop, T_Top, T_Bottom, T_Right, T_Left, Cross, Big_Alone, Big_Alone_2, Small_Alone

    }

    public enum ConstructionType {

        None, Village, Pit, Bastion, Wall, Ruin, Castle

    }

    public enum CastleType {

        Plain, River, Mountain, Forest, Desert, Snow

    }

    public enum SoldierType {

        None, Adept, Archer, Builder, Guard, Herald, Hermit, Lancer, Monk, Scout

    }

    public enum HeroType {

        None, Lin, Lei, Jung, Feng, Yama, Tudi

    }

    public struct HeroTile {

        public HeroType hero;

        public SC_EditorTile tile;

        public HeroTile (HeroType h, SC_EditorTile t) {

            hero = h;

            tile = t;

        }

    }

    public void SetSprite (int index, string s) {
        
        if (transform.GetChild (index).GetComponent<SpriteRenderer> ())
            transform.GetChild(index).GetComponent<SpriteRenderer>().sprite = (s == "") ? null : Resources.Load<Sprite>("Sprites/" + s);

    }

    public static void ChangeTileRegion(SC_EditorTile tile) {

        if (!map)
            map = FindObjectOfType<SC_MapEditorScript>();

        if (regions == null || map.Tiles == null)
            map.SetupMap();

        bool changed = true;      

        int r = (int)tile.region;

        if (r != -1) {

            if (regions[r].Count == 0)
                regions[r].Add(tile);
            else {

                bool canAdd = false;

                foreach (SC_EditorTile eTile in regions[r])
                    if (SC_Tile_Manager.TileDistance(tile.transform.position, eTile.transform.position) <= 1)
                        canAdd = true;

                if (canAdd)
                    regions[r].Add(tile);
                else
                    changed = false;

            }

        }

        if(changed) {

            if (tile.PrevRegion != Region.None)
                regions[(int)tile.PrevRegion].Remove(tile);

            tile.UpdateBorders();

            foreach (SC_EditorTile eTile in SC_Tile_Manager.GetTilesAtDistance(map.Tiles, tile, 1))
                eTile.UpdateBorders();

            tile.PrevRegion = tile.region;

        } else
            tile.region = tile.PrevRegion;        

    }

    Transform Borders { get { return transform.GetChild(2); } }

    public void UpdateBorders() {

        Borders.gameObject.SetActive(region != Region.None);

        if (region != Region.None) {

            foreach (SC_EditorTile eTile in SC_Tile_Manager.GetTilesAtDistance(map.Tiles, this, 1)) {

                if (eTile.transform.position.y > transform.position.y)
                    SetBorderSprite(0, eTile);
                else if (eTile.transform.position.x < transform.position.x)
                    SetBorderSprite(1, eTile);
                else if (eTile.transform.position.x > transform.position.x)
                    SetBorderSprite(2, eTile);
                else
                    SetBorderSprite(3, eTile);

            }

        }

    }

    void SetBorderSprite(int i, SC_EditorTile eTile) {

        Borders.GetChild(i).GetComponent<SpriteRenderer>().sprite = eTile.region == region ? null : Resources.Load<Sprite>("Sprites/RegionBorders/" + region);

    }

}