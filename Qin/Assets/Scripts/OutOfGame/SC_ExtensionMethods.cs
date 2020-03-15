using UnityEngine;
using UnityEngine.UI;
using static SC_Global;

public static class SC_ExtensionMethods {

   public static void SetPos(this Transform trans, Vector3 v3, string layer = "") {

        trans.position = new Vector3(v3.x, v3.y, layer == "" ? trans.position.z : SC_Game_Manager.Instance.elementLayers.IndexOf (layer));
        trans.GetComponentInChildren<SpriteRenderer>().sortingOrder = -(v3.x.I() + v3.y.I());

    }

    public static void ShowInfos(this MonoBehaviour MB) {

        if(MB)
            SC_UI_Manager.Instance.ShowInfos(MB.gameObject, MB.GetType());

    }

    public static void TryRefreshInfos (this MonoBehaviour MB) {

        if (MB)
            SC_UI_Manager.Instance.TryRefreshInfos (MB.gameObject, MB.GetType ());

    }

    public static Vector3 I (this Vector3 v) {

        return new Vector3(v.x.I(), v.y.I(), v.z.I());

    }

    public static int I (this float f) {

        return Mathf.RoundToInt(f);

    }

    public static bool In (this Vector2 v, int i) {

        return (i >= v.x) && (i <= v.y);

    }

    public static void Set (this Slider s, float a, float b, ColorMode cm = ColorMode.Health, Color param = new Color()) {

        s.value = a / b;

        if (cm == ColorMode.Health)
            s.image.color = Color.Lerp(SC_UI_Manager.Instance.minHealthColor, SC_UI_Manager.Instance.maxHealthColor, s.value);
        else if (cm == ColorMode.Param)
            s.image.color = param;
    }

}
