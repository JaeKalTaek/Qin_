using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Prototype.NetworkLobby {

    //List of players in the lobby
    public class LobbyPlayerList : MonoBehaviour {

        public static LobbyPlayerList _instance = null;

        public RectTransform playerListContentTransform;
        public GameObject warningDirectPlayServer;
        public Transform addButtonRow;

        protected VerticalLayoutGroup _layout;
        protected List<LobbyPlayer> _players = new List<LobbyPlayer>();

        public void OnEnable() {

            _instance = this;
            _layout = playerListContentTransform.GetComponent<VerticalLayoutGroup>();

        }

        public void DisplayDirectServerWarning(bool enabled) {

            warningDirectPlayServer?.SetActive(enabled);

        }

        void Update() {
            //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
            //sometime to child being assigned before layout was enabled/init, leading to broken layouting)
            
            if(_layout)
                _layout.childAlignment = Time.frameCount%2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;

        }

        public void AddPlayer(LobbyPlayer player) {

            if (_players.Contains(player))
                return;

            _players.Add(player);

            player.transform.SetParent(playerListContentTransform, false);
            addButtonRow.transform.SetAsLastSibling();

            PlayerListModified();

        }

        public void RemovePlayer(LobbyPlayer player) {

            _players.Remove(player);
            PlayerListModified();

        }

        public void PlayerListModified() {

            /*int i = 0;

            foreach (LobbyPlayer p in _players) {

                p.OnPlayerListChanged(i);
                ++i;
            }*/

            for (int i = 0; i < _players.Count; i++)
                _players[i].OnPlayerListChanged(i);

        }

        public void UpdateReadyButtons() {

            foreach (LobbyPlayer p in _players)
                p.SetReadyButton();

        }

        public bool CanGetReady() {

            if (_players.Count != 2)
                return false;
            else
                return _players[0].playerSide != _players[1].playerSide;            

        }

    }

}
