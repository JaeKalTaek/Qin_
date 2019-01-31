using UnityEngine;
using UnityEngine.UI;

public class SC_Scaled_Image : MonoBehaviour {

    void OnEnable () {

        GetComponent<Image>().preserveAspect = true;

    }

}
