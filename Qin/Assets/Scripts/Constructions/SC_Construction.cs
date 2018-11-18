using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_Construction : NetworkBehaviour {

    [Header("Constructions Variables")]
    [Tooltip("Name of the construction")]
    public string Name;

    [Tooltip("Base maximum health of the construction, put 0 for a construction who doesn't have health")]
    public int maxHealth;

    public int Health { get; set; }

    [Tooltip("Cost for Qin to build this construction")]
    public int cost;

    [Tooltip("Is this a Production Construction")]
    public bool production;

    [Tooltip("Combat modifiers for this construction")]
    public SC_CombatModifiers combatModifers;

    [Tooltip("Description of the construction")]
    public string description;

    public SC_Lifebar Lifebar { get; set; }

    public bool GreatWall { get { return (this as SC_Castle) || (this as SC_Bastion) || (this as SC_Wall); } }

    public SC_Pump Pump { get { return this as SC_Pump; } }

    public SC_Ruin Ruin { get { return this as SC_Ruin; } }

    public SC_Tile Tile { get { return tileManager.GetTileAt(gameObject);  } }

	protected static SC_Game_Manager gameManager;

	protected static SC_Tile_Manager tileManager;

	protected static SC_UI_Manager uiManager;

    public static SC_Construction lastConstru;

    public static SC_Soldier lastConstruSoldier;

    protected void Awake () {

        if (!tileManager)
            tileManager = FindObjectOfType<SC_Tile_Manager>();

        if (tileManager && (tileManager.tiles != null))
            Tile.Construction = this;

    }

    protected virtual void Start () {

		if (!gameManager)
			gameManager = FindObjectOfType<SC_Game_Manager> ();		

		if (!uiManager)
			uiManager = FindObjectOfType<SC_UI_Manager> ();

		Health = maxHealth;

        if(Health != 0) {

            Lifebar = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/P_Lifebar"), transform).GetComponent<SC_Lifebar>();
            Lifebar.transform.position += new Vector3(0, -.44f, 0);

        }

        Tile.Construction = this;

        Tile.Cost = 1;

    }

	public virtual void DestroyConstruction() {

        if (GreatWall)
            Tile.Soldier?.DestroyCharacter();

        Tile.Construction = null;

        uiManager.TryRefreshInfos(Tile.gameObject, Tile.GetType());

        Tile.Cost = Tile.baseCost;

        Destroy(gameObject);

        //SC_Player.localPlayer.CmdDestroyGameObject(gameObject);

		/*if(isServer)
			Network.Destroy (gameObject);*/

	}

    public static void CancelLastConstruction () {

        tileManager.RemoveAllFilters();

        lastConstru.gameObject.SetActive(false);

        lastConstru.DestroyConstruction();

        if (lastConstruSoldier) {

            SC_Qin.ChangeEnergy(-lastConstruSoldier.sacrificeValue);

            lastConstruSoldier.gameObject.SetActive(true);

            lastConstruSoldier.Tile.Character = lastConstruSoldier;

        }                

        if (SC_Player.localPlayer.Qin) {

            if (!gameManager.QinTurnStarting) {

                SC_Qin.ChangeEnergy(SC_Qin.GetConstruCost(lastConstru.Name));

                SC_Player.localPlayer.CmdChangeQinEnergyOnClient(SC_Qin.GetConstruCost(lastConstru.Name), false);

            }

            uiManager.UpdateCreationPanel(uiManager.qinConstrus);

            SC_Player.localPlayer.Busy = true;

            if (!SC_Cursor.Instance.Locked) {

                if (CanCreateConstruct(gameManager.CurrentConstru))
                    tileManager.DisplayConstructableTiles(gameManager.CurrentConstru);
                else
                    uiManager.SelectConstruct();

            }

        }

        lastConstru = null;

        lastConstruSoldier = null;        

    }

}
