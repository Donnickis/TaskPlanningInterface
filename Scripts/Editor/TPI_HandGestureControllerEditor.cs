using TaskPlanningInterface.Controller;
using UnityEditor;
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
    [CustomEditor(typeof(TPI_HandGestureController))]
    public class TPI_HandGestureControllerEditor : Editor {

        bool foldoutValue_events;

        private void OnEnable() {
            foldoutValue_events = false;
        }

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/HandGestureController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("Hand Gesture Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The HandGestureController allows you to easily assign one or multiple functions to a Hand Gesture, which will be invoked once the gesture is recognized.\n" +
                "If needed, you can add your own Hand Gestures that should get recognized to this script by extending the given functionalities.\n" +
                "Furthermore, if nothing should happen for a specific Hand Gesture that has been provided to you with the initial version of this script, please feel free to assign no UnityEvents to that specific Gesture.", MessageType.Info);
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

            // enable hand gestures bool
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableHandGestures"));
            EditorGUILayout.Space();

            if (serializedObject.FindProperty("enableHandGestures").boolValue) {
                // Indent content
                EditorGUI.indentLevel++;

                // selected hand
                EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedHand"));
                EditorGUILayout.Space();

                // gesture check frequency float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gestureCheckFrequency"));
                EditorGUILayout.Space();

                // curled threshold float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("curled_threshold"), new GUIContent("Curled Threshold", "Determine the curled threshold, i.e. after which the finger counts as a curled finger."));
                EditorGUILayout.Space();

                // straight finger threshold float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("straight_threshold_finger"), new GUIContent("Straight Finger Threshold", "Determine the straight threshold of a finger, i.e. before which the finger counts as a straight finger."));
                EditorGUILayout.Space();

                // straight thumb threshold float
                EditorGUILayout.PropertyField(serializedObject.FindProperty("straight_threshold_thumb"), new GUIContent("Straight Thumb Threshold", "Determine the straight threshold of a thumb, i.e. before which the thumb counts as a straight finger."));
                EditorGUILayout.Space(20);

                // recognized gesture readonly field
                GUI.enabled = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gesture"), new GUIContent("Recognized Hand Gesture", "This field tells you what Hand Gesture is currently getting recognized by the TPI_HandGestureController."));
                GUI.enabled = true;
                EditorGUILayout.Space(15);


                // Experimental Text
                GUIStyle titleStyle = new GUIStyle();
                titleStyle.fontSize = 15;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = Color.white;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent("Experimental Features", "Here, you can find features that are experimental. Therefore, they can contains bugs or not work correctly."), titleStyle);
                EditorGUILayout.Space();

                // selected hand
                EditorGUILayout.PropertyField(serializedObject.FindProperty("visualizeRecognizedGesture"));
                EditorGUILayout.Space();

                if (!serializedObject.FindProperty("visualizeRecognizedGesture").boolValue) {
                    GUI.enabled = false;
                }
                // Indent content
                EditorGUI.indentLevel++;

                // gesture visualization prefab
                serializedObject.FindProperty("gestureVisualizationPrefab").objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Gesture Visualization Prefab:", "Please add the Prefab that should be visible if a hand gesture is recognized (only important if visualizeRecognizedGesture was set to true)."), serializedObject.FindProperty("gestureVisualizationPrefab").objectReferenceValue, typeof(GameObject), false);
                EditorGUILayout.Space();

                // Remove content indent
                EditorGUI.indentLevel--;

                if (!serializedObject.FindProperty("visualizeRecognizedGesture").boolValue) {
                    GUI.enabled = true;
                }

                // gesture verification amount int
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gestureVerificationAmount"));
                EditorGUILayout.Space();

                // Remove content indent
                EditorGUI.indentLevel--;

                // Separating Line
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space();

                // Hand Gesture Events Text
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent("Hand Gesture Events", "Here, you can add the Events that should be invoked when a Hand Gesture is recognized."), menuStyle);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(20);

                // References Folout
                foldoutValue_events = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutValue_events, new GUIContent("Hand Gesture Events Container Foldout", "Here, you can add the Events that should be invoked when a Hand Gesture is recognized."));
                EditorGUILayout.Space();

                if (foldoutValue_events) {
                    // Indent foldout content
                    EditorGUI.indentLevel++;

                    // thumbs up event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("thumbsUpEvent"));
                    EditorGUILayout.Space();

                    // thumbs sideways event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("thumbsSidewaysEvent"));
                    EditorGUILayout.Space();

                    // thumbs down event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("thumbsDownEvent"));
                    EditorGUILayout.Space();

                    // index up event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("indexUpEvent"));
                    EditorGUILayout.Space();

                    // index other event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("indexOtherEvent"));
                    EditorGUILayout.Space();

                    // fist event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fistEvent"));
                    EditorGUILayout.Space();

                    // two fingers event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("twoFingersEvent"));
                    EditorGUILayout.Space();

                    // five fingers event
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fiveFingersEvent"));
                    EditorGUILayout.Space();

                    // Remove indent
                    EditorGUI.indentLevel--;
                }

            }

            serializedObject.ApplyModifiedProperties();

        }

    }

}