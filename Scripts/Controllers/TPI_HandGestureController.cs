using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// The aim of this script is to recognized the hand gestures performed by the operator.
    /// <br></br>After successful recognition, designated functions are called by the TPI_HandGestureController.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// <br></br>However, if you want to add additional Hand Gestures that should be recognized, this can be done in this script.
    /// </para>
    /// 
    /// <para>
    /// Source:
    /// <br></br>Heavily adapted Version of the "HandPoseRecognition" script found in the Master Thesis "Human-Robot Object Handovers using Mixed Reality" by Manuel Koch at PDZ. -> Therefore, he will be also listed as author.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Manuel Koch (mankoch@student.ethz.ch)
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [RequireComponent(typeof(TPI_MainController))]
    public class TPI_HandGestureController : MonoBehaviour {

        [Tooltip("Determine whether Hand Gestures should be recognized and whether the underlying UnityEvents should thus be invoked.")]
        public bool enableHandGestures = true;

        [Tooltip("Determine whether a text should appear on the hand with the name of the gesture once a hand gesture is recognized.")]
        public bool visualizeRecognizedGesture = false;
        private bool showVisualizationPrefab = false;

        [Tooltip("Please add the Prefab that should be visible if a hand gesture is recognized (only important if visualizeRecognizedGesture was set to true).")]
        public GameObject gestureVisualizationPrefab;
        private GameObject spawnedVisualizatonObject;

        [Tooltip("Select on which Hand the Gestures should be detected.")]
        public SelectedHand selectedHand = SelectedHand.left;
        private Handedness selectedHandedness = Handedness.Both; // only for internal storage

        // Options for how often Finger Curl Check and Hand Gesture Check should be performed
        private float timeElapsed;
        [Tooltip("Select how many times per second the current Hand Gesture should be checked.")][Range(0.25f, 15f)]
        public float gestureCheckFrequency = 5f;
        [Tooltip("Select how many times a Hand Gesture must be recognized in a row before it is ultimately selected as active Hand Gesture, and thus before the underlying event is invoked. -> reduces chance of accidentally recognized gestures")][Range(1, 15)]
        public int gestureVerificationAmount = 3;

        // Options for the Curl Detection: 1 -> finger curled, 0 -> not curled
        [Tooltip("Determine the curled threshold, i.e. after which the finger counts as a curled finger.")][Range(0f, 1f)]
        public float curled_threshold = 0.6f;
        [Tooltip("Determine the straight threshold of a finger, i.e. before which the finger counts as a straight finger.")][Range(0f, 1f)]
        public float straight_threshold_finger = 0.1f;
        [Tooltip("Determine the straight threshold of a thumb, i.e. before which the thumb counts as a straight finger.")][Range(0f, 1f)]
        public float straight_threshold_thumb = 0.4f;

        // Detected Finger Curls (and thus the status)
        private float[] finger_curl = new float[5];
        private string[] finger_status = new string[5];

        // Needed for the gestureVerificationAmount -> saves previous gestures
        private System.Collections.Generic.List<Gestures> previousGestures;

        // Detected Gesture
        [EditorAndInspector.ReadOnly][Tooltip("This field tells you what Hand Gesture is currently getting recognized by the TPI_HandGestureController.")]
        public Gestures gesture;

        // Events for the Hand Gestures
        [Tooltip("Determine what happens if the Hand Gesture 'thumbs up' is recognized.")]
        public UnityEvent thumbsUpEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'thumbs sideways' is recognized.")]
        public UnityEvent thumbsSidewaysEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'thumbs down' is recognized.")]
        public UnityEvent thumbsDownEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'index up' is recognized.")]
        public UnityEvent indexUpEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'index other' is recognized.")]
        public UnityEvent indexOtherEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'fist' is recognized.")]
        public UnityEvent fistEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'two fingers' is recognized.")]
        public UnityEvent twoFingersEvent;
        [Tooltip("Determine what happens if the Hand Gesture 'five fingers' is recognized.")]
        public UnityEvent fiveFingersEvent;

        private void Start() {
            previousGestures = new System.Collections.Generic.List<Gestures>();
            if (visualizeRecognizedGesture && gestureVisualizationPrefab == null) {
                Debug.LogWarning("Please assign the Gesture Visualization Prefab in the TPI_HandGestureController component in " + transform.name + " in order to visualize the Hand Gesture.");
                visualizeRecognizedGesture = false;
            }

        }

        private void LateUpdate() {

            if(enableHandGestures) {

                timeElapsed += Time.deltaTime;

                // Check for Finger Curl and Hand Gestures every 1 / checkGestureFrequency seconds
                if (timeElapsed > 1 / gestureCheckFrequency) {

                    // Resizes the previous gestures list if needed
                    if(gestureVerificationAmount > 1) {
                        if (previousGestures.Count > gestureVerificationAmount) {
                            for (int i = 0; i < previousGestures.Count - gestureVerificationAmount; i++) {
                                previousGestures.RemoveAt(0);
                            }
                        } else if (previousGestures.Count < gestureVerificationAmount) {
                            for (int i = 0; i < gestureVerificationAmount - previousGestures.Count; i++) {
                                previousGestures.Add(Gestures.none);
                            }
                        }
                    }

                    CheckFingerCurl();
                    timeElapsed = 0;

                }

                // Shows the Visualization Object and tries to place it on the hand palm facing the operator.
                if (visualizeRecognizedGesture && showVisualizationPrefab) {
                    if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, selectedHandedness, out MixedRealityPose pose)) {
                        if(!spawnedVisualizatonObject.activeSelf)
                            spawnedVisualizatonObject.SetActive(true);
                        spawnedVisualizatonObject.transform.position = pose.Position;
                        spawnedVisualizatonObject.transform.LookAt(Camera.main.transform);
                        spawnedVisualizatonObject.transform.GetComponent<TextMeshPro>().text = GetCurrentGestureName();
                    } else {
                        if(spawnedVisualizatonObject.activeSelf)
                            spawnedVisualizatonObject.SetActive(false);
                    }
                }

            }
            
        }

        /// <summary>
        /// Returns the currently recognized Hand Gesture.
        /// </summary>
        public Gestures getCurrentGesture() {
            return gesture;
        }

        /// <summary>
        /// Returns the name of the currently recognized Hand Gesture.
        /// </summary>
        private string GetCurrentGestureName() {
            switch (gesture) {
                case Gestures.thumbsUp:
                    return "Thumbs Up";
                case Gestures.thumbsSideways:
                    return "Thumbs Sideways";
                case Gestures.thumbsDown:
                    return "Thumbs Down";
                case Gestures.indexUp:
                    return "Index Up";
                case Gestures.indexOther:
                    return "Index Other";
                case Gestures.fist:
                    return "Fist";
                case Gestures.two:
                    return "Two";
                case Gestures.five:
                    return "Five";
                default: // Gestures.none
                    return "None";
            }
        }

        /// <summary>
        /// Converts the selectedHand to the MRTK Handedness equivalent (the SelectedHand enum was created in order to make the choice a bit more organized)
        /// </summary>
        private Handedness ConvertSelectedHandToHandedness() {
            if(selectedHand == SelectedHand.left)
                return Handedness.Left;
            if (selectedHand == SelectedHand.right)
                return Handedness.Right;
            return Handedness.Both;

        }

        /// <summary>
        /// Assign a new UnityEvent that should be invoked once the specific gesture is triggered.
        /// <br></br>IMPORTANT: Overrides all current UnityEvents and UnityActions that have been either setup in the Inspector or via AssignUnityActionToGesture(Gestures gesture, UnityAction unityAction) for that gesture!
        /// <para><paramref name="gesture"/>= Gesture for which you want to assign the UnityEvent</para>
        /// <para><paramref name="unityEvent"/>= UnityEvent that you want to assign</para>
        /// </summary>
        public void AssignUnityEventToGesture(Gestures gesture, UnityEvent unityEvent) {
            if (unityEvent == null) {
                Debug.LogWarning("The UnityEvent that should be assigned cannot be null! (AssignUnityEventToGesture in TPI_HandGestureController)");
                return;
            }
            UnityEvent gestureEvent = unityEvent; // Fixes problem that can occur if a UnityEvent parameter is directly assigned
            switch (gesture) {
                case Gestures.thumbsUp:
                    thumbsUpEvent ??= new UnityEvent();
                    thumbsUpEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.thumbsSideways:
                    thumbsSidewaysEvent ??= new UnityEvent();
                    thumbsSidewaysEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.thumbsDown:
                    thumbsDownEvent ??= new UnityEvent();
                    thumbsDownEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.indexUp:
                    indexUpEvent ??= new UnityEvent();
                    indexUpEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.indexOther:
                    indexOtherEvent ??= new UnityEvent();
                    indexOtherEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.fist:
                    fistEvent ??= new UnityEvent();
                    fistEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.two:
                    twoFingersEvent ??= new UnityEvent();
                    twoFingersEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                case Gestures.five:
                    fiveFingersEvent ??= new UnityEvent();
                    fiveFingersEvent.AddListener(() => gestureEvent?.Invoke());
                    break;
                default: // Gestures.none -> do nothing
                    break;
            }
        }

        /// <summary>
        /// Assign a UnityAction that should be invoked once the specific gesture is triggered.
        /// <br></br>You can assign multiple UnityActions to a Gesture this way! (Big difference to AssignUnityEventToGesture(Gestures gesture, UnityEvent unityEvent))
        /// <para><paramref name="gesture"/>= Gesture for which you want to assign the UnityAction</para>
        /// <para><paramref name="unityAction"/>= UnityAction that you want to assign</para>
        /// </summary>
        public void AssignUnityActionToGesture(Gestures gesture, UnityAction unityAction) {
            if (unityAction == null) {
                Debug.LogWarning("The UnityAction that should be assigned cannot be null! (AssignUnityActionToGesture in TPI_HandGestureController)");
                return;
            }
            UnityAction gestureAction = unityAction; // Fixes problem that can occur if a UnityAction parameter is directly assigned
            switch (gesture) {
                case Gestures.thumbsUp:
                    thumbsUpEvent.AddListener(gestureAction);
                    break;
                case Gestures.thumbsSideways:
                    thumbsSidewaysEvent.AddListener(gestureAction);
                    break;
                case Gestures.thumbsDown:
                    thumbsDownEvent.AddListener(gestureAction);
                    break;
                case Gestures.indexUp:
                    indexUpEvent.AddListener(gestureAction);
                    break;
                case Gestures.indexOther:
                    indexOtherEvent.AddListener(gestureAction);
                    break;
                case Gestures.fist:
                    fistEvent.AddListener(gestureAction);
                    break;
                case Gestures.two:
                    twoFingersEvent.AddListener(gestureAction);
                    break;
                case Gestures.five:
                    fiveFingersEvent.AddListener(gestureAction);
                    break;
                default: // Gestures.none -> do nothing
                    break;
            }
        }

        /// <summary>
        /// Checks the current position of the fingers and determines whether they are curled or not.
        /// </summary>
        private void CheckFingerCurl() {

            /*
            Finger curl:
            [0] Thumb
            [1] Index
            [2] Middle
            [3] Ring
            [4] Pinky
            */

            selectedHandedness = ConvertSelectedHandToHandedness();

            finger_curl[0] = HandPoseUtils.ThumbFingerCurl(selectedHandedness);
            finger_curl[1] = HandPoseUtils.IndexFingerCurl(selectedHandedness);
            finger_curl[2] = HandPoseUtils.MiddleFingerCurl(selectedHandedness);
            finger_curl[3] = HandPoseUtils.RingFingerCurl(selectedHandedness);
            finger_curl[4] = HandPoseUtils.PinkyFingerCurl(selectedHandedness);

            float straight_threshold = straight_threshold_thumb;
            for (int i = 0; i < 5; i++) {

                if (finger_curl[i] < straight_threshold) {
                    finger_status[i] = "straight";
                } else if (finger_curl[i] > curled_threshold) {
                    finger_status[i] = "curled";
                } else {
                    finger_status[i] = "neither";
                }
                straight_threshold = straight_threshold_finger;
            }

            RecogniseGesture(finger_status);
        }


        /// <summary>
        /// Recognizes the currently performed Hand Gestures based on the Finger Curls and then invokes the corresponding UnityEvent.
        /// <para><paramref name="status"/>= List of the individual finger curl status</para>
        /// </summary>
        private void RecogniseGesture(string[] status) {

            if (status[0] == "straight" && status[1] == "curled" && status[2] == "curled" && status[3] == "curled" && status[4] == "curled") {
                //gesture = "one_thumb";
                float thumb_angle = ThumbAngleFromUp();
                if (thumb_angle < 45) {
                    CheckPreviousGestures(Gestures.thumbsUp, thumbsUpEvent);
                } else if (thumb_angle > 135) {
                    CheckPreviousGestures(Gestures.thumbsDown, thumbsDownEvent);
                } else {
                    CheckPreviousGestures(Gestures.thumbsSideways, thumbsSidewaysEvent);
                }
                return;
            }

            if (status[0] == "curled" && status[1] == "straight" && status[2] != "straight" && status[3] == "curled" && status[4] == "curled") {
                //gesture = "one_index";
                float index_angle = IndexAngleFromUp();
                if (index_angle < 45) {
                    CheckPreviousGestures(Gestures.indexUp, indexUpEvent);
                } else {
                    CheckPreviousGestures(Gestures.indexOther, indexOtherEvent);
                }
                return;
            }

            if (status[0] == "curled" && status[1] == "curled" && status[2] == "curled" && status[3] == "curled" && status[4] == "curled") {
                CheckPreviousGestures(Gestures.fist, fistEvent);
                return;
            }

            if (status[0] != "curled" && status[1] == "straight" && status[2] == "straight" && status[3] == "straight" && status[4] == "straight") {
                //gesture = "five";
                float index_angle = IndexAngleFromUp();
                if (index_angle < 30) {
                    CheckPreviousGestures(Gestures.five, fiveFingersEvent);
                }
                return;
            }

            if (status[0] == "straight" && status[1] == "straight" && status[2] == "curled" && status[3] == "curled" && status[4] == "curled") {
                CheckPreviousGestures(Gestures.two, twoFingersEvent);
                return;
            }

            //////////////////////////////////////////// This would be the place to add additional hand gesture recognitions. ////////////////////////////////////////////

            CheckPreviousGestures(Gestures.none, null);

        }

        /// <summary>
        /// Checks if enough Hand Gesture of the same type have been recognized in a row before it is ultimately selected as active Hand Gesture, and thus before the underlying event is invoked. -> reduces chance of accidentally recognized gestures
        /// <para><paramref name="_gesture"/>= Gesture for which you want to check whether it has been detected enough times</para>
        /// <para><paramref name="_unityEvent"/>= UnityEvent that you want to invoke</para>
        /// </summary>
        private void CheckPreviousGestures(Gestures _gesture, UnityEvent _unityEvent) {
            UnityEvent gestureEvent = _unityEvent; // Fixes problem that can occur if a UnityEvent parameter is directly assigned
            if (gestureVerificationAmount == 1) {
                gesture = _gesture;
                gestureEvent?.Invoke();
            } else {
                bool otherGestureFound = false;
                for (int i = 0; i < previousGestures.Count; i++) {
                    if (previousGestures[i] != _gesture) {
                        otherGestureFound = true;
                        break;
                    }
                }
                if (!otherGestureFound) {
                    gesture = _gesture;
                    gestureEvent?.Invoke();

                    // Visualize performed Hand Gesture (show text with name on the palm)
                    if (visualizeRecognizedGesture) {
                        if (spawnedVisualizatonObject == null)
                            spawnedVisualizatonObject = Instantiate(gestureVisualizationPrefab);
                        if (gesture == Gestures.none) {
                            showVisualizationPrefab = false;
                            spawnedVisualizatonObject.SetActive(false);
                        } else {
                            showVisualizationPrefab = true;
                        }
                    }

                } else {
                    previousGestures.RemoveAt(0);
                    previousGestures.Add(gesture);
                }
            }
        }

        /// <summary>
        /// Returns the angle between the up direction and the current angle of the index finger
        /// </summary>
        private float IndexAngleFromUp() {
            float angle = 360;
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out MixedRealityPose pose)) {
                Vector3 index_direction = pose.Forward;
                angle = Vector3.Angle(index_direction, Vector3.up);
            }
            return angle;
        }

        /// <summary>
        /// Returns the angle between the up direction and the current angle of the thumb
        /// </summary>
        private float ThumbAngleFromUp() {
            float angle = 360;
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out MixedRealityPose pose)) {
                Vector3 index_direction = pose.Forward;
                angle = Vector3.Angle(index_direction, Vector3.up);
            }
            return angle;
        }

        /// <summary>
        /// This enum is used to give the user a choice of which hand that should be recognized.
        /// </summary>
        public enum SelectedHand {
            [InspectorName("Both Hands")]
            both, // 0
            [InspectorName("Left Hand")]
            left, // 1
            [InspectorName("Right Hand")]
            right, // 2
        }

        /// <summary>
        /// This enum is used to distinguish between the different hand position that are detected.
        /// </summary>
        public enum Gestures {
            [InspectorName("No Gesture Recognized")]
            none, // 0
            [InspectorName("Thumbs Up")]
            thumbsUp, // 1
            [InspectorName("Thumbs Sideways")]
            thumbsSideways, // 2
            [InspectorName("Thumbs Down")]
            thumbsDown, // 3
            [InspectorName("Index Up")]
            indexUp, // 4
            [InspectorName("Index Other than Up")]
            indexOther, // 5
            [InspectorName("Fist (all fingers curled)")]
            fist, // 6
            [InspectorName("Two (Thumb + Index)")]
            two, // 7
            [InspectorName("Five (all fingers straight)")]
            five, // 8
        }

        #region handGesture_ReadyToUseFunctions

        /// <summary>
        /// Used to invoke the 'StartSequence()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void StartSequence_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().StartSequence(false);
        }

        /// <summary>
        /// Used to invoke the 'StopSequence()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void StopSequence_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().StopSequence(false);
        }

        /// <summary>
        /// Used to invoke the 'RestartSequence()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void RestartSequence_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().RestartSequence(false);
        }

        /// <summary>
        /// Used to invoke the 'TogglePauseSnippet()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void TogglePauseSnippet_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().TogglePauseSnippet(false);
        }

        /// <summary>
        /// Used to invoke the 'SkipSnippet()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void SkipSnippet_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().SkipSnippet(false);
        }

        /// <summary>
        /// Used to invoke the 'RepeatSnippet()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void RepeatSnippet_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().RepeatSnippet(false);
        }

        /// <summary>
        /// Used to invoke the 'ReturnToPreviousSnippet()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void ReturnToPreviousSnippet_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().ReturnToPreviousSnippet(false);
        }

        /// <summary>
        /// Used to invoke the 'EmergencyStopSnippet()' function of the TPI_SequenceMenuController class with a hand gesture.
        /// </summary>
        public void EmergencyStopSnippet_withHandGesture() {
            GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>().EmergencyStopSnippet(false);
        }

        #endregion handGesture_ReadyToUseFunctions

    }

}