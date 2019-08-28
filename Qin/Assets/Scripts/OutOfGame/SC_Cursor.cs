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

    float mouseDist;

    SC_Camera cam;

    public static SC_Cursor Instance { get; set; }    

    public static SC_Tile Tile { get { return SC_Tile_Manager.Instance?.GetTileAt(Instance.gameObject); } }

    void Start() {

        // Locked = true;

        cam = FindObjectOfType<SC_Camera>();

        oldMousePos = WorldMousePos;

        newMousePos = oldMousePos;

        transform.SetPos(transform.position, 1);

    }

    void Update () {

        #region Set mouse cursor visibility
        if (Input.GetButton ("Horizontal") || Input.GetButton ("Vertical")) {

            Cursor.visible = false;

        } else {

            oldMousePos = newMousePos;

            newMousePos = WorldMousePos;

            mouseDist = Cursor.visible ? 0 : mouseDist + Vector3.Distance (oldMousePos, newMousePos);

            if ((mouseDist >= mouseThreshold) && (oldCamPos == cam.transform.position))
                Cursor.visible = true;

            oldCamPos = cam.transform.position;

        }        
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

            transform.SetPos(new Vector3(Mathf.Clamp(x, 0, XSize - 1), Mathf.Clamp(y, 0, YSize - 1), 0) * TileSize);

            #region Cursor has moved
            if (oldPos != transform.position) {

                SC_Sound_Manager.Instance.OnCursorMoved();

                SC_Tile_Manager.Instance?.GetTileAt(oldPos)?.OnCursorExit();

                Tile?.OnCursorEnter();

                SetRightLeftPanels();             

            }
            #endregion

        }
        #endregion       

        #region Cursor Inputs
        if (!Locked && (Input.GetButtonDown("Submit") || (Input.GetMouseButtonDown(0) && Cursor.visible)))
            Tile?.CursorClick();           
        #endregion
    }
    
    public static void SetLock(bool b) {

        Instance.Locked = b;

        Instance.gameObject.SetActive(!b);

        if(b)
            Tile?.OnCursorExit();
        else
            Tile?.OnCursorEnter();

    }

    void SetRightLeftPanels() {

        float x = Camera.main.WorldToViewportPoint(transform.position).x;

        if (((x < .5f - moveTileTooltipDistance) && right) || ((x > .5f + moveTileTooltipDistance) && !right)) {

            right ^= true;

            foreach (RectTransform rectT in new RectTransform[] { SC_UI_Manager.Instance.tileTooltip.panel.GetComponent<RectTransform>(), SC_UI_Manager.Instance.constructPanel.GetComponent<RectTransform>() })
                rectT.anchoredPosition = new Vector2(right ? 0 : SC_UI_Manager.Size.x - rectT.sizeDelta.x, rectT.anchoredPosition.y);

        }

    }

}
