using UnityEngine;
using static SC_Global;

public class SC_DeploymentHero : MonoBehaviour {

    public SpriteRenderer SpriteR { get { return GetComponent<SpriteRenderer> (); } }

    Vector3 oldPos;

    void Start () {

        SC_Tile_Manager.Instance.GetTileAt (transform.position).DeployedHero = this;

    }

    void OnMouseDown () {

        oldPos = transform.position;

    }

    void OnMouseDrag () {

        if (SC_UI_Manager.Instance.PreparationPhase == (int)EHeroPreparationElement.Deployment)
            transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);

    }

    void OnMouseUp () {

        SC_Tile tileAtPos = SC_Tile_Manager.Instance.GetTileAt (transform.position);

        if (tileAtPos && tileAtPos.CurrentDisplay == TDisplay.Deploy && tileAtPos.transform.position != oldPos) {            

            if (tileAtPos.DeployedHero)
                tileAtPos.DeployedHero.transform.position = oldPos;

            SC_Tile_Manager.Instance.GetTileAt (oldPos).DeployedHero = tileAtPos.DeployedHero;

            tileAtPos.DeployedHero = this;

            transform.position = tileAtPos.transform.position;

        } else
            transform.position = oldPos;

    }



}
