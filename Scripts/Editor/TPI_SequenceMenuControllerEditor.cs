using TaskPlanningInterface.Controller;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_SequenceMenuController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_SequenceMenuController in this instance, you also have to edit this script to reflect said changes.
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
    [CustomEditor(typeof(TPI_SequenceMenuController))]
    public class TPI_SequenceMenuControllerEditor : Editor {

        private ReorderableList snippetsList;
        private ReorderableList constraintsList;

        private SerializedProperty snippetsProperty;
        private SerializedProperty constraintsProperty;

        private void OnEnable() {



            //---------------------------------------------------- Snippets List ----------------------------------------------------//



            snippetsProperty = serializedObject.FindProperty("_snippetObjects");
            snippetsList = new ReorderableList(serializedObject, snippetsProperty, true, true, false, false);

            // List menu name
            snippetsList.drawHeaderCallback = (Rect rect) => {
                snippetsProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), snippetsProperty.isExpanded, new GUIContent(" Created Snippets", "Information of all the Snippets of the current Workflow sorted by their correct position in the Sequence. You cannot make any alterations in this list as this should only provide you with the information whether everything has been created properly."));
                snippetsList.draggable = snippetsProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), snippetsProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            snippetsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (snippetsProperty.isExpanded) {
                    var element = snippetsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, true);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            snippetsList.elementHeightCallback = (index) => {
                if (!snippetsProperty.isExpanded)
                    return 0;
                var height = EditorGUIUtility.singleLineHeight;
                if (snippetsList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height = EditorGUIUtility.singleLineHeight * 5; /////////////////////////////////////////////////////////////////// CHANGE THIS VALUE IF MORE ELEMENTS ARE ADDED
                }
                return height;
            };

            // What happens if a new element is added to the list?
            snippetsList.onAddCallback = (ReorderableList l) => {
                return;
            };

            // What happens if an element is removed from the list?
            snippetsList.onCanRemoveCallback = (ReorderableList l) => {
                return false;
            };



            //---------------------------------------------------- Constraints List ----------------------------------------------------//



            constraintsProperty = serializedObject.FindProperty("_constraintObjects");
            constraintsList = new ReorderableList(serializedObject, constraintsProperty, true, true, false, false);

            // List menu name
            constraintsList.drawHeaderCallback = (Rect rect) => {
                constraintsProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), constraintsProperty.isExpanded, new GUIContent(" Created Constraints (both global and snippet-specific)", "Information of all the Constraints (both global and snippet-specific) of the current Workflow. You cannot make any alterations in this list as this should only provide you with the information whether everything has been created properly."));
                constraintsList.draggable = constraintsProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), constraintsProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            constraintsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (constraintsProperty.isExpanded) {
                    var element = constraintsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, true);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            constraintsList.elementHeightCallback = (index) => {
                if (!constraintsProperty.isExpanded)
                    return 0;
                var height = EditorGUIUtility.singleLineHeight;
                if (constraintsList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height = EditorGUIUtility.singleLineHeight * 5; /////////////////////////////////////////////////////////////////// CHANGE THIS VALUE IF MORE ELEMENTS ARE ADDED
                }
                return height;
            };

            // What happens if a new element is added to the list?
            constraintsList.onAddCallback = (ReorderableList l) => {
                return;
            };

            // What happens if an element is removed from the list?
            constraintsList.onCanRemoveCallback = (ReorderableList l) => {
                return false;
            };

        }

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/SequenceMenuController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("Sequence Menu Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Info Box
            EditorGUILayout.HelpBox("The SequenceMenuController manages the Snippets and Constraints, which were selected in the Building Blocks Menu and configured in separate Dialog Menus.\n" +
                "Futhermore, it handles the Sequence and the underlying functions thereof, calling the Snippet and Constraint functions as demanded. " +
                "For instance, if the operator decides to start the Snippet Sequence, the SequenceMenuController calls the RunSnippet() Coroutine of the specific Snippet and starts the global and snippet-specifc Constraints. " +
                "Once the Coroutine of a snippet has ended, it automatically starts the next snippet and handles the specific Constraints.\n" +
                "There are various functions you can use to alter the Sequence of Snippets and the List of both global and snippet-specific Constraints.\n" +
                "In total, there are 8 main Sequence Functions: StartSequence, StopSequence, RestartSequence, TogglePauseSnippet (Pause & Unpause), SkipSnippet, RepeatSnippet, ReturnToPreviousSnippet and finally EmergencyStopSnippet.", MessageType.Info);
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
            EditorGUILayout.LabelField(new GUIContent("General Options", "Options relating to the general behaviour of the Sequence Menu"), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // General Snippets Text
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent("Created Snippets", "This section relates to all the options about the Snippets that have been created with the help of the Building Blocks Menu."), titleStyle);
            EditorGUILayout.Space(15);

            // Created Snippets List
            snippetsList.DoLayoutList();
            EditorGUILayout.Space();

            // Snippets Prefab
            serializedObject.FindProperty("snippetPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Snippet Button Prefab:", "Add the Prefab of a Snippet Sequence Menu Button (whatever should be shown in the Sequence Menu)"), serializedObject.FindProperty("snippetPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space(15);

            // Created Constraints Text
            EditorGUILayout.LabelField(new GUIContent("Created Constraints", "This section relates to all the options about the Constraints that have been created with the help of the Building Blocks Menu."), titleStyle);
            EditorGUILayout.Space(15);

            // Created Constraints List
            constraintsList.DoLayoutList();
            EditorGUILayout.Space();

            // Constraints Prefab
            serializedObject.FindProperty("constraintPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Constraint Button Prefab:", "Add the Prefab of a Constraint Sequence Menu Button (whatever should be shown in the Sequence Menu)"), serializedObject.FindProperty("constraintPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space(15);

            // References
            EditorGUILayout.LabelField(new GUIContent("Dynamic Scroll Container References", "Please add a reference to the objects in this section in the Inspector."), titleStyle);
            EditorGUILayout.Space(15);

            // Snippet Container GameObject
            serializedObject.FindProperty("snippetContainerPath").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Snippets:", "Add the GameObject from the Editor in which the Snippet Buttons will be instantiated in. If you did not change anything, it should be the SnippetGridObjectCollection GameObject found at: Sequence_Menu/BodyContent/SnippetDynamicScrollPopulator"), serializedObject.FindProperty("snippetContainerPath").objectReferenceValue, typeof(GameObject), true);
            EditorGUILayout.Space();

            // Global Constraint Container GameObject
            serializedObject.FindProperty("globalConstraintContainerPath").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Global Constraints:", "Add the GameObject from the Editor in which the Global Constraint buttons will be instantiated in. If you did not change anything, it should be the GlobalConstraintsGridObjectCollection GameObject found at: Sequence_Menu/BodyContent/GlobalConstraintDynamicScrollPopulator"), serializedObject.FindProperty("globalConstraintContainerPath").objectReferenceValue, typeof(GameObject), true);
            EditorGUILayout.Space();

            // Snippet Specific Constraint Container GameObject
            serializedObject.FindProperty("specificConstraintContainerPath").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Specific Constraints:", "Add the GameObject from the Editor in which the Snippet-Specific Constraint buttons will be instantiated in. If you did not change anything, it should be the SpecificConstraintsGridObjectCollection GameObject found at: Sequence_Menu/BodyContent/SpecificConstraintDynamicScrollPopulator"), serializedObject.FindProperty("specificConstraintContainerPath").objectReferenceValue, typeof(GameObject), true);
            EditorGUILayout.Space();

            // Options concerning the button events
            EditorGUILayout.LabelField(new GUIContent("Button Event Options", "Change these options to alter what happens if a snippet or constraint button is pressed or held."), titleStyle);
            EditorGUILayout.Space(15);

            // Time available to Double Click
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standardButtonPressedCooldown"), new GUIContent("Time available to Double Click:", "Time given to the operator in order to perform a double click on a Snippet in seconds"));
            EditorGUILayout.Space();

            // standard hold time
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonHoldTime"));
            EditorGUILayout.Space();

            // require variable change confirmation
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requireVariableChangeConfirmation"));
            EditorGUILayout.Space();

            // require deletion confirmation
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requireDeletionConfirmation"));
            EditorGUILayout.Space();


            //---------------------------------------------------- Sequence Options ----------------------------------------------------//



            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // General Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Sequence Options", "Options relating to the general behaviour of the Sequence Menu"), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Sequence State Enum
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sequenceState"));
            EditorGUILayout.Space();

            // Sequence State Enum
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snippetProgression"));
            EditorGUILayout.Space();

            if (serializedObject.FindProperty("snippetProgression").enumValueIndex == 2) {
                // Indent content
                EditorGUI.indentLevel++;

                // robot base publish frequency
                EditorGUILayout.PropertyField(serializedObject.FindProperty("snippetProgressionDelay"), new GUIContent("Delay of Execution (s)", "In the case of snippetProgression being set to start with delay, set how many seconds should be waited until the next snippet is started."));
                EditorGUILayout.Space();

                // Remove content indent
                EditorGUI.indentLevel--;
            }

            // Snippet Visualization Speed
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snippetVisualizationSpeed"));
            EditorGUILayout.Space();

            // Sequence Functions Container GameObject
            serializedObject.FindProperty("coroutineButtonsContainerPath").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sequence Buttons Container:", "Add the GameObject from the Editor in which the Sequence Buttons are located in.  If you did not change anything, it should be the SpecificConstraintsGridObjectCollection GameObject found at: SequenceFunctions_Menu/SequenceButtonCollection/"), serializedObject.FindProperty("coroutineButtonsContainerPath").objectReferenceValue, typeof(GameObject), true);
            EditorGUILayout.Space();

            // Icon Options Text
            EditorGUILayout.LabelField(new GUIContent("Icon Options", "Please add the Icons for the following items."), titleStyle);
            EditorGUILayout.Space(15);

            // snippet visualization Texture
            serializedObject.FindProperty("snippetVisualizationIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Snippet Visualization Icon:", "Add the Icon that should be shown on the Visualize Snippet Button of the Sequence Functions"), serializedObject.FindProperty("snippetVisualizationIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // constraint visualization Texture
            serializedObject.FindProperty("constraintVisualizationIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Constraint Visualization Icon:", "Add the Icon that should be shown on the Visualize Constraints Button of the Sequence Functions"), serializedObject.FindProperty("constraintVisualizationIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // stop visualization Texture
            serializedObject.FindProperty("stopVisualizationIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Stop Visualization Icon:", "Add the Icon that should be shown on either the Visualize Snippet or Visualize Constraints Button in order to stop the visualization"), serializedObject.FindProperty("stopVisualizationIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // Pause Button Icon
            serializedObject.FindProperty("PauseIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Pause Button Icon:", "Add the Icon that should be shown on the Pause Button of the Sequence Functions"), serializedObject.FindProperty("PauseIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // Unpause Button Icon
            serializedObject.FindProperty("UnpauseIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Unpause Button Icon:", "Add the Icon that should be shown on the Unpause Button of the Sequence Functions"), serializedObject.FindProperty("UnpauseIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();


            // Info Box
            EditorGUILayout.HelpBox("Please make sure to add all the prefabs, as the 'Task Planning Interface' does not work without them.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();

        }

    }

}