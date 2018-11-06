using UnityEngine;
using UnityEngine.Networking;

public class SC_Lobby_Player : NetworkLobbyPlayer {

    public override void OnStartLocalPlayer () {        

        SendReadyToBeginMessage ();

	}

}
