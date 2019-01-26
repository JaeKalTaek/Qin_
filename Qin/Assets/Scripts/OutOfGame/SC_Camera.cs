using UnityEngine;
using static SC_Game_Manager;

public class SC_Camera : MonoBehaviour {
		
    [Header("Camera Variables")]
    [Tooltip("Speed at which the camera lerps to its target position")]
	public float moveSpeed;

    [Tooltip("List of zooms possible for the camera")]
    public float[] zooms;
    int zoomIndex;

    [Tooltip("Index of the default zoom value in the zooms array")]
    public int defaultZoomIndex;

    [Tooltip("Speed at which the camera lerps to its target zoom")]
    public float zoomSpeed;

    /*[Tooltip("Speed at which the camera lerps to its target position when the player is zooming wider")]
    public float widerZoomSpeedMultiplier;*/

    [Tooltip("Distance between the border of the cursor and the border of the camera (except when the camera is at the border of the board)")]
    public float cursorMargin;

    [Tooltip("Margin between the board and the camera border")]
    public float boardMargin;

    Vector3 CursorPos { get { return SC_Cursor.Instance.transform.position; } }

    Vector3 targetPos;

    Camera cam;

    private void OnValidate () {

        defaultZoomIndex = Mathf.Clamp(defaultZoomIndex, 0, zooms.Length - 1);

    }

    public void Setup(int sizeX, int sizeY) {

        cam = GetComponent<Camera>();

        zoomIndex = defaultZoomIndex;

        cam.orthographicSize = zooms[zoomIndex] * TileSize;

        transform.position = ClampedPos(CursorPos + Vector3.forward * -16);

    }

    void Update() {        

        if (cam) {

            zoomIndex = Mathf.Clamp(zoomIndex - Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel")), 0, zooms.Length - 1);

            if (cam.orthographicSize != (zooms[zoomIndex] * TileSize))
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zooms[zoomIndex] * TileSize, zoomSpeed * Time.deltaTime);

            Vector3 topRight = CursorCornerCamPos(true);
            Vector3 bottomLeft = CursorCornerCamPos(false);

            targetPos = ClampedPos(targetPos + new Vector3(topRight.x > 1 ? 1 : bottomLeft.x < 0 ? -1 : 0, topRight.y > 1 ? 1 : bottomLeft.y < 0 ? -1 : 0, 0));

            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

        }        

    }

    Vector3 CursorCornerCamPos (bool sign) {

        float f = ((.5f + cursorMargin) * TileSize) * (sign ? 1 : -1);

        Vector3 oldPos = transform.position;

        transform.position = targetPos;

        Vector3 returnValue = cam.WorldToViewportPoint(CursorPos + new Vector3(f, f, 0));

        transform.position = oldPos;

        return returnValue;

    }

    Vector3 ClampedPos(Vector3 p) {        

        float xMax = (SC_Tile_Manager.Instance.xSize - cam.orthographicSize * cam.aspect + boardMargin) * TileSize;
        float xMin = (cam.orthographicSize * cam.aspect - boardMargin) * TileSize;

        float x = (CursorPos.x == 0) ? xMin : (CursorPos.x.I() == SC_Tile_Manager.Instance.xSize - 1) ? xMax : Mathf.Clamp(p.x, xMin, xMax);

        float yMax = (SC_Tile_Manager.Instance.ySize - cam.orthographicSize + boardMargin) * TileSize;
        float yMin = (cam.orthographicSize - boardMargin) * TileSize;

        float y = (CursorPos.y == 0) ? yMin : (CursorPos.y.I() == SC_Tile_Manager.Instance.ySize - 1) ? yMax : Mathf.Clamp(p.y, yMin, yMax);
    
        return new Vector3(x, y, -16);

    }

}