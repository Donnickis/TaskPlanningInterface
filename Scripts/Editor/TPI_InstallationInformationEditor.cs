using TaskPlanningInterface.Helper;
using UnityEditor;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_InstallationInformation class. 
    /// </para>
    ///
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [CustomEditor(typeof(TPI_InstallationInformation))]
    public class TPI_InstallationInformationEditor : Editor {

        public override void OnInspectorGUI() {

            EditorGUILayout.Space(30);
            // Header Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 30;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Task Planning Interface", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("This GameObject acts as a container for the TPI_Manager and the TPI_DebugTools.\n" +
                "It was saved this way, as this prevents the references between both children to disappear.\n" +
                "Please extract both from this container into your Unity Hierarchy and then delete this GameObject.", MessageType.Info);
            EditorGUILayout.Space(15);

        }

    }

}