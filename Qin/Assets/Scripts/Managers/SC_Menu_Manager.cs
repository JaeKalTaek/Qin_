using UnityEngine;
using static SC_Global;
using System.Collections.Generic;

public class SC_Menu_Manager : MonoBehaviour {

    #region Variables

    static SC_UI_Manager uiManager;

    public SC_Tile_Manager TileManager { get; set; }

    public static SC_Menu_Manager Instance { get; set; }

    public enum actionMenu {Player, Character};

    public GameObject PanelMenu;

    public GameObject ButtonMenu;

    List<Actions> actionsToDisplay = new List<Actions>();

    Dictionary<Actions, int> ActionsIndex = new Dictionary<Actions, int>();

    GameObject menu; 

    #endregion

    #region Setup

    // Use this for initialization
    void Start () {
 
        uiManager = SC_UI_Manager.Instance;

    }

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region Menu Management

    public void DisplayActions(List<Actions> actions)
    {
        int actionIndex;

        actionsToDisplay = actions;

        foreach(Actions action in actionsToDisplay)
        {
            //actionIndex = ActionsIndex.TryGetValue(action);
        }
    }

    //Move the menu next to the tile
    public void MenuPos(actionMenu type)
    {
        switch (type)
        {
            case actionMenu.Character:
                menu = uiManager.characterActionsPanel;
                break;
            case actionMenu.Player:
                menu = uiManager.playerActionsPanel;
                break;
        }

        RectTransform Rect = menu.GetComponent<RectTransform>();

        //Get the viewport position of the tile
        Vector3 currentTileViewportPos = Camera.main.WorldToViewportPoint(TileManager.GetTileAt(SC_Cursor.Instance.gameObject).transform.position);

        //If tile on the left side of the screen, offset the menu on the right
        //If tile on the right side of the screen, offset the menu on the left
        int offset = currentTileViewportPos.x < 0.5 ? 1 : -1;

        Rect.anchorMin = new Vector3(currentTileViewportPos.x + (offset * (0.1f + (0.05f * (1 / (Mathf.Pow(Camera.main.orthographicSize, Camera.main.orthographicSize / 4)))))), currentTileViewportPos.y, currentTileViewportPos.z);
        Rect.anchorMax = Rect.anchorMin;

        menu.SetActive(true);
    }

    #endregion

}
