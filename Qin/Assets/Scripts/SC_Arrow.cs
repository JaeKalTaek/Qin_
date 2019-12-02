using UnityEngine;
using static SC_Character;
using static SC_Player;
using static SC_Global;
using System.Collections.Generic;

public class SC_Arrow : MonoBehaviour {

    public static GameObject arrow;

    public static List<SC_Tile> path;

    static SC_Common_Characters_Variables CharaVariables { get { return SC_Game_Manager.Instance.CommonCharactersVariables; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public static void CursorMoved(SC_Tile newTile) {

        if (MovingCharacter && localPlayer.Turn && !localPlayer.Busy) {

            if (!arrow)
                arrow = new GameObject("Arrow");
            else
                foreach (Transform t in arrow.transform)
                    Destroy(t.gameObject);

            SC_Tile tile = (newTile.CurrentDisplay == TDisplay.Movement) ? newTile : SC_Tile_Manager.Instance.ClosestMovementTile(newTile);

            path = SC_Tile_Manager.Instance.PathFinder(activeCharacter.Tile, tile);

            if (activeCharacter.Hero)
                SC_Hero.SetStaminaCost(new int[] { path != null ? activeCharacter.Hero.MovementCost(path.Count - 1) : 0, (newTile.CanAttack ? activeCharacter.Hero.ActionCost : 0) });

            if(newTile.CanAttack)
                UI.PreviewFight(tile);

            #region Setup visual
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

                    GameObject arrowPart = Instantiate(Resources.Load<GameObject>("Prefabs/P_Arrow"), pos[i], Quaternion.AngleAxis(rot, Vector3.forward), arrow.transform);
                    arrowPart.GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("Sprites/Arrow/" + s);
                    arrowPart.transform.SetPos (arrowPart.transform.position, 0);

                }

            }
            #endregion

        }

    }    

    public static void DestroyArrow () {

        Destroy(arrow);

        path = null;

    }

}
