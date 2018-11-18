using UnityEngine;

public class SC_AdaptableMenu : MonoBehaviour {

    public bool resize;

    public int heightMultiplier;

    RectTransform RecT { get { return transform as RectTransform; } }

    void OnEnable () {

        float nbr = 0;

        foreach (Transform t in transform)
            nbr += t.gameObject.activeSelf ? 1 : 0;

        if(resize)
            RecT.sizeDelta = new Vector2(RecT.sizeDelta.x, heightMultiplier * nbr);

        int index = 0;

        foreach (Transform t in transform) {

            if (t.gameObject.activeSelf) {

                RectTransform rT = t as RectTransform;

                rT.anchorMin = new Vector2(rT.anchorMin.x, 1 - ((index + 1) / nbr));

                rT.anchorMax = new Vector2(rT.anchorMax.x, 1 - index / nbr);

                index++;

            }

        }

    }

}
