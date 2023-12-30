using System.Linq.Expressions;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StandAloneRenderer))]
public class CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        // Display a custom section in the Inspector
        EditorGUILayout.LabelField("Additional Information", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Some helpful information goes here.");
        EditorGUILayout.LabelField("You can add more details or instructions.");
    }
}