using UnityEngine;
using UnityEngine.UI;

public class SC_FightValue : MonoBehaviour {

    public Slider PrevGauge { get { return transform.GetChild(1).GetComponent<Slider>(); } }

    public Slider NewGauge { get { return transform.GetChild(2).GetComponent<Slider>(); } }    

    public Text Values { get { return transform.GetChild(3).GetComponent<Text>(); } }

    public void Set (int a, int b, int c) {

        Values.text = (a == b) ? a.ToString() : a + " " + (((name == "Crit") || (name == "Dodge")) ? "=>" : "<=") + " " + b;

        NewGauge.Set(a, c);

        PrevGauge.Set(b, c);

    }

}
