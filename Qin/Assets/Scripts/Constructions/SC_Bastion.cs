public class SC_Bastion : SC_Construction {

    protected override void Start() {

        base.Start();

        tileManager.UpdateWallGraph(gameObject);

        tileManager.UpdateNeighborWallGraph(Tile);

        transform.parent = uiManager.bastionsT;

    }

    public override void DestroyConstruction () {

		gameObject.SetActive (false);

		base.DestroyConstruction ();

        tileManager.UpdateNeighborWallGraph (Tile);

	}

}
