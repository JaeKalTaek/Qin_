using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SC_Menu : MonoBehaviour {

	public GameObject mainPanel, onlinePanel, qmPanel, searchGamePanel, emptyPanel;

	void Awake() {
		
		mainPanel.SetActive (true);

	}

	public void ShowPanel(GameObject panel) {

		foreach (Transform t in transform)
			t.gameObject.SetActive (t.name.Equals (panel.name));

	}

	public void QuickMatchmaking(bool qin) {

		NetworkManager.singleton.GetComponent<SC_Network_Manager> ().QuickMatchmaking (qin);

	}

	public void CancelMatchmaking() {

		NetworkManager.singleton.GetComponent<SC_Network_Manager> ().CancelMatchmaking ();

	}
		
}
