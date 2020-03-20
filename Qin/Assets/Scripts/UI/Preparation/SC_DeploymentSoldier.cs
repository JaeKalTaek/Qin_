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

    bool SameRegion { get { return TileUnderMouse.Region == (prevTile?.Region ?? -1); } }

    void TryPlace () {

        if (TileUnderMouse && (!TileUnderMouse.infos.heroDeploy) && TileUnderMouse.baseCost < 100 && !TileUnderMouse.Qin && TileUnderMouse != prevTile && (SameRegion || regionPoints[TileUnderMouse.Region] + (TileUnderMouse.DeployedSoldier?.Cost ?? 0) >= Cost)) {

            regionPoints[TileUnderMouse.Region] -= Cost;

            if (prevTile) {

                regionPoints[prevTile.Region] += Cost;

                prevTile.DeployedSoldier = null;

            }

            if (TileUnderMouse.DeployedSoldier) {
                
                regionPoints[TileUnderMouse.Region] += TileUnderMouse.DeployedSoldier.Cost;

                if (SameRegion || prevTile && regionPoints[prevTile.Region] >= TileUnderMouse.DeployedSoldier.Cost) {

                    TileUnderMouse.DeployedSoldier.transform.SetPos (prevTile.transform.position, "Character");

                    prevTile.DeployedSoldier = TileUnderMouse.DeployedSoldier;

                    prevTile.DeployedSoldier.prevTile = prevTile;

                    regionPoints[prevTile.Region] -= prevTile.DeployedSoldier.Cost;

                } else
                    Destroy (TileUnderMouse.DeployedSoldier.gameObject);

            }                 

            if (prevTile)
                UIManager.qinPreprationUI.castleDecks[prevTile.Region].SoldiersPoints.text = regionPoints[prevTile.Region].ToString ();

            UIManager.qinPreprationUI.castleDecks[TileUnderMouse.Region].SoldiersPoints.text = regionPoints[TileUnderMouse.Region].ToString ();

            transform.SetPos (TileUnderMouse.transform.position, "Character");

            TileUnderMouse.DeployedSoldier = this;

            prevTile = TileUnderMouse;

        } else {

            if (!prevTile)
                Destroy (gameObject);
            else
                transform.SetPos (prevTile.transform.position, "Character");

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
