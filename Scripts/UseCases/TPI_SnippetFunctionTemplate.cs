using System.Collections;
using TaskPlanningInterface.Controller;
using TaskPlanningInterface.Workflow;
using UnityEngine;

namespace TaskPlanningInterface.UseCase {

    public class TPI_SnippetFunctionTemplate : TPI_Snippet {

        public Texture2D nextButtonTexture;
        public GameObject snippetNameMenuPrefab;



        //---------------------------------------------------- Building Blocks Button related Functions ----------------------------------------------------//



        // As an example: a new dialog menu is openend that asks the operator to enter a Name for the Snippet
        public override void ButtonPressed() {

            // selectInitialName refers to the function below the ButtonPressed() function.
            // It is important that selectInitialName only receives one parameter of type string in order to work properly with the ShowObjectNameMenu function of the DialogMenuController!
            dialogMenuController.ShowObjectNameMenu(snippetInformation.snippetTemplateName + " Snippet", "Please enter the desired name of the snippet:", selectInitialName, true);

        }

        // This function receives the name of the snippet from the "ShowObjectNameMenu" function of the DialogMenuController as a parameter
        // It then applies the name to the snippet (assigns it to the TPI_SnippetInformation reference) and then adds the snippet to the sequence menu
        private void selectInitialName(string selectedName) {

            snippetInformation.snippetName = selectedName;
            sequenceMenuController.AddSnippet(gameObject.GetComponent<TPI_SnippetFunctionTemplate>()); // Add the snippet to the sequence menu (make it visible)

        }



        //---------------------------------------------------- Sequence Functions ----------------------------------------------------//



        // As an example: the Snippet is setup to wait for 2 seconds before checking whether the sequence has been paused.
        // If the Sequence is paused, the Coroutine also pauses here. Otherwise, the Snippet waits for another second before ending.
        // Usually, you would use ROS in order to send messages to the robot arm in this function
        public override IEnumerator RunSnippet() {
            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(() => TPI_SequenceMenuController.isPaused == false);
            yield return new WaitForSeconds(1f);
        }

        // As an example: nothing should happen as the "RunSnippet()" function does not make any chances to the robot arm (no movement, etc.)"
        // Usually, this would be the function that undoes any changes made in the RunSnippet() function.
        // StopSnippet will only get called mid-execution as there is a different behaviour for the pauses
        public override void StopSnippet() {
            return;
        }

        // As an example: nothing special should happen -> therefore calls "StopSnippet();"
        // Usually, this would be the function that undoes any changes made in the RunSnippet() function.
        // OnEmergencyStop will only get called mid-execution and only if the operator signals the TPI that it is an emergency
        public override void OnEmergencyStop() {
            StopSnippet();
        }



        //---------------------------------------------------- Data Management ----------------------------------------------------//



        // Setup of the UpdateSaveData as it is described in the TPI_Snippet class -> you can also hover over the function name to see the details
        public override void UpdateSaveData() {
            TPI_SnippetSaveData snippetSaveData = saveData;
            if (snippetSaveData == null)
                snippetSaveData = new TPI_SnippetFunctionTemplate_SaveData(); // Replace "TPI_SnippetFunctionTemplate_SaveData" with your own class that derives from TPI_SnippetSaveData
            snippetSaveData.saveManager_functionObjectName = snippetInformation.functionObject.name;
            snippetSaveData.information_snippetName = snippetInformation.snippetName;
            snippetSaveData.information_snippetTemplateName = snippetInformation.snippetTemplateName;
            snippetSaveData.information_snippetIcon = saveController.ConvertTextureToBytes(snippetInformation.snippetIcon);
            snippetSaveData.information_snippetID = snippetInformation.snippetID;
            this.saveData = snippetSaveData;

            /*
            
            Assign values here to your variables that you have created in your own SaveData class that derives from TPI_SnippetSaveData

            */

        }

        // As an example: like in ButtonPressed, a new dialog menu is opened to give the operator the chance to rename the snippet
        public override void ChangeVariables() {
            SetupControllerReferences();

            // selectNewName refers to the function below the ChangeVariables() function.
            // It is important that selectNewName only receives one parameter of type string in order to work properly with the ShowObjectNameMenu function of the DialogMenuController!
            dialogMenuController.ShowObjectNameMenu(snippetInformation.snippetTemplateName + " Snippet", "Please enter the desired name of the snippet:", selectNewName, true);

        }

        // As in the selectInitialName function from above, selectNewName receives the name of the snippet from the "ShowObjectNameMenu" function of the DialogMenuController as a parameter
        // It then applies the name to the snippet (assigns it to the TPI_SnippetInformation reference) and then updates the entry in the sequence menu
        private void selectNewName(string selectedName) {

            snippetInformation.snippetName = selectedName;
            sequenceMenuController.UpdateSnippetVisuals(gameObject.GetComponent<TPI_SnippetFunctionTemplate>());

        }

    }

    // Setup of the SaveData class for this template
    public class TPI_SnippetFunctionTemplate_SaveData : TPI_SnippetSaveData {

        /*
        
        Here, you can create your own variables that should be saved and loaded by the TaskPlanningInterface.
        Your SaveData class automatically includes all the variables setup in the TPI_SnippetSaveData abstract class.
        
        Please visit the following website in order to look at the supported types:
        https://docs.unity3d.com/2020.1/Documentation/Manual/JSONSerialization.html#:~:text=is%20not%20supported.-,Supported%20types,-The%20JSON%20Serializer


        Example:

        public string variable1;
        public int variable2;
        public bool variable3;
        public Vector3 variable4;

        */

    }

}