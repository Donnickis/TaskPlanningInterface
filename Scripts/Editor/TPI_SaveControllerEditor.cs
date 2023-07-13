using System;
using TaskPlanningInterface.Controller;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_SaveController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_SaveController in this instance, you also have to edit this script to reflect said changes.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// 
    /// <para>
    /// @sources
    /// <br></br><see href="https://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/">Reorderable Lists Source 1</see>
    /// <br></br><see href="https://gist.github.com/Democide/70da781eb2706899b823de6af42d0871">Reorderable Lists Source 2</see>
    /// </para>
    /// </summary>
    [CustomEditor(typeof(TPI_SaveController))]
    public class TPI_SaveControllerEditor : Editor {

        private ReorderableList workflowSaveList;
        private ReorderableList reorderableList;

        private SerializedProperty workflowSaveProperty;

        private void OnEnable() {



            //---------------------------------------------------- Workflow List ----------------------------------------------------//



            workflowSaveProperty = serializedObject.FindProperty("workflowSaveData").FindPropertyRelative("workflows");
            workflowSaveList = new ReorderableList(serializedObject, workflowSaveProperty, false, true, false, false);

            // List menu name
            workflowSaveList.drawHeaderCallback = (Rect rect) => {
#if UNITY_WSA || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA_10_0
                workflowSaveProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), workflowSaveProperty.isExpanded, new GUIContent(" Saved Workflows", "This List contains all the workflows that have been saved in the past and were thus loaded at startup, and all the workflows that the operator has saved during the runtime of this session. You cannot make any alterations in this list as this should only provide you with the information whether everything has been loaded and saved properly. If you want to make any changes, please do so in the JSON files found at: " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/TaskPlanningInterface/JSON/"));
#else
                workflowSaveProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), workflowSaveProperty.isExpanded, new GUIContent(" Saved Workflows", "This List contains all the workflows that have been saved in the past and were thus loaded at startup, and all the workflows that the operator has saved during the runtime of this session. You cannot make any alterations in this list as this should only provide you with the information whether everything has been loaded and saved properly. If you want to make any changes, please do so in the JSON files found at: " + Application.persistentDataPath + "/JSON/"));
#endif
                if (workflowSaveProperty.arraySize == 0)
                    workflowSaveProperty.isExpanded = true;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), workflowSaveProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            workflowSaveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (workflowSaveProperty.isExpanded) {
                    var element = workflowSaveList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, false);
                    if (element.isExpanded) {
                        float heightModifier = EditorGUI.GetPropertyHeight(element.FindPropertyRelative("workflowSnippets"), true);
                        if (element.FindPropertyRelative("workflowSnippets").arraySize == 0)
                            heightModifier = EditorGUIUtility.singleLineHeight * 2;
                        float constraintsY = rect.y + 4 * (2 + EditorGUIUtility.singleLineHeight) + heightModifier;

                        EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + 1 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("workflowID"), false);
                        EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + 2 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("workflowName"), false);

#if UNITY_WSA || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA_10_0
                        CreateReorderableList(serializedObject, element.FindPropertyRelative("workflowSnippets"), new GUIContent(" " + "Workflow Snippet IDs", "The Snippets with the IDs contained in this List belong to the Workflow. You can find the Save File in the  " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/TaskPlanningInterface/JSON/Snippets folder by looking for the correct ID."), "No Snippets were added to this workflow.").DoList(new Rect(rect.x + 10, rect.y + 3 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight));
                        CreateReorderableList(serializedObject, element.FindPropertyRelative("workflowConstraints"), new GUIContent(" " + "Workflow Constraint IDs", "The Constraints with the IDs contained in this List belong to the Workflow. You can find the Save File in the " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/TaskPlanningInterface/JSON/Constraints folder by looking for the correct ID."), "No Constraints were added to this workflow.").DoList(new Rect(rect.x + 10, constraintsY, rect.width - 3, EditorGUIUtility.singleLineHeight));
#else
                CreateReorderableList(serializedObject, element.FindPropertyRelative("workflowSnippets"), new GUIContent(" " + "Workflow Snippet IDs", "The Snippets with the IDs contained in this List belong to the Workflow. You can find the Save File in the  " + Application.persistentDataPath + "/JSON/Snippets folder by looking for the correct ID."), "No Snippets were added to this workflow.").DoList(new Rect(rect.x + 10, rect.y + 3 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight));
                        CreateReorderableList(serializedObject, element.FindPropertyRelative("workflowConstraints"), new GUIContent(" " + "Workflow Constraint IDs", "The Constraints with the IDs contained in this List belong to the Workflow. You can find the Save File in the " + Application.persistentDataPath + "/JSON/Constraints folder by looking for the correct ID."), "No Constraints were added to this workflow.").DoList(new Rect(rect.x + 10, constraintsY, rect.width - 3, EditorGUIUtility.singleLineHeight));
#endif


                    }

                }
            };

            // Set height of the entries according to whether they are folded out or not
            workflowSaveList.elementHeightCallback = (index) => {
                if (!workflowSaveProperty.isExpanded)
                    return 0;
                var height = workflowSaveList.elementHeight;
                if (workflowSaveList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height += EditorGUI.GetPropertyHeight(workflowSaveList.serializedProperty.GetArrayElementAtIndex(index), false);
                    height += 2 * (2 + EditorGUIUtility.singleLineHeight);
                    if(!workflowSaveList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("workflowConstraints").isExpanded)
                        height += (2 + EditorGUIUtility.singleLineHeight);
                    if (workflowSaveList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("workflowSnippets").arraySize != 0)
                        height += EditorGUI.GetPropertyHeight(workflowSaveList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("workflowSnippets"), true);
                    else
                        height += EditorGUIUtility.singleLineHeight * 2.5f;
                    if(workflowSaveList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("workflowConstraints").arraySize != 0)
                        height += EditorGUI.GetPropertyHeight(workflowSaveList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("workflowConstraints"), true);
                    else
                        height += EditorGUIUtility.singleLineHeight * 2.5f;

                }
                return height;
            };

            // What happens if a new element is added to the list?
            workflowSaveList.onAddCallback = (ReorderableList l) => {
                return;
            };

            // What happens if an element is removed from the list?
            workflowSaveList.onCanRemoveCallback = (ReorderableList l) => {
                return false;
            };

        }

        public ReorderableList CreateReorderableList(SerializedObject serializedObject, SerializedProperty serializedProperty, GUIContent content, string emptyMessage) {

            reorderableList = new ReorderableList(serializedObject, serializedProperty, false, true, false, false);

            // List menu name
            reorderableList.drawHeaderCallback = (Rect rect) => {
                serializedProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), serializedProperty.isExpanded, content);
                if (serializedProperty.arraySize == 0)
                    serializedProperty.isExpanded = true;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), serializedProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (serializedProperty.isExpanded) {
                    var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.LabelField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.displayName);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            reorderableList.elementHeightCallback = (index) => {
                if (!serializedProperty.isExpanded)
                    return 0;
                return reorderableList.elementHeight;
            };

            // What happens if a new element is added to the list?
            reorderableList.onAddCallback = (ReorderableList l) => {
                return;
            };

            // What happens if an element is removed from the list?
            reorderableList.onCanRemoveCallback = (ReorderableList l) => {
                return false;
            };

            reorderableList.drawNoneElementCallback = (Rect rect) => {
                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), emptyMessage);
                return;
            };

            return reorderableList;

        }

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/SaveController_header.png", typeof(Texture2D));
            GUILayout.Box(TPI_Banner, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.9f), GUILayout.Height(EditorGUIUtility.currentViewWidth * 0.23f));
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Header Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 25;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Save Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Info Box
            EditorGUILayout.HelpBox("The SaveController manages the saving and loading of data, especially of Workflows, Snippets and Constraints.\n" +
                "To make sure, that Snippets (and Constraints) are saved properly, please create your own class that inherits from TPI_SnippetSaveData (TPI_ConstraintSaveData). For instance: public class Example_SaveData : TPI_SnippetSaveData {}\n" +
                "Once you have done that, please implement the InitializeSaveData() function in your class that inherits from TPI_Snippet (TPI_Constraint) exactly how it is described in the 'Data Management' section of the TPI_Snippet (TPI_Constraint) class.\n" +
                "Then, if you have done so, please feel free to setup other variables in your class that inherits from TPI_SnippetSaveData (TPI_ConstraintSaveData) that should be setup during the initialization of your Snippet (Constraint).\n" +
                "By following exactly these steps, the SaveController is able to not only save and load the TPI internal data but also the data that was created by you.", MessageType.Info);
            EditorGUILayout.Space(15);



            //---------------------------------------------------- General Save Options ----------------------------------------------------//



            /*// General Save Options Text
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent("General Save Options", "Options relating to the saving and loading of data in general, especially of Workflows, Snippets and Constraints"), titleStyle);
            EditorGUILayout.Space(15);*/

            workflowSaveList.DoLayoutList();
            EditorGUILayout.Space();

            // Info Box
            EditorGUILayout.HelpBox("Please visit the 'Supported types' section of the 'JSON Serialization Unity Documentation' to inform yourself about the supported types for saving. You can open the website by clicking on the button below. Other types of variables are not supported and will thus not be saved.", MessageType.Warning);

            // Unity Documentation Button
            if(GUILayout.Button(new GUIContent("Open 'JSON Serialization Unity Documentation'", "Click this Button to open the Unity Documentation about the JSON Serialization.")))
                Application.OpenURL("https://docs.unity3d.com/2020.1/Documentation/Manual/JSONSerialization.html#:~:text=is%20not%20supported.-,Supported%20types,-The%20JSON%20Serializer");

            EditorGUILayout.Space();
            // Info Box
            EditorGUILayout.HelpBox("If you want to make any changes to the saves, please do so in the JSON files found by clicking on the Button below.", MessageType.Warning);

            // Save Folder Location Button
            if (GUILayout.Button(new GUIContent("Open Project Save Folder", "Click this Button to open the Project Save Folder, where you can find the JSON files."))) {

#if UNITY_WSA || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA_10_0
                EditorUtility.RevealInFinder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/TaskPlanningInterface/JSON/");
#else
                EditorUtility.RevealInFinder(Application.persistentDataPath + "/JSON/");
#endif

            }

            serializedObject.ApplyModifiedProperties();

        }

    }

}