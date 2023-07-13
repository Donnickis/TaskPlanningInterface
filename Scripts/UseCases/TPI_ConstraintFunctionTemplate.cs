using TaskPlanningInterface.Workflow;

namespace TaskPlanningInterface.UseCase {
    public class TPI_ConstraintFunctionTemplate : TPI_Constraint {



        //---------------------------------------------------- Building Blocks Button related Function ----------------------------------------------------//



        // As an example: a new dialog menu is openend that asks the operator to enter a Name for the Snippet
        public override void ButtonPressed() {

            // selectInitialName refers to the function below the ButtonPressed() function.
            // It is important that selectInitialName only receives one parameter of type string in order to work properly with the ShowObjectNameMenu function of the DialogMenuController!
            dialogMenuController.ShowObjectNameMenu(constraintInformation.constraintTemplateName + " Constraint", "Please enter the desired name of the constraint:", selectInitialName, true);

        }

        // This function receives the name of the snippet from the "ShowObjectNameMenu" function of the DialogMenuController as a parameter
        // It then applies the name to the snippet (assigns it to the TPI_ConstraintInformation reference)
        private void selectInitialName(string selectedName) {

            constraintInformation.constraintName = selectedName;

            // As an example: a new dialog menu is openend that asks the operator to select whether it is a global or snippet-specific constraint
            // Furthermore, in the case of a snippet-specific constraint, it asks the operator to select the snippet to which the constraint should belong to
            dialogMenuController.ShowConstraintTypeMenu(selectType);

        }


        private void selectType(TPI_ConstraintType selectedType, string selectedSnippetID) {

            constraintInformation.constraintType = selectedType;
            constraintInformation.snippetID = selectedSnippetID;
            sequenceMenuController.AddConstraint(gameObject.GetComponent<TPI_ConstraintFunctionTemplate>(), selectedType); // Add the constraint to the sequence menu (make it visible if it it is a global one)


        }



        //---------------------------------------------------- Sequence Functions ----------------------------------------------------//



        // As an example: nothing is applied currently
        // Usually, you would use ROS in order to send messages to the robot arm in this function
        public override void ApplyConstraint() {
            return;
        }

        // As an example: nothing should happen as the "ApplyConstraint()" function does not make any chances to the robot arm"
        // Usually, this would be the function that undoes any changes made in the ApplyConstraint() function.
        // This function gets calld once the sequence has ended (global constraint) or once the snippet has ended (snippet-specific constraint)
        public override void StopConstraint() {
            return;
        }



        //---------------------------------------------------- Visualization ----------------------------------------------------//


        
        // Setup what happens if the Constraint should be visualized
        public override void VisualizeConstraint() {

            return; // If you want this function to visualize something (e.g. by instantiating a prefab), you can do it here.

        }

        // Setup what happens if the Constraint should no longer be visualized -> i.e. undo the changes made in the VisualizeConstraint() function
        public override void StopVisualization() {

            return; // As nothing was setup in the VisualizeConstraint() function, nothing has to be undone.
        }




        //---------------------------------------------------- Data Management ----------------------------------------------------//



        // Setup of the UpdateSaveData as it is described in the TPI_Constraint class -> you can also hover over the function name to see the details
        public override void UpdateSaveData() {
            TPI_ConstraintSaveData constraintSaveData = saveData;
            if (saveData == null)
                constraintSaveData = new TPI_ConstraintFunctionTemplate_SaveData();
            constraintSaveData.saveManager_functionObjectName = constraintInformation.functionObject.name;
            constraintSaveData.information_constraintName = constraintInformation.constraintName;
            constraintSaveData.information_constraintTemplateName = constraintInformation.constraintTemplateName;
            constraintSaveData.information_constraintType = constraintInformation.constraintType;
            constraintSaveData.information_constraintIcon = saveController.ConvertTextureToBytes(constraintInformation.constraintIcon);
            constraintSaveData.information_snippetID = constraintInformation.snippetID;
            constraintSaveData.information_constraintID = constraintInformation.constraintID;
            this.saveData = constraintSaveData;

            /*
            
            Assign values here to your variables that you have created in your own SaveData class that derives from TPI_SnippetSaveData

            */

        }

        // As an example: like in ButtonPressed, a new dialog menu is opened to give the operator the chance to rename the snippet
        public override void ChangeVariables() {

            SetupControllerReferences();
            // selectNewName refers to the function below the ChangeVariables() function.
            // It is important that selectNewName only receives one parameter of type string in order to work properly with the ShowObjectNameMenu function of the DialogMenuController!
            dialogMenuController.ShowObjectNameMenu(constraintInformation.constraintTemplateName + " Constraint", "Please enter the desired name of the constraint:", selectNewName, true);

        }

        // As in the selectInitialName function from above, selectNewName receives the name of the snippet from the "ShowObjectNameMenu" function of the DialogMenuController as a parameter
        // It then applies the name to the snippet (assigns it to the TPI_SnippetInformation reference) and then updates the entry in the sequence menu
        private void selectNewName(string selectedName) {

            constraintInformation.constraintName = selectedName;
            sequenceMenuController.UpdateConstraintVisuals(gameObject.GetComponent<TPI_ConstraintFunctionTemplate>());

        }

    }

    // Setup of the SaveData class for this template
    public class TPI_ConstraintFunctionTemplate_SaveData : TPI_ConstraintSaveData {

        /*
        
        Here, you can create your own variables that should be saved and loaded by the TaskPlanningInterface.
        Your SaveData class automatically includes all the variables setup in the TPI_ConstraintSaveData abstract class.
        
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
