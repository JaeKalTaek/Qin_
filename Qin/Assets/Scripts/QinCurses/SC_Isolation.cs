using System.Collections.Generic;
using UnityEngine;

namespace QinCurses {    

    public class SC_Isolation : SC_BaseQinCurse {

        [Header ("Isolation variables")]
        [Tooltip ("Isolation damage")]
        public int isolationDamage;

        [Tooltip ("Isolation range")]
        public int isolationRange;

        protected override List<GameObject> GetAvailableTiles () {

            List<GameObject> tiles = new List<GameObject> ();

            foreach (SC_Hero h in SC_Hero.heroes)
                tiles.Add (h.gameObject);

            return tiles;

        }

        public override void Use (SC_Tile tile) {

            base.Use (tile);

            tile.Hero.Isolated = true;

        }

        public void ApplyDamage (SC_Hero target) {

            foreach (SC_Hero h in TileManager.HeroesInRange (target, isolationRange))
                h.Hit (isolationDamage);

        }
        
    }

}
