using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Qin : NetworkBehaviour {

	[Header("Qin variables")]
	[Tooltip("Energy of Qin at the start of the game")]
	public int startEnergy;
	public static int Energy { get; set; }

	[Tooltip("Energy necessary for Qin to win the game")]
	public int energyToWin;

	[Tooltip("Cost of the power of Qin")]
	public int powerCost;

	[Tooltip("Energy won when a hero dis")]
	public int energyWhenHeroDies;    

    [Tooltip("Energy won for each village at the beginning of each of Qin's turn")]
    public int regenPerVillage;

    //static SC_Game_Manager gameManager;

	static SC_Tile_Manager tileManager;

	static SC_UI_Manager uiManager;

	public static SC_Qin Qin;

	void Start() {

		Qin = this;

		//gameManager = FindObjectOfType<SC_Game_Manager> ();

		tileManager = FindObjectOfType<SC_Tile_Manager> ();

		uiManager = FindObjectOfType<SC_UI_Manager> ();

        Energy = startEnergy;

		uiManager.qinEnergy.text = "Qin's Energy : " + Energy;

        tileManager.GetTileAt(gameObject).Qin = this;

        tileManager.GetTileAt(transform.position + Vector3.up).Qin = this;

        transform.SetPos(transform.position, 0);

    }

	/*public static void UsePower(Vector3 pos) {

		SC_Hero hero = gameManager.LastHeroDead;

		hero.transform.SetPos(pos);
		hero.Qin = true;
		hero.PowerUsed = false;
		hero.PowerBacklash = 0;
		hero.BaseColor = new Color (255, 0, 205);
		hero.Health = hero.maxHealth;
		hero.Lifebar.UpdateGraph(hero.Health, hero.maxHealth);
        hero.CanMove = true;
		hero.Berserk = false;
		hero.BerserkTurn = false;
		hero.UnTired ();

		Quaternion rotation = Quaternion.identity;
		rotation.eulerAngles = new Vector3(0, 0, 180);

		Quaternion lifebarRotation = Quaternion.identity;
		lifebarRotation.eulerAngles = hero.Lifebar.transform.parent.rotation.eulerAngles;

		hero.transform.rotation = rotation;
		hero.Lifebar.transform.parent.rotation = lifebarRotation;

		hero.gameObject.SetActive (true);

		ChangeEnergy(-Qin.powerCost);

		gameManager.LastHeroDead = null;

	}*/

	public static void ChangeEnergy(int amount) {

		Energy += amount;

        if (Energy >= Qin.energyToWin)
            uiManager.ShowVictory(true);
        else if (Energy > 0) {
            uiManager.qinEnergy.text = "Qin's Energy : " + Energy;
            uiManager.TryRefreshInfos(Qin.gameObject, Qin.GetType());
        }  else
            uiManager.ShowVictory(false);

	}    

    public static int GetConstruCost (string s) {

        s = s.Replace(" ", "");

        return Resources.Load<SC_Construction>("Prefabs/Constructions/P_" + s)?.cost ?? Resources.Load<SC_Construction>("Prefabs/Constructions/Production/P_" + s).cost;

    }

    public static void SendQinInfos () {

        CastleDeck[] castleDecks = new CastleDeck[6];

        for (int i = 0; i < 6; i++) {

            castleDecks[i] = new CastleDeck (

                SC_UI_Manager.Instance.qinPreprationUI.castleDecks[i].Castle.Renderer.sprite.name.Replace("Castle", ""),

                SC_UI_Manager.Instance.qinPreprationUI.castleDecks[i].Trap.Renderer.sprite.name

            );

        }

        SC_DeploymentSoldier[] deploymentSoldiers = FindObjectsOfType<SC_DeploymentSoldier> ();

        SoldierInfos[] soldierInfos = new SoldierInfos[deploymentSoldiers.Length];

        for (int i = 0; i < soldierInfos.Length; i++) {

            soldierInfos[i] = new SoldierInfos (

                deploymentSoldiers[i].transform.position,

                deploymentSoldiers[i].SpriteR.sprite.name
                
            );

        }

        foreach (SC_DeploymentSoldier s in deploymentSoldiers)
            Destroy (s.gameObject);

        SC_Player.localPlayer.CmdSendQinInfos (castleDecks, SC_UI_Manager.Instance.qinPreprationUI.curseSlot.Renderer.sprite.name, soldierInfos);

    }

}
