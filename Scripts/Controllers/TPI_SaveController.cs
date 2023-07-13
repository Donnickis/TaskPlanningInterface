using TaskPlanningInterface.Workflow;
using UnityEngine;
using System.IO;
using TaskPlanningInterface.Helper;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// This script acts as a data manager in order to save and load the required data.
    /// <br></br>Besides handling the Task Planning Interface internal variables, this system can also handle your personally created variables as long as they are not in breach of the supported types.
    /// <br></br>The supported types can be seen on this website (link is provided): <see href="https://docs.unity3d.com/2020.1/Documentation/Manual/JSONSerialization.html#:~:text=is%20not%20supported.-,Supported%20types,-The%20JSON%20Serializer">Supported Types</see>
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// <br></br>In order to add your own variables to the save files of the Snippets, Constraints and Workflows, please extend the SaveData classes of the respective type.
    /// </para>
    /// 
    /// <para>
    /// Source for JSON save system: <see href="https://videlais.com/2021/02/25/using-jsonutility-in-unity-to-save-and-load-game-data/">Click me</see>
    /// <br></br>Source for abstract class loading: <see href="https://answers.unity.com/questions/1300376/savingloading-inherited-class-scripts-c.html">Click me</see>
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>

    public class TPI_SaveController : MonoBehaviour {

        [Tooltip("This List contains all the workflows that have been saved in the past and were thus loaded at startup, and all the workflows that the operator has saved during the runtime of this session. You cannot make any alterations in this list as this should only provide you with the information whether everything has been loaded and saved properly. If you want to make any changes, please do so in the JSON files found at ...\\AppData\\LocalLow\\PDZ\\TaskPlanningInterface\\JSON\\")]
        public TPI_WorkflowSaveData workflowSaveData;

        private string savePath;
        private string workflowsSavePath;
        private string snippetSavePath;
        private string constraintSavePath;

        [Tooltip("Indicates if a workflow has been loaded or saved during this session, showing the ID of that workflow. If it is empty, a new one has been created or it has not been saved before.")][HideInInspector]
        public string activeWorkflowID = "";

        // Reference to TPI_MainController Component
        private TPI_MainController mainController;

        private void Awake() {

#if UNITY_WSA || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/TaskPlanningInterface/JSON";
#else
            savePath = Application.persistentDataPath + "/JSON";
#endif
            workflowsSavePath = savePath + "/workflows.json";
            snippetSavePath = savePath + "/Snippets";
            constraintSavePath = savePath + "/Constraints";
            LoadAllWorkflowsFromJSON();
        }

        private void Start() {
            mainController = GetComponent<TPI_MainController>();
        }

        /// <summary>
        /// Resets the SaveController variables
        /// </summary>
        public void ResetSaveController() {
            activeWorkflowID = "";
        }



        //---------------------------------------------------- Workflows ----------------------------------------------------//



        /// <summary>
        /// Loads and opens a specific workflow by providing the desired workflow ID.
        /// <para><paramref name="workflowID"/> = ID of the workflow that should be loaded</para>
        /// </summary>
        public void LoadWorkflow(string workflowID) {
            TPI_Workflow workflow = null;
            foreach (TPI_Workflow wf in workflowSaveData.workflows) {
                if (wf.workflowID == workflowID) {
                    workflow = wf;
                    break;
                }
            }
            if(workflow == null) {
                Debug.LogError("The workflow with the workflowID " + workflowID + " has not been found in the JSON save file. (LoadWorkflow in TPI_SaveController)");
                return;
            }
            mainController.currentWorkflow = workflow;
            mainController.sequenceMenu.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = workflow.workflowName + " Workflow"; // Set Workflow Name in Sequence Menu
            foreach (TPI_WorkflowIDAndType snippet in workflow.workflowSnippets) {
                LoadSnippet(snippet.GUID, snippet.saveDataClassType);
            }
            foreach (TPI_WorkflowIDAndType constraint in workflow.workflowConstraints) {
                LoadConstraint(constraint.GUID, constraint.saveDataClassType);
            }
            activeWorkflowID = workflowID;
        }

        /// <summary>
        /// Saves the currently active Workflow
        /// <para><paramref name="overwriteSave"/> = overwrites an existing save if set to true (if possible), otherwise creates a new save if set to false (generates a new workflowID)</para>
        /// </summary>
        public void SaveWorkflow(bool overwriteSave = true) { // overwriteSave = true -> overwrites an existing save if possible, overwriteSave = false -> creates a new save (Generates a new workflowID)

            if (mainController.currentWorkflow == null) {
                mainController.dialogMenuContainer.SetActive(true); // Dialog Menus
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "Please first create a workflow before you try to save.", "Confirm");
                return;
            }

            if (workflowSaveData == null)
                LoadAllWorkflowsFromJSON();

            if (overwriteSave) {

                OverwriteSave();

            } else { // Create a new save by setting a new name and workflowID and setting new IDs for all snippets and constraints

                GetComponent<TPI_DialogMenuController>().ShowObjectNameMenu("Create a new Workflow", "Please enter the desired name of the workflow:", "Create Workflow", GetComponent<TPI_MainController>().startButtonIcon,
                        "Workflow Name:", "The name of the workflow cannot be null. Please use the keyboard field to give the workflow a name.", CreateNewSave, true, true);

            }
            
        }

        /// <summary>
        /// Overwrites a currently existing workflow save
        /// </summary>
        private void OverwriteSave() {

            TPI_Workflow workflow;
            if (mainController.currentWorkflow != null) {
                workflow = GetComponent<TPI_MainController>().currentWorkflow;
                workflow.workflowSnippets.Clear();
                workflow.workflowConstraints.Clear();
            } else {
                workflow = new TPI_Workflow(activeWorkflowID); // activeWorkflowID can also be ""
                workflow.workflowName = mainController.currentWorkflow.workflowName;
            }

            foreach (TPI_Snippet snippet in mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetSnippetSequence()) {
                workflow.workflowSnippets.Add(new TPI_WorkflowIDAndType(snippet.snippetInformation.snippetID, snippet.saveData.GetType().ToString() + ", Assembly-CSharp"));
                SaveSnippet(snippet);
            }
            foreach (TPI_Constraint constraint in mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetConstraints()) {
                workflow.workflowConstraints.Add(new TPI_WorkflowIDAndType(constraint.constraintInformation.constraintID, constraint.saveData.GetType().ToString() + ", Assembly-CSharp"));
                SaveConstraint(constraint);
            }

            if (activeWorkflowID != "") { // delete unused snippets & constraints
                foreach (TPI_Workflow wf in workflowSaveData.workflows.ToList()) {
                    if (wf.workflowID == activeWorkflowID) {
                        foreach (TPI_WorkflowIDAndType sp in wf.workflowSnippets) { // Find Changes between previous save and current version for snippets
                            bool wasFound = false;
                            foreach (TPI_WorkflowIDAndType snippet in workflow.workflowSnippets) {
                                if (sp.GUID == snippet.GUID) {
                                    wasFound = true;
                                    break;
                                }
                            }
                            if (!wasFound)
                                DeleteSnippet(sp.GUID);
                        }
                        foreach (TPI_WorkflowIDAndType cs in wf.workflowConstraints) { // Find Changes between previous save and current version for constraints
                            bool wasFound = false;
                            foreach (TPI_WorkflowIDAndType constraint in workflow.workflowConstraints) {
                                if (cs.GUID == constraint.GUID) {
                                    wasFound = true;
                                    break;
                                }
                            }
                            if (!wasFound)
                                DeleteConstraint(cs.GUID);
                        }
                        workflowSaveData.workflows.Remove(wf); // Remove loaded workflow and afterwards add it again to "overwrite" it
                        break;
                    }
                }
            }

            activeWorkflowID = workflow.workflowID; // marks it for future saves
            mainController.currentWorkflow = workflow;
            workflowSaveData.workflows.Add(workflow);
            try { // Save data to JSON file
                SaveWorkflowsToJSON();
            } catch (Exception e) {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "An unexpected error occured when trying to save your data!a Please visit the Console in Unity in order to see a detailed description of the Error.", "Accept");
                Debug.LogException(e, this);
                return;
            }
            GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "Your workflow was successfully saved!", "Continue");

        }

        /// <summary>
        /// Receives the name from the ShowObjectNameMenu function and then creates an entirely new workflow save.
        /// <para><paramref name="selectedName"/> = name that was selected for the workflow</para>
        /// </summary>
        private void CreateNewSave(string selectedName) {

            TPI_Workflow workflow = new TPI_Workflow();
            workflow.workflowName = selectedName;

            // Assign new GUID to currently active snippets and constraints
            List<TPI_Constraint> specificConstraints = mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetSpecificConstraintList();
            foreach (TPI_Snippet snippet in mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetSnippetSequence()) {
                Guid snippetGuid = Guid.NewGuid();
                foreach (TPI_Constraint constraint in specificConstraints.ToList()) {
                    if (constraint.constraintInformation.snippetID == snippet.snippetInformation.snippetID) {
                        constraint.constraintInformation.snippetID = snippetGuid.ToString();
                        constraint.constraintInformation.constraintID = Guid.NewGuid().ToString();
                        specificConstraints.Remove(constraint);
                        workflow.workflowConstraints.Add(new TPI_WorkflowIDAndType(constraint.constraintInformation.constraintID, constraint.saveData.GetType().ToString() + ", Assembly-CSharp"));
                        SaveConstraint(constraint);
                    }
                }
                snippet.snippetInformation.snippetID = snippetGuid.ToString();
                workflow.workflowSnippets.Add(new TPI_WorkflowIDAndType(snippet.snippetInformation.snippetID, snippet.saveData.GetType().ToString() + ", Assembly-CSharp"));
                SaveSnippet(snippet);
            }
            if (specificConstraints.Count != 0) {
                Debug.LogWarning("The TaskPlanningInterface found snippet-specific constraints that did not belong to any snippets! IMPORTANT: They will be deleted as they serve no purpose! (SaveWorkflow in TPI_SaveController)");
                foreach (TPI_Constraint constraint in specificConstraints) {
                    DeleteConstraint(constraint.constraintInformation.constraintID);
                }
                specificConstraints.Clear(); // To avoid errors
            }
                
            for (int i = 0; i < mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetGlobalConstraintCount(); i++) {
                TPI_Constraint constraint = mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetGlobalConstraintAt(i);
                constraint.constraintInformation.constraintID = Guid.NewGuid().ToString();
                workflow.workflowConstraints.Add(new TPI_WorkflowIDAndType(constraint.constraintInformation.constraintID, constraint.saveData.GetType().ToString() + ", Assembly-CSharp"));
                SaveConstraint(constraint);
            }

            activeWorkflowID = workflow.workflowID; // marks it for future saves
            mainController.currentWorkflow = workflow;
            workflowSaveData.workflows.Add(workflow);
            try { // Save data to JSON file
                SaveWorkflowsToJSON();
            } catch (Exception e) {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "An unexpected error occured when trying to save your data!a Please visit the Console in Unity in order to see a detailed description of the Error.", "Accept");
                Debug.LogException(e, this);
                return;
            }
            GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "Your workflow was successfully saved!", "Continue");

        }

        /// <summary>
        /// Saves the provided workflow as a new workflow by assigning an new workflowID. -> will not be used much
        /// </summary>
        /// <returns>the the newly assigned workflowID (string)</returns>
        public string SaveWorkflow(TPI_Workflow workflow) {
            if (workflow == null) {
                Debug.LogError("The workflow cannot be null. (SaveWorkflow in TPI_SaveController)");
                return "ERROR: Workflow is null";
            }
            foreach (TPI_WorkflowIDAndType snippet in workflow.workflowSnippets) {
                SaveSnippet(snippet.GUID);
            }
            foreach (TPI_WorkflowIDAndType constraint in workflow.workflowConstraints) {
                SaveConstraint(constraint.GUID);
            }
            if (workflowSaveData == null)
                LoadAllWorkflowsFromJSON();

            System.Guid guid = System.Guid.NewGuid();
            workflow.workflowID = guid.ToString();

            workflowSaveData.workflows.Add(workflow);
            try { // Save data to JSON file
                SaveWorkflowsToJSON();
            } catch (Exception e) {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "An unexpected error occured when trying to save your data!a Please visit the Console in Unity in order to see a detailed description of the Error.", "Accept");
                Debug.LogException(e, this);
                return "ERROR: Was not able to save the workflow using SaveWorkflowsToJSON(); in TPI_SaveController in " + transform.name;
            }
            return guid.ToString();
        }

        /// <summary>
        /// Deletes a specific workflow when given the reference.
        /// </summary>
        public void DeleteWorkflow(TPI_Workflow workflow) {
            if (workflow == null) {
                Debug.LogError("The workflow cannot be null. (DeleteWorkflow in TPI_SaveController)");
                return;
            }
            if (!workflowSaveData.workflows.Contains(workflow)) {
                Debug.LogWarning("The workflow with the workflowID " + workflow.workflowID + " has not been found in the JSON save file. (DeleteWorkflow in TPI_SaveController)");
                return;
            }
            workflowSaveData.workflows.Remove(workflow);
            foreach (TPI_WorkflowIDAndType snippet in workflow.workflowSnippets) { // Delete unused snippets (after checking if the snippets are used in an other workflow)
                bool wasFound = false;
                foreach (TPI_Workflow wf in workflowSaveData.workflows.ToList()) {
                    foreach (TPI_WorkflowIDAndType sp in wf.workflowSnippets.ToList()) {
                        if (sp.GUID == snippet.GUID) {
                            wasFound = true;
                            break;
                        }
                    }
                    if (wasFound)
                        break;
                }
                if (!wasFound)
                    DeleteSnippet(snippet.GUID);
            }
            foreach (TPI_WorkflowIDAndType constraint in workflow.workflowConstraints) { // Delete unused constraints (after checking if the constraints are used in an other workflow)
                bool wasFound = false;
                foreach (TPI_Workflow wf in workflowSaveData.workflows.ToList()) {
                    foreach (TPI_WorkflowIDAndType cs in wf.workflowConstraints.ToList()) {
                        if (cs.GUID == constraint.GUID) {
                            wasFound = true;
                            break;
                        }
                    }
                    if (wasFound)
                        break;
                }
                if (!wasFound)
                    DeleteConstraint(constraint.GUID);
            }
            try { // Save data to JSON file
                SaveWorkflowsToJSON();
            } catch (Exception e) {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "An unexpected error occured when trying to save your data!a Please visit the Console in Unity in order to see a detailed description of the Error.", "Accept");
                Debug.LogException(e, this);
                return;
            }
        }

        /// <summary>
        /// Deletes a specific workflow when given the workflow ID.
        /// </summary>
        public void DeleteWorkflow(string workflowID) {
            TPI_Workflow workflow = null;
            foreach (TPI_Workflow wf in workflowSaveData.workflows) {
                if (workflowID == wf.workflowID) {
                    workflow = wf;
                    break;
                }
            }
            if (workflow == null) {
                Debug.LogError("The workflow with the workflowID " + workflowID + " was not found. (DeleteWorkflow in TPI_SaveController)");
                return;
            }
            workflowSaveData.workflows.Remove(workflow);
            foreach (TPI_WorkflowIDAndType snippet in workflow.workflowSnippets) { // Delete unused snippets (after checking if the snippets are used in an other workflow)
                bool wasFound = false;
                foreach (TPI_Workflow wf in workflowSaveData.workflows.ToList()) {
                    foreach (TPI_WorkflowIDAndType sp in wf.workflowSnippets.ToList()) {
                        if (sp.GUID == snippet.GUID) {
                            wasFound = true;
                            break;
                        }
                    }
                    if (wasFound)
                        break;
                }
                if (!wasFound)
                    DeleteSnippet(snippet.GUID);
            }
            foreach (TPI_WorkflowIDAndType constraint in workflow.workflowConstraints) { // Delete unused constraints (after checking if the constraints are used in an other workflow)
                bool wasFound = false;
                foreach (TPI_Workflow wf in workflowSaveData.workflows.ToList()) {
                    foreach (TPI_WorkflowIDAndType cs in wf.workflowConstraints.ToList()) {
                        if (cs.GUID == constraint.GUID) {
                            wasFound = true;
                            break;
                        }
                    }
                    if (wasFound)
                        break;
                }
                if (!wasFound)
                    DeleteConstraint(constraint.GUID);
            }
            try { // Save data to JSON file
                SaveWorkflowsToJSON();
            } catch {
                GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Save Controller", "An unexpected error occured when trying to save your data!", "Accept");
                return;
            }
        }

        /// <summary>
        /// Helper function that loads all workflow data when the program is started.
        /// </summary>
        private void LoadAllWorkflowsFromJSON() {
            if (File.Exists(workflowsSavePath)) {
                string fileContents = File.ReadAllText(workflowsSavePath);
                workflowSaveData = JsonUtility.FromJson<TPI_WorkflowSaveData>(fileContents);
            } else
                workflowSaveData = new TPI_WorkflowSaveData();
        }

        /// <summary>
        /// Helper function that saves all workflow data
        /// </summary>
        private void SaveWorkflowsToJSON() {
            if (workflowSaveData == null)
                workflowSaveData = new TPI_WorkflowSaveData();
            else
                File.Delete(workflowsSavePath);
            string jsonContents = JsonUtility.ToJson(workflowSaveData);
            if(!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            File.WriteAllText(workflowsSavePath, jsonContents);
        }



        //---------------------------------------------------- Snippets ----------------------------------------------------//



        /// <summary>
        /// Saves a snippet when given the reference.
        /// </summary>
        public void SaveSnippet(TPI_Snippet snippet) {
            if (snippet == null) {
                Debug.LogError("The snippet cannot be null. (SaveSnippet in TPI_SaveController)");
                return;
            }
            if (File.Exists(snippetSavePath + "/" + snippet.snippetInformation.snippetID + ".json"))
                File.Delete(snippetSavePath + "/" + snippet.snippetInformation.snippetID + ".json");
            snippet.SetupControllerReferences();
            snippet.UpdateSaveData();
            string jsonContents = JsonUtility.ToJson(snippet.saveData);
            if (!Directory.Exists(snippetSavePath))
                Directory.CreateDirectory(snippetSavePath);
            File.WriteAllText(snippetSavePath + "/" + snippet.snippetInformation.snippetID + ".json", jsonContents);
        }

        /// <summary>
        /// Saves a snippet when given the snippet ID.
        /// </summary>
        public void SaveSnippet(string snippetID) {
            TPI_Snippet snippet = null;
            for (int i = 0; i < mainController.snippetFunctionContainer.transform.childCount; i++) {
                if (mainController.snippetFunctionContainer.transform.GetChild(i).GetComponent<TPI_ObjectIdentifier>().GUID == snippetID) {
                    snippet = mainController.snippetFunctionContainer.transform.GetChild(i).GetComponent<TPI_Snippet>();
                    break;
                }
            }

            if (snippet == null) {
                Debug.LogError("The snippet with the snippetID " + snippetID + " was not found. (SaveSnippet in TPI_SaveController)");
                return;
            }
            if (File.Exists(snippetSavePath + "/" + snippet.snippetInformation.snippetID + ".json"))
                File.Delete(snippetSavePath + "/" + snippet.snippetInformation.snippetID + ".json");
            snippet.SetupControllerReferences();
            snippet.UpdateSaveData();
            string jsonContents = JsonUtility.ToJson(snippet.saveData);
            if (!Directory.Exists(snippetSavePath))
                Directory.CreateDirectory(snippetSavePath);
            File.WriteAllText(snippetSavePath + "/" + snippet.snippetInformation.snippetID + ".json", jsonContents);
        }

        /// <summary>
        /// Returns the snippet when given the snippet ID.
        /// </summary>
        public TPI_Snippet GetSnippet(string snippetID, string classType) {
            TPI_SnippetSaveData saveData = null;
            if (File.Exists(snippetSavePath + "/" + snippetID + ".json")) {
                string fileContents = File.ReadAllText(snippetSavePath + "/" + snippetID + ".json");
                saveData = (TPI_SnippetSaveData)JsonUtility.FromJson(fileContents, System.Type.GetType(classType));
            } else {
                Debug.LogError("The snippet with the snippetID " + snippetID + " was not found. (LoadSnippet in TPI_SaveController)");
                return null;
            }

            GameObject gameObjectToInstantiate = Resources.Load("TaskPlanningInterface/UseCases/" + saveData.saveManager_functionObjectName) as GameObject;
            if (gameObjectToInstantiate == null) {
                Debug.LogError("The snippet function gameobject prefab was not found in the Resources/TaskPlanningInterface/UseCases/ folder for the snippetID: " + snippetID + " and the classType: " + classType + ". (GetSnippet in TPI_SaveController)");
                return null;
            }

            GameObject functionObject = Instantiate(gameObjectToInstantiate, new Vector3(0,0,0), Quaternion.identity);
            functionObject.transform.parent = mainController.snippetFunctionContainer.transform;
            functionObject.name = "Snippet Function: " + saveData.information_snippetName;
            TPI_Snippet snippet = functionObject.GetComponent(typeof(TPI_Snippet)) as TPI_Snippet;

            snippet.saveData = saveData;
            snippet.snippetInformation = new TPI_SnippetInformation(saveData.information_snippetName, saveData.information_snippetTemplateName, "does no longer matter", "does no longer matter", ConvertBytesToTexture(saveData.information_snippetIcon), gameObjectToInstantiate, saveData.information_snippetID);

            return snippet;
        }

        /// <summary>
        /// Loades the snippet when given the snippet ID and adds it to the sequence list in the sequence menu.
        /// <para><paramref name="classType"/> = Type of the save data in order to reconstruct your component that derives from TPI_Snippet (<c>snippet.saveData.GetType().ToString() + ", Assembly-CSharp"</c>)</para>
        /// </summary>
        public void LoadSnippet(string snippetID, string classType) {
            TPI_Snippet snippet = GetSnippet(snippetID, classType);
            if(snippet != null)
                mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().AddSnippet(snippet);
        }

        /// <summary>
        /// Deletes a snippet when given the snippet ID.
        /// </summary>
        public void DeleteSnippet(string snippetID) {
            if (File.Exists(snippetSavePath + "/" + snippetID + ".json"))
                File.Delete(snippetSavePath + "/" + snippetID + ".json");
            else {
                Debug.LogError("The snippet with the snippetID " + snippetID + " was not found. (DeleteSnippet in TPI_SaveController)");
            }
        }



        //---------------------------------------------------- Constraints ----------------------------------------------------//



        /// <summary>
        /// Saves a constraint when given the reference
        /// </summary>
        public void SaveConstraint(TPI_Constraint constraint) {
            if (constraint == null) {
                Debug.LogError("The constraint cannot be null. (SaveConstraint in TPI_SaveController)");
                return;
            }
            if (File.Exists(constraintSavePath + "/" + constraint.constraintInformation.constraintID + ".json"))
                File.Delete(constraintSavePath + "/" + constraint.constraintInformation.constraintID + ".json");
            constraint.SetupControllerReferences();
            constraint.UpdateSaveData();
            string jsonContents = JsonUtility.ToJson(constraint.saveData);
            if (!Directory.Exists(constraintSavePath))
                Directory.CreateDirectory(constraintSavePath);
            File.WriteAllText(constraintSavePath + "/" + constraint.constraintInformation.constraintID + ".json", jsonContents);
        }

        /// <summary>
        /// Saves a constraint when given the constraint ID.
        /// </summary>
        public void SaveConstraint(string constraintID) {
            TPI_Constraint constraint = null;
            for (int i = 0; i < mainController.constraintFunctionContainer.transform.childCount; i++) {
                if (mainController.constraintFunctionContainer.transform.GetChild(i).GetComponent<TPI_ObjectIdentifier>().GUID == constraintID) {
                    constraint = mainController.constraintFunctionContainer.transform.GetChild(i).GetComponent<TPI_Constraint>();
                    break;
                }
            }

            if (constraint == null) {
                Debug.LogError("The constraint with the constraintID " + constraintID + " was not found. (SaveConstraint in TPI_SaveController)");
                return;
            }

            if (File.Exists(constraintSavePath + "/" + constraint.constraintInformation.constraintID + ".json"))
                File.Delete(constraintSavePath + "/" + constraint.constraintInformation.constraintID + ".json");
            constraint.SetupControllerReferences();
            constraint.UpdateSaveData();
            string jsonContents = JsonUtility.ToJson(constraint.saveData);
            if (!Directory.Exists(constraintSavePath))
                Directory.CreateDirectory(constraintSavePath);
            File.WriteAllText(constraintSavePath + "/" + constraint.constraintInformation.constraintID + ".json", jsonContents);

        }

        /// <summary>
        /// Returns the constraint when given the constraint ID.
        /// </summary>
        public TPI_Constraint GetConstraint(string constraintID, string classType) {
            TPI_ConstraintSaveData saveData = null;
            if (File.Exists(constraintSavePath + "/" + constraintID + ".json")) {
                string fileContents = File.ReadAllText(constraintSavePath + "/" + constraintID + ".json");
                saveData = (TPI_ConstraintSaveData)JsonUtility.FromJson(fileContents, System.Type.GetType(classType));
            } else {
                Debug.LogError("The constraint with the constraintID " + constraintID + " was not found. (LoadConstraint in TPI_SaveController)");
                return null;
            }

            GameObject gameObjectToInstantiate = Resources.Load("TaskPlanningInterface/UseCases/" + saveData.saveManager_functionObjectName) as GameObject;
            if (gameObjectToInstantiate == null) {
                Debug.LogError("The constraint function gameobject prefab was not found in the Resources/TaskPlanningInterface/UseCases/ folder for the constraintID: " + constraintID + " and the classType: " + classType + ". (GetConstraint in TPI_SaveController)");
                return null;
            }

            GameObject functionObject = Instantiate(gameObjectToInstantiate, new Vector3(0, 0, 0), Quaternion.identity);
            functionObject.transform.parent = mainController.constraintFunctionContainer.transform;
            if (saveData.information_constraintType == TPI_ConstraintType.global)
                functionObject.name = "Global Constraint Function: " + saveData.information_constraintName;
            else
                functionObject.name = "Specific Constraint Function: " + saveData.information_constraintName;
            TPI_Constraint constraint = functionObject.GetComponent(typeof(TPI_Constraint)) as TPI_Constraint;

            constraint.saveData = saveData;
            constraint.constraintInformation = new TPI_ConstraintInformation(saveData.information_constraintName, saveData.information_constraintTemplateName, saveData.information_constraintType, "does no longer matter", ConvertBytesToTexture(saveData.information_constraintIcon), gameObjectToInstantiate, saveData.information_snippetID, saveData.information_constraintID);

            return constraint;
        }

        /// <summary>
        /// Loades the constraint when given the snippet ID and adds it to the constraint list in the sequence menu.
        /// <para><paramref name="classType"/> = Type of the save data in order to reconstruct your component that derives from TPI_Constraint (<c>constraint.saveData.GetType().ToString() + ", Assembly-CSharp"</c>)</para>
        /// </summary>
        public void LoadConstraint(string constraintID, string classType) {
            TPI_Constraint constraint = GetConstraint(constraintID, classType);
            if (constraint != null)
                mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().AddConstraint(constraint);
        }

        /// <summary>
        /// Deletes a constraint when given the constraint ID.
        /// </summary>
        public void DeleteConstraint(string constraintID) {
            if (File.Exists(constraintSavePath + "/" + constraintID + ".json"))
                File.Delete(constraintSavePath + "/" + constraintID + ".json");
            else {
                Debug.LogError("The constraint with the constraintID " + constraintID + " was not found. (DeleteConstraint in TPI_SaveController)");
            }
        }

        //---------------------------------------------------- Special Functions ----------------------------------------------------//

        /// <summary>
        /// Use this function to convert your Texture2D png file into an array of bytes (bytes[]).
        /// <br></br>This thus can be used to save your textures in your save data file.
        /// </summary>
        /// <returns>Converted array of bytes (byte[])</returns>
        public byte[] ConvertTextureToBytes(Texture2D texture) {

            if (texture == null)
                return null;
            return texture.EncodeToPNG();

        }

        /// <summary>
        /// Use this function to convert an array of bytes (bytes[]) into a Texture2D.
        /// <br></br>This thus can be used to load your textures from your save data file.
        /// </summary>
        /// <returns>Converted Texture2D</returns>
        public Texture2D ConvertBytesToTexture(byte[] bytes) {
            
            if(bytes == null || bytes.Length == 0)
                return null;
            Texture2D icon = new Texture2D(1, 1);
            icon.LoadImage(bytes);
            return icon;

        }

    }

}