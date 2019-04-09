using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Tile_Manager : NetworkBehaviour {

    [Header("Tweakable Variables")]
    [Tooltip("Number of border sprites around the board")]
    public int borderSize;

    [HideInInspector]
	[SyncVar]
	public int xSize, ySize;

	public SC_Tile[,] tiles;

    public List<SC_Tile>[] regions;

    public List<SC_Tile> ChangingTiles { get; set; }

    static SC_Game_Manager gameManager;

    static SC_UI_Manager uiManager;

    public static SC_Tile_Manager Instance { get; set; }

    //Variables used to determine the movements possible
    List<SC_Tile> OpenList { get; set; }
    // List<SC_Tile> MovementRange { get; set; }
    Dictionary<SC_Tile, int> movementPoints = new Dictionary<SC_Tile, int>();

    public SC_DrainingStele DisplayedDrainingStele { get; set; }    

    void Awake() {

        Instance = this;

    }

    void Start () {

        gameManager = SC_Game_Manager.Instance;

        uiManager = SC_UI_Manager.Instance;
        uiManager.TileManager = this;

        SC_Fight_Manager.Instance.TileManager = this;

        /*SC_Cursor.Instance = Instantiate(Resources.Load<SC_Cursor>("Prefabs/P_Cursor"));

        if (gameManager.Player.Qin)
            SC_Cursor.Instance.transform.position = new Vector3(xSize - 1, ySize - 1, 0) * SC_Game_Manager.TileSize;
        else
            SC_Cursor.Instance.transform.position = Vector3.zero;*/

        // FindObjectOfType<SC_Camera>().Setup(xSize, ySize);

        tiles = new SC_Tile[xSize, ySize];

        regions = new List<SC_Tile>[6];

        for (int i = 0; i < regions.Length; i++)
            regions[i] = new List<SC_Tile>();

        ChangingTiles = new List<SC_Tile>();

        foreach (SC_Tile t in FindObjectsOfType<SC_Tile>()) {

            tiles[t.transform.position.x.I(), t.transform.position.y.I()] = t;

            if (t.infos.type == "Changing")
                ChangingTiles.Add(t);

            if(t.Region != -1)
                regions[t.Region].Add(t);

        }

        for (int i = -borderSize; i <= xSize + borderSize; i++) {

            for (int j = -borderSize; j <= ySize + borderSize; j++) {

                Vector3 pos = new Vector3(i - .5f, j - .5f, 0) * SC_Game_Manager.TileSize;

                string path = "";

                bool l = i == 0;
                bool b = j == 0;
                bool r = i == xSize;
                bool t = j == ySize;

                if ((i < 0) || (j < 0) || (i > xSize) || (j > ySize)) {

                    path += "Full";

                } else if (b || t || l || r) {

                    if (b || t)
                        path += (l || r) ? "Corner/" + (b ? "Bottom" : "Top") + (l ? "Left" : "Right") : RandomBorder(b ? "Bottom" : "Top");
                    else
                        path += RandomBorder(l ? "Left" : "Right");

                }

                if (path != "") {

                    GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/P_Border"), pos, Quaternion.identity, uiManager.bordersT);
                    go.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Borders/" + path);

                }

            }

        }

		gameManager.FinishSetup ();

        OpenList = new List<SC_Tile>();
        // MovementRange = new List<SC_Tile>();

    }

    string RandomBorder(string folderPath) {

        return folderPath + "/" + UnityEngine.Random.Range(0, Resources.LoadAll<Sprite>("Sprites/Borders/" + folderPath).Length);

    }

    #region Utility Functions
    public static List<T> GetTilesAtDistance<T>(Array array, T center, int distance) where T : MonoBehaviour {

        return GetTilesAtDistance<T>(array, center.transform.position, distance);

    }

    public static List<T> GetTilesAtDistance<T>(Array array, Vector3 center, int distance) where T : MonoBehaviour {

        List<T> returnValue = new List<T>();

        foreach(T tile in array) {

            if (TileDistance(center, tile) == distance)
                returnValue.Add(tile);

        }

        return returnValue;

    }

    public List<SC_Tile> GetRange(Vector3 center, int distance) {

        return GetRange(center, new Vector2(0, distance));

    }

    public List<SC_Tile> GetRange (Vector3 center, Vector2 range) {

        List<SC_Tile> returnValue = new List<SC_Tile>();

        foreach (SC_Tile tile in tiles) {

            int dist = TileDistance(center, tile);

            if ((dist >= range.x) && (dist <= range.y))
                returnValue.Add(tile);

        }

        return returnValue;

    }

    public static int TileDistance<T>(Vector3 a, T b) where T : MonoBehaviour {

        return TileDistance(a, b.transform.position);

    }

    public static int TileDistance (Vector3 a, Vector3 b) {

        return Mathf.Abs((a.x - b.x).I()) + Mathf.Abs((a.y - b.y).I());

    }

    public SC_Tile GetUnoccupiedNeighbor (SC_Character target) {

        SC_Tile t = null;

        foreach (SC_Tile tile in GetTilesAtDistance<SC_Tile>(tiles, target.transform.position, 1))
            if (target.CanCharacterSetOn(tile))
                t = tile;

        return t;

    }

    public SC_Tile GetTileAt (GameObject g) {

        return GetTileAt(g.transform.position);

    }

    public SC_Tile GetTileAt (int x, int y) {

        return tiles[x, y];

    }

    public SC_Tile GetTileAt (Vector3 pos) {

        try {

            return tiles[pos.x.I(), pos.y.I()];

        } catch (IndexOutOfRangeException) {

            return null;

        }

    }

    public void RemoveAllFilters (bool async = false) {

        if(SC_Player.localPlayer.Turn || async)
            foreach (SC_Tile tile in tiles)
                tile.RemoveDisplay();

    }
    #endregion

    #region Attack
    public List<SC_Tile> GetAttackTiles () {

        return GetAttackTiles(SC_Character.attackingCharacter, SC_Character.attackingCharacter.Tile);

    }

    public List<SC_Tile> GetAttackTiles(SC_Character attacker, SC_Tile center) {

        List<SC_Tile> attackableTiles = GetRange(center.transform.position, attacker.GetRange(center));

        attackableTiles.RemoveAll(t => !t.CanCharacterAttack(attacker));

        return attackableTiles;

    }

    public void CheckAttack () {

        RemoveAllFilters();

        foreach (SC_Tile tile in GetAttackTiles()) {

            tile.ChangeDisplay(TDisplay.Attack);

            if (tile.CursorOn)
                tile.Hero?.PreviewAttackOnHero();

        }

    }

    public void PreviewAttack() {

        foreach (SC_Tile t in GetAttackTiles())
            t.SetFilter(TDisplay.Attack, true);

    }

    public List<SC_Hero> HeroesInRange (SC_Hero target) {

        List<SC_Hero> heroesInRange = new List<SC_Hero>();

        foreach (SC_Tile tile in GetRange(target.transform.position, 3)) {

            if (tile.Character) {

                if (tile.Character.Hero && !tile.Character.Qin) {

                    if (!tile.Character.characterName.Equals(target.characterName) && !heroesInRange.Contains(tile.Character.Hero))
                        heroesInRange.Add(tile.Character.Hero);

                }

            }

        }

        return heroesInRange;

    }    
    #endregion

    #region Display Movements
    public void CheckMovements (SC_Character target) {

        RemoveAllFilters();

        DisplayMovementAndAttack(target, false);

    }

    public List<SC_Tile> GetMovementRange (SC_Character target) {

        OpenList.Clear();
        List<SC_Tile> movementRange = new List<SC_Tile>();

        movementPoints[target.Tile] = target.Movement;

        ExpandTile(ref movementRange, target.Tile, target);

        while (OpenList.Count > 0) {

            OpenList.Sort((a, b) => movementPoints[a].CompareTo(movementPoints[b]));

            SC_Tile tile = OpenList[OpenList.Count - 1];

            OpenList.RemoveAt(OpenList.Count - 1);

            ExpandTile(ref movementRange, tile, target);

        }

        return movementRange;

    }

    public void DisplayMovementAndAttack (SC_Character target, bool preview) {

        List<SC_Tile> movementRange = GetMovementRange(target);

        if (SC_Player.localPlayer.Turn || preview) {

            foreach (SC_Tile tile in new List<SC_Tile>(movementRange) { target.Tile }) {

                if (target.CanCharacterSetOn(tile)) {

                    if (preview)
                        tile.SetFilter(TDisplay.Movement, true);
                    else
                        tile.ChangeDisplay(TDisplay.Movement);

                    foreach (SC_Tile t in GetAttackTiles(target, tile))
                        if (t.CurrentDisplay == TDisplay.None && !movementRange.Contains(t))
                            t.SetFilter(TDisplay.Attack, true);

                }

            }

        }

    }

    void ExpandTile (ref List<SC_Tile> list, SC_Tile aTile, SC_Character target) {

        int parentPoints = movementPoints[aTile];

        list.Add(aTile);

        foreach (SC_Tile tile in GetTilesAtDistance(tiles, aTile, 1)) {

            if (list.Contains(tile) || OpenList.Contains(tile) || !target.CanCharacterGoThrough(tile))
                continue;

            int points = parentPoints - tile.Cost; //((target.Hero?.Berserk ?? false) ? 1 : tile.Cost);

            if (points >= 0) {

                OpenList.Add(tile);

                movementPoints[tile] = points;

            }

        }

    }

    public List<SC_Tile> PathFinder (SC_Tile start, SC_Tile end) {

        List<SC_Tile> movementRange = GetMovementRange(start.Character);

        List<SC_Tile> openList = new List<SC_Tile>();
        List<SC_Tile> tempList = new List<SC_Tile>();
        List<SC_Tile> closedList = new List<SC_Tile>();

        start.Parent = null;
        openList.Add(start);

        while (!openList.Contains(end)) {

            foreach (SC_Tile tile in openList) {

                foreach (SC_Tile neighbor in GetTilesAtDistance(tiles, tile, 1)) {

                    if (!closedList.Contains(neighbor) && movementRange.Contains(neighbor) && !tempList.Contains(neighbor)) {

                        tempList.Add(neighbor);
                        neighbor.Parent = tile;

                    }

                }

                closedList.Add(tile);

            }

            openList = new List<SC_Tile>(tempList);
            tempList.Clear();

        }

        List<SC_Tile> path = new List<SC_Tile>();
        SC_Tile currentParent = end;

        while (!path.Contains(start)) {

            path.Add(currentParent);
            currentParent = currentParent.Parent;

        }

        foreach (SC_Tile tile in tiles)
            tile.Parent = null;

        path.Reverse();

        return (path.Count > 1) ? path : null;

    }
    #endregion

    #region Construction
    public List<SC_Tile> GetConstructableTiles(string c) {

        List<SC_Tile> constructableTiles = new List<SC_Tile>();

        if (c == "Wall") {

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>()) {

                if (construction.GreatWall) {

                    foreach (SC_Tile neighbor in GetTilesAtDistance<SC_Tile>(tiles, construction.transform.position, 1))
                        if (neighbor.Constructable(false) && !constructableTiles.Contains(neighbor))
                            constructableTiles.Add(neighbor);

                }

            }

        } else {

            for (int i = 0; i < regions.Length; i++)
                if (SC_Castle.castles[i])
                    foreach (SC_Tile tile in regions[i])
                        if (tile.Constructable(gameManager.QinTurnStarting && tile.Soldier))
                            constructableTiles.Add(tile);

        }

        return constructableTiles;

    }

    public void DisplayConstructableTiles (string c) {

        foreach (SC_Tile tile in GetConstructableTiles(c))
            tile.GetComponent<SC_Tile>().ChangeDisplay(TDisplay.Construct);

    }

    public void UpdateNeighborWallGraph (SC_Tile center) {

        foreach (SC_Tile tile in GetTilesAtDistance(tiles, center, 1))
            if (tile.GreatWall)
                UpdateWallGraph(tile.Construction.gameObject);

    }

    public void UpdateWallGraph (GameObject go) {

        SC_Construction construction = go.GetComponent<SC_Construction>();

        SC_Tile under = construction.Tile;

        bool left = false;
        bool right = false;
        bool top = false;
        int count = 0;

        foreach (SC_Tile tile in GetTilesAtDistance(tiles, under, 1)) {

            if (tile.GreatWall) {

                if (tile.transform.position.x < under.transform.position.x)
                    left = true;
                else if (tile.transform.position.x > under.transform.position.x)
                    right = true;
                else if (tile.transform.position.y > under.transform.position.y)
                    top = true;

                count++;

            }

        }

        string rotation = "";

        if (count == 1)
            rotation = right ? "Right" : left ? "Left" : top ? "Top" : "Bottom";
        else if (count == 2)
            rotation = right ? (left ? "RightLeft" : top ? "RightTop" : "RightBottom") : left ? (top ? "LeftTop" : "LeftBottom") : "TopBottom";
        else if (count == 3)
            rotation = !right ? "Left" : (!left ? "Right" : (!top ? "Bottom" : "Top"));

        if (!rotation.Equals(""))
            rotation = "_" + rotation;

        construction.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/" + (construction as SC_Castle ? "Castle" : (construction as SC_Wall ? "Wall" : "Bastion")) + "/" + count.ToString() + rotation);

    }
    #endregion

    #region Display Actions
    public void DisplaySacrifices () {        

        foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>())
            soldier.Tile.ChangeDisplay(TDisplay.Sacrifice);

    }

    /*public void DisplayResurrection () {

        foreach (SC_Tile tile in tiles)
            if (tile.Empty)
                tile.ChangeDisplay(TDisplay.Resurrection);

    }*/
    #endregion

}
