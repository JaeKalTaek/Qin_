using UnityEngine;
using UnityEngine.Networking;
using static SC_Game_Manager;

public class SC_Cursor : NetworkBehaviour {

    [Header("Cursor variables")]
    [Tooltip("Distance needed for the mouse to move to make the mouse cursor visible again, and snap the cursor to it")]
    public float mouseThreshold;

    [Tooltip("Delay between two movements of the cursor when using keys")]
    public float inputsMoveDelay;
    float inputsMoveTimer;

    [Tooltip("Distance between the border of the cursor and the border of the camera (except when the camera is at the border of the board)")]
    public float cursorMargin;    

    public bool Locked { get; set; }

    Vector3 oldMousePos, newMousePos;

    bool cameraMoved;

    SC_Camera cam;

    Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

    public static SC_Cursor Instance { get; set; }

    private void OnValidate () {

        if (cursorMargin < 0)
            cursorMargin = 0;

    }

    void Start() {

        Instance = this;

        cam = FindObjectOfType<SC_Camera>();

        oldMousePos = SC_Global.WorldMousePos;

        newMousePos = oldMousePos;

        inputsMoveTimer = 0;

        SC_Tile_Manager.Instance.GetTileAt(transform.position).CursorOn = true;

    }

    void Update () {

        #region Cursor Movement
        inputsMoveTimer -= Time.deltaTime;

        oldMousePos = newMousePos;

        newMousePos = SC_Global.WorldMousePos;

        if ((Vector3.Distance(oldMousePos, newMousePos) >= mouseThreshold) && !cameraMoved)
             Cursor.visible = true;

        cameraMoved = false;

        if (!Locked) {

            Vector3 oldPos = transform.position;

            Vector3 newPos = -Vector3.one;            

            if ((Input.GetButton("Horizontal") || Input.GetButton("Vertical")) && (inputsMoveTimer <= 0)) {

                inputsMoveTimer = inputsMoveDelay;

                Cursor.visible = false;

                newPos = transform.position + new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0) * TileSize;

            } else if (Cursor.visible && screenRect.Contains(Input.mousePosition)) {

                newPos = SC_Global.WorldMousePos;

            }            

            int x = newPos.x.I();
            int y = newPos.y.I();

            if ((x >= 0) && (y >= 0) && (x < SC_Tile_Manager.Instance.xSize) && (y < SC_Tile_Manager.Instance.ySize))
                transform.SetPos(new Vector2(x, y) * TileSize);

            /*cam.minX = x == 0;
            cam.maxX = x == SC_Tile_Manager.Instance.xSize;
            cam.minY = y == 0;
            cam.maxY = y == SC_Tile_Manager.Instance.ySize;*/

            if (oldPos != transform.position) {

                SC_Tile_Manager.Instance?.GetTileAt(oldPos)?.OnCursorExit();

                SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.OnCursorEnter();                

                if (!Cursor.visible) {

                    Vector3 oldCamPos = cam.TargetPosition;

                    Vector3 topRight = CursorCamPos(true);
                    Vector3 bottomLeft = CursorCamPos(false);

                    cam.TargetPosition += new Vector3(topRight.x > 1 ? 1 : bottomLeft.x < 0 ? -1 : 0, topRight.y > 1 ? 1 : bottomLeft.y < 0 ? -1 : 0, 0) * TileSize;

                    cameraMoved = oldCamPos != cam.TargetPosition;

                }

            }            

        }
        #endregion       

        #region Cursor Inputs
        if (Input.GetButtonDown("Action"))
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorClick();           
        /*else if (Input.GetButtonDown("Infos"))
            SC_Tile_Manager.Instance?.GetTileAt(transform.position)?.CursorSecondaryClick();*/
        #endregion
    }

    Vector3 CursorCamPos(bool sign) {

        float f = ((TileSize / 2) + cursorMargin) * (sign ? 1 : -1);

        return Camera.main.WorldToViewportPoint(transform.position + new Vector3(f, f, 0));

    }    

}
