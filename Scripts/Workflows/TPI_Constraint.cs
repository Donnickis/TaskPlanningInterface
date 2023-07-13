using TaskPlanningInterface.EditorAndInspector;
using TaskPlanningInterface.Controller;
using UnityEngine;

namespace TaskPlanningInterface.Workflow {

    /// <summary>
    /// <para>
    /// The aim of this script is to act as a template for different kinds of constraints.
    /// <br></br>Therefore, every constraint (global and snippet-specific) will inherit these functions and variables defined here and can either change them or just plainly use them.
    /// <br></br> The following abstract functions have to always be implemented as they are important for every constraint:
    /// </para>
    /// 
    /// <list type="bullet">
    /// <listheader>
    ///    <term>ButtonPressed()</term>
    ///    <description>This function should implement what happens when the button is pressed in the building blocks menu.</description>
    /// </listheader>
    /// <item>
    ///    <term>ApplyConstraint()</term>
    ///    <description>This function handles what happens when the constraint starts to be applied.</description>
    /// </item>
    /// <item>
    ///    <term>StopConstraint()</term>
    ///    <description>This function handles what happens when the operator either wants to stop the snippet sequence or when the snippet has ended and the snippet-specific constraint thus should no longer be applied</description>
    /// </item>
    /// <item>
    ///    <term>UpdateSaveData()</term>
    ///    <description>This function correctly sets up and updates your save data script. For a detailed view how to do it please visit the respective summary of the function.</description>
    /// </item>
    /// </list>
    ///
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top,
    /// e.g. "using TaskPlanningInterface.Workflow" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior,
    /// you do not need to make any changes in this script.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public abstract class TPI_Constraint : MonoBehaviour {

        /// <summary>
        /// The dialogMenuController helps you with the creation of Dialog Menus -> visit DialogMenuController script to see how to implement Dialog Menus.
        /// </summary>
        [HideInInspector]
        public TPI_DialogMenuController dialogMenuController;

        /// <summary>
        /// The sequenceMenuController helps you in controlling the coroutine that controlls the sequence of snippets and constraints.
        /// </summary>
        [HideInInspector]
        public TPI_SequenceMenuController sequenceMenuController;

        /// <summary>
        /// The saveController helps you with saving and loading the variables of your constraint function scripts. Usually, you will hardly use this reference.
        /// </summary>
        [HideInInspector]
        public TPI_SaveController saveController;

        /// <summary>
        /// Information belonging to the constraint
        /// </summary>
        [Tooltip("Information belonging to the constraint")]
        public TPI_ConstraintInformation constraintInformation;

        /// <summary>
        /// Class that saves all variables that you want saved and loaded by the TPI
        /// </summary>
        [Tooltip("Class that saves all variables that you want saved and loaded by the TPI")]
        public TPI_ConstraintSaveData saveData;

        /// <summary>
        /// This function sets up the references to those Task Planning Interface Controllers, which you will use the most.
        /// </summary>
        public virtual void SetupControllerReferences() {
            dialogMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>();
            sequenceMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>();
            saveController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_SaveController>();
        }



        //---------------------------------------------------- Building Blocks Button related Function ----------------------------------------------------//



        /// <summary>
        /// This function should implement what happens when the button is pressed in the building blocks menu.
        /// <br></br>Create and start you dialog menus in order to configure the constraint
        /// </summary>
        public abstract void ButtonPressed();



        //---------------------------------------------------- Sequence Functions ----------------------------------------------------//



        /// <summary>
        /// This function handles what happens when the constraint starts to be applied, 
        /// <br></br>i.e. when the sequence progress in the sequence menu reaches the snippet to which constraint belongs to in the case of a snippet-specific constraint or when the snippet sequence is started in the case of a global constraint.
        /// <br></br>Therefore, apply the constraint in the necessary places.
        /// </summary>
        public abstract void ApplyConstraint();

        /// <summary>
        /// This function handles what happens when the operator either wants to stop the snippet sequence or when the snippet has ended and the snippet-specific constraint thus should no longer be applied
        /// <br></br>Please undo your constraint.
        /// </summary>
        public abstract void StopConstraint();



        //---------------------------------------------------- Visualization ----------------------------------------------------//



        /// <summary>
        /// This function handles what happens when the operator wants to visualize the constraint.
        /// <br></br>Therefore, you can determine yourself what should be visible or what should happen during the visualization.
        /// </summary>
        public abstract void VisualizeConstraint();


        /// <summary>
        /// This function handles what happens when the operator no longer wants to visualize the constraint.
        /// <br></br>Therefore, please undo the changes made in the VisualizeConstraint() function (e.g. destroy any instantiated gameObjects).
        /// </summary>
        public abstract void StopVisualization();



        //---------------------------------------------------- Data Management ----------------------------------------------------//



        /** <summary>
        This function correctly sets up and updates your save data script.
        <para>IMPORTANT: Please include the following in your function:</para>
        <code>
        TPI_ConstraintSaveData constraintSaveData = saveData;
        if(saveData == null)
            constraintSaveData = new TPI_ConstraintFunctionTemplate_SaveData();
        constraintSaveData.saveManager_functionObjectName = constraintInformation.functionObject.name;
        constraintSaveData.information_constraintName = constraintInformation.constraintName;
        constraintSaveData.information_constraintTemplateName = constraintInformation.constraintTemplateName;
        constraintSaveData.information_constraintType = constraintInformation.constraintType;
        constraintSaveData.information_constraintIcon = saveController.ConvertTextureToBytes(constraintInformation.constraintIcon);
        constraintSaveData.information_snippetID = constraintInformation.snippetID;
        constraintSaveData.information_constraintID = constraintInformation.constraintID;
        this.saveData = constraintSaveData;
        </code>
        You can also add more values that need to be setup when a save data class gets initialized
        </summary> */
        public abstract void UpdateSaveData();

        /// <summary>
        /// This function get called when the operator clicks on a Constraint in the Sequence Menu in order to change the values of the variables.
        /// <br></br>You can either let the operator go through the functions used in ButtonPressed in order to reassign them or create a new dialog menu that lets the operator choose what to change.
        /// </summary>
        public abstract void ChangeVariables();



        //---------------------------------------------------- Questions & Answers ----------------------------------------------------//
        /*
        

        1) Do I have to implement a save and load function myself?
        
            Those two functions were already implemented in the Task Planning Interface and are also able to handle your variables as long as also create and assign
            the variables in your own class that derives from TPI_ConstraintSaveData.

            But be careful: The TPI will only save your variables if you save the whole workflow using the "Save Workflow" button.
                            Otherwise, your workflow and therefore constraint will be discarded once the Hololens 2 is shutdown or once the TPI is reset.
        
        */

    }

    /// <summary>
    /// <para>
    /// The aim of this script is to store the main information of the constraints and thereby allowing it to be saved and passed between different C# scripts.
    /// <br>It also helps the Task Planning Interface distinguish between global and snippet-specific constraints.</br>
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top,
    /// e.g. "using TaskPlanningInterface.Workflow" without the quotes.
    /// 
    /// </para>
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior,
    /// you do not need to make any changes in this script.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [System.Serializable]
    public class TPI_ConstraintInformation {

        [Tooltip("Name of the constraint")]
        public string constraintName; // For the building block menu constrait templates: Contains the template name
                                      // For the sequence menu constraints: Contains the name that the operator selected
        [Tooltip("Name of the constraint template from the building blocks menu")][HideInInspector]
        public string constraintTemplateName; // Used for the Option to display, hide or shorten the constraint template name.
                                              // It is empty for the constraint templates in the building blocks menu, but contains the template name from the building blocks menu once the constraints are in the sequence menu
        [Tooltip("The constraint type (whether it is global or snippet-specific) is chosen in a dialog menu.")][ReadOnly][Rename("Constraint Type (configured in dialog menu)")]
        public TPI_ConstraintType constraintType;
        [Tooltip("Description of the underlying function of the constraint")] [TextArea]
        public string constraintDescription; // optional
        [Tooltip("Icon displayed next to the constraint (optional as there is a standard icon)")][Rename("Constraint icon (optional)")]
        public Texture2D constraintIcon; // optional
        [Tooltip("Empty GameObject that contains your from TPI_Constraint deriving script. The TPI_Snippet script contains the desired function of the snippet.")][Rename("Function Script Prefab")]
        public GameObject functionObject;
        [Tooltip("In the case of a snippet specific constraint, the TPI automatically adds the ID of the snippet.")][Rename("Snippet Parent ID (snippet-specific)")][HideInInspector]
        public string snippetID;
        [Tooltip("Constraint ID is automatically setup by the system")][HideInInspector]
        public string constraintID;

        public TPI_ConstraintInformation(string _constraintName, string _constraintTemplateName, TPI_ConstraintType _constraintType, string _constraintDescription, Texture2D _constraintIcon, GameObject _functionObject, string _snippetID) {
            this.constraintName = _constraintName;
            this.constraintTemplateName = _constraintTemplateName;
            this.constraintType = _constraintType;
            this.constraintDescription = _constraintDescription;
            this.constraintIcon = _constraintIcon;
            this.functionObject = _functionObject;
            this.snippetID = _snippetID;
            this.constraintID = "";
        }

        public TPI_ConstraintInformation(string _constraintName, string _constraintTemplateName, TPI_ConstraintType _constraintType, string _constraintDescription, Texture2D _constraintIcon, GameObject _functionObject, string _snippetID, string _constraintID) {
            this.constraintName = _constraintName;
            this.constraintTemplateName = _constraintTemplateName;
            this.constraintType = _constraintType;
            this.constraintDescription = _constraintDescription;
            this.constraintIcon = _constraintIcon;
            this.functionObject = _functionObject;
            this.snippetID = _snippetID;
            this.constraintID = _constraintID;
        }

    }

    /// <summary>
    /// This enum is used to distinguish between global and snippet specific constraints.
    /// </summary>
    public enum TPI_ConstraintType {
        [InspectorName("Global Constraint")]
        global, // 0
        [InspectorName("Snippet-specific Constraint")]
        snippetSpecific // 1
    }

    /// <summary>
    /// <para>
    /// The aim of this script is to make the saving and loading process in the TPI_SaveController much easier as every class that derives from TPI_ConstraintSaveData can be treated as a TPI_ConstraintSaveData instanceinstance.
    /// <br></br>Feel free to create as many variables in your own class that intherits from TPI_ConstraintSaveData as you would like.
    /// <br></br>Please make sure that you only use the supported types, which can be seen on this website (link is provided): <see href="https://docs.unity3d.com/2020.1/Documentation/Manual/JSONSerialization.html#:~:text=is%20not%20supported.-,Supported%20types,-The%20JSON%20Serializer">Supported Types</see>
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Workflow" without the quotes.
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
    [System.Serializable]
    public abstract class TPI_ConstraintSaveData {

        [HideInInspector]
        public string saveManager_functionObjectName; // Used to reconstruct the contraint after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public string information_constraintName; // Used to reconstruct constraintInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public string information_constraintTemplateName; // Used to reconstruct constraintInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public TPI_ConstraintType information_constraintType; // Used to reconstruct constraintInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public byte[] information_constraintIcon;
        [HideInInspector]
        public string information_snippetID; // Used to reconstruct constraintInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public string information_constraintID; // Used to reconstruct constraintInformation after loading as GameObject & Component cannot be saved properly in JSON

        /*
        
        Please create a new class that derives from TPI_ConstraintSaveData for every TPI_Constraint deriving script you create.
        Then, you can define your own variables that will get saved and loaded.
        This could look similiar to this:

        public class TPI_ConstraintFunctionTemplate_SaveData : TPI_ConstraintSaveData {

            public string variable1;
            public int variable2;
            public bool variable3;
            public Vector3 variable4;

        }
        
        */
    }

}