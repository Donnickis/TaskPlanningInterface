using System;
using System.Collections.Generic;
using TaskPlanningInterface.Controller;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {
    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_WorkflowConfigurationController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_WorkflowConfigurationController in this instance, you also have to edit this script to reflect said changes.
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
    [CustomEditor(typeof(TPI_WorkflowConfigurationController))]
    public class TPI_WorkflowConfigurationControllerEditor : Editor {

        private ReorderableList categoriesList;
        private ReorderableList snippetList;
        private ReorderableList constraintList;

        private SerializedProperty categoriesProperty;
        private SerializedProperty snippetsProperty;
        private SerializedProperty constraintsProperty;

        // needed for custom Snippet Category ID Popup
        private List<string> categoryStrings = null;
        private List<string> categoryIDs = null;
        private List<int> selectedIndices = null;

        private void OnEnable() {



            //---------------------------------------------------- Categories List ----------------------------------------------------//



            categoriesProperty = serializedObject.FindProperty("categories");
            categoriesList = new ReorderableList(serializedObject, categoriesProperty, true, true, true, true);

            // needed for custom Snippet Category ID Popup
            if (categoryStrings == null)
                categoryStrings = new List<string>();
            if (categoryIDs == null)
                categoryIDs = new List<string>();
            if(selectedIndices == null)
                selectedIndices = new List<int>();

            // List menu name
            categoriesList.drawHeaderCallback = (Rect rect) => {
                categoriesProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), categoriesProperty.isExpanded, new GUIContent(" " + categoriesProperty.displayName, "Add, alter or remove Categories by changing the information of them contained in this list"));
                if (categoriesProperty.arraySize == 0)
                    categoriesProperty.isExpanded = true;
                categoriesList.draggable = categoriesProperty.isExpanded;
                categoriesList.displayAdd = categoriesProperty.isExpanded;
                categoriesList.displayRemove = categoriesProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), categoriesProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            categoriesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (categoriesProperty.isExpanded) {
                    var element = categoriesList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, true);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            categoriesList.elementHeightCallback = (index) => {
                if (!categoriesProperty.isExpanded)
                    return 0;
                var height = categoriesList.elementHeight;
                if (categoriesList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height += EditorGUI.GetPropertyHeight(categoriesList.serializedProperty.GetArrayElementAtIndex(index), true);
                }
                return height;
            };

            // What happens if a new element is added to the list?
            // Important as it blocks the list from cloning the last element when a new element is added
            categoriesList.onAddCallback = (ReorderableList l) => {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("categoryName").stringValue = "";
                element.FindPropertyRelative("categoryDescription").stringValue = "";
                element.FindPropertyRelative("categoryIcon").objectReferenceValue = null;
                element.FindPropertyRelative("categoryID").stringValue = Guid.NewGuid().ToString();

                // needed for custom Snippet Category ID Popup
                categoryStrings.Insert(index, "New Category at Index " + index);
                categoryIDs.Insert(index, element.FindPropertyRelative("categoryID").stringValue);
            };

            // What happens if an element is removed from the list?
            // needed for custom Snippet Category ID Popup
            categoriesList.onRemoveCallback = (ReorderableList l) => {
                categoryStrings.RemoveAt(l.index);
                categoryIDs.RemoveAt(l.index);

                for(int i = 0; i < selectedIndices.Count; i++) {
                    if (selectedIndices[i] == l.index) {
                        selectedIndices[i] = 0;
                    } else if (selectedIndices[i] > l.index) {
                        selectedIndices[i] = selectedIndices[i] - 1;
                    }
                }

                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            };

            // What happens if an element is moved in the list
            // needed for custom Snippet Category ID Popup
            categoriesList.onReorderCallbackWithDetails = (ReorderableList l, int oldIndex, int newIndex) => {
                for (int i = 0; i < selectedIndices.Count; i++) {
                    if (selectedIndices[i] == oldIndex) {
                        selectedIndices[i] = newIndex;
                    } else if (selectedIndices[i] == newIndex) {
                        selectedIndices[i] = oldIndex;
                    }
                }
                string backup = categoryStrings[oldIndex];
                categoryStrings[oldIndex] = categoryStrings[newIndex];
                categoryStrings[newIndex] = backup;

                backup = categoryIDs[oldIndex];
                categoryIDs[oldIndex] = categoryIDs[newIndex];
                categoryIDs[newIndex] = backup;
            };



            //---------------------------------------------------- Snippets List ----------------------------------------------------//



            UpdateSnippetCategoryPopup(true);
            snippetsProperty = serializedObject.FindProperty("snippetTemplates");
            snippetList = new ReorderableList(serializedObject, snippetsProperty, true, true, true, true);

            // List menu name
            snippetList.drawHeaderCallback = (Rect rect) => {
                snippetsProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), snippetsProperty.isExpanded, new GUIContent(" " + snippetsProperty.displayName, "Add, alter or remove Snippets by changing the information of them contained in this list"));
                if (snippetsProperty.arraySize == 0)
                    snippetsProperty.isExpanded = true;
                snippetList.draggable = snippetsProperty.isExpanded;
                snippetList.displayAdd = snippetsProperty.isExpanded;
                snippetList.displayRemove = snippetsProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), snippetsProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            snippetList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (snippetsProperty.isExpanded) {
                    var element = snippetList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, false);
                    if (element.isExpanded) {
                        EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + 1 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("snippetName"), false);

                        // Custom Snippet Category ID Popup
                        if (selectedIndices.Count < snippetsProperty.arraySize) {
                            if(categoryIDs.Contains(element.FindPropertyRelative("snippetCategoryID").stringValue))
                                selectedIndices.Add(categoryIDs.IndexOf(element.FindPropertyRelative("snippetCategoryID").stringValue));
                            else
                                selectedIndices.Add(0);
                        }
                            
                        if (selectedIndices[index] >= categoriesProperty.arraySize)
                            selectedIndices[index] = 0;

                        selectedIndices[index] = EditorGUI.Popup(new Rect(rect.x + 10, rect.y + 2 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight), "Selection Menu Category", selectedIndices[index], categoryStrings.ToArray());
                        element.FindPropertyRelative("snippetCategoryID").stringValue = categoriesProperty.GetArrayElementAtIndex(selectedIndices[index]).FindPropertyRelative("categoryID").stringValue;

                        element.FindPropertyRelative("snippetDescription").stringValue = EditorGUI.TextField(new Rect(rect.x + 10, rect.y + 3 * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, 3.5f * EditorGUIUtility.singleLineHeight), new GUIContent("Snippet Description", "Description of the underlying machine task of the snippet"), element.FindPropertyRelative("snippetDescription").stringValue);
                        EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + 6.5f * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("snippetIcon"), false);
                        EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y + 7.5f * (2 + EditorGUIUtility.singleLineHeight), rect.width - 3, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("functionObject"), false);
                        
                    }
                }
            };

            // Set height of the entries according to whether they are folded out or not
            snippetList.elementHeightCallback = (index) => {
                if (!snippetsProperty.isExpanded)
                    return 0;
                var height = snippetList.elementHeight;
                if (snippetList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height += EditorGUI.GetPropertyHeight(snippetList.serializedProperty.GetArrayElementAtIndex(index), true);
                }
                return height;
            };

            // What happens if a new element is added to the list?
            // Important as it blocks the list from cloning the last element when a new element is added
            snippetList.onAddCallback = (ReorderableList l) => {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("snippetName").stringValue = "";
                element.FindPropertyRelative("snippetDescription").stringValue = "";
                element.FindPropertyRelative("snippetCategoryID").stringValue = "";
                element.FindPropertyRelative("snippetIcon").objectReferenceValue = null;
                element.FindPropertyRelative("functionObject").objectReferenceValue = null;
                element.FindPropertyRelative("snippetID").stringValue = "";

                // needed for custom Snippet Category ID Popup
                selectedIndices.Insert(index, 0);
            };

            // What happens if an element is removed from the list?
            snippetList.onRemoveCallback = (ReorderableList l) => {
                selectedIndices.RemoveAt(l.index); // needed for custom Snippet Category ID Popup
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            };

            // What happens if an element is moved in the list
            // needed for custom Snippet Category ID Popup
            snippetList.onReorderCallbackWithDetails = (ReorderableList l, int oldIndex, int newIndex) => {
                int backupIndex = selectedIndices[oldIndex];
                selectedIndices[oldIndex] = selectedIndices[newIndex];
                selectedIndices[newIndex] = backupIndex;
            };



            //---------------------------------------------------- Constraints List ----------------------------------------------------//



            constraintsProperty = serializedObject.FindProperty("constraintTemplates");
            constraintList = new ReorderableList(serializedObject, constraintsProperty, true, true, true, true);

            // List menu name
            constraintList.drawHeaderCallback = (Rect rect) => {
                constraintsProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), constraintsProperty.isExpanded, new GUIContent(" " + constraintsProperty.displayName, "Add, alter or remove Constraints by changing the information of them contained in this list"));
                if (constraintsProperty.arraySize == 0)
                    constraintsProperty.isExpanded = true;
                constraintList.draggable = constraintsProperty.isExpanded;
                constraintList.displayAdd = constraintsProperty.isExpanded;
                constraintList.displayRemove = constraintsProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), constraintsProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            constraintList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (constraintsProperty.isExpanded) {
                    var element = constraintList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, true);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            constraintList.elementHeightCallback = (index) => {
                if (!constraintsProperty.isExpanded)
                    return 0;
                var height = constraintList.elementHeight;
                if (constraintList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height += EditorGUI.GetPropertyHeight(constraintList.serializedProperty.GetArrayElementAtIndex(index), true);
                }
                return height;
            };

            // What happens if a new element is added to the list?
            // Important as it blocks the list from cloning the last element when a new element is added
            constraintList.onAddCallback = (ReorderableList l) => {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("constraintName").stringValue = "";
                element.FindPropertyRelative("constraintDescription").stringValue = "";
                element.FindPropertyRelative("constraintType").enumValueIndex = 0;
                element.FindPropertyRelative("constraintIcon").objectReferenceValue = null;
                element.FindPropertyRelative("functionObject").objectReferenceValue = null;
                element.FindPropertyRelative("snippetID").stringValue = "";
                element.FindPropertyRelative("constraintID").stringValue = "";
            };

        }


        // needed for custom Snippet Category ID Popup
        private void UpdateSnippetCategoryPopup(bool firstTime) {
            
            // Resource efficient version to generate the correct list for the popup
            if (!firstTime && categoryStrings.Count == categoriesProperty.arraySize) {
                for (int i = 0; i < categoriesProperty.arraySize; i++) {
                    if (categoriesProperty.GetArrayElementAtIndex(i).isExpanded) {
                        string categoryName = categoriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("categoryName").stringValue;
                        if (categoryName == "")
                            categoryName = "New Category at Index " + i;
                        categoryStrings[i] = categoryName;
                        categoryIDs[i] = categoriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("categoryID").stringValue;
                    }
                }
            } else {
                if(!firstTime) {
                    categoryStrings.Clear();
                    categoryIDs.Clear();
                }
                for (int i = 0; i < categoriesProperty.arraySize; i++) {
                    string categoryName = categoriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("categoryName").stringValue;
                    if (categoryName == "")
                        categoryName = "New Category at Index " + i;
                    categoryStrings.Add(categoryName);
                    categoryIDs.Add(categoriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("categoryID").stringValue);
                }
            }
            
        }


        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();
            if (categoriesProperty.isExpanded) {
                UpdateSnippetCategoryPopup(false);
            }



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/WorkflowConfigurationController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("Workflow Configuration Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The WorkflowConfigurationController helps you in making changes to the two first main menus that you will see, the 'Selection Menu', being responsible for choosing the desired Category, and the 'Building Blocks Menu', being responsible for choosing the desired Snippet or Constraint.\n" +
                "As those two menus handle the Snippet and Constraint Templates, they are responsible for populating the Workflow. " +
                "Once the operator has selected the desired Snippet or Constraint from their respective Category, the WorkflowConfigurationController automatically calls the ButtonPressed function found in the script underlying the Snippet or Constraint.\n" +
                "This Unity Inspector section contains the following main features: Setting up the Snippet Categories, setting up the Snippet Templates and Setting up the Constraint Templates.", MessageType.Info);
            EditorGUILayout.Space(15);



            //---------------------------------------------------- Selection Menu Options ----------------------------------------------------//



            // Selection Menu Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle menuStyle = new GUIStyle();
            menuStyle.fontSize = 22;
            menuStyle.fontStyle = FontStyle.Bold;
            menuStyle.normal.textColor = Color.white;
            menuStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(new GUIContent("Selection Menu Options", "Options relating to the Selection Menu, which automatically opens once a new Workflow is created or an existing one is loaded."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // buttonsPerRow
            serializedObject.FindProperty("buttonsPerRow").intValue = EditorGUILayout.IntField(new GUIContent("Categories per Row:", "How many Categories per row should be shown in the Selection Menu?"), serializedObject.FindProperty("buttonsPerRow").intValue);
            EditorGUILayout.Space();

            // Categories List
            categoriesList.DoLayoutList();
            EditorGUILayout.Space();

            // Category Button Prefab
            serializedObject.FindProperty("categoryButtonPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Category Button Prefab:", "Add the prefab of a Category Button (whatever should be instantiated in the Selection Menu)"), serializedObject.FindProperty("categoryButtonPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Category Container Prefab
            serializedObject.FindProperty("categoryContainerPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Category Contrainer Prefab:", "Add the prefab of a Category Container, in which the objects belonging to that category will be placed (whatever should be instantiated in the background -> not visible)"), serializedObject.FindProperty("categoryContainerPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Constraints Category Button icon
            serializedObject.FindProperty("categoryIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Constraints Category Icon: (optional)", "Choose what Icon should be shown next to the 'Constraints' Category in the Selection Menu"), serializedObject.FindProperty("categoryIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;



            //---------------------------------------------------- If there is the desire to create custom Workflows at runtime in the future, you can enable the following things: ----------------------------------------------------//



            /*// New Snippet at runtime Text
            EditorGUILayout.LabelField(new GUIContent("New Snippet at runtime Options", "Options relating to the function of creating new Snippets at runtime"), titleStyle);
            EditorGUILayout.Space(15);

            // New Snippet Button Name
            serializedObject.FindProperty("newSnippetButtonName").stringValue = EditorGUILayout.TextField(new GUIContent("New Snippet Button Text: (optional)", "What should the 'Add new Snippet at runtime' Button in the Selection Menu be called?"), serializedObject.FindProperty("newSnippetButtonName").stringValue);
            EditorGUILayout.Space();

            // New Snippet Button Icon
            serializedObject.FindProperty("newSnippetButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("New Snippet Icon: (optional)", "Choose what Icon should be shown next to the 'Add new Snippet at runtime' Button in the Selection Menu"), serializedObject.FindProperty("newSnippetButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();*/



            //---------------------------------------------------- Building Blocks Menu Options ----------------------------------------------------//



            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Building Blocks Menu Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Building Blocks Menu Options", "Options relating to the Building Blocks Menu, which can be accessed by clicking on one of the Categories Buttons in the Selection Menu. It is automatically configured for the respective Category."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // objectsPerRow
            serializedObject.FindProperty("objectsPerRow").intValue = EditorGUILayout.IntField(new GUIContent("Objects per Row:", "How many objects per row should be shown in the Building Block Menu?"), serializedObject.FindProperty("objectsPerRow").intValue);
            EditorGUILayout.Space();

            // Snippets Text
            EditorGUILayout.LabelField(new GUIContent("Snippets", "Options relating to Snippet Templates in the Building Blocks Menu."), titleStyle);
            EditorGUILayout.Space(15);

            // Snippets List
            snippetList.DoLayoutList();
            EditorGUILayout.Space();

            // Snippet Button Prefab
            serializedObject.FindProperty("snippetButtonPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Snippet Button Prefab:", "Add the prefab of a Snippet Template Button (whatever should be instantiated in the correct Category of the Building Blocks Menu)"), serializedObject.FindProperty("snippetButtonPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Show Template Name Snippet
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showTemplateName_snippet"), new GUIContent("Show Snippet Template Name:", "Should the Snippet Template name be shown (e.g. MoveTo)?"));
            EditorGUILayout.Space();

            // Characters Showed Snippet
            if (serializedObject.FindProperty("showTemplateName_snippet").boolValue) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("charactersShowed_snippet"), new GUIContent("Number of Characters shown:", "How many characters of the Template Name should be shown? To show the full name, set it to -1."));
                EditorGUILayout.Space();
            }

            // Constraints Text
            EditorGUILayout.LabelField(new GUIContent("Constraints", "Options relating to Constraint Templates in the Building Blocks Menu."), titleStyle);
            EditorGUILayout.Space(15);

            // Constraints List
            constraintList.DoLayoutList();

            // Constraint Button Prefab
            serializedObject.FindProperty("constraintButtonPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Constraint Button Prefab:", "Add the prefab of a Constraint Template Button (whatever should be instantiated in the correct Category of the Building Blocks Menu)"), serializedObject.FindProperty("constraintButtonPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Show Template Name Constraint
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showTemplateName_constraint"), new GUIContent("Show Constraint Template Name:", "Should the Constraint Template name be shown (e.g. NoGoZone)?"));
            EditorGUILayout.Space();

            // Characters Showed Constraint
            if (serializedObject.FindProperty("showTemplateName_constraint").boolValue) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("charactersShowed_constraint"), new GUIContent("Number of Characters shown:", "How many characters of the Template Name should be shown? To show the full name, set it to -1."));
                EditorGUILayout.Space();
            }


            // Info Box
            EditorGUILayout.HelpBox("Please make sure to add all the prefabs, as the 'Task Planning Interface' does not work without them.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();

        }

    }

}