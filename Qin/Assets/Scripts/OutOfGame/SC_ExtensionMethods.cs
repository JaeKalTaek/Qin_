using UnityEngine;
using UnityEngine.UI;

public static class SC_ExtensionMethods {

	public static void SetPos(this Transform trans, Transform other) {

		trans.SetPos(other.position);

	}

	public static void SetPos(this Transform trans, Vector3 v3) {

		trans.SetPos (new Vector2 (v3.x, v3.y));

	}

	public static void SetPos(this Transform trans, Vector2 v2) {

		trans.position = new Vector3 (v2.x, v2.y, trans.position.z);

	}
		
    public static void ShowInfos(this MonoBehaviour MB) {

        if(MB)
            SC_UI_Manager.Instance.ShowInfos(MB.gameObject, MB.GetType());

    }

    public static int I (this float f) {

        return Mathf.RoundToInt(f / .96f);

    }

    public static bool In (this Vector2 v, int i) {

        return (i >= v.x) && (i <= v.y);

    }

    public static void Set (this Slider s, float a, float b, bool c = true) {

        s.value = a / b;

        if(c)
            s.image.color = Color.Lerp(SC_UI_Manager.Instance.minHealthColor, SC_UI_Manager.Instance.maxHealthColor, s.value);

    }

}
