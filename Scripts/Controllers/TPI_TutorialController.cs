using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TaskPlanningInterface.EditorAndInspector;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// The TutorialController handles the creation and progression of the Tutorial.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
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
    /// 
    [RequireComponent(typeof(TPI_MainController))]
    public class TPI_TutorialController : MonoBehaviour {

        [Tooltip("Decide whether you want to disable the tutorial feature in its entirety.")]
        [SerializeField] private bool enableTutorialFeature = false;

        // Tutorial Options
        [Tooltip("This bool states whether the Tutorial is currently active. You cannot change the value manually. Therefore, this entry to the inspector only acts as a status indication for you.")][ReadOnly][Rename("Is the Tutorial active?")]
        [SerializeField] private bool isTutorialActive = false;

        [Tooltip("This list contains the references to each of the specific tutorial steps.")]
        [SerializeField] private List<TutorialDialog> _tutorialDialogs;

        private int currentTutorialIndex;
        private GameObject spawnedTutorialDialog;

        [Tooltip("Add the icon that will be shown on the Tutorial Button in the Workflow HandMenu to start the tutorial.")]
        [SerializeField] private Texture2D activateTutorialIcon;
        [Tooltip("Add the icon that will be shown on the Tutorial Button in the Workflow HandMenu to stop the tutorial.")]
        [SerializeField] private Texture2D deactivateTutorialIcon;

        // Reference to TPI_MainController Component
        private TPI_MainController mainController;



        //---------------------------------------------------- Tutorial ----------------------------------------------------//
        /*


        The ’TPI_TutorialController’ script helps you to easily create a functioning tutorial for the operator, which can consist of multiple steps in a sequence.
        The operator can start said tutorial by pressing the "Start Tutorial" button in the ’Workflow HandMenu’.



        A)  How it works:
    -------------------------


            1) The easiest way to create the tutorial is by filling in the ’Tutorial Steps’ List in the Inspector of the ’TPI_TutorialController’ component, located on the ’TPI_Manager’ GameObject.
               Otherwise, you can also add additional tutorial dialogs at runtime (see subsection B: ’Creation of new Tutorial Steps at Runtime’ for a detailed view).


            2) Once the tutorial has started, it will automatically instantiate the prefab, which belongs to the first tutorial step, and invoke all the function(s) that were added to the ’Tutorial Start Event’ field.

            
            3) Then, it waits for the ’TriggerNextTutorialStep(string tutorialID)’ function with the correct tutorialID of the first step to be invoked somewhere in your code.

            
            4) Once this happens, it destroys the previous tutorial prefab, invokes the function(s) that were added to the ’Tutorial End Event’ field and starts the next tutorial step, looping through this process
               until the end of the list of tutorial steps is reached.

            
            To sum up:
               Please add the ’TriggerNextTutorialStep(string tutorialID)’ function at the correct spot in your code, where you want the ’TPI_TutorialController’ to proceed to the next tutorial step.
               You can get the tutorialID by either copying it (right click + Copy) from the Unity inspector or by extracting it from your tutorial dialog that was created at runtime
               (see subsection B: ’Creation of new Tutorial Steps at Runtime’).



        B)  Creation of new Tutorial Steps at Runtime:
    --------------------------------------------------------

        
            New Tutorial Dialogs can be easily created by using the following code:
            
            ////////////////////////////////////////////// CODE STARTS HERE //////////////////////////////////////////////

            // Assign the Prefab in the Inspector or by other means
            GameObject tutorialPrefab;

            // Create new TutorialDialog instance
            TPI_TutorialController.TutorialDialog myTutorialStep = new TutorialDialog ("Insert Tutorial Title", "Insert Tutorial Text", tutorialPrefab, new UnityEvent());

            // Configure the Action that should be invoked once the Tutorial Step is started
            myTutorialStep.tutorialStartEvent.AddListener( AddYourFunctionNames );

            // Configure the Action that should be invoked once the Tutorial Step has ended ( immediately before the next one starts )
            myTutorialStep.tutorialEndEvent.AddListener( AddYourFunctionNames );

            // Extract the ID of the Tutorial Step
            string tutorialID = myTutorialStep.tutorialID;

            TPI_TutorialController tutorialController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_TutorialController>().AddTutorialStep (myTutorialStep);
            
            ////////////////////////////////////////////// CODE ENDS HERE //////////////////////////////////////////////



        C)  Creation of Tutorial Prefabs:
    ----------------------------------------

            
            Finally, we discuss how you can set up a tutorial prefab (the GameObject that is instantiated). You can either start from scratch or copy and then adapt an already existing one provided
            to you by the TPI. Keep in mind, it is always good to name your tutorial prefabs in a way that makes it instantaneously clear what that template offers or where it is used.

            As the Tutorial Prefabs do not differ that much from the ’Dialog Menu’ system implemented in the ’TPI_DialogMenuController’, it is advised to use the ’Dialog Menu Building Blocks’
            for this process, them being located in the ’Prefabs’ folder of the ’TaskPlanningInterface’ directory.

            NAME: To specify the place where the name of the tutorial should be located, move a text field template to your desired position and set the displayed text to ’[Name]’ without the apostrophes.

            TEXT FIELD: To specify the place where the text field should be located, move a text field prefab to your desired position and set the displayed text to ’[Textfield]’ without the apostrophes.
    */


        private void Start() {
            if (_tutorialDialogs == null)
                _tutorialDialogs = new List<TutorialDialog>();
            mainController = GetComponent<TPI_MainController>();
            currentTutorialIndex = 0;
        }

        /// <summary>
        /// Resets all the variables of the Tutorial to their initial states.
        /// </summary>
        public void ResetTutorial() {
            isTutorialActive = false;
            currentTutorialIndex = 0;
            if(spawnedTutorialDialog != null)
                Destroy(spawnedTutorialDialog);
        }

        /// <summary>
        /// Returns whether the tutorial is active.
        /// </summary>
        public bool IsTutorialActive() {
            return isTutorialActive;
        }

        /// <summary>
        /// Toggles the Tutorial. If the Tutorial should be deactivated, it resets it completely.
        /// </summary>
        public void ToggleTutorial() {

            if (!enableTutorialFeature) {
                mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "The Tutorial Feature has been disabled.", "Confirm", mainController.abortButtonIcon);
                return;
            }

            if (isTutorialActive) { // Deactivate Tutorial
                Transform tutorialButton = mainController.handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(4); // TPI Tutorial Button in WorkflowHandMenu
                tutorialButton.GetComponent<ButtonConfigHelper>().MainLabelText = "Start\nTutorial";
                tutorialButton.GetComponent<ButtonConfigHelper>().SetQuadIcon(activateTutorialIcon);
                ResetTutorial();
            } else { // Activate Tutorial

                if (_tutorialDialogs.Count == 0) {
                    mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "No Tutorial Dialogs were setup either in the inspector or during runtime! Therefore, the Tutorial cannot be started.", "Confirm", mainController.abortButtonIcon);
                    return;
                }

                Transform tutorialButton = mainController.handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(4); // TPI Tutorial Button in WorkflowHandMenu
                tutorialButton.GetComponent<ButtonConfigHelper>().MainLabelText = "Stop\nTutorial";
                tutorialButton.GetComponent<ButtonConfigHelper>().SetQuadIcon(deactivateTutorialIcon);

                isTutorialActive = true;

                _tutorialDialogs[0].tutorialStartEvent?.Invoke();
                SpawnTutorialDialog();

            }

        }

        /// <summary>
        /// Triggers the next step of the tutorial if the conditions are met.
        /// <br></br>The conditions include the check of whether the given tutorialID of the trigger belongs to the currently active tutorial.
        /// <br></br>If it does, the function continues. Otherwise, false is returned.
        /// <para><paramref name="tutorialID"/>= ID of the Tutorial Dialog that this Trigger belongs to; the next step will only trigger if the ID matches with the ID of the current Tutorial Step</para>
        /// </summary>
        /// <returns>bool which states whether the next tutorial step has been triggered: bool = false -> conditions were not met</returns>
        public bool TriggerNextTutorialStep(string tutorialID) {
            if (!enableTutorialFeature)
                return false;

            if (!isTutorialActive)
                return false;

            if (!IsSpecificTutorialActive(tutorialID))
                return false;

            // Deactivate current Tutorial Dialog
            if (spawnedTutorialDialog != null) {
                _tutorialDialogs[currentTutorialIndex].tutorialEndEvent?.Invoke();
                Destroy(spawnedTutorialDialog);
                spawnedTutorialDialog = null; // Prevent Problems
            }

            // Activate next Tutorial Dialog
            currentTutorialIndex++;

            if (currentTutorialIndex > _tutorialDialogs.Count - 1) {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu_TwoButtons("TPI Tutorial", "Hooray! You have successfully completed the tutorial!\nDo you want to reset the Task Planning Interface in order to start fresh?", "Reset TPI", null, mainController.ResetTPI, "Decline", mainController.abortButtonIcon, null);
                // Deactivate Tutorial as all steps were completed
                Transform tutorialButton = mainController.handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(4); // TPI Tutorial Button in WorkflowHandMenu
                tutorialButton.GetComponent<ButtonConfigHelper>().MainLabelText = "Start\nTutorial";
                tutorialButton.GetComponent<ButtonConfigHelper>().SetQuadIcon(activateTutorialIcon);
                ResetTutorial();
                return true;
            }

            _tutorialDialogs[currentTutorialIndex].tutorialStartEvent?.Invoke();
            SpawnTutorialDialog();

            return true;
        }

        /// <summary>
        /// Toggles the Tutorial. If the Tutorial should be deactivated, it resets it completely.
        /// </summary>
        private void SpawnTutorialDialog() {

            spawnedTutorialDialog = Instantiate(_tutorialDialogs[currentTutorialIndex].tutorialPrefab);
            spawnedTutorialDialog.name = "TPI Tutorial Step: " + _tutorialDialogs[currentTutorialIndex].tutorialTitle + " with ID: " + _tutorialDialogs[currentTutorialIndex].tutorialID;

            TPI_ObjectPlacementController.PositionAndRotation pose = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().FindAndReservePosition(spawnedTutorialDialog, TPI_ObjectPlacementController.StartingPosition.MiddleCenter, TPI_ObjectPlacementController.SearchAlgorithm.closestPosition, TPI_ObjectPlacementController.SearchDirection.bothWays);
            spawnedTutorialDialog.transform.position = pose.position;
            spawnedTutorialDialog.transform.rotation = pose.rotation;

            // Configure Tutorial Title, Key: "[Name]"
            // Configure Tutorial Text, Key: "[Textfield]"
            TextMeshPro[] texts = spawnedTutorialDialog.GetComponentsInChildren<TextMeshPro>();
            for (int i = 0; i < texts.Length; i++) {
                if (texts[i] == null)
                    continue;
                if (texts[i].text.Contains("[Name]", System.StringComparison.OrdinalIgnoreCase)) {
                    texts[i].text = _tutorialDialogs[currentTutorialIndex].tutorialTitle;
                    texts[i] = null;
                    continue;
                }
                if (texts[i].text.Contains("[Textfield]", System.StringComparison.OrdinalIgnoreCase)) {
                    texts[i].text = _tutorialDialogs[currentTutorialIndex].tutorialText;
                    texts[i] = null;
                    continue;
                }
            }

        }

        /// <summary>
        /// Returns whether the tutorial with the given tutorialID is currently active.
        /// <para><paramref name="tutorialID"/>= ID of the tutorial dialog that you want to check</para>
        /// </summary>
        public bool IsSpecificTutorialActive(string tutorialID) {
            if (_tutorialDialogs[currentTutorialIndex].tutorialID == tutorialID)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns how many tutorial dialogs are left to be executed (not including the current one).
        /// </summary>
        public int GetRemainingTutorialSteps() {
            return _tutorialDialogs.Count - currentTutorialIndex - 1;
        }

        /// <summary>
        /// Returns how many tutorial dialogs there are in total.
        /// </summary>
        public int GetTotalTutorialSteps() {
            return _tutorialDialogs.Count;
        }

        /// <summary>
        /// Returns the list of all tutorial dialogs.
        /// </summary>
        public List<TutorialDialog> GetAllTutorialDialogs() {
            return _tutorialDialogs;
        }

        /// <summary>
        /// Adds a tutorial dialog to the end of the list.
        /// <para><paramref name="tutorialDialog"/>= TutorialDialog that should be added</para>
        /// </summary>
        public void AddTutorialStep(TutorialDialog tutorialDialog) {
            _tutorialDialogs.Add(tutorialDialog);
        }

        /// <summary>
        /// Inserts a tutorial dialog at a specific position in the list.
        /// <para><paramref name="tutorialDialog"/>= TutorialDialog that should be added</para>
        /// <para><paramref name="position"/>= position at which you want to insert the TutorialDialog</para>
        /// </summary>
        public void InsertTutorialStep(TutorialDialog tutorialDialog, int position) {
            if (position < 0 || position > _tutorialDialogs.Count - 1) {
                Debug.LogWarning("The desired position is out of bounds! (RemoveAtTutorialStep in TPI_TutorialController)");
            }
            _tutorialDialogs.Insert(position, tutorialDialog);
        }

        /// <summary>
        /// Removes a specific tutorial dialog from the list.
        /// <para><paramref name="tutorialDialog"/>= TutorialDialog that should be removed</para>
        /// </summary>
        public void RemoveTutorialStep(TutorialDialog tutorialDialog) {
            _tutorialDialogs.Remove(tutorialDialog);
        }

        /// <summary>
        /// Removes the tutorial dialog found at the specified position from the list.
        /// <para><paramref name="position"/>= position at which you want to remove an entry</para>
        /// </summary>
        public void RemoveAtTutorialStep(int position) {
            if (position < 0 || position > _tutorialDialogs.Count - 1) {
                Debug.LogWarning("The desired position is out of bounds! (RemoveAtTutorialStep in TPI_TutorialController)");
            }
            _tutorialDialogs.RemoveAt(position);
        }

        /// <summary>
        /// Removes all entries of the tutorial dialog list
        /// </summary>
        public void ClearTutorialDialogList() {
            _tutorialDialogs.Clear();
        }

        /// <summary>
        /// Returns the tutorial dialog with the specified ID from the list if it exists
        /// <para><paramref name="tutorialID"/>= ID of the tutorial dialog that you want to get the TutorialDialog from</para>
        /// </summary>
        public TutorialDialog GetTutorialDialogFromList(string tutorialID) {
            TutorialDialog tutorialDialog = null;
            foreach (TutorialDialog td in _tutorialDialogs) {
                if (td.tutorialID == tutorialID) {
                    tutorialDialog = td;
                    break;
                }
            }
            if (tutorialDialog == null) {
                Debug.LogWarning("The desired Tutorial Dialog with the ID: " + tutorialID + " was not found. Returning null... (GetTutorialDialogFromList in TPI_TutorialController)");
            }
            return tutorialDialog;
        }

        /// <summary>
        /// <para>
        /// The aim of this script is to store the information of a Tutorial Dialog and thereby allowing it to be saved and passed between different C# scripts.
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
        ///</para>
        /// </summary>
        [System.Serializable]
        public class TutorialDialog {

            [Tooltip("Title of the tutorial Dialog Menu")]
            public string tutorialTitle;
            [Tooltip("Text in the tutorial Dialog Menu")]
            public string tutorialText;
            [Tooltip("Prefab that will be instantiated and populated with the title and text")]
            public GameObject tutorialPrefab;
            [Tooltip("Function(s) that should be invoked once this tutorial step starts")]
            public UnityEvent tutorialStartEvent;
            [Tooltip("Function(s) that should be invoked once this tutorial step ends")]
            public UnityEvent tutorialEndEvent;
            [ReadOnly][Rename("Tutorial ID")][Tooltip("ID of this specific Tutorial Dialog -> copy to use it in the TriggerNextTutorialStep function")]
            public string tutorialID;

            public TutorialDialog() {
                this.tutorialTitle = "";
                this.tutorialText = "";
                this.tutorialPrefab = null;
                this.tutorialStartEvent = null;
                this.tutorialEndEvent = null;
                System.Guid guid = System.Guid.NewGuid();
                this.tutorialID = guid.ToString();
            }

            public TutorialDialog(string _tutorialTitle, string _tutorialText, GameObject _tutorialPrefab, UnityEvent _tutorialStartEvent, UnityEvent _tutorialEndEvent) {
                this.tutorialTitle = _tutorialTitle;
                this.tutorialText = _tutorialText;
                this.tutorialPrefab = _tutorialPrefab;
                this.tutorialStartEvent = _tutorialStartEvent;
                this.tutorialEndEvent = _tutorialEndEvent;
                System.Guid guid = System.Guid.NewGuid();
                this.tutorialID = guid.ToString();
            }

        }

    }

}
