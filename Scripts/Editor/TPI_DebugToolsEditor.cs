using TaskPlanningInterface.Helper;
using UnityEditor;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_ObjectPlacementController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_ObjectPlacementController in this instance, you also have to edit this script to reflect said changes.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [CustomEditor(typeof(TPI_DebugTools))]
    public class TPI_DebugToolsEditor : Editor {

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//


            EditorGUILayout.Space(30);
            // Header Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 30;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("TPI Debug Tools", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The 'TPI Debug Tools' are primarily buttons that can help you to debug the TPI or that replace the functionality of the hand menus, which are not accessible in the Unity Editor.\n" +
                "Please place all the debug buttons asa child of this Container GameObject, as this will allow the TPI to automatically toggle the visiblity when the programm is run on the mixed reality glasses (e.g. HoloLens 2).\n" +
                "The 'TPI Debug Tools' will not be turned off in the Unity Editor.", MessageType.Info);
            EditorGUILayout.Space(15);



            //---------------------------------------------------- General Options ----------------------------------------------------//



            // General Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle menuStyle = new GUIStyle();
            menuStyle.fontSize = 22;
            menuStyle.fontStyle = FontStyle.Bold;
            menuStyle.normal.textColor = Color.white;
            menuStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(new GUIContent("General Options", "Options relating to Debug Tools in general."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // show debug tools bool
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDebugTools"), new GUIContent("Show Debug Tools in deployed Program", "Decide whether the Debug Tools should be shown on the deployed program.  They will not be turned off in the Unity Editor."));
            EditorGUILayout.Space();

            // hand gesture debug GameObject
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handGestureDebugGameObject"));
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

        }

    }

}