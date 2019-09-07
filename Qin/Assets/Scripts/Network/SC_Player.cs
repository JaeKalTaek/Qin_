﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Character;

public class SC_Player : NetworkBehaviour {

	[SyncVar]
	public bool Qin;

    public bool Turn { get { return Qin == GameManager.QinTurn; } }

	SC_Game_Manager GameManager { get { return SC_Game_Manager.Instance; } }

	SC_Tile_Manager TileManager { get { return SC_Tile_Manager.Instance; } }

    SC_UI_Manager UIManager { get { return SC_UI_Manager.Instance; } }

    SC_Sound_Manager SoundManager { get { return SC_Sound_Manager.Instance; } }

    SC_Fight_Manager FightManager { get { return SC_Fight_Manager.Instance; } }

	public static SC_Player localPlayer;

    public bool Busy { get; set; }

    public bool Ready { get; set; }

    public override void OnStartLocalPlayer () {

        base.OnStartLocalPlayer();

        localPlayer = this;

        StartCoroutine(SetupGameManagerPlayer());

    }

    IEnumerator SetupGameManagerPlayer() {

        while (!GameManager)
            yield return new WaitForEndOfFrame();

        GameManager.Player = this;

    }

    #region Commands

    #region Connecting
    [Command]
    public void CmdFinishConnecting() {        

        RpcFinishConnecting();

    }

    [ClientRpc]
    void RpcFinishConnecting() {

        UIManager.SetupUI(localPlayer.Qin);

        SC_Cursor.Instance = Instantiate(Resources.Load<SC_Cursor>("Prefabs/P_Cursor"));

        if (localPlayer.Qin)
            SC_Cursor.Instance.transform.position = new Vector3(GameManager.CurrentMapPrefab.SizeMapX - 1, GameManager.CurrentMapPrefab.SizeMapY - 1, 0) * SC_Game_Manager.TileSize;
        else
            SC_Cursor.Instance.transform.position = Vector3.zero;

        FindObjectOfType<SC_Camera>().Setup();

        UIManager.connectingPanel.SetActive(false);

        SoundManager.StartCombatMusic(UIManager.musicVolume);

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

            UIManager.SetReady(UIManager.otherPlayerReady, ready);

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

        UIManager.Load();

        localPlayer.GameManager.Load();        

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
    public void CmdMoveCharacterTo(GameObject character, GameObject target) {

        RpcMoveCharacterTo(character, target);

    }

    [ClientRpc]
    void RpcMoveCharacterTo(GameObject character, GameObject target) {

        character.GetComponent<SC_Character>().MoveTo(target.GetComponent<SC_Tile>());

    }

    [Command]
    public void CmdResetMovement() {

        RpcResetMovement();

    }

    [ClientRpc]
    void RpcResetMovement() {

        activeCharacter.ResetMovementFunction();

    }
    #endregion

    #region Attack
    [Command]
    public void CmdSetChainAttack(bool c) {

        RpcSetChainAttack(c);

    }

    [ClientRpc]
    void RpcSetChainAttack(bool c) {

        ChainAttack = c;

    }

    [Command]
    public void CmdAttack(GameObject target) {

        RpcAttack(target);

    }

    [ClientRpc]
    void RpcAttack(GameObject target) {

        activeCharacter.AttackTarget = target.GetComponent<SC_Tile>();

        FightManager.Attack();

    }

    [Command]
    public void CmdHeroAttack(GameObject target, bool usedActiveWeapon) {

        RpcHeroAttack(usedActiveWeapon, target);

    }

    [ClientRpc]
    void RpcHeroAttack(bool usedActiveWeapon, GameObject target) {

        activeCharacter.AttackTarget = target.GetComponent<SC_Tile>();

        SC_Hero.Attack(usedActiveWeapon);

    }

    [Command]
    public void CmdApplyDamage(bool counter) {

        RpcApplyDamage(counter);

    }

    [ClientRpc]
    void RpcApplyDamage(bool counter) {

        FightManager.ApplyDamage(counter);

    }
    #endregion

    /*#region Remove filters
    [Command]
	public void CmdRemoveAllFilters() {

		RpcRemoveAllFilters ();

	}    

    [ClientRpc]
	void RpcRemoveAllFilters() {

        localPlayer.tileManager.RemoveAllFilters();

    }

    /*[Command]
    public void CmdRemoveAllFiltersOnClient(bool qin) {

        RpcRemoveAllFiltersForClient(qin);

    }

    [ClientRpc]
    void RpcRemoveAllFiltersForClient(bool qin) {

        if(localPlayer.Qin == qin)
            localPlayer.tileManager.RemoveAllFilters();

    }
    #endregion*/

    #region Next Turn
    [Command]
	public void CmdNextTurn() {
		
		RpcNextTurn ();

	}
    
    [ClientRpc]
	void RpcNextTurn() {  

		localPlayer.GameManager.NextTurnFunction ();

	}
    #endregion

    #region Construction
    [Command]
    public void CmdSetConstru (string c) {

        RpcSetConstru(c);

    }

    [ClientRpc]
    public void RpcSetConstru (string c) {

        localPlayer.GameManager.CurrentConstru = c;

    }

    [Command]
	public void CmdConstructAt(int x, int y, bool soldier) {

        RpcConstructAt(x, y, soldier);

    }

    [ClientRpc]
    public void RpcConstructAt(int x, int y, bool soldier) {

        localPlayer.GameManager.ConstructAt(x, y, soldier);

    }

    [Command]
    public void CmdFinishConstruction (bool qinConstru) {

        RpcFinishConstruction(qinConstru);

    }

    [ClientRpc]
    public void RpcFinishConstruction (bool qinConstru) {

        if(localPlayer.Qin)
            localPlayer.GameManager.FinishConstruction(qinConstru);

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

        activeCharacter.Tile.Construction?.DestroyConstruction(true);

        localPlayer.GameManager.TryFocusOn (activeCharacter.transform.position);

        activeCharacter.Hero?.Hit(activeCharacter.Hero.ActionCost);        

        localPlayer.GameManager.FinishAction();

    }
    #endregion

    #region Sacrificed Soldier
    [Command]
    public void CmdSacrificedSoldier(int value, Vector3 pos) {

        RpcSacrificedSoldier (value, pos);

    }

    [ClientRpc]
    void RpcSacrificedSoldier(int value, Vector3 pos) {

        SC_Qin.ChangeEnergy (value);

        localPlayer.GameManager.TryFocusOn (pos);

    }
    #endregion

    #region Create Soldier
    [Command]
    public void CmdCreateSoldier(Vector3 pos, string soldierName) {

        localPlayer.GameManager.CreateSoldier(pos, soldierName);

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

        localPlayer.GameManager.CurrentCastle = castle.GetComponent<SC_Castle>();

        localPlayer.GameManager.CreateDemonFunction();

    }
    #endregion

    #region Sacrifice Castle
    [Command]
    public void CmdSacrificeCastle (GameObject castle) {

        RpcSacrificeCastle(castle);

    }

    [ClientRpc]
    void RpcSacrificeCastle (GameObject castle) {

        localPlayer.GameManager.SacrificeCastle(castle.GetComponent<SC_Castle>());

    }
    #endregion

    #region Finish Actions
    [Command]
    public void CmdTryFocus (Vector3 pos) {

        RpcTryFocus (pos);

    }

    [ClientRpc]
    void RpcTryFocus (Vector3 pos) {

        localPlayer.GameManager.TryFocusOn (pos);

    }

    static bool synchro;

    [Command]
    public void CmdSynchroFinishAction () {

        if (synchro)
            RpcFinishAction(false);

        synchro ^= true;

    }

    [Command]
    public void CmdFinishAction() {

        RpcFinishAction(true);

    }

    [ClientRpc]
    void RpcFinishAction(bool wait) {

        GameManager.FinishAction(wait);

    }
    #endregion

    #region Set Qin Turn Starting
    [Command]
    public void CmdSetQinTurnStarting(bool b) {

        RpcSetQinTurnStarting(b);

    }

    [ClientRpc]
    public void RpcSetQinTurnStarting (bool b) {

        localPlayer.GameManager.QinTurnStarting = b;

    }
    #endregion

    #region Victory
    [Command]
    public void CmdShowVictory(bool qinWon) {

        RpcShowVictory(qinWon);

    }

    [ClientRpc]
    void RpcShowVictory(bool qinWon) {

        UIManager.ShowVictory(qinWon);

    }
    #endregion

	#endregion

}
