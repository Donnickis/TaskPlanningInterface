using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TaskPlanningInterface.EditorAndInspector;
using UnityEngine;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// The ObjectPlacementController was created in order to easily control the position of objects and dialog menus in the environment.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this class.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [RequireComponent(typeof(GridObjectCollection))]
    public class TPI_ObjectPlacementController : MonoBehaviour {

        [Tooltip("How many Spots should be available for objects and dialog menus?")][Min(1)]
        [SerializeField] private int availableSpots = 15;

        [Tooltip("How many coloumns should there be in total?")][Min(1)][Rename("Number of Columns")]
        [SerializeField] private int _numCol = 5;
        [Tooltip("Automatically shows you how many rows will be visible in the GridObjectCollection.")][Min(1)][ReadOnly]
        [SerializeField] private int _numRow; // number of rows -> calculated in Start()

        [Tooltip("How much space should the spots take up in the width?")][Range(0.1f, 1f)]
        [SerializeField] private float cellWidth = 0.4f;
        [Tooltip("How much space should the spots take up in the height?")][Range(0.1f, 1f)]
        [SerializeField] private float cellHeight = 0.3f;

        [Tooltip("Decide whether the GridObjectCollection should automatically follow the operator around the environment. All the attached content will also be move if they have not been moved away / detached individually. This can also be manually activated and deactivated by the operator at runtime.")]
        public bool attachToOperator;

        [Tooltip("Decide whether the GridObjectCollection should automatically rotate with the operator. All the attached content will also be rotated if they have not been moved away / detached individually. This can also be manually activated and deactivated by the operator at runtime.")]
        public bool rotateWithOperator;

        [Tooltip("Determine the distance of the GridObjectCollection to the operators view")][Min(0f)]
        public float distanceToCamera = 0f;
        [Tooltip("How much smooth time should the GridObjectCollection have while following the operator?")][Range(0, 2)]
        public float smoothTime = 0.25f;
        private Vector3 velocity = Vector3.zero; // Velocity of the GridObjectCollection -> only needed internally

        [Tooltip("How fast should the GridObjectCollection rotate?")][Range(0.01f, 1)]
        public float rotationSpeed = 0.1f;
        
        private Camera headCamera;

        // List of GameObjects that currently take up the spots -> index = child order of the GridObjectCollection
        private List<GameObject> _occupyingObjects;

        private int previousPositionIndex; // Position of the last removed Dialog Menu in the Grid

        /*[Space]
        [Header("Debug")]
        [Space]
        public StartingPosition _startingPosition;
        public SearchAlgorithm _searchAlgorithm;
        public SearchDirection _searchDirection;
        public int numShouldOccupy;

        public void DebugPlacement() {
        
            if(GetNumUnoccupiedSpots() != availableSpots) {
                for (int i = 0; i < availableSpots; i++) {
                    if (_occupyingObjects[i] == null)
                        continue;
                    Destroy(_occupyingObjects[i]);
                    _occupyingObjects[i] = null;
                }
            }

            for (int i = 0; i < numShouldOccupy; i++) {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.GetComponent<MeshRenderer>().material.color = Color.red;
                go.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                TPI_PositionAndRotation pose = FindAndReservePosition(go, _startingPosition, _searchAlgorithm, _searchDirection);
                if (pose != null) {
                    go.transform.position = pose.position;
                    go.transform.rotation = pose.rotation;
                } else
                    Destroy(go);
            }

        }*/

        // Reference to TPI_MainController Component
        private TPI_MainController mainController;

        private void Start() {

            // Setup of general variables
            mainController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>();
            _occupyingObjects = new List<GameObject>();
            _numRow = (int)Mathf.Ceil(((float)availableSpots) / _numCol);
            headCamera = Camera.main;

            previousPositionIndex = ConvertAnchorToPosition(StartingPosition.MiddleCenter);

            // Configuration of GridObjectCollection
            GridObjectCollection collection = gameObject.GetComponent<GridObjectCollection>();
            collection.IgnoreInactiveTransforms = true;
            collection.SortType = CollationOrder.ChildOrder;
            collection.SurfaceType = ObjectOrientationSurfaceType.Cylinder;
            collection.OrientType = OrientationType.FaceOrigin;
            collection.Layout = LayoutOrder.ColumnThenRow;
            collection.Columns = _numCol;
            collection.ColumnAlignment = LayoutHorizontalAlignment.Left;
            collection.CellWidth = cellWidth;
            collection.CellHeight = cellHeight;
            collection.Radius = 1;
            collection.RadialRange = 180;
            collection.Anchor = LayoutAnchor.MiddleCenter;

            // Destroy all children
            if (transform.childCount != 0) {
                for (int i = 0; i < transform.childCount; i++) {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            // Create new children
            for (int i = 0; i < availableSpots; i++) {
                GameObject spotObject = new GameObject(); // replace with GameObject.CreatePrimitive(PrimitiveType.Cube) if you want to display the spots
                spotObject.transform.parent = transform;
                spotObject.name = "Spot " + i.ToString();

                spotObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

                // Prepare list of spots
                _occupyingObjects.Add(null);
            }
            StartCoroutine(InvokeLateUpdateCollection());
        }

        /// <summary>
        /// Return the position index of the last GameObject that was removed from the Grid.
        /// </summary>
        public int GetPreviousPositionIndex() {
            return previousPositionIndex;
        }

        /// <summary>
        /// Resets the previous position index, so that new dialog menus will be spawned in the Center Middle.
        /// </summary>
        public void ResetPreviousPositionIndex() {
            previousPositionIndex = ConvertAnchorToPosition(StartingPosition.MiddleCenter);
        }

        /// <summary>
        /// This function can be called with the help of the checkbox in the ObjectPlacementHandMenu.
        /// <br></br> It activates the feature that the GridObjectColllection follows the operator.
        /// <para><paramref name="status"/> : status = true ->  move with operator, status = false -> no longer move with operator</para>
        /// </summary>
        public void SetAttachmentState(bool status) {
            attachToOperator = status;
        }

        /// <summary>
        /// This function can be called with the help of the checkbox in the ObjectPlacementHandMenu.
        /// <br></br> It activates the feature that the GridObjectColllection rotates with the operator.
        /// <para><paramref name="status"/> : status = true ->  rotate with operator, status = false -> no longer rotate with operator</para>
        /// </summary>
        public void SetRotationState(bool status) {
            rotateWithOperator = status;
        }

        private void FixedUpdate() {

            // If attachToOperator is set to true, the GridObjectCollection and its content will follow the operator around the environment
            // Please keep in mind that the elements of the grid automatically snap back to their positions if they have been moved
            if (attachToOperator) {

                Vector3 currentGridDirection = (transform.rotation * (transform.position - headCamera.transform.position).normalized).normalized;
                Vector3 targetPosition = headCamera.transform.position + distanceToCamera * currentGridDirection;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            }

            // If rotateWithOperator se to true, the GridObjectCollection will rotate with the operator
            if (rotateWithOperator) {
                transform.rotation = Quaternion.Slerp(transform.rotation, headCamera.transform.rotation, rotationSpeed);
            }

            if(attachToOperator || rotateWithOperator) {
                // Move Content
                for (int i = 0; i < availableSpots; i++) {
                    if (_occupyingObjects[i] == null)
                        continue;
                    _occupyingObjects[i].transform.position = transform.GetChild(i).position;
                    _occupyingObjects[i].transform.rotation = transform.GetChild(i).rotation;
                }
            }

        }

        /// <summary>
        /// This IEnumerator fixes a problem with the UpdateCollection function of the GridObjectCollection script, moving the update to the next frame.
        /// </summary>
        private IEnumerator InvokeLateUpdateCollection() {
            yield return new WaitForEndOfFrame();
            GetComponent<GridObjectCollection>().UpdateCollection();
        }

        /// <summary>
        /// Converts the index (int) of a spot into the Vector3 of it at that index.
        /// <para><paramref name="position"/> = position index that should be converted into the position (Vector3)</para>
        /// </summary>
        /// <returns>Vector3 of the position</returns>
        public Vector3 ConvertSpotIntToVector3(int position) {
            if (position > _occupyingObjects.Count - 1 || position < 0) {
                Debug.LogError("The provided position is out of bounds! Returning zero Vector3... (ConvertSpotIntToVector3 in TPI_ObjectPlacementController)");
                return Vector3.zero;
            }
            return transform.GetChild(position).position;
        }

        /// <summary>
        /// Converts the Vector3 of a spot into the position index (int) of it.
        /// <br></br>IMPORTANT: This function only works for the exact position of the spot! (floating point errors are also considered, anything larger than that however will end in a output of -1)
        /// <para><paramref name="position"/> = position that should be converted into the index (int)</para>
        /// <para><paramref name="logError"/> = indicate whether an error should be thrown if the spot could not be converted into the position index</para>
        /// </summary>
        /// <returns>index (int) of the position</returns>
        public int ConvertSpotVector3ToInt(Vector3 position, bool logError = true) {
            int pos = -1;
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).position == position) {
                    pos = i;
                    break;
                }

            }
            if (logError && pos == -1) { // Position not found
                Debug.LogError("The provided Vector3 position does not fit to any available spot! Returning -1... (ConvertSpotVector3ToInt in TPI_ObjectPlacementController)");
            }
            return pos;
        }

        /// <summary>
        /// Looks for the GameObject in the Grid and return the index of it.
        /// <para><paramref name="gameObject"/> = GameObject of which you would like to obtain the position index</para>
        /// <para><paramref name="logError"/> = indicate whether the ObjectPlacementController should use Debug.LogError (to indicate errors)</para>
        /// </summary>
        /// <returns>Index of the GameObject in the Grid (int)</returns>
        public int GetIndexOfGameObject(GameObject gameObject, bool logError = true) {
            int index = -1;
            for (int i = 0; i < _occupyingObjects.Count; i++) {
                if (_occupyingObjects[i] == gameObject) {
                    index = i;
                    break;
                }
            }
            if (index == -1 && logError)
                Debug.LogWarning("The provided GameObject does not currently take up a spot in the grid! Returning -1... (GetIndexOfGameObject in TPI_ObjectPlacementController)");
            return index;
        }

        /// <summary>
        /// Calculates the total number of unoccupied spots, and returns the value.
        /// </summary>
        /// <returns>number of total unoccupied spots (int)</returns>
        public int GetNumUnoccupiedSpots() {
            int num = 0;
            foreach (GameObject go in _occupyingObjects) {
                if (go == null)
                    num++;
            }
            return num;
        }

        /// <summary>
        /// Calculates the number of unoccupied spots for a given starting position, search algorithm and search direction, and returns the value.
        /// <para><paramref name="startingPosition"/> = select the starting position from 9 different points and from which the search algorithm starts working</para>
        /// <para><paramref name="searchAlgorithm"/> = select the search algorithm that should be applied</para>
        /// <para><paramref name="searchDirection"/> = select the search direction that should be used</para>
        /// </summary>
        /// <returns>number of unoccupied spots for a specified starting position, search algorithm and search direction (int)</returns>
        public int GetNumUnoccupiedSpots(StartingPosition startingPosition = StartingPosition.MiddleCenter, SearchAlgorithm searchAlgorithm = SearchAlgorithm.closestPosition, SearchDirection searchDirection = SearchDirection.bothWays) {
            return GetNumUnoccupiedSpots(ConvertAnchorToPosition(startingPosition), searchAlgorithm, searchDirection);
        }

        /// <summary>
        /// Calculates the number of unoccupied spots for a given starting position, search algorithm and search direction, and returns the value.
        /// <para><paramref name="startingPosition"/> = select the starting position from which the search algorithm starts working</para>
        /// <para><paramref name="searchAlgorithm"/> = select the search algorithm that should be applied</para>
        /// <para><paramref name="searchDirection"/> = select the search direction that should be used</para>
        /// </summary>
        /// <returns>number of unoccupied spots for a specified starting position, search algorithm and search direction (int)</returns>
        public int GetNumUnoccupiedSpots(int startingPosition, SearchAlgorithm searchAlgorithm = SearchAlgorithm.closestPosition, SearchDirection searchDirection = SearchDirection.bothWays) {

            int count = 0;

            if (searchAlgorithm == SearchAlgorithm.horizontally) { // Search leftwards & rightwards
                if (searchDirection == SearchDirection.upOrLeft) {
                    for (int i = 0; i < startingPosition; i++) {
                        if (_occupyingObjects[i] == null)
                            count++;
                    }
                } else if (searchDirection == SearchDirection.downOrRight) {
                    for (int i = startingPosition; i < availableSpots; i++) {
                        if (_occupyingObjects[i] == null)
                            count++;
                    }
                } else {
                    count = GetNumUnoccupiedSpots();
                }

            } else if (searchAlgorithm == SearchAlgorithm.vertically) { // Search upwards & downwards
                if (searchDirection == SearchDirection.upOrLeft) {
                    int currentPosition = startingPosition;
                    for (int i = 0; i < availableSpots; i++) {
                        if (currentPosition == -1)
                            break;
                        if(_occupyingObjects[currentPosition] == null)
                            count++;
                        currentPosition = GetSpotUp(currentPosition, 1, true);
                    }
                } else if (searchDirection == SearchDirection.downOrRight) {
                    int currentPosition = startingPosition;
                    for (int i = 0; i < availableSpots; i++) {
                        if (currentPosition == availableSpots)
                            break;
                        if (_occupyingObjects[currentPosition] == null)
                            count++;
                        currentPosition = GetSpotDown(currentPosition, 1, true);
                    }
                } else {
                    count = GetNumUnoccupiedSpots();
                }
            } else if (searchAlgorithm == SearchAlgorithm.closestPosition) { // search in any direction for the closes point (physical distance from starting point to the other points), search direction does not matter here
                count = GetNumUnoccupiedSpots();
            }

            return count;
        }


        /// <summary>
        /// Finds a suitable unoccupied spot according to your anchor point, search algorithm and search direction and returns the index.
        /// <para><paramref name="startingPosition"/> = select the starting position from 9 different points and from which the search algorithm starts working</para>
        /// <para><paramref name="searchAlgorithm"/> = select the search algorithm that should be applied</para>
        /// <para><paramref name="searchDirection"/> = select the search direction that should be used</para>
        /// <para><paramref name="logErrors"/> = indicate whether the ObjectPlacementController should use Debug.LogError (to indicate errors)</para>
        /// </summary>
        /// <returns>position index of the unoccupied spot, which fits best to your provided anchor point and search direction</returns>
        public int FindUnoccupiedSpot(StartingPosition startingPosition = StartingPosition.MiddleCenter, SearchAlgorithm searchAlgorithm = SearchAlgorithm.closestPosition, SearchDirection searchDirection = SearchDirection.bothWays, bool logErrors = true) {
            return FindUnoccupiedSpot(ConvertAnchorToPosition(startingPosition), searchAlgorithm, searchDirection, logErrors);
        }


        /// <summary>
        /// Finds a suitable unoccupied spot according to your anchor point, search algorithm and search direction and returns the index.
        /// <para><paramref name="startingPosition"/> = select the starting position from which the search algorithm starts working</para>
        /// <para><paramref name="searchAlgorithm"/> = select the search algorithm that should be applied</para>
        /// <para><paramref name="searchDirection"/> = select the search direction that should be used</para>
        /// <para><paramref name="logErrors"/> = indicate whether the ObjectPlacementController should use Debug.LogError (to indicate errors)</para>
        /// </summary>
        /// <returns>position index of the unoccupied spot, which fits best to your provided anchor point and search direction</returns>
        public int FindUnoccupiedSpot(int startingPosition, SearchAlgorithm searchAlgorithm = SearchAlgorithm.closestPosition, SearchDirection searchDirection = SearchDirection.bothWays, bool logErrors = true) {

            // Check if there are any unoccupied spots left
            if (GetNumUnoccupiedSpots(startingPosition, searchAlgorithm, searchDirection) == 0) {
                if(logErrors)
                    Debug.LogError("All spots for the chosen starting position, search algorithm and search direction are currently occupied! Returning -1... (FindUnoccupiedSpot in TPI_ObjectPlacementController)");
                return -1;
            }

            if (_occupyingObjects[startingPosition] == null)
                return startingPosition;

            int unoccupiedSpot = -1;
            if (searchAlgorithm == SearchAlgorithm.horizontally) { // Search leftwards & rightwards

                int step = 1;
                for(int i = 0; i < availableSpots; i++) {

                    if (searchDirection == SearchDirection.bothWays || searchDirection == SearchDirection.upOrLeft) {
                        // Search leftwards
                        int goLeft = GetSpotLeft(startingPosition, step, true);
                        if (goLeft != -1 && _occupyingObjects[goLeft] == null) {
                            unoccupiedSpot = goLeft;
                            break;
                        }
                    }

                    if (searchDirection == SearchDirection.bothWays || searchDirection == SearchDirection.downOrRight) {
                        // Search rightwards
                        int goRight = GetSpotRight(startingPosition, step, true);
                        if (goRight != availableSpots && _occupyingObjects[goRight] == null) {
                            unoccupiedSpot = goRight;
                            break;
                        }
                    }

                    step++;

                }

            } else if (searchAlgorithm == SearchAlgorithm.vertically) { // Search upwards & downwards

                int step = 1;
                for (int i = 0; i < availableSpots; i++) {

                    if (searchDirection == SearchDirection.bothWays || searchDirection == SearchDirection.upOrLeft) {
                        // Search upwards
                        int goUp = GetSpotUp(startingPosition, step, true);
                        if (goUp != -1 && _occupyingObjects[goUp] == null) {
                            unoccupiedSpot = goUp;
                            break;
                        }
                    }

                    if (searchDirection == SearchDirection.bothWays || searchDirection == SearchDirection.downOrRight) {
                        // Search downwards
                        int goDown = GetSpotDown(startingPosition, step, true);
                        if (goDown != availableSpots && _occupyingObjects[goDown] == null) {
                            unoccupiedSpot = goDown;
                            break;
                        }
                    }

                    step++;

                }

            } else if (searchAlgorithm == SearchAlgorithm.closestPosition) { // search in any direction for the closes point (physical distance from starting point to the other points), search direction does not matter here

                List<int> closestPoints = new List<int>();
                float closestDistance = float.MaxValue;

                // look for closest point(s)
                for (int i = 0; i < transform.childCount; i++) {
                    if (_occupyingObjects[i] != null)
                        continue;
                    float distance = Vector3.Distance(transform.GetChild(startingPosition).position, transform.GetChild(i).position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestPoints.Clear();
                        closestPoints.Add(i);
                    } else if (distance == closestDistance)
                        closestPoints.Add(i);
                }

                // Choose a point from the list of closest points
                if (closestPoints.Count > 1) {
                    unoccupiedSpot = closestPoints[Random.Range(0, closestPoints.Count)];
                } else { // one point is closer than all the others
                    unoccupiedSpot = closestPoints[0];
                }

            }

            return unoccupiedSpot;
        }

        /// <summary>
        /// Converts the provided anchor point to its index in the list.
        /// <para><paramref name="startingPosition"/> = select the StartingPosition anchir that should be converted in to the starting position index</para>
        /// </summary>
        /// <returns>index of the position (int)</returns>
        public int ConvertAnchorToPosition(StartingPosition startingPosition) {

            int remainder = availableSpots % _numCol;
            if(remainder == 0)
                remainder = _numCol;
            int middleRow = Mathf.CeilToInt(((float)_numRow) / 2);

            switch (startingPosition) {
                case StartingPosition.UpperLeft:
                    return 0;
                case StartingPosition.UpperCenter:
                    return Mathf.CeilToInt(((float)_numCol) / 2) - 1;
                case StartingPosition.UpperRight:
                    return _numCol - 1;
                case StartingPosition.MiddleLeft:
                    return (middleRow - 1) * _numCol;
                case StartingPosition.MiddleCenter:
                    return (middleRow - 1) * _numCol + Mathf.CeilToInt(((float)_numCol) / 2) - 1;
                case StartingPosition.MiddleRight:
                    return middleRow * _numCol - 1;
                case StartingPosition.BottomLeft:
                    return (_numRow - 1) * _numCol;
                case StartingPosition.BottomCenter:
                    return (_numRow - 1) * _numCol + Mathf.CeilToInt(((float)remainder) / 2) - 1;
                case StartingPosition.BottomRight:
                    return availableSpots - 1;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Moves from an initial position upwards along a column and return the index of that spot.
        /// <para><paramref name="initialPosition"/> = indicated the position from which the shift should happen</para>
        /// <para><paramref name="step"/> = indicated how many steps should be done in the upward direction during this shift</para>
        /// <para><paramref name="goOutOfBounds"/>: goOutOfBounds = true -> it can return -1 or the max possible index + 1 (useful if you want to catch the border cases), goOutOfBounds = false -> output limited to the possible position indices</para>
        /// </summary>
        /// <returns>index of the desired spot</returns>
        public int GetSpotUp(int initialPosition, int step, bool goOutOfBounds = false) {
            int newPosition = initialPosition;
            for (int stp = 0; stp < step; stp++) {
                newPosition -= _numCol;
                if (newPosition < 0) {
                    int currentColumn = (newPosition + _numCol) % _numCol;
                    if (currentColumn > 0) {
                        newPosition = (_numRow - 1) * _numCol + currentColumn - 1;
                    } else {
                        if (!goOutOfBounds) {
                            Debug.LogWarning("The resulting position from initialPosition = " + initialPosition.ToString() + " and step = " + step + " is out of bounds! Returning position 0 (top left)... (GoUpGrid in TPI_ObjectPlacementController)");
                            return 0;
                        } else
                            return -1;
                    }

                }

            }
            return newPosition;
        }

        /// <summary>
        /// Moves from an initial position downwards along a column and return the index of that spot.
        /// <para><paramref name="initialPosition"/> = indicated the position from which the shift should happen</para>
        /// <para><paramref name="step"/> = indicated how many steps should be done in the downward direction during this shift</para>
        /// <para><paramref name="goOutOfBounds"/>: goOutOfBounds = true -> it can return -1 or the max possible index + 1 (useful if you want to catch the border cases), goOutOfBounds = false -> output limited to the possible position indices</para>
        /// </summary>
        /// <returns>index of the desired spot</returns>
        public int GetSpotDown(int initialPosition, int step, bool goOutOfBounds = false) {
            int newPosition = initialPosition;
            for (int stp = 0; stp < step; stp++) {
                newPosition += _numCol;
                if (newPosition > availableSpots - 1) {
                    int currentColumn = (newPosition - _numCol) % _numCol;
                    if (currentColumn < _numCol - 1) {
                        newPosition = currentColumn + 1;
                    } else {
                        if (!goOutOfBounds) {
                            Debug.LogWarning("The resulting position from initialPosition = " + initialPosition.ToString() + " and step = " + step + " is out of bounds! Returning position of last item (bottom right).. (GoDownGrid in TPI_ObjectPlacementController)");
                            return availableSpots - 1;
                        } else
                            return availableSpots;
                    }
                }
            }
            return newPosition;
        }

        /// <summary>
        /// Moves from an initial position leftwards along a row and return the index of that spot.
        /// <para><paramref name="initialPosition"/> = indicated the position from which the shift should happen</para>
        /// <para><paramref name="step"/> = indicated how many steps should be done in the leftward direction during this shift</para>
        /// <para><paramref name="goOutOfBounds"/>: goOutOfBounds = true -> it can return -1 or the max possible index + 1 (useful if you want to catch the border cases), goOutOfBounds = false -> output limited to the possible position indices</para>
        /// </summary>
        /// <returns>index of the desired spot</returns>
        public int GetSpotLeft(int initialPosition, int step, bool goOutOfBounds = false) {
            int newPosition = initialPosition - step;
            if (newPosition < 0) {
                if (!goOutOfBounds) {
                    Debug.LogWarning("The resulting position from initialPosition = " + initialPosition.ToString() + " and step = " + step + " is out of bounds! Returning position 0 (top left).. (GoLeftGrid in TPI_ObjectPlacementController)");
                    return 0;
                } else
                    return -1;
            }
            return newPosition;
        }

        /// <summary>
        /// Moves from an initial position rightwards along a row and return the index of that spot.
        /// <para><paramref name="initialPosition"/> = indicated the position from which the shift should happen</para>
        /// <para><paramref name="step"/> = indicated how many steps should be done in the rightward direction during this shift</para>
        /// <para><paramref name="goOutOfBounds"/>: goOutOfBounds = true -> it can return -1 or the max possible index + 1 (useful if you want to catch the border cases), goOutOfBounds = false -> output limited to the possible position indices</para>
        /// </summary>
        /// <returns>index of the desired spot</returns>
        public int GetSpotRight(int initialPosition, int step, bool goOutOfBounds = false) {
            int newPosition = initialPosition + step;
            if (newPosition > availableSpots - 1) {
                if (!goOutOfBounds) {
                    Debug.LogWarning("The resulting position from initialPosition = " + initialPosition.ToString() + " and step = " + step + " is out of bounds! Returning position of last item (bottom right).. (GoRightGrid in TPI_ObjectPlacementController)");
                    return availableSpots - 1;
                } else
                    return availableSpots;
            }
            return newPosition;
        }

        /// <summary>
        /// Reserves a spot for a GameObject by providing the TPI_PositionAndRotation of the desired spot.
        /// <para><paramref name="pose"/> = indicate in which spot the GameObject should be placed (TPI_PositionAndRotation needs to include the exact position; the rotation does not matter)</para>
        /// <para><paramref name="reservingObject"/> = GameObject that should be placed inside the Grid</para>
        /// <para><paramref name="forceOverride"/> = indicated whether the spot should be occupied by this new object even if the spot was already occupied -> tries to relocate the other object</para>
        /// </summary>
        public void ReserveSpot(PositionAndRotation pose, GameObject reservingObject, bool forceOverride = false, bool applyPose = false) {
            ReserveSpot(pose.position, reservingObject, forceOverride, applyPose);
        }

        /// <summary>
        /// Reserves a spot for a GameObject by providing the Vector3 of the desired spot.
        /// <para><paramref name="position"/> = indicate in which spot the GameObject should be placed (position Vector3)</para>
        /// <para><paramref name="reservingObject"/> = GameObject that should be placed inside the Grid</para>
        /// <para><paramref name="forceOverride"/> = indicated whether the spot should be occupied by this new object even if the spot was already occupied -> tries to relocate the other object</para>
        /// </summary>
        public void ReserveSpot(Vector3 position, GameObject reservingObject, bool forceOverride = false, bool applyPose = false) {
            int pos = ConvertSpotVector3ToInt(position, true);
            if (pos == -1) {
                Debug.LogError("The provided Vector3 position does not fit to any available spot! (ReserveSpot in TPI_ObjectPlacementController)");
                return;
            }
            ReserveSpot(pos, reservingObject, forceOverride, applyPose);
        }

        /// <summary>
        /// Reserves a spot for a GameObject by providing the index (int) of the desired spot.
        /// <para><paramref name="position"/> = indicate in which spot the GameObject should be placed (position index)</para>
        /// <para><paramref name="reservingObject"/> = GameObject that should be placed inside the Grid</para>
        /// <para><paramref name="forceOverride"/> = indicated whether the spot should be occupied by this new object even if the spot was already occupied -> tries to relocate the other object</para>
        /// </summary>
        public void ReserveSpot(int position, GameObject reservingObject, bool forceOverride = false, bool applyPose = false) {

            if (position > _occupyingObjects.Count - 1 || position < 0) {
                Debug.LogError("The provided position is out of bounds! (ReserveSpot in TPI_ObjectPlacementController)");
                return;
            }
            if (_occupyingObjects[position] != null && !forceOverride) {
                Debug.LogWarning("The desired position is not empty! Please free it up first or set the optional parameter 'forceOverride' of type bool to true. (ReserveSpot in TPI_ObjectPlacementController)");
                return;
            }
            if(forceOverride && _occupyingObjects[position] != null) {
                PositionAndRotation tempPosition = FindAndReservePosition(_occupyingObjects[position], applyPose: true, logErrors: false, internalForceOverride: false); // Finds a new spot for the gameObject that is currently in the desired spot
                if (tempPosition == null) { // No Spots were left -> set position & rotation to the coordinate origin
                    _occupyingObjects[position].transform.position = Vector3.zero;
                    _occupyingObjects[position].transform.rotation = Quaternion.identity;
                }
            }
            _occupyingObjects[position] = reservingObject;
            if (applyPose) {
                reservingObject.transform.position = transform.GetChild(position).position;
                reservingObject.transform.rotation = transform.GetChild(position).rotation;
            }
        }

        /// <summary>
        /// Finds a spot that fits best to the provided anchor point (and search algorithm and search direction) and reserves it for the GameObject, returning the position and rotation of said spot.
        /// <para><paramref name="reservingObject"/> = GameObject for which a spot should be found</para>
        /// <para><paramref name="startingPosition"/> = select the starting position from 9 different points and from which the search algorithm starts working</para>
        /// <para><paramref name="searchAlgorithm"/> = select the search algorithm that should be applied</para>
        /// <para><paramref name="searchDirection"/> = select the search direction that should be used</para>
        /// <para><paramref name="applyPose"/> = indicate whether the position and rotation should be applied by this function or whether you want to do it yourself</para>
        /// <para><paramref name="logErrors"/> = indicate whether the function should be able to use Debug.LogError()</para>
        /// <para><paramref name="internalForceOverride"/> = please do not use! It is strictly for internal purposes of other functions (to prevent stack overflows)</para>
        /// </summary>
        /// <returns>TPI_PositionAndRotation of the unoccupied spot, which fits best to your provided anchor point and search direction</returns>
        public PositionAndRotation FindAndReservePosition(GameObject reservingObject, StartingPosition startingPosition = StartingPosition.MiddleCenter, SearchAlgorithm searchAlgorithm = SearchAlgorithm.closestPosition, SearchDirection searchDirection = SearchDirection.bothWays, bool applyPose = false, bool logErrors = true, bool internalForceOverride = false) {
            return FindAndReservePosition(reservingObject, ConvertAnchorToPosition(startingPosition), searchAlgorithm, searchDirection, applyPose, logErrors, internalForceOverride);
        }

        /// <summary>
        /// Finds a spot that fits best to the provided starting position (and search algorithm and search direction) and reserves it for the GameObject, returning the position and rotation of said spot.
        /// <para><paramref name="reservingObject"/> = GameObject for which a spot should be found</para>
        /// <para><paramref name="startingPosition"/> = select the starting position from which the search algorithm starts working</para>
        /// <para><paramref name="searchAlgorithm"/> = select the search algorithm that should be applied</para>
        /// <para><paramref name="searchDirection"/> = select the search direction that should be used</para>
        /// <para><paramref name="applyPose"/> = indicate whether the position and rotation should be applied by this function or whether you want to do it yourself</para>
        /// <para><paramref name="logErrors"/> = indicate whether the function should be able to use Debug.LogError()</para>
        /// <para><paramref name="internalForceOverride"/> = please do not use! It is strictly for internal purposes of other functions (to prevent stack overflows)</para>
        /// </summary>
        /// <returns>TPI_PositionAndRotation of the unoccupied spot, which fits best to your provided anchor point and search direction</returns>
        public PositionAndRotation FindAndReservePosition(GameObject reservingObject, int startingPosition, SearchAlgorithm searchAlgorithm = SearchAlgorithm.closestPosition, SearchDirection searchDirection = SearchDirection.bothWays, bool applyPose = false, bool logErrors = true, bool internalForceOverride = false) {

            if (startingPosition > _occupyingObjects.Count - 1 || startingPosition < 0) {
                if(logErrors)
                    Debug.LogError("The provided starting position is out of bounds! (FindAndReservePosition in TPI_ObjectPlacementController)");
                return null;
            }

            int freeSpot = FindUnoccupiedSpot(startingPosition, searchAlgorithm, searchDirection, logErrors);
            if (freeSpot == -1 || freeSpot == availableSpots) {
                return null;
            } else {
                ReserveSpot(freeSpot, reservingObject, internalForceOverride, applyPose);
                PositionAndRotation pose = new PositionAndRotation();
                pose.position = transform.GetChild(freeSpot).position;
                pose.rotation = transform.GetChild(freeSpot).rotation;
                return pose;
            }
        }

        /// <summary>
        /// Frees up a spot in the grid by providing the TPI_PositionAndRotation of said spot.
        /// <para><paramref name="pose"/> = GameObject of which you want to free up the spot (TPI_PositionAndRotation needs to include the exact position; the rotation does not matter)</para>
        /// </summary>
        public void FreeUpSpot(PositionAndRotation pose) {
            FreeUpSpot(pose.position);
        }

        /// <summary>
        /// Frees up a spot in the grid by providing the TPI_PositionAndRotation of said spot.
        /// <para><paramref name="gameObject"/> = GameObject of which you want to free up the spot</para>
        /// </summary>
        public void FreeUpSpot(GameObject gameObject) {
            int index = GetIndexOfGameObject(gameObject, false);
            if(index != -1)
                FreeUpSpot(index);
            else
                Debug.LogWarning("The provided GameObject does not currently take up a spot in the grid! (FreeUpSpot in TPI_ObjectPlacementController)");
        }

        /// <summary>
        /// Frees up a spot in the grid by providing the Vector3 of said spot.
        /// <para><paramref name="position"/> = position Vector3 of the GameObject of which you want to free up the spot</para>
        /// </summary>
        public void FreeUpSpot(Vector3 position) {
            int pos = ConvertSpotVector3ToInt(position, true);
            if (pos == -1) {
                Debug.LogError("The provided Vector3 position does not fit to any available spot! (FreeUpSpot in TPI_ObjectPlacementController)");
                return;
            }
            FreeUpSpot(pos);
        }

        /// <summary>
        /// Frees up a spot in the grid by providing the index (int) of said spot.
        /// <para><paramref name="position"/> = position index of the GameObject of which you want to free up the spot</para>
        /// </summary>
        public void FreeUpSpot(int position) {
            if (position > _occupyingObjects.Count - 1 || position < 0) {
                Debug.LogError("The provided position is out of bounds! (FreeUpPosition in TPI_ObjectPlacementController)");
                return;
            }
            if (_occupyingObjects[position] == null) {
                Debug.LogWarning("The desired position is already empty and can thus not be freed up again! (FreeUpSpot in TPI_ObjectPlacementController)");
                return;
            }
            _occupyingObjects[position] = null;
            previousPositionIndex = position;
        }

        /// <summary>
        /// Frees up all the spots in the grid.
        /// </summary>
        public void ClearAllSpots() {
            if (GetNumUnoccupiedSpots() == availableSpots)
                return;
            for (int i = 0; i < availableSpots; i++) {
                _occupyingObjects[i] = null;
            }
        }

        /// <summary>
        /// Exchanges the contents of two spots.
        /// <para><paramref name="gameObject1"/> = the first GameObject that should be swapped</para>
        /// <para><paramref name="gameObject1"/> = the other GameObject that should be swapped</para>
        /// </summary>
        public void SwapSpotContents(GameObject gameObject1, GameObject gameObject2) {
            SwapSpotContents(GetIndexOfGameObject(gameObject1), GetIndexOfGameObject(gameObject2));
        }

        /// <summary>
        /// Exchanges the contents of two spots.
        /// <para><paramref name="spotIndex1"/> = the position index of the first GameObject that should be swapped</para>
        /// <para><paramref name="spotIndex2"/> = the position index of the other GameObject that should be swapped</para>
        /// </summary>
        public void SwapSpotContents(int spotIndex1, int spotIndex2) {

            GameObject backupGameObject = _occupyingObjects[spotIndex1];

            _occupyingObjects[spotIndex1].transform.position = _occupyingObjects[spotIndex2].transform.position;
            _occupyingObjects[spotIndex1].transform.rotation = _occupyingObjects[spotIndex2].transform.rotation;

            _occupyingObjects[spotIndex2].transform.position = backupGameObject.transform.position;
            _occupyingObjects[spotIndex2].transform.rotation = backupGameObject.transform.rotation;


            _occupyingObjects[spotIndex1] = _occupyingObjects[spotIndex2];
            _occupyingObjects[spotIndex2] = backupGameObject;
        }

        /// <summary>
        /// This enum is used to distinguish between the different anchor positions.
        /// </summary>
        public enum StartingPosition {
            [InspectorName("Upper Left")]
            UpperLeft, // 0
            [InspectorName("Upper Center")]
            UpperCenter, // 1
            [InspectorName("Upper Right")]
            UpperRight, // 2
            [InspectorName("Middle Left")]
            MiddleLeft, // 3
            [InspectorName("Middle Center")]
            MiddleCenter, // 4
            [InspectorName("Middle Right")]
            MiddleRight, // 5
            [InspectorName("Bottom Left")]
            BottomLeft, // 6
            [InspectorName("Bottom Center")]
            BottomCenter, // 7
            [InspectorName("Bottom Right")]
            BottomRight, // 8
        }

        /// <summary>
        /// This enum is used to distinguish between the different search algorithms
        /// </summary>
        public enum SearchAlgorithm {
            [InspectorName("Closest Position available")]
            closestPosition, // 0
            [InspectorName("Horizontally")]
            horizontally, // 1
            [InspectorName("Vertically")]
            vertically, // 2
        }

        /// <summary>
        /// This enum is used to distinguish between the different search directions
        /// </summary>
        public enum SearchDirection {
            /// <summary>
            /// Look in both directions for a free spot.
            /// </summary>
            [InspectorName("Both ways")]
            bothWays, // 0
            /// <summary>
            /// Only look upwards or leftwards for a free spot. In the "diagonally right to left" search algorithm, the ObjectPlacementController will look upwards to the right (upwards takes precedent over leftwards).
            /// </summary>
            [InspectorName("Only Upwards or Leftwards")]
            upOrLeft, // 1
            /// <summary>
            /// Only look downwards or rightwards for a free spot. In the "diagonally right to left" search algorithm, the ObjectPlacementController will look downwards to the left (downwards takes precedent over rightwards).
            /// </summary>
            [InspectorName("Only Downwards or Rightwards")]
            downOrRight, // 2
        }

        /// <summary>
        /// <para>
        /// This is a helper class, which allows to simultaneously save the position and rotation of a spot.
        /// </para>
        /// 
        /// <para>
        /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Controller" without the quotes.
        /// </para>
        /// 
        /// <para>
        /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this class.
        /// </para>
        /// 
        /// <para>
        /// @author
        /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
        /// </para>
        /// </summary>
        [System.Serializable]
        public class PositionAndRotation {

            [Tooltip("Position of a spot")]
            public Vector3 position;
            [Tooltip("Rotation of a spot")]
            public Quaternion rotation;

            public PositionAndRotation() {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }

            public PositionAndRotation(Vector3 position, Quaternion rotation) { 
                this.position = position;
                this.rotation = rotation;
            }
        }

    }

}