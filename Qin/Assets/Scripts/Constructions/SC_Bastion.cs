public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        tileManager.UpdateWallGraph(this);

        tileManager.UpdateNeighborWallGraph(Tile);

        transform.parent = uiManager.bastionsT;

    }

    public override void DestroyConstruction (bool playSound) {

		gameObject.SetActive (false);

		base.DestroyConstruction (playSound);

        tileManager.UpdateNeighborWallGraph (Tile);

	}

}
