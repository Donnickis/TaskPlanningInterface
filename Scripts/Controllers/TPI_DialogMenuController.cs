using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using TaskPlanningInterface.DialogMenu;
using TaskPlanningInterface.EditorAndInspector;
using TaskPlanningInterface.Helper;
using TaskPlanningInterface.Workflow;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// The aim of this script is to assist you in creating "Dialog Menus" to your liking. You can choose from multiple pre-made templates (prefabs) or create your own ones, which will automatically be populated correctly for you.
    /// <br></br>For a more detailed view on how to create those dialog menus, please visit the 'Tutorial' section in this class (TPI_DialogMenuController).
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// <br></br>On the other hand, if you want to add additional dialog menu options, this can be achieved with the help of this class.
    /// <br></br>In order to add more, you need to add a section to the <c>InstantiateDialogMenu</c> function.
    /// <br></br>Furthermore, depending on whether the choices made should be passed along to a different function, you also need to extend the <c>GetDialogMenuChoices</c> function.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public class TPI_DialogMenuController : MonoBehaviour {

        [Tooltip("Add, alter or remove Dialog Menu Templates by changing the information of them contained in this list")]
        public List<TPI_DialogMenuInformation> dialogMenuPrefabList;
        private List<TPI_DialogMenuInformation> spawnedDialogMenus;

        [Tooltip("Object instantiated if the operator wants to select a point in the environment.")]
        [SerializeField] private GameObject pointSelectionSphere;

        // Error / Alert Dialog Menu Options
        [Tooltip("Add the Prefab of the Error / Alert Dialog Menu that points out an error to the operator or alerts the operator to a specific fact")]
        [SerializeField] private GameObject errorMenuPrefab;
        [Tooltip("Add the Prefab of the Error / Alert Dialog Menu with two buttons that points out an error to the operator or alerts the operator to a specific fact")]
        [SerializeField] private GameObject errorMenuPrefab_twoButtons;
        [Tooltip("Choose what accept Icon should be shown next to the Button in the Error / Alert Dialog Menu")]
        [SerializeField] private Texture2D errorAcceptButtonIcon;
        [Tooltip("Choose what decline Icon should be shown next to the Button in the Error / Alert Dialog Menu")]
        [SerializeField] private Texture2D errorDeclineButtonIcon;

        // Object Name Menu Options
        [Tooltip("Add the Prefab of the Object Name Menu that that allows the operator to choose a name for an object.")]
        [SerializeField] private GameObject objectNameMenuPrefab;
        [Tooltip("Choose what Icon should be shown next to the Button in the Object Name Menu")]
        [SerializeField] private Texture2D objectNameButtonIcon;

        // Constraint Type Selection Menu Options
        [Tooltip("Add the Prefab of the Constraint Type Selection Dialog Menu that allows the operator to choose whether a Constraint is globally active or snippet-specific.")]
        [SerializeField] private GameObject constraintTypeMenuPrefab;
        [Tooltip("Choose what Icon should be shown next to the globally active Constraint Button in the Constraint Type Selection Dialog Menu")]
        [SerializeField] private Texture2D constraintTypeButtonIcon_global;
        [Tooltip("Choose what Icon should be shown next to the snippet-specific Constraint Button in the Constraint Type Selection Dialog Menu")]
        [SerializeField] private Texture2D constraintTypeButtonIcon_specific;

        // Dropdown Selection Menu Options
        [Tooltip("Add the Prefab of the Dropdown Selection Menu that that allows the operator to choose an item from a dropdown list.")]
        [SerializeField] private GameObject dropdownSelectionMenuPrefab;
        [Tooltip("Choose what Icon should be shown next to the Button in the Object Name Menu")]
        [SerializeField] private Texture2D dropdownSelectionButtonIcon;

        // Reference to TPI_MainController Component
        private TPI_MainController mainController;

        private void Start() {

            mainController = GetComponent<TPI_MainController>();

            // Check that the prefabs have been setup correctly
            if (errorMenuPrefab == null)
                Debug.LogError("No prefab for the error dialog menu was assigned in the DialogMenuController component in " + transform.name);
            if (objectNameMenuPrefab == null)
                Debug.LogError("No prefab for the object name dialog menu was assigned in the DialogMenuController component in " + transform.name);
            if (constraintTypeMenuPrefab == null)
                Debug.LogError("No prefab for the constraint type dialog menu was assigned in the DialogMenuController component in " + transform.name);
            if (dropdownSelectionMenuPrefab == null)
                Debug.LogError("No prefab for the dropdown selection dialog menu was assigned in the DialogMenuController component in " + transform.name);

            if (dialogMenuPrefabList == null)
                dialogMenuPrefabList = new List<TPI_DialogMenuInformation>();
            spawnedDialogMenus = new List<TPI_DialogMenuInformation>();

        }



        //---------------------------------------------------- Tutorial ----------------------------------------------------//
        /*


        The ’TPI_DialogMenuController’ helps you to easily create complex and good-looking ’Dialog Menus’ to your liking. All you need to create them is a reference to the ’TPI_DialogMenuController’
        component located on the ’TPI_Manager’ GameObject. You can for example achieve this by using the following code sample:

        TPI_DialogMenuController dialogMenuController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>();



        A)  Creation and Spawning of a Dialog Menu:
    -------------------------------------


            1) "Inspector + using the functions given in this script"

                The easiest way to create a dialog menu is by creating and filling out a ’Dialog Menu Template’ in the ’TPI_DialogMenuController’ component of the ’TPI_Manager’ GameObject in the Unity inspector.
                This allows you to easily decide with the help of a graphical interface how your dialog menu should be populated.

                Once you have set up the template via the Unity inspector, you can simply call either the ’SpawnDialogMenu(string dialogMenuID)’ function or the
                ’SpawnDialogMenu(int dialogMenuIndex)’ function at the right place in your code in order to create the dialog menu.
                The ’dialogMenuID’ or ’dialogMenuIndex’ can be found in the ’Dialog Menu Template’ list, by either determining the index of your template entry
                (starting at 0) or by simply copying the content of the ’Dialog Menu ID’ field for the ’dialogMenuID’ (right-click + Copy).


            2) "Coding all the way"
            
                If you do not want to use the Unity inspector, you have to create a new ’TPI_DialogMenuInformation’ instance and assign every value yourself.
                Furthermore, you then have to use the function ’SpawnDialogMenu(TPI_DialogMenuInformation dialogMenuInformation)’ to create your menu.

                An example of how to create such a ’TPI_DialogMenuInformation’ instance, showing all the possible options that you are able to utilize, can be seen in the following code sample:
            
                ////////////////////////////////////////////// CODE STARTS HERE //////////////////////////////////////////////
                
                GameObject menuPrefab; // Setup in the inspector or assigned otherwise
                Texture2D buttonIcon; // OPTIONAL: Setup in the inspector or assigned otherwise
                
                // Setup general information
                TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
                dialogMenuInfo.dialogMenuName = "Put the menu name here. It will replace the field [Name]";
                dialogMenuInfo.dialogMenuPrefab = menuPrefab;
                dialogMenuInfo.showCloseMenuButton = true; // Decide if there should be a "Close Menu" Button in the title bar
                
                // Add text fields
                dialogMenuInfo.dialogMenuTexts.Add("Put the first text here. It will replace the field [Textfield0]");
                dialogMenuInfo.dialogMenuTexts.Add("Put the second text here. It will replace the field [Textfield1]");

                // Add a button
                TPI_DialogMenuButton button1 = new TPI_DialogMenuButton();
                button1.buttonText = "Put the button name here. It will replace the field [Button0]";
                button1.buttonIcon = buttonIcon; // OPTIONAL: icon visible on the button
                button1.buttonOnClick.AddListener(delegate { PutYourFunctionHere(); }); // What should happen if the button is clicked?
                dialogMenuInfo.dialogMenuButtons.Add(button1);
        
                // Add a keyboard input field
                dialogMenuInfo.keyboardInputFieldTitles.Add("Put the title of the keyboard input field here. It will replace the field [KeyboardInput0]");
                
                // Add a checkbox field
                dialogMenuInfo.checkboxTitles.Add("Put the title of the checkbox field here. It will replace the field [Checkbox0]");

                // Add a toggle field
                dialogMenuInfo.toggleTitles.Add("Put the title of the toggle field here. It will replace the field [Toggle0]");

                // Add a dropdown field
                // Create the correct List of options for the Dropdown
                List<string> dropdownOptions; // Setup in inspector or assign otherwise
                List<TPI_DialogMenuDropDown.OptionData> dropdownList = new List<TPI_DialogMenuDropDown.OptionData>();
                for (int i = 0; i < dropdownOptions.Count; i++) { // Create the correct list of options
                    dropdownList.Add(new TPI_DialogMenuDropDown.OptionData(dropdownOptions[i], null)); // instead of null you can also assign a Texture2D
                }
                dialogMenuInfo.dialogMenuDropdowns.Add(new TPI_DialogMenuDropDown("Put the dropdown title here. It will replace the field [Dropdown0]", dropdownOptions));
                
                // Add a point selection field
                dialogMenuInfo.pointSelections.Add(new TPI_DialogMenuPointSelection("Put the title of the point selection field here. It will replace the field [PointSelection0]", null));  // instead of null you can also assign a Texture2D
                
                GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>().SpawnDialogMenu(dialogMenuInfo); // Create the Dialog Menu
            
                ////////////////////////////////////////////// CODE ENDS HERE //////////////////////////////////////////////
                
                IMPORTANT: Generally speaking, when planning on having multiple dialog menus in a sequence, them being connected by the click on a button,
                please first delete the old dialog menu before you spawn in the new one as problems with the ’ObjectPlacementController’ otherwise might arise.
            


        B)  Retrieval of the Choices made in a Dialog Menu:
    -----------------------------------------------------------
            
            
            The ’TPI_DialogMenuController’ makes it extremely easy for you to retrieve the choices made by the operator, as you just have to know the ’dialogMenuID’
            of the dialog menu you want to get the choices of.
            This can then be achieved by performing the following code BEFORE you delete the dialog menu:
            
            ////////////////////////////////////////////// CODE STARTS HERE //////////////////////////////////////////////
            
            TPI_DialogMenuChoices choices = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_DialogMenuController>().GetDialogMenuChoices(dialogMenuID);
            
            ////////////////////////////////////////////// CODE ENDS HERE //////////////////////////////////////////////
            

            Then, you can extract the desired information from each option (e.g. keyboard input field) by using the following code:
            Short reminder: the identifier for e.g. ’[KeyboardInput0]’ would be 0 and the one of ’[KeyboardInput3]’ would be 3!
            
            ////////////////////////////////////////////// CODE STARTS HERE //////////////////////////////////////////////
            
            choices.keyboardTexts[index] // returns the string of the text belonging to the keyboard input field with identifier "index"
            choices.checkboxes[index] // returns the bool belonging to the checkbox field with identifier "index"
            choices.toggles[index] // returns the bool belonging to the toggle field with identifier "index"
            choices.dropdown[index] // returns the index of the selected option belonging to the dropdown field with identifier "index"
            choices.selectedPoints[index] // returns the Vector3 of the position belonging to the point selection field with identifier "index"
            
            ////////////////////////////////////////////// CODE ENDS HERE //////////////////////////////////////////////


        C)  Deletion of a Dialog Menu:
    --------------------------------------

        
            1) "Coding all the way":
                
                There are two ways how you can delete a dialog menu by code:

                - First of all, you can save the dialogMenuID when you spawned the dialog menu and then plug it into the ’UnSpawnDialogMenu(string dialogMenuID)’ function.
                  The dialogMenuID can be obtained by using the following line: ’dialogMenuInfo.dialogMenuID;’

                - Otherwise, you could also save the ’TPI_DialogMenuInformation’ instance and then plug it into the ’UnSpawnDialogMenu(TPI_DialogMenuInformation dialogMenuInformation)’
                  function once you want to delete the dialog menu.
            
            2) "Inspector + using the functions given in this script":
            
               If you want to delete a dialog menu, which was registered in the ’Dialog Menu Template’ list in the Unity inspector, you can achieve this by looking up
               the respective ’dialogMenuID’ and plugging it into to following function: ’UnSpawnDialogMenu(string dialogMenuID)’.
               You can simply right-click and select ’Copy’ while hovering over the ’Dialog Menu ID’ field to obtain it.
            
            3) Deleting the last element added:
            
                To delete the last element added, you can simply call the ’UnSpawnLastDialogMenu()’ function.
            
            4) Delete all dialog menus:
            
                If you want to delete all active dialog menus at once (usually you have just one open), then you can call the ’ClearDialogMenus()’ function.



        D)  Creation of Dialog Menu Prefabs:
    --------------------------------------------

            
            Finally, in order to set up a new dialog menu prefab (the GameObject that is instantiated and then populated with the correct information), you can either start from
            scratch or you can copy and adapt an already existing one provided to you by the TPI. Keep in mind, it is always good to name your dialog menu prefabs in a way that
            makes it instantaneously clear what that template offers or where it is used, e.g. you could name a prefab with two text fields, a button and a keyboard input field:
            ’DMP: 2TXT 1BUT 1KIF Variation1’, where ’DMP’ refers to ’DialogMenuPrefab’, ’2TXT’ refers to ’2 text fields’, ’1BUT’ refers to ’1 button’ and ’1KIF’ refers to
            ’1 keyboard input field’. Finally, if you have multiple versions of the same prefab, each looking a bit different but containing the same elements, then it might make
            sense to also add a ’Variation1’ tag after it to signal that.


            IMPORTANT: The TPI can only guarantee that the population of information works with the provided ’Dialog Menu Building Blocks’ prefabs found in the ’Prefabs’ folder of the TaskPlanningInterface directory.
            Short reminder: the identifier for ’[KeyboardInput0]’ would be 0 and the one of ’[KeyboardInput3]’ would be 3!
            

            NAME: To specify the place where the name of the dialog menu should be located, move a text field template to your desired position and set the displayed text to "[Name]" without the quotes.

            TEXT FIELD: To specify the place where a text field should be located, move a text field prefab to your desired position and set the displayed text to the correct identifier,
                       e.g. "[Textfield0]" without the quotes.

            BUTTON: To specify the place where a button should be located, move a button prefab to your desired position and set the displayed text to the correct identifier,
                    e.g. "[Button0]" without the quotes.

            KEYBOARD INPUT FIELD: To specify the place where a keyboard input field should be located, move a keyboard input field prefab to your desired position and set the displayed text to the correct identifier,
                                   e.g. "[KeyboardInput0]" without the quotes.

            CHECKBOX: To specify the place where a checkbox field should be located, move a checkbox prefab to your desired position and set the displayed text to the correct identifier,
                      e.g. "[Checkbox0]" without the quotes.

            TOGGLE: To specify the place where a toggle field should be located, move a toggle prefab to your desired position and set the displayed text to the correct identifier,
                    e.g. "[Toggle0]" without the quotes.

            DROPDOWN: To specify the place where a dropdown selection field should be located, move a dropdown prefab to your desired position and set the displayed text to the correct identifier,
                      e.g. "[Dropdown0]" without the quotes.

            POINT SELECTION: To specify the place where a point selection field should be located, move a point selection prefab to your desired position and set the displayed text to the correct identifier,
                             e.g. "[PointSelection0]" without the quotes.


        E) Pre-Made Functions:
    ------------------------------

            
            There are multiple functions already implemented in this class, which will be used a lot to for example configure a snippet or constraint.
            Therefore, this should make it easier and faster for you, preventing repetition and errors.
            

            Short summary of each function:

            ShowErrorMenu: Creates a Dialog Menu that points out an error to the operator or provides the operator with information (e.g. not every field might have been filled in or the operator needs to confirm something).

            ShowErrorMenu_TwoButtons: Creates an version of the 'ShowErrorMenu' Dialog Menu containing two buttons. This can be useful to give the operator a choice between two options.

            ShowObjectNameMenu: Creates a Dialog Menu that allows the operator to select a Name for an Object (e.g. Snippet or Constraint).

            ShowConstraintTypeMenu: Creates a Dialog Menu that allows the operator to choose the type of a Constraint (whether it is globally active or snippet-specific).

            DropdownMenu: Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).

            
            For a more detailed description and how to use them, please visit the implementation further down below in this class.


        */
        //---------------------------------------------------- General Functions ----------------------------------------------------//



        #region GeneralFunctions
        /// <summary>
        /// Get the choices made by the operator during runtime. This includes for example the text input by a keyboard.
        /// <br></br>You can find the ID in the Unity Inspector in the "Dialog Menu Prefab List" of the "Dialog Menu Controller" Component on the TPI_Manager GameObject.
        /// <br></br>You can also get the ID from TPI_DialogMenuInformation by using '.dialogMenuID' after your reference (it is not static).
        /// <para><paramref name="dialogMenuID"/> = ID of the Dialog Menu that you want to get the choices of</para>
        /// </summary>
        /// <returns>TPI_DialogMenuChoices parameter containing all the choices made in the dialog menu with the provided ID</returns>
        public TPI_DialogMenuChoices GetDialogMenuChoices(string dialogMenuID) {

            if (spawnedDialogMenus.Count == 0) {
                Debug.LogError("No Dialog Menu were spawned! (GetDialogMenuChoices in TPI_DialogMenuController)");
                return null;
            }

            // Look for the dialog menu with the provided dialogMenuID
            GameObject dialogMenuObject = null;
            for (int i = 0; i < mainController.dialogMenuContainer.transform.childCount; i++) {
                if (mainController.dialogMenuContainer.transform.GetChild(i).GetComponent<TPI_ObjectIdentifier>().GUID == dialogMenuID) {
                    dialogMenuObject = mainController.dialogMenuContainer.transform.GetChild(i).gameObject;
                    break;
                } else
                    continue;
            }

            if (dialogMenuObject == null) {
                Debug.LogError("No dialog menu with the ID " + dialogMenuID + " was found while looking through the spawned dialog menus! (GetDialogMenuChoices in TPI_DialogMenuController)");
                return null;
            }

            TPI_DialogMenuChoices choices = new TPI_DialogMenuChoices();

            TPI_ObjectIdentifier[] objectIdentficators = dialogMenuObject.GetComponentsInChildren<TPI_ObjectIdentifier>();
            foreach (TPI_ObjectIdentifier obj in objectIdentficators) {

                // Configure Keyboard Input Field, Key: "[KeyboardInputX]" with X = Index -> Output: string
                if (obj.GUID.Contains("[KeyboardInput", System.StringComparison.OrdinalIgnoreCase)) {
                    choices.keyboardTexts.Insert(GetIndexFromString(obj.GetComponent<TPI_ObjectIdentifier>().GUID, "[KeyboardInput"), obj.transform.parent.GetComponentInChildren<MRTKTMPInputField>().text);
                }

                // Configure Checkboxes, Key: "[CheckBoxY]" with Y = Index -> Output: bool
                if (obj.GUID.Contains("[CheckBox", System.StringComparison.OrdinalIgnoreCase)) {
                    bool status = false; // false -> noTick, true -> tick
                    if (obj.transform.GetChild(3).GetChild(4).gameObject.activeSelf == false) // GetChild(4) -> UIButtonToggleIconOff
                        status = true;
                    choices.checkboxes.Insert(GetIndexFromString(obj.GUID, "[CheckBox"), status);
                }

                // Configure Toggles, Key: "[ToggleX]" with X = Index -> Output: bool
                if (obj.GUID.Contains("[Toggle", System.StringComparison.OrdinalIgnoreCase)) {
                    bool status = false; // false -> notToggled, true -> toggled
                    if (obj.transform.GetChild(3).GetChild(4).gameObject.activeSelf == false) // GetChild(4) -> UIButtonToggleIconOff
                        status = true;
                    choices.toggles.Insert(GetIndexFromString(obj.GUID, "[Toggle"), status);
                }

                // Configure Dropdowns, Key: "[DropdownX]" with X = Index -> Output: int (index in the string list)
                if (obj.GUID.Contains("[Dropdown", System.StringComparison.OrdinalIgnoreCase)) {
                    choices.dropdown.Insert(GetIndexFromString(obj.GUID, "[Dropdown"), obj.transform.parent.GetComponentInChildren<TMP_Dropdown>().value);
                }

                // Configure Point Selection, Key: "[PointSelectionX]" with X = Index -> Output: Vector3
                if (obj.GUID.Contains("[PointSelection", System.StringComparison.OrdinalIgnoreCase)) {
                    choices.selectedPoints.Insert(GetIndexFromString(obj.GUID, "[PointSelection"), obj.transform.position);
                }


            }

            // Configure Object Selection, Key: "[ObjectSelectionX]" with X = Index -> Output: UNKNOWN



            // Configure Surface Selection, Key: "[SurfaceSelectionX]" with X = Index -> Output: UNKNOWn
            // uses spatial awareness



            return choices;

        }

        /// <summary>
        /// Helper function to get a number from a string (extract number and convert to int)
        /// <br></br>This function only works for the template, which was used in this class, e.g. [PointSelection].
        /// <para><paramref name="text"/> = the text from which the number should get extracted from</para>
        /// <para><paramref name="prefix"/> = the prefix that is located before the number in the string, e.g. "[KeyboardInput"</para>
        /// </summary>
        /// <returns>extracted int</returns>
        private int GetIndexFromString(string text, string prefix) { // e.g.: type = "[KeyboardInput"
            int numBefore = text.IndexOf(prefix) + prefix.Length;
            int numAfter = text.LastIndexOf("]");
            int number;
            int.TryParse(text.Substring(numBefore, numAfter - numBefore), out number);
            return number;
        }

        /// <summary>
        /// Returns how many dialog menus are currently active
        /// </summary>
        public int GetDialogMenuCount() {
            return spawnedDialogMenus.Count;
        }

        /// <summary>
        /// Create a Dialog Menu by plugging in the dialogMenuID of the respective Template that you want.
        /// <br></br>You can find the ID in the Unity Inspector in the "Dialog Menu Templates List" of the "Dialog Menu Controller" Component on the TPI_Manager GameObject.
        /// <br></br>If you want to use this function in a script, you have to first insert your dialog menu template into the dialogMenuPrefabList and then extract the dialogMenuID.
        /// <para>IMPORTANT: This function does not work if you have not inserted the DialogMenu into the dialogMenuPrefabList (either in the Inspector or via script)!</para>
        /// <para><paramref name="dialogMenuID"/> = ID of the Dialog Menu that you want to spawn</para>
        /// <para>OPTIONAL: <paramref name="previousDialogMenuIndex"/> = Index of the previous Dialog Menu; set it to -1 if there was none. Can be used to position the new dialog menu in the vicinity of the old one.</para>
        /// </summary>
        public void SpawnDialogMenu(string dialogMenuID, int previousDialogMenuIndex = -1) {

            if (dialogMenuPrefabList.Count == 0) {
                Debug.LogError("No Dialog Menu was setup in the Prefab List! (SpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }

            // Look for the dialogMenuInformation
            TPI_DialogMenuInformation information = null;
            foreach (TPI_DialogMenuInformation dialogMenu in dialogMenuPrefabList) {
                if (dialogMenu.dialogMenuID == dialogMenuID) {
                    information = dialogMenu;
                    break;
                } else
                    continue;
            }

            if (information == null) {
                Debug.LogError("The Dialog Menu Prefab List does not contain a prefab with this ID! (SpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }
            InstantiateDialogMenu(information, previousDialogMenuIndex);

        }

        /// <summary>
        /// Create a Dialog Menu by plugging in the index of the respective Prefab in the "Dialog Menu Prefab List" that you want.
        /// <br></br>You can find the Index in the Unity Inspector in the "Dialog Menu Prefab List" of the "Dialog Menu Controller" Component on the TPI_Manager GameObject.
        /// <para>IMPORTANT: This function does not work if you have not inserted your DialogMenu into the dialogMenuPrefabLis (either in the Inspector or via script)!</para>
        /// <para><paramref name="dialogMenuIndex"/> = Index of the Dialog Menu that you want to spawn</para>
        /// <para>OPTIONAL: <paramref name="previousDialogMenuIndex"/> = Index of the previous Dialog Menu; set it to -1 if there was none. Can be used to position the new dialog menu in the vicinity of the old one.</para>
        /// </summary>
        public void SpawnDialogMenu(int dialogMenuIndex, int previousDialogMenuIndex = -1) {

            if (dialogMenuPrefabList.Count == 0) {
                Debug.LogError("No Dialog Menu was setup in the Prefab List! (SpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }

            if (dialogMenuPrefabList.Count - 1 < dialogMenuIndex) {
                Debug.LogError("The given dialogMenuIndex is out of bounds! (SpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }

            if (dialogMenuPrefabList[dialogMenuIndex] == null) {
                Debug.LogError("The Dialog Menu Prefab List does not contain a valid prefab at this index! (SpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }
            InstantiateDialogMenu(dialogMenuPrefabList[dialogMenuIndex], previousDialogMenuIndex);

        }

        /// <summary>
        /// Create a Dialog Menu by plugging in the dialogMenuInformation.
        /// <br></br>In contrast to the other SpawnDialogMenu functions, this DialogMenuInformation file does not have to be setup in the "Dialog Menu Prefab List" via the Unity Inspector and can be created freely by coding.
        /// <para><paramref name="dialogMenuInformation"/> = dialogMenuInformation of the Dialog Menu that you want to spawn</para>
        /// <para>OPTIONAL: <paramref name="previousDialogMenuIndex"/> = Index of the previous Dialog Menu; set it to -1 if there was none. Can be used to position the new dialog menu in the vicinity of the old one.</para>
        /// </summary>
        public void SpawnDialogMenu(TPI_DialogMenuInformation dialogMenuInformation, int previousDialogMenuIndex = -1) {
            InstantiateDialogMenu(dialogMenuInformation, previousDialogMenuIndex);
        }


        /// <summary>
        /// This function sets up and configures the visual dialog menu controller.
        /// <br></br>It was separated into a new function in order to prevent repetition (three ways to spawn and prepare a dialog menu -> one way to instantiate it)
        /// <para><paramref name="information"/> = dialogMenuInformation of the Dialog Menu that you want to spawn</para>
        /// <para>OPTIONAL: <paramref name="previousDialogMenuIndex"/> = Index of the previous Dialog Menu; set it to -1 if there was none. Can be used to position the new dialog menu in the vicinity of the old one.</para>
        /// </summary>
        private void InstantiateDialogMenu(TPI_DialogMenuInformation information, int previousDialogMenuIndex = -1) {

            if (information.dialogMenuPrefab == null) {
                Debug.LogError("The given TPI_DialogMenuInformation does not contain a prefab! (InstantiateDialogMenu in TPI_DialogMenuController)");
                return;
            }

            spawnedDialogMenus.Add(information);

            // Instantiate the prefab and configure it
            GameObject menuObject = Instantiate(information.dialogMenuPrefab);
            menuObject.transform.parent = mainController.dialogMenuContainer.transform;
            menuObject.name = information.dialogMenuName + " Dialog Menu";
            menuObject.AddComponent<TPI_ObjectIdentifier>().GUID = information.dialogMenuID;

            TPI_ObjectPlacementController.PositionAndRotation pose;
            if (previousDialogMenuIndex == -1)
                pose = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().FindAndReservePosition(menuObject, TPI_ObjectPlacementController.StartingPosition.MiddleCenter, TPI_ObjectPlacementController.SearchAlgorithm.closestPosition, TPI_ObjectPlacementController.SearchDirection.bothWays);
            else
                pose = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().FindAndReservePosition(menuObject, previousDialogMenuIndex, TPI_ObjectPlacementController.SearchAlgorithm.closestPosition, TPI_ObjectPlacementController.SearchDirection.bothWays);
            menuObject.transform.position = pose.position;
            menuObject.transform.rotation = pose.rotation;

            // Configure Buttons (Replace Text, Add Icon and Listener), Key: "[ButtonX]" with X = Index
            ButtonConfigHelper[] buttons = menuObject.GetComponentsInChildren<ButtonConfigHelper>();
            int counter = 0;
            for (int i = 0; i < buttons.Length; i++) {
                if (buttons[i] == null)
                    continue;
                if (!buttons[i].MainLabelText.Contains("[Button", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int buttonIndex = 0; buttonIndex < information.dialogMenuButtons.Count; buttonIndex++) {
                    if (!buttons[i].MainLabelText.Contains("[Button" + buttonIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    buttons[i].MainLabelText = information.dialogMenuButtons[buttonIndex].buttonText;
                    if (information.dialogMenuButtons[buttonIndex].buttonIcon != null)
                        buttons[i].SetQuadIcon(information.dialogMenuButtons[buttonIndex].buttonIcon);
                    UnityEvent buttonEvent = information.dialogMenuButtons[buttonIndex].buttonOnClick;
                    buttons[i].OnClick.AddListener(() => buttonEvent?.Invoke()); // Converts UnityEvent to UnityAction
                    buttons[i] = null; // Improves Performance in the other for loops
                    break;
                }
            }
            CheckCounter(counter, information.dialogMenuButtons.Count, "Buttons", information);


            // Configure Dialog Menu Name, Key: "[Name]"
            // Configure Textfields, Key: "[TextfieldY]" with Y = Index
            TextMeshPro[] texts = menuObject.GetComponentsInChildren<TextMeshPro>();
            counter = 0;
            for (int i = 0; i < texts.Length; i++) {
                if (texts[i] == null)
                    continue;
                if (texts[i].text.Contains("[Name]", System.StringComparison.OrdinalIgnoreCase)) {
                    texts[i].text = information.dialogMenuName;
                    texts[i] = null; // Improves Performance in the other for loops
                    continue;
                }
                if (!texts[i].text.Contains("[Textfield", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int textIndex = 0; textIndex < information.dialogMenuTexts.Count; textIndex++) {
                    if (!texts[i].text.Contains("[Textfield" + textIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    texts[i].text = information.dialogMenuTexts[textIndex];
                    texts[i] = null; // Improves Performance in the other for loops
                    break;
                }
            }
            CheckCounter(counter, information.dialogMenuTexts.Count, "Textfields", information);


            // Configure Keyboard Input Field, Key: "[KeyboardInputX]" with X = Index -> Output: string
            TextMeshProUGUI[] textMeshProUGUIs = menuObject.GetComponentsInChildren<TextMeshProUGUI>();
            counter = 0;
            for (int i = 0; i < textMeshProUGUIs.Length; i++) {
                if (textMeshProUGUIs[i] == null)
                    continue;
                if (!textMeshProUGUIs[i].text.Contains("[KeyboardInput", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int keyboardIndex = 0; keyboardIndex < information.keyboardInputFieldTitles.Count; keyboardIndex++) {
                    if (!textMeshProUGUIs[i].text.Contains("[KeyboardInput" + keyboardIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    textMeshProUGUIs[i].text = information.keyboardInputFieldTitles[keyboardIndex];
                    textMeshProUGUIs[i].AddComponent<TPI_ObjectIdentifier>().GUID = "[KeyboardInput" + keyboardIndex.ToString() + "]";
                    keyboardIndex++;
                    textMeshProUGUIs[i] = null; // Improves Performance in the other for loops
                    break;
                }
            }
            CheckCounter(counter, information.keyboardInputFieldTitles.Count, "Keyboard Input Fields", information);


            // Configure Checkboxes, Key: "[CheckBoxY]" with Y = Index -> Output: bool
            counter = 0;
            for (int i = 0; i < buttons.Length; i++) {
                if (buttons[i] == null)
                    continue;
                if (!buttons[i].MainLabelText.Contains("[CheckBox", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int buttonIndex = 0; buttonIndex < information.checkboxTitles.Count; buttonIndex++) {
                    if (!buttons[i].MainLabelText.Contains("[CheckBox" + buttonIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    buttons[i].MainLabelText = information.checkboxTitles[buttonIndex];
                    buttons[i].AddComponent<TPI_ObjectIdentifier>().GUID = "[CheckBox" + buttonIndex.ToString() + "]";
                    buttons[i] = null; // Improves Performance in the other for loops
                    break;
                }
            }
            CheckCounter(counter, information.checkboxTitles.Count, "Checkboxes", information);


            // Configure Toggles, Key: "[ToggleX]" with X = Index -> Output: bool
            counter = 0;
            for (int i = 0; i < buttons.Length; i++) {
                if (buttons[i] == null)
                    continue;
                if (!buttons[i].MainLabelText.Contains("[Toggle", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int buttonIndex = 0; buttonIndex < information.toggleTitles.Count; buttonIndex++) {
                    if (!buttons[i].MainLabelText.Contains("[Toggle" + buttonIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    buttons[i].MainLabelText = information.toggleTitles[buttonIndex];
                    buttons[i].AddComponent<TPI_ObjectIdentifier>().GUID = "[Toggle" + buttonIndex.ToString() + "]";
                    buttons[i] = null; // Improves Performance in the other for loop
                    break;
                }
            }
            CheckCounter(counter, information.toggleTitles.Count, "Toggles", information);


            // Create the correct Options Lists for the Dropdowns
            List<List<TMP_Dropdown.OptionData>> _dropdownOptionsList = new List<List<TMP_Dropdown.OptionData>>();
            for (int i = 0; i < information.dialogMenuDropdowns.Count; i++) {
                List<TMP_Dropdown.OptionData> localList = new List<TMP_Dropdown.OptionData>();
                for (int optionIndex = 0; optionIndex < information.dialogMenuDropdowns[i].dropdownOptions.Count; optionIndex++) {
                    Texture2D image = information.dialogMenuDropdowns[i].dropdownOptions[optionIndex].image;
                    if (image != null)
                        localList.Add(new TMP_Dropdown.OptionData(information.dialogMenuDropdowns[i].dropdownOptions[optionIndex].text, Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f))));
                    else
                        localList.Add(new TMP_Dropdown.OptionData(information.dialogMenuDropdowns[i].dropdownOptions[optionIndex].text, null));
                }
                _dropdownOptionsList.Add(localList);
            }

            // Configure Dropdowns, Key: "[DropdownX]" with X = Index -> Output: int (index in the string list)
            counter = 0;
            for (int i = 0; i < textMeshProUGUIs.Length; i++) {
                if (textMeshProUGUIs[i] == null)
                    continue;
                if (!textMeshProUGUIs[i].text.Contains("[Dropdown", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int dropdownIndex = 0; dropdownIndex < information.dialogMenuDropdowns.Count; dropdownIndex++) {
                    if (!textMeshProUGUIs[i].text.Contains("[Dropdown" + dropdownIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;
                    textMeshProUGUIs[i].text = information.dialogMenuDropdowns[dropdownIndex].dropdownTitle;
                    textMeshProUGUIs[i].transform.parent.GetComponentInChildren<TMP_Dropdown>().options = _dropdownOptionsList[dropdownIndex];
                    textMeshProUGUIs[i].AddComponent<TPI_ObjectIdentifier>().GUID = "[Dropdown" + dropdownIndex.ToString() + "]";
                    textMeshProUGUIs[i] = null; // Improves Performance in the other for loops
                    break;
                }
            }
            CheckCounter(counter, information.dialogMenuDropdowns.Count, "Dropdowns", information);


            // Configure Point Selection, Key: "[PointSelectionX]" with X = Index -> Output: Vector3
            counter = 0;
            for (int i = 0; i < buttons.Length; i++) {
                if (buttons[i] == null)
                    continue;
                if (!buttons[i].MainLabelText.Contains("[PointSelection", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                for (int buttonIndex = 0; buttonIndex < information.pointSelections.Count; buttonIndex++) {
                    if (!buttons[i].MainLabelText.Contains("[PointSelection" + buttonIndex.ToString() + "]", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    buttons[i].MainLabelText = information.pointSelections[buttonIndex].buttonText;
                    if (information.pointSelections[buttonIndex].buttonIcon != null)
                        buttons[i].SetQuadIcon(information.pointSelections[buttonIndex].buttonIcon);
                    buttons[i].OnClick.AddListener(delegate { pointSelectionHelper(information.pointSelections[buttonIndex], buttonIndex, information.dialogMenuID); });
                    buttons[i] = null; // Improves Performance in the other for loop
                    break;
                }
            }
            CheckCounter(counter, information.pointSelections.Count, "Point Selections", information);


            // Configure Object Selection, Key: "[ObjectSelectionX]" with X = Index -> Output: UNKNOWN


            // Configure Surface Selection, Key: "[SurfaceSelectionX]" with X = Index -> Output: UNKNOWn
            // uses spatial awareness


            // Configure Close Menu Button, Key: "[CloseMenu]"
            counter = 0;
            for (int i = 0; i < buttons.Length; i++) {
                if (buttons[i] == null)
                    continue;
                if (!buttons[i].MainLabelText.Contains("[CloseMenu]", System.StringComparison.OrdinalIgnoreCase))
                    continue;
                counter++;
                if (information.showCloseMenuButton) {
                    buttons[i].MainLabelText = "Close Menu";
                    buttons[i].OnClick.AddListener(delegate { UnSpawnDialogMenu(information.dialogMenuID); });
                    buttons[i] = null; // Improves Performance in the other for loop
                } else
                    buttons[i].gameObject.SetActive(false);
            }

            if (information.showCloseMenuButton & counter == 0)
                Debug.LogWarning("No CloseMenu Button with the Text '[CloseMenu]' was found in the Dialog Menu Template. (ShowCloseMenuButton was selected in the Dialog Menu: " + information.dialogMenuName
                        + " with the ID: " + information.dialogMenuID + ") (InstantiateDialogMenu in TPI_DialogMenuController)");


        }

        /// <summary>
        /// Helper function that determines whether the correct amount of items have been setup and configured in the dialog menu template (GameObject) and in the inspector / by script
        /// <para><paramref name="counter"/> = number of elements contained in the prefab</para>
        /// <para><paramref name="listSize"/> = number of elements set up in your template (in the inspector or by script)</para>
        /// <para><paramref name="counterType"/> = name of the element that should be checked (in plural!)</para>
        /// <para><paramref name="information"/> = dialogMenuInformation of the Dialog Menu that will be spawned</para>
        /// </summary>
        private void CheckCounter(int counter, int listSize, string counterType, TPI_DialogMenuInformation information) {
            if (counter > listSize)
                Debug.LogWarning("Not enough " + counterType + " were configured for the dialog menu with the name " + information.dialogMenuName + " and the ID "
                    + information.dialogMenuID + " in order to properly populate the template! Please either configure more " + counterType + " (e.g. from the inspector or via script), or remove some prefabs from your template. (InstantiateDialogMenu in TPI_DialogMenuController)");
            else if (counter < listSize)
                Debug.LogWarning("Too many " + counterType + " were configured for the dialog menu with the name " + information.dialogMenuName + " and the ID "
                    + information.dialogMenuID + " Please either remove some " + counterType + " from the inspector or your code, or add more prefabs to your template! (InstantiateDialogMenu in TPI_DialogMenuController)");
        }

        /// <summary>
        /// Helper function that is needed for the Point Selection Dialog Menu Option.
        /// <br></br>If this function is called the first time, it instantiates the spherical point selection object and configures it.
        /// <br></br>Afterwards during the next executions, it either hides or shows the sphere with it's tooltips.
        /// <para><paramref name="pointSelection"/> = dialogMenuPointSelection of the specific button that was clicked</para>
        /// <para><paramref name="index"/> = the index of the point selection button in the dialogMenuInformation (in the inspector or by script)</para>
        /// <para><paramref name="dialogMenuID"/> = ID of the Dialog Menu that was spawned</para>
        /// </summary>
        private void pointSelectionHelper(TPI_DialogMenuPointSelection pointSelection, int index, string dialogMenuID) {

            if (pointSelection.selectionSphere == null) {

                // Look for the dialog menu with the provided dialogMenuID
                GameObject dialogMenuObject = null;
                for (int i = 0; i < mainController.dialogMenuContainer.transform.childCount; i++) {
                    if (mainController.dialogMenuContainer.transform.GetChild(i).GetComponent<TPI_ObjectIdentifier>().GUID == dialogMenuID) {
                        dialogMenuObject = mainController.dialogMenuContainer.transform.GetChild(i).gameObject;
                        break;
                    } else
                        continue;
                }

                if (dialogMenuObject == null) {
                    Debug.LogError("No dialog menu with the ID " + dialogMenuID + " was found while looking through the spawned dialog menus! (pointSelectionHelper in TPI_DialogMenuController)");
                    return;
                }

                GameObject sphereObject = Instantiate(pointSelectionSphere, dialogMenuObject.transform);
                sphereObject.name = "Point Selection Sphere: " + index.ToString() + ", Name: " + pointSelection.buttonText;
                sphereObject.AddComponent<TPI_ObjectIdentifier>().GUID = "[PointSelection" + index.ToString() + "]";
                sphereObject.transform.localPosition = new Vector3(dialogMenuObject.transform.GetComponent<BoxCollider>().bounds.size.x / 2 * 1.1f, 0, 0);
                sphereObject.GetComponentInChildren<ToolTip>().ToolTipText = pointSelection.buttonText;
                sphereObject.GetComponent<MeshRenderer>().material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                pointSelection.selectionSphere = sphereObject;
            } else {
                if (pointSelection.selectionSphere.gameObject.activeSelf)
                    pointSelection.selectionSphere.gameObject.SetActive(false);
                else
                    pointSelection.selectionSphere.gameObject.SetActive(true);
            }

        }

        /// <summary>
        /// Remove an active Dialog Menu by plugging in the dialogMenuID of the respective Prefab that you want.
        /// <br></br>You can find the ID in the Unity Inspector in the "Dialog Menu Prefab List" of the "Dialog Menu Controller" Component on the TPI_Manager GameObject.
        /// <br></br>If you want to use this function in a script, you had to first insert your dialog menu template into the dialogMenuPrefabList and then extract the dialogMenuID.
        /// <para>IMPORTANT: This function does not work if you have not inserted the DialogMenu into the dialogMenuPrefabList (either in the Inspector or via script)!</para>
        /// <para><paramref name="dialogMenuID"/> = ID of the Dialog Menu that should be destroyed</para>
        /// </summary>
        public void UnSpawnDialogMenu(string dialogMenuID) {

            if (spawnedDialogMenus.Count == 0) {
                Debug.LogError("No Dialog Menu currently exists! (UnSpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }

            // Look for the correct dialog menu
            int index = -1;
            for (int i = 0; i < spawnedDialogMenus.Count; i++) {
                if (spawnedDialogMenus[i].dialogMenuID == dialogMenuID) {
                    index = i;

                    // Destroy Point Selection Spheres
                    for (int buttonIndex = 0; buttonIndex < spawnedDialogMenus[i].pointSelections.Count; buttonIndex++) {
                        Destroy(spawnedDialogMenus[i].pointSelections[buttonIndex].selectionSphere);
                    }

                    break;
                } else
                    continue;
            }

            if (index == -1) {
                Debug.LogError("No Dialog Menu with this ID was spawned! (UnSpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }
            DestroyDialogMenu(index);

        }

        /// <summary>
        /// Remove an active Dialog Menu by plugging in the dialogMenuInformation.
        /// <br></br>In contrast to the other UnspawnDialogMenu functions, this DialogMenuInformation file does not have to be setup in the "Dialog Menu Prefab List" via the Unity Inspector and can be created freely by coding.
        /// <para><paramref name="dialogMenuInformation"/> = dialogMenuInformation of the Dialog Menu that should be destroyed</para>
        /// </summary>
        public void UnSpawnDialogMenu(TPI_DialogMenuInformation dialogMenuInformation) {

            if (spawnedDialogMenus.Count == 0) {
                Debug.LogError("No Dialog Menu currently exists! (UnSpawnDialogMenu in TPI_DialogMenuController)");
                return;
            }

            // Look for the correct dialog menu
            int index = -1;
            for (int i = 0; i < spawnedDialogMenus.Count; i++) {
                if (spawnedDialogMenus[i] == dialogMenuInformation) {
                    index = i;

                    // Destroy Point Selection Spheres
                    for (int buttonIndex = 0; buttonIndex < spawnedDialogMenus[i].pointSelections.Count; buttonIndex++) {
                        Destroy(spawnedDialogMenus[i].pointSelections[buttonIndex].selectionSphere);
                    }

                    break;
                } else
                    continue;
            }

            if (index == -1) {
                Debug.LogError("No active Dialog Menu with this DialogMenuInformation currently exists! (UnSpawnDialogMenu in TPI_DialogMenuController)");
                return;
            } else
                DestroyDialogMenu(index);

        }

        /// <summary>
        /// Remove the last Dialog Menu that was created.
        /// </summary>
        public void UnSpawnLastDialogMenu() {

            if (spawnedDialogMenus.Count > 0) {
                DestroyDialogMenu(spawnedDialogMenus.Count - 1);
            } else {
                Debug.LogError("No Dialog Menu currently exists! (UnSpawnLastDialogMenu in TPI_DialogMenuController)");
                return;
            }

        }

        /// <summary>
        /// Remove all active Dialog Menus that were previously created
        /// </summary>
        public void ClearDialogMenus() {
            for (int i = 0; i < spawnedDialogMenus.Count; i++) {
                DestroyDialogMenu(i);
            }
        }

        /// <summary>
        /// Helper function to destroy all dialog menus.
        /// <para><paramref name="index"/> = index of the Dialog Menu that should be destroyed</para>
        /// </summary>
        private void DestroyDialogMenu(int index) {
            spawnedDialogMenus.Remove(spawnedDialogMenus[index]);
            mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().FreeUpSpot(mainController.dialogMenuContainer.transform.GetChild(index).gameObject);
            Destroy(mainController.dialogMenuContainer.transform.GetChild(index).gameObject);
        }
        #endregion GeneralFunctions



        //---------------------------------------------------- Error / Assert Menu ----------------------------------------------------//



        #region ErrorMenu
        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        ///<br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuText"/> = Text displayed in the Menu</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu/para>
        /// </summary>
        public void ShowErrorMenu(string menuText, bool placeAtPositionOfPrevious = false) {
            ShowErrorMenu("Error", menuText, "Confirm", null, null, placeAtPositionOfPrevious);
        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in the Menu</para>
        /// <para><paramref name="buttonText"/> = Text displayed on the Button</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu/para>
        /// </summary>
        public void ShowErrorMenu(string menuName, string menuText, string buttonText, bool placeAtPositionOfPrevious = false) {
            ShowErrorMenu(menuName, menuText, buttonText, null, null, placeAtPositionOfPrevious);
        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in the Menu</para>
        /// <para><paramref name="buttonText"/> = Text displayed on the Button</para>
        /// <para><paramref name="buttonIcon"/> = Icon displayed on the Button</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu/para>
        /// </summary>
        public void ShowErrorMenu(string menuName, string menuText, string buttonText, Texture2D buttonIcon, bool placeAtPositionOfPrevious = false) {
            ShowErrorMenu(menuName, menuText, buttonText, buttonIcon, null, placeAtPositionOfPrevious);
        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para>Compared to the other ShowErrorMenu functions, this functions has a UnityEvent as an input instead of a UnityAction. -> It will hardly be used, but was added in order to cover all cases.</para>
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="buttonText"/> = Text displayed on the Button</para>
        /// <para><paramref name="buttonIcon"/> = Icon displayed on the Button</para>
        /// <para><paramref name="buttonOnClick"/> = UnityEvent invoked when the Button is pressed</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu/para>
        /// </summary>
        public void ShowErrorMenuUnityEvent(string menuName, string menuText, string buttonText, Texture2D buttonIcon, UnityEvent buttonOnClick, bool placeAtPositionOfPrevious = false) {

            int previousPositionIndex;
            if (placeAtPositionOfPrevious)
                previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid
            else
                previousPositionIndex = -1; // place it near the Middle of Height and Width

            // Create the "Error" dialog menu
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = errorMenuPrefab;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton button = new TPI_DialogMenuButton();
            button.buttonText = buttonText;
            if (buttonIcon == null)
                button.buttonIcon = errorAcceptButtonIcon;
            else
                button.buttonIcon = buttonIcon;
            button.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            if (buttonOnClick != null) { // Give the option to add custom events -> call your own function
                UnityEvent buttonEvent = buttonOnClick;
                button.buttonOnClick.AddListener(() => buttonEvent?.Invoke()); // Converts UnityEvent to UnityAction
            }
            dialogMenuInfo.dialogMenuButtons.Add(button);

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);

        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="buttonText"/> = Text displayed on the Button</para>
        /// <para><paramref name="buttonIcon"/> = Icon displayed on the Button</para>
        /// <para><paramref name="buttonOnClick"/> = UnityAction performed when the Button is pressed</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu/para>
        /// </summary>
        public void ShowErrorMenu(string menuName, string menuText, string buttonText, Texture2D buttonIcon, UnityAction buttonOnClick, bool placeAtPositionOfPrevious = false) {

            int previousPositionIndex;
            if (placeAtPositionOfPrevious)
                previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid
            else
                previousPositionIndex = -1; // place it near the Center Middle

            // Creates the "Error" dialog menu
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = errorMenuPrefab;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton button = new TPI_DialogMenuButton();
            button.buttonText = buttonText;
            if (buttonIcon == null)
                button.buttonIcon = errorAcceptButtonIcon;
            else
                button.buttonIcon = buttonIcon;
            button.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            if (buttonOnClick != null) { // Give the option to add custom events -> call your own function
                UnityAction buttonEvent = buttonOnClick;
                button.buttonOnClick.AddListener(buttonEvent); // Converts UnityEvent to UnityAction
            }
            dialogMenuInfo.dialogMenuButtons.Add(button);

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);

        }
        #endregion ErrorMenu


        #region ErrorMenu_TwoButtons
        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        ///<br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuText"/> = Text displayed in the Menu</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu</para>
        /// </summary>
        public void ShowErrorMenu_TwoButtons(string menuText, bool placeAtPositionOfPrevious = false) {
            ShowErrorMenu_TwoButtons("Error", menuText, "Accept", null, null, "Decline", null, null, placeAtPositionOfPrevious);
        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in the Menu</para>
        /// <para><paramref name="firstButtonText"/> = Text displayed on the first Button</para>
        /// <para><paramref name="secondButtonText"/> = Text displayed on the second Button</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu</para>
        /// </summary>
        public void ShowErrorMenu_TwoButtons(string menuName, string menuText, string firstButtonText, string secondButtonText, bool placeAtPositionOfPrevious = false) {
            ShowErrorMenu_TwoButtons(menuName, menuText, firstButtonText, null, null, secondButtonText, null, null, placeAtPositionOfPrevious);
        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in the Menu</para>
        /// <para><paramref name="firstButtonText"/> = Text displayed on the first Button</para>
        /// <para><paramref name="firstButtonIcon"/> = Icon displayed on the first Button</para>
        /// <para><paramref name="secondButtonText"/> = Text displayed on the second Button</para>
        /// <para><paramref name="secondButtonIcon"/> = Icon displayed on the second Button</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu</para>
        /// </summary>
        public void ShowErrorMenu_TwoButtons(string menuName, string menuText, string firstButtonText, Texture2D firstButtonIcon, string secondButtonText, Texture2D secondButtonIcon, bool placeAtPositionOfPrevious = false) {
            ShowErrorMenu_TwoButtons(menuName, menuText, firstButtonText, firstButtonIcon, null, secondButtonText, secondButtonIcon, null, placeAtPositionOfPrevious);
        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para>Compared to the other ShowErrorMenu functions, this functions has a UnityEvent as an input instead of a UnityAction. -> It will hardly be used, but was added in order to cover all cases.</para>
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="firstButtonText"/> = Text displayed on the first Button</para>
        /// <para><paramref name="firstButtonIcon"/> = Icon displayed on the first Button</para>
        /// <para><paramref name="firstButtonOnClick"/> = UnityEvent invoked when the first Button is pressed</para>
        /// <para><paramref name="secondButtonText"/> = Text displayed on the second Button</para>
        /// <para><paramref name="secondButtonIcon"/> = Icon displayed on the second Button</para>
        /// <para><paramref name="secondButtonOnClick"/> = UnityEvent invoked when the second Button is pressed</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu</para>
        /// </summary>
        public void ShowErrorMenuUnityEvent_TwoButtons(string menuName, string menuText, string firstButtonText, Texture2D firstButtonIcon, UnityEvent firstButtonOnClick, string secondButtonText, Texture2D secondButtonIcon, UnityEvent secondButtonOnClick, bool placeAtPositionOfPrevious = false) {

            int previousPositionIndex;
            if (placeAtPositionOfPrevious)
                previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid
            else
                previousPositionIndex = -1; // place it near the Center Middle

            // Create the "Error" dialog menu
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = errorMenuPrefab_twoButtons;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton firstButton = new TPI_DialogMenuButton();
            firstButton.buttonText = firstButtonText;
            if (firstButtonIcon == null)
                firstButton.buttonIcon = errorAcceptButtonIcon;
            else
                firstButton.buttonIcon = firstButtonIcon;
            firstButton.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            if (firstButtonOnClick != null) { // Give the option to add custom events -> call your own function
                UnityEvent buttonEvent = firstButtonOnClick;
                firstButton.buttonOnClick.AddListener(() => buttonEvent?.Invoke()); // Converts UnityEvent to UnityAction
            }
            dialogMenuInfo.dialogMenuButtons.Add(firstButton);


            TPI_DialogMenuButton secondButton = new TPI_DialogMenuButton();
            secondButton.buttonText = secondButtonText;
            if (secondButtonIcon == null)
                secondButton.buttonIcon = errorDeclineButtonIcon;
            else
                secondButton.buttonIcon = secondButtonIcon;
            secondButton.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            if (secondButtonOnClick != null) { // Give the option to add custom events -> call your own function
                UnityEvent buttonEvent = secondButtonOnClick;
                secondButton.buttonOnClick.AddListener(() => buttonEvent?.Invoke()); // Converts UnityEvent to UnityAction
            }
            dialogMenuInfo.dialogMenuButtons.Add(secondButton);

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);

        }

        /// <summary>
        /// Creates a Dialog Menu that points out an error to the operator (e.g. not every field might have been filled in or the operator needs to confirm choices).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="firstButtonText"/> = Text displayed on the first Button</para>
        /// <para><paramref name="firstButtonIcon"/> = Icon displayed on the first Button</para>
        /// <para><paramref name="firstButtonOnClick"/> = UnityEvent invoked when the first Button is pressed</para>
        /// <para><paramref name="secondButtonText"/> = Text displayed on the second Button</para>
        /// <para><paramref name="secondButtonIcon"/> = Icon displayed on the second Button</para>
        /// <para><paramref name="secondButtonOnClick"/> = UnityEvent invoked when the second Button is pressed</para>
        /// <para><paramref name="placeAtPositionOfPrevious"/> = Decide whether the Error Message should be placed at the location of the previous Dialog Menu</para>
        /// </summary>
        /// <returns>dialogMenuID (string) of the spawned dialog menu</returns>
        public string ShowErrorMenu_TwoButtons(string menuName, string menuText, string firstButtonText, Texture2D firstButtonIcon, UnityAction firstButtonOnClick, string secondButtonText, Texture2D secondButtonIcon, UnityAction secondButtonOnClick, bool placeAtPositionOfPrevious = false, bool showCloseMenuButton = false) {

            int previousPositionIndex;
            if (placeAtPositionOfPrevious)
                previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid
            else
                previousPositionIndex = -1; // place it near the Center Middle

            // Creates the "Error" dialog menu
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = errorMenuPrefab_twoButtons;
            dialogMenuInfo.showCloseMenuButton = showCloseMenuButton;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton firstButton = new TPI_DialogMenuButton();
            firstButton.buttonText = firstButtonText;
            if (firstButtonIcon == null)
                firstButton.buttonIcon = errorAcceptButtonIcon;
            else
                firstButton.buttonIcon = firstButtonIcon;
            firstButton.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            if (firstButtonOnClick != null) { // Give the option to add custom events -> call your own function
                UnityAction buttonEvent = firstButtonOnClick;
                firstButton.buttonOnClick.AddListener(buttonEvent); // Converts UnityEvent to UnityAction
            }
            dialogMenuInfo.dialogMenuButtons.Add(firstButton);


            TPI_DialogMenuButton secondButton = new TPI_DialogMenuButton();
            secondButton.buttonText = secondButtonText;
            if (secondButtonIcon == null)
                secondButton.buttonIcon = errorDeclineButtonIcon;
            else
                secondButton.buttonIcon = secondButtonIcon;
            secondButton.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            if (secondButtonOnClick != null) { // Give the option to add custom events -> call your own function
                UnityAction buttonEvent = secondButtonOnClick;
                secondButton.buttonOnClick.AddListener(buttonEvent); // Converts UnityEvent to UnityAction
            }
            dialogMenuInfo.dialogMenuButtons.Add(secondButton);

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);

            return dialogMenuInfo.dialogMenuID;

        }
        #endregion ErrorMenu_TwoButtons

        //---------------------------------------------------- Object Name Menu ----------------------------------------------------//


        #region ObjectNameMenu
        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select a Name for an Object (e.g. Snippet or Constraint)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type string) that will receive the name (value of the parameter). (e.g. <c>void selectName(string selectedText){}</c> can be passed in by simply using <c>selectName</c>)</para>
        /// </summary>
        public void ShowObjectNameMenu(UnityAction<string> performedAction) {
            ShowObjectNameMenu("Object Name", "Please enter the desired name of the object:", "Select Name", null, "Name: ", "The name cannot be null. Please use the keyboard field to enter a valid name.", performedAction, true, false);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select a Name for an Object (e.g. Snippet or Constraint)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type string) that will receive the name (value of the parameter). (e.g. <c>void selectName(string selectedText){}</c> can be passed in by simply using <c>selectName</c>)</para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowObjectNameMenu(UnityAction<string> performedAction, bool closeAllDialogMenus) {
            ShowObjectNameMenu("Object Name", "Please enter the desired name of the object:", "Select Name", null, "Name: ", "The name cannot be null. Please use the keyboard field to enter a valid name.", performedAction, true, closeAllDialogMenus);
        }


        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select a Name for an Object (e.g. Snippet or Constraint)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type string) that will receive the name (value of the parameter). (e.g. <c>void selectName(string selectedText){}</c> can be passed in by simply using <c>selectName</c>)</para>
        /// </summary>
        public void ShowObjectNameMenu(string menuName, string menuText, UnityAction<string> performedAction) {
            ShowObjectNameMenu(menuName, menuText, "Select Name", null, "Name: ", "The name cannot be null. Please use the keyboard field to enter a valid name.", performedAction, true, false);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select a Name for an Object (e.g. Snippet or Constraint)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type string) that will receive the name (value of the parameter). (e.g. <c>void selectName(string selectedText){}</c> can be passed in by simply using <c>selectName</c>)</para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowObjectNameMenu(string menuName, string menuText, UnityAction<string> performedAction, bool closeAllDialogMenus) {
            ShowObjectNameMenu(menuName, menuText, "Select Name", null, "Name: ", "The name cannot be null. Please use the keyboard field to enter a valid name.", performedAction, true, closeAllDialogMenus);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select a Name for an Object (e.g. Snippet or Constraint)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="buttonText"/> = Text displayed on the Button</para>
        /// <para><paramref name="buttonIcon"/> = Icon displayed on the Button</para>
        /// <para><paramref name="keyboardTitle"/> = Title shown next to the Keyboard Input Field</para>
        /// <para><paramref name="errorText_cannotBeNull"/> = Error Message that will be shown if the operator clicks on the Button without entering a name</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type string) that will receive the name (value of the parameter). (e.g. <c>void selectName(string selectedText){}</c> can be passed in by simply using <c>selectName</c>)</para>
        /// <para><paramref name="showCloseMenuButton"/> = Should there be a button in the title bar to close the dialog menu? </para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowObjectNameMenu(string menuName, string menuText, string buttonText, Texture2D buttonIcon, string keyboardTitle, string errorText_cannotBeNull, UnityAction<string> performedAction, bool showCloseMenuButton, bool closeAllDialogMenus) {

            int previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid

            if (closeAllDialogMenus && spawnedDialogMenus.Count != 0)
                ClearDialogMenus();

            // Create the Object Name dialog menu
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = objectNameMenuPrefab;
            dialogMenuInfo.showCloseMenuButton = showCloseMenuButton;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton button = new TPI_DialogMenuButton();
            button.buttonText = buttonText;
            if (buttonIcon == null)
                button.buttonIcon = objectNameButtonIcon;
            else
                button.buttonIcon = buttonIcon;
            button.buttonOnClick.AddListener(delegate { selectName(dialogMenuInfo.dialogMenuID, performedAction); });
            dialogMenuInfo.dialogMenuButtons.Add(button);

            dialogMenuInfo.keyboardInputFieldTitles.Add(keyboardTitle);

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);
        }

        /// <summary>
        /// Helper Function that gets called once the button in the object name menu is pressed
        /// <para><paramref name="dialogMenuID"/> = ID of the Dialog Menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type string) that will receive the name (value of the parameter). (e.g. <c>void selectName(string selectedText){}</c> can be passed in by simply using <c>selectName</c>)</para>
        /// </summary>
        private void selectName(string dialogMenuID, UnityAction<string> performedAction) {
            TPI_DialogMenuChoices choices = GetDialogMenuChoices(dialogMenuID);
            if (choices.keyboardTexts[0] == "") {
                ShowErrorMenu("Error", "The name of the snippet cannot be null. Please use the keyboard field to give the snippet a name.", "Confirm");
                return;
            }
            UnSpawnDialogMenu(dialogMenuID);
            performedAction.Invoke(choices.keyboardTexts[0]);
        }
        #endregion ObjectNameMenu



        //---------------------------------------------------- Constraint Type Selection Menu ----------------------------------------------------//



        #region ConstraintTypeMenu
        /// <summary>
        /// Creates a Dialog Menu that allows the operator to choose the type of a Constraint (whether it is globally active or snippet-specific)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowConstraintTypeMenu(UnityAction<TPI_ConstraintType, string> performedAction, bool closeAllDialogMenus) {
            ShowConstraintTypeMenu("Constraint Type", "Please select the Type of the Constraint:", "Global Constraint", null, "Snippet-Specific Constraint", null, performedAction, true, closeAllDialogMenus);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to choose the type of a Constraint (whether it is globally active or snippet-specific)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// </summary>
        public void ShowConstraintTypeMenu(UnityAction<TPI_ConstraintType, string> performedAction) {
            ShowConstraintTypeMenu("Constraint Type", "Please select the Type of the Constraint:", "Global Constraint", null, "Snippet-Specific Constraint", null, performedAction, true, false);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to choose the type of a Constraint (whether it is globally active or snippet-specific)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowConstraintTypeMenu(string menuName, string menuText, UnityAction<TPI_ConstraintType, string> performedAction, bool closeAllDialogMenus) {
            ShowConstraintTypeMenu(menuName, menuText, "Global Constraint", null, "Snippet-Specific Constraint", null, performedAction, true, closeAllDialogMenus);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to choose the type of a Constraint (whether it is globally active or snippet-specific)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// </summary>
        public void ShowConstraintTypeMenu(string menuName, string menuText, UnityAction<TPI_ConstraintType, string> performedAction) {
            ShowConstraintTypeMenu(menuName, menuText, "Global Constraint", null, "Snippet-Specific Constraint", null, performedAction, true, false);
        }


        /// <summary>
        /// Creates a Dialog Menu that allows the operator to choose the type of a Constraint (whether it is globally active or snippet-specific)
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Name displayed in the Title Bar of the Menu</para>
        /// <para><paramref name="menuText"/> = Text displayed in Menu</para>
        /// <para><paramref name="buttonGlobalText"/> = Text displayed on the Global Constraint Button</para>
        /// <para><paramref name="buttonGlobalIcon"/> = Icon displayed on the Global Constraint Button</para>
        /// <para><paramref name="buttonSpecificText"/> = Text displayed on the Snippet-Specific Constraint Button</para>
        /// <para><paramref name="buttonSpecificIcon"/> = Icon displayed on the Snippet-Specific Constraint Button</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// <para><paramref name="showCloseMenuButton"/> = Should there be a button in the title bar to close the dialog menu? </para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowConstraintTypeMenu(string menuName, string menuText, string buttonGlobalText, Texture2D buttonGlobalIcon, string buttonSpecificText, Texture2D buttonSpecificIcon, UnityAction<TPI_ConstraintType, string> performedAction, bool showCloseMenuButton, bool closeAllDialogMenus) {

            int previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid

            if (closeAllDialogMenus && spawnedDialogMenus.Count != 0)
                ClearDialogMenus();

            // Create the Constraint Type dialog menu
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = constraintTypeMenuPrefab;
            dialogMenuInfo.showCloseMenuButton = showCloseMenuButton;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton buttonGlobal = new TPI_DialogMenuButton();
            buttonGlobal.buttonText = buttonGlobalText;
            if (buttonGlobalIcon == null)
                buttonGlobal.buttonIcon = constraintTypeButtonIcon_global;
            else
                buttonGlobal.buttonIcon = buttonGlobalIcon;
            buttonGlobal.buttonOnClick.AddListener(delegate { performedAction.Invoke(TPI_ConstraintType.global, ""); });
            buttonGlobal.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            dialogMenuInfo.dialogMenuButtons.Add(buttonGlobal);

            TPI_DialogMenuButton buttonSpecific = new TPI_DialogMenuButton();
            buttonSpecific.buttonText = buttonSpecificText;
            if (buttonSpecificIcon == null)
                buttonSpecific.buttonIcon = constraintTypeButtonIcon_specific;
            else
                buttonSpecific.buttonIcon = buttonSpecificIcon;
            buttonSpecific.buttonOnClick.AddListener(delegate { OpenSnippetSelectionMenu(performedAction); });
            buttonSpecific.buttonOnClick.AddListener(delegate { UnSpawnDialogMenu(dialogMenuInfo.dialogMenuID); });
            dialogMenuInfo.dialogMenuButtons.Add(buttonSpecific);

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);
        }

        /// <summary>
        /// Helper Function that gets called if the operator decides to create a snippet-specific constraint 
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// </summary>
        private void OpenSnippetSelectionMenu(UnityAction<TPI_ConstraintType, string> performedAction) {

            if (mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetSnippetSequenceLength() == 0) {
                ShowErrorMenu("Error", "There must be at least one active snippet. Please create a snippet before you try to create a snippet-specific Constraint.", "Confirm");
                return;
            }

            int previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid

            // Create the Snippet selection dialog menu, treated different than the "Dropdown Selection Dialog Menu" implemented further down in this script as it also has to handle the TPI_ConstraintType type
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = "Snippet Selection";
            dialogMenuInfo.dialogMenuPrefab = dropdownSelectionMenuPrefab;
            dialogMenuInfo.showCloseMenuButton = true;

            dialogMenuInfo.dialogMenuTexts.Add("Please select the Snippet to which the created Constraint should be applied to.");

            TPI_DialogMenuButton button = new TPI_DialogMenuButton();
            button.buttonText = "Select Snippet";
            button.buttonIcon = dropdownSelectionButtonIcon;
            button.buttonOnClick.AddListener(delegate { SelectSnippet(performedAction, dialogMenuInfo.dialogMenuID); });
            dialogMenuInfo.dialogMenuButtons.Add(button);

            // Create the correct List of options for the Dropdown
            List<TPI_DialogMenuDropDown.OptionData> dropdownList = new List<TPI_DialogMenuDropDown.OptionData>();
            TPI_SequenceMenuController sequenceMenuController = mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>();
            for (int i = 0; i < sequenceMenuController.GetSnippetSequenceLength(); i++) {
                string positionText = (i + 1).ToString();
                if (positionText.Length == 1)
                    positionText = "0" + positionText;
                dropdownList.Add(new TPI_DialogMenuDropDown.OptionData(positionText + " |  " + sequenceMenuController.GetSnippetAt(i).snippetInformation.snippetName, sequenceMenuController.GetSnippetAt(i).snippetInformation.snippetIcon));
            }
            dialogMenuInfo.dialogMenuDropdowns.Add(new TPI_DialogMenuDropDown("Created Snippets:", dropdownList));

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);
        }

        /// <summary>
        /// Helper Function that gets called once the designated snippet for the snippet-specific constraint was selected
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only two parameter of type TPI_ConstraintType and string) that will receive the constraint type and snippetID (value of the parameters). (e.g. <c>void selectType(TPI_ConstraintType selectedText, string selectedSnippet){}</c> can be passed in by simply using <c>selectType</c>)</para>
        /// <para><paramref name="dialogMenuID"/> = ID of the active Dialog Menu</para>
        /// </summary>
        private void SelectSnippet(UnityAction<TPI_ConstraintType, string> performedAction, string dialogMenuID) {
            TPI_DialogMenuChoices choices = GetDialogMenuChoices(dialogMenuID);
            UnSpawnDialogMenu(dialogMenuID);
            performedAction.Invoke(TPI_ConstraintType.snippetSpecific, mainController.sequenceMenu.GetComponent<TPI_SequenceMenuController>().GetSnippetAt(choices.dropdown[0]).snippetInformation.snippetID);
        }
        #endregion ConstraintTypeMenu



        //---------------------------------------------------- Dropdown Selection Dialog Menu ----------------------------------------------------//



        #region DropdownMenu
        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="dropdownOptions"/> = List of possible selection options shown in the dialog menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// </summary>
        public void ShowDropdownSelectionMenu(List<string> dropdownOptions, UnityAction<int> performedAction) {
            ShowDropdownSelectionMenu("Dropdown Selection", "Please select one item of the following list:", "Select Item", null, "Options:", dropdownOptions, performedAction, true, false);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="dropdownOptions"/> = List of possible selection options shown in the dialog menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowDropdownSelectionMenu(List<string> dropdownOptions, UnityAction<int> performedAction, bool closeAllDialogMenus) {
            ShowDropdownSelectionMenu("Dropdown Selection", "Please select one item of the following list:", "Select Item", null, "Options:", dropdownOptions, performedAction, true, closeAllDialogMenus);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="dropdownTitle"/> = Title of the Dropdown Menu</para>
        /// <para><paramref name="dropdownOptions"/> = List of possible selection options shown in the dialog menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// </summary>
        public void ShowDropdownSelectionMenu(string dropdownTitle, List<string> dropdownOptions, UnityAction<int> performedAction) {
            ShowDropdownSelectionMenu("Dropdown Selection", "Please select one item of the following list:", "Select Item", null, dropdownTitle, dropdownOptions, performedAction, true, false);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="dropdownTitle"/> = Title of the Dropdown Menu</para>
        /// <para><paramref name="dropdownOptions"/> = List of possible selection options shown in the dialog menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowDropdownSelectionMenu(string dropdownTitle, List<string> dropdownOptions, UnityAction<int> performedAction, bool closeAllDialogMenus) {
            ShowDropdownSelectionMenu("Dropdown Selection", "Please select one item of the following list:", "Select Item", null, dropdownTitle, dropdownOptions, performedAction, true, closeAllDialogMenus);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Text shown in Title Bar of the Dialog Menu</para>
        /// <para><paramref name="menuText"/> = Text shown in the Dialog Menu</para>
        /// <para><paramref name="buttonText"/> =Text shown on the Button in the Dialog Menu</para>
        /// <para><paramref name="buttonIcon"/> = Icon visible on the Button in the Dialog Menu</para>
        /// <para><paramref name="dropdownTitle"/> = Title of the Dropdown Menu</para>
        /// <para><paramref name="dropdownOptions"/> = List of possible selection options shown in the dialog menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// <para><paramref name="showCloseMenuButton"/> = Should there be a button in the title bar to close the dialog menu? </para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowDropdownSelectionMenu(string menuName, string menuText, string buttonText, Texture2D buttonIcon, string dropdownTitle, List<string> dropdownOptions, UnityAction<int> performedAction, bool showCloseMenuButton, bool closeAllDialogMenus) {

            // Create the correct List of options for the Dropdown
            List<TPI_DialogMenuDropDown.OptionData> dropdownList = new List<TPI_DialogMenuDropDown.OptionData>();
            for (int i = 0; i < dropdownOptions.Count; i++) {
                dropdownList.Add(new TPI_DialogMenuDropDown.OptionData(dropdownOptions[i], null));
            }
            ShowDropdownSelectionMenu(menuName, menuText, buttonText, buttonIcon, dropdownTitle, dropdownList, performedAction, showCloseMenuButton, closeAllDialogMenus);
        }

        /// <summary>
        /// Creates a Dialog Menu that allows the operator to select an item from a list of strings (styled as a dropdown).
        /// <br></br>The Menu will be closed automatically when the Button is pressed
        /// <para><paramref name="menuName"/> = Text shown in Title Bar of the Dialog Menu</para>
        /// <para><paramref name="menuText"/> = Text shown in the Dialog Menu</para>
        /// <para><paramref name="buttonText"/> =Text shown on the Button in the Dialog Menu</para>
        /// <para><paramref name="buttonIcon"/> = Icon visible on the Button in the Dialog Menu</para>
        /// <para><paramref name="dropdownTitle"/> = Title of the Dropdown Menu</para>
        /// <para><paramref name="dropdownOptions"/> = List of possible selection options shown in the dialog menu</para>
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// <para><paramref name="showCloseMenuButton"/> = Should there be a button in the title bar to close the dialog menu? </para>
        /// <para><paramref name="closeAllDialogMenus"/> = true -> Close all currently opened dialog menus, false -> nothing happens</para>
        /// </summary>
        public void ShowDropdownSelectionMenu(string menuName, string menuText, string buttonText, Texture2D buttonIcon, string dropdownTitle, List<TPI_DialogMenuDropDown.OptionData> dropdownOptions, UnityAction<int> performedAction, bool showCloseMenuButton, bool closeAllDialogMenus) {

            int previousPositionIndex = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().GetPreviousPositionIndex(); // Position of the last removed Dialog Menu in the Grid

            if (closeAllDialogMenus && spawnedDialogMenus.Count != 0)
                ClearDialogMenus();

            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = menuName;
            dialogMenuInfo.dialogMenuPrefab = dropdownSelectionMenuPrefab;
            dialogMenuInfo.showCloseMenuButton = showCloseMenuButton;

            dialogMenuInfo.dialogMenuTexts.Add(menuText);

            TPI_DialogMenuButton button = new TPI_DialogMenuButton();
            button.buttonText = buttonText;
            if (buttonIcon == null)
                button.buttonIcon = dropdownSelectionButtonIcon;
            else
                button.buttonIcon = buttonIcon;
            button.buttonOnClick.AddListener(delegate { GetDropdownSelection(performedAction, dialogMenuInfo.dialogMenuID); });
            dialogMenuInfo.dialogMenuButtons.Add(button);

            dialogMenuInfo.dialogMenuDropdowns.Add(new TPI_DialogMenuDropDown(dropdownTitle, dropdownOptions));

            SpawnDialogMenu(dialogMenuInfo, previousPositionIndex);

        }

        /// <summary>
        /// Helper Function that gets called once the button in the Dropdown Selection Dialog Menu was pressed
        /// <para><paramref name="performedAction"/> = Pass in the function (can have only one parameter of type int) that will receive the index of the selected item (value of the parameter). (e.g. <c>void selectItem(int selectedIndex){}</c> can be passed in by simply using <c>selectItem</c>)</para>
        /// <para><paramref name="dialogMenuID"/> = ID of the active Dialog Menu</para>
        /// </summary>
        private void GetDropdownSelection(UnityAction<int> performedAction, string dialogMenuID) {
            TPI_DialogMenuChoices choices = GetDialogMenuChoices(dialogMenuID);
            UnSpawnDialogMenu(dialogMenuID);
            performedAction.Invoke(choices.dropdown[0]);
        }
        #endregion DropdownMenu

    }

}

namespace TaskPlanningInterface.DialogMenu {

    #region HelperClasses

    /// <summary>
    /// <para>
    /// The aim of this script is to store and display the main information of the respective dialog menus and thereby allowing it to be saved and passed between different C# scripts.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.DialogMenu" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// <br></br>On the other hand, if you want to add additional dialog menu options, this can be achieved with the help of this class.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [System.Serializable]
    public class TPI_DialogMenuInformation {

        // General Variables

        [Tooltip("Name of the Dialog Menu")]
        public string dialogMenuName;
        [Tooltip("Add the Dialog Menu Template prefab that should be instantiated.")]
        public GameObject dialogMenuPrefab;
        [TextArea][Tooltip("Enter the texts that you want to be displayed. The indices of the texts correspond to the fields shown in the template prefab.")]


        // Replacement Options

        public List<string> dialogMenuTexts;
        [Tooltip("Enter the texts that you want to be displayed. The indices of the texts correspond to the fields shown in the template prefab.")]
        public List<TPI_DialogMenuButton> dialogMenuButtons;
        [Tooltip("Enter the titles of the keyboard fields you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [KeyboardInput0].")]
        public List<string> keyboardInputFieldTitles;

        [Tooltip("Enter the titles of the checkboxes you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [CheckBox0].")]
        public List<string> checkboxTitles;
        [Tooltip("Enter the titles of the toggles you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [Toggle0].")]
        public List<string> toggleTitles;
        [Tooltip("Enter the data of the dropdown menus you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [Dropdown0].")]
        public List<TPI_DialogMenuDropDown> dialogMenuDropdowns;
        [Tooltip("Enter the titles of the point selection fields you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [PointSelection0].")]
        public List<TPI_DialogMenuPointSelection> pointSelections;

        /*[Tooltip("Enter the titles of the object selection fields you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [ObjectSelection0].")]
        public List<string> objectSelectionTitles;
        [Tooltip("Enter the titles of the surface selection fields you want to be displayed. The indices of the titles correspond to the fields shown in the template prefab e.g. [SurfaceSelection0].")]
        public List<string> surfaceSelectionTitles;*/

        [Tooltip("Should there be a button in the title bar to close the dialog menu?")]
        public bool showCloseMenuButton;

        // Dialog Menu ID

        [UniqueID(true)][Tooltip("Dialog Menu ID is automatically setup by the system")]
        public string dialogMenuID;

        public TPI_DialogMenuInformation() {

            this.dialogMenuName = string.Empty;
            this.dialogMenuPrefab = null;
            this.dialogMenuTexts = new List<string>();
            this.dialogMenuButtons = new List<TPI_DialogMenuButton>();
            this.keyboardInputFieldTitles = new List<string>();
            this.checkboxTitles = new List<string>();
            this.toggleTitles = new List<string>();
            this.dialogMenuDropdowns = new List<TPI_DialogMenuDropDown>();
            this.pointSelections = new List<TPI_DialogMenuPointSelection>();

            Guid guid = Guid.NewGuid();
            this.dialogMenuID = guid.ToString();
            this.showCloseMenuButton = false;

        }

        public TPI_DialogMenuInformation(string _dialogMenuName, GameObject _dialogMenuPrefab, List<string> _dialogMenuTexts, List<TPI_DialogMenuButton> _dialogMenuButtons, List<string> _keyboardInputFieldTitles,
            List<string> _checkboxTitles, List<string> _toggleTitles, List<TPI_DialogMenuDropDown> _dialogMenuDropdowns, List<TPI_DialogMenuPointSelection> _pointSelections) {

            this.dialogMenuName = _dialogMenuName;
            this.dialogMenuPrefab = _dialogMenuPrefab;
            this.dialogMenuTexts = _dialogMenuTexts;
            this.dialogMenuButtons = _dialogMenuButtons;
            this.keyboardInputFieldTitles = _keyboardInputFieldTitles;
            this.checkboxTitles = _checkboxTitles;
            this.toggleTitles = _toggleTitles;
            this.dialogMenuDropdowns = _dialogMenuDropdowns;
            this.pointSelections = _pointSelections;

            Guid guid = Guid.NewGuid();
            this.dialogMenuID = guid.ToString();
            this.showCloseMenuButton = false;

        }

        public TPI_DialogMenuInformation(string _dialogMenuName, GameObject _dialogMenuPrefab, List<string> _dialogMenuTexts, List<TPI_DialogMenuButton> _dialogMenuButtons, List<string> _keyboardInputFieldTitles,
            List<string> _checkboxTitles, List<string> _toggleTitles, List<TPI_DialogMenuDropDown> _dialogMenuDropdowns, List<TPI_DialogMenuPointSelection> _pointSelections, bool _showCloseMenuButton) {

            this.dialogMenuName = _dialogMenuName;
            this.dialogMenuPrefab = _dialogMenuPrefab;
            this.dialogMenuTexts = _dialogMenuTexts;
            this.dialogMenuButtons = _dialogMenuButtons;
            this.keyboardInputFieldTitles = _keyboardInputFieldTitles;
            this.checkboxTitles = _checkboxTitles;
            this.toggleTitles = _toggleTitles;
            this.dialogMenuDropdowns = _dialogMenuDropdowns;
            this.pointSelections = _pointSelections;

            Guid guid = Guid.NewGuid();
            this.dialogMenuID = guid.ToString();
            this.showCloseMenuButton = _showCloseMenuButton;

        }

    }

    /// <summary>
    /// <para>
    /// This is a helper script that holds the information of a button that will be placed inside the dialog menus.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.DialogMenu" without the quotes.
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
    public class TPI_DialogMenuButton {
        [Tooltip("Text displayed on the button")]
        public string buttonText;
        [Tooltip("Icon displayed alongside the text on the button (optional as there is a standard icon)")][Rename("Button Icon (optional)")]
        public Texture2D buttonIcon;
        [Tooltip("Add what should happen once the operator clicks on the button")]
        public UnityEvent buttonOnClick;

        public TPI_DialogMenuButton(string _buttonText, Texture2D _buttonIcon, UnityEvent _buttonOnClick) {
            this.buttonText = _buttonText;
            this.buttonIcon = _buttonIcon;
            this.buttonOnClick = _buttonOnClick;
        }

        public TPI_DialogMenuButton() {
            this.buttonText = string.Empty;
            this.buttonIcon = null;
            this.buttonOnClick = new UnityEvent();
        }
    }

    /// <summary>
    /// <para>
    /// This is a helper script that holds the information of a dropdown menu that will be placed inside the dialog menus.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.DialogMenu" without the quotes.
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
    public class TPI_DialogMenuDropDown {
        [Tooltip("Text displayed next to the Dropdown Menu")]
        public string dropdownTitle;
        [Tooltip("Add which options should be available in the dropdown menu")]
        public List<TPI_DialogMenuDropDown.OptionData> dropdownOptions;

        public TPI_DialogMenuDropDown(string _dropDownTitle, List<TPI_DialogMenuDropDown.OptionData> _options) {
            this.dropdownTitle = _dropDownTitle;
            this.dropdownOptions = _options;
        }

        public TPI_DialogMenuDropDown() {
            this.dropdownTitle = string.Empty;
            this.dropdownOptions = new List<TPI_DialogMenuDropDown.OptionData>();
        }
        /// <summary>
        /// <para>
        /// This is a helper class to store the text and/or image of a single option in the dropdown list.
        /// </para>
        /// 
        /// <para>
        /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.DialogMenu" without the quotes.
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
        public class OptionData {

            [Tooltip("Text displayed in the Dropdown")]
            public string text;
            [Tooltip("Image displayed in the Dropdown")]
            public Texture2D image;

            public OptionData(string _text, Texture2D _image) {
                this.text = _text;
                this.image = _image;
            }

            public OptionData() {
                this.text = string.Empty;
                this.image = null;
            }
        }

    }

    /// <summary>
    /// <para>
    /// This is a helper script that holds the information of a point selection button that will be placed inside the dialog menus.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.DialogMenu" without the quotes.
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
    public class TPI_DialogMenuPointSelection {

        [Tooltip("Text displayed on the button")]
        public string buttonText;
        [Tooltip("Icon displayed alongside the text on the button (optional as there is a standard icon)")][Rename("Button Icon (optional)")]
        public Texture2D buttonIcon;
        [HideInInspector]
        public GameObject selectionSphere;

        public TPI_DialogMenuPointSelection(string _buttonText, Texture2D _buttonIcon) {
            this.buttonText = _buttonText;
            this.buttonIcon = _buttonIcon;
            this.selectionSphere = null;
        }

        public TPI_DialogMenuPointSelection() {
            this.buttonText = string.Empty;
            this.buttonIcon = null;
            this.selectionSphere = null;
        }
    }

    /// <summary>
    /// <para>
    /// This is a helper script that holds the information of the changes made to the dialog menu during runntime due to for example keyboard input.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.DialogMenu" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// <br></br>On the other hand, if you want to add additional dialog menu options that require their choices to be passed along, this can be achieved with the help of this class.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [System.Serializable]
    public class TPI_DialogMenuChoices {

        [Tooltip("List of the texts that were typed in by the operator in keyboard input fields")][ReadOnly]
        public List<string> keyboardTexts;

        [Tooltip("List of the bools that were setup due to checkboxes")][ReadOnly]
        public List<bool> checkboxes;

        [Tooltip("List of the bools that were setup due to toggles")][ReadOnly]
        public List<bool> toggles;

        [Tooltip("List of the strings that were selected in dropdown menus")][ReadOnly]
        public List<int> dropdown;

        [Tooltip("List of the Points that were selected in the environment")][ReadOnly]
        public List<Vector3> selectedPoints;

        /*[Tooltip("List of the GameObjects that were selected in the environment using spatial awareness")][ReadOnly]
        public List<GameObject> selectedObjects;

        [Tooltip("List of the Surfaces that were selected in the environment using spatial awareness")][ReadOnly]
        public List<GameObject> selectedSurfaces;*/

        public TPI_DialogMenuChoices() {
            keyboardTexts = new List<string>();
            checkboxes = new List<bool>();
            toggles = new List<bool>();
            dropdown = new List<int>();
            selectedPoints = new List<Vector3>();
            /*selectedObjects = new List<GameObject>();
            selectedSurfaces = new List<GameObject>();*/
        }
    }
    #endregion HelperClasses

}