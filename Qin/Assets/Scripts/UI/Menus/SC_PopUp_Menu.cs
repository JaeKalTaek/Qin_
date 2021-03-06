﻿using UnityEngine;

public class SC_PopUp_Menu : MonoBehaviour {

    void OnEnable () {

        RectTransform Rect = GetComponent<RectTransform>();

        Vector3 currentTileViewportPos = Camera.main.WorldToViewportPoint(SC_Cursor.Instance.transform.position);

        Rect.anchorMin = new Vector3(currentTileViewportPos.x + ((currentTileViewportPos.x < 0.5 ? 1 : -1) * (0.1f + (0.05f * (1 / (Mathf.Pow(Camera.main.orthographicSize, Camera.main.orthographicSize / 4)))))), .5f, 0);
        Rect.anchorMax = Rect.anchorMin;

    }

}
