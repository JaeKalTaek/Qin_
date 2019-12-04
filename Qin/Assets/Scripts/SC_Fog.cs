using UnityEngine;
using DG.Tweening;

public class SC_Fog : MonoBehaviour {

    SpriteRenderer Renderer { get; set; }

    public SC_Tile Tile { get; set; }

    public bool Qin { get; set; }

    public void Setup (SC_Tile tile, bool qin) {

        Qin = qin;

        Renderer = GetComponent<SpriteRenderer> ();

        Tile = tile;

        UpdateFog ();
        
    }

    public void UpdateFog () {

        bool hasVision = false;

        foreach (SC_Tile t in SC_Tile_Manager.Instance.GetRange (Tile.transform.position, 1))
            hasVision |= (t.Character && t.Character.Qin == Qin) || (t.Qin && Qin);

        Renderer.DOFade (hasVision ? .5f : 1f, 0);

    }

}
