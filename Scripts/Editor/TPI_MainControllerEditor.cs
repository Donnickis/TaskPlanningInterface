using TaskPlanningInterface.Controller;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_MainController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_MainController in this instance, you also have to edit this script to reflect said changes.
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
    [CustomEditor(typeof(TPI_MainController))]
    public class TPI_MainControllerEditor : Editor {

        
        private SerializedProperty isOpenProperty;
        bool foldoutValue_textures;
        bool foldoutValue_references;

        private void OnEnable() {

            isOpenProperty = serializedObject.FindProperty(nameof(TPI_MainController.isOpen));

            foldoutValue_textures = false;
            foldoutValue_references = false;

        }

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/MainController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("TPI Main Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("Welcome to the Task Planning Interface (TPI), a bachelor thesis made by Yannick Huber.\n" +
                "The TPI allows you to to create your own 'Workflow', which is made up of a sequence of individual machine operations, lovingly called 'Snippets'. " +
                "Examples of Snippets include the 'Move To' task, the 'Bring Me' task or the 'Pick Up' task. " +
                "These Snippets are premade templates in the unity editor, which can be setup and configured at runtime on your Hololens or different kind of Augmented or Mixed Reality Device.\n" +
                "To further enhance your Workflow you can assign specific Constraints to each snippet. " +
                "Examples for Constraints include a max weight, max force, max speed or the ability to designate a No-Go Zone, which prevents the machine from accessing a specific area. " +
                "Additionally, you can also assign these Constraints to the entire Workflow (globally), applying them to each Snippet at the same time.\n" +
                "Initially, the TPI is designed to work with a robot arm, but could be extended to other kinds of machines.\n" +
                "Finally, I'd like to thank you for using the TPI! Enjoy using it!", MessageType.Info);
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
            EditorGUILayout.LabelField(new GUIContent("General Options", "Options relating to the general behaviour of the TPI"), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // isOpen bool
            EditorGUILayout.PropertyField(isOpenProperty);
            EditorGUILayout.Space();



            //---------------------------------------------------- Hand Menu Options ----------------------------------------------------//



            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Hand Menu Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Hand Menu Options", "Options relating to the Hand Menu, which you can find by looking at the palm of either of your hands. The Hand Menu contains the options to start and toggle the TPI, load a previous Workflow, save the current Workflow and the option to start and stop the Tutorial."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // Workflow Management Menu Options Text
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent("Workflow Management Menu Options", "Options relating to the Dialog Menu that gives the operator the option to manage the workflows once the ManageWorkflows Button in the HandMenu is pressed."), titleStyle);
            EditorGUILayout.Space(20);


            // Workflow Management Menu Prefab
            serializedObject.FindProperty("manageWorkflowsMenuPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Menu Prefab:", "Add the Prefab for the Dialog Menu that gives the operator the option to manage the workflows once the ManageWorkflows Button in the HandMenu is pressed."), serializedObject.FindProperty("manageWorkflowsMenuPrefab").objectReferenceValue, typeof(GameObject), false);
            EditorGUILayout.Space();

            // Workflow Management Menu Button Icon
            serializedObject.FindProperty("deleteButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Delete Button Texture:", "Add the delete icon that will be shown on the button in the Dialog Menu thatgives the operator the option to manage the workflows once the ManageWorkflows Button in the HandMenu is pressed."), serializedObject.FindProperty("deleteButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();

            // Workflow Management Menu Button Icon
            serializedObject.FindProperty("abortButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Abort Button Texture:", "Add the abort icon that will be shown on the button in the Dialog Menu thatgives the operator the option to manage the workflows once the ManageWorkflows Button in the HandMenu is pressed."), serializedObject.FindProperty("abortButtonIcon").objectReferenceValue, typeof(Texture2D), false);
            EditorGUILayout.Space();



            //---------------------------------------------------- Various Icon Options ----------------------------------------------------//



            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Various Icon Options Text
            EditorGUILayout.LabelField(new GUIContent("Various Icon Options", "Options relating to the icons various different parts of the HandMenu and the TaskPlanningInterface in general."), menuStyle);
            EditorGUILayout.Space(20);

            // References Folout
            foldoutValue_textures = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue_textures, new GUIContent("Icon Container Foldout", "Please add the icons relating to various different parts of the HandMenu and the TaskPlanningInterface in general."));
            EditorGUILayout.Space();

            if (foldoutValue_textures) {
                // Indent foldout content
                EditorGUI.indentLevel++;

                // Workflow Naming Menu Button Icon
                serializedObject.FindProperty("startButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Open Workflow Icon:", "Add the icon that will be shown on the button in the Dialog Menu that asks the operator to enter a Workflow Name once the CreateWorkflow Button in the HandMenu is pressed."), serializedObject.FindProperty("startButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Create Workflow Button Icon
                serializedObject.FindProperty("createWorkflowButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Create Workflow Icon:", "Add the icon that will be shown on the CreateWorkflow Button in the HandMenu to create a new workflow."), serializedObject.FindProperty("createWorkflowButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Hide TPI Button Icon
                serializedObject.FindProperty("hideButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Hide TPI Icon:", "Add the icon that will be shown on the ToggleTPI Button in the HandMenu to hide the TaskPlanningInterface."), serializedObject.FindProperty("hideButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Show TPI Menu Button Icon
                serializedObject.FindProperty("showButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Show TPI Icon:", "Add the icon that will be shown on the ToggleTPI Button in the HandMenu to show the TaskPlanningInterface if it is hidden."), serializedObject.FindProperty("showButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Workflow Loading Menu Button Icon
                serializedObject.FindProperty("loadButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Load Save Icon:", "Add the icon that will be shown on the button in the Dialog Menu that asks the operator to select and load a Workflow once the LoadWorkflow Button in the HandMenu is pressed."), serializedObject.FindProperty("loadButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Overwrite Save Menu Button Icon
                serializedObject.FindProperty("overwriteButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Overwrite Save Icon:", "Add the overwrite icon that will be shown on the button in the Dialog Menu that gives the operator the either overwrite an exisiting save or create a new one."), serializedObject.FindProperty("overwriteButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Create new Save Menu Button Icon
                serializedObject.FindProperty("createNewSaveButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Create new Save Icon:", "Add the create new save icon that will be shown on the button in the Dialog Menu that gives the operator the either overwrite an exisiting save or create a new one."), serializedObject.FindProperty("createNewSaveButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Reassign Button Icon
                serializedObject.FindProperty("reassignButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Reassign Icon:", "Add the icon that should be shown on the button that asks the operator to reassign something."), serializedObject.FindProperty("reassignButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Rename Button Icon
                serializedObject.FindProperty("renameButtonIcon").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Rename Icon:", "Add the icon that should be shown on the button in the Manage Workflows menu that renames the current workflow."), serializedObject.FindProperty("renameButtonIcon").objectReferenceValue, typeof(Texture2D), false);
                EditorGUILayout.Space();

                // Remove indent
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            //---------------------------------------------------- References ----------------------------------------------------//



            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Various Icon Options Text
            EditorGUILayout.LabelField(new GUIContent("References", "Please add the references to all the different Container GameObjects."), menuStyle);
            EditorGUILayout.Space(20);

            // References Folout
            foldoutValue_references = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue_references, new GUIContent("Reference Container Foldout", "Please add the references to all the different Container GameObjects."));
            EditorGUILayout.Space();

            if (foldoutValue_references) {
                // Indent foldout content
                EditorGUI.indentLevel++;

                // Selection Menu Container
                serializedObject.FindProperty("selectionMenu").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Selection Menu", "Please add the Reference to the Selection Menu of the TaskPlanningInterface."), serializedObject.FindProperty("selectionMenu").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Building Blocks Menu Container
                serializedObject.FindProperty("buildingBlocksMenu").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Building Blocks Menu", "Please add the Reference to the Building Blocks Menu of the TaskPlanningInterface."), serializedObject.FindProperty("buildingBlocksMenu").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Sequence Menu Container
                serializedObject.FindProperty("sequenceMenu").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sequence Menu", "Please add the Reference to the Sequence Menu of the TaskPlanningInterface."), serializedObject.FindProperty("sequenceMenu").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Sequence Functions Menu Container
                serializedObject.FindProperty("sequenceFunctionsMenu").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Sequence Functions Menu", "Please add the Reference to the Sequence Functions Menu of the TaskPlanningInterface."), serializedObject.FindProperty("sequenceFunctionsMenu").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Sequence Functions Menu Container
                serializedObject.FindProperty("rosStatusMenu").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("ROS Status Menu", "Please add the Reference to the ROS Status Menu of the TaskPlanningInterface."), serializedObject.FindProperty("rosStatusMenu").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // ROS Controller Container
                serializedObject.FindProperty("rosController").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("ROS Controller", "Please add the Reference to the ROS Controller of the TaskPlanningInterface."), serializedObject.FindProperty("rosController").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Object Placement Controller Container
                serializedObject.FindProperty("objectPlacementController").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Object Placement Controller", "Please add the Reference to the Object Placement Controller of the TaskPlanningInterface."), serializedObject.FindProperty("objectPlacementController").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Workflow HandMenu Container
                serializedObject.FindProperty("handMenu_workflow").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Workflow HandMenu", "Please add the Reference to the Workflow HandMenu of the TaskPlanningInterface."), serializedObject.FindProperty("handMenu_workflow").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // ObjectPlacement HandMenu Container
                serializedObject.FindProperty("handMenu_objectPlacement").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("ObjectPlacement HandMenu", "Please add the Reference to the ObjectPlacement HandMenu of the TaskPlanningInterface."), serializedObject.FindProperty("handMenu_objectPlacement").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Dialog Menus Container
                serializedObject.FindProperty("dialogMenuContainer").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Dialog Menu Container", "Please add the Reference to the Container, where the Dialog Menus will be instantiated in."), serializedObject.FindProperty("dialogMenuContainer").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Snippet Function Container
                serializedObject.FindProperty("snippetFunctionContainer").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Snippet Function Container", "Please add the Reference to the Container, where the GameObjects with Snippet Function Scripts will be instantiated in."), serializedObject.FindProperty("snippetFunctionContainer").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Contraint Function Container
                serializedObject.FindProperty("constraintFunctionContainer").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Contraint Function Container", "Please add the Reference to the Container, where the GameObjects with Contraint Function Scripts will be instantiated in."), serializedObject.FindProperty("constraintFunctionContainer").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Robot URDF
                serializedObject.FindProperty("robotURDF").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Robot URDF", "Please add the Reference to the Robot URDF Model, which contains the 'Urdf Robot' and 'TPI_RobotController' components."), serializedObject.FindProperty("robotURDF").objectReferenceValue, typeof(GameObject), true);
                EditorGUILayout.Space();

                // Remove indent
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(15);

            // Info Box
            EditorGUILayout.HelpBox("Please make sure to add all the prefabs, as the 'Task Planning Interface' does not work without them.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();

        }

    }

}