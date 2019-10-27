using UnityEngine;
using UnityEngine.UI;

public class SC_Menu : MonoBehaviour {

    public GameObject mainPanel;

	public void ShowPanel(GameObject panel) {

		foreach (Transform t in transform)
			t.gameObject.SetActive (t.gameObject == panel);

        panel.GetComponentInChildren<Button>()?.Select();

	}
		
}
