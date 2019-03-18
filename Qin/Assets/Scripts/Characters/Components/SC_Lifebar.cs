using UnityEngine;

public class SC_Lifebar : MonoBehaviour {

	public Transform health, health2;
	public GameObject lifebar;

    void Start() {

        lifebar.SetActive(SC_UI_Manager.Instance.healthBarsToggle.isOn);

    }

    public void UpdateGraph(int h, int maxH) {

        float percentage = (float)h / maxH;

        health.localScale = new Vector3(percentage, 1, 1);
        health2.localScale = new Vector3(percentage, 1, 1);

        Vector3 pos = health.localPosition;
        float posX = -0.6f + (0.6f * percentage);
        health.localPosition = new Vector3(posX, pos.y, pos.z);
        health2.localPosition = new Vector3(posX, pos.y, pos.z);

    }

	public void Toggle() {

		lifebar.SetActive(!lifebar.activeSelf);

	}

}
