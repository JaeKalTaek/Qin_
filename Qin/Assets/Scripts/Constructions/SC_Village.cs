public class SC_Village : SC_Construction {

	public static int number;

	protected override void Start() {

		base.Start ();

		number++;

        transform.parent = uiManager.villagesT;

	}

	public override void DestroyConstruction(bool playSound) {

		base.DestroyConstruction (playSound);

		number--;

		//gameManager.SpawnConvoy (transform.position + new Vector3 (-1, 0, 0));

	}

}
