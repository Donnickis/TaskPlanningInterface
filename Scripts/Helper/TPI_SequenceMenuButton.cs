using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using TaskPlanningInterface.Controller;
using TaskPlanningInterface.DialogMenu;
using TaskPlanningInterface.Workflow;
using UnityEngine;
using UnityEngine.EventSystems;
using static TaskPlanningInterface.Controller.TPI_SequenceMenuController;

namespace TaskPlanningInterface.Helper {

    /// <summary>
    /// <para>
    /// This script handles the deletion of Snippets and Constraints and the drag and drop feature to move Snippets.
    /// <br></br>It was copied and heavily adapted from the provided sources.
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
    /// Source for the deletion of list items for Snippets and Constraints: <see href="https://www.youtube.com/watch?v=QXKDtojz8zw">Click me</see>
    /// <br></br>Source for the drag and drop feature to move Snippets: <see href="https://www.youtube.com/watch?v=fOHK-pbgiD8">Click me</see>
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public class TPI_SequenceMenuButton : MonoBehaviour, IMixedRealityPointerHandler {

        // General Options
        private Transform topView;
        [Tooltip("Refers to the index of the object (snippet or constraint) in the snippet list or the constraint list of the SequenceMenu. Please do not change it manually.")]
        public int objectIndex = -1;
        [Tooltip("Amount of time remaining to allow double clicks.")]
        public float secondsRemaining = -1; // Keeps track on how much time is left to allow the operator to click the button again to activate the double click behaviour

        private Vector3 initialPointerPosition;
        private Vector3 initialTopViewPosition;
        private Vector3 currentTotalPosition;
        private Vector3 velocity; // only used internally

        [Tooltip("Determines whether this component belongs to a snippet button (= true) or to a constraint button (=false).")]
        public bool isSnippet = false;
        [HideInInspector] public TPI_Snippet snippetScript; // used to update the objectIndex for a snippet
        [HideInInspector] public TPI_Constraint constraintScript; // used to update the objectIndex for a constraint

        [Tooltip("Determines whether the button is currently held.")]
        public bool isHeld = false; //  -> assigned in the Handle...OnHoldButtonEvents function of the SequenceMenuController (... is either 'Snippet' or 'Constraint')
        private bool horizontalDrag = false; // designates whether the button is dragged horizontally
        private bool verticalDrag = false; // designates whether the button is dragged vertically

        [Tooltip("Distance needed to determine that the button is dragged horizontally.")]
        public float minimalHorizontalDragDistance = 0.00425f;
        [Tooltip("Distance needed to determine that the button is dragged vertically.")]
        public float minimalVerticalDragDistance = 0.00425f;
        [Tooltip("Maximal distance that the button can be dragged horizontally to the right.")]
        public float maximalHorizontalDragDistance = 0.0225f;
        [Tooltip("Distance that the button is moved away from the sequence menu when it is dragged vertically (provides visual feedback that it is dragged).")]
        public float dragAndDrop_ZOffset = 0.015f;

        private List<TPI_Constraint> temporaryConstraintList; // internal use only

        private TPI_SequenceMenuController sequenceMenuController; // reference to the SequenceMenuCOntroller
        private TPI_DialogMenuController dialogMenuController; // reference to the DialogMenuController

        private string openDeletionMenuID = ""; // internal use only

        private void Start() {
            topView = transform.GetChild(0);
            sequenceMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>();
            dialogMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>();
        }

        private void Update() {
            if (secondsRemaining != -1) { // Otherwise the countdown is not active
                secondsRemaining -= Time.deltaTime;
                if (secondsRemaining <= 0)
                    secondsRemaining = -1; // Deactivate coundown
            }
        }

        /// <summary>
        /// Prevents Problems with MRTK that the InteractableOnHoldReceiver never stops 
        /// </summary>
        public void ResetComponent() {
            ResetButton();
            isHeld = false;
            horizontalDrag = false;
            verticalDrag = false;
            velocity = Vector3.zero;
            if(temporaryConstraintList != null) {
                temporaryConstraintList.Clear();
                temporaryConstraintList = null;
            }
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData) {

            // Prevents Problems with MRTK that the InteractableOnHoldReceiver never stops
            // isHeld is only active if there was a problem with the InteractableOnHoldReceiver before, as the InteractableOnHoldReceiver has not detected a new hold yet.
            if (isHeld)
                ResetButton();

            if (isSnippet)
                objectIndex = sequenceMenuController._snippetObjects.IndexOf(snippetScript);
            else
                objectIndex = sequenceMenuController._constraintObjects.IndexOf(constraintScript);

            initialPointerPosition = eventData.Pointer.Result.Details.Point;
            initialTopViewPosition = topView.localPosition;
            currentTotalPosition = transform.position;

        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData) {}
        public void OnPointerDragged(MixedRealityPointerEventData eventData) {

            if(isHeld) { // isHeld is only true if snippet has already been selected before -> assigned in the Handle...OnHoldButtonEvents function of the SequenceMenuController (... is either Snippet or Constraint)

                if (!(sequenceMenuController._sequenceState == SequenceState.notStarted || sequenceMenuController._sequenceState == SequenceState.stopped)) // Snippet Selection should be possible at runtime; change in the values of variables, drag and drop and deletion not
                    return;

                // Disable scrolling in ScrollingObjectCollection
                transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().CanScroll = false;

                Vector3 distance = eventData.Pointer.Result.Details.Point - initialPointerPosition;

                // Determines whether the button is dragged horizontally or vertically
                if (!horizontalDrag && !verticalDrag) {
                    if (distance.x > minimalHorizontalDragDistance) {
                        horizontalDrag = true;
                        transform.GetChild(1).gameObject.SetActive(true); // BottomView GameObject
                        return;
                    } else if (isSnippet && Mathf.Abs(distance.y) > minimalVerticalDragDistance) {
                        verticalDrag = true;
                        return;
                    }
                }

                //---------------------------------------------------- Horizontal Drag in OnPointerDragged ----------------------------------------------------//
                if (horizontalDrag) {

                    if (distance.x > 0) {
                        if(distance.x > maximalHorizontalDragDistance)
                            topView.localPosition = new Vector3(initialTopViewPosition.x + maximalHorizontalDragDistance, initialTopViewPosition.y, initialTopViewPosition.z);
                        else if (distance.x < minimalHorizontalDragDistance) {
                            topView.localPosition = Vector3.SmoothDamp(topView.localPosition, new Vector3(initialTopViewPosition.x + Mathf.Abs(distance.x), initialTopViewPosition.y, initialTopViewPosition.z), ref velocity, 0.25f);
                        } else {
                            topView.localPosition = new Vector3(initialTopViewPosition.x + Mathf.Abs(distance.x), initialTopViewPosition.y, initialTopViewPosition.z);
                        }

                    } else {
                        topView.localPosition = initialTopViewPosition;
                        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler); ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }



                //---------------------------------------------------- Vertical Drag in OnPointerDragged ----------------------------------------------------//
                } else if (isSnippet && verticalDrag) {

                    Vector3 tempPosition = new Vector3(transform.position.x, eventData.Pointer.Result.Details.Point.y, currentTotalPosition.z - dragAndDrop_ZOffset);

                    if(objectIndex > 0) { // not the top-most snippet


                        if (objectIndex < sequenceMenuController.GetSnippetSequenceLength() - 1) { // not the bottom-most snippet (somewhere in the middle)

                            transform.position = tempPosition;

                            if (tempPosition.y > transform.parent.GetChild(objectIndex - 1).position.y) {

                                currentTotalPosition = transform.parent.GetChild(objectIndex - 1).position;
                                sequenceMenuController.SwitchSnippets(objectIndex, objectIndex - 1);

                            } else if(tempPosition.y < transform.parent.GetChild(objectIndex + 1).position.y) {

                                currentTotalPosition = transform.parent.GetChild(objectIndex + 1).position;
                                sequenceMenuController.SwitchSnippets(objectIndex, objectIndex + 1);

                            }

                        } else { // the bottom-most snippet

                            if (tempPosition.y >= currentTotalPosition.y) {

                                transform.position = tempPosition;

                                if (tempPosition.y > transform.parent.GetChild(objectIndex - 1).position.y) {

                                    currentTotalPosition = transform.parent.GetChild(objectIndex - 1).position;
                                    sequenceMenuController.SwitchSnippets(objectIndex, objectIndex - 1);

                                }

                            }

                        }

                    } else { // the top-most snippet

                        if (tempPosition.y <= currentTotalPosition.y) {

                            transform.position = tempPosition;

                            if (tempPosition.y <= transform.parent.GetChild(objectIndex + 1).position.y) {

                                currentTotalPosition = transform.parent.GetChild(objectIndex + 1).position;
                                sequenceMenuController.SwitchSnippets(objectIndex, objectIndex + 1);

                            }

                        }

                    }

                }

            }

        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) {
            
            if (isHeld) { // isHeld is only true if snippet has already been selected before -> assigned in the Handle...OnHoldButtonEvents function of the SequenceMenuController (... is either Snippet or Constraint)

                if(!(sequenceMenuController._sequenceState == SequenceState.notStarted || sequenceMenuController._sequenceState == SequenceState.stopped)) // Snippet Selection should be possible at runtime; change in the values of variables, drag and drop and deletion not
                    return;

                // Enable scrolling in ScrollingObjectCollection
                transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().CanScroll = true;

                Vector3 distance = eventData.Pointer.Result.Details.Point - initialPointerPosition;



                //---------------------------------------------------- Horizontal Drag in OnPointerUp ----------------------------------------------------//
                if (horizontalDrag) { // object deletion -> for snippets & constraints
                    if (distance.x >= maximalHorizontalDragDistance) {
                        if (isSnippet) {
                            if (sequenceMenuController.requireDeletionConfirmation)
                                openDeletionMenuID = dialogMenuController.ShowErrorMenu_TwoButtons("Confirmation required", "Please confirm that you want to delete the Snippet: " + sequenceMenuController._snippetObjects[objectIndex].snippetInformation.snippetName, "Confirm", null, delegate { DeleteSnippet(); }, "Abort", null, delegate { ResetButton(false); });
                            else
                                sequenceMenuController.RemoveSnippet(objectIndex);
                        } else {
                            if (sequenceMenuController.requireDeletionConfirmation) {
                                for(int i = 0; i < transform.parent.childCount; i++) {
                                    if (i != transform.GetSiblingIndex())
                                        transform.parent.GetChild(i).GetComponent<TPI_SequenceMenuButton>().ResetButton();
                                }
                                openDeletionMenuID = dialogMenuController.ShowErrorMenu_TwoButtons("Confirmation required", "Please confirm that you want to delete the Constraint: " + sequenceMenuController._constraintObjects[objectIndex].constraintInformation.constraintName, "Confirm", null, delegate { DeleteConstraint(); }, "Abort", null, delegate { ResetButton(false); });
                            } else
                                sequenceMenuController.RemoveConstraint(sequenceMenuController._constraintObjects[objectIndex]);
                        }
                        
                    } else {
                        ResetButton();
                    }
                    horizontalDrag = false;
                    if (distance.x <= maximalHorizontalDragDistance) {
                        transform.GetChild(1).gameObject.SetActive(false); // BottomView GameObject
                        if (!isSnippet)
                            transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>().enabled = false;
                    }



                    //---------------------------------------------------- Vertical Drag in OnPointerDragged ----------------------------------------------------//
                } else if (isSnippet && verticalDrag) { // drag and drop -> only for snippets

                    transform.position = currentTotalPosition;
                    verticalDrag = false;

                }

                if(distance.x < minimalHorizontalDragDistance && !isSnippet) {
                    transform.GetChild(1).gameObject.SetActive(false); // BottomView GameObject
                    transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>().enabled = false;
                }

                isHeld = false;
                transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().MaskEnabled = true;
                transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().CanScroll = true;


                //---------------------------------------------------- Single & Double Click Behaviour ----------------------------------------------------//
            } else {

                if (isSnippet) { // Behaviour of Snippets -> single click and double click possible

                    if (secondsRemaining == -1) { // has not been pressed yet -> single click behaviour -> snippet selection

                        secondsRemaining = sequenceMenuController.standardButtonPressedCooldown;
                        sequenceMenuController.SelectSnippet(objectIndex);

                    } else { // double click behaviour -> change the values of the variables
                        if (sequenceMenuController._sequenceState == SequenceState.notStarted || sequenceMenuController._sequenceState == SequenceState.stopped) { // Snippet Selection should be possible at runtime; change in the values of variables, drag and drop and deletion not                                                                    
                            secondsRemaining = -1;
                            ResetButton();
                            dialogMenuController.ClearDialogMenus();
                            if(sequenceMenuController.requireVariableChangeConfirmation)
                                dialogMenuController.ShowErrorMenu_TwoButtons("Confirmation required", "Please confirm that you want to change the values of the variables of the Snippet: " + sequenceMenuController._snippetObjects[objectIndex].snippetInformation.snippetName, "Confirm", null, delegate { sequenceMenuController._snippetObjects[objectIndex].ChangeVariables(); }, "Abort", null, null);
                            else
                                sequenceMenuController._snippetObjects[objectIndex].ChangeVariables();
                        }

                    }

                } else { // Behaviour of Constraints -> only single click possible
                    // change the values of the variables
                    if (sequenceMenuController._sequenceState == SequenceState.notStarted || sequenceMenuController._sequenceState == SequenceState.stopped) { // Snippet Selection should be possible at runtime, move & deletion not
                        ResetButton();
                        dialogMenuController.ClearDialogMenus();
                        if(sequenceMenuController.requireVariableChangeConfirmation)
                            dialogMenuController.ShowErrorMenu_TwoButtons("Confirmation required", "Please confirm that you want to change the values of the variables of the Constraint: " + sequenceMenuController._constraintObjects[objectIndex].constraintInformation.constraintName, "Confirm", null, delegate { sequenceMenuController._constraintObjects[objectIndex].ChangeVariables(); }, "Abort", null, null);
                        else
                            sequenceMenuController._constraintObjects[objectIndex].ChangeVariables();
                    }

                }

            }
            
        }

        /// <summary>
        /// Helper function to reset the bottom view of the buttons and the current position.
        /// <para><paramref name="closeDialogMenu"/> = determines whether any previous dialog menus to delete this object will get closed</para>
        /// </summary>
        public void ResetButton(bool closeDialogMenu = true) {
            topView.localPosition = initialTopViewPosition;
            transform.GetChild(1).gameObject.SetActive(false); // BottomView GameObject
            if(!isSnippet)
                transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>().enabled = false;
            if (closeDialogMenu && openDeletionMenuID != "")
                dialogMenuController.UnSpawnDialogMenu(openDeletionMenuID);
            openDeletionMenuID = "";
        }

        /// <summary>
        /// Helper function to delete a snippet.
        /// </summary>
        private void DeleteSnippet() {
            openDeletionMenuID = "";
            sequenceMenuController.SelectSnippet(objectIndex);
            List<TPI_Constraint> specificConstraints = sequenceMenuController.GetSpecificConstraintsOfSnippet(sequenceMenuController._snippetObjects[objectIndex].snippetInformation.snippetID);
            sequenceMenuController.RemoveSnippet(objectIndex);
            if(specificConstraints.Count > 0) {
                dialogMenuController.ShowErrorMenu_TwoButtons("Snippet-specific Constraints", "The Task Planning Interface found snippet-specific Constraints that belong to the Snippet: " + sequenceMenuController._snippetObjects[objectIndex].snippetInformation.snippetName + "\n" +
                    "Please decide whether those snippet-specific Constraints should be deleted or whether all of them should be reassigned.", "Delete all", dialogMenuController.GetComponent<TPI_MainController>().deleteButtonIcon, delegate { DeleteAllSpecific(specificConstraints); }, "Reassign all", dialogMenuController.GetComponent<TPI_MainController>().reassignButtonIcon, delegate { ReassignAllSpecific(specificConstraints); });
            }
            foreach(TPI_Snippet snippet in sequenceMenuController.GetSnippetSequence()) {
                sequenceMenuController.UpdateSnippetVisuals(snippet);
            }
        }

        /// <summary>
        /// Helper function to delete all snippet-specific constraints that belong to a snippet.
        /// <para><paramref name="specificConstraints"/> = specific constraints that should be deleted</para>
        /// </summary>
        private void DeleteAllSpecific(List<TPI_Constraint> specificConstraints) {
            foreach(TPI_Constraint constraint in specificConstraints) {
                sequenceMenuController.RemoveConstraint(constraint);
            }
            foreach (TPI_Snippet snippet in sequenceMenuController.GetSnippetSequence()) {
                sequenceMenuController.UpdateSnippetVisuals(snippet);
            }
        }

        /// <summary>
        /// Helper function to initiate the reassignment of all snippet-specific constraints that belong to a snippet.
        /// <para><paramref name="specificConstraints"/> = specific constraints that should be reassigned</para>
        /// </summary>
        private void ReassignAllSpecific(List<TPI_Constraint> specificConstraints) {
            temporaryConstraintList = specificConstraints;
            List<TPI_DialogMenuDropDown.OptionData> dropdownList = new List<TPI_DialogMenuDropDown.OptionData>();
            for (int i = 0; i < sequenceMenuController.GetSnippetSequenceLength(); i++) {
                string positionText = (i + 1).ToString();
                if (positionText.Length == 1)
                    positionText = "0" + positionText;
                dropdownList.Add(new TPI_DialogMenuDropDown.OptionData(positionText + " |  " + sequenceMenuController.GetSnippetAt(i).snippetInformation.snippetName, sequenceMenuController.GetSnippetAt(i).snippetInformation.snippetIcon));
            }
            dialogMenuController.ShowDropdownSelectionMenu("Snippet Selection", "Please select the Snippet to which the Constraints should be reassigned to.", "Reassign", dialogMenuController.GetComponent<TPI_MainController>().reassignButtonIcon, "Available Snippets:", dropdownList, ReassignConstraints, false, false);
        }

        /// <summary>
        /// Helper function to reassign all snippet-specific constraints that belong to a snippet.
        /// <br></br>Gets invoked in the ReassignAllSpecific function.
        /// <para><paramref name="snippetIndex"/> = index of the new snippet ID</para>
        /// </summary>
        private void ReassignConstraints(int snippetIndex) {
            string snippetID = sequenceMenuController._snippetObjects[snippetIndex].snippetInformation.snippetID;
            foreach (TPI_Constraint constraint in temporaryConstraintList) {
                constraint.constraintInformation.snippetID = snippetID;
            }
            temporaryConstraintList.Clear();
            temporaryConstraintList = null;
        }

        /// <summary>
        /// Helper function to delete a constraint.
        /// </summary>
        private void DeleteConstraint() {
            openDeletionMenuID = "";
            sequenceMenuController.RemoveConstraint(sequenceMenuController._constraintObjects[objectIndex]);
        }

    }

}