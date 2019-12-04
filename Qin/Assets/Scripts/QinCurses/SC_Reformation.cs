using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QinCurses {

    public class SC_Reformation : SC_BaseQinCurse {

        public override void Activate (bool canReturn) {

            base.Activate (true);

        }

        protected override List<GameObject> GetAvailableTiles () {

            List<GameObject> tiles = new List<GameObject> ();

            foreach (SC_Castle c in FindObjectsOfType<SC_Castle> ())
                tiles.Add (c.gameObject);

            return tiles;

        }

        public override void Use (SC_Tile tile) {

            base.Use (tile);

            List<SC_Character> characters = new List<SC_Character> (FindObjectsOfType<SC_Character> ());

            characters.RemoveAll (new System.Predicate<SC_Character> (c => !c.Qin || (c.Demon?.Linked ?? false)));

            if (characters.Count > 0) {

                foreach (SC_Character c in characters) {

                    c.Tile.Character = null;

                    c.Tile.UpdateFog ();

                }

                List<SC_Tile> orderedTiles = new List<SC_Tile> ();

                foreach (SC_Tile t in SC_Tile_Manager.Instance.tiles)             
                    if (characters[0].CanCharacterSetOn (t))
                        orderedTiles.Add (t);

                orderedTiles.Sort ((a, b) => SC_Tile_Manager.CompareTilesClockwiseOrder (tile, a, b));

                for (int i = 0; i < characters.Count; i++) {

                    characters[i].transform.SetPos (orderedTiles[i].transform.position);

                    orderedTiles[i].Character = characters[i];

                    orderedTiles[i].UpdateFog ();

                }

            }

        }

    }

}
