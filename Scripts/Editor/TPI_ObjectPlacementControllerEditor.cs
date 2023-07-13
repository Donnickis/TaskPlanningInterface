using TaskPlanningInterface.Controller;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEditorInternal;
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
    [CustomEditor(typeof(TPI_ObjectPlacementController))]
    public class TPI_ObjectPlacementControllerEditor : Editor {

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/ObjectPlacementController_header.png", typeof(Texture2D));
            GUILayout.Box(TPI_Banner, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.9f), GUILayout.Height(EditorGUIUtility.currentViewWidth * 0.23f));
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Header Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 30;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Object Placement Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The ObjectPlacementController helps you with the positioning of your Menus and Objects. \n" +
                "It uses an MRTK GridObjectCollection and custom search algorithms in order to find you a suitable place in the environment.\n" +
                "Furthermore, it gives you the option to choose from a list of possible starting position, search algorithms and search directions in order for you to have as much say in the placement as possible.\n" +
                "By coupling the ObjectPlacementController with a hand menu (right hand ulnar side), the operator has the option to select whether the GridObjectCollection should be attached to the operator's position " +
                "and whether it should also rotate with the operator.\n" +
                "This custom Inspector gives you the following main options: Configuring the GridObjectCollection and configuring the 'follow me' behaviour.", MessageType.Info);
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
            EditorGUILayout.LabelField(new GUIContent("General Options", "Options relating to the GridPlacementController in general, e.g. concerning the GridObjectCollection"), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // GridObjectCollection Options Text
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent("GridObjectCollection Options", "Options relating to the GridObjectCollection that the TPI_ObjectPlacementController uses. Those options will be automatically applied to the GridObjectCollection."), titleStyle);
            EditorGUILayout.Space(20);

            // available spots int
            EditorGUILayout.PropertyField(serializedObject.FindProperty("availableSpots"));
            EditorGUILayout.Space();

            // number of columns int
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_numCol"));
            EditorGUILayout.Space();

            // number of rows int
            GUI.enabled = false;
            serializedObject.FindProperty("_numRow").intValue = (int)Mathf.Ceil(((float)serializedObject.FindProperty("availableSpots").intValue) / serializedObject.FindProperty("_numCol").intValue);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_numRow"));
            GUI.enabled = true;
            EditorGUILayout.Space();

            // cell width float
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cellWidth"));
            EditorGUILayout.Space();

            // cell height float
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cellHeight"));
            EditorGUILayout.Space(20);


            // Follow Behaviour Options Text
            EditorGUILayout.LabelField(new GUIContent("Follow Behaviour Options", "Options relating to the attachement of the GridObjectCollection to the operators view."), titleStyle);
            EditorGUILayout.Space(10);

            // attach to operator bool
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attachToOperator"));
            EditorGUILayout.Space();

            //if(serializedObject.FindProperty("attachToOperator").boolValue) {
                // Indent foldout content
                EditorGUI.indentLevel++;

                // camera distance float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceToCamera"));
                EditorGUILayout.Space();

                // smooth time float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothTime"));
                EditorGUILayout.Space();

                // Remove indent
                EditorGUI.indentLevel--;
            //}

            EditorGUILayout.Space();

            // rotate with operator bool
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateWithOperator"));
            EditorGUILayout.Space();

            //if (serializedObject.FindProperty("rotateWithOperator").boolValue) {
                // Indent foldout content
                EditorGUI.indentLevel++;

                // rotation speed float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));
                EditorGUILayout.Space();

                // Remove indent
                EditorGUI.indentLevel--;
            //}

            serializedObject.ApplyModifiedProperties();
            // Info Box
            EditorGUILayout.HelpBox("IMPORTANT: \nChanges made to the GridObjectCollection below will be automatically reverted by the TPI_ObjectPlacementController. Therefore, please either make the changes in the Inspector above with the given options (generally they are good enough) or in the Start() function of the TPI_ObjectPlacementController class.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();

        }

    }

}