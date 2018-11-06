using UnityEngine;
using UnityEngine.Networking;

/*public class SC_Convoy : NetworkBehaviour {

	SC_GameManager gameManager;
	SC_Tile_Manager tileManager;

	void Start () {

		gameManager = FindObjectOfType<SC_GameManager> ();

		tileManager = FindObjectOfType<SC_Tile_Manager> ();

		SC_Tile under = tileManager.GetTileAt (gameObject);

		under.constructable = false;
		under.attackable = false;

	}

	public void MoveConvoy() {

		Vector3 targetPos = (transform.position + new Vector3 (-1, 0, 0));

		if(tileManager.GetTileAt (targetPos).IsEmpty()) {

			SC_Tile leavingTile = tileManager.GetTileAt (gameObject);
			leavingTile.constructable = !leavingTile.IsPalace();
			leavingTile.canSetOn = true;
			leavingTile.attackable = true;

			transform.position = targetPos;

			if (targetPos.x >= 0) {

				SC_Tile posTile = tileManager.GetTileAt (targetPos); 
				posTile.constructable = false;
				posTile.canSetOn = !gameManager.CoalitionTurn();
				posTile.attackable = false;

			} else {

				Destroy (gameObject);

			}

		}

	}

	public void DestroyConvoy() {

		tileManager.GetTileAt (gameObject).constructable = true;

		SC_Qin.ChangeEnergy (50);

		Destroy (gameObject);

	}

}*/
