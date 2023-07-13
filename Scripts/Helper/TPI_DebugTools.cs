using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using TaskPlanningInterface.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaskPlanningInterface.Helper {

    /// <summary>
    /// <para>
    /// The aim of this script is to handle the TPI Debug Tools, by automatically disabling them on the HoloLens and by providing the underlying functions.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Helper" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public class TPI_DebugTools : MonoBehaviour {

        [Tooltip("Decide whether the Debug Tools should be shown on the deployed program. They will not be turned off in the Unity Editor.")]
        public bool showDebugTools = false;

        [Tooltip("Add the GameObject that displays the name of the current hand gesture.")]
        public GameObject handGestureDebugGameObject;

        private void Start() {

#if UNITY_EDITOR
            // if the program is run in the Unity Editor
            showDebugTools = true;
            
#endif

            // Enable or Disable Debug Tools
            if(!showDebugTools) {
                for(int i = 0; i < transform.childCount; i++) {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }

        }


        /// <summary>
        /// Reloads the current scene, therefore resets the entire TPI.
        /// </summary>
        public void ReloadScene() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Loads a specific scene.
        /// <para><paramref name="sceneName"/> = Name of the scene that should be loaded</para>
        /// </summary>
        public void LoadScene(string sceneName) {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Toggles the visibility of the MRTK Hand Mesh.
        /// </summary>
        public void ToggleHandMesh() {
            MixedRealityInputSystemProfile inputSystemProfile = CoreServices.InputSystem?.InputSystemProfile;
            if (inputSystemProfile == null) {
                return;
            }

            MixedRealityHandTrackingProfile handTrackingProfile = inputSystemProfile.HandTrackingProfile;
            if (handTrackingProfile != null) {
                handTrackingProfile.EnableHandMeshVisualization = !handTrackingProfile.EnableHandMeshVisualization;
            }
        }

        /// <summary>
        /// Toggles the visibility of the MRTK Hand Joints.
        /// </summary>
        public void ToggleHandJoints() {
            MixedRealityHandTrackingProfile handTrackingProfile = null;

            if (CoreServices.InputSystem?.InputSystemProfile != null) {
                handTrackingProfile = CoreServices.InputSystem.InputSystemProfile.HandTrackingProfile;
            }

            if (handTrackingProfile != null) {
                handTrackingProfile.EnableHandJointVisualization = !handTrackingProfile.EnableHandJointVisualization;
            }
        }

        /// <summary>
        /// Can be used to log a message to the console.
        /// <para><paramref name="message"/> = Message that should be shown</para>
        /// </summary>
        public void LogMessageConsole(string message) {
            Debug.Log(message);
        }

        /// <summary>
        /// Can be used to open an error dialog menu showing a specific message.
        /// <para><paramref name="message"/> = Message that should be shown</para>
        /// </summary>
        public void LogMessage(string message) {
            GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Debug Message", message, "Confirm", null, null);
        }

        /// <summary>
        /// Can be used to log a warning to the console.
        /// <para><paramref name="warning"/> = Warning that should be shown</para>
        /// </summary>
        public void LogWarningConsole(string warning) {
            Debug.LogWarning(warning);
        }

        /// <summary>
        /// Can be used to open an error dialog menu showing a specific warning.
        /// <para><paramref name="warning"/> = Warning that should be shown</para>
        /// </summary>
        public void LogWarning(string warning) {
            GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Debug Warning", warning, "Confirm", null, null);
        }

        /// <summary>
        /// Can be used to log a error to the console.
        /// <para><paramref name="error"/> = Error that should be shown</para>
        /// </summary>
        public void LogErrorConsole(string error) {
            Debug.Log(error);
        }

        /// <summary>
        /// Can be used to open an error dialog menu showing a specific error.
        /// <para><paramref name="error"/> = Error that should be shown</para>
        /// </summary>
        public void LogError(string error) {
            GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Debug Error", error, "Confirm", null, null);
        }

        /// <summary>
        /// Can be used to add a new error status entry to the 'ROS Status Menu'.
        /// </summary>
        public void AddRosStatusMsg() {
            RosMessageTypes.Diagnostic.DiagnosticStatusMsg statusMsg = new RosMessageTypes.Diagnostic.DiagnosticStatusMsg(0, "Error", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec eget iaculis neque. Sed nulla ligula, elementum vel erat et, tristique tempor sapien. Mauris tincidunt pellentesque gravida. Vestibulum odio arcu, aliquam non sapien at, rhoncus vulputate justo. Proin venenatis luctus eros, convallis consequat odio volutpat sit amet. Proin pellentesque, sem at pulvinar euismod, sem augue consectetur nibh, et efficitur arcu leo quis leo. In et lacus tristique, laoreet nulla eu, blandit mi. Quisque ullamcorper fermentum faucibus. Vestibulum ex nisi, hendrerit sed tortor quis, dictum pharetra tellus. Vivamus viverra vestibulum felis, id molestie purus efficitur vitae. Curabitur hendrerit interdum tristique.", "", null);
            GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().rosController.GetComponent<TPI_ROSController>().AddStatusEntry(statusMsg);
        }

        /// <summary>
        /// Can be used to display the name of the current hand gesture.
        /// </summary>
        public void HandGestureDebug(string currentHandGestureName) {
            handGestureDebugGameObject.GetComponentInChildren<TextMeshPro>().text = "Current Hand Gesture: " + currentHandGestureName;
        }

        /// <summary>
        /// Can be used to enable and disable the digital twin
        /// </summary>
        public void ToggleDigitalTwinVisiblity() {
            GameObject digitalTwin = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().robotURDF.transform.GetChild(1).gameObject;
            if(digitalTwin.activeSelf)
                digitalTwin.SetActive(false);
            else
                digitalTwin.SetActive(true);
        }

    }

}
