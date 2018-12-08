﻿public class SC_Workshop : SC_Construction {

    protected override void Start () {

        base.Start();

        transform.parent = uiManager.workshopsT;

    }

    public void SelectWorkshop() {

        gameManager.CurrentWorkshopPos = transform.position;

        if (gameManager.QinTurnStarting)
            SC_Player.localPlayer.CmdSetQinTurnStarting(false);

        uiManager.DisplayWorkshopPanel();

	}

}
