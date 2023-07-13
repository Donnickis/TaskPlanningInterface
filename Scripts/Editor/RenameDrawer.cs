using UnityEditor;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// 
    /// This PropertyDrawer allows unity to show a different name for a variable in the Unity Inspector than was assigned in the C# script. <see href="https://answers.unity.com/questions/1487864/change-a-variable-name-only-on-the-inspector.html">Source</see>
    ///
    /// </summary>
    [CustomPropertyDrawer(typeof(RenameAttribute))]
    public class RenameDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, new GUIContent((attribute as RenameAttribute).NewName));
        }

    }

}