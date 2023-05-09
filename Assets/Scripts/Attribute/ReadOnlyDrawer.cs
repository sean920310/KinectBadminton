using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* reference: https://www.youtube.com/watch?v=r3nwTGLHygI&ab_channel=Comp-3Interactive */

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]

public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Saving previous GUI enabled value
        var previousGUIState = GUI.enabled;
        // Disabling edit for property
        GUI.enabled = false;
        // Drawing Property
        EditorGUI.PropertyField(position, property, label);
        // Setting old GUI enabled value
        GUI.enabled = previousGUIState;
    }
}
#endif