using UnityEngine;

public class SC_RelationshipGainFeedback : MonoBehaviour {

    [Header("Variables")]
    [Tooltip("Time this feedback will stay on")]
    public float lifetime;

    void Start () {

        Destroy(gameObject, lifetime);

    }

}
