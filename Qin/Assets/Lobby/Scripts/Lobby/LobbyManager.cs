using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System;

namespace Prototype.NetworkLobby {

    public class LobbyManager : NetworkLobbyManager {

        static short MsgKicked = MsgType.Highest + 1;

        static public LobbyManager s_Singleton;

        [Header("UI Reference")]
        public LobbyTopPanel topPanel;

        public RectTransform mainMenuPanel;
        public RectTransform lobbyPanel;

        public LobbyInfoPanel infoPanel;
        public GameObject addPlayerButton;

        protected RectTransform currentPanel;

        public Button backButton;

        public Text statusInfo;
        public Text hostInfo;

        //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
        //of players, so that even client know how many player there is.
        [HideInInspector]
        public int _playerNumber = 0;

        //used to disconnect a client properly when exiting the matchmaker
        [HideInInspector]
        public bool _isMatchmaking = false;

        protected bool _disconnectServer = false;
        
        protected ulong _currentMatchID;

        protected LobbyHook _lobbyHooks;

        void Start() {

            s_Singleton = this;
            _lobbyHooks = GetComponent<LobbyHook>();
            currentPanel = mainMenuPanel;

            backButton.gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;

            DontDestroyOnLoad(gameObject);

            SetServerInfo("Offline", "None");

        }

        public override void OnLobbyClientSceneChanged(NetworkConnection conn) {

            if (SceneManager.GetSceneAt(0).name == lobbyScene) {

                ChangeTo(topPanel.isInGame ? lobbyPanel : mainMenuPanel);

                if (topPanel.isInGame)
                    backDelegate = (_isMatchmaking == conn.playerControllers[0].unetView.isServer) ? (Action)StopHostClbk : StopClientClbk;

                topPanel.ToggleVisibility(true);
                topPanel.isInGame = false;

            } else {

                ChangeTo(null);

                Destroy(GameObject.Find("MainMenuUI(Clone)"));

                //backDelegate = StopGameClbk;

                topPanel.isInGame = true;
                topPanel.ToggleVisibility(false);

            }

        }

        public void ChangeTo(RectTransform newPanel) {

            currentPanel?.gameObject.SetActive(false);

            newPanel?.gameObject.SetActive(true);

            currentPanel = newPanel;

            backButton.gameObject.SetActive(currentPanel != mainMenuPanel);

            if (currentPanel == mainMenuPanel) {

                SetServerInfo("Offline", "None");

                _isMatchmaking = false;

            }

        }

        public void DisplayIsConnecting() {

            infoPanel.Display("Connecting...", "Cancel", () => { backDelegate(); });

        }

        public void SetServerInfo(string status, string host) {

            statusInfo.text = status;
            hostInfo.text = host;

        }

        public Action backDelegate;

        public void GoBackButton() {

            backDelegate();

			topPanel.isInGame = false;

        }

        // ----------------- Server management

        public void AddLocalPlayer() {

            TryToAddPlayer();

        }

        public void RemovePlayer(LobbyPlayer player) {

            player.RemovePlayer();

        }

        public void SimpleBackClbk() {

            ChangeTo(mainMenuPanel);

        }
                 
        public void StopHostClbk() {

            if (_isMatchmaking) {

				matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
				_disconnectServer = true;

            } else
                StopHost();          
                        
            ChangeTo(mainMenuPanel);

        }

        public void StopClientClbk() {

            StopClient();

            if (_isMatchmaking)
                StopMatchMaker();            

            ChangeTo(mainMenuPanel);

        }

        public void StopServerClbk() {

            StopServer();

            ChangeTo(mainMenuPanel);

        }

        class KickMsg : MessageBase { }

        public void KickPlayer(NetworkConnection conn) {

            conn.Send(MsgKicked, new KickMsg());

        }        

        public void KickedMessageHandler(NetworkMessage netMsg) {

            infoPanel.Display("Kicked by Server", "Close", null);

            netMsg.conn.Disconnect();

        }

        //===================

        public override void OnStartHost() {

            base.OnStartHost();

            ChangeTo(lobbyPanel);

            backDelegate = StopHostClbk;

            SetServerInfo("Hosting", networkAddress);

        }

		public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) {

			base.OnMatchCreate(success, extendedInfo, matchInfo);

            _currentMatchID = (System.UInt64)matchInfo.networkId;

		}

		public override void OnDestroyMatch(bool success, string extendedInfo) {

			base.OnDestroyMatch(success, extendedInfo);

			if (_disconnectServer) {

                StopMatchMaker();
                StopHost();

            }

        }

        //allow to handle the (+) button to add/remove player
        public void OnPlayersNumberModified(int count) {

            _playerNumber += count;

            int localPlayerCount = 0;

            foreach (PlayerController p in ClientScene.localPlayers)
                localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

            addPlayerButton.SetActive(localPlayerCount < maxPlayersPerConnection && _playerNumber < maxPlayers);

        }

        // ----------------- Server callbacks ------------------

        //we want to disable the button JOIN if we don't have enough player
        //But OnLobbyClientConnect isn't called on hosting player. So we override the lobbyPlayer creation

        void UpdateJoinButtons (int nbr = 1) {

            for (int i = 0; i < lobbySlots.Length; ++i) {

                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null) {

                    p.RpcUpdateRemoveButton();
                    p.ToggleJoinButton(numPlayers + nbr >= minPlayers);

                }

            }

        }

        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {

            GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject);

            LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
            newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);

            UpdateJoinButtons();

            return obj;

        }

        public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId) {

            UpdateJoinButtons();

        }

        public override void OnLobbyServerDisconnect(NetworkConnection conn) {

            UpdateJoinButtons(0);

        }

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {

            //This hook allows you to apply state data from the lobby-player to the game-player
            //just subclass "LobbyHook" and add it to the lobby object.

            _lobbyHooks?.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

            return true;
        }

        // --- Countdown management

        public override void OnLobbyServerPlayersReady() {

			bool allready = true;

			for(int i = 0; i < lobbySlots.Length; ++i)
				allready &= lobbySlots[i]?.readyToBegin ?? false;

			if(allready)
                ServerChangeScene(playScene);

        }

        // ----------------- Client callbacks ------------------

        public override void OnClientConnect(NetworkConnection conn) {

            base.OnClientConnect(conn);

            infoPanel.gameObject.SetActive(false);

            conn.RegisterHandler(MsgKicked, KickedMessageHandler);

            if (!NetworkServer.active) {//only to do on pure client (not self hosting client)

                ChangeTo(lobbyPanel);
                backDelegate = StopClientClbk;
                SetServerInfo("Client", networkAddress);

            }

        }

        public override void OnClientDisconnect(NetworkConnection conn) {

            base.OnClientDisconnect(conn);
            ChangeTo(mainMenuPanel);

        }

        public override void OnClientError(NetworkConnection conn, int errorCode) {

            ChangeTo(mainMenuPanel);
            infoPanel.Display("Client error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()), "Close", null);

        }

    }

}
