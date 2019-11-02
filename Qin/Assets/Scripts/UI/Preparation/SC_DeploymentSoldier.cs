using UnityEngine;
using static SC_Global;

public class SC_DeploymentSoldier : MonoBehaviour {

    public SpriteRenderer SpriteR { get { return GetComponent<SpriteRenderer> (); } }

    SC_Tile prevTile;

    public static int[] regionPoints;

    int Cost { get { return Resources.Load<SC_Soldier> ("Prefabs/Characters/Soldiers/Basic/P_" + SpriteR.sprite.name).cost; } }

    int MaxRegionPoints { get { return SC_Game_Manager.Instance.CommonQinVariables.maxRegionSoldierPoints; } }

    SC_UI_Manager UIManager { get { return SC_UI_Manager.Instance; } }

    void Start () {

        if (regionPoints == null) {

            regionPoints = new int[6];

            for (int i = 0; i < regionPoints.Length; i++)
                regionPoints[i] = MaxRegionPoints;            

        }

        TryPlace ();

    }

    void OnMouseDrag () {

        if (UIManager.PreparationPhase == (int) EQinPreparationElement.Soldiers)
            transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);

    }

    void OnMouseUp () {

        if (this && UIManager.PreparationPhase == (int) EQinPreparationElement.Soldiers)
            TryPlace ();

    }

    bool SameRegion { get { return SC_Cursor.Tile.Region == (prevTile?.Region ?? -1); } }

    void TryPlace () {

        if (SC_Cursor.Tile && SC_Cursor.Tile != prevTile && (SameRegion || regionPoints[SC_Cursor.Tile.Region] + (SC_Cursor.Tile.DeployedSoldier?.Cost ?? 0) >= Cost)) {

            regionPoints[SC_Cursor.Tile.Region] -= Cost;

            if (prevTile) {

                regionPoints[prevTile.Region] += Cost;

                prevTile.DeployedSoldier = null;

            }

            if (SC_Cursor.Tile.DeployedSoldier) {
                
                regionPoints[SC_Cursor.Tile.Region] += SC_Cursor.Tile.DeployedSoldier.Cost;

                if (SameRegion || prevTile && regionPoints[prevTile.Region] >= SC_Cursor.Tile.DeployedSoldier.Cost) {

                    SC_Cursor.Tile.DeployedSoldier.transform.position = prevTile.transform.position;

                    prevTile.DeployedSoldier = SC_Cursor.Tile.DeployedSoldier;

                    prevTile.DeployedSoldier.prevTile = prevTile;

                    regionPoints[prevTile.Region] -= prevTile.DeployedSoldier.Cost;

                } else
                    Destroy (SC_Cursor.Tile.DeployedSoldier.gameObject);

            }                 

            if (prevTile)
                UIManager.qinPreprationUI.castleDecks[prevTile.Region].SoldiersPoints.text = regionPoints[prevTile.Region].ToString ();
                       
            prevTile = SC_Cursor.Tile;

            UIManager.qinPreprationUI.castleDecks[prevTile.Region].SoldiersPoints.text = regionPoints[prevTile.Region].ToString ();

            transform.position = prevTile.transform.position;

            prevTile.DeployedSoldier = this;

        } else {

            if (!prevTile)
                Destroy (gameObject);
            else
                transform.position = prevTile.transform.position;

        }

    }

    void OnMouseOver () {

        if (Input.GetMouseButtonDown (1) && UIManager.PreparationPhase == (int) EQinPreparationElement.Soldiers) {

            regionPoints[prevTile.Region] += Cost;

            UIManager.qinPreprationUI.castleDecks[prevTile.Region].SoldiersPoints.text = regionPoints[prevTile.Region].ToString ();

            prevTile.DeployedSoldier = null;

            Destroy (gameObject);

        }

    }

}
