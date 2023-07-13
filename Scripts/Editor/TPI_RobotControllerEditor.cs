using TaskPlanningInterface.Controller;
using UnityEditor;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_RobotController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_RobotController in this instance, you also have to edit this script to reflect said changes.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [CustomEditor(typeof(TPI_RobotController))]
    public class TPI_RobotControllerEditor : Editor {

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
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/RobotController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("Robot Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The RobotController allows you to tweak the settings of the virtual twin, by for example moving the base link or by changing the joint angles.\n" +
                "To accomplish this, you are given multiple settings that can be customized in order to first configure the TPI_RobotController to work with you robot. All the initial values are configured and set up in order to work properly with the 'Franka Emika Panda' Robotic Arm.\n" +
                "The base features of this script include the options to either publish the base link pose or to subscribe to it, the option to subscribe to robot joint angles and finally the option to send robot poses (either in the world coordinate system or in a local robot base link one).", MessageType.Info);
            EditorGUILayout.Space(15);



            //---------------------------------------------------- Robot Specific Options ----------------------------------------------------//



            // Robot Specific Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUIStyle menuStyle = new GUIStyle();
            menuStyle.fontSize = 22;
            menuStyle.fontStyle = FontStyle.Bold;
            menuStyle.normal.textColor = Color.white;
            menuStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(new GUIContent("Robot Specific Options", "Options relating to the virtual twin that you are currently using."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            // robot base GameObject field
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_robotBase"));
            EditorGUILayout.Space();

            // robot link names string[] list
            EditorGUILayout.PropertyField(serializedObject.FindProperty("robotLinkNames"));
            EditorGUILayout.Space();



            //---------------------------------------------------- ROS Related Options ----------------------------------------------------//




            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // General Options Text
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("ROS Related Options", "Options relating to ROS, i.e. changing the way how messages are published and how Unity subscribes to topics."), menuStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;

            if(GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated()) {
                // Info Box
                EditorGUILayout.HelpBox("Activate the ROS Connection in the 'ROS Connection Type' field in the TPI_ROSController Component of the ROSController GameObject in order to edit these settings.", MessageType.Warning);
                EditorGUILayout.Space(15);
                GUI.enabled = false;
            }

            // Robot Base Text
            EditorGUILayout.LabelField(new GUIContent("Robot Base Link Options", "Options relating to Robot Base Link."), titleStyle);
            EditorGUILayout.Space(15);

            // robot base topic
            EditorGUILayout.PropertyField(serializedObject.FindProperty("robotBaseTopic"));
            EditorGUILayout.Space();

            // receive robot base pose
            EditorGUILayout.PropertyField(serializedObject.FindProperty("receiveRobotBasePose"));
            EditorGUILayout.Space();

            // publish robot base pose
            EditorGUILayout.PropertyField(serializedObject.FindProperty("publishRobotBasePose"));
            EditorGUILayout.Space();

            if (serializedObject.FindProperty("publishRobotBasePose").enumValueIndex == 2) {
                // Indent content
                EditorGUI.indentLevel++;

                // robot base publish frequency
                EditorGUILayout.PropertyField(serializedObject.FindProperty("robotBase_publishFrequency"), new GUIContent("Frequency of Publishing (1/s)", "In the case of publishRobotBasePose being set to automatically publishing, set the frequency how many times each seconds the robot base pose should be published."));
                EditorGUILayout.Space();

                // Remove content indent
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(15);


            // Robot joint Text
            EditorGUILayout.LabelField(new GUIContent("Robot Joint Options", "Options relating to Robot Joints."), titleStyle);
            EditorGUILayout.Space(15);

            // robot joint topic
            EditorGUILayout.PropertyField(serializedObject.FindProperty("robotJointTopic"));
            EditorGUILayout.Space(15);


            // Robot Pose Text
            EditorGUILayout.LabelField(new GUIContent("Robot Pose and Reachability Options", "Options relating to Robot Poses and their Reachabilities."), titleStyle);
            EditorGUILayout.Space(15);

            // robot pose topic
            EditorGUILayout.PropertyField(serializedObject.FindProperty("robotPoseTopic"));
            EditorGUILayout.Space();

            // pose reachable topic
            EditorGUILayout.PropertyField(serializedObject.FindProperty("poseReachableTopic"));
            EditorGUILayout.Space();

            // pose reachable event
            EditorGUILayout.PropertyField(serializedObject.FindProperty("poseReachableEvent"));
            EditorGUILayout.Space();

            // pose not reachable event
            EditorGUILayout.PropertyField(serializedObject.FindProperty("poseNotReachableEvent"));
            EditorGUILayout.Space();

            if (GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated()) {
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();

        }

    }

}