using UnityEngine;
using UnityEngine.UI;
using static SC_Global;

public class SC_FightValue : MonoBehaviour {

    public Slider PrevGauge { get { return transform.GetChild(1).GetComponent<Slider>(); } }

    public Slider NewGauge { get { return transform.GetChild(2).GetComponent<Slider>(); } }    

    public Text Values { get { return transform.GetChild(3).GetComponent<Text>(); } }

    public void Set (int a, int b, int c, bool trigger = false) {

        Values.text = trigger ? (name == "Preparation" ? "Crit" : "Dodge") + " ! " : ((a == b) ? a.ToString() : a + " " + (((name == "Preparation") || (name == "Anticipation")) ? "=>" : "<=") + " " + b);

        NewGauge.Set(a, c, ColorMode.Default);

        NewGauge.gameObject.SetActive(true);

        PrevGauge.Set(b, c, ColorMode.Default);

        PrevGauge.gameObject.SetActive(true);

    }

}
