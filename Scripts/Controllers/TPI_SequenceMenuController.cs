using System.Collections.Generic;
using UnityEngine;
using TaskPlanningInterface.Workflow;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using TaskPlanningInterface.Helper;
using System.Threading.Tasks;
using System.Collections;
using TaskPlanningInterface.EditorAndInspector;
using System.Linq;
using TMPro;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    ///<para>
    /// The aim of this script is to control the sequence menu, especially the snippets and the constraints sequences.
    /// <br></br>You can utilise various functions to access, alter and visualize them.
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

    public class TPI_SequenceMenuController : MonoBehaviour {

        // General Sequence Menu Variables
        [Tooltip("Information of all the Snippets of the current Workflow sorted by their correct position in the Sequence. You cannot make any alterations in this list as this should only provide you with the information whether everything has been created properly.")][ReadOnly]
        public List<TPI_Snippet> _snippetObjects; // Sequence of the TPI_Snippet scripts
        [Tooltip("Information of all the Constraints (both global and snippet-specific) of the current Workflow. You cannot make any alterations in this list as this should only provide you with the information whether everything has been created properly.")][ReadOnly]
        public List<TPI_Constraint> _constraintObjects; // List of the TPI_Constraint scripts (both global and snippet-specific)

        private List<GameObject> _visualSnippetObjects; // Sequence of the visible snippet object added to the sequence menu
        private List<GameObject> _visualConstraintObjects; // List of the visible constraint object added to the sequence menu (both global and snippet-specific)

        [Tooltip("Add the Prefab of a Snippet Sequence Menu Button (whatever should be shown in the Sequence Menu)")]
        [SerializeField] private GameObject snippetPrefab;
        [Tooltip("Add the Prefab of a Constraint Sequence Menu Button (whatever should be shown in the Sequence Menu)")]
        [SerializeField] private GameObject constraintPrefab;

        [Tooltip("Add the GameObject from the Editor in which the Snippet Buttons will be instantiated in. If you did not change anything, it should be the SnippetGridObjectCollection GameObject found at: Sequence_Menu/BodyContent/SnippetDynamicScrollPopulator")]
        [SerializeField] private GameObject snippetContainerPath;
        [Tooltip("Add the GameObject from the Editor in which the Global Constraint buttons will be instantiated in. If you did not change anything, it should be the GlobalConstraintsGridObjectCollection GameObject found at: Sequence_Menu/BodyContent/GlobalConstraintDynamicScrollPopulator")]
        [SerializeField] private GameObject globalConstraintContainerPath;
        [Tooltip("Add the GameObject from the Editor in which the Snippet-Specific Constraint buttons will be instantiated in. If you did not change anything, it should be the SpecificConstraintsGridObjectCollection GameObject found at: Sequence_Menu/BodyContent/SpecificConstraintDynamicScrollPopulator")]
        [SerializeField] private GameObject specificConstraintContainerPath;

        // Selection of Snippets -> save index, -1 -> no snippet selected
        private int selectedSnippet;

        // Options for Drag and Drop + Deletion
        [Tooltip("Time given to the operator in order to perform a double click on a Snippet in seconds")]
        public float standardButtonPressedCooldown = 4f;
        [Tooltip("Time it takes for Unity to register the OnHold Event (the smaller the value, the faster)")]
        public float buttonHoldTime = 0.5f;
        [Tooltip("Decide whether the operator has to confirm his choice of changing the values of the underlying variables of a snippet or constraint.")]
        public bool requireVariableChangeConfirmation = true;
        [Tooltip("Decide whether the operator has to confirm his choice of deleting a snippet or constraint.")]
        public bool requireDeletionConfirmation = true;

        // Sequence Function Variables

        [Tooltip("This enum states the current status of the Sequence. You cannot change the value manually.")][ReadOnly][Rename("Current Status of Sequence:")]
        public SequenceState _sequenceState;
        private int currentStep = 0;

        [Tooltip("Determine what should happen to the next snippet after a snippet has finished.")]
        [SerializeField] private SnippetProgression snippetProgression = SnippetProgression.automatically;
        [SerializeField] private float snippetProgressionDelay = 1f;

        [Tooltip("Determine how fast the simulation should be running if the operator decides to visualize the snippet sequence.")][Range(0.1f, 5f)]
        [SerializeField] private float snippetVisualizationSpeed = 1f;
        private bool isSnippetVisualizationActive = false;
        private bool isConstraintVisualizationActive = false;

        [Tooltip("Add the GameObject from the Editor in which the Coroutine Buttons are located in.  If you did not change anything, it should be the SpecificConstraintsGridObjectCollection GameObject found at: SequenceFunctions_Menu/SequenceButtonCollection")]
        [SerializeField] private GameObject coroutineButtonsContainerPath;

        [Tooltip("Add the Icon that should be shown on the Visualize Snippet Button of the Sequence Functions")]
        [SerializeField] private Texture2D snippetVisualizationIcon;
        [Tooltip("Add the Icon that should be shown on the Visualize Constraints Button of the Sequence Functions")]
        [SerializeField] private Texture2D constraintVisualizationIcon;
        [Tooltip("Add the Icon that should be shown on either the Visualize Snippet or Visualize Constraints Button in order to stop the visualization")]
        [SerializeField] private Texture2D stopVisualizationIcon;

        [Tooltip("Add the Icon that should be shown on the Pause Button of the Sequence Functions")]
        [SerializeField] private Texture2D PauseIcon = null;
        [Tooltip("Add the Icon that should be shown on the Unpause Button of the Sequence Functions")]
        [SerializeField] private Texture2D UnpauseIcon = null;
        [HideInInspector]
        public static bool isPaused = false;

        private Coroutine sequenceCoroutine = null;

        // Reference to TPI_MainController Component
        private TPI_MainController mainController;
        private TPI_ROSController rosController;



        //---------------------------------------------------- General Functions ----------------------------------------------------//



        #region GeneralFunctions
        private void Start() {

            mainController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>();
            rosController = mainController.rosController.GetComponent<TPI_ROSController>();

            if(!rosController.IsROSConnectionDeactivated()) {
                rosController.RegisterShouldExecuteInstructionsPublisher();
                rosController.RegisterVisualizationSpeedPublisher();
            }

            // Check that the prefabs have been setup correctly
            if (snippetPrefab == null)
                Debug.LogError("No snippet prefab was assigned in the SequenceMenuController component in " + transform.name);
            if (constraintPrefab == null)
                Debug.LogError("No constraint prefab was assigned in the SequenceMenuController component in " + transform.name);

            _snippetObjects = new List<TPI_Snippet>();
            _constraintObjects = new List<TPI_Constraint>();
            _visualSnippetObjects = new List<GameObject>();
            _visualConstraintObjects = new List<GameObject>();
            _sequenceState = SequenceState.notStarted;

            // Lock Coroutine Buttons
            SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, true); // Lock Start Button
            SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(3).gameObject, true); // Lock Stop Button
            SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(4).gameObject, true); // Lock Repeat Button
            SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, true); // Lock Skip Button

            // == -1 if no snippet has ever been selected
            selectedSnippet = -1;
            specificConstraintContainerPath.transform.GetChild(4).gameObject.SetActive(false); // Disable the SelectedSnippetText of the specific snippets

            // Used for Scrolling & Masking of the List
            MakeScrollingList(snippetContainerPath.transform, 0.144f, 0.032f, 0.032f, 1, 6);
            MakeScrollingList(globalConstraintContainerPath.transform, 0.128f, 0.032f, 0.032f, 1, 6);
            MakeScrollingList(specificConstraintContainerPath.transform, 0.128f, 0.032f, 0.032f, 1, 6);
        }

        /// <summary>
        /// This IEnumerator fixes a problem with the UpdateCollection function of the GridObjectCollection script, moving the update to the next frame.
        /// <br></br>Furthermore, it also updates the ScrollingObjectCollection and the vsual Snippet Object if needed.
        /// <para><paramref name="snippetObject"/> = Snippet Object that needs to be updated due to the changes made by the masking of the scrolling object collectionc</para>
        /// <para><paramref name="gridObjectCollection"/> = Grid Object Collection that needs to be updated</para>
        /// <para><paramref name="scrollingObjectCollection"/> = Scrolling Object Collection that needs to be updated</para>
        /// </summary>
        private IEnumerator InvokeUpdateCollection(GameObject snippetObject, GridObjectCollection gridObjectCollection, ScrollingObjectCollection scrollingObjectCollection) {
            yield return new WaitForEndOfFrame();
            gridObjectCollection.UpdateCollection();
            if (scrollingObjectCollection != null) {
                yield return new WaitForEndOfFrame();
                scrollingObjectCollection.UpdateContent();
                if (snippetObject != null) {
                    yield return new WaitForEndOfFrame();
                    snippetObject.transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>().enabled = false;
                    snippetObject.transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// This function resets the sequence menu to it's initial state (it will be like it was when the TPI was started for the first time)
        /// </summary>
        public void ResetSequenceMenu() {

            if (_snippetObjects.Count != 0 && (_sequenceState == SequenceState.running || _sequenceState == SequenceState.paused))
                StopSequence();

            // Change Restart Button to Stop Button
            coroutineButtonsContainerPath.transform.GetChild(3).GetChild(0).gameObject.SetActive(true); // Show Stop Button
            coroutineButtonsContainerPath.transform.GetChild(3).GetChild(3).gameObject.SetActive(false); // Hide Restart Button

            //Lock Stop Button
            SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(3).gameObject, true);

            ClearSnippets();
            ClearConstraints();

            _sequenceState = SequenceState.notStarted;
            currentStep = 0;
        }

        /// <summary>
        /// This function creates and configures a new ScrollingObjectCollection.
        /// <br></br>adapted from: ScrollableListPopulator script of the ScrollingObjectCollection Example
        /// <para><paramref name="objectContainer"/> = Transform in which the Scrolling Object Collection should be created</para>
        /// <para><paramref name="cellWidth"/> = Width of the List Cells</para>
        /// <para><paramref name="cellHeight"/> = Height of the List Cells</para>
        /// <para><paramref name="cellDepth"/> = Depth of the List Cells</para>
        /// <para><paramref name="cellsPerTier"/> = Number of Cells per List Row</para>
        /// <para><paramref name="tiersPerPage"/> = Number of Rows that should be visible in the Scrolling Object Collection before masking sets in</para>
        /// </summary>
        private void MakeScrollingList(Transform objectContainer, float cellWidth, float cellHeight, float cellDepth, int cellsPerTier, int tiersPerPage) {

            // Setup ScrollingObjectCollection
            GameObject newScroll = new GameObject("ScrollingObjectCollection");
            newScroll.transform.parent = objectContainer.Find("ScrollParent");
            newScroll.transform.localPosition = Vector3.zero;
            newScroll.transform.localRotation = Quaternion.identity;
            ScrollingObjectCollection scrollView = newScroll.AddComponent<ScrollingObjectCollection>();

            scrollView.CellWidth = cellWidth;
            scrollView.CellHeight = cellHeight;
            scrollView.CellDepth = cellDepth;
            scrollView.CellsPerTier = cellsPerTier;
            scrollView.TiersPerPage = tiersPerPage;
            scrollView.MaskEnabled = true;

            // Setup GridObjectCollection
            GameObject collectionGameObject = new GameObject("GridObjectCollection");
            collectionGameObject.transform.position = scrollView.transform.position;
            collectionGameObject.transform.rotation = scrollView.transform.rotation;

            GridObjectCollection gridObjectCollection = collectionGameObject.AddComponent<GridObjectCollection>();
            gridObjectCollection.CellWidth = cellWidth;
            gridObjectCollection.CellHeight = cellHeight;
            gridObjectCollection.SortType = CollationOrder.ChildOrder;
            gridObjectCollection.SurfaceType = ObjectOrientationSurfaceType.Plane;
            gridObjectCollection.Layout = LayoutOrder.ColumnThenRow;
            gridObjectCollection.Columns = cellsPerTier;
            gridObjectCollection.Anchor = LayoutAnchor.UpperLeft;

            scrollView.AddContent(collectionGameObject);
        }

        /// <summary>
        /// This function removes an item from a specified ScrollingObjectCollection.
        /// <br></br>adapted from: ScrollableListPopulator script of the ScrollingObjectCollection Example
        /// <para><paramref name="item"/> = Item that should be removed</para>
        /// <para><paramref name="scrollingObjectCollection"/> = The Scrolling Object Collection from which the item should be removed from</para>
        /// </summary>
        private void RemoveScrollingObject(GameObject item, ScrollingObjectCollection scrollingObjectCollection) {
            scrollingObjectCollection.RemoveItem(item);
            StartCoroutine(InvokeUpdateCollection(null, scrollingObjectCollection.GetComponentInChildren<GridObjectCollection>(), null));
        }
        #endregion GeneralFunctions



        //---------------------------------------------------- Snippets ----------------------------------------------------//



        #region Snippets
        /// <summary>
        /// Allows you to get a snippet at a specific position (index starts at 0).
        /// </summary>
        /// <returns>Snippet at specified positon (TPI_Snippet)</returns>
        public TPI_Snippet GetSnippetAt(int position) {
            return _snippetObjects[position];
        }

        /// <summary>
        /// Allows you to get the entire snippet function sequence (not the visual objects spawned in the sequence menu though).
        /// </summary>
        /// <returns>The whole Sequence of Snippets (List of TPI_Snippet)</returns>
        public List<TPI_Snippet> GetSnippetSequence() {
            return _snippetObjects;
        }

        /// <summary>
        /// Allows you to get the snippet count in the sequence list.
        /// </summary>
        /// <returns>Amount of Snippets in Snippet Sequence (int)</returns>
        public int GetSnippetSequenceLength() {
            return _snippetObjects.Count;
        }

        /// <summary>
        /// Allows you to add a snippet at the end of the sequence list by providing the snippet function component.
        /// </summary>
        public void AddSnippet(TPI_Snippet snippet) {
            if (snippet == null) {
                Debug.LogError("The snippet cannot be null! (AddSnippet in TPI_SequenceMenuController)");
                return;
            }
            snippet.gameObject.name = "Snippet Function: " + snippet.snippetInformation.snippetName;
            _snippetObjects.Add(snippet);
            InstantiateVisualSnippetObject(snippet);
        }

        /// <summary>
        /// Allows you to insert a snippet at a specific position in the sequence list by providing the snippet function component and the position.
        /// </summary>
        public void InsertSnippetAt(TPI_Snippet snippet, int position) {
            if (snippet == null) {
                Debug.LogError("The snippet cannot be null! (InsertSnippetAt in TPI_SequenceMenuController)");
                return;
            }
            snippet.gameObject.name = snippet.snippetInformation.snippetName;
            _snippetObjects.Insert(position, snippet);
            InstantiateVisualSnippetObject(snippet, position);
        }

        /// <summary>
        /// Allows you to remove a snippet from the sequence list by providing the snippet function component.
        /// </summary>
        public void RemoveSnippet(int snippetIndex) {
            RemoveSnippet(_snippetObjects[snippetIndex]);
        }

        /// <summary>
        /// Allows you to remove a snippet from the sequence list by providing the snippet function component.
        /// </summary>
        public void RemoveSnippet(TPI_Snippet snippet) {

            if (snippet == null) {
                Debug.LogError("The snippet cannot be null! (RemoveSnippet in TPI_SequenceMenuController)");
                return;
            }

            if (!_snippetObjects.Contains(snippet)) {
                Debug.LogError("The snippet sequence list does not contain this snippet! (RemoveSnippet in TPI_SequenceMenuController)");
                return;
            }

            _snippetObjects.Remove(snippet);
            DeleteVisualSnippetObject(snippet.snippetInformation.snippetID);
            Destroy(snippet.gameObject);

        }

        /// <summary>
        /// Allows you to remove a snippet from the sequence list by providing the snippet ID.
        /// </summary>
        public void RemoveSnippet(string snippetID) {

            if (snippetID == "") {
                Debug.LogError("The snippetID cannot be empty! (RemoveSnippet in TPI_SequenceMenuController)");
                return;
            }

            TPI_Snippet snippet = null;

            foreach (TPI_Snippet snippetObj in _snippetObjects) {
                if (snippetObj.snippetInformation.snippetID == snippetID) {
                    snippet = snippetObj;
                    break;
                }
            }

            if (snippet == null) {
                Debug.LogError("The snippet sequence list does not contain the snippet with the ID: " + snippetID + "! (RemoveSnippet in TPI_SequenceMenuController)");
                return;
            }

            _snippetObjects.Remove(snippet);
            DeleteVisualSnippetObject(snippetID);
            Destroy(snippet.gameObject);

        }

        /// <summary>
        /// Allows you to remove a snippet from the sequence list by providing the position in the list (index starts at 0).
        /// </summary>
        public void RemoveSnippetAt(int position) {
            if (_snippetObjects.Count == 0) {
                Debug.LogError("The snippet sequence List is empty! (RemoveSnippetAt in TPI_SequenceMenuController)");
                return;
            }
            if (position > _snippetObjects.Count - 1) {
                Debug.LogError("The position is out of bounds! (RemoveSnippetAt in TPI_SequenceMenuController)");
                return;
            }
            TPI_Snippet snippet = _snippetObjects[position];
            _snippetObjects.Remove(snippet);
            DeleteVisualSnippetObject(snippet.snippetInformation.snippetID);
            Destroy(snippet.gameObject);

        }

        /// <summary>
        /// Allows you to remove the last snippet from the sequence list.
        /// </summary>
        public void RemoveLastSnippet() {
            if (_snippetObjects.Count == 0) {
                Debug.LogError("The snippet list is already empty! (RemoveLastSnippet in TPI_SequenceMenuController)");
                return;
            }
            RemoveSnippetAt(_snippetObjects.Count - 1);
        }

        /// <summary>
        /// Allows you to switch two snippets in the sequence list by providing both of their positions (index starts at 0).
        /// </summary>
        public void SwitchSnippets(int positionFirstSnippet, int positionSecondSnippet) {

            if (positionFirstSnippet == positionSecondSnippet)
                return;
            if (positionFirstSnippet > _snippetObjects.Count - 1 || positionFirstSnippet < 0) {
                Debug.LogError("The position of the first snippet is out of bounds! (SwitchSnippets in TPI_SequenceMenuController)");
                return;
            }

            if (positionSecondSnippet > _snippetObjects.Count - 1 || positionSecondSnippet < 0) {
                Debug.LogError("The position of the second snippet is out of bounds! (SwitchSnippets in TPI_SequenceMenuController)");
                return;
            }


            TPI_Snippet tempSnippet = GetSnippetAt(positionFirstSnippet);
            _snippetObjects[positionFirstSnippet] = GetSnippetAt(positionSecondSnippet);
            _snippetObjects[positionSecondSnippet] = tempSnippet;

            GameObject firstSnippet = _visualSnippetObjects[positionFirstSnippet];
            GameObject secondSnippet = _visualSnippetObjects[positionSecondSnippet];

            firstSnippet.GetComponent<TPI_SequenceMenuButton>().objectIndex = positionSecondSnippet;
            secondSnippet.GetComponent<TPI_SequenceMenuButton>().objectIndex = positionFirstSnippet;

            if (selectedSnippet == positionFirstSnippet)
                selectedSnippet = positionSecondSnippet;
            else if (selectedSnippet == positionSecondSnippet)
                selectedSnippet = positionFirstSnippet;

            // Set to the correct sibling index
            firstSnippet.transform.SetSiblingIndex(_visualSnippetObjects.Count - 1);
            secondSnippet.transform.SetSiblingIndex(_visualSnippetObjects.Count - 1);

            if (positionFirstSnippet < positionSecondSnippet) {
                secondSnippet.transform.SetSiblingIndex(positionFirstSnippet);
                firstSnippet.transform.SetSiblingIndex(positionSecondSnippet);
            } else {
                firstSnippet.transform.SetSiblingIndex(positionSecondSnippet);
                secondSnippet.transform.SetSiblingIndex(positionFirstSnippet);
            }

            // Update Sequence Number on Button
            string positionText = (positionSecondSnippet + 1).ToString();
            if (positionText.Length == 1)
                positionText = "0" + positionText;
            firstSnippet.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;
            positionText = (positionFirstSnippet + 1).ToString();
            if (positionText.Length == 1)
                positionText = "0" + positionText;
            secondSnippet.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;

            _visualSnippetObjects[positionFirstSnippet] = secondSnippet;
            _visualSnippetObjects[positionSecondSnippet] = firstSnippet;

            StartCoroutine(InvokeUpdateCollection(null, snippetContainerPath.GetComponentInChildren<GridObjectCollection>(), null));

        }

        /// <summary>
        /// Allows you to switch two snippets in the sequence list by providing both of their IDs.
        /// </summary>
        public void SwitchSnippets(string IDFirstSnippet, string IDSecondSnippet) {

            if (IDFirstSnippet == IDSecondSnippet)
                return;

            int positionFirstSnippet = -1;
            int positionSecondSnippet = -1;
            for (int i = 0; i < _snippetObjects.Count; i++) {
                if (_snippetObjects[i].snippetInformation.snippetID == IDFirstSnippet) {
                    positionFirstSnippet = i;
                    continue;
                }
                if (_snippetObjects[i].snippetInformation.snippetID == IDSecondSnippet) {
                    positionSecondSnippet = i;
                    continue;
                }
                if (positionFirstSnippet != -1 && positionSecondSnippet != -1)
                    break;
            }

            if (positionFirstSnippet == -1) {
                Debug.LogError("The snippet with the first ID could not be found! (SwitchSnippets in TPI_SequenceMenuController)");
                return;
            }
            if (positionSecondSnippet == -1) {
                Debug.LogError("The snippet with the second ID could not be found! (SwitchSnippets in TPI_SequenceMenuController)");
                return;
            }
            SwitchSnippets(positionFirstSnippet, positionSecondSnippet);

        }

        /// <summary>
        /// Allows you to remove all snippets from the sequence list.
        /// </summary>
        public void ClearSnippets() {
            foreach (var snippet in _snippetObjects.ToList()) {
                RemoveSnippet(snippet);
            }
        }

        /// <summary>
        /// Instantiates the object visible in the sequence list in the sequence menu at a specific position and configures it.
        /// </summary>
        private void InstantiateVisualSnippetObject(TPI_Snippet snippetScript, int position) {

            if (_visualSnippetObjects.Count == 0) { // Only for the first entry into the list
                if(rosController.IsROSConnectionDeactivated() || (rosController.hasConnectionThread() && !rosController.hasConnectionError()))
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, false); // Unlock Start Button
                else {
                    TPI_ROSController.OnConnected += delegate { UnlockStartButtonIfNotStarted(); }; // Unlock Start Button when the ROS connection is stable
                }
                snippetContainerPath.transform.GetChild(3).gameObject.SetActive(false); // Disable NoContent Text
            }

            GameObject snippetObj = Instantiate(snippetPrefab, transform.position, transform.rotation);
            snippetObj.transform.localScale = transform.localScale * 2.5f;
            snippetObj.transform.parent = snippetContainerPath.GetComponentInChildren<GridObjectCollection>().transform;
            snippetObj.name = "Snippet Button: " + snippetScript.snippetInformation.snippetName;
            _visualSnippetObjects.Insert(position, snippetObj);
            snippetObj.AddComponent<TPI_ObjectIdentifier>().GUID = snippetScript.snippetInformation.snippetID;

            // Change GameObject Sibling Positon -> otherwise UpdateCollection() isn't updating correctly
            snippetObj.transform.SetSiblingIndex(position);

            // Update Sequence Number on Button
            string positionText = (position + 1).ToString();
            if (positionText.Length == 1)
                positionText = "0" + positionText;
            snippetObj.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;

            // Update Sequence Number on other Buttons
            if (position != _snippetObjects.Count - 1) {
                for (int i = position + 1; i < _snippetObjects.Count; i++) {
                    positionText = (i + 1).ToString();
                    if (positionText.Length == 1)
                        positionText = "0" + positionText;
                    _visualSnippetObjects[i].transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;
                }
            }
            // Set the correct name
            if (mainController.GetComponent<TPI_WorkflowConfigurationController>().showTemplateName_snippet) { // Show the template name
                if (mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_snippet == -1) // Show the full name
                    snippetObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = snippetScript.snippetInformation.snippetTemplateName + ": " + snippetScript.snippetInformation.snippetName;
                else // Shorten the name
                    snippetObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = snippetScript.snippetInformation.snippetTemplateName.Substring(0, mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_snippet) + ": " + snippetScript.snippetInformation.snippetName;
            } else // Do not show the template name
                snippetObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = snippetScript.snippetInformation.snippetName;

            // Set Icon of the Snippet
            if (snippetScript.snippetInformation.snippetIcon != null) //otherwise use the standard icon
                snippetObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().SetQuadIcon(snippetScript.snippetInformation.snippetIcon);


            // Add Button Event Handlers
            // OnClick Event is handled in the TPI_SequenceMenuButton
            TPI_SequenceMenuButton dragScript = _visualSnippetObjects[position].GetComponent<TPI_SequenceMenuButton>();
            dragScript.isSnippet = true;
            dragScript.objectIndex = position;
            dragScript.snippetScript = snippetScript;

            InteractableOnHoldReceiver holdReceiver = snippetObj.transform.GetChild(0).GetComponent<Interactable>().AddReceiver<InteractableOnHoldReceiver>();
            holdReceiver.OnHold.AddListener(() => HandleSnippetOnHoldButtonEvents(dragScript));
            holdReceiver.HoldTime = buttonHoldTime;


            // Add Item to ScrollingObjectCollection
            StartCoroutine(InvokeUpdateCollection(snippetObj, snippetContainerPath.GetComponentInChildren<GridObjectCollection>(), snippetContainerPath.GetComponentInChildren<ScrollingObjectCollection>()));


        }

        /// <summary>
        /// Helper function to unlock the start button once a stable connection to ROS has been made.
        /// </summary>
        private void UnlockStartButtonIfNotStarted() {
            if(_sequenceState == SequenceState.notStarted)
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, false);
        }

        /// <summary>
        /// Instantiates the object visible in the sequence list in the sequence menu at the end and configures it.
        /// </summary>
        private void InstantiateVisualSnippetObject(TPI_Snippet snippet) {
            InstantiateVisualSnippetObject(snippet, _visualSnippetObjects.Count);
        }

        /// <summary>
        /// Updates the visual snippet representation in the sequence menu by providing the TPI_Snippet reference and the visual snippet GameObject itself.
        /// </summary>
        public void UpdateSnippetVisuals(TPI_Snippet snippet) {

            int position = _snippetObjects.IndexOf(snippet);
            GameObject snippetObject = _visualSnippetObjects[position];
            // Update Sequence Number on Button
            string positionText = (position + 1).ToString();
            if (positionText.Length == 1)
                positionText = "0" + positionText;
            snippetObject.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;

            // Update Sequence Number on other Buttons
            if (position != _snippetObjects.Count - 1) {
                for (int i = position + 1; i < _snippetObjects.Count; i++) {
                    positionText = (i + 1).ToString();
                    if (positionText.Length == 1)
                        positionText = "0" + positionText;
                    _visualSnippetObjects[i].transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;
                }
            }

            // set the correct name
            snippet.gameObject.name = "Snippet Function: " + snippet.snippetInformation.snippetName;
            if (mainController.GetComponent<TPI_WorkflowConfigurationController>().showTemplateName_snippet) { // Show the template name
                if (mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_snippet == -1) // Show the full name
                    snippetObject.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = snippet.snippetInformation.snippetTemplateName + ": " + snippet.snippetInformation.snippetName;
                else // Shorten the name
                    snippetObject.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = snippet.snippetInformation.snippetTemplateName.Substring(0, mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_snippet) + ": " + snippet.snippetInformation.snippetName;
            } else // Do not show the template name
                snippetObject.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = snippet.snippetInformation.snippetName;

            // Update Snippet Index
            _visualSnippetObjects[position].GetComponent<TPI_SequenceMenuButton>().objectIndex = position;

        }

        /// <summary>
        /// Deletes the visual object with the provided ID from the snippet sequence list in the sequence menu.
        /// </summary>
        private void DeleteVisualSnippetObject(string snippetID) {

            GameObject snippet = null;
            foreach (GameObject snippetButtonObj in _visualSnippetObjects) {
                if (snippetButtonObj.GetComponent<TPI_ObjectIdentifier>().GUID == snippetID) {
                    snippet = snippetButtonObj;
                    break;
                }
            }

            if (snippet == null) {
                Debug.LogError("The visual snippet button list does not contain the snippet button with the ID: " + snippetID + "! (DeleteVisualSnippetObject in TPI_SequenceMenuController)");
                return;
            }

            // Update Sequence Number on other Buttons
            if (snippet.transform.GetSiblingIndex() != _visualSnippetObjects.Count - 1) {
                for (int i = snippet.transform.GetSiblingIndex() + 1; i < _visualSnippetObjects.Count; i++) {
                    string positionText = i.ToString();
                    if (positionText.Length == 1)
                        positionText = "0" + positionText;
                    _visualSnippetObjects[i].transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().text = "|  " + positionText;
                }
            }

            _visualSnippetObjects.Remove(snippet);

            // Remove Object from ScrollingObjectCollection
            RemoveScrollingObject(snippet, snippetContainerPath.GetComponentInChildren<ScrollingObjectCollection>());

            Destroy(snippet);

            if (_visualSnippetObjects.Count == 0) {
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, true); // Lock Start Button
                snippetContainerPath.transform.GetChild(3).gameObject.SetActive(true); // Enable NoContent Text
            }

        }

        /// <summary>
        /// Handles what happens if a Snippet is selected
        /// <para><paramref name="snippetIndex"/> = Index of the snippet that was selected (button pressed)</para>
        /// </summary>
        public void SelectSnippet(int snippetIndex) {

            // Deselect previous Snippet
            if (selectedSnippet != -1 || selectedSnippet == snippetIndex) {
                if (_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.stopped) {
                    SetHighlightButtonState(_visualSnippetObjects[selectedSnippet].transform.GetChild(0).gameObject, false);
                    _visualSnippetObjects[selectedSnippet].GetComponent<TPI_SequenceMenuButton>().ResetButton();
                }    
                _visualSnippetObjects[snippetIndex].GetComponent<TPI_SequenceMenuButton>().secondsRemaining = -1;

                // If Constraints are being visualized -> stop visualiuation of the snippet-specific Constraints that belong to this snippet
                if (isConstraintVisualizationActive) {
                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific) {
                            if (constraint.constraintInformation.snippetID == _snippetObjects[selectedSnippet].snippetInformation.snippetID) {
                                constraint.StopVisualization();
                            }
                        }
                    }
                }
            }

            if (selectedSnippet != snippetIndex) { // If the same snippet is not selected

                // Select new Snippet
                selectedSnippet = snippetIndex;
                if (_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.stopped) {
                    specificConstraintContainerPath.transform.GetChild(4).gameObject.SetActive(false); // Disable the SelectedSnippetText of the specific snippets
                    SetHighlightButtonState(_visualSnippetObjects[selectedSnippet].transform.GetChild(0).gameObject, true);
                } else {
                    // Enable the SelectedSnippetText and set the correct text (only if the sequence is running)
                    specificConstraintContainerPath.transform.GetChild(4).gameObject.SetActive(true);
                    string positionText = (snippetIndex + 1).ToString();
                    if (positionText.Length == 1)
                        positionText = "0" + positionText;
                    specificConstraintContainerPath.transform.GetChild(4).GetComponent<TextMeshPro>().text = "Currently Selected Snippet: " + positionText + "  |  " + _snippetObjects[snippetIndex].snippetInformation.snippetName;
                }

                int specificSnippets = 0;

                // Show & Hide Specific Constraints
                for (int i = 0; i < GetSpecificConstraintCount(); i++) {
                    if (GetSpecificConstraintAt(i).constraintInformation.snippetID == GetSnippetAt(snippetIndex).snippetInformation.snippetID) {
                        specificSnippets++;
                        _visualConstraintObjects[_constraintObjects.IndexOf(GetSpecificConstraintAt(i))].SetActive(true);
                    } else {
                        _visualConstraintObjects[_constraintObjects.IndexOf(GetSpecificConstraintAt(i))].SetActive(false);
                    }

                }

                if (specificSnippets == 0) { // Show Text that there are no specific Constraints
                    specificConstraintContainerPath.transform.GetChild(3).gameObject.SetActive(true); // Enable NoContent Text
                    specificConstraintContainerPath.transform.GetChild(3).GetComponent<TextMeshPro>().text = "No snippet-specific Constraints have been created for this Snippet yet."; // Set Correct Text
                    return;
                } else {
                    specificConstraintContainerPath.transform.GetChild(3).gameObject.SetActive(false); // Disable NoContent Text
                }

                // If Constraints are being visualized -> start visualiuation of the snippet-specific Constraints that belong to this snippet
                if (isConstraintVisualizationActive) {
                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific) {
                            if(constraint.constraintInformation.snippetID == _snippetObjects[snippetIndex].snippetInformation.snippetID) {
                                constraint.VisualizeConstraint();
                            }
                        }
                    }
                }

            } else { // If the same snippet is selected (after the cooldown on the double click has run out)

                selectedSnippet = -1; // No snippet selected
                specificConstraintContainerPath.transform.GetChild(4).gameObject.SetActive(false); // Disable the SelectedSnippetText of the specific snippets
                specificConstraintContainerPath.transform.GetChild(3).gameObject.SetActive(true); // Enable NoContent Text
                specificConstraintContainerPath.transform.GetChild(3).GetComponent<TextMeshPro>().text = "Please select a Snippet in order to display the snippet-specific Constraints that belong to it."; // Set Correct Text

                // Hide all Specific Constraints
                for (int i = 0; i < GetSpecificConstraintCount(); i++) {
                    _visualConstraintObjects[_constraintObjects.IndexOf(GetSpecificConstraintAt(i))].SetActive(false);
                }

            }

            StartCoroutine(InvokeUpdateCollection(null, specificConstraintContainerPath.GetComponentInChildren<GridObjectCollection>(), null));

        }

        /// <summary>
        /// Handles the OnHold Button Event in order to properly move the position of a Snippet inside the Sequence with drag and drop and to properly delete a snippet
        /// <para><paramref name="dragScript"/> = TPI_SequenceMenuButton component of the snippet button that is being held</para>
        /// </summary>
        public void HandleSnippetOnHoldButtonEvents(TPI_SequenceMenuButton dragScript) {
            if (_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.stopped) {
                // Snippet Deletion -> drag to the right
                // Move Snippet -> drag upwards or downwards and drop

                if (dragScript.secondsRemaining == -1) { // Select Button if it is held
                    dragScript.secondsRemaining = standardButtonPressedCooldown;
                    if(selectedSnippet != dragScript.objectIndex)
                        SelectSnippet(dragScript.objectIndex);
                }

                dragScript.isHeld = true;
                dragScript.transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().MaskEnabled = false;
                dragScript.transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().CanScroll = false;

                // Prevents Problems with MRTK that the InteractableOnHoldReceiver never stops 
                foreach (GameObject snippetObject in _visualSnippetObjects) {
                    if (snippetObject == dragScript.gameObject)
                        continue;
                    TPI_SequenceMenuButton component = snippetObject.GetComponent<TPI_SequenceMenuButton>();
                    if (component.isHeld) {
                        component.ResetComponent();
                    }
                }
            }
        }

        /// <summary>
        /// Visualizes the Snippets by moving the digital twin of the robot arm along the planned path.
        /// </summary>
        public void VisualizeSnippets() {
            
            if(rosController.IsROSConnectionDeactivated()) {
                mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "The ROS Connection was disabled and must be enabled in order to visualize Snippets!", "Confirm");
                return;
            }

            if (!rosController.hasConnectionThread()) {
                mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "The ROS Connection has not been initiated yet. Therefore, the snippets currently cannot be visualized.", "Confirm");
                return;
            }

            if (rosController.hasConnectionThread() && rosController.hasConnectionError()) {
                mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "The ROS Connection has errors. Therefore, the snippets currently cannot be visualized.", "Confirm");
                return;
            }

            if(_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.stopped) {

                if (!isSnippetVisualizationActive) {

                    isSnippetVisualizationActive = true;
                    rosController.PublishShouldExecuteInstructions(false); // only simulate snippets
                    rosController.PublishVisualizationSpeed(snippetVisualizationSpeed); // move at the speed setup in the unity inspector
                    sequenceCoroutine = StartCoroutine(InvokeVisualizationSequence());

                    ButtonConfigHelper helper = mainController.sequenceFunctionsMenu.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<ButtonConfigHelper>(); // Visualize Sequence Button
                    helper.MainLabelText = "Stop Snippet Visualization";
                    if (stopVisualizationIcon != null)
                        helper.SetQuadIcon(stopVisualizationIcon);

                    if(_sequenceState == SequenceState.notStarted) // not started
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, true); // Lock Start Button (all the others are already locked)
                    else // stopped
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(3).gameObject, true); // Lock Restart Button (all the others are already locked)

                } else {

                    StopCoroutine(sequenceCoroutine);
                    sequenceCoroutine = null;
                    isSnippetVisualizationActive = false;

                    ButtonConfigHelper helper = mainController.sequenceFunctionsMenu.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<ButtonConfigHelper>(); // Visualize Sequence Button
                    helper.MainLabelText = "Visualize Sequence";
                    if (snippetVisualizationIcon != null)
                        helper.SetQuadIcon(snippetVisualizationIcon);

                    if (_sequenceState == SequenceState.notStarted) // not started
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, false); // Unlock Start Button (all the others should stay locked)
                    else // stopped
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(3).gameObject, false); // Unlock Restart Button (all the others should stay locked)

                }

            } else {
                Debug.LogError("The Sequence can only be visualized if the sequence has not been started yet or is currently stopped! (VisualizeSnippets in TPI_SequenceMenuController)");
            }

        }

        /// <summary>
        /// Coroutine that handles the sequence of snippets for the visualization
        /// </summary>
        private IEnumerator InvokeVisualizationSequence() {

            while (currentStep < _snippetObjects.Count) {
                TPI_Snippet snippet = _snippetObjects[currentStep];
                yield return StartCoroutine(snippet.RunSnippet());
                snippet.OnHasEnded();
            }
            currentStep = 0;
            sequenceCoroutine = null;
            isSnippetVisualizationActive = false;

            ButtonConfigHelper helper = mainController.sequenceFunctionsMenu.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<ButtonConfigHelper>(); // Visualize Sequence Button
            helper.MainLabelText = "Visualize Sequence";
            if (snippetVisualizationIcon != null)
                helper.SetQuadIcon(snippetVisualizationIcon);

        }

        #endregion Snippets



        //---------------------------------------------------- Constraints ----------------------------------------------------//



        #region Constraints
        /// <summary>
        /// Allows you to get all constraints (both global and snippet-specific).
        /// </summary>
        /// <returns>The complete List of Constraints (List of TPI_Constraint)</returns>
        public List<TPI_Constraint> GetConstraints() {
            return _constraintObjects;
        }

        /// <summary>
        /// Allows you to get all global constraints.
        /// </summary>
        /// <returns>The List of global Constraints (List of TPI_Constraint)</returns>
        public List<TPI_Constraint> GetGlobalConstraintList() {
            List<TPI_Constraint> globalConstraints = new List<TPI_Constraint>();
            foreach (TPI_Constraint constraint in _constraintObjects) {
                if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                    globalConstraints.Add(constraint);
            }
            return globalConstraints;
        }

        /// <summary>
        /// Allows you to get all snippet-specific constraints.
        /// </summary>
        /// <returns>The List of snippet-specific Constraints (List of TPI_Constraint)</returns>
        public List<TPI_Constraint> GetSpecificConstraintList() {
            List<TPI_Constraint> specificConstraints = new List<TPI_Constraint>();
            foreach (TPI_Constraint constraint in _constraintObjects) {
                if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific)
                    specificConstraints.Add(constraint);
            }
            return specificConstraints;
        }

        /// <summary>
        /// Allows you to get all snippet-specific constraints that belong to the snippet with the given snippetID
        /// </summary>
        /// <returns>The List of snippet-specific Constraints that belong to the Snippet(List of TPI_Constraint)</returns>
        public List<TPI_Constraint> GetSpecificConstraintsOfSnippet(string snippetID) {
            List<TPI_Constraint> specificConstraints = GetSpecificConstraintList();
            foreach (TPI_Constraint constraint in specificConstraints.ToList()) {
                if (constraint.constraintInformation.snippetID != snippetID)
                    specificConstraints.Remove(constraint);
            }
            return specificConstraints;
        }
        
        /// <summary>
        /// Allows you to get the constraints count (both global and snippet-specific).
        /// </summary>
        /// <returns>The Amount of Objects in the complete List of Constraints (int)</returns>
        public int GetConstraintCount() {
            return _constraintObjects.Count;
        }

        /// <summary>
        /// Allows you to get the global constraint count.
        /// </summary>
        /// <returns>Global Constraint Count (int)</returns>
        public int GetGlobalConstraintCount() {
            return GetGlobalConstraintList().Count;
        }

        /// <summary>
        /// Allows you to get the snippet-specific constraint count.
        /// </summary>
        /// <returns>Snippet-Specific Constraint Count (int)</returns>
        public int GetSpecificConstraintCount() {
            return GetSpecificConstraintList().Count;
        }

        /// <summary>
        /// Allows you to get the global constraint at a specific position in the constraints list (index starts at 0).
        /// </summary>
        /// <returns>Global Constraint at specific position (TPI_Constraint)</returns>
        public TPI_Constraint GetGlobalConstraintAt(int position) {
            List<TPI_Constraint> globalConstraints = GetGlobalConstraintList();
            if(globalConstraints.Count == 0) {
                Debug.LogError("The list of global constraints is empty! Returning null... (GetGlobalConstraintAt in TPI_SequenceMenuController)");
                return null;
            }
            if (position < 0 || position > globalConstraints.Count - 1) {
                Debug.LogError("The position is out of bounds! Returning null... (GetGlobalConstraintAt in TPI_SequenceMenuController)");
                return null;
            }
            return globalConstraints[position];
        }

        /// <summary>
        /// Allows you to get the snippet-specific constraint at a specific position in the constraints list (index starts at 0).
        /// </summary>
        /// <returns>Snippet-Specific Constraint at specific position (TPI_Constraint)</returns>
        public TPI_Constraint GetSpecificConstraintAt(int position) {
            List<TPI_Constraint> specificConstraints = GetSpecificConstraintList();
            if (specificConstraints.Count == 0) {
                Debug.LogError("The list of snippet-specific constraints is empty! Returning null... (GetSpecificConstraintAt in TPI_SequenceMenuController)");
                return null;
            }
            if (position < 0 || position > specificConstraints.Count - 1) {
                Debug.LogError("The position is out of bounds! Returning null... (GetSpecificConstraintAt in TPI_SequenceMenuController)");
                return null;
            }
            return specificConstraints[position];
        }

        /// <summary>
        /// Allows you add a new constraint to the constraints list by providing the constraint function component.
        /// </summary>
        public void AddConstraint(TPI_Constraint constraint) {
            if (constraint == null) {
                Debug.LogError("The constraint cannot be null! (AddConstraint in TPI_SequenceMenuController)");
                return;
            }
            AddConstraint(constraint, constraint.constraintInformation.constraintType);
        }

        /// <summary>
        /// Allows you add a new constraint to the constraints list by providing the constraint function component and the constraint type (global or snippet-specific).
        /// <para><paramref name="type"/> = Please use TPI_ConstraintType.global or TPI_ConstraintType.snippetSpecific to select the Constraint Type</para>
        /// </summary>
        public void AddConstraint(TPI_Constraint constraint, TPI_ConstraintType type) {
            if (constraint == null) {
                Debug.LogError("The constraint cannot be null! (AddConstraint in TPI_SequenceMenuController)");
                return;
            }
            if (type == TPI_ConstraintType.global)
                constraint.gameObject.name = "Global Constraint Function: " + constraint.constraintInformation.constraintName;
            else
                constraint.gameObject.name = "Specific Constraint Function: " + constraint.constraintInformation.constraintName;
            _constraintObjects.Add(constraint);
            InstantiateVisualConstraintObject(constraint, type);
        }

        /// <summary>
        /// Allows you remove a constraint from the constraints list by providing the constraint function component.
        /// </summary>
        public void RemoveConstraint(TPI_Constraint constraint) {
            if (constraint == null) {
                Debug.LogError("The constraint cannot be null! (RemoveConstraint in TPI_SequenceMenuController)");
                return;
            }
            if (!_constraintObjects.Contains(constraint)) {
                Debug.LogError("The constraint list does not contain this constraint! (RemoveConstraint in TPI_SequenceMenuController)");
                return;
            }
            _constraintObjects.Remove(constraint);
            DeleteVisualConstraintObject(constraint.constraintInformation.constraintID, constraint.constraintInformation.constraintType);
            Destroy(constraint.gameObject);
        }

        /// <summary>
        /// Allows you remove a constraint from the constraints list by providing the constraint ID.
        /// </summary>
        public void RemoveConstraint(string constraintID) {
            if (constraintID == "") {
                Debug.LogError("The constraintID cannot be empty! (RemoveConstraint in TPI_SequenceMenuController)");
                return;
            }
            if (_constraintObjects.Count == 0) {
                Debug.LogError("The constraint List is empty! (RemoveConstraintAt in TPI_SequenceMenuController)");
                return;
            }

            TPI_Constraint constraint = null;
            foreach (TPI_Constraint constraintObject in _constraintObjects) {
                if (constraintObject.constraintInformation.constraintID == constraintID) {
                    constraint = constraintObject;
                    break;
                }
            }

            if (constraint == null) {
                Debug.LogError("The constraint list does not contain this constraint! (RemoveConstraint in TPI_SequenceMenuController)");
                return;
            }
            _constraintObjects.Remove(constraint);
            DeleteVisualConstraintObject(constraintID, constraint.constraintInformation.constraintType);
            Destroy(constraint.gameObject);
        }

        /// <summary>
        /// Allows you remove all constraints from the constraints list.
        /// </summary>
        public void ClearConstraints() {
            foreach (var constraint in _constraintObjects.ToList()) {
                RemoveConstraint(constraint);
            }
        }

        /// <summary>
        /// Instantiates the object visible in the constraints list (depends on the type) in the sequence menu and configures it.
        /// <para><paramref name="type"/> = Please use TPI_ConstraintType.global or TPI_ConstraintType.snippetSpecific to select the Constraint Type</para>
        /// </summary>
        private void InstantiateVisualConstraintObject(TPI_Constraint constraintScript, TPI_ConstraintType type) {

            if(type == TPI_ConstraintType.global && GetGlobalConstraintCount() == 1)
                globalConstraintContainerPath.transform.GetChild(3).gameObject.SetActive(false); // Disable NoContent Text

            GameObject constraintObj = Instantiate(constraintPrefab, transform.position, transform.rotation);
            _visualConstraintObjects.Add(constraintObj);
            constraintObj.transform.localScale = transform.localScale * 2.5f;

            // Set the correct name
            if (mainController.GetComponent<TPI_WorkflowConfigurationController>().showTemplateName_constraint) { // Show the template name
                if(mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_constraint == -1) // Show the full name
                    constraintObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = constraintScript.constraintInformation.constraintTemplateName + ": " + constraintScript.constraintInformation.constraintName;
                else // Shorten the name
                    constraintObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = constraintScript.constraintInformation.constraintTemplateName.Substring(0, mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_constraint) + ": " + constraintScript.constraintInformation.constraintName;
            } else // Do not show the template name
                constraintObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = constraintScript.constraintInformation.constraintName;

            constraintObj.AddComponent<TPI_ObjectIdentifier>().GUID = constraintScript.constraintInformation.constraintID;
            if (constraintScript.constraintInformation.constraintIcon != null) //otherwise use the standard icon
                constraintObj.transform.GetChild(0).GetComponent<ButtonConfigHelper>().SetQuadIcon(constraintScript.constraintInformation.constraintIcon);

            // Add Button Event Handler
            //constraintObj.GetComponent<ButtonConfigHelper>().OnClick.AddListener(delegate { RemoveConstraint(constraintScript.constraintInformation.constraintID); }); /////////////////////////////////////////////////// CHANGE TO DRAG & DROP AND ADD SLIDE TO DELETE

            if (type == TPI_ConstraintType.global) {
                constraintObj.transform.parent = globalConstraintContainerPath.GetComponentInChildren<GridObjectCollection>().transform;
                constraintObj.name = "Global Constraint Button: " + constraintScript.constraintInformation.constraintName;
                // Add Item to ScrollingObjectCollection
                StartCoroutine(InvokeUpdateCollection(null, globalConstraintContainerPath.GetComponentInChildren<GridObjectCollection>(), globalConstraintContainerPath.GetComponentInChildren<ScrollingObjectCollection>()));
            } else {
                constraintObj.transform.parent = specificConstraintContainerPath.GetComponentInChildren<GridObjectCollection>().transform;
                constraintObj.name = "Specific Constraint Button: " + constraintScript.constraintInformation.constraintName;
                constraintObj.SetActive(false);
                // Add Item to ScrollingObjectCollection
                StartCoroutine(InvokeUpdateCollection(null, specificConstraintContainerPath.GetComponentInChildren<GridObjectCollection>(), specificConstraintContainerPath.GetComponentInChildren<ScrollingObjectCollection>()));
            }

            // Handle OnClick Event in order to edit the values of the variables
            // OnClick Event is handled in the TPI_SequenceMenuButton
            TPI_SequenceMenuButton dragScript = constraintObj.GetComponent<TPI_SequenceMenuButton>();
            dragScript.isSnippet = false;
            dragScript.objectIndex = _visualConstraintObjects.IndexOf(constraintObj);
            dragScript.constraintScript = constraintScript;

            InteractableOnHoldReceiver holdReceiver = constraintObj.transform.GetChild(0).GetComponent<Interactable>().AddReceiver<InteractableOnHoldReceiver>();
            holdReceiver.OnHold.AddListener(() => HandleConstraintOnHoldButtonEvents(dragScript));
            holdReceiver.HoldTime = buttonHoldTime;

            SetHighlightButtonState(dragScript.transform.GetChild(0).gameObject, false);

        }

        /// <summary>
        /// Updates the visual constraint representation in the sequence menu by providing the TPI_Constraint reference and the visual constraint GameObject itself.
        /// </summary>
        public void UpdateConstraintVisuals(TPI_Constraint constraint) {

            GameObject constraintObject  = null;
            foreach (GameObject go in _visualConstraintObjects) {
                if(go.GetComponent<TPI_ObjectIdentifier>().GUID == constraint.constraintInformation.constraintID) {
                    constraintObject = go;
                    break;
                }
            }

            // Set the correct name
            if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                constraint.gameObject.name = "Global Constraint Function: " + constraint.constraintInformation.constraintName;
            else
                constraint.gameObject.name = "Specific Constraint Function: " + constraint.constraintInformation.constraintName;
            if (mainController.GetComponent<TPI_WorkflowConfigurationController>().showTemplateName_constraint) { // Show the template name
                if (mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_constraint == -1) // Show the full name
                    constraintObject.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = constraint.constraintInformation.constraintTemplateName + ": " + constraint.constraintInformation.constraintName;
                else // Shorten the name
                    constraintObject.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = constraint.constraintInformation.constraintTemplateName.Substring(0, mainController.GetComponent<TPI_WorkflowConfigurationController>().charactersShowed_constraint) + ": " + constraint.constraintInformation.constraintName;
            } else // Do not show the template name
                constraintObject.transform.GetChild(0).GetComponent<ButtonConfigHelper>().MainLabelText = constraint.constraintInformation.constraintName;

        }

            /// <summary>
            /// Deletes the object visible in the constraints list in the sequence menu.
            /// <para><paramref name="type"/> = Please use TPI_ConstraintType.global or TPI_ConstraintType.snippetSpecific to select the Constraint Type</para>
            /// </summary>
            private void DeleteVisualConstraintObject(string constraintID, TPI_ConstraintType constraintType) {

            GameObject constraintObject = null;
            foreach (GameObject constraint in _visualConstraintObjects) {
                if (constraint.GetComponent<TPI_ObjectIdentifier>().GUID == constraintID) {
                    constraintObject = constraint;
                    break;
                }
            }

            if (constraintObject == null) {
                Debug.LogError("The visual constraint list does not contain this constraint! (DeleteVisualConstraintObject in TPI_SequenceMenuController)");
                return;
            }

            _visualConstraintObjects.Remove(constraintObject);

            // Remove Item from ScrollingObjectCollection
            if (constraintType == TPI_ConstraintType.global) {
                RemoveScrollingObject(constraintObject, globalConstraintContainerPath.GetComponentInChildren<ScrollingObjectCollection>());
                if (GetGlobalConstraintCount() == 0)
                    globalConstraintContainerPath.transform.GetChild(3).gameObject.SetActive(true); // Enable NoContent Text
            } else
                RemoveScrollingObject(constraintObject, specificConstraintContainerPath.GetComponentInChildren<ScrollingObjectCollection>());

            Destroy(constraintObject);

        }

        /// <summary>
        /// Handles the OnHold Button Event in order to delete the held Constraint.
        /// <para><paramref name="dragScript"/> =TPI_SequenceMenuButton component of the constraint button that is being held</para>
        /// </summary>
        public void HandleConstraintOnHoldButtonEvents(TPI_SequenceMenuButton dragScript) {
            if (_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.stopped) {
                // Constraint Deletion -> drag to the right
                dragScript.isHeld = true;
                dragScript.transform.GetChild(1).gameObject.SetActive(true);
                SetHighlightButtonState(dragScript.transform.GetChild(0).gameObject, true);
                dragScript.transform.parent.parent.parent.GetComponent<ScrollingObjectCollection>().MaskEnabled = false;

                // Prevents Problems with MRTK that the InteractableOnHoldReceiver never stops 
                foreach (GameObject constraintObjects in _visualConstraintObjects) {
                    if (constraintObjects == dragScript.gameObject)
                        continue;
                    TPI_SequenceMenuButton component = constraintObjects.GetComponent<TPI_SequenceMenuButton>();
                    if (component.isHeld) {
                        component.ResetComponent();
                    }
                }

            }
        }

        /// <summary>
        /// Visualizes the global or snippet-specific constraints by creating tooltips in the environment.
        /// </summary>
        public void VisualizeConstraints() {

            if (_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.stopped) {

                if (!isConstraintVisualizationActive) {

                    isConstraintVisualizationActive = true;

                    foreach(TPI_Constraint constraint in _constraintObjects) {
                        if(constraint.constraintInformation.constraintType == TPI_ConstraintType.global) {
                            constraint.VisualizeConstraint();
                        } else {
                            if (selectedSnippet != -1) {
                                if(constraint.constraintInformation.snippetID == _snippetObjects[selectedSnippet].snippetInformation.snippetID)
                                    constraint.VisualizeConstraint();
                            }
                        }
                        
                    }

                    ButtonConfigHelper helper = mainController.sequenceFunctionsMenu.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Visualize Constraints Button
                    helper.MainLabelText = "Stop Constraint Visualization";
                    if (stopVisualizationIcon != null)
                        helper.SetQuadIcon(stopVisualizationIcon);

                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, true); // Lock Start Button (all the others are already locked)

                } else {

                    isConstraintVisualizationActive = false;

                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global) {
                            constraint.StopVisualization();
                        } else {
                            if (selectedSnippet != -1) {
                                if (constraint.constraintInformation.snippetID == _snippetObjects[selectedSnippet].snippetInformation.snippetID)
                                    constraint.StopVisualization();
                            }
                        }

                    }

                    ButtonConfigHelper helper = mainController.sequenceFunctionsMenu.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<ButtonConfigHelper>(); // Visualize Constraints Button
                    helper.MainLabelText = "Visualize Constraints";
                    if (constraintVisualizationIcon != null)
                        helper.SetQuadIcon(constraintVisualizationIcon);

                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, false); // Unlock Start Button (all the others should stay locked)

                }

            } else {
                Debug.LogError("The Sequence can only be visualized if the sequence has not been started! (VisualizeSnippets in TPI_SequenceMenuController)");
            }

        }
        #endregion Constraints



        //---------------------------------------------------- Sequence Functions ----------------------------------------------------//



        #region SequenceFunctions
        /// <summary>
        /// Locks or unlocks a button, so that the operator can either interact with it or not.
        /// <para><paramref name="status"/> = true -> lock button, false -> unlock button</para>
        /// </summary>
        private void SetLockButtonState(GameObject buttonObject, bool status) { // true -> lock button, false -> unlock button

            buttonObject.transform.GetChild(0).GetComponent<Interactable>().enabled = !status;
            buttonObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(!status);
            buttonObject.transform.GetChild(1).gameObject.SetActive(!status);
            buttonObject.transform.GetChild(2).gameObject.SetActive(status);

            if (buttonObject.transform.childCount == 4) { // Check for secondary (replacement) buttons (e.g. TogglePause or Restart)
                buttonObject.transform.GetChild(3).GetComponent<Interactable>().enabled = !status;
                buttonObject.transform.GetChild(3).GetChild(1).gameObject.SetActive(!status);
            }
            
        }

        /// <summary>
        /// Turns the highlight backplate of the snippets on or off depending on the status.
        /// <para><paramref name="status"/> = true -> highlight button, false -> no longer highlight button</para>
        /// </summary>
        private void SetHighlightButtonState(GameObject snippetButton, bool status) { // true -> highlight button, false -> no longer highlight button
            snippetButton.transform.GetChild(3).GetComponent<MeshRenderer>().enabled = status;
        }

        /// <summary>
        /// Turns the rotating orbs on or off depending on the status.
        /// <para><paramref name="status"/> = true -> rotating orbs enabled, false -> rotating orbs disabled</para>
        /// </summary>
        private async void SetRotatingOrbsState(bool status) { // true -> rotating orbs enabled, false -> rotating orbs disabled
            Transform rotatingOrbsTransform = transform.GetChild(1).GetChild(0).GetChild(4);
            MeshRenderer[] orbs = rotatingOrbsTransform.GetComponentsInChildren<MeshRenderer>();
            if (!status) {
                if (!rotatingOrbsTransform.gameObject.activeSelf) // Otherwise it might throw an error
                    return;
                await rotatingOrbsTransform.GetComponent<ProgressIndicatorOrbsRotator>().AwaitTransitionAsync();
                rotatingOrbsTransform.GetComponent<ProgressIndicatorOrbsRotator>().CloseImmediate();
                foreach (MeshRenderer orb in orbs) {
                    orb.enabled = false;
                }
            } else {
                if (rotatingOrbsTransform.gameObject.activeSelf) // Otherwise it might throw an error
                    return;
                foreach (MeshRenderer orb in orbs) {
                    orb.enabled = true;
                }
                await rotatingOrbsTransform.GetComponent<ProgressIndicatorOrbsRotator>().OpenAsync();
                float progress = 0;
                while (progress < 1) {
                    progress += Time.deltaTime;
                    rotatingOrbsTransform.GetComponent<ProgressIndicatorOrbsRotator>().Progress = progress;
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Starts the Sequence of snippets and enables the coroutine and respective constraints.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void StartSequence(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the "StartSequence" button will be locked if there aren't any snippets in the sequence list
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (StartSequence in TPI_SequenceMenuController)");
                return;
            }


            // Only run if the ROS Connection was successfully made or if it is disabled (not if there are any errors)
            if(!rosController.IsROSConnectionDeactivated() && !rosController.hasConnectionThread()) {
                if (reportError)
                    Debug.LogError("Please start the ROS Connection before you try to run the sequence! (StartSequence in TPI_SequenceMenuController)");
                return;
            }
            if (!rosController.IsROSConnectionDeactivated() && rosController.hasConnectionThread() && rosController.hasConnectionError()) {
                if (reportError)
                    Debug.LogError("The ROS Connection has errors! Therefore, the sequence cannot be run before you fix them. (StartSequence in TPI_SequenceMenuController)");
                return;
            }


            if (_sequenceState == SequenceState.paused) { // Just to make sure, generally this should not happen as the "StartSequence" button will be replaced by the "TogglePauseState" button.
                if(reportError)
                    Debug.LogError("Please use the Unpause function instead of the StartSequence function! (StartSequence in TPI_SequenceMenuController)");
                return;
            } else if (_sequenceState == SequenceState.running) { // Just to make sure, generally this should not happen as the "StartSequence" button will be replaced by the "TogglePauseState" button.
                if(reportError)
                    Debug.LogError("The sequence is already running! (StartSequence in TPI_SequenceMenuController)");
                return;
            }  else if (_sequenceState == SequenceState.stopped) {
                if(reportError)
                    Debug.LogError("Please use the Restart function instead of the StartSequence function to start the Sequence Again! (StartSequence in TPI_SequenceMenuController)");
                return;
            } else if (_sequenceState == SequenceState.notStarted || _sequenceState == SequenceState.restarting) {

                if(isSnippetVisualizationActive) {
                    Debug.LogWarning("The Snippets are still being visualized. However, this function did automatically turn it off. Usually, it is better if you turn it off yourself before you call this function.  (StartSequence in TPI_SequenceMenuController).");
                    VisualizeSnippets();
                } else if (isConstraintVisualizationActive) {
                    Debug.LogWarning("The Constraints are still being visualized. However, this function did automatically turn it off. Usually, it is better if you turn it off yourself before you call this function.  (StartSequence in TPI_SequenceMenuController).");
                    VisualizeConstraints();
                }

                // Disable the Selection Menu & Building Blocks Menu to avoid adding snippets / constraints when the sequence is running
                mainController.selectionMenu.gameObject.SetActive(false);
                mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().FreeUpSpot(mainController.selectionMenu);
                mainController.buildingBlocksMenu.gameObject.SetActive(false);
                mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>().FreeUpSpot(mainController.buildingBlocksMenu);

                // Unlock the other Coroutine Buttons & Lock Go Back Button
                if (_sequenceState != SequenceState.restarting) {
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false); // Unlock Emergency Stop Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, true); // Lock Return To Previous Button 
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(3).gameObject, false); // Unlock Stop Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(4).gameObject, false); // Unlock Repeat Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, false); // Unlock Skip Button
                }

                // Change Start Button to TogglePause Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(0).gameObject.SetActive(false); // Hide Start Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).gameObject.SetActive(true); // Show TogglePause Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().MainLabelText = "Pause Button";
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().SetQuadIcon(PauseIcon);

                // Change Visualization Buttons to EmergencyStop and ReturnToPreviousSnippet
                coroutineButtonsContainerPath.transform.GetChild(0).GetChild(0).gameObject.SetActive(false); // Hide Visualize Snippets Button
                coroutineButtonsContainerPath.transform.GetChild(0).GetChild(3).gameObject.SetActive(true); // Show Emergency Stop Button
                coroutineButtonsContainerPath.transform.GetChild(1).GetChild(0).gameObject.SetActive(false); // Hide Visualize Constraints Button
                coroutineButtonsContainerPath.transform.GetChild(1).GetChild(3).gameObject.SetActive(true); // Show Return To Previous Button

                // Start Sequence of Snippet Functions and the Constraints
                if (sequenceCoroutine != null) { // Just to make sure that there will be no memory leaks or problems.
                    StopCoroutine(sequenceCoroutine);
                    sequenceCoroutine = null;
                }

                if(!rosController.IsROSConnectionDeactivated() && rosController.hasConnectionThread() && !rosController.hasConnectionError()) {
                    rosController.PublishShouldExecuteInstructions(true); // move robot
                    rosController.PublishVisualizationSpeed(1f); // move at normal speed
                }

                sequenceCoroutine = StartCoroutine(InvokeSequence());
                foreach (TPI_Constraint constraint in _constraintObjects) {
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                        constraint.ApplyConstraint();
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                        constraint.ApplyConstraint();
                }

                if (selectedSnippet != -1) {
                    specificConstraintContainerPath.transform.GetChild(4).gameObject.SetActive(true); // Enable the SelectedSnippetText of the specific snippets
                    SetHighlightButtonState(_visualSnippetObjects[selectedSnippet].transform.GetChild(0).gameObject, false); // Remove highlight of the currently selected Button (Stems from the SelectSnippet Function)
                }

                // Clear all Dialog Menus
                mainController.GetComponent<TPI_DialogMenuController>().ClearDialogMenus();

                SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, true);
                _sequenceState = SequenceState.running;

                // Enable the Rotating Orbs -> Indicate that it is running
                SetRotatingOrbsState(true);

            }

        }

        /// <summary>
        /// Coroutine that handles the sequence of snippets -> automatically switches to the next snippet once the previous is finished.
        /// </summary>
        private IEnumerator InvokeSequence() {

            while(currentStep < _snippetObjects.Count) {

                TPI_Snippet snippet = _snippetObjects[currentStep];

                yield return StartCoroutine(snippet.RunSnippet());
                snippet.OnHasEnded();
                if (currentStep == _snippetObjects.Count - 1)
                    StopSequence();
                else {
                    if (snippetProgression == SnippetProgression.pause) {
                        TogglePauseSnippet();
                        yield return new WaitUntil(() => isPaused == false);
                        LaunchNextStep();
                    } else if (snippetProgression == SnippetProgression.automatically) {
                        LaunchNextStep();
                    } else if (snippetProgression == SnippetProgression.delay) {
                        foreach (TPI_Constraint constraint in _constraintObjects) {
                            if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                                constraint.StopConstraint();
                        }
                        SetRotatingOrbsState(false);
                        yield return new WaitForSeconds(snippetProgressionDelay);
                        LaunchNextStep(true);
                        SetRotatingOrbsState(true);
                    }
                }
            }
        }

        /// <summary>
        /// Helper function that automatically prepares the sequence menu environment for the next snippet.
        /// <para><paramref name="delayedExecution"/> = true -> if the SnippetProgression is set to delay, false -> otherwise</para>
        /// </summary>
        private void LaunchNextStep(bool delayedExecution = false) {

            if(_sequenceState == SequenceState.running) {

                foreach (TPI_Constraint constraint in _constraintObjects) {
                    if (!delayedExecution) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                            constraint.StopConstraint();
                    }
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep + 1].snippetInformation.snippetID)
                        constraint.ApplyConstraint();
                }
                currentStep++;

                // Change Button Highlight
                SetHighlightButtonState(_visualSnippetObjects[currentStep - 1].transform.GetChild(0).gameObject, false);
                SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, true);

                // Unlock Go Back Button if it is not the first snippet
                if (currentStep >= 1)
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, false);

                // Lock Skip Button if it is the last snippet
                if(currentStep == _snippetObjects.Count - 1)
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, true);

            }

        }

        /// <summary>
        /// Manually Stops the Sequence of snippets and disables the coroutine and constraints.
        /// <br></br>Please only run this function if the sequence is either running or paused.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void StopSequence(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running (StartSequence also asserts this).
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (StopSequence in TPI_SequenceMenuController)");
                return;
            }

            if (_sequenceState == SequenceState.running || _sequenceState == SequenceState.paused) {

                // Stop Snippet Function & Constraints
                _snippetObjects[currentStep].StopSnippet();
                StopCoroutine(sequenceCoroutine);
                sequenceCoroutine = null;
                foreach (TPI_Constraint constraint in _constraintObjects) {
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                        constraint.StopConstraint();
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                        constraint.StopConstraint();
                }

                _sequenceState = SequenceState.stopped;
                isPaused = false;

                // Change Stop Button to Restart Button
                coroutineButtonsContainerPath.transform.GetChild(3).GetChild(0).gameObject.SetActive(false); // Hide Stop Button
                coroutineButtonsContainerPath.transform.GetChild(3).GetChild(3).gameObject.SetActive(true); // Show Restart Button

                // Change TogglePause Button to Start Button and lock Start Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, true); // Lock Start Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(0).gameObject.SetActive(true); // Show Start Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).gameObject.SetActive(false); // Hide Toggle Pause Button

                // Change EmergencyStop and ReturnToPreviousSnippet to Visualization Buttons
                coroutineButtonsContainerPath.transform.GetChild(0).GetChild(0).gameObject.SetActive(true); // Show Visualize Sequence
                coroutineButtonsContainerPath.transform.GetChild(0).GetChild(3).gameObject.SetActive(false); // Hide Emergency Stop
                coroutineButtonsContainerPath.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);  // Show Visualize Constraints
                coroutineButtonsContainerPath.transform.GetChild(1).GetChild(3).gameObject.SetActive(false); // Hide Return To Previous Snippet

                // Lock the other Coroutine Buttons
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false); // Unlock Visualize Sequence Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, false); // Unlock Visualize Constraints Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(4).gameObject, true); // Lock Repeat Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, true); // Lock Skip Button

                // Enable the Selection Menu & Building Blocks Menu to allow adding snippets / constraints when the sequence is stopped
                mainController.selectionMenu.SetActive(true);
                TPI_ObjectPlacementController objectPlacementController = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>();
                objectPlacementController.ReserveSpot(objectPlacementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), mainController.selectionMenu, true);
                if (mainController.GetComponent<TPI_WorkflowConfigurationController>().IsBuildingBlocksMenuOpen()) {
                    mainController.buildingBlocksMenu.SetActive(true);
                    objectPlacementController.ReserveSpot(objectPlacementController.GetSpotRight(objectPlacementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 1), mainController.buildingBlocksMenu, true);
                }


                // Disable the Rotating Orbs -> Indicate that it is not running
                SetRotatingOrbsState(false);

                // Disable Button Highlight
                SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, false);

                if (selectedSnippet != -1) {
                    specificConstraintContainerPath.transform.GetChild(4).gameObject.SetActive(false); // Disable the SelectedSnippetText of the specific snippets
                    SetHighlightButtonState(_visualSnippetObjects[selectedSnippet].transform.GetChild(0).gameObject, true); // Remove highlight of the currently selected Button (Stems from the SelectSnippet Function)
                }

            } else { // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running (StartSequence also asserts this).
                if(reportError)
                    Debug.LogError("The sequence is not running! (StopSequence in TPI_SequenceMenuController)");
                return; 
            }

        }

        /// <summary>
        /// Restarts the whole sequence after it was stopped by the StopSequence function.
        /// <br></br>Please only run this when the sequence was stopped beforehand.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void RestartSequence(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the button will not be visible when the sequence has never been started.
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (RestartSequence in TPI_SequenceMenuController)");
                return;
            }

            if (_sequenceState == SequenceState.stopped) {

                if (isSnippetVisualizationActive) {
                    Debug.LogWarning("The Snippets are still being visualized. However, this function did automatically turn it off. Usually, it is better if you turn it off yourself before you call this function.  (RestartSequence in TPI_SequenceMenuController).");
                    VisualizeSnippets();
                } else if (isConstraintVisualizationActive) {
                    Debug.LogWarning("The Constraints are still being visualized. However, this function did automatically turn it off. Usually, it is better if you turn it off yourself before you call this function.  (RestartSequence in TPI_SequenceMenuController).");
                    VisualizeConstraints();
                }

                // Change Restart Button to Stop Button
                coroutineButtonsContainerPath.transform.GetChild(3).GetChild(0).gameObject.SetActive(true); // Show Stop Button
                coroutineButtonsContainerPath.transform.GetChild(3).GetChild(3).gameObject.SetActive(false); // Hide Restart Button

                // Unlock Start Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, false);

                // No longer highlight any button
                SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, false);
                currentStep = 0;

                _sequenceState = SequenceState.restarting;

                // unlock the other Coroutine Buttons & lock Go Back Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false); // Unlock Emergency Stop Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, true); // Lock Return To Previous Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(4).gameObject, false); // Unlock Repeat Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, false); // Unlock Skip Button

                StartSequence();

            } else { // Just to make sure, generally this should not happen as the button will not be visible if the sequence has not been stopped
                if(reportError)
                    Debug.LogError("The sequence has not been stopped! Please use the StopSequence function first! (RestartSequence in TPI_SequenceMenuController)");
                return;
            }

        }

        /// <summary>
        /// Pauses or unpauses the sequence of snippets.
        /// <br></br>Please only run this function if the sequence is either running or paused.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void TogglePauseSnippet(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the button will only appear once the sequence has started, which also asserts this. 
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (TogglePauseSnippet in TPI_SequenceMenuController)");
                return;
            }

            if(_sequenceState == SequenceState.running || _sequenceState == SequenceState.paused) {

                if (_sequenceState == SequenceState.paused) { // Unpause Sequence

                    isPaused = false;
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().MainLabelText = "Pause Button";
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().SetQuadIcon(PauseIcon);

                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                            constraint.ApplyConstraint();
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                            constraint.ApplyConstraint();
                    }

                    // Enable the Rotating Orbs -> Indicate that it is running
                    SetRotatingOrbsState(true);

                    _sequenceState = SequenceState.running;

                    // Unlock Emergency Stop Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false);

                } else { // Pause Sequence

                    isPaused = true;
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().MainLabelText = "Unpause Button";
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().SetQuadIcon(UnpauseIcon);
                    

                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                            constraint.StopConstraint();
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                            constraint.StopConstraint();
                    }

                    // Disable the Rotating Orbs -> Indicate that it is not running
                    SetRotatingOrbsState(false);

                    _sequenceState = SequenceState.paused;

                    // Lock Emergency Stop Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, true);

                }

            } else {
                if(reportError)
                    Debug.LogError("The sequence is currently not running or paused! (TogglePauseSnippet in TPI_SequenceMenuController)");
                return;
            }

        }

        /// <summary>
        /// Skips the current snippet and starts the next one.
        /// <br></br>Please only run this function if the sequence is either running or paused.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void SkipSnippet(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running (StartSequence also asserts this).
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (SkipSnippet in TPI_SequenceMenuController)");
                return;
            }

            if (_sequenceState == SequenceState.running) {

                if (currentStep < _snippetObjects.Count - 1) {

                    // Stop current snippet function & start the next one
                    StopCoroutine(sequenceCoroutine);
                    sequenceCoroutine = null;
                    _snippetObjects[currentStep].SkipSnippet();
                    _snippetObjects[currentStep].StopSnippet();
                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                            constraint.StopConstraint();
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep + 1].snippetInformation.snippetID)
                            constraint.ApplyConstraint();
                    }
                    currentStep++;
                    sequenceCoroutine = StartCoroutine(InvokeSequence());

                    // Change Button Highlight
                    SetHighlightButtonState(_visualSnippetObjects[currentStep - 1].transform.GetChild(0).gameObject, false);
                    SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, true);

                    // Unlock Go Back Button if it is not the first snippet
                    if(currentStep >= 1)
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, false);

                    // Lock Skip Button if it is the last snippet
                    if (currentStep == _snippetObjects.Count - 1)
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, true);

                } else {
                    StopSequence();
                }

            } else if (_sequenceState == SequenceState.paused) {

                if (currentStep < _snippetObjects.Count - 1) {

                    StopCoroutine(sequenceCoroutine);
                    sequenceCoroutine = null;
                    _snippetObjects[currentStep].SkipSnippet();
                    _snippetObjects[currentStep].StopSnippet();
                    currentStep++;
                    foreach (TPI_Constraint constraint in _constraintObjects) {
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                            constraint.ApplyConstraint();
                        if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                            constraint.ApplyConstraint();
                    }
                    sequenceCoroutine = StartCoroutine(InvokeSequence());

                    // Unpause Sequence
                    isPaused = false;
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().MainLabelText = "Pause Button";
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().SetQuadIcon(PauseIcon);
                    _sequenceState = SequenceState.running;

                    // Change Button Highlight
                    SetHighlightButtonState(_visualSnippetObjects[currentStep - 1].transform.GetChild(0).gameObject, false);
                    SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, true);

                    // Turn on the Rotating Orbs
                    SetRotatingOrbsState(true);

                    // Unlock Go Back Button if it is not the first snippet
                    if (currentStep >= 1)
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, false);

                    // Unlock Emergency Stop Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false);

                    // Lock Skip Button if it is the last snippet
                    if (currentStep == _snippetObjects.Count - 1)
                        SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, true);

                } else {
                    StopSequence();
                }

            } else {
                if(reportError)
                    Debug.LogError("The sequence is currently not running or paused! (SkipSnippet in TPI_SequenceMenuController)");
                return;
            }
                
        }

        /// <summary>
        /// Repeates the current snippet by adding a copy of it at the next position in the sequence.
        /// <br></br>Please only run this function if the sequence is either running or paused.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void RepeatSnippet(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running (StartSequence also asserts this).
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (RepeatSnippet in TPI_SequenceMenuController)");
                return;
            }

            if (_sequenceState == SequenceState.running || _sequenceState == SequenceState.paused) {

                InsertSnippetAt(_snippetObjects[currentStep], currentStep + 1);

            } else {
                if(reportError)
                    Debug.LogError("The sequence is currently not running or paused! (RepeatSnippet in TPI_SequenceMenuController)");
                return;
            }

        }

        /// <summary>
        /// Stops the current snippet and starts the previous one.
        /// <br></br>Please only run this function if the sequence is either running or paused.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void ReturnToPreviousSnippet(bool reportError = true) {

            if (_snippetObjects.Count == 0) {  // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running (StartSequence also asserts this).
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (ReturnToPreviousSnippet in TPI_SequenceMenuController)");
                return;
            }

            if (_sequenceState == SequenceState.running || _sequenceState == SequenceState.paused) { // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running or paused

                if (currentStep - 1 < 0) {  // Just to make sure, generally this should not happen as the button will be locked when the sequence has not reached the second snippet.
                    if(reportError)
                        Debug.LogError("The snippet sequence list has not reached the second snippet! (ReturnToPreviousSnippet in TPI_SequenceMenuController)");
                    return;
                }

                // Unlock Skip Button if it was the last snippet
                if (currentStep == _snippetObjects.Count - 1)
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, false);

                StopCoroutine(sequenceCoroutine);
                sequenceCoroutine = null;
                _snippetObjects[currentStep].StopSnippet();
                foreach (TPI_Constraint constraint in _constraintObjects) {
                    if (_sequenceState == SequenceState.paused && constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                        constraint.ApplyConstraint();
                    if (_sequenceState == SequenceState.running && constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                        constraint.StopConstraint();
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep - 1].snippetInformation.snippetID)
                        constraint.ApplyConstraint();
                }
                currentStep--;
                sequenceCoroutine = StartCoroutine(InvokeSequence());

                // Change Button Highlight
                SetHighlightButtonState(_visualSnippetObjects[currentStep + 1].transform.GetChild(0).gameObject, false);
                SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, true);

                // Lock Go Back Button if it is the first snippet
                if (currentStep == 0)
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, true);

                // Turn on the Rotating Orbs
                if (_sequenceState == SequenceState.paused)
                    SetRotatingOrbsState(true);

                // Unpause Sequence
                if (_sequenceState == SequenceState.paused) {
                    isPaused = false;
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().MainLabelText = "Pause Button";
                    coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).GetComponent<ButtonConfigHelper>().SetQuadIcon(PauseIcon);
                    _sequenceState = SequenceState.running;
                    // Unlock Emergency Stop Button
                    SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false);
                }

            } else {
                if(reportError)
                    Debug.LogError("The sequence is currently not running or paused! (ReturnToPreviousSnippet in TPI_SequenceMenuController)");
                return;
            }

        }

        /// <summary>
        /// In case of an emergency, it stops the sequence of snippets and the constraints.
        /// <br></br>This function is different to the StopSequence function in the regard that the users of the TPI can define special behaviours.
        /// <para><paramref name="reportError"/> = true -> default case to log errors in the console, false -> used for the hand gestures (no errors are logged)</para>
        /// </summary>
        public void EmergencyStopSnippet(bool reportError = true) {

            if (_snippetObjects.Count == 0) { // Just to make sure, generally this should not happen as the button will be locked when the sequence is not running (StartSequence also asserts this).
                if(reportError)
                    Debug.LogError("The snippet sequence list is empty! (EmergencyStopSnippet in TPI_SequenceMenuController)");
                return;
            }

            if (_sequenceState == SequenceState.running) {

                // Stop Snippet Function and the Constraints
                StopCoroutine(sequenceCoroutine);
                sequenceCoroutine = null;
                _snippetObjects[currentStep].OnEmergencyStop();
                foreach (TPI_Constraint constraint in _constraintObjects) {
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.global)
                        constraint.StopConstraint();
                    if (constraint.constraintInformation.constraintType == TPI_ConstraintType.snippetSpecific && constraint.constraintInformation.snippetID == _snippetObjects[currentStep].snippetInformation.snippetID)
                        constraint.StopConstraint();
                }

                _sequenceState = SequenceState.stopped;
                isPaused = false;

                // Change Stop Button to Restart Button
                coroutineButtonsContainerPath.transform.GetChild(3).GetChild(0).gameObject.SetActive(false); // Hide Stop Button
                coroutineButtonsContainerPath.transform.GetChild(3).GetChild(3).gameObject.SetActive(true); // Show Restart Button

                // Change TogglePause Button to Start Button and lock Start Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(2).gameObject, true); // Lock Start Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(0).gameObject.SetActive(true); // Show Start Button
                coroutineButtonsContainerPath.transform.GetChild(2).GetChild(3).gameObject.SetActive(false); // Hide Toggle Pause Button

                // Lock the other Coroutine Buttons
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(0).gameObject, false); // Unlock Visualize Snippets Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(1).gameObject, false); // Unlock Visualize Constraints Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(4).gameObject, true); // Lock Repeat Button
                SetLockButtonState(coroutineButtonsContainerPath.transform.GetChild(5).gameObject, true); // Lock Skip Button

                // Enable the Selection Menu & Building Blocks Menu to allow adding snippets / constraints when the sequence is stopped
                mainController.selectionMenu.SetActive(true);
                TPI_ObjectPlacementController objectPlacementController = mainController.objectPlacementController.GetComponent<TPI_ObjectPlacementController>();
                objectPlacementController.ReserveSpot(objectPlacementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), mainController.selectionMenu, true);
                if (mainController.GetComponent<TPI_WorkflowConfigurationController>().IsBuildingBlocksMenuOpen()) {
                    mainController.buildingBlocksMenu.SetActive(true);
                    objectPlacementController.ReserveSpot(objectPlacementController.GetSpotRight(objectPlacementController.ConvertAnchorToPosition(TPI_ObjectPlacementController.StartingPosition.MiddleCenter), 1), mainController.buildingBlocksMenu, true);
                }

                // Change EmergencyStop and ReturnToPreviousSnippet to Visualization Buttons
                coroutineButtonsContainerPath.transform.GetChild(0).GetChild(0).gameObject.SetActive(true); // Show Visualize Snippets Button
                coroutineButtonsContainerPath.transform.GetChild(0).GetChild(3).gameObject.SetActive(false); // Hide Emergency Stop Button
                coroutineButtonsContainerPath.transform.GetChild(1).GetChild(0).gameObject.SetActive(true); // Show Visualize Constraints Button
                coroutineButtonsContainerPath.transform.GetChild(1).GetChild(3).gameObject.SetActive(false); // Hide Return To Previous Button

                // Disable Button Hightlight
                SetHighlightButtonState(_visualSnippetObjects[currentStep].transform.GetChild(0).gameObject, false);

                // Disable the Rotating Orbs -> Indicate that it is not running
                SetRotatingOrbsState(false);

            } else if (_sequenceState == SequenceState.paused) {
                if(reportError)
                    Debug.LogError("Please use the Stop button instead as the sequence is currently paused and therefore poses no emergency! (EmergencyStopSnippet in TPI_SequenceMenuController)");
                return;
            } else {
                if(reportError)
                    Debug.LogError("The sequence is currently not running or paused! (EmergencyStopSnippet in TPI_SequenceMenuController)");
                return;
            }

        }
        #endregion SequenceFunctions

        /// <summary>
        /// Helper enum that gives the user to choice to determine what happens after a snippet has finished executing.
        /// </summary>
        private enum SnippetProgression {
            [InspectorName("Pause after each Snippet")]
            pause, // 0
            [InspectorName("Automatically start next")]
            automatically, // 1
            [InspectorName("Start next Snippet with Delay")]
            delay, // 2
        }

        /// <summary>
        /// Helper enum that acts as a status indication between the different states of the sequence.
        /// </summary>
        public enum SequenceState {
            [InspectorName("Sequence is not running")]
            notStarted, // 0
            [InspectorName("Sequence is running")]
            running, // 1
            [InspectorName("Sequence is paused")]
            paused, // 2
            [InspectorName("Sequence has stopped.")]
            stopped, // 3
            [InspectorName("Sequence is restarting.")]
            restarting, // 4
        }

    }

}