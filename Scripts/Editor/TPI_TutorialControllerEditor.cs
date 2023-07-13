using InspectorGadgets.Editor;
using TaskPlanningInterface.Controller;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_HandGestureController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_HandGestureController in this instance, you also have to edit this script to reflect said changes.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [CustomEditor(typeof(TPI_TutorialController))]
    public class TPI_TutorialControllerEditor : Editor {

        private ReorderableList tutorialList;

        private SerializedProperty tutorialProperty;

        private void OnEnable() {



            //---------------------------------------------------- Tutorial List ----------------------------------------------------//



            tutorialProperty = serializedObject.FindProperty("_tutorialDialogs");
            tutorialList = new ReorderableList(serializedObject, tutorialProperty, true, true, true, true);

            // List menu name
            tutorialList.drawHeaderCallback = (Rect rect) => {
                tutorialProperty.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10, rect.y, rect.width - 60, rect.height), tutorialProperty.isExpanded, new GUIContent(" Tutorial Steps", "This list contains the references to each of the specific tutorial steps."));
                if (tutorialProperty.arraySize == 0)
                    tutorialProperty.isExpanded = true;
                tutorialList.draggable = tutorialProperty.isExpanded;
                tutorialList.displayAdd = tutorialProperty.isExpanded;
                tutorialList.displayRemove = tutorialProperty.isExpanded;
                GUI.enabled = false;
                EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), tutorialProperty.arraySize);
                GUI.enabled = true;
            };

            // Display list elements
            tutorialList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (tutorialProperty.isExpanded) {
                    var element = tutorialList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, true);
                }
            };

            // Set height of the entries according to whether they are folded out or not
            tutorialList.elementHeightCallback = (index) => {
                if (!tutorialProperty.isExpanded)
                    return 0;
                var height = tutorialList.elementHeight;
                if (tutorialList.serializedProperty.GetArrayElementAtIndex(index).isExpanded) {
                    height += EditorGUI.GetPropertyHeight(tutorialList.serializedProperty.GetArrayElementAtIndex(index), true);
                }
                return height;
            };

            // What happens if a new element is added to the list?
            // Important as it blocks the list from cloning the last element when a new element is added
            tutorialList.onAddCallback = (ReorderableList l) => {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("tutorialTitle").stringValue = "";
                element.FindPropertyRelative("tutorialText").stringValue = "";
                element.FindPropertyRelative("tutorialPrefab").objectReferenceValue = null;
                element.FindPropertyRelative("tutorialStartEvent").SetUnderlyingValue(null);
                element.FindPropertyRelative("tutorialEndEvent").SetUnderlyingValue(null);
                System.Guid guid = System.Guid.NewGuid();
                element.FindPropertyRelative("tutorialID").stringValue = guid.ToString();
            };

        }

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/TutorialController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("Tutorial Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The TutorialController helps you to easily create a simple and functioning Tutorial to your liking.\n" +
                "The Tutorial consists of steps (TutorialDialog) that can be either configured in the Unity Inspector or at runtime via code.\n" +
                "Once the operator presses the 'Start Tutorial' Button in the Workflow HandMenu, the TutorialController starts the Tutorial.", MessageType.Info);
            EditorGUILayout.Space(15);



            //---------------------------------------------------- Tutorial Options ----------------------------------------------------//



            // General Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle menuStyle = new GUIStyle();
            menuStyle.fontSize = 22;
            menuStyle.fontStyle = FontStyle.Bold;
            menuStyle.normal.textColor = Color.white;
            menuStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(new GUIContent("Tutorial Options", "Options relating to the Tutorial. The Tutorial can be started by pressing the startTutorial Button in the HandMenu."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);


            if (Application.isPlaying)
                GUI.enabled = false;
            // isTutorialActiveProperty bool
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableTutorialFeature"));
            EditorGUILayout.Space();
            if (Application.isPlaying)
                GUI.enabled = true;

            if (serializedObject.FindProperty("enableTutorialFeature").boolValue) {
                // Indent content
                EditorGUI.indentLevel++;

                // isTutorialActiveProperty bool
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isTutorialActive"));
                EditorGUILayout.Space();

                // activate tutorial icon Texture2D
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activateTutorialIcon"));
                EditorGUILayout.Space();

                // deactivate tutorial icon Texture2D
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deactivateTutorialIcon"));
                EditorGUILayout.Space();

                // tutorial dialogs list
                tutorialList.DoLayoutList();
                EditorGUILayout.Space();

                // Remove content indent
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

        }

    }

}