using UnityEngine;
using UnityEditor;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// 
    /// This PropertyDrawer allows Unity to lock a variable in the Inspector, therefore set it to a "ReadOnly" state. <see href="https://gist.github.com/LotteMakesStuff/c0a3b404524be57574ffa5f8270268ea">Source</see>
    ///
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

}



