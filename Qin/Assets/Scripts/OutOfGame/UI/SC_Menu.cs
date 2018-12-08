using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_Menu : MonoBehaviour {

	public GameObject mainPanel, onlinePanel, qmPanel, searchGamePanel, emptyPanel;

    public Button online;

	void Awake() {

        ShowPanel(mainPanel);


    }

	public void ShowPanel(GameObject panel) {

		foreach (Transform t in transform)
			t.gameObject.SetActive (t.name.Equals (panel.name));

        panel.GetComponentInChildren<Button>()?.Select();

	}

	public void QuickMatchmaking(bool qin) {

		NetworkManager.singleton.GetComponent<SC_Network_Manager> ().QuickMatchmaking (qin);

	}

	public void CancelMatchmaking() {

		NetworkManager.singleton.GetComponent<SC_Network_Manager> ().CancelMatchmaking ();

	}
		
}
