using UnityEngine;
using UnityEngine.Networking;
using static SC_Game_Manager;
using static SC_Global;

public class SC_Cursor : NetworkBehaviour {

    [Header("Cursor variables")]
    [Tooltip("Distance needed for the mouse to move to make the mouse cursor visible again, and snap the cursor to it")]
    public float mouseThreshold;

    [Tooltip("Delay between two movements of the cursor when using keys")]
    public float inputsMoveDelay;
    float inputsMoveTimer;    

    [Tooltip("Distance from the center to move the tile tooltip to the other side of the screen")]
    [Range(.01f, .49f)]
    public float moveTileTooltipDistance;
    bool right;

    public bool Locked { get; set; }

    Vector3 oldMousePos, newMousePos, oldCamPos;

    SC_Camera cam;

    public static SC_Cursor Instance { get; set; }    

    void Start() {

        Locked = true;

        cam = FindObjectOfType<SC_Camera>();

        oldMousePos = WorldMousePos;

        newMousePos = oldMousePos;

    }

    void Update () {    

        #region Set mouse cursor visibility
        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            Cursor.visible = false;      
        
        oldMousePos = newMousePos;

        newMousePos = WorldMousePos;

        if ((Vector3.Distance(oldMousePos, newMousePos) >= mouseThreshold) && (oldCamPos == cam.transform.position))
            Cursor.visible = true;

        oldCamPos = cam.transform.position;
        #endregion

        #region Cursor Movement
        inputsMoveTimer -= Time.deltaTime;

        if (!Locked) {

            Vector3 oldPos = transform.position;

            Vector3 newPos = transform.position;

            if ((Input.GetButton("Horizontal") || Input.GetButton("Vertical")) && (inputsMoveTimer <= 0)) {

                inputsMoveTimer = inputsMoveDelay;

                newPos = transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0) * TileSize;

            } else if (Cursor.visible) {

                newPos = WorldMousePos;

            }            

            int x = newPos.x.I();
            int y = newPos.y.I();

            transform.SetPos(new Vector2(Mathf.Clamp(x, 0, SC_Tile_Manager.Instance.xSize - 1), Mathf.Clamp(y, 0, SC_Tile_Manager.Instance.ySize - 1)) * TileSize);

            if (oldPos != transform.position) {

                SC_Tile_Manager.Instance?.GetTileAt(oldPos)?.OnCursorExit();

                SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.OnCursorEnter();

                SetTileTooltipPos();             

            }            

        }
        #endregion       

        #region Cursor Inputs
        if (!Locked && (Input.GetButtonDown("Submit") || (Input.GetMouseButtonDown(0) && Cursor.visible)))
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorClick();           
        #endregion
    }
    
    public static void SetLock(bool b) {

        Instance.Locked = b;

        Instance.gameObject.SetActive(!b);

        if(b)
            SC_Tile_Manager.Instance?.GetTileAt(Instance.gameObject)?.OnCursorExit();
        else
            SC_Tile_Manager.Instance?.GetTileAt(Instance.gameObject)?.OnCursorEnter();

    }

    void SetTileTooltipPos() {

        float x = Camera.main.WorldToViewportPoint(transform.position).x;

        if (((x < .5f - moveTileTooltipDistance) && right) || ((x > .5f + moveTileTooltipDistance) && !right)) {

            right ^= true;

            RectTransform rectT = SC_UI_Manager.Instance.tileTooltip.panel.GetComponent<RectTransform>();

            rectT.anchoredPosition = new Vector2(right ? 0 : SC_UI_Manager.Size.x  - rectT.sizeDelta.x, rectT.anchoredPosition.y);            

        }

    }

}
