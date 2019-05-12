using UnityEngine;
using static SC_Character;
using static SC_Player;
using static SC_Global;
using System.Collections.Generic;

public class SC_Arrow : MonoBehaviour {

    public static GameObject arrow;

	public static void CursorMoved(SC_Tile newTile) {

        if (MovingCharacter && localPlayer.Turn && !localPlayer.Busy) {

            if (!arrow)
                arrow = Instantiate(new GameObject("Arrow"));
            else
                foreach (Transform t in arrow.transform)
                    Destroy(t.gameObject);

            SC_Tile tile = (newTile.CurrentDisplay == TDisplay.Movement) ? newTile : SC_Tile_Manager.Instance.ClosestMovementTile(newTile);

            SC_UI_Manager.Instance.PreviewFight(tile);

            #region Setup visual
            List<SC_Tile> path = SC_Tile_Manager.Instance.PathFinder(activeCharacter.Tile, tile);            
            
            if (path != null) {

                List<Vector3> pos = new List<Vector3>();

                foreach (SC_Tile t in path)
                    pos.Add(t.transform.position);

                for (int i = 0; i < pos.Count; i++) {

                    string s = "";

                    float rot = 0;

                    if (i == 0) {

                        s = "Start";

                        if (pos[i + 1].x < pos[i].x)
                            rot = 180;
                        else if (pos[i + 1].y > pos[i].y)
                            rot = 90;
                        else if (pos[i + 1].y < pos[i].y)
                            rot = 270;

                    } else if (i == pos.Count - 1) {

                        s = "End";

                        if (pos[i - 1].x > pos[i].x)
                            rot = 180;
                        else if (pos[i - 1].y > pos[i].y)
                            rot = 270;
                        else if (pos[i - 1].y < pos[i].y)
                            rot = 90;

                    } else {

                        if((pos[i - 1].x == pos[i + 1].x) || (pos[i - 1].y == pos[i + 1].y)) {

                            s = "Line";

                            rot = (pos[i - 1].y != pos[i].y) ? 90 : 0;

                        } else {

                            s = "Bend";

                            if((pos[i - 1].x < pos[i].x) || (pos[i + 1].x < pos[i].x))
                                rot = ((pos[i - 1].y < pos[i].y) || (pos[i + 1].y < pos[i].y)) ? 270 : 180;
                            else
                                rot = ((pos[i - 1].y < pos[i].y) || (pos[i + 1].y < pos[i].y)) ? 0 : 90;

                        }

                    }

                    Instantiate(Resources.Load<GameObject>("Prefabs/P_Arrow"), pos[i], Quaternion.AngleAxis(rot, Vector3.forward), arrow.transform).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Arrow/" + s);

                }

            }
            #endregion

        }

    }    

}
