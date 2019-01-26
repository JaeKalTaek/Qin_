using UnityEngine;
using UnityEngine.Networking;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	public bool Qin;

    public bool Turn { get { return Qin == localPlayer.gameManager.Qin; } }

	SC_Game_Manager gameManager;

	SC_Tile_Manager tileManager;

    SC_UI_Manager uiManager;

    SC_Fight_Manager FightManager { get { return SC_Fight_Manager.Instance; } }

	public static SC_Player localPlayer;

    public bool Busy { get; set; }

    public bool Ready { get; set; }

	public override void OnStartLocalPlayer () {

        SetSide();

        tag = "Player";

		gameManager = FindObjectOfType<SC_Game_Manager> ();

		if(gameManager)
			gameManager.Player = this;

		if(FindObjectOfType<SC_Tile_Manager> ())
			tileManager = FindObjectOfType<SC_Tile_Manager> ();

        uiManager = SC_UI_Manager.Instance;

        localPlayer = this;
		
	}

    #region Commands

    #region Connecting
    [Command]
    public void CmdFinishConnecting() {        

        RpcFinishConnecting();

    }

    [ClientRpc]
    void RpcFinishConnecting() {

        //Instantiate(Resources.Load<GameObject>("Prefabs/P_Cursor"));

        SC_Cursor.SetLock(false);

        localPlayer.uiManager.connectingPanel.SetActive(false);

    }
    #endregion

    #region Ready
    [Command]
    public void CmdReady (bool ready, bool qin) {

        RpcReady(ready, qin);

    }

    [ClientRpc]
    void RpcReady (bool ready, bool qin) {

        if (localPlayer.Qin != qin) {

            localPlayer.uiManager.SetReady(localPlayer.uiManager.otherPlayerReady, ready);

            if (ready && localPlayer.Ready)
                localPlayer.CmdBothPlayersReady();

        }

    }

    [Command]
    void CmdBothPlayersReady () {

        RpcBothPlayersReady();

    }

    [ClientRpc]
    void RpcBothPlayersReady () {

        localPlayer.uiManager.Load();

        localPlayer.gameManager.Load();        

    }
    #endregion

    #region Castle changes tile type
    [Command]
    public void CmdChangeCastleType(GameObject castle, string newType, int newSprite) {

        RpcChangeCastleType(castle, newType, newSprite);

    }

    [ClientRpc]
    void RpcChangeCastleType(GameObject castle, string newType, int newSprite) {

        castle.GetComponent<SC_Castle>().SetCastle(newType, newSprite);

    }
    #endregion

    #region Characters movements
    [Command]
    public void CmdCheckMovements(int x, int y) {

        RpcCheckMovements(x, y);

    }

    [ClientRpc]
    void RpcCheckMovements(int x, int y) {

        localPlayer.tileManager.CheckMovements(localPlayer.tileManager.GetTileAt(x, y).Character);

    }

    [Command]
    public void CmdMoveCharacterTo(int x, int y) {

        RpcMoveCharacterTo(x, y);

    }

    [ClientRpc]
    void RpcMoveCharacterTo(int x, int y) {

        SC_Character.characterToMove.MoveTo(localPlayer.tileManager.GetTileAt(x, y));

    }

    [Command]
    public void CmdResetMovement() {

        RpcResetMovement();

    }

    [ClientRpc]
    void RpcResetMovement() {

        SC_Character.characterToMove.ResetMovementFunction();

    }
    #endregion

    #region Attack
    [Command]
	public void CmdPrepareForAttack(int attackRange, GameObject targetTileObject) {

        RpcPrepareForAttack(attackRange, targetTileObject);

    }

	[ClientRpc]
	void RpcPrepareForAttack(int attackRange, GameObject targetTileObject) {

        localPlayer.FightManager.AttackRange = attackRange;

        SC_Character.attackingCharacter.AttackTarget = targetTileObject.GetComponent<SC_Tile>();

	}

    [Command]
    public void CmdAttack() {

        RpcAttack();

    }

    [ClientRpc]
    void RpcAttack() {

        localPlayer.FightManager.Attack();

    }

    [Command]
    public void CmdHeroAttack(bool usedActiveWeapon) {

        RpcHeroAttack(usedActiveWeapon);

    }

    [ClientRpc]
    void RpcHeroAttack(bool usedActiveWeapon) {

        SC_Hero.Attack(usedActiveWeapon);

    }
    #endregion

    #region Remove filters
    /*[Command]
	public void CmdRemoveAllFilters() {

		RpcRemoveAllFilters ();

	}    

    [ClientRpc]
	void RpcRemoveAllFilters() {

        localPlayer.tileManager.RemoveAllFilters();

    }*/

    /*[Command]
    public void CmdRemoveAllFiltersOnClient(bool qin) {

        RpcRemoveAllFiltersForClient(qin);

    }

    [ClientRpc]
    void RpcRemoveAllFiltersForClient(bool qin) {

        if(localPlayer.Qin == qin)
            localPlayer.tileManager.RemoveAllFilters();

    }*/
    #endregion

    #region Next Turn
    [Command]
	public void CmdNextTurn() {
		
		RpcNextTurn ();

	}
    
    [ClientRpc]
	void RpcNextTurn() {  

		localPlayer.gameManager.NextTurnFunction ();

	}
    #endregion

    #region Construction
    [Command]
    public void CmdSetConstru (string c) {

        RpcSetConstru(c);

    }

    [ClientRpc]
    public void RpcSetConstru (string c) {

        localPlayer.gameManager.CurrentConstru = c;

    }

    [Command]
	public void CmdConstructAt(int x, int y) {

        RpcConstructAt(x, y);

    }

    [ClientRpc]
    public void RpcConstructAt(int x, int y) {

        localPlayer.gameManager.ConstructAt(x, y);

    }

    [Command]
    public void CmdFinishConstruction (bool qinConstru) {

        RpcFinishConstruction(qinConstru);

    }

    [ClientRpc]
    public void RpcFinishConstruction (bool qinConstru) {

        if(localPlayer.Qin)
            localPlayer.gameManager.FinishConstruction(qinConstru);

    }

    [Command]
    public void CmdSetLastConstru (GameObject g) {

        RpcSetLastConstru(g);

    }

    [ClientRpc]
    public void RpcSetLastConstru (GameObject g) {

        SC_Construction.lastConstru = g.GetComponent<SC_Construction>();

    }

    [Command]
    public void CmdCancelLastConstru () {

        RpcCancelLastConstru();

    }

    [ClientRpc]
    public void RpcCancelLastConstru () {

        SC_Construction.CancelLastConstruction();

    }
    #endregion

    #region Change Qin Energy
    [Command]
	public void CmdChangeQinEnergy(int amount) {

		RpcChangeQinEnergy (amount);

	}

	[ClientRpc]
	void RpcChangeQinEnergy(int amount) {

		SC_Qin.ChangeEnergy (amount);

	}

    [Command]
    public void CmdChangeQinEnergyOnClient (int amount, bool qin) {

        RpcChangeQinEnergyOnClient(amount, qin);

    }

    [ClientRpc]
    void RpcChangeQinEnergyOnClient (int amount, bool qin) {

        if (localPlayer.Qin == qin)
            SC_Qin.ChangeEnergy(amount);

    }
    #endregion

    #region Destroy Character
    [Command]
    public void CmdDestroyCharacter(GameObject c) {

        RpcDestroyCharacter(c);

    }

    [ClientRpc]
    void RpcDestroyCharacter(GameObject c) {

        c.GetComponent<SC_Character>().DestroyCharacter();

    }
    #endregion

    #region Destroy Production Building
    [Command]
    public void CmdDestroyProductionBuilding( ) {

        RpcDestroyProductionBuilding();

    }

    [ClientRpc]
    void RpcDestroyProductionBuilding () {

        localPlayer.gameManager.FinishAction();

        SC_Character.attackingCharacter.Tile.Construction?.DestroyConstruction();

    }
    #endregion

    #region Destroy Construction
    [Command]
    public void CmdDestroyConstruction (GameObject c) {

        RpcDestroyConstruction(c);

    }

    [ClientRpc]
    void RpcDestroyConstruction (GameObject c) {

        c.GetComponent<SC_Construction>().DestroyConstruction();

    }
    #endregion

    #region Create Soldier
    [Command]
    public void CmdCreateSoldier(Vector3 pos, string soldierName) {

        localPlayer.gameManager.CreateSoldier(pos, soldierName);

    }

    [Command]
    public void CmdSetupNewSoldier (GameObject g) {

        RpcSetupnewSoldier(g);

    }

    [ClientRpc]
    void RpcSetupnewSoldier (GameObject g) {

        g.GetComponent<SC_Soldier>().SetupNew();

    }
    #endregion

    #region Create Demon
    [Command]
    public void CmdCreateDemon(GameObject castle) {

        localPlayer.gameManager.CurrentCastle = castle.GetComponent<SC_Castle>();

        localPlayer.gameManager.CreateDemonFunction();

    }
    #endregion

    #region Sacrifice Castle
    [Command]
    public void CmdSacrificeCastle (GameObject castle) {

        RpcSacrificeCastle(castle);

    }

    [ClientRpc]
    void RpcSacrificeCastle (GameObject castle) {

        localPlayer.gameManager.SacrificeCastle(castle.GetComponent<SC_Castle>());

    }
    #endregion

    #region Wait
    [Command]
    public void CmdFinishCharacterAction() {

        RpcFinishCharacterAction();

    }

    [ClientRpc]
    void RpcFinishCharacterAction() {

        SC_Character.FinishCharacterAction();

    }
    #endregion

    #region Set Qin Turn Starting
    [Command]
    public void CmdSetQinTurnStarting(bool b) {

        RpcSetQinTurnStarting(b);

    }

    [ClientRpc]
    public void RpcSetQinTurnStarting (bool b) {

        localPlayer.gameManager.QinTurnStarting = b;

    }
    #endregion

    #region Victory
    [Command]
    public void CmdShowVictory(bool qinWon) {

        RpcShowVictory(qinWon);

    }

    [ClientRpc]
    void RpcShowVictory(bool qinWon) {

        localPlayer.uiManager.ShowVictory(qinWon);

    }
    #endregion

    [Command]
	public void CmdDestroyGameObject(GameObject go) {

		if(go && (go.name != "dead")) {

			go.name = "dead";

			NetworkServer.Destroy (go);

		}

	}
	#endregion

	public void SetGameManager(SC_Game_Manager gm) {

		gameManager = gm; 

	}

	public void SetTileManager(SC_Tile_Manager tm) {

		tileManager = tm;

	}

	public void SetSide() {

        Qin = FindObjectOfType<SC_Network_Manager>().IsQinHost() == isServer;

    }

}
