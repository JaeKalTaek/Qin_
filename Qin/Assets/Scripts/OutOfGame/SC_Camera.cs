using UnityEngine;
using static SC_Game_Manager;
using static SC_Global;

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

    [Tooltip("Speed at which the camera lerps to its target position when the player is zooming wider")]
    public float widerZoomSpeedMultiplier;

    [Header("Mouse Cursor")]
    [Tooltip("Distance between the mouse and the border of the camera for the camera to move")]
    public float mouseMargin;

    [Tooltip("Speed at which the camera moves when \"pushed\" by the mouse")]
    public float mouseCameraSpeed;

    [Tooltip("Maximum distance you can push the camera to using the mouse")]
    public float maxMouseMovement;

    float MaxMouseMovement { get { return maxMouseMovement * TileSize; } }

    /*[Tooltip("Margin between the board and the camera border")]
    public float boardMargin;*/

    public Vector3 TargetPosition { get; set; }

    /*[HideInInspector]
    public bool minX, maxX, minY, maxY;*/

    Camera cam;

    private void OnValidate () {

        /*if (boardMargin < 0)
            boardMargin = 0;*/

        defaultZoomIndex = Mathf.Clamp(defaultZoomIndex, 0, zooms.Length - 1);

    }

    public void Setup(int sizeX, int sizeY) {

        cam = GetComponent<Camera>();

		transform.position = new Vector3 (((sizeX - 1) / 2) * TileSize, ((sizeY - 1) / 2) * TileSize, -16);

        TargetPosition = transform.position;

        zoomIndex = defaultZoomIndex;

        cam.orthographicSize = zooms[zoomIndex];

    }

    void Update() {        

        if (cam) {

            int previousZoomIndex = zoomIndex;

            zoomIndex = Mathf.Clamp(zoomIndex - Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel")), 0, zooms.Length - 1);

            if (cam.orthographicSize != zooms[zoomIndex])
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zooms[zoomIndex], zoomSpeed * Time.deltaTime);

            /*float xMax = SC_Tile_Manager.Instance.xSize * TileSize - cam.orthographicSize * cam.aspect - .5f + boardMargin;
            float xMin = cam.orthographicSize* cam.aspect - .5f - boardMargin;

            float x = Mathf.Clamp(TargetPosition.x, xMin, xMax);

            x = minX ? xMin : maxX ? xMax : x;

            float yMax = SC_Tile_Manager.Instance.ySize * TileSize - cam.orthographicSize - .5f + boardMargin;
            float yMin = cam.orthographicSize - .5f - boardMargin;

            float y = Mathf.Clamp(TargetPosition.y, yMin, yMax);

            y = minY ? yMin : maxY ? yMax : y;

            TargetPosition = new Vector3(x, y, -16);*/

            if (Cursor.visible && (WorldMousePos.x > -MaxMouseMovement) && (WorldMousePos.y > -MaxMouseMovement) &&
                (WorldMousePos.x < (SC_Tile_Manager.Instance.xSize + maxMouseMovement) * TileSize) && (WorldMousePos.y < (SC_Tile_Manager.Instance.ySize + maxMouseMovement) * TileSize)) {

                Vector3 topRight = MouseCamPos(true);
                Vector3 bottomLeft = MouseCamPos(false);

                float x2 = topRight.x > 1 ? 1 : bottomLeft.x < 0 ? -1 : 0;
                float y2 = topRight.y > 1 ? 1 : bottomLeft.y < 0 ? -1 : 0;

                TargetPosition += new Vector3(x2, y2, 0) * mouseCameraSpeed;

            }

            if (transform.position != TargetPosition) {

                float speed = moveSpeed * Time.deltaTime * (previousZoomIndex < zoomIndex ? widerZoomSpeedMultiplier : 1);

                transform.position = Vector3.Lerp(transform.position, TargetPosition, speed);

            }

        }        

    }

    Vector3 MouseCamPos (bool sign) {

        return Camera.main.WorldToViewportPoint(WorldMousePos + new Vector3(mouseMargin, mouseMargin, 0) * (sign ? 1 : -1));

    }

}