using System.Collections.Generic;
using UnityEngine;

namespace QinCurses {

    public class SC_Finality : SC_BaseQinCurse {

        [Header ("Finality variables")]
        [Tooltip ("Maximum percentage of health left for a hero to be a valid target")]
        [Range (0, 100)]
        public int finalityHealthPercent;

        public override void Activate (bool canReturn) {

            base.Activate (true);

        }

        protected override List<GameObject> GetAvailableTiles () {

            List<GameObject> tiles = new List<GameObject> ();

            foreach (SC_Hero h in SC_Hero.heroes)
                if (h.Health <= Mathf.CeilToInt (h.MaxHealth * (finalityHealthPercent / 100f)))
                    tiles.Add (h.gameObject);

            return tiles;

        }

        public override bool CanActivate () {

            return base.CanActivate () && GetAvailableTiles ().Count > 0;

        }

        public override void Use (SC_Tile tile) {

            base.Use (tile);

            SC_Player.localPlayer.CmdFinality (tile.Hero.gameObject);

        }

    }

}
