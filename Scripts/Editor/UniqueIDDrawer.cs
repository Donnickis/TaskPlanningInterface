using UnityEngine;
using UnityEditor;
using System;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// 
    /// This PropertyDrawer allows Unity to automatically set a string to a uniquely generated identifier (GUID). <see href="https://answers.unity.com/questions/487121/automatically-assigning-gameobjects-a-unique-and-c.html">Source</see>
    ///
    /// </summary>
    [CustomPropertyDrawer(typeof(UniqueIDAttribute))]
    public class UniqueIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Generate a unique ID, defaults to an empty string if nothing has been serialized yet
            if (prop.stringValue == "") {
                Guid guid = Guid.NewGuid();
                prop.stringValue = guid.ToString();
            }

            if ((attribute as UniqueIDAttribute).locked) {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, prop, label, true);
                GUI.enabled = true;
            } else {
                EditorGUI.PropertyField(position, prop, label, true);
            }
        }

        void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.LabelField(position, label, new GUIContent(prop.stringValue));
        }

    }

 }