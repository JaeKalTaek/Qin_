using System;
using System.Collections.Generic;
using UnityEngine;

namespace QinCurses {

    public abstract class SC_BaseQinCurse : MonoBehaviour {

        protected static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

        protected static SC_Tile_Manager TileManager { get { return SC_Tile_Manager.Instance; } }

        [Header ("Base Qin curse variables")]
        [Tooltip ("Cost of this curse")]
        public int cost;

        public virtual void Activate (bool canReturn) {

            SC_Player.localPlayer.Busy = true;

            UI.playerActionsPanel.SetActive (false);

            UI.StartCoroutine (UI.ClickSafety (() => { SC_Cursor.SetLock (false); }));

            if (GetAvailableTiles () != null)
                foreach (GameObject g in GetAvailableTiles ())
                    TileManager.GetTileAt (g).ChangeDisplay (SC_Global.TDisplay.QinCurse);

            UI.backAction = canReturn ? (Action) (() => {

                SC_Player.localPlayer.Busy = false;

                TileManager.RemoveAllFilters ();

                UI.ActivateMenu (UI.playerActionsPanel);

            }) : SC_Global.DoNothing;

        }

        public virtual bool CanActivate () {

            return cost < SC_Qin.Energy;

        }

        protected abstract List<GameObject> GetAvailableTiles ();

        public virtual void Use (SC_Tile tile) {

            SC_Qin.ChangeEnergy (-SC_Qin.Curse.cost);

            if (SC_Player.localPlayer.Turn) {

                SC_Qin.CurseUsed = true;

                SC_Player.localPlayer.Busy = false;

                UI.backAction = SC_Global.DoNothing;

            }

        }

    }

}
