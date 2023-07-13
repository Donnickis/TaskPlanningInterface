using TaskPlanningInterface.Controller;
using Unity.Robotics.ROSTCPConnector;
using UnityEditor;
using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// <para>
    /// This editor script changes what the Unity Inspector displays for the TPI_ROSController class.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.EditorAndInspector" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// IMPORTANT: if you make any changes to the variables that should be visible in the Unity Inspector of the underyling class, i.e. the TPI_ROSController in this instance, you also have to edit this script to reflect said changes.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [CustomEditor(typeof(TPI_ROSController))]
    public class TPI_ROSControllerEditor : Editor {

        public override void OnInspectorGUI() {

            //base.OnInspectorGUI();
            serializedObject.Update();



            //---------------------------------------------------- Header ----------------------------------------------------//



            // Banner Image
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            Texture2D TPI_Banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/TaskPlanningInterface/Textures/ROSController_header.png", typeof(Texture2D));
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
            EditorGUILayout.LabelField("ROS Controller", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(30);

            // Info Box
            EditorGUILayout.HelpBox("The ROSController helps with the establishment of a connection to ROS and with the ROS Message System.\n" +
                "Furthermore, it controls the ROS Status Menu, which can highlight information, warnings and errors concerning ROS.\n" +
                "Finally, the ROSController provides documentation for all public functions of the ROSConnection script of the 'Unity Robotics Hub' along with some examples for the major functions.", MessageType.Info);
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

            if (Application.isPlaying)
                GUI.enabled = false;
            // enable hand gestures bool
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rosConnectionType"));
            EditorGUILayout.Space(15);
            if (Application.isPlaying)
                GUI.enabled = true;

            if (serializedObject.FindProperty("rosConnectionType").enumValueIndex == 0)
                ((TPI_ROSController)target).gameObject.GetComponent<ROSConnection>().enabled = false;
            else
                ((TPI_ROSController)target).gameObject.GetComponent<ROSConnection>().enabled = true;

            if (serializedObject.FindProperty("rosConnectionType").enumValueIndex == 0) {
                // Info Box
                EditorGUILayout.HelpBox("Activate the ROS Connection in the 'ROS Connection Type' field in order to edit these settings.", MessageType.Warning);
                EditorGUILayout.Space(15);
                GUI.enabled = false;
            }

            // Indent content
            EditorGUI.indentLevel++;

            // address request prefab GameObject
            EditorGUILayout.PropertyField(serializedObject.FindProperty("addressRequestPrefab"));
            EditorGUILayout.Space();

            // Separating Line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // ROS Status Menu Text
            EditorGUILayout.LabelField(new GUIContent("ROS Status Menu", "Options relating to the ROS Status Menu."), menuStyle);
            EditorGUILayout.Space(20);

            // status entry GameObject
            EditorGUILayout.PropertyField(serializedObject.FindProperty("statusEntry"));
            EditorGUILayout.Space();

            // info Texture2D
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infoTexture"));
            EditorGUILayout.Space();

            // warning Texture2D
            EditorGUILayout.PropertyField(serializedObject.FindProperty("warningTexture"));
            EditorGUILayout.Space();

            // error Texture2D
            EditorGUILayout.PropertyField(serializedObject.FindProperty("errorTexture"));
            EditorGUILayout.Space();

            // Remove content indent
            EditorGUI.indentLevel--;

            if (serializedObject.FindProperty("rosConnectionType").enumValueIndex == 0) {
                GUI.enabled = true;
            }

            // Info Box
            EditorGUILayout.HelpBox("Please change the Ros IP Address and ROS Port in the 'ROS Connection' component below.\n" +
                "Furthermore, you can also change the 'Keepalive Time', the 'Network Timeout Seconds' and the 'Sleep Time Seconds' values in the 'ROS Connection' component below.\n\n" +
                "IMPORTANT: Please do not enable 'Connect on Start' as this is automatically done in the 'TPI_ROSController' script.", MessageType.Warning);
            EditorGUILayout.Space(15);
            serializedObject.ApplyModifiedProperties();

        }

    }

}