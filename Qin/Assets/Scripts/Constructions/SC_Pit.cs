public class SC_Pit : SC_Construction {

    protected override void Start () {

        base.Start();

        transform.parent = uiManager.pitsT;

    }

    public void SelectPit() {

        gameManager.CurrentPitPos = transform.position;

        if (gameManager.QinTurnStarting)
            SC_Player.localPlayer.CmdSetQinTurnStarting(false);

        uiManager.DisplayPitPanel();

	}

}
