using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ReplaceFontScript))]
public class ReplaceFontEditor : Editor {    

    public override void OnInspectorGUI () {

        DrawDefaultInspector();

        if (GUILayout.Button("Test"))
            Recursive(((ReplaceFontScript)target).start);

    }

    void Recursive(Transform startT) {

        foreach (Transform t in startT) {

            if (t.GetComponent<Text>())
                t.GetComponent<Text>().font = Resources.Load<Font>("Fonts/Flailed");

            if (t.childCount > 0)
                Recursive(t);

        }

    }

}
