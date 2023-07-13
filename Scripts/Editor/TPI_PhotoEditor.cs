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
    [CustomEditor(typeof(TPI_Photo))]
    public class TPI_PhotoEditor : Editor {

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
            EditorGUILayout.LabelField("TPI Photo", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The 'TPI_Photo' script is used to take a picture using the HoloLens Webcam and send it to ROS." +
                "An example use of this class is to calibrate the position of the robot's base using arUco markers.", MessageType.Info);
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
            EditorGUILayout.LabelField(new GUIContent("General Options", "Options relating to TPI_Photo in general."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // cam axes GameObject
            EditorGUILayout.PropertyField(serializedObject.FindProperty("camAxes"));
            EditorGUILayout.Space();

            // image topic string
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topic_Image"), new GUIContent("Image ROS Topic", "Topic name under which the HoloLens camera image (ImageMsg) should be published to ROS."));
            EditorGUILayout.Space();

            // camera transform topic string
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topic_CameraTransform"), new GUIContent("Camera Transform ROS Topic", "Topic name under which the camera matrix (TransformMsg) should be published to ROS."));
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

        }

    }

}