using TaskPlanningInterface.EditorAndInspector;
using TaskPlanningInterface.Controller;
using UnityEngine;
using System.Collections;

namespace TaskPlanningInterface.Workflow {

    /// <summary>
    /// <para>
    /// The aim of this script is to act as a template for different kinds of snippets. 
    /// <br></br>Therefore, every snippet will inherit these functions and variables defined here and can either change them or just plainly use them.
    /// <br></br> The following abstract functions have to always be implemented as they are important for every snippet:
    /// </para>
    /// 
    /// <list type="bullet">
    /// <listheader>
    ///    <term>ButtonPressed()</term>
    ///    <description>This function should implement what happens when the button is pressed in the building blocks menu.</description>
    /// </listheader>
    /// <item>
    ///    <term>RunSnippet()</term>
    ///    <description>This function handles what happens when the snippet is started, i.e. when the sequence progress in the sequence menu reaches your snippet.</description>
    /// </item>
    /// <item>
    ///    <term>StopSnippet()</term>
    ///    <description>This function handles what happens when the operator wants to manually stop the snippet and go to the next one mid-execution.</description>
    /// </item>
    /// <item>
    ///    <term>UpdateSaveData()</term>
    ///    <description>This function correctly sets up and updates your save data script. For a detailed view how to do it please visit the respective summary of the function.</description>
    /// </item>
    /// </list>
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
    ///</para>
    /// </summary>
    public abstract class TPI_Snippet : MonoBehaviour {

        /// <summary>
        /// The dialogMenuController helps you with the creation of Dialog Menus -> visit the TPI_DialogMenuController script to see how it is done correctly.
        /// </summary>
        [HideInInspector]
        public TPI_DialogMenuController dialogMenuController;

        /// <summary>
        /// The sequenceMenuController helps you in controlling the coroutine that handles the sequence of snippets and constraints.
        /// </summary>
        [HideInInspector]
        public TPI_SequenceMenuController sequenceMenuController;

        /// <summary>
        /// The saveController helps you with saving and loading the variables of your snippet function scripts. Usually, you will hardly use this reference.
        /// </summary>
        [HideInInspector]
        public TPI_SaveController saveController;

        /// <summary>
        /// Information belonging to the snippet
        /// </summary>
        [Tooltip("Information belonging to the snippet")]
        public TPI_SnippetInformation snippetInformation;

        /// <summary>
        /// Class that saves all variables that you want saved and loaded by the TPI
        /// </summary>
        [Tooltip("Class that saves all variables that you want saved and loaded by the TPI")]
        public TPI_SnippetSaveData saveData;

        /// <summary>
        /// This function sets up the references to those Task Planning Interface Controllers, which you will use the most.
        /// </summary>
        public virtual void SetupControllerReferences() {
            dialogMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>();
            sequenceMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().sequenceMenu.GetComponent<TPI_SequenceMenuController>();
            saveController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_SaveController>();
        }



        //---------------------------------------------------- Building Blocks Button related Functions ----------------------------------------------------//



        /// <summary>
        /// This function should implement what happens when the button is pressed in the building blocks menu.
        /// <br></br>Create and start you dialog menus in order to configure the snippet
        /// </summary>
        public abstract void ButtonPressed();



        //---------------------------------------------------- Sequence Functions ----------------------------------------------------//



        /// <summary>
        /// This function handles what happens when the snippet is started, i.e. when the sequence progress in the sequence menu reaches your snippet.
        /// <br></br> Add <code>yield return new WaitUntil(() => TPI_SequenceMenuController.isPaused == false);</code> at the points, where the Task Planning Interface should pause if the Pause Button got pressed.
        /// </summary>
        /// 
        public abstract IEnumerator RunSnippet();

        /// <summary>
        /// This function handles what happens when your snippet function has ended, i.e. when the sequence progress in the sequence menu reaches the next snippet.
        /// <br></br>If you do not want something special to happen, just leave it as it is.
        /// </summary>
        public virtual void OnHasEnded() { return; }

        /// <summary>
        /// This function handles what happens when the operator wants to manually stop the sequence mid-execution.
        /// <br></br>Please undo the settings made in your <c>StartSnippet();</c> function
        /// </summary>
        public abstract void StopSnippet();

        /// <summary>
        /// This function handles what happens when the operator wants to manually stop the snippet and go to the next one mid-execution.
        /// <br></br>Please move the robot to your end position.
        /// </summary>
        public virtual void SkipSnippet() { return; }

        /// <summary>
        /// The operator indicated that he intends to let the snippet run again once the current iteration of the function has ended.
        /// <br></br>This function handles what should happen immediately before the second iteration starts.
        /// <br></br>It might be worth it to override this function in order to navigate the robot back to the start position from the end position or to tell the robot to refill his materials.
        /// <para>
        /// Please DO NOT call <c>StartSnippet();</c> in here as the SequenceMenuController handles this itself.
        /// </para>
        /// If you do not want something special to happen, just leave it as it is.
        /// </summary>
        public virtual void RepeatSnippet() { return; }

        /// <summary>
        /// This function handles what happens if the operator singals with his hands to abruptly stop due to an emergency.
        /// </summary>
        public virtual void OnEmergencyStop() { StopSnippet(); }



        //---------------------------------------------------- Data Management ----------------------------------------------------//



        /**<summary>
        This function correctly sets up and updates your save data script.
        <para>IMPORTANT: Please include the following in your function:</para>
        <code>
        TPI_SnippetSaveData snippetSaveData = saveData;
        if (snippetSaveData == null)
            snippetSaveData = new TPI_SnippetFunctionTemplate_SaveData();
        snippetSaveData.saveManager_functionObjectName = snippetInformation.functionObject.name;
        snippetSaveData.information_snippetName = snippetInformation.snippetName;
        snippetSaveData.information_snippetTemplateName = snippetInformation.snippetTemplateName;
        snippetSaveData.information_snippetIcon = saveController.ConvertTextureToBytes(snippetInformation.snippetIcon);
        snippetSaveData.information_snippetID = snippetInformation.snippetID;
        this.saveData = snippetSaveData;
        </code>
        You can also add more values that need to be updated and setup
        </summary> */
        public abstract void UpdateSaveData();

        /// <summary>
        /// This function gets called when the operator double clicks on a Snippet in the Sequence Menu in order to change the values of the variables.
        /// <br></br>You can either let the operator go through the functions used in ButtonPressed in order to reassign them or create a new dialog menu that lets the operator choose what to change.
        /// </summary>
        public abstract void ChangeVariables();



        //---------------------------------------------------- Questions & Answers ----------------------------------------------------//
        /*
        

        1) How can I override a function? (e.g. if you want to implement a different behaviour for SkipSnippet())
        
            Use the following to for example implement OnTouchBegin() in you own class that derives from TPI_Snippet:
            
            public override void SkipSnippet() {
                // Your Code
            }

        2) Do I have to implement a save and load function myself?
        
            Those two functions were already implemented in the Task Planning Interface and are also able to handle your variables as long as also create and assign
            the variables in your own class that derives from TPI_SnippetSaveData.

            But be careful: The TPI will only save your variables if you save the whole workflow using the "Save Workflow" button.
                            Otherwise, your workflow and therefore snippet will be discarded once the Hololens 2 is shutdown or once the TPI is reset.
        
        */

    }


    /// <summary>
    /// <para>
    /// The aim of this script is to store the main information of the snippets and thereby allowing it to be saved and passed between different C# scripts.
    /// </para>
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
    ///</para>
    /// </summary>

    [System.Serializable]
    public class TPI_SnippetInformation {

        [Tooltip("Name of the snippet")]
        public string snippetName; // For the building block menu snippet templates: Contains the template name
                                   // For the sequence menu snippets: Contains the name that the operator selected
        [Tooltip("Name of the snippet template from the building blocks menu")][HideInInspector]
        public string snippetTemplateName; // Used for the Option to display, hide or shorten the snippet template name.
                                           // It is empty for the snippet templates in the building blocks menu, but contains the template name from the building blocks menu once the snippet are in the sequence menu
        [Tooltip("Category in which this button should be placed")][Rename("Category ID")]
        public string snippetCategoryID;
        [Tooltip("Description of the underlying machine task of the snippet")][TextArea]
        public string snippetDescription; // optional
        [Tooltip("Icon displayed alongside the snippet (optional as there is a standard icon)")][Rename("Snippet icon (optional)")]
        public Texture2D snippetIcon; // optional
        [Tooltip("Empty GameObject that contains your from TPI_Snippet deriving script. The TPI_Snippet script contains the desired function of the snippet.")][Rename("Function Script Prefab")]
        public GameObject functionObject;
        [Tooltip("Snippet ID is automatically setup by the system")][HideInInspector]
        public string snippetID;

        public TPI_SnippetInformation(string _snippetName, string _snippetTemplateName, string _snippetCategoryID, string _snippetDescription, Texture2D _snippetIcon, GameObject _functionObject) {
            this.snippetName = _snippetName;
            this.snippetTemplateName = _snippetTemplateName;
            this.snippetCategoryID = _snippetCategoryID;
            this.snippetDescription = _snippetDescription;
            this.snippetIcon = _snippetIcon;
            this.functionObject = _functionObject;
            this.snippetID = "";
        }

        public TPI_SnippetInformation(string _snippetName, string _snippetTemplateName, string _snippetCategoryID, string _snippetDescription, Texture2D _snippetIcon, GameObject _functionObject, string _snippetID) {
            this.snippetName = _snippetName;
            this.snippetTemplateName = _snippetTemplateName;
            this.snippetCategoryID = _snippetCategoryID;
            this.snippetDescription= _snippetDescription;
            this.snippetIcon = _snippetIcon;
            this.functionObject = _functionObject;
            this.snippetID = _snippetID;
        }
    }

    /// <summary>
    /// <para>
    /// The aim of this script is to make the saving and loading process in the TPI_SaveController much easier as every class that derives from TPI_SnippetSaveData can be treated as a TPI_SnippetSaveData TPI_ConstraintSaveData.
    /// <br></br>Feel free to create as many variables in your own class that intherits from TPI_SnippetSaveData as you would like.
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
    ///</para>
    /// </summary>
    [System.Serializable]
    public abstract class TPI_SnippetSaveData {

        [HideInInspector]
        public string saveManager_functionObjectName; // Used to reconstruct the snippet after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public string information_snippetName; // Used to reconstruct snippetInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public string information_snippetTemplateName; // Used to reconstruct snippetInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public byte[] information_snippetIcon; // Used to reconstruct snippetInformation after loading as GameObject & Component cannot be saved properly in JSON
        [HideInInspector]
        public string information_snippetID; // Used to reconstruct snippetInformation after loading as GameObject & Component cannot be saved properly in JSON

        public Vector3 startPosition;

        /*
        
        Please create a new class that derives from TPI_SnippetSaveData for every TPI_Snippet deriving script you create.
        Then, you can define your own variables that will get saved and loaded.
        This could look similiar to this:

        public class TPI_SnippetFunctionTemplate_SaveData : TPI_SnippetSaveData {

            public string variable1;
            public int variable2;
            public bool variable3;
            public Vector3 variable4;

        }
        
        */
    }

}
