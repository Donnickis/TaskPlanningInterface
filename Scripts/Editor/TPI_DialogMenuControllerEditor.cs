using System;
using TaskPlanningInterface.Controller;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_DialogMenuController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_DialogMenuController in this instance, you also have to edit this script to reflect said changes.
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
    [CustomEditor(typeof(TPI_DialogMenuController))]
    public class TPI_DialogMenuControllerEditor : Editor {

        private ReorderableList prefabList;

        private SerializedProperty prefabProperty;

        private void OnEnable() {



            //---------------------------------------------------- DialogMenuPrefab List ----------------------------------------------------//



            prefabProperty = serializedObject.FindProperty("dialogMenuPrefabList");
            prefabList = new ReorderableList(serializedObject, prefabProperty, true, true, true, true);

            // List menu name
            prefabList.drawHeaderCallback = (Rect rect) => {
                prefabProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), prefabProperty.isExpanded, new GUIContent(" Dialog Menu Templates", "Add, alter or remove Dialog Menu Templates by changing the information of them contained in this list"));
                if (prefabProperty.arraySize == 0)
                    prefabProperty.isExpanded = true;
                prefabList.draggable = prefabProperty.isExpanded;
                prefabList.displayAdd = prefabProperty.isExpanded;
                prefabList.displayRemove = prefabProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), prefabProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            prefabList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (prefabProperty.isExpanded) {
                    var element = prefabList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, true);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            prefabList.elementHeightCallback = (index) => {
                if (!prefabProperty.isExpanded)
                    return 0;
                var height = prefabList.elementHeight;
                if (prefabList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height += EditorGUI.GetPropertyHeight(prefabList.serializedProperty.GetArrayElementAtIndex(index), true);
                }
                return height;
            };


            // What happens if a new element is added to the list?
            // Important as it blocks the list from cloning the last element when a new element is added
            prefabList.onAddCallback = (ReorderableList l) => {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("dialogMenuName").stringValue = "";
                element.FindPropertyRelative("dialogMenuPrefab").objectReferenceValue = null;
                element.FindPropertyRelative("dialogMenuTexts").ClearArray();
                element.FindPropertyRelative("dialogMenuButtons").ClearArray();
                element.FindPropertyRelative("keyboardInputFieldTitles").ClearArray();
                element.FindPropertyRelative("checkboxTitles").ClearArray();
                element.FindPropertyRelative("toggleTitles").ClearArray();
                element.FindPropertyRelative("dialogMenuDropdowns").ClearArray();

                Guid guid = Guid.NewGuid();
                element.FindPropertyRelative("dialogMenuID").stringValue = guid.ToString();

            };


        }

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/DialogMenuController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("Dialog Menu Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Info Box
            EditorGUILayout.HelpBox("The DialogMenuController helps you in easily creating complex and good looking Dialog Menus to your liking.\n" +
                "For a detailed introduction to creating and managing Dialog Menus, please visit the DialogMenuController C# script and thoroughly read the Tutorial section.\n" +
                "This Unity Inspector section contains the following main feature: Setting up the Dialog Menu Templates", MessageType.Info);
            EditorGUILayout.Space(15);



            //---------------------------------------------------- Dialog Menu Options ----------------------------------------------------//



            // General Options Text
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent("General Options", "Options relating to the dialog menus in general, including for examples the templates of them."), titleStyle);
            EditorGUILayout.Space(15);

            // Dialog Prefab List
            prefabList.DoLayoutList();
            EditorGUILayout.Space();

            // Dropdown Selection Menu Prefab
            serializedObject.FindProperty("pointSelectionSphere").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Point Selection Sphere Prefab:", "Object instantiated if the operator wants to select a point in the environment."), serializedObject.FindProperty("pointSelectionSphere").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();



            //---------------------------------------------------- Error / Alert Menu Options ----------------------------------------------------//



            // Error / Alert Menu Options Text
            EditorGUILayout.LabelField(new GUIContent("Error / Alert Dialog Menu Options", "Options relating to the Error / Alert Dialog Menu that can be spawned by using the ShowErrorMenu function of the DialogMenuController. An instance, where it might be used, is to either hightlight to the operator that there was an error or to alert the operator to a specific fact."), titleStyle);
            EditorGUILayout.Space(15);

            // Error Menu Prefab
            serializedObject.FindProperty("errorMenuPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Menu Prefab with 1 button:", "Add the Prefab of the Error / Alert Dialog Menu that points out an error to the operator or alerts the operator to a specific fact"), serializedObject.FindProperty("errorMenuPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Error Menu Prefab
            serializedObject.FindProperty("errorMenuPrefab_twoButtons").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Menu Prefab with 2 buttons:", "Add the Prefab of the Error / Alert Dialog Menu with two buttons that points out an error to the operator or alerts the operator to a specific fact"), serializedObject.FindProperty("errorMenuPrefab_twoButtons").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Error Accept Menu Button Icon
            serializedObject.FindProperty("errorAcceptButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Accept Button Icon:", "Choose what accept Icon should be shown next to the Button in the Error / Alert Dialog Menu"), serializedObject.FindProperty("errorAcceptButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // Error Decline Menu Button Icon
            serializedObject.FindProperty("errorDeclineButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Abort Button Icon:", "Choose what decline Icon should be shown next to the Button in the Error / Alert Dialog Menu"), serializedObject.FindProperty("errorDeclineButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();



            //---------------------------------------------------- Object Name Menu Options ----------------------------------------------------//



            // Object Name Menu Options Text
            EditorGUILayout.LabelField(new GUIContent("Object Name Dialog Menu Options", "Options relating to the Object Name Dialog Menu that can be spawned by using the ShowObjectNameMenu function of the DialogMenuController. An instance, where it might be used, is to easily give a name to your Snippet or Constraint."), titleStyle);
            EditorGUILayout.Space(15);

            // Object Name Menu Prefab
            serializedObject.FindProperty("objectNameMenuPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Menu Prefab:", "Add the Prefab of the Object Name Menu that that allows the operator to choose a name for an object."), serializedObject.FindProperty("objectNameMenuPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Object Name Menu Button Icon
            serializedObject.FindProperty("objectNameButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Button Icon:", "Choose what Icon should be shown next to the Button in the Object Name Menu"), serializedObject.FindProperty("objectNameButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();



            //---------------------------------------------------- Constraint Type Selection Menu ----------------------------------------------------//



            // Constraint Type Selection Menu Options Text
            EditorGUILayout.LabelField(new GUIContent("Constraint Type Dialog Menu Options", "Options relating to the Constraint Type Selection Dialog Menu that can be spawned by using the ShowConstraintTypeMenu function of the DialogMenuController. It can be used to select whether a Constraint is either globally active or snippet-specific."), titleStyle);
            EditorGUILayout.Space(15);

            // Constraint Type Selection Menu Prefab
            serializedObject.FindProperty("constraintTypeMenuPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Menu Prefab:", "Add the Prefab of the Constraint Type Selection Dialog Menu that allows the operator to choose whether a Constraint is globally active or snippet-specific."), serializedObject.FindProperty("constraintTypeMenuPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Constraint Type Selection Menu Button Icon Global
            serializedObject.FindProperty("constraintTypeButtonIcon_global").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Global Constraint Button Icon:", "Choose what Icon should be shown next to the globally active Constraint Button in the Constraint Type Selection Dialog Menu"), serializedObject.FindProperty("constraintTypeButtonIcon_global").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // Constraint Type Selection Menu Button Icon Specific
            serializedObject.FindProperty("constraintTypeButtonIcon_specific").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Snippet-Specific Constraint Button Icon:", "Choose what Icon should be shown next to the snippet-specific Constraint Button in the Constraint Type Selection Dialog Menu"), serializedObject.FindProperty("constraintTypeButtonIcon_specific").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();



            //---------------------------------------------------- Dropdown Selection Menu Options ----------------------------------------------------//



            // Dropdown Selection Menu Options Text
            EditorGUILayout.LabelField(new GUIContent("Dropdown Selection Menu Options", "Options relating to the Dropdown Selection Dialog Menu that can be spawned by using the ShowDropdownSelectionMenu function of the DialogMenuController. An instance, where it might be used, is to easily give an operator the ability to choose an item from a list."), titleStyle);
            EditorGUILayout.Space(15);

            // Dropdown Selection Menu Prefab
            serializedObject.FindProperty("dropdownSelectionMenuPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Menu Prefab:", "Add the Prefab of the Dropdown Selection Menu that that allows the operator to choose an item from a dropdown list."), serializedObject.FindProperty("dropdownSelectionMenuPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Dropdown Selection Menu Button Icon
            serializedObject.FindProperty("dropdownSelectionButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Button Icon:", "Choose what Icon should be shown next to the Button in the Dropdown Selection Menu"), serializedObject.FindProperty("dropdownSelectionButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // Info Box
            EditorGUILayout.HelpBox("Please make sure to add all the prefabs, as the 'Task Planning Interface' does not work without them.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
        }

    }

}