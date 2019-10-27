using UnityEngine;
using UnityEngine.UI;

public class SC_ShieldBar : MonoBehaviour {

    [Header("Shield Bar Variables")]
    [Tooltip("Spacing between two shields")]
    public float spacing;

    public RectTransform RecT { get { return transform as RectTransform; } }    

    float Height { get { return RecT.rect.height; } }

    public void Set(int value, bool damaged = false) {

        foreach (Transform t in RecT)
            Destroy(t.gameObject);

        if (value > 0) {

            float w = Height * Mathf.Min(1, RecT.rect.width / (Height * (value + (value + 1) * spacing)));

            bool e = value % 2 == 0;

            for (int i = -value / 2; i <= value / 2; i++) {

                if (!e || i != 0) {

                    RectTransform r = Instantiate(Resources.Load<GameObject>("Prefabs/UI/P_Shield"), transform).transform as RectTransform;

                    r.sizeDelta = new Vector2(w, Height);

                    r.anchoredPosition = new Vector2((i + (e ? .5f * -Mathf.Sign(i) : 0)) * w * (spacing + 1), 0);

                    if (damaged && (i == value / 2))
                        r.GetComponent<Image>().color = new Color(1, 1, 1, .5f);

                }

            }

            gameObject.SetActive(true);

        }

    }

}
