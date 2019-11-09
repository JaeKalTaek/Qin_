using System.Collections;
using UnityEngine;
using static SC_Game_Manager;
using static SC_Global;

public class SC_Camera : MonoBehaviour {
		
    [Header("Camera movement & zoom variables")]
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

    [Tooltip("Distance between the border of the cursor and the border of the camera (except when the camera is at the border of the board), depending on the current zoom")]
    public float[] cursorMargins;

    [Tooltip("Margins between the board and the camera border, depending on the current zoom")]
    public float[] boardMargins;

    [Header("Camera focus variables")]
    [Tooltip("Camera will focus on target if it's out of screen or at this distance of being out of screen")]
    public int tileDistanceFocus;

    [Tooltip ("If the camera centers on the tile to focus or if it just moves enough for it to be included in the in-focus zone")]
    public bool centeredFocus;

    Vector3 CursorPos { get { return (SC_Game_Manager.Instance.prep ? TileUnderMouse.transform : SC_Cursor.Instance.transform).position; } }

    Vector3 targetPos;

    Camera cam;

    public static SC_Camera Instance;

    private void OnValidate () {

        defaultZoomIndex = Mathf.Clamp(defaultZoomIndex, 0, zooms.Length - 1);

    }

    public void Setup () {

        Instance = this;

        cam = GetComponent<Camera>();

        zoomIndex = defaultZoomIndex;

        cam.orthographicSize = zooms[zoomIndex] * TileSize;

        transform.position = ClampedPos(CursorPos - (Vector3.forward * -16f));

    }

    #region Movement
    void Update () {        

        if (cam) {

            zoomIndex = Mathf.Clamp(zoomIndex - Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel")), 0, zooms.Length - 1);

            if (cam.orthographicSize != (zooms[zoomIndex] * TileSize))
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zooms[zoomIndex] * TileSize, zoomSpeed * Time.deltaTime);

            Vector3 topRight = CursorCornerCamPos(true);
            Vector3 bottomLeft = CursorCornerCamPos(false);

            targetPos = ClampedPos(targetPos + new Vector3(topRight.x > 1 ? 1 : bottomLeft.x < 0 ? -1 : 0, topRight.y > 1 ? 1 : bottomLeft.y < 0 ? -1 : 0, 0) * TileSize);

            transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);

        }        

    }

    Vector3 CursorCornerCamPos (bool sign) {

        float f = cursorMargins[zoomIndex] * (sign ? 1 : -1);

        Vector3 oldPos = transform.position;

        transform.position = targetPos;

        Vector3 returnValue = cam.WorldToViewportPoint(CursorPos + new Vector3(f, f, 0));

        transform.position = oldPos;

        return returnValue;

    }

    Vector3 ClampedPos (Vector3 p) {

        float xMax = (XSize + boardMargins[zoomIndex] - .5f) * TileSize - cam.orthographicSize * cam.aspect;
        float prepUIOffset = cam.ViewportToWorldPoint (Vector3.right * (SC_UI_Manager.Instance.heroPreparationUI.decks.GetComponent<RectTransform> ().sizeDelta.x / UISize.x)).x - cam.ViewportToWorldPoint (Vector3.zero).x;
        float xMin = (-boardMargins[zoomIndex] - .5f) * TileSize + cam.orthographicSize * cam.aspect - (SC_Game_Manager.Instance.prep ? prepUIOffset : 0);

        float x = (CursorPos.x == 0) ? xMin : (CursorPos.x.I() == XSize - 1) ? xMax : Mathf.Clamp(p.x, xMin, xMax);

        float yMax = (YSize + boardMargins[zoomIndex] - .5f) * TileSize - cam.orthographicSize;
        float yMin = (-.5f - boardMargins[zoomIndex]) * TileSize + cam.orthographicSize;

        float y = (CursorPos.y == 0) ? yMin : (CursorPos.y.I() == YSize - 1) ? yMax : Mathf.Clamp(p.y, yMin, yMax);

        return new Vector3(x, y, -16);

    }
    #endregion

    #region Focus
    Vector2 TargetOnScreen (Vector3 pos) {

        Vector3 min = cam.WorldToViewportPoint (pos - new Vector3 (1, 1, 0) * (TileSize * (tileDistanceFocus - .5f)));

        Vector3 max = cam.WorldToViewportPoint (pos + new Vector3 (1, 1, 0) * (TileSize * (tileDistanceFocus + .5f)));

        return new Vector2 (min.x < 0 ? -1 : (max.x > 1 ? 1 : 0), min.y < 0 ? -1 : (max.y > 1 ? 1 : 0));

    }

    public bool ShouldFocus (Vector3 pos) {

        return TargetOnScreen (pos).magnitude != 0;

    }

    public void FocusOn (Vector3 pos) {

        if (centeredFocus) {

            targetPos = new Vector3 (pos.x, pos.y, -16f);

        } else {
            
            Vector2 target = TargetOnScreen (pos);

            if (target.x != 0)
                targetPos.x += pos.x - SC_Tile_Manager.Instance.GetTileAt (cam.ViewportToWorldPoint (target.x == 1 ? Vector3.one : Vector3.zero) + Vector3.left * TileSize * Mathf.Sign(target.x) * tileDistanceFocus, true).transform.position.x;

            if (target.y != 0)
                targetPos.y += pos.y - SC_Tile_Manager.Instance.GetTileAt (cam.ViewportToWorldPoint (target.y == 1 ? Vector3.one : Vector3.zero) + Vector3.down * TileSize * Mathf.Sign (target.y) * tileDistanceFocus, true).transform.position.y;

        }

    }

    float ScreenToWorldDistance (Vector3 a, Vector3 b) {

        return Vector3.Distance (cam.ViewportToWorldPoint (a), cam.ViewportToWorldPoint (b));

    }
    #endregion

    #region Camera Shake
    [Header ("Camera shake variables")]
    [Tooltip("Duration of a camera shake")]
    public float shakeDuration;

    [Tooltip("Stragth of a camera shake")]
    public float shakeMagnitude;

    IEnumerator Screenshake () {

        Vector3 initialPosition = transform.localPosition;

        float shakeTimer = shakeDuration;

        while (shakeTimer > 0) { 

            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

            shakeTimer -= Time.deltaTime;

            yield return new WaitForEndOfFrame();

        }

        transform.localPosition = initialPosition;
        
    }
    #endregion

}