using System.Collections.Generic;
using UnityEngine;

namespace QinCurses {

    public class SC_Resurrection : SC_BaseQinCurse {

        public override void Activate (bool canReturn) {

            base.Activate (false);

            SC_Player.localPlayer.CmdUseCurse (null);

        }

        protected override List<GameObject> GetAvailableTiles () {

            return null;

        }

        public override void Use (SC_Tile tile) {

            base.Use (tile);

            SC_Hero h = SC_Game_Manager.LastHeroDead;

            h.Tile.Character = h;

            Destroy (h.Tile.Grave);

            h.Qin = true;

            h.Health = h.MaxHealth;

            h.PreparationCharge = 0;

            h.AnticipationCharge = 0;

            h.DrainingSteleSlow = 0;

            h.ActionCount = -1;

            h.MovementCount = -1;

            h.MovementPoints = h.Movement;

            h.Stunned = 0;

            h.SetTired (false);

            h.CanBeSelected = true;

            h.Sprite.flipX = true;

            h.gameObject.SetActive (true);

        }

    }

}
