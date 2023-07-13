using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TaskPlanningInterface.DialogMenu;
using TaskPlanningInterface.EditorAndInspector;
using Microsoft.MixedReality.Toolkit.UI;
using TaskPlanningInterface.Workflow;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// The MainController handles the TPI in general the and the workflow hand menu.
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

    [RequireComponent(typeof(TPI_DialogMenuController), typeof(TPI_SaveController), typeof(TPI_WorkflowConfigurationController))]
    public class TPI_MainController : MonoBehaviour {

        // General Options
        [Tooltip("This bool states whether the TPI has been opened. You cannot change the value manually. Therefore, this entry to the inspector only acts as a status indication for you.")][ReadOnly][Rename("Is the TPI open?")]
        public bool isOpen = false;

        [HideInInspector]
        public TPI_Workflow currentWorkflow; // workflowSnippets and workflowConstraints will only get updated once it is saved

        // Manage Workflows Menu Options
        [Tooltip("Add the Prefab for the Dialog Menu that gives the operator the option to manage the workflows.")]
        [SerializeField] private GameObject manageWorkflowsMenuPrefab;
        [Tooltip("Add the delete icon that will be shown on the button in the Dialog Menu that gives the operator the option to manage the workflows once the ManageWorkflows Button in the HandMenu is pressed.")]
        public Texture2D deleteButtonIcon;
        [Tooltip("Add the abort icon that will be shown on the button in the Dialog Menu that gives the operator the option to manage the workflows once the ManageWorkflows Button in the HandMenu is pressed.")]
        public Texture2D abortButtonIcon;

        // Various Icon Options
        [Tooltip("Add the icon that will be shown on the button in the Dialog Menu that asks the operator to enter a Workflow Name once the CreateWorkflow Button in the HandMenu is pressed.")]
        public Texture2D startButtonIcon;
        [Tooltip("Add the icon that will be shown on the CreateWorkflow Button in the HandMenu to create a new workflow.")]
        public Texture2D createWorkflowButtonIcon;
        [Tooltip("Add the icon that will be shown on the ToggleTPI Button in the HandMenu to hide the TaskPlanningInterface.")]
        public Texture2D hideButtonIcon;
        [Tooltip("Add the icon that will be shown on the ToggleTPI Button in the HandMenu to show the TaskPlanningInterface if it is hidden.")]
        public Texture2D showButtonIcon;
        [Tooltip("Add the overwrite icon that will be shown on the button in the Dialog Menu that gives the operator the either overwrite an exisiting save or create a new one.")]
        public Texture2D overwriteButtonIcon;
        [Tooltip("Add the icon that will be shown on the button in the Dialog Menu that asks the operator to select and load a Workflow once the LoadWorkflow Button in the HandMenu is pressed.")]
        public Texture2D loadButtonIcon;
        [Tooltip("Add the create new save icon that will be shown on the button in the Dialog Menu that gives the operator the either overwrite an exisiting save or create a new one.")]
        public Texture2D createNewSaveButtonIcon;
        [Tooltip("Add the icon that should be shown on the button that asks the operator to reassign something.")]
        public Texture2D reassignButtonIcon;
        [Tooltip("Add the icon that should be shown on the button in the Manage Workflows menu that renames the current workflow.")]
        public Texture2D renameButtonIcon;

        // References (Foldout in the Inspector)
        public GameObject selectionMenu;
        public GameObject buildingBlocksMenu;
        public GameObject sequenceMenu;
        public GameObject sequenceFunctionsMenu;
        public GameObject rosStatusMenu;
        public GameObject rosController;
        public GameObject objectPlacementController;
        public GameObject handMenu_workflow;
        public GameObject handMenu_objectPlacement;
        public GameObject dialogMenuContainer;
        public GameObject snippetFunctionContainer;
        public GameObject constraintFunctionContainer;
        public GameObject robotURDF;

        private void Start() {

            // Check that the prefabs have been setup correctly
            if (manageWorkflowsMenuPrefab == null)
                Debug.LogError("No prefab for the workflow management dialog menu assigned in the TPI_MainController component in " + transform.name);

            // Check that the References have been setup correctly
            if(selectionMenu == null)
                Debug.LogError("Please assign the Selection Menu GameObject in the TPI_MainController component in " + transform.name);
            if (buildingBlocksMenu == null)
                Debug.LogError("Please assign the Building Blocks Menu GameObject in the TPI_MainController component in " + transform.name);
            if (sequenceMenu == null)
                Debug.LogError("Please assign the Sequence Menu GameObject in the TPI_MainController component in " + transform.name);
            if (sequenceFunctionsMenu == null)
                Debug.LogError("Please assign the Sequence Functions Menu GameObject in the TPI_MainController component in " + transform.name);
            if (rosStatusMenu == null)
                Debug.LogError("Please assign the ROS Status Menu GameObject in the TPI_MainController component in " + transform.name);
            if (rosController == null)
                Debug.LogError("Please assign the ROS Controller GameObject in the TPI_MainController component in " + transform.name);
            if (objectPlacementController == null)
                Debug.LogError("Please assign the Object Placement Controller GameObject in the TPI_MainController component in " + transform.name);
            if (handMenu_workflow == null)
                Debug.LogError("Please assign the Workflow HandMenu GameObject in the TPI_MainController component in " + transform.name);
            if (handMenu_objectPlacement == null)
                Debug.LogError("Please assign the ObjectPlacement HandMenu GameObject in the TPI_MainController component in " + transform.name);
            if (dialogMenuContainer == null)
                Debug.LogError("Please assign the Dialog Menu Container GameObject in the TPI_MainController component in " + transform.name);
            if (snippetFunctionContainer == null)
                Debug.LogError("Please assign theSnippet Functions Container GameObject in the TPI_MainController component in " + transform.name);
            if (constraintFunctionContainer == null)
                Debug.LogError("Please assign the Constraint Functions GameObject in the TPI_MainController component in " + transform.name);
            if (robotURDF == null)
                Debug.LogError("Please assign the Robot URDF GameObject in the TPI_MainController component in " + transform.name);

            // Prepare TPI for startup (Activate & Deactivate GameObjects depending on whether it is needed)
            selectionMenu.SetActive(false); // Selection Menu
            buildingBlocksMenu.SetActive(false); // Building Blocks Menu
            sequenceMenu.SetActive(false); // Sequence Menu
            sequenceFunctionsMenu.SetActive(false); // Sequence Functions Menu
            rosStatusMenu.SetActive(false); // ROS Status Menu
            rosController.SetActive(true); // ROS Controller
            robotURDF.transform.GetChild(1).gameObject.SetActive(false);
            objectPlacementController.SetActive(true); // Object Placement Controller
            handMenu_workflow.SetActive(true); // Workflow HandMenu
            handMenu_objectPlacement.SetActive(true); // Object Placement HandMenu
            dialogMenuContainer.SetActive(false); // Dialog Menus
            snippetFunctionContainer.SetActive(false); // Snippets
            constraintFunctionContainer.SetActive(false); // Constraints


            currentWorkflow = null;

            // Reset Create Workflow / Toggle TPI Visibility Button
            ButtonConfigHelper helper = handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Setup "Create Workflow / Toggle TPI Visibility Button" in Workflow HandMenu
            helper.MainLabelText = "Create\nWorkflow";
            if (createWorkflowButtonIcon != null)
                helper.SetQuadIcon(createWorkflowButtonIcon);

        }

        /// <summary>
        /// Call this function if you want to reset all changes and settings made in the TPI Menu.
        /// </summary>
        public void ResetTPI() {
            sequenceMenu.GetComponent<TPI_SequenceMenuController>().ResetSequenceMenu();
            GetComponent<TPI_SaveController>().ResetSaveController();
            GetComponent<TPI_WorkflowConfigurationController>().CloseBuildingBlocksMenu();
            GetComponent<TPI_DialogMenuController>().ClearDialogMenus(); // Close all Dialog Menus
            GetComponent<TPI_TutorialController>().ResetTutorial();
            isOpen = false;
            selectionMenu.SetActive(false); // Selection Menu
            buildingBlocksMenu.SetActive(false); // Building Blocks Menu
            sequenceMenu.SetActive(false); // Sequence Menu
            sequenceFunctionsMenu.SetActive(false); // Sequence Functions Menu
            rosStatusMenu.SetActive(false); // ROS Status Menu
            dialogMenuContainer.SetActive(true); // Dialog Menus
            snippetFunctionContainer.SetActive(false); // Snippets
            constraintFunctionContainer.SetActive(false); // Constraints
            currentWorkflow = null;
            objectPlacementController.GetComponent<TPI_ObjectPlacementController>().ClearAllSpots();
            objectPlacementController.GetComponent<TPI_ObjectPlacementController>().ResetPreviousPositionIndex();

            // Reset Create Workflow / Toggle TPI Visibility Button
            ButtonConfigHelper helper = handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Setup "Create Workflow / Toggle TPI Visibility Button" in Workflow HandMenu
            helper.MainLabelText = "Create\nWorkflow";
            if (createWorkflowButtonIcon != null)
                helper.SetQuadIcon(createWorkflowButtonIcon);
        }

        #region ToggleTPI
        /// <summary>
        /// Create a new Workflow (only if using for the first time) or toggle the visibility of the Task Planning Interface by using this function.
        /// </summary>
        public void ToggleTPI() {

            if (isOpen) { //Deactivate TPI_Menu

                isOpen = false;
                GetComponent<TPI_DialogMenuController>().ClearDialogMenus(); // Close all Dialog Menus

                selectionMenu.SetActive(false); // Selection Menu
                buildingBlocksMenu.SetActive(false); // Building Blocks Menu
                sequenceMenu.SetActive(false); // Sequence Menu
                sequenceFunctionsMenu.SetActive(false); // Sequence Functions Menu
                rosStatusMenu.SetActive(false); // ROS Status Menu
                dialogMenuContainer.SetActive(false); // Dialog Menus
                snippetFunctionContainer.SetActive(false); // Snippets
                constraintFunctionContainer.SetActive(false); // Constraints

                // Configure "Create Workflow / Toggle TPI Visibility Button" -> Show TPI
                ButtonConfigHelper helper = handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Create Workflow / Toggle TPI Visibility Button
                helper.MainLabelText = "Show\nTPI";
                if (showButtonIcon != null)
                    helper.SetQuadIcon(showButtonIcon);

            } else { //Activate TPI_Menu

                dialogMenuContainer.SetActive(true); // Dialog Menus
                snippetFunctionContainer.SetActive(true); // Snippets
                constraintFunctionContainer.SetActive(true); // Constraints
                if (currentWorkflow != null) { // check if a workflow has been created or loaded
                    isOpen = true;
                    selectionMenu.SetActive(true); // Selection Menu
                    sequenceMenu.SetActive(true); // Sequence Menu
                    sequenceFunctionsMenu.SetActive(true); // Sequence Functions Menu
                    if(!rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated())
                        rosStatusMenu.SetActive(true); // ROS Status Menu
                    sequenceMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = currentWorkflow.workflowName + " Workflow"; // Set correct Workflow Name in Sequence Menu

                    // Configure "Create Workflow / Toggle TPI Visibility Button" -> Hide TPI
                    ButtonConfigHelper helper = handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Create Workflow / Toggle TPI Visibility Button
                    helper.MainLabelText = "Hide\nTPI";
                    if (hideButtonIcon != null)
                        helper.SetQuadIcon(hideButtonIcon);
                } else { // Create a new Workflow
                    GetComponent<TPI_DialogMenuController>().ShowObjectNameMenu("Create a new Workflow", "Please enter the desired name of the workflow:", "Create Workflow", startButtonIcon,
                        "Workflow Name:", "The name of the workflow cannot be null. Please use the keyboard field to give the workflow a name.", CreateWorkflow, true, true);
                }

            }

        }

        /// <summary>
        /// Helper Function that gets called once the button in the workflow name selection menu is pressed
        /// <para><paramref name="workflowName"/> = Name of the Workflow</para>
        /// </summary>
        private void CreateWorkflow(string workflowName) {

            isOpen = true;
            currentWorkflow = new TPI_Workflow();
            currentWorkflow.workflowName = workflowName;
            selectionMenu.SetActive(true); // Selection Menu
            sequenceMenu.SetActive(true); // Sequence Menu
            sequenceFunctionsMenu.SetActive(true); // Sequence Functions Menu
            if (!rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated())
                rosStatusMenu.SetActive(true); // ROS Status Menu
            sequenceMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = currentWorkflow.workflowName + " Workflow"; // Set Workflow Name in Sequence Menu

            // Configure "Create Workflow / Toggle TPI Visibility Button" -> Hide TPI
            ButtonConfigHelper helper = handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Create Workflow / Toggle TPI Visibility Button
            helper.MainLabelText = "Hide\nTPI";
            if (hideButtonIcon != null)
                helper.SetQuadIcon(hideButtonIcon);

            // Setup correct positions for the different menus in the environment
            TPI_ObjectPlacementController placementController = objectPlacementController.GetComponent<TPI_ObjectPlacementController>();
            placementController.ReserveSpot(placementController.GetSpotLeft(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 1), selectionMenu, forceOverride: true, applyPose: true); // Selection Menu placed one spot to the left from the Middle
            placementController.ReserveSpot(placementController.GetSpotRight(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 1), sequenceMenu, forceOverride: true, applyPose: true); // Sequence Menu  placed one spot to the right from the Middle
            placementController.ReserveSpot(placementController.GetSpotRight(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 2), sequenceFunctionsMenu, forceOverride: true, applyPose: true); // Sequence Functions Menu placed two spots to the right from the Middle
            if (!rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated())
                placementController.ReserveSpot(placementController.GetSpotLeft(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 2), rosStatusMenu, forceOverride: true, applyPose: true); // ROS Status Menu placed two spots to the left from the Middle

        }
        #endregion ToggleTPI

        #region ManageWorkflows
        /// <summary>
        /// Function that gets called once the ManageWorkflow button in the HandMenu is pressed.
        /// <br></br>Opens a dialog menu that allows the operator to reset the current workflow (if applicable) or delete saved workflows.
        /// <br></br> If no workflow is currently active or no workflows were able to be loaded, it opens an error dialog menu.
        /// </summary>
        public void ManageWorkflows() {

            dialogMenuContainer.SetActive(true); // Dialog Menus
            GetComponent<TPI_DialogMenuController>().ClearDialogMenus();
            if (currentWorkflow == null && GetComponent<TPI_SaveController>().workflowSaveData.workflows.Count == 0) { // Show error menu (no workflow created or loaded)
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "You currently cannot perform this action as you have not created a workflow yet and as the Task Planning Interface was not able to find any saved workflows.", "Accept", startButtonIcon);
                return;
            }

            if (currentWorkflow != null && GetComponent<TPI_SaveController>().workflowSaveData.workflows.Count == 0) { // Show dialog menu to confirm that the current workflow should be deleted.
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu_TwoButtons("Confirmation Required", "The Task Planning Interface was not able to find any saved workflows.Therefore, the only action you are able to take is to discard the current changes or to rename the current workflow.\n" +
                    "To abort, please use the button in the title bar.", "Discard Changes", deleteButtonIcon, delegate { ResetTPI(); }, "Rename Workflow", renameButtonIcon, delegate {
                        GetComponent<TPI_DialogMenuController>().ShowObjectNameMenu("Rename the current Workflow", "Please enter the desired new name of the workflow:", "Rename the Workflow", startButtonIcon,
                            "Workflow Name:", "The name of the workflow cannot be null. Please use the keyboard field to give the workflow a new name.", RenameWorkflow, true, true);
                    }, showCloseMenuButton: true);
                return;
            }

            List<TPI_DialogMenuDropDown.OptionData> dropdownList = new List<TPI_DialogMenuDropDown.OptionData>(); // Create the dropdown options list
            for (int i = 0; i < GetComponent<TPI_SaveController>().workflowSaveData.workflows.Count; i++) {
                dropdownList.Add(new TPI_DialogMenuDropDown.OptionData(GetComponent<TPI_SaveController>().workflowSaveData.workflows[i].workflowName, null));
            }

            if (currentWorkflow == null && GetComponent<TPI_SaveController>().workflowSaveData.workflows.Count != 0) { // Show dropdown selection dialog menu in order for the operator to select which workflows he wants to delete
                GetComponent<TPI_DialogMenuController>().ShowDropdownSelectionMenu("Manage Workflows", "As you have not created or loaded a workflow yet, you can only manage your workflow saves.", "Delete Workflow", deleteButtonIcon, "Select Workflow to delete:", dropdownList, DeleteWorkflow, true, false);
                return;
            }

            // Show the full workflow management dialog menu -> there is an active workflow and the TPI was able to load workflows
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = "Manage Workflows";
            dialogMenuInfo.dialogMenuPrefab = manageWorkflowsMenuPrefab;
            dialogMenuInfo.showCloseMenuButton = true;

            dialogMenuInfo.dialogMenuTexts.Add("In the case that you want to discard all changes of the current workflow or rename the current workflow, please select the respective button below.");
            dialogMenuInfo.dialogMenuTexts.Add("In the case that you want to delete a saved workflow, please select the desired workflow from the dropdown list and then click on the button below.");

            TPI_DialogMenuButton discardButton = new TPI_DialogMenuButton();
            discardButton.buttonText = "Discard Workflow";
            if (deleteButtonIcon != null)
                discardButton.buttonIcon = deleteButtonIcon;
            discardButton.buttonOnClick.AddListener(delegate {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu_TwoButtons("Confirmation Required", "Are you sure that you want to discard all changes of your current Workflow?", "Confirm", deleteButtonIcon, delegate { ResetTPI(); }, "Abort", abortButtonIcon, null);
            });
            dialogMenuInfo.dialogMenuButtons.Add(discardButton);


            TPI_DialogMenuButton renameButton = new TPI_DialogMenuButton();
            renameButton.buttonText = "Rename Workflow";
            if (renameButtonIcon != null)
                renameButton.buttonIcon = renameButtonIcon;
            renameButton.buttonOnClick.AddListener(delegate {
                GetComponent<TPI_DialogMenuController>().ShowObjectNameMenu("Rename the current Workflow", "Please enter the desired new name of the workflow:", "Rename the Workflow", startButtonIcon,
                            "Workflow Name:", "The name of the workflow cannot be null. Please use the keyboard field to give the workflow a new name.", RenameWorkflow, true, true);
            });
            dialogMenuInfo.dialogMenuButtons.Add(renameButton);


            TPI_DialogMenuButton saveButton = new TPI_DialogMenuButton();
            saveButton.buttonText = "Delete Workflow";
            if (deleteButtonIcon != null)
                saveButton.buttonIcon = deleteButtonIcon;
            saveButton.buttonOnClick.AddListener(delegate { DeleteWorkflow(GetComponent<TPI_DialogMenuController>().GetDialogMenuChoices(dialogMenuInfo.dialogMenuID).dropdown[0]); });
            dialogMenuInfo.dialogMenuButtons.Add(saveButton);

            dialogMenuInfo.dialogMenuDropdowns.Add(new TPI_DialogMenuDropDown("Select Workflow", dropdownList));

            GetComponent<TPI_DialogMenuController>().SpawnDialogMenu(dialogMenuInfo);

        }

        /// <summary>
        /// Helper Function to rename the current Workflow
        /// <para><paramref name="workflowName"/> = Name of the Workflow</para>
        /// </summary>
        private void RenameWorkflow(string workflowName) {
            currentWorkflow.workflowName = workflowName;
            sequenceMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = workflowName + " Workflow"; // Set Workflow Name in Sequence Menu
        }

        /// <summary>
        /// Helper Function that gets called once the operator has decided to delete a workflow
        /// <para><paramref name="index"/> = index of the workflow in the saved workflows list that should get deleted</para>
        /// </summary>
        public void DeleteWorkflow(int index) {
            GetComponent<TPI_SaveController>().DeleteWorkflow(GetComponent<TPI_SaveController>().workflowSaveData.workflows[index].workflowID);
        }
        #endregion ManageWorkflows

        #region LoadWorkflow
        /// <summary>
        /// Function that gets called once the LoadWorkflow button in the HandMenu is pressed.
        /// <br></br>Opens a dialog menu if a workflow has been loaded before or if one has been created in order to confirm that that the operator wants to delete the previous workflow.
        /// <br></br>Finally, it opens a dialog menu, where the operator can select the workflow he wants to load.
        /// </summary>
        public void LoadWorkflow() {

            dialogMenuContainer.SetActive(true); // Dialog Menus
            if (GetComponent<TPI_SaveController>().workflowSaveData.workflows.Count != 0) {
                if (currentWorkflow != null) {

                    GetComponent<TPI_DialogMenuController>().ShowErrorMenu_TwoButtons("Confirmation Required", "Are you sure that you want to discard all changes of your current Workflow in order to load an existing Workflow?", "Confirm", startButtonIcon, delegate { OpenLoadTPIMenu(true); }, "Abort", abortButtonIcon, null);

                } else {
                    OpenLoadTPIMenu(false); // false -> load workflow without resetting TPI
                }
            } else
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "No saved Workflows were found. Please save a Workflow before you try to load one.", "Confirm");
        }

        /// <summary>
        /// Helper Function that gets called once the user is ready to load a new workflow (after LoadWorkflow was called).
        /// <para><paramref name="resetTPI"/> = Determines whether the TPI should be reset before loading the workflow</para>
        /// </summary>
        private void OpenLoadTPIMenu(bool resetTPI) {

            // Reset SequenceMenu
            if(resetTPI) {
                ResetTPI();
            }

            // Create the correct List of options for the Dropdown
            List<TPI_DialogMenuDropDown.OptionData> dropdownList = new List<TPI_DialogMenuDropDown.OptionData>();
            TPI_SaveController saveController = GetComponent<TPI_SaveController>();
            for (int i = 0; i < saveController.workflowSaveData.workflows.Count; i++) {
                dropdownList.Add(new TPI_DialogMenuDropDown.OptionData(saveController.workflowSaveData.workflows[i].workflowName, null));
            }

            GetComponent<TPI_DialogMenuController>().ShowDropdownSelectionMenu("Load Workflow", "Please select the workflow you want to load:", "Load Workflow", loadButtonIcon, "Saved Workflows:", dropdownList, LoadTPI, true, false);

        }

        /// <summary>
        /// Helper Function that gets called once a previously created workflow has beed selected using the dropdown menu.
        /// <br></br>This function then makes the final changes needed for the TPI and tells the SaveController to load a specific workflow.
        /// <para><paramref name="selectedIndex"/> = Index of the dropdown options selected in the dropdown menu</para>
        /// </summary>
        private void LoadTPI(int selectedIndex) {

            isOpen = true;
            selectionMenu.SetActive(true); // Selection Menu
            sequenceMenu.SetActive(true); // Sequence Menu
            sequenceFunctionsMenu.SetActive(true); // Sequence Functions Menu
            if (!rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated())
                rosStatusMenu.SetActive(true); // ROS Status Menu
            snippetFunctionContainer.SetActive(true); // Snippets
            constraintFunctionContainer.SetActive(true); // Constraints

            string selectedWorkflowID = GetComponent<TPI_SaveController>().workflowSaveData.workflows[selectedIndex].workflowID;
            GetComponent<TPI_SaveController>().LoadWorkflow(selectedWorkflowID);

            // Configure "Create Workflow / Toggle TPI Visibility Button" -> Hide TPI
            ButtonConfigHelper helper = handMenu_workflow.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Create Workflow / Toggle TPI Visibility Button
            helper.MainLabelText = "Hide\nTPI";
            if (hideButtonIcon != null)
                helper.SetQuadIcon(hideButtonIcon);

            // Setup correct positions for the different menus in the environment
            TPI_ObjectPlacementController placementController = objectPlacementController.GetComponent<TPI_ObjectPlacementController>();
            placementController.ReserveSpot(placementController.GetSpotLeft(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 1), selectionMenu, forceOverride: true, applyPose: true); // Selection Menu placed one spot to the left from the Middle
            placementController.ReserveSpot(placementController.GetSpotRight(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 1), sequenceMenu, forceOverride: true, applyPose: true); // Sequence Menu  placed one spot to the right from the Middle
            placementController.ReserveSpot(placementController.GetSpotRight(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 2), sequenceFunctionsMenu, forceOverride: true, applyPose: true); // Sequence Functions Menu placed two spots to the right from the Middle
            if (!rosController.GetComponent<TPI_ROSController>().IsROSConnectionDeactivated())
                placementController.ReserveSpot(placementController.GetSpotLeft(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 2), rosStatusMenu, forceOverride: true, applyPose: true); // ROS Status Menu placed two spots to the left from the Middle

        }
        #endregion LoadWorkflow

        #region SaveWorkflow
        /// <summary>
        /// Function that gets called once the SaveWorkflow button in the HandMenu is pressed.
        /// <br></br>Opens a dialog menu where the operator can choose to either create a new workflow save or overwrite an exisiting (if this workflow was loaded before)
        /// <br></br>If no workflow was created or loaded, it opens an error menu.
        /// </summary>
        public void SaveWorkflow() {
            dialogMenuContainer.SetActive(true); // Dialog Menus
            if (currentWorkflow == null) { // no workflow has been created
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "Please first create a workflow before you try to save.", "Confirm");
                return;
            }

            if (GetComponent<TPI_SaveController>().activeWorkflowID == "") { // New Workflow has been created
                GetComponent<TPI_SaveController>().SaveWorkflow();
            } else { // Workflow has been loaded or the workflow has been saved before
                if (GetComponent<TPI_SaveController>().activeWorkflowID == currentWorkflow.workflowID) { // active workflow = previously loaded workflow
                    GetComponent<TPI_DialogMenuController>().ShowErrorMenu_TwoButtons("Save Option", "Please select whether you want to create a new workflow save or whether you would like to overwrite the exisiting save.",
                    "Create new save", createNewSaveButtonIcon, delegate { GetComponent<TPI_SaveController>().SaveWorkflow(false); }, "Overwrite save", overwriteButtonIcon, delegate { GetComponent<TPI_SaveController>().SaveWorkflow(true); });
                } else { // active workflow != previously loaded workflow -> should not happen, but the check was added as a safety measure
                    GetComponent<TPI_SaveController>().SaveWorkflow(false);
                }
            }
        }
        #endregion SaveWorkflow

        #region Tutorial
        /// <summary>
        /// Call this function to toggle the tutorial.
        /// </summary>
        public void ShowTutorial() {

            dialogMenuContainer.SetActive(true); // Dialog Menus
            GetComponent<TPI_TutorialController>().ToggleTutorial();

        }
        #endregion Tutorial

    }

}