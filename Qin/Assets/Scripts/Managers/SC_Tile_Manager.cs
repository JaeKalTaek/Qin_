using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Tile_Manager : NetworkBehaviour {

    /*[Header("Tweakable Variables")]
    [Tooltip("Number of border sprites around the board")]
    public int borderSize;*/

    [HideInInspector]
    [SyncVar]
    public bool qinIsServerForStart;

	public SC_Tile[,] tiles;

    public List<SC_Tile>[] regions;

    public List<SC_Tile> ChangingTiles { get; set; }

    public List<SC_Tile> DeploymentTiles { get; set; }

    static SC_Game_Manager gameManager;

    static SC_UI_Manager uiManager;

    public static SC_Tile_Manager Instance { get; set; }

    List<SC_Tile> OpenList { get; set; }

    Dictionary<SC_Tile, int> movementPoints = new Dictionary<SC_Tile, int>();

    void Awake() {

        Instance = this;

    }

    void Start () {

        gameManager = SC_Game_Manager.Instance;

        uiManager = SC_UI_Manager.Instance;
        uiManager.TileManager = this;

        SC_Fight_Manager.Instance.TileManager = this;

        tiles = new SC_Tile[XSize, YSize];

        regions = new List<SC_Tile>[6];

        for (int i = 0; i < regions.Length; i++)
            regions[i] = new List<SC_Tile>();

        ChangingTiles = new List<SC_Tile>();

        DeploymentTiles = new List<SC_Tile> ();

        foreach (SC_Tile t in FindObjectsOfType<SC_Tile>()) {

            tiles[t.transform.position.x.I(), t.transform.position.y.I()] = t;

            if (t.infos.type == "Changing")
                ChangingTiles.Add(t);

            if(t.Region != -1)
                regions[t.Region].Add(t);            

            if (t.infos.heroDeploy) {

                t.ChangeDisplay (TDisplay.Deploy);

                DeploymentTiles.Add (t);                
                
            }

        }

        /*for (int i = -borderSize; i <= XSize + borderSize; i++) {

            for (int j = -borderSize; j <= YSize + borderSize; j++) {

                Vector3 pos = new Vector3(i - .5f, j - .5f, 0);

                string path = "";

                bool l = i == 0;
                bool b = j == 0;
                bool r = i == XSize;
                bool t = j == YSize;

                if ((i < 0) || (j < 0) || (i > XSize) || (j > YSize)) {

                    path += "Sprites/Borders/Full";

                } else if (b || t || l || r) {

                    if (b || t)
                        path += (l || r) ? "Sprites/Borders/Corner/" + (b ? "Bottom" : "Top") + (l ? "Left" : "Right") : RandomBorder(b ? "Bottom" : "Top");
                    else
                        path += RandomBorder(l ? "Left" : "Right");

                }

                if (path != "") {

                    GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/P_Border"), pos, Quaternion.identity, uiManager.bordersT);
                    go.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(path);
                    go.GetComponent<SpriteRenderer>().sortingOrder -= i + j - (b ? 2 : (t ? -2 : 0));

                }

            }

        }*/

		gameManager.FinishSetup ();

        OpenList = new List<SC_Tile>();

    }

    string RandomBorder (string folderPath) {

        return GetRandomSprite ("Sprites/Borders/" + folderPath + "/");

    }

    public void SetupTiles () {

        foreach (SC_Tile t in tiles)
            t.SetupTile ();

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

    public static int TileDistance (SC_Tile a, SC_Tile b) {

        return TileDistance(a.transform.position, b.transform.position);

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

        try {

            return tiles[x, y];

        } catch (IndexOutOfRangeException) {

            return null;

        }

    }

    public SC_Tile GetTileAt (Vector3 pos, bool clampFirst = false) {

        if(clampFirst)
            return tiles[Mathf.Clamp (pos.x.I (), 0, XSize - 1), Mathf.Clamp (pos.y.I (), 0, YSize - 1)];

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

    public SC_Tile ClosestMovementTile (SC_Tile currentTile) {

        List<SC_Tile> validTiles = new List<SC_Tile>();

        List<SC_Tile> idealTiles = new List<SC_Tile>();

        foreach (SC_Tile t in tiles) {

            if ((t.CurrentDisplay == TDisplay.Movement) && (!currentTile.CanAttack || (SC_Character.activeCharacter.GetRange(t).In(TileDistance(t, SC_Cursor.Tile))))) {

                validTiles.Add(t);

                if(IsTileIdeal(t))
                    idealTiles.Add(t);

            }

        }

        List<SC_Tile> range = new List<SC_Tile>((idealTiles.Count > 0) ? idealTiles : validTiles);        

        if (!currentTile.CanAttack && range.Contains(SC_Character.activeCharacter.Tile) && (range.Count > 1))
            range.Remove(SC_Character.activeCharacter.Tile);

        SC_Tile validTile = null;

        int minDistanceToHero = int.MaxValue;

        int minDistanceToCursor = int.MaxValue;

        foreach (SC_Tile t in range) {

            int distanceToHero = TileDistance(SC_Character.activeCharacter.Tile, t);

            int distanceToCursor = TileDistance(currentTile, t);

            if (currentTile.CanAttack ? IsMinDistance(distanceToHero, minDistanceToHero, distanceToCursor, minDistanceToCursor) : IsMinDistance(distanceToCursor, minDistanceToCursor, distanceToHero, minDistanceToHero)) {

                validTile = t;

                minDistanceToHero = distanceToHero;

                minDistanceToCursor = distanceToCursor;

            }
        }

        return validTile;

    }

    bool IsMinDistance (int a, int b, int c, int d) {

        return (a < b) || ((a == b) && (c < d));

    }

    public bool IsTileIdeal (SC_Tile tile) {

        SC_Character attacked = SC_Cursor.Tile.Character;

        if (SC_Cursor.Tile.CanAttack && attacked)
            return !attacked.CanCounterAttack (!SC_Cursor.Tile.Construction && (attacked.Health - SC_Fight_Manager.Instance.CalcDamage (SC_Character.activeCharacter, attacked)) <= 0, TileDistance (SC_Cursor.Tile, tile));
        else
            return true;

    }

    public bool NearTiles (SC_Tile a, SC_Tile b) {

        int dist = TileDistance (a, b);

        return dist == 1 || (dist == 2 && a.transform.position.x != b.transform.position.x && a.transform.position.y != b.transform.position.y);

    }

    public static int NearDistance (SC_Tile a, SC_Tile b) {

        return Mathf.Max (Mathf.Abs (a.transform.position.x.I () - b.transform.position.x.I ()), Mathf.Abs (a.transform.position.y.I () - b.transform.position.y.I ()));

    }

    public static int CompareTilesClockwiseOrder (SC_Tile center, SC_Tile a, SC_Tile b) {

        if (NearDistance (center, a) < NearDistance (center, b))
            return -1;
        else if (NearDistance (center, a) > NearDistance (center, b))
            return 1;
        else {

            float aAngle = Vector3.SignedAngle (Vector3.up, a.transform.position - center.transform.position, Vector3.back);

            float bAngle = Vector3.SignedAngle (Vector3.up, b.transform.position - center.transform.position, Vector3.back);

            return (int) Mathf.Sign ((aAngle < 0 ? 360 + aAngle : aAngle) - (bAngle < 0 ? 360 + bAngle : bAngle));

        }

    }

    public delegate bool IsTileNotable (SC_Tile t);

    public string GetNotableNeighborsName (SC_Tile center, IsTileNotable test) {

        bool left = false;
        bool right = false;
        bool top = false;
        int count = 0;

        foreach (SC_Tile tile in GetTilesAtDistance (tiles, center, 1)) {

            if (test (tile)) {

                if (tile.transform.position.x < center.transform.position.x)
                    left = true;
                else if (tile.transform.position.x > center.transform.position.x)
                    right = true;
                else if (tile.transform.position.y > center.transform.position.y)
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

        if (!rotation.Equals (""))
            rotation = "_" + rotation;

        return count.ToString () + rotation;

    }
    #endregion

    #region Attack
    public List<SC_Tile> GetAttackTiles () {

        return GetAttackTiles(SC_Character.activeCharacter, SC_Character.activeCharacter.Tile);

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

            if (tile.CursorOn && tile.CanAttack)
                uiManager.PreviewFight(SC_Character.activeCharacter.Tile);

        }

    }

    public void PreviewAttack() {

        foreach (SC_Tile t in GetAttackTiles())
            t.SetFilter(TDisplay.Attack, true);

    }

    public List<SC_Hero> HeroesInRange (SC_Hero target, int? range = null) {

        List<SC_Hero> heroesInRange = new List<SC_Hero>();

        foreach (SC_Tile tile in GetRange(target.transform.position, new Vector2 (1, range ?? gameManager.CommonCharactersVariables.relationshipDistance)))
            if (tile.Character?.Hero)
                    heroesInRange.Add(tile.Character.Hero);                

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

        movementPoints[target.Tile] = target.Hero?.MovementPoints ?? target.Movement;

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

                    tile.ChangeDisplay(TDisplay.Movement, preview);

                    foreach (SC_Tile t in GetAttackTiles(target, tile))
                        if (t.CurrentDisplay == TDisplay.None && !movementRange.Contains(t))
                            t.ChangeDisplay(TDisplay.Attack, preview);

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

            int points = parentPoints - tile.Cost;

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

            foreach (SC_Construction construction in FindObjectsOfType<SC_Construction>())
                if (construction.GreatWall)
                    foreach (SC_Tile neighbor in GetRange (construction.transform.position, 1))
                        if (neighbor.Constructable && !constructableTiles.Contains (neighbor))
                            constructableTiles.Add (neighbor);     
            
        } else {

            for (int i = 0; i < regions.Length; i++)
                if (SC_Castle.castles[i])
                    foreach (SC_Tile tile in regions[i])
                        if (tile.Constructable/*(gameManager.QinTurnStarting && tile.Soldier)*/)
                            constructableTiles.Add(tile);

        }

        return constructableTiles;

    }

    public void DisplayConstructableTiles (string c) {

        foreach (SC_Tile tile in GetConstructableTiles(c))
            tile.ChangeDisplay(TDisplay.Construct);

    }

    public void UpdateNeighborWallGraph (SC_Tile center) {

        foreach (SC_Tile tile in GetTilesAtDistance(tiles, center, 1))
            if (tile.GreatWall)
                UpdateWallGraph(tile.Construction);

    }

    public void UpdateWallGraph (SC_Construction construction) {

        string n = GetNotableNeighborsName (construction.Tile, (SC_Tile t) => { return t.GreatWall; });

        construction.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Constructions/" + (construction as SC_Castle ? "Castle" : (construction as SC_Wall ? "Wall" : "Bastion")) + "/" + n);

    }
    #endregion

    #region Display Actions
    public void DisplaySacrifices () {        

        foreach (SC_Soldier soldier in FindObjectsOfType<SC_Soldier>())
            soldier.Tile.ChangeDisplay(TDisplay.Sacrifice);

    }
    #endregion

}
