using UnityEngine;

public class SC_Scroll_Menu : MonoBehaviour {

    public float offset, elementHeight, margin;

    RectTransform RecT { get { return transform as RectTransform; } }

    void OnEnable () {

        float nbr = 0; 

        foreach (Transform t in RecT)
            nbr += t.gameObject.activeSelf ? 1 : 0;

        RecT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, elementHeight * nbr + margin * (nbr + 1) + offset);

        int index = 0;

        foreach (Transform t in RecT) {

            if (t.gameObject.activeSelf) {

                (t as RectTransform).anchoredPosition = Vector2.up * (margin * (index + 1) + elementHeight * index + (elementHeight / 2));

                (t as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, elementHeight);

                index++;

            }

        }                
        
    }

}
