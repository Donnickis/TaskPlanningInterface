using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using TaskPlanningInterface.Workflow;
using TaskPlanningInterface.Helper;
using UnityEngine;
using System.Collections;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// This script handles the "Selection Menu" and the "Building Blocks Menu".
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// <br></br> On the other hand, if you want to allow the creation of new snippets at runtime (no template setup beforehand in Unity), you can uncomment those parts in the code.
    /// <br></br> However, the Task Planning Interface does not have an implementation of the functionality of this feature, which means that you have to code the logic behind it yourself (The TPI only provides the graphical interface).
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public class TPI_WorkflowConfigurationController : MonoBehaviour {

        // Selection Menu 
        [Min(0)][Tooltip("How many Categories per row should be shown in the Selection Menu?")]
        [SerializeField] private int buttonsPerRow;
        [Tooltip("Add, alter or remove Categories by changing the information of contained them in this list")]
        public List<TPI_CategoryInformation> categories;
        [Tooltip("Add the prefab of a Category Button (whatever should be instantiated in the Selection Menu)")]
        [SerializeField] private GameObject categoryButtonPrefab;
        [Tooltip("Add the prefab of a Category Container, in which the objects belonging to that category will be placed (whatever should be instantiated in the background -> not visible)")]
        [SerializeField] private GameObject categoryContainerPrefab;

        [Tooltip("Choose what Icon should be shown next to the 'Constraints' Category in the Selection Menu")]
        [SerializeField] private Texture2D categoryIcon;

        // Add New Snippet during runtime -> Currently disabled -> still part of the code as it can be easily turned back on
        /*[Tooltip("What should the 'Add new Snippet at runtime' Button in the Selection Menu be called?")]
        public string newSnippetButtonName;
        [Tooltip("Choose what Icon should be shown next to the 'Add new Snippet at runtime' Button in the Selection Menu")]
        public Texture2D newSnippetButtonIcon;*/


        // Building Blocks Menu
        [Min(0)][Tooltip("How many objects per row should be shown in the Building Block Menu?")]
        [SerializeField] private int objectsPerRow;
        private Transform buildingBlockMenuContainer;

        // Snippets in Building Blocks Menu
        [Tooltip("Add, alter or remove Snippets by changing the information of contained them in this list.")]
        public List<TPI_SnippetInformation> snippetTemplates;
        [Tooltip("Add the prefab of a Snippet Template Button (whatever should be instantiated in the correct Category of the Building Blocks Menu)")]
        [SerializeField] private GameObject snippetButtonPrefab;
        [Tooltip("Should the Snippet Template name be shown (e.g. MoveTo)?")]
        public bool showTemplateName_snippet;
        [Tooltip("How many characters of the Template Name should be shown? To show the full name, set it to -1.")][HideInInspector][Min(-1)]
        public int charactersShowed_snippet = 4;

        // Constraints in Building Blocks Menu
        [Tooltip("Add, alter or remove Constraints by changing the information of contained them in this list")]
        public List<TPI_ConstraintInformation> constraintTemplates;
        [Tooltip("Add the prefab of a Constraint Template Button (whatever should be instantiated in the correct Category of the Building Blocks Menu)")]
        [SerializeField] private GameObject constraintButtonPrefab;
        [Tooltip("Should the Constraint Template name be shown (e.g. NoGoZone)?")]
        public bool showTemplateName_constraint;
        [Tooltip("How many characters of the Template Name should be shown? To show the full name, set it to -1.")][HideInInspector][Min(-1)]
        public int charactersShowed_constraint = 4;


        private List<int> buildingBlockRows; // stores how many rows the building blocks menu has to show for each category (index = index of category in categories list)
        private string lastCategoryIndex = ""; // "" signals that the building block menu is currently closed

        // Reference to TPI_MainController Component
        private TPI_MainController mainController;

        private GameObject currentTooltip = null;

        private void Start() {

            mainController = GetComponent<TPI_MainController>();

            // Check that the prefabs have been setup correctly
            if (categoryButtonPrefab == null) // Custom Assert message
                Debug.LogError("No category button prefab was assigned in the ButtonController component in " + transform.name);
            if (categoryContainerPrefab == null)
                Debug.LogError("No category container prefab was assigned in the ButtonController component in " + transform.name);
            if (snippetButtonPrefab == null)
                Debug.LogError("No button prefab was assigned in the ButtonController component in " + transform.name);
            if (constraintButtonPrefab == null)
                Debug.LogError("No constraint prefab was assigned in the ButtonController component in " + transform.name);
            /*if (newSnippetButtonName == "")
                newSnippetButtonName = "Add new Snippet at runtime";*/

            buildingBlockMenuContainer = mainController.buildingBlocksMenu.transform.GetChild(2);
            buildingBlockRows = new List<int>();

            InstantiateCategories();
            InstantiateButtons();

        }

        private void Update() {
            
            // Place Description Tooltip next to the Finger / Pointer
            if(currentTooltip != null) {
                foreach (IMixedRealityPointer pointer in currentTooltip.GetComponentInParent<Interactable>().FocusingPointers) {
                    if (pointer == null || !pointer.IsActive || !pointer.IsInteractionEnabled || pointer.Result == null)
                        continue;

                    currentTooltip.transform.GetChild(5).GetChild(0).position = new Vector3(pointer.Result.Details.Point.x + 0.0825f, pointer.Result.Details.Point.y - 0.005f, pointer.Result.Details.Point.z);
                    break;
                }
            }
        }

        /// <summary>
        /// This IEnumerator fixes a problem with the UpdateCollection function of the GridObjectCollection script, moving the update to the next frame.
        /// <para><paramref name="gridObjectCollection"/> = Grid Object Collection that needs to be updated</para>
        /// </summary>
        private IEnumerator InvokeUpdateCollection(GridObjectCollection gridObjectCollection) {
            yield return null;
            gridObjectCollection.UpdateCollection();
        }

        /// <summary>
        /// This function is used to create the category buttons, configure the category near menu and create the empty category GameObjects.
        /// </summary>
        private void InstantiateCategories() {

            categories.Add(new TPI_CategoryInformation("Constraints", "A constraint is a restriction placed on a robotic system that narrows its achievable motion and function possibilities." +
                "Constraints can include both the mechanical constraints of the system, such as a maximal weight, and the constraints that are applied by the user, such as the desired maximal time.", categoryIcon));
            //categories.Add(new TPI_CategoryButton(newSnippetButtonName, newSnippetButtonIcon)); -> Currently disabled -> still part of the code as it can be easily turned back on

            int rows = (int)Math.Ceiling((double)categories.Count/ buttonsPerRow);
            Transform selectionMenu = mainController.selectionMenu.transform;

            foreach (var category in categories) {

                // Setup of the category buttons in the selection menu
                if (category.categoryID == "") {
                    Guid guid = Guid.NewGuid();
                    category.categoryID = guid.ToString();
                }
                GameObject categoryButton = Instantiate(categoryButtonPrefab, selectionMenu.position, selectionMenu.rotation);
                categoryButton.transform.localScale = selectionMenu.localScale;
                categoryButton.transform.parent = selectionMenu.GetChild(2);
                categoryButton.name = category.categoryName + " Button";
                categoryButton.GetComponent<ButtonConfigHelper>().MainLabelText = category.categoryName;
                categoryButton.AddComponent<TPI_ObjectIdentifier>().GUID = category.categoryID;
                if (category.categoryIcon != null) //otherwise use the standard icon
                    categoryButton.GetComponent<ButtonConfigHelper>().SetQuadIcon(category.categoryIcon);

                // Tooltip Description
                TextMeshProUGUI[] texts = categoryButton.GetComponentsInChildren<TextMeshProUGUI>();
                for (int i = 0; i < texts.Length; i++) {
                    if (texts[i].text.Contains("[TooltipDescription]", StringComparison.OrdinalIgnoreCase)) {
                        if (category.categoryDescription != "") {
                            texts[i].text = category.categoryDescription;
                            InteractableOnFocusReceiver interactableOnFocusReceiver = categoryButton.GetComponent<Interactable>().AddReceiver<InteractableOnFocusReceiver>();
                            interactableOnFocusReceiver.OnFocusOn.AddListener(delegate { currentTooltip = categoryButton; });
                            interactableOnFocusReceiver.OnFocusOff.AddListener(delegate { currentTooltip = null; });
                            texts[i].transform.parent.parent.parent.gameObject.SetActive(false);
                        } else {
                            texts[i].transform.parent.gameObject.SetActive(false);
                        }
                    }
                }

                if (category.categoryName != "Add new Snippet at runtime") { // Category Button Settings
                    
                    categoryButton.GetComponent<ButtonConfigHelper>().OnClick.AddListener(delegate { OpenCategory(category.categoryID); });

                    // Setup of the the empty category GameObjects
                    GameObject categoryObject = Instantiate(categoryContainerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    categoryObject.transform.parent = buildingBlockMenuContainer;
                    categoryObject.name = category.categoryName + " Category";
                    categoryObject.GetComponent<GridObjectCollection>().Columns = objectsPerRow;
                    categoryObject.SetActive(false);
                    categoryObject.transform.localPosition = new Vector3(0.0356f, -0.004f, -1.0104f);
                    categoryObject.AddComponent<TPI_ObjectIdentifier>().GUID = category.categoryID;


                } else // "Add new Snippet at runtime" OnClick Event -> // Currently disabled -> still part of the code as it can be easily turned back on
                    categoryButton.GetComponent<ButtonConfigHelper>().OnClick.AddListener(delegate { OpenNewWorkflow(); });
            }


            // Configure the category near menu
            Transform quadObject = selectionMenu.GetChild(1).GetChild(0);
            quadObject.localScale = new Vector3(0.032f * (buttonsPerRow + 1), 0.032f * (rows + 1), 0.0106f);
            selectionMenu.GetChild(0).position = new Vector3(quadObject.position.x + quadObject.GetComponent<MeshRenderer>().bounds.size.x/2 + selectionMenu.GetChild(0).localScale.x * 0.032f / 2 + 0.002f,
                quadObject.position.y + quadObject.GetComponent<MeshRenderer>().bounds.size.y/2 -selectionMenu.GetChild(0).localScale.y * 0.032f / 2,
                quadObject.position.z - 0.008f);

            selectionMenu.GetComponentInChildren<GridObjectCollection>().Columns = buttonsPerRow;

            StartCoroutine(InvokeUpdateCollection(selectionMenu.GetComponentInChildren<GridObjectCollection>()));

        }

        /// <summary>
        /// This helper function gets called if the operator presses a category button in the selection menu, starting and configuring the building blocks near menu.
        /// <para><paramref name="categoryID"/> = ID of the category that should be opened</para>
        /// </summary>
        private void OpenCategory(string categoryID) {

            for(int i = 0; i < buildingBlockMenuContainer.childCount; i++) {

                if (buildingBlockMenuContainer.GetChild(i).GetComponent<TPI_ObjectIdentifier>().GUID == categoryID) {

                    buildingBlockMenuContainer.GetChild(i).gameObject.SetActive(true);
                    Transform quadObject = mainController.buildingBlocksMenu.transform.GetChild(1).GetChild(0);
                    quadObject.localScale = new Vector3(0.032f * (buttonsPerRow + 1), 0.032f * (buildingBlockRows[i] + 1), 0.0106f);
                    mainController.buildingBlocksMenu.transform.GetChild(0).position = new Vector3(quadObject.position.x + quadObject.GetComponent<MeshRenderer>().bounds.size.x / 2 + mainController.buildingBlocksMenu.transform.GetChild(0).localScale.x * 0.032f / 2 + 0.002f,
                        quadObject.position.y + quadObject.GetComponent<MeshRenderer>().bounds.size.y / 2 - mainController.buildingBlocksMenu.transform.GetChild(0).localScale.y * 0.032f / 2,
                        quadObject.position.z - 0.008f);

                    TPI_ObjectPlacementController placementController = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>();
                    if (lastCategoryIndex == "") {
                        lastCategoryIndex = categoryID;
                        mainController.buildingBlocksMenu.gameObject.SetActive(true);
                        placementController.ReserveSpot(placementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), mainController.buildingBlocksMenu, forceOverride: true, applyPose: true); // Building Blocks Menu placed in the Middle
                    } else {
                        if (lastCategoryIndex == categoryID) {
                            placementController.FreeUpSpot(mainController.buildingBlocksMenu);
                            mainController.buildingBlocksMenu.gameObject.SetActive(false);
                            lastCategoryIndex = "";
                        } else
                            lastCategoryIndex = categoryID;
                    }

                } else
                    buildingBlockMenuContainer.GetChild(i).gameObject.SetActive(false);
            }

        }

        /// <summary>
        /// Helper Function that returns whether the building blocks menu is currently open.
        /// </summary>
        /// <returns>bool - status whether building blocks menu is open</returns>
        public bool IsBuildingBlocksMenuOpen() {
            return lastCategoryIndex != "";
        }

        /// <summary>
        /// Helper Function that that closes the building blocks menu.
        /// </summary>
        public void CloseBuildingBlocksMenu() {
            lastCategoryIndex = "";
            mainController.buildingBlocksMenu.gameObject.SetActive(false);
        }

        /// <summary>
        /// Helper Function that gets called if the operator presses the "Add new Snippet at runtime" button.
        /// <br></br>The Task Planning Interface does not have an implementation of the functionality of this feature, which means that you have to code the logic behind it yourself (The TPI only provides the graphical interface).
        /// <br></br>You could for example use the DialogMenuController to open up a custom dialog menu or call a specific function of yours.
        /// </summary>
        private void OpenNewWorkflow() {
            // Setup What Happens if the "Add new Snippet at runtime" is pressed -> currently disabled
        }

        /// <summary>
        /// This function is used to create the building block buttons for the snippets and constraints.
        /// </summary>
        private void InstantiateButtons() {

            Transform selectionMenu = mainController.selectionMenu.transform;
            // Instanitate the snippet templates in the building blocks menu
            foreach (var snippet in snippetTemplates) {

                // Look for correct category container
                Transform category = null;
                for (int i = 0; i < buildingBlockMenuContainer.childCount; i++) {
                    if (buildingBlockMenuContainer.GetChild(i).GetComponent<TPI_ObjectIdentifier>().GUID == snippet.snippetCategoryID) {
                        category = buildingBlockMenuContainer.GetChild(i);
                        break;
                    } else
                        continue;
                }

                if (category == null) {
                    Debug.LogError("The category with the ID " + snippet.snippetCategoryID + " was not found. (InstantiateButtons in TPI_WorkflowConfigurationController)");
                    continue;
                }

                // Instantiate the template button
                GameObject buttonObject = Instantiate(snippetButtonPrefab, selectionMenu.position, selectionMenu.rotation);
                buttonObject.transform.localScale = selectionMenu.localScale;
                buttonObject.transform.parent = category;
                buttonObject.name = snippet.snippetName + " Button";
                buttonObject.GetComponent<ButtonConfigHelper>().MainLabelText = snippet.snippetName;
                buttonObject.GetComponent<ButtonConfigHelper>().OnClick.AddListener(delegate { SnippetButtonPressed(snippet); });
                if (snippet.snippetIcon != null) //otherwise use the standard icon
                    buttonObject.GetComponent<ButtonConfigHelper>().SetQuadIcon(snippet.snippetIcon);

                // Tooltip Description
                TextMeshProUGUI[] texts = buttonObject.GetComponentsInChildren<TextMeshProUGUI>();
                for (int i = 0; i < texts.Length; i++) {
                    if (texts[i].text.Contains("[TooltipDescription]", StringComparison.OrdinalIgnoreCase)) {
                        if (snippet.snippetDescription != "") {
                            texts[i].text = snippet.snippetDescription;
                            texts[i].transform.parent.parent.parent.gameObject.SetActive(false);
                        } else {
                            texts[i].transform.parent.gameObject.SetActive(false);
                        }
                    }
                }

            }

            // Instanitate the constraint templates in the building blocks menu
            foreach (var constraint in constraintTemplates) {

                // Look for the correct category
                Transform category = null;
                for(int i = 0; i < buildingBlockMenuContainer.childCount; i++) {
                    if (buildingBlockMenuContainer.GetChild(i).name == "Constraints Category")
                        category = buildingBlockMenuContainer.GetChild(i);
                }

                // Instantiate the template button
                GameObject buttonObject = Instantiate(constraintButtonPrefab, selectionMenu.position, selectionMenu.rotation);
                buttonObject.transform.localScale = selectionMenu.localScale;
                buttonObject.transform.parent = category;
                buttonObject.name = constraint.constraintName + " Button";
                buttonObject.GetComponent<ButtonConfigHelper>().MainLabelText = constraint.constraintName;
                buttonObject.GetComponent<ButtonConfigHelper>().OnClick.AddListener(delegate { ConstraintButtonPressed(constraint); });
                if (constraint.constraintIcon != null) //otherwise use the standard icon
                    buttonObject.GetComponent<ButtonConfigHelper>().SetQuadIcon(constraint.constraintIcon);

                // Tooltip Description
                TextMeshProUGUI[] texts = buttonObject.GetComponentsInChildren<TextMeshProUGUI>();
                for (int i = 0; i < texts.Length; i++) {
                    if (texts[i].text.Contains("[TooltipDescription]", StringComparison.OrdinalIgnoreCase)) {
                        if (constraint.constraintDescription != "") {
                            texts[i].text = constraint.constraintDescription;
                            texts[i].transform.parent.parent.parent.gameObject.SetActive(false);
                        } else {
                            texts[i].transform.parent.gameObject.SetActive(false);
                        }
                    }
                }

            }

            // Update GridObjectCollection and collect information about rows
            for (int i = 0; i < buildingBlockMenuContainer.childCount; ++i) {
                Transform containerChild = buildingBlockMenuContainer.GetChild(i);
                int rows = 0;
                if (containerChild.name == "Constraints Category")
                    rows = (int)Math.Ceiling((double)constraintTemplates.Count / objectsPerRow);
                else
                    rows = (int)Math.Ceiling((double)snippetTemplates.Count / objectsPerRow);

                buildingBlockRows.Insert(i, rows);
                StartCoroutine(InvokeUpdateCollection(containerChild.GetComponent<GridObjectCollection>()));
            }

        }

        /// <summary>
        /// This helper function gets called if the operator presses a snippet button in the building blocks menu menu, setting up the snippet function for the SequenceMenu.
        /// <para><paramref name="snippet"/> = Information of the snippet that was selected</para>
        /// </summary>
        private void SnippetButtonPressed(TPI_SnippetInformation snippet) {

            if (snippet.functionObject == null) {
                Debug.LogError("There is no GameObject attached to the snippet with the name " + snippet.snippetName + " (SnippetButtonPressed in TPI_WorkflowConfigurationController)");
                return;
            }

            // Instantiate Snippet Function GameObject and configure it
            GameObject snippetObject = Instantiate(snippet.functionObject, new Vector3(0, 0, 0), Quaternion.identity);
            snippetObject.transform.parent = mainController.snippetFunctionContainer.transform;
            snippetObject.name = "Snippet Function: " + snippet.snippetName;

            TPI_Snippet snippetScript = snippetObject.GetComponent(typeof(TPI_Snippet)) as TPI_Snippet;
            if (snippetScript == null) {
                Debug.LogError("The GameObject attached to the snippet with the ID " + snippet.snippetID + " does not contain a script that inherits from TPI_Snippet. (SnippetButtonPressed in TPI_WorkflowConfigurationController)");
                return;
            }

            Guid guid = Guid.NewGuid();
            TPI_SnippetInformation information = new TPI_SnippetInformation("", snippet.snippetName, snippet.snippetCategoryID, snippet.snippetDescription, snippet.snippetIcon, snippet.functionObject, guid.ToString()); // To make sure that they are different instances of the script
            snippetObject.AddComponent<TPI_ObjectIdentifier>().GUID = guid.ToString();

            snippetScript.snippetInformation = information;
            snippetScript.SetupControllerReferences();
            snippetScript.UpdateSaveData();

            snippetScript.ButtonPressed();

        }

        /// <summary>
        /// This helper function gets called if the operator presses a constraint button in the building blocks menu menu, setting up the constraint function for the SequenceMenu.
        /// <para><paramref name="constraint"/> = Information of the constraint that was selected</para>
        /// </summary>
        private void ConstraintButtonPressed(TPI_ConstraintInformation constraint) { 

            if (constraint.functionObject == null) {
                Debug.LogError("There is no GameObject attached to the constraint with the name " + constraint.constraintName + " (ConstraintButtonPressed in TPI_WorkflowConfigurationController)");
                return;
            }

            // Instantiate Constraint Function GameObject and configure it
            GameObject constraintObject = Instantiate(constraint.functionObject, new Vector3(0, 0, 0), Quaternion.identity);
            constraintObject.transform.parent = mainController.constraintFunctionContainer.transform;
            if(constraint.constraintType == TPI_ConstraintType.global)
                constraintObject.name = "Global Constraint Function: " + constraint.constraintName;
            else
                constraintObject.name = "Specific Constraint Function: " + constraint.constraintName;

            TPI_Constraint constraintScript = constraintObject.GetComponent(typeof(TPI_Constraint)) as TPI_Constraint;
            if (constraintScript == null) {
                Debug.LogError("The GameObject attached to the constraint with the ID " + constraint.constraintID + " does not contain a script that inherits from TPI_Constraint. (ConstraintButtonPressed in TPI_WorkflowConfigurationController)");
                return;
            }

            Guid guid = Guid.NewGuid();
            TPI_ConstraintInformation information = new TPI_ConstraintInformation("", constraint.constraintName, constraint.constraintType, constraint.constraintDescription, constraint.constraintIcon, constraint.functionObject, constraint.snippetID, guid.ToString()); // To make sure that they are different instances of the script

            constraintObject.AddComponent<TPI_ObjectIdentifier>().GUID = guid.ToString();

            constraintScript.constraintInformation = information;
            constraintScript.SetupControllerReferences();
            constraintScript.UpdateSaveData();

            constraintScript.ButtonPressed();

        }

    }

}
