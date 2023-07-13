using RosMessageTypes.Diagnostic;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskPlanningInterface.DialogMenu;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.UI;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// This class handles the ROS Integration of the unity side in order to facilitate successfull interactions, including a working messaging system.
    /// <br></br>Furthermore, as the Unity Robotics Hub documentation is virtually nonexistent, it provides a summary and some examples for the most important public functions of the ROSConnection script.
    /// <br></br>Finally, it handles the ROS Status Menu in order to display information, warnings and erros concerning ROS and the robot itself.
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
    /// Source for Unity Robotics Hub (Tutorials): <see href="https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/README.md">Click me</see>
    /// <br></br>Source for ROS in general (Documentation): <see href="http://wiki.ros.org/">Click me</see>
    /// <br></br>Source for the ROS description text and the description of the message types (texts were adapted and improved): <see href="https://chat.openai.com/">Click me</see>
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    /// 
    [RequireComponent(typeof(ROSConnection))]
    public class TPI_ROSController : MonoBehaviour {

        /*
         
        ROS (Robot Operating System) is a middleware framework that enables communication between different components in a robotic system.
        Here is a brief high-level overview of how ROS works:

        Nodes: 
            The basic building blocks of ROS are nodes. A node is an executable that performs a specific task in the robotic system,
            such as reading sensor data or controlling actuators. Nodes communicate with each other by publishing and subscribing to messages.

        Topics:
            Nodes communicate by sending and receiving messages through topics, being the named channel through which the messages are exchanged.
            Nodes can publish messages on a topic, and other nodes interested in that data can subscribe to the topic to receive the messages.

        Messages:
            Messages define the structure and content of the data being exchanged between nodes. ROS provides a variety of predefined message types,
            such as geometry data, sensor measurements, and control commands. Custom message types can also be created to represent specific data types.

        Services:
            In addition to message-based communication, ROS supports services. A service allows nodes to make requests and receive responses synchronously.
            Nodes can provide services, and other nodes can make requests to these services, invoking specific functions and returning results.

        Launch files:
            ROS provides launch files that simplify the process of starting multiple nodes and configuring the system.
            Launch files allow you to start nodes, set parameters, and establish connections between nodes, making it easier to launch complex robotic systems.

        Tools and Libraries:
            ROS provides a rich set of tools and libraries to aid in development. These include visualization tools, debugging tools,
            simulation environments, and libraries for common functionalities like motion planning, perception, and control.

        Ecosystem:
            ROS has a large and active community, contributing to a vast ecosystem of packages and resources.
            You can leverage existing ROS packages, libraries, and tutorials to accelerate development and benefit from community support.

        Overall, ROS facilitates the development of complex robotic systems by providing a communication infrastructure, tools, and libraries
        that enable modular and distributed software architecture. It promotes code reuse, collaboration, and rapid prototyping,
        making it a popular choice for robotic software development.

        */

        //---------------------------------------------------- List of all the Message Types available to you ----------------------------------------------------//

        /*

        The whole list with descriptions can also be found at: http://wiki.ros.org/common_msgs
        Furthermore, a description is available in most scripts by using CTRL + left click on the name of a message (therefore by visiting the script)

        Usually, compared to the website of ROS, you have to add 'Msg' after each member in order for it to work in Unity

        Additionally, below, you can find a list of the most important categories and their description:
        

        actionlib_msgs:
            Defines the common messages used in the ActionLib package for ROS, which provides a standardized interface for executing long-running tasks in a distributed system.

        diagnostic_msgs:
            Contains messages used in the diagnostic_msgs package, which provides a way to report diagnostic information about the state of various systems in a robot or an application.

        geometry_msgs:
            Defines messages related to geometric concepts such as points, vectors, poses, and transformations. It is widely used in ROS for representing spatial information.

        nav_msgs:
            Provides messages related to navigation, such as maps, paths, and odometry. These messages are commonly used in robotic navigation systems.

        sensor_msgs:
            Contains messages for different types of sensor data, including images, laser scans, point clouds, and IMU measurements. It is a fundamental package for handling sensor information in ROS.

        shape_msgs:
            Defines messages related to geometric shapes, including basic shapes like boxes, cylinders, and spheres, as well as more complex shapes like meshes.

        std_msgs:
            Provides standard message types used across ROS packages. It includes basic data types like integers, floats, strings, and timestamps.

        stereo_msgs:
            Contains messages specifically designed for stereo vision, including disparity maps and point clouds representing 3D data from stereo cameras.

        trajectory_msgs:
            Defines messages related to robot trajectories, including joint trajectories and multi-DOF trajectories. These messages are commonly used in robot motion planning and control.

        visualization_msgs:
            Provides messages for visualizing various types of data in ROS, such as markers, images, and interactive markers. It is commonly used for visualization purposes in robotic applications.

         */

        [Tooltip("Decide whether Unity should try to create a ROS Connection on Startup.")]
        [SerializeField] private ROSConnectionType rosConnectionType = ROSConnectionType.autoStart;

        private ROSConnection _connection; // reference to the ROSConnection component

        [Tooltip("Add the Prefab for the dialog menu that request the operator to enter the ip and port of the ROS workstation.")]
        [SerializeField] private GameObject addressRequestPrefab;

        private List<DiagnosticStatusMsg> _statusList; // list that saves the DiagnosticStatusMsgs
        private List<GameObject> _statusObjectsList; // list that saves the instantiated status message objects

        [Tooltip("Add the Prefab for the ROS Status Menu Entries (whatever will be instantiated and thus shown in the ROS Status Menu).")]
        [SerializeField] private GameObject statusEntry;
        [Tooltip("Add the Texture for the Status that provides the ability to show information messages from ROS.")]
        [SerializeField] private Texture2D infoTexture;
        [Tooltip("Add the Texture for the Status that indicates that a warning was thrown.")]
        [SerializeField] private Texture2D warningTexture;
        [Tooltip("Add the Texture for the Status that indicates that an error has occured.")]
        [SerializeField] private Texture2D errorTexture;

        private DiagnosticStatusMsg connectionErrorMsg;
        private DiagnosticStatusMsg connectionEstablishedMsg;
        private DiagnosticStatusMsg connectionSuccessfulMsg;
        private TPI_MainController mainController;

        // Register Publishers and Subscribers using this action -> they will be registered once the connection has been made and then removed from the Action
        // This action keeps getting called. Therefore, you can add actions to it throughout the whole runtime.
        public static Action RegisterDataStream;

        private bool connectionCalled = false; // internal use only

        // This action is invoked if the connection to ROS was successfully established WITHOUT errors.
        public static Action OnConnected;

        // This action is invoked if the connection to ROS was either stopped or if errors occurred.
        public static Action OnDisconnected;


        //---------------------------------------------------- General Functions ----------------------------------------------------//



        private void Start() {

            mainController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>();

            if (addressRequestPrefab == null)
                Debug.LogError("Please assign the Address Request Prefab GameObject in the TPI_ROSController component in " + transform.name);
            if (statusEntry == null)
                Debug.LogError("Please assign the Status Menu Entry Prefab GameObject in the TPI_ROSController component in " + transform.name);
            if (infoTexture == null)
                Debug.LogWarning("Please assign an icon for the Information status in the TPI_ROSController component in " + transform.name);
            if (warningTexture == null)
                Debug.LogWarning("Please assign an icon for the Warning status in the TPI_ROSController component in " + transform.name);
            if (errorTexture == null)
                Debug.LogWarning("Please assign an icon for the Error status in the TPI_ROSController component in " + transform.name);

            // Get correct instance
            _connection = ROSConnection.GetOrCreateInstance();

            if (rosConnectionType == ROSConnectionType.autoStart) {

                // Start the Connection to ROS
                _connection.Connect();

            } else if(rosConnectionType == ROSConnectionType.deactivated) {

                // Change Message to display that ROS was deactivated if the ROS Status Menu was somehow activated (should stay disabled in that case -> just a precaution)
                mainController.rosStatusMenu.transform.GetChild(4).GetComponent<TextMeshPro>().text = "The ROS Connection was disabled.";

            } else { // ROSConnectionType.requestIP

                // Change Message to display that the ROS Connection has not been started yet (should stay disabled until it the connection was made -> just a precaution)
                mainController.rosStatusMenu.transform.GetChild(4).GetComponent<TextMeshPro>().text = "The ROS Connection has not been started yet.";

            }

            SubscribeToDiagnosticStatus(AddStatusEntry);

            _statusList = new List<DiagnosticStatusMsg>();
            _statusObjectsList = new List<GameObject>();

            // Display the dialog menu that requests the operator to enter the IPv4 address and the port of the ROS workstation
            // (only used in the case of rosConnectionType == ROSConnectionType.requestIP).
            if (rosConnectionType == ROSConnectionType.requestIP) {
                StartCoroutine(CreateAddressRequestMenu());
            }

        }

        private IEnumerator CreateAddressRequestMenu() {
            yield return new WaitForSeconds(1);
            TPI_DialogMenuInformation dialogMenuInfo = new TPI_DialogMenuInformation();
            dialogMenuInfo.dialogMenuName = "ROS Workstation Address";
            dialogMenuInfo.dialogMenuPrefab = addressRequestPrefab;
            dialogMenuInfo.showCloseMenuButton = false;

            dialogMenuInfo.dialogMenuTexts.Add("Please enter the IPv4 address and the port of the ROS workstation.");

            TPI_DialogMenuButton button = new TPI_DialogMenuButton();
            button.buttonText = "Establish ROS Connection";
            button.buttonIcon = mainController.startButtonIcon;
            button.buttonOnClick.AddListener(delegate { GetROSAddress(dialogMenuInfo.dialogMenuID); });
            dialogMenuInfo.dialogMenuButtons.Add(button);

            dialogMenuInfo.keyboardInputFieldTitles.Add("Please enter the IPv4 address:");
            dialogMenuInfo.keyboardInputFieldTitles.Add("Please enter the port:");

            mainController.dialogMenuContainer.SetActive(true);
            mainController.GetComponent<TPI_DialogMenuController>().SpawnDialogMenu(dialogMenuInfo);
        }

        private void Update() {

            if (Time.frameCount % 10 == 0) {
                if (mainController.rosStatusMenu.activeSelf) {

                    // Check for Connection Thread and add Connection Thread Message to ROS Status Menu
                    if (hasConnectionThread()) {
                        if (connectionEstablishedMsg == null) {
                            connectionEstablishedMsg = new DiagnosticStatusMsg(0, "Connection Information", "The connection to the ROS workstation is " + _connection.RosIPAddress + ":" + _connection.RosPort.ToString() + " currently being established.", "", null);
                            AddStatusEntry(connectionEstablishedMsg);
                        }
                    } else {
                        if (connectionEstablishedMsg != null)
                            RemoveStatusEntry(connectionEstablishedMsg);
                    }

                    // Check for Connection Error and add Connection Error Message to ROS Status Menu if needed
                    if (hasConnectionError()) {
                        if (connectionErrorMsg == null) {
                            connectionErrorMsg = new DiagnosticStatusMsg(2, "Connection Error", "The connection to the ROS workstation at " + _connection.RosIPAddress + ":" + _connection.RosPort.ToString() + " has failed.", "", null);
                            AddStatusEntry(connectionErrorMsg);
                        }
                    } else {
                        if (connectionErrorMsg != null)
                            RemoveStatusEntry(connectionErrorMsg);
                    }

                    // Display Successful Connection Msg if it has the thread and if there are no errors
                    if(hasConnectionThread() && !hasConnectionError()) {
                        if(connectionSuccessfulMsg == null) {
                            connectionSuccessfulMsg = new DiagnosticStatusMsg(0, "Connection Successful", "The connection to the ROS workstation at " + _connection.RosIPAddress + ":" + _connection.RosPort.ToString() + " was successful.", "", null);
                            AddStatusEntry(connectionSuccessfulMsg);
                        }
                    } else {
                        if (connectionSuccessfulMsg != null)
                            RemoveStatusEntry(connectionSuccessfulMsg);
                    }
                }

                if (hasConnectionThread() && !hasConnectionError()) {
                    // Register Publishers and Subscribers once a stable connection was successfully made without any errors.
                    if (RegisterDataStream != null) {
                        RegisterDataStream();
                        RegisterDataStream = null;
                    }

                    // Invoke actions once a stable connection was successfully made without any errors.
                    if (!connectionCalled) {
                        if (OnConnected != null) {
                            OnConnected();
                        }
                        connectionCalled = true;
                    }

                } else {
                    // Invoke actions once the connection was either stopped or once there were errors
                    if (connectionCalled) {
                        if (OnDisconnected != null) {
                            OnDisconnected();
                        }
                        connectionCalled = false;
                    }
                }

            }
        }

        private void OnApplicationQuit() {

            // Disconnect when the application is closed
            if(hasConnectionThread() && !hasConnectionError())
                _connection.Disconnect();

        }

        /// <summary>
        /// Returns whether Unity tries to create a ROS Connection on Startup.
        /// </summary>
        public bool IsROSConnectionDeactivated() {
            if (rosConnectionType == ROSConnectionType.deactivated)
                return true;
            return false;
        }

        /// <summary>
        /// Returns the instance of the ROSConnection that is currently active.
        /// </summary>
        public ROSConnection GetROSConnection() {
            return _connection;
        }

        /// <summary>
        /// Return whether ROS tried to initiate a connection. 
        /// <br></br>IMPORTANT: If you want to check if the ROS Connection was successful or not, please use <c>hasConnectionError();</c>.
        /// </summary>
        /// <returns>true -> the TPI tried to connect to ROS using the Unity Robotics Hub
        /// <br></br>false -> the TPI has not tried to connect to ROS yet</returns>
        public bool hasConnectionThread() {
            return _connection.HasConnectionThread;
        }

        /// <summary>
        /// Return whether the connection to ROS has errors and was thus not successful.
        /// </summary>
        /// <returns>true -> connection has errors / was unsuccessful
        /// <br></br>false -> otherwise</returns>
        public bool hasConnectionError() {
            return _connection.HasConnectionError;
        }

        /// <summary>
        /// Helper Function that gets called once the button in Address Request Dialog Menu is pressed (only used in the case of rosConnectionType == ROSConnectionType.requestIP).
        /// <para><paramref name="dialogMenuID"/> = ID of the active Dialog Menu</para>
        /// </summary>
        private void GetROSAddress(string dialogMenuID) {
            TPI_DialogMenuChoices choices = mainController.GetComponent<TPI_DialogMenuController>().GetDialogMenuChoices(dialogMenuID);

            if (System.Net.IPAddress.TryParse(choices.keyboardTexts[0], out _)) {
                if (int.TryParse(choices.keyboardTexts[1], out _)) {
                    mainController.GetComponent<TPI_DialogMenuController>().UnSpawnDialogMenu(dialogMenuID);
                    EnterROSAddress(System.Net.IPAddress.Parse(choices.keyboardTexts[0]).ToString(), int.Parse(choices.keyboardTexts[1]));
                } else {
                    mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "The valid ROS Port can only consist of numbers, e.g. '10000'.", "Confirm", null, null);
                }
            } else {
                mainController.GetComponent<TPI_DialogMenuController>().ShowErrorMenu("Error", "The ROS IP address is not valid. \nA sample valid IPv4 address looks like this: 172.20.10.2 (only numbers and points).", "Confirm", null, null);
            }
        }

        /// <summary>
        /// Used to enter the ROS IP and port, and start the ROS connection (only used in the case of rosConnectionType == ROSConnectionType.requestIP).
        /// </summary>
        public void EnterROSAddress(string IP, int Port) {
            _connection.RosIPAddress = IP;
            _connection.RosPort = Port;
            _connection.Connect();
        }


        //---------------------------------------------------- Topic Related Functions ----------------------------------------------------//



        #region topicRelatedFunctions

        /// <summary>
        /// Allows you to add a function as a listener, which will get called if a new topic is recognized.
        /// <para><paramref name="callback"/> = function to be called</para>
        /// <para><paramref name="notifyAllExistingTopics"/> = decide whether the function should at this moment be called on all active topics</para>
        /// </summary>
        public void ListenForTopics(Action<RosTopicState> callback, bool notifyAllExistingTopics = false) {
            _connection.ListenForTopics(callback, notifyAllExistingTopics);
        }

        /// <summary>
        /// Searches for a topic by name and returns it (RosTopicState).
        /// <br></br>Side note: returns null if the topic does not exist
        /// <para><paramref name="rosTopicName"/> = name of the topic that should be accessed</para>
        /// </summary>
        public RosTopicState GetTopic(string rosTopicName) {
            return _connection.GetTopic(rosTopicName);
        }

        /// <summary>
        /// Returns all the topics that are currently in the topics list (IEnumerable).
        /// </summary>
        public IEnumerable<RosTopicState> AllTopics => _connection.AllTopics;

        /// <summary>
        /// Searches for a topic by name and returns it (RosTopicState).
        /// <br></br>If the topic exists, it updates the rosMessageName that is connected to the topic if needed.
        /// <br></br>If the topic does not exist, it creates it and adds it to the list of topics.
        /// <para><paramref name="rosTopicName"/> = name of the topic that should be accessed</para>
        /// <para><paramref name="rosMessageName"/> = name of the rosMessageType that should be accessed</para>
        /// </summary>
        public RosTopicState GetOrCreateTopic(string rosTopicName, string rosMessageName) {
            return _connection.GetOrCreateTopic(rosTopicName, rosMessageName);
        }

        /// <summary>
        /// Allows you to perform actions on the entire topic list by providing the function.
        /// <br></br> The topics will be accessible as a parameter of type string[]
        /// <para><paramref name="rosCallback"/> = function that should be called, must include string[] as the only parameter</para>
        /// </summary>
        public void GetTopicList(Action<string[]> rosCallback) {
            _connection.GetTopicList(rosCallback);
        }

        /// <summary>
        /// Allows you to perform actions on the entire topic and type lists by providing the function.
        /// <br></br>The topics and types will be accessible as a parameter of type Dictionary&lt;string, string&gt;
        /// <para><paramref name="rosCallback"/> = function that should be called, must include Dictionary&lt;string, string&gt; as the only parameter</para>
        /// </summary>
        public void GetTopicAndTypeList(Action<Dictionary<string, string>> rosCallback) {
            _connection.GetTopicAndTypeList(rosCallback);
        }

        /// <summary>
        /// Makes sure that the topics list in general and each member of it is setup correctly.
        /// <br></br>If needed, it updates the RosMessageName of the members if there were any changes, that have not been applied.
        /// </summary>
        public void RefreshTopicsList() {
            _connection.RefreshTopicsList();
        }

        #endregion topicRelatedFunctions



        //---------------------------------------------------- Publishing Related Functions ----------------------------------------------------//



        #region publishingRelatedFunctions

        /// <summary>
        /// Publishing is used to send Messages (containing information) from Unity to ROS.
        /// <br></br>However, before you can actually send a message, you have to set up the Publisher (a topic for a specific message), which can be done with the help of this function.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want to send your first message.
        /// <br></br>How it works: First, it creates a new topic, which it then registers as a publisher.
        /// <br></br>The next step for you would be to call <c>Publish(string rosTopicName, Message rosMessage)</c> in order to actually send a message.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the publisher</para>
        /// </summary>
        /// <returns>Topic that was created (TopicState)</returns>
        public RosTopicState RegisterPublisher<MessageType>(string rosTopicName) where MessageType : Message {
            return _connection.RegisterPublisher<MessageType>(rosTopicName);
        }

        /// <summary>
        /// Publishing is used to send Messages (containing information) from Unity to ROS.
        /// <br></br>However, before you can actually send a message, you have to set up the Publisher (a topic for a specific message), which can be done with the help of this function.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want to send your first message.
        /// <br></br>How it works: First, it creates a new topic (if it does not already exist), which it then registers as a publisher.
        /// <br></br>The next step for you would be to call <c>Publish(string rosTopicName, Message rosMessage)</c> in order to actually send a message.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the publisher</para>
        /// <para><paramref name="rosMessageTypeName"/> = name of the message type that should be registered under you topic as a publisher</para>
        /// </summary>
        /// <returns>Topic that was created (TopicState)</returns>
        public RosTopicState RegisterPublisher(string rosTopicName, string rosMessageTypeName) {
            return _connection.RegisterPublisher(rosTopicName, rosMessageTypeName);
        }

        /// <summary>
        /// Publishing is used to send Messages from Unity to ROS.
        /// <br></br>How it works: First, it converts your rosTopicName into the topic instance. Then, it publishes your message in the topic.
        /// <br></br>Please make sure that you have run <c>RegisterPublisher(string rosTopicName, string rosMessageName)</c> or <c>RegisterPublisher&lt;MessageType&gt;(rosTopicName);</c> before you call this function.
        /// <br></br>Side note: topic names that start with '__' will be sent as System Commands (QueueSysCommand)!
        /// <para><paramref name="rosTopicName"/> = name of the topic under which you want to send the message</para>
        /// <para><paramref name="rosMessage"/> = message that should be published</para>
        /// </summary>
        public void Publish(string rosTopicName, Message rosMessage) {
            _connection.Publish(rosTopicName, rosMessage);
        }

        /// <summary>
        /// System commands consist of two parts, the actual command, which always starts with '__' to distinguish them from ros topics, then followed by a message.
        /// <br></br>The command is sent as a 4 byte command length, followed by that many bytes of the command.
        /// <br></br>The parameters is sent as a 4-byte json length, followed by a json string of that length.
        /// <para><paramref name="command"/> = command that should be executed</para>
        /// <para><paramref name="parameters"/> = parameters of your command that should be executed</para>
        /// </summary>
        public void QueueSysCommand(string command, object parameters) {
            _connection.QueueSysCommand(command, parameters);
        }

        /*
        
        Example from the PositionChange function integrated in the 'Ready-to-use Functions' section below:
        
        Features:
            - Registers the Publisher
            - Translates a position and rotation from the coordinate system of Unity into the coordinate system of ROS
            - Publishes the PoseMsg to ROS

        void Start() {
            // Add the reference to this rosController script first!
        
            // Registers the publisher
            rosController.RegisterPublisher<PoseMsg>("tpi_robot_destination");
        }

        public void PublishRobotDestination(Vector3 desiredPosition, Quaternion desiredRotation) {
            if (desiredPosition == null && desiredRotation == null) {
                Debug.LogError("The desiredPosition and desiredRotation cannot both be null! (PublishRobotDestination in TPI_ROSController)");
                return;
            }
            PoseMsg msg = new PoseMsg();
            if (desiredPosition != null) {
                Vector3<FLU> rosPositon = desiredPosition.To<FLU>(); // convert Unity coordinates into ROS coordinate system
                PointMsg pointMsg = new PointMsg(rosPositon.x, rosPositon.y, rosPositon.z);
                msg.position = pointMsg;
            }
            if (desiredRotation != null) {
                Quaternion<FLU> rosRotation = desiredRotation.To<FLU>();
                QuaternionMsg quaternionMsg = new QuaternionMsg(rosRotation.x, rosRotation.y, rosRotation.z, rosRotation.w);
                msg.orientation = quaternionMsg;
            }
            _connection.Publish("tpi_robot_destination", msg);
        }


        */

        #endregion publishingRelatedFunctions



        //---------------------------------------------------- Subscriber Related Functions ----------------------------------------------------//



        #region subscriberRelatedFunctions

        /// <summary>
        /// Return whether this specific Topic has any Subscribers.
        /// <br></br>Side note: returns false if the topic does not exist
        /// <para><paramref name="rosTopicName"/> = name of the topic the RosController has to check</para>
        /// </summary>
        public bool HasSubscriber(string rosTopicName) {
            return _connection.HasSubscriber(rosTopicName);
        }

        /// <summary>
        /// Subscribing is used to receive Messages from ROS in Unity.
        /// <br></br>However, before you actually receive the messages, you have to setup a Listener, which only listenes to a specific topic and a specific messageType.
        /// <br></br>Please do not send different message types into the same topic, as this will throw an error.
        /// <br></br>Once Unity receives a message in the correct form, it will automatically call your rosCallback function, which only takes the MessageType T as a parameter.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the listener</para>
        /// <para><paramref name="rosCallback"/> = function that should be called, must include the type of your message as the only parameter</para>
        /// </summary>
        public void Subscribe<MessageType>(string rosTopicName, Action<MessageType> rosCallback) where MessageType : Message {
            _connection.Subscribe<MessageType>(rosTopicName, rosCallback);
        }

        /// <summary>
        /// Subscribing is used to receive Messages from ROS in Unity.
        /// <br></br>However, before you actually receive the messages, you have to setup a Listener, which only listenes to a specific topic and a specific messageType.
        /// <br></br>Please do not send different message types into the same topic, as this will throw an error.
        /// <br></br>Once Unity receives a message in the correct form, it will automatically call your rosCallback function, which only takes the MessageType T as a parameter.
        /// <para><paramref name="rosMessageName"/> = name of your message type that you want to subscribe to as a listener under your topic</para>
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the listener</para>
        /// <para><paramref name="rosCallback"/> = function that should be called, must include the type of your message as the only parameter</para>
        /// </summary>
        public void SubscribeByMessageName(string rosMessageName, string rosTopicName, Action<Message> rosCallback) {
            _connection.SubscribeByMessageName(rosTopicName, rosMessageName, rosCallback);
        }

        /// <summary>
        /// Once you no longer wish to receive the messages of a topic, you can unsubscribe from it.
        /// <br></br>This will result in your rosCallback function no longer being called.
        /// <para><paramref name="rosTopicName"/> = name of the topic from which you want to unsubscribe</para>
        /// </summary>
        public void Unsubscribe(string rosTopicName) {
            _connection.Unsubscribe(rosTopicName);
        }

        /*
        
        Example from the PositionChange function integrated in the 'Ready-to-use Functions' section below:
        
        Features:
            - Subscribes to a topic
            - Translates a position and rotation from the coordinate system of ROS into the coordinate system of Unity

        void Start() {
            // Add the reference to this rosController script first!
        
            // subscribes to the topic
            rosController.Subscribe<PoseMsg>("tpi_pose_change", PositionChange);
        }

        public void PositionChange(PoseMsg poseMsg) {

            Vector3 unityPosition = (Vector3)new Vector3<FLU>((float)poseMsg.position.x, (float)poseMsg.position.y, (float)poseMsg.position.z).To<RUF>();
            Quaternion rotation = (Quaternion)new Quaternion<FLU>((float)poseMsg.orientation.x, (float)poseMsg.orientation.y, (float)poseMsg.orientation.z, (float)poseMsg.orientation.w).To<RUF>();
            
            // Here you can do whatever you want with the position and rotation that was obtained from ROS.
            
        }

        */

        #endregion subscriberRelatedFunctions



        //---------------------------------------------------- Service Related Functions ----------------------------------------------------//



        #region serviceRelatedFunctions

        /// <summary>
        /// ROS Services can be used to setup a request and reply system, which in turn is defined by a pair of messages.
        /// <br></br>The first message is used for the request, the second is being used for the reply.
        /// <br></br>The <c>ImplementService</c> function is used to RESPOND to request being made by ROS.
        /// <br></br>This function will setup a listener for a specific topic name and for the two specific message types.
        /// <br></br>IMPORTANT: Your rosCallback function must return the type of your response message, generating the response in that rosCallback function.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the listener</para>
        /// <para><paramref name="rosCallback"/> = function that should be called, must include the types of your request and / or response message as the only parameters</para>
        /// </summary>
        public void ImplementService<MessageRypeRequest, MessageTypeResponse>(string rosTopicName, Func<MessageRypeRequest, MessageTypeResponse> rosCallback)
            where MessageRypeRequest : Message
            where MessageTypeResponse : Message {
            _connection.ImplementService<MessageRypeRequest, MessageTypeResponse>(rosTopicName, rosCallback);
        }

        /// <summary>
        /// ROS Services can be used to setup a request and reply system, which in turn is defined by a pair of messages.
        /// <br></br>The first message is used for the request, the second is being used for the reply.
        /// <br></br>The <c>ImplementService</c> function is used to RESPOND to request being made by ROS.
        /// <br></br>Therefore, it will setup a listener for a specific topic name and for the two specific message types.
        /// <br></br>IMPORTANT: Your rosCallback function must return the type of your response message, generating the response in that rosCallback function.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the listener</para>
        /// <para><paramref name="rosCallback"/> = function that should be called, must include the types of your request and / or response message as the only parameters</para>
        /// </summary>
        public void ImplementService<MessageTypeRequest, MessageTypeResponse>(string rosTopicName, Func<MessageTypeRequest, Task<MessageTypeResponse>> rosCallback)
            where MessageTypeRequest : Message
            where MessageTypeResponse : Message {
            _connection.ImplementService<MessageTypeRequest, MessageTypeResponse>(rosTopicName, rosCallback);
        }

        /*
        
        Example from the the Unity Robotics Hub (https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/unity_service.md):
        
        Features:
            - Implements a ROS Service that responds to a request made by ROS (requests the position and rotation of a gameObject with the name object_name)

        void Start() {
            // Add the reference to this rosController script first!
        
            // Registers the service with ROS
            rosController.ImplementService<ObjectPoseServiceRequest, ObjectPoseServiceResponse>("obj_pose_srv", GetObjectPose);
        }

        /// <summary>
        ///  Callback to respond to the request
        /// </summary>
        /// <param name="request">service request containing the object name</param>
        /// <returns>service response containing the object pose (or 0 if object not found)</returns>
        private ObjectPoseServiceResponse GetObjectPose(ObjectPoseServiceRequest request) {
            // process the service request
            Debug.Log("Received request for object: " + request.object_name);

            // prepare a response
            ObjectPoseServiceResponse objectPoseResponse = new ObjectPoseServiceResponse();
            // Find a game object with the requested name
            GameObject gameObject = GameObject.Find(request.object_name);
            if (gameObject)
            {
                // Fill-in the response with the object pose converted from Unity coordinate to ROS coordinate system
                objectPoseResponse.object_pose.position = gameObject.transform.position.To<FLU>();
                objectPoseResponse.object_pose.orientation = gameObject.transform.rotation.To<FLU>();
            }

            return objectPoseResponse;
        }

        */

        /// <summary>
        /// ROS Services can be used to setup a request and reply system, which in turn is defined by a pair of messages.
        /// <br></br>The first message is used for the request, the second is being used for the reply.
        /// <br></br>The <c>RegisterRosService</c> function is used to make REQUESTS from ROS by Unity.
        /// <br></br>Therefore, it will setup the correct listener for the incoming response from ROS for a specific topic name and for the two specific message types.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want make your first request.
        /// <br></br>The next step for you would be to call <c>SendServiceMessage&lt;MessageTypeResponse&gt;(string rosServiceName, Message serviceRequest, Action&lt;MessageTypeResponse&gt; callback)</c>
        /// or <c>SendServiceMessage&lt;MessageTypeResponse&gt;(string rosServiceName, Message serviceRequest)</c> in order to actually send a message.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the listener</para>
        /// </summary>
        public void RegisterRosService<TRequest, TResponse>(string rosTopicName) where TRequest : Message where TResponse : Message {
            _connection.RegisterRosService(rosTopicName, MessageRegistry.GetRosMessageName<TRequest>(), MessageRegistry.GetRosMessageName<TResponse>());
        }

        /// <summary>
        /// ROS Services can be used to setup a request and reply system, which in turn is defined by a pair of messages.
        /// <br></br>The first message is used for the request, the second is being used for the reply.
        /// <br></br>The <c>RegisterRosService</c> function is used to make REQUESTS from ROS by Unity.
        /// <br></br>Therefore, it will setup the correct listener for the incoming response from ROS for a specific topic name and for the two specific message types.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want make your first request.
        /// <br></br>The next step for you would be to call <c>SendServiceMessage&lt;MessageTypeResponse&gt;(string rosServiceName, Message serviceRequest, Action&lt;MessageTypeResponse&gt; callback)</c>
        /// or <c>SendServiceMessage&lt;MessageTypeResponse&gt;(string rosServiceName, Message serviceRequest)</c> in order to actually send a message.
        /// <para><paramref name="rosTopicName"/> = name of the topic to which you want to add the listener</para>
        /// <para><paramref name="requestMessageName"/> = name of your rquest message type that you want to publish</para>
        /// <para><paramref name="responseMessageName"/> = name of your response message type that you want to subscribe to as listener under your topic</para>
        /// </summary>
        public void RegisterRosService(string rosTopicName, string requestMessageName, string responseMessageName) {
            _connection.RegisterRosService(rosTopicName, requestMessageName, responseMessageName);
        }

        /// <summary>
        /// Once the Listener for the Service Response from ROS has been setup using RegisterRosService, you can call the SendServiceMessage function to actually send requests to ROS.
        /// <br></br>How it works: First, it creates a new topic (if it does not already exist) and serializes the message. Then, it send the requests and waits until it receives a response (happens in a different thread so that unity does not stop).
        /// <br></br>Finally, once it received the response, it calls your rosCallback function with the response as a parameter (parameter is of your respond message type).
        /// <para><paramref name="rosServiceName"/> = name of the topic of the service under which you want to send your request</para>
        /// <para><paramref name="serviceRequest"/> = Message that you want to send as your request</para>
        /// <para><paramref name="rosCallback"/> = function that should be called, must include the type of your response message as the only parameter</para>
        /// </summary>
        public async void SendServiceMessage<MessageTypeResponse>(string rosServiceName, Message serviceRequest, Action<MessageTypeResponse> rosCallback) where MessageTypeResponse : Message, new() {
            await Task.Run(delegate { _connection.SendServiceMessage<MessageTypeResponse>(rosServiceName, serviceRequest, rosCallback); });
        }

        /// <summary>
        /// Once the Listener for the Service Response from ROS has been setup using RegisterRosService, you can call the SendServiceMessage function to actually send requests to ROS.
        /// <br></br>How it works: First, it creates a new topic (if it does not already exist) and serializes the message. Then, it send the requests and waits until it receives a response (happens in a different thread so that unity does not stop).
        /// <br></br>Finally, once it received the response, it returns it.
        /// <para><paramref name="rosServiceName"/> = name of the topic of the service under which you want to send your request</para>
        /// <para><paramref name="serviceRequest"/> = Message that you want to send as your request</para>
        /// </summary>
        /// <returns>the response from ROS (Task&lt;MessageTypeResponse&gt;) to the initial request, i.e. what would be used as the parameter in the rosCallback function of the other SendServiceMessage function</returns>
        public async Task<MessageTypeResponse> SendServiceMessage<MessageTypeResponse>(string rosServiceName, Message serviceRequest) where MessageTypeResponse : Message, new() {
            return await _connection.SendServiceMessage<MessageTypeResponse>(rosServiceName, serviceRequest);
        }

        /*
        
        Example from the the Unity Robotics Hub (https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/service_call.md):
        
        Features:
            - Implements a ROS Service that sends the current position of a cuve to ROS and request a new position from ROS

        public GameObject cube;

        // Cube movement conditions
        public float delta = 1.0f;
        public float speed = 2.0f;
        private Vector3 destination;

        float awaitingResponseUntilTimestamp = -1;

        void Start() {
            // Add the reference to this rosController script first!
        
            // Registers the service
            rosController.RegisterRosService<PositionServiceRequest, PositionServiceResponse>("pos_srv");
            destination = cube.transform.position;
        }

        private void Update() {
            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            cube.transform.position = Vector3.MoveTowards(cube.transform.position, destination, step);

            if (Vector3.Distance(cube.transform.position, destination) < delta && Time.time > awaitingResponseUntilTimestamp)
            {
                Debug.Log("Destination reached.");

                PosRotMsg cubePos = new PosRotMsg(
                    cube.transform.position.x,
                    cube.transform.position.y,
                    cube.transform.position.z,
                    cube.transform.rotation.x,
                    cube.transform.rotation.y,
                    cube.transform.rotation.z,
                    cube.transform.rotation.w
                );

                PositionServiceRequest positionServiceRequest = new PositionServiceRequest(cubePos);

                // Send message to ROS and return the response
                ros.SendServiceMessage<PositionServiceResponse>("pos_srv", positionServiceRequest, Callback_Destination);
                awaitingResponseUntilTimestamp = Time.time + 1.0f; // don't send again for 1 second, or until we receive a response
            }
        }

        void Callback_Destination(PositionServiceResponse response) {
            awaitingResponseUntilTimestamp = -1;
            destination = new Vector3(response.output.pos_x, response.output.pos_y, response.output.pos_z);
            Debug.Log("New Destination: " + destination);
        }

        */

        #endregion serviceRelatedFunctions



        //---------------------------------------------------- UI Related Functions ----------------------------------------------------//



        #region UIrelatedFunctions

        /// <summary>
        /// This function can be used in order to easily draw the connection arrows (indicate the status of the connection).
        /// <para><paramref name="withBar"/> = indicate whether there should be an additional bar</para>
        /// <para><paramref name="x"/> = size of the connection arrows in x direction (width)</para>
        /// <para><paramref name="y"/> = size of the connection arrows in y direction (height)</para>
        /// <para><paramref name="receivedTime"/> = indicate the elapsed time since you have received anything</para>
        /// <para><paramref name="sentTime"/> = indicate the elapsed time since you have sent anything</para>
        /// <para><paramref name="isPublisher"/> = indicate whether you are publishing anything</para>
        /// <para><paramref name="isSubscriber"/> = indicate whether you are subscribed to anything</para>
        /// <para><paramref name="hasError"/> = indicate whether there were any connection errors</para>
        /// </summary>
        public void DrawConnectionArrows(bool withBar, float x, float y, float receivedTime, float sentTime, bool isPublisher, bool isSubscriber, bool hasError) {
            ROSConnection.DrawConnectionArrows(withBar, x, y, receivedTime, sentTime, isPublisher, isSubscriber, hasError);
        }

        /// <summary>
        /// Returns the correct Color that belongs to each connection status: noConnection -> gray, hasErrors -> red, elapsedTime is smaller than 0.03f -> bright, else -> fading color from bright to dark 
        /// <para><paramref name="elapsedTime"/> = insert the elapsed time since the last reply of ROS</para>
        /// <para><paramref name="hasConnection"/> = indicate whether there is an active connection</para>
        /// <para><paramref name="hasError"/> = indicate whether there were any connection errors</para>
        /// </summary>
        /// <returns>Color of Connection Status</returns>
        public Color GetConnectionColor(float elapsedTime, bool hasConnection, bool hasError) {
            return ROSConnection.GetConnectionColor(elapsedTime, hasConnection, hasError);
        }



        #endregion UIrelatedFunctions



        //---------------------------------------------------- Ready-to-use Functions ----------------------------------------------------//



        #region readyToUseFunctions

        /// <summary>
        /// This function registers the publisher for the shouldExecuteInstructions bool message.
        /// <br></br>The shouldExecuteInstructions BoolMsg is used to tell ROS whether the given machine instructions should be actually performed or whether they should only be simulated.
        /// <br></br>Regardless of the chosen shouldExecuteInstructions bool, unity will receive the pose changes of the robot arm.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want to send your first shouldExecuteInstructions BoolMsg.
        /// </summary>
        public void RegisterShouldExecuteInstructionsPublisher() {
            RegisterDataStream += delegate { _connection.RegisterPublisher<BoolMsg>("tpi_should_execute_instructions"); };
        }

        /// <summary>
        /// This function sends the message of the shouldExecuteInstructions bool.
        /// <br></br>The shouldExecuteInstructions BoolMsg is used to tell ROS whether the given machine instructions should be actually performed or whether they should only be simulated.
        /// <br></br>Regardless of the chosen shouldExecuteInstructions, unity will receive the pose changes of the robot arm.
        /// <para><paramref name="shouldExecuteInstructions"/>: shouldExecuteInstructions = true -> actually perform instructions, shouldExecuteInstructions = false -> only simulate instructions</para>
        /// </summary>
        public void PublishShouldExecuteInstructions(bool shouldExecuteInstructions) {
            BoolMsg msg = new BoolMsg(shouldExecuteInstructions);
            _connection.Publish("tpi_should_execute_instructions", msg);
        }

        /// <summary>
        /// This function registers the publisher for the visualizationSpeed float message.
        /// <br></br>The VisualizationSpeed Float32 is used to tell ROS at what speed the visualization of Snippets should happen, i.e. if it should be sped up. (only needed in the case that the shouldExecuteInstructions bool is false)
        /// <br></br>Regardless of the chosen soeed, unity will receive the pose changes of the robot arm.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want to send your first visualizationSpeed Float32Msg.
        /// </summary>
        public void RegisterVisualizationSpeedPublisher() {
            RegisterDataStream += delegate { _connection.RegisterPublisher<Float32Msg>("tpi_visualization_speed"); };
        }

        /// <summary>
        /// This function sends the massage of the visualizationSpeed float.
        /// <br></br>The VisualizationSpeed Float32 is used to tell ROS at what speed the visualization of Snippets should happen, i.e. if it should be sped up. (only needed in the case that the shouldExecuteInstructions bool is false)
        /// <br></br>Regardless of the chosen soeed, unity will receive the pose changes of the robot arm.
        /// <para><paramref name="speed"/> = speed at which the simulation of the robot movements for the visualization should happen</para>
        /// </summary>
        public void PublishVisualizationSpeed(float speed) {
            _connection.Publish("tpi_visualization_speed", new Float32Msg(speed));
        }

        /// <summary>
        /// This function registers the PoseMsg in order to later on tell ROS that the robot arm has to move to a specific position with a specific rotation.
        /// <br></br>Usually, this function is called from the <c>Start()</c> function of your class or before you want to send your first robotDestination PoseMsg.
        /// </summary>
        public void RegisterRobotDestinationPublisher() {
            RegisterDataStream += delegate { _connection.RegisterPublisher<PoseMsg>("tpi_robot_destination"); };
        }

        /// <summary>
        /// This function sends a PoseMsg in order to tell ROS that the robot arm has to move to a specific position with a specific rotation.
        /// <para><paramref name="desiredPosition"/> = the desired position the robot should move to</para>
        /// <para><paramref name="desiredRotation"/> = the desired rotation the robot should have in the end</para>
        /// <para><paramref name="convertToRelativeCoordinates"/>= Determine whether to convert the world coordinates to coordinates that are relative to the position of the robot base. True is the standard option, as it makes the PoseMsg much easier to handle in ROS.</para>
        /// </summary>
        public void PublishRobotDestination(Vector3 desiredPosition, Quaternion desiredRotation, bool convertToRelativeCoordinates = true) {
            if (desiredPosition == null && desiredRotation == null) {
                Debug.LogError("The desiredPosition and desiredRotation cannot both be null! (PublishRobotDestination in TPI_ROSController)");
                return;
            }
            PoseMsg msg;
            if (convertToRelativeCoordinates) {
                msg = mainController.robotURDF.GetComponent<TPI_RobotController>().GetRelativePose(desiredPosition, desiredRotation);
            } else {
                msg = new PoseMsg();
                if (desiredPosition != null) {
                    Vector3<FLU> rosPositon = desiredPosition.To<FLU>(); // convert Unity coordinates into ROS coordinate system
                    PointMsg pointMsg = new PointMsg(rosPositon.x, rosPositon.y, rosPositon.z);
                    msg.position = pointMsg;
                }
                if (desiredRotation != null) {
                    Quaternion<FLU> rosRotation = desiredRotation.To<FLU>();
                    QuaternionMsg quaternionMsg = new QuaternionMsg(rosRotation.x, rosRotation.y, rosRotation.z, rosRotation.w);
                    msg.orientation = quaternionMsg;
                }
            }
            _connection.Publish("tpi_robot_destination", msg);
        }

        /** <summary>
        Call this function in order to subscribe to a pose change of the robot arm. A pose consists of the position (Vector3) and of the rotation (Quaternion).
        <br></br>An acceptable function for rosAction could look like this:
        <code>
        public void PositionChange(PoseMsg poseMsg) {
            Vector3 unityPosition = (Vector3)new Vector3&lt;FLU&gt;((float)poseMsg.position.x, (float)poseMsg.position.y, (float)poseMsg.position.z).To&lt;RUF&gt;();
            Quaternion rotation = (Quaternion)new Quaternion&lt;FLU&gt;((float)poseMsg.orientation.x, (float)poseMsg.orientation.y, (float)poseMsg.orientation.z, (float)poseMsg.orientation.w).To&lt;RUF&gt;();
        }
        </code>
        <para><paramref name="rosAction"/> = Function that should receive the pose. It is important that the function has nothing else than the parameter PoseMsg. Example for rosAction: <c>PositionChange</c> -> therefore only the name of the function</para>
        </summary> */
        public void SubscribeToPoseChange(Action<PoseMsg> rosAction) {
            RegisterDataStream += delegate { _connection.Subscribe<PoseMsg>("tpi_pose_change", rosAction); };
        }
        /*Copy the code from SubscribeToPoseChange from here: (< and > cannot be display in summaries and thus have to be replaced)
        public void PositionChange(PoseMsg poseMsg) {
            Vector3 unityPosition = (Vector3)new Vector3<FLU>((float)poseMsg.position.x, (float)poseMsg.position.y, (float)poseMsg.position.z).To<RUF>();
            Quaternion rotation = (Quaternion)new Quaternion<FLU>((float)poseMsg.orientation.x, (float)poseMsg.orientation.y, (float)poseMsg.orientation.z, (float)poseMsg.orientation.w).To<RUF>();
        }
        */

        /// <summary>
        /// Call this function in order to subscribe to the diagnostic status messages.
        /// <br></br> This function is mainly used for the ROS Status Menu of the Task Planning Interface.
        /// <para><paramref name="rosAction"/> = Function that should receive the diagnostic status message. It is important that the function has nothing else than the parameter DiagnosticStatusMsg. Example for rosAction: <c>ReportStatus</c> -> therefore only the name of the function</para>
        /// </summary>
        public void SubscribeToDiagnosticStatus(Action<DiagnosticStatusMsg> rosAction) {
            RegisterDataStream += delegate { _connection.Subscribe<DiagnosticStatusMsg>("tpi_diagnostic_status", rosAction); };
        }

        #endregion readyToUseFunctions



        //---------------------------------------------------- ROS Status Menu ----------------------------------------------------//



        #region rosStatusMenu

        /// <summary>
        /// Returns the amount of entries in the ROS Status Menu.
        /// </summary>
        public int GetStatusEntryCount() {
            return _statusList.Count;
        }

        /// <summary>
        /// Returns the list of entries in the ROS Status Menu.
        /// </summary>
        public List<DiagnosticStatusMsg> GetStatusEntries() {
            return _statusList;
        }

        /// <summary>
        /// Call this function in order to add a DiagnosticStatusMsg to the ROS Status Menu.
        /// <para><paramref name="statusMsg"/> = DiagnosticStatusMsg you want to add</para>
        /// </summary>
        public void AddStatusEntry(DiagnosticStatusMsg statusMsg) {

            if(rosConnectionType == ROSConnectionType.deactivated || (rosConnectionType == ROSConnectionType.requestIP && !hasConnectionThread())) {
                Debug.LogError("Please enable the ROS connection by either not setting the 'ROS Connection Type' to 'disabled' or by entering the IP address of the ROS workstation in order to use the 'ROS Status Menu'! (AddStatusEntry in TPI_ROSController)");
                return;
            }

            _statusList.Insert(0, statusMsg);

            if (_statusList.Count == 1) { // Deactivate No-Content Text in ROS Status Menu
                mainController.rosStatusMenu.transform.GetChild(4).gameObject.SetActive(false);
            }

            // Setup Status Entry
            GameObject statusObject = Instantiate(statusEntry, mainController.rosStatusMenu.transform.GetChild(3).GetChild(0).GetChild(0).GetChild(0).GetChild(0));
            statusObject.transform.SetSiblingIndex(0);
            _statusObjectsList.Insert(0, statusObject);
            statusObject.transform.localScale = mainController.rosStatusMenu.transform.localScale / 0.6f;
            statusObject.name = "Status Entry: " + statusMsg.name;

            /*
            Possible levels of operations.
            public const sbyte OK = 0;
            public const sbyte WARN = 1;
            public const sbyte ERROR = 2;
            public const sbyte STALE = 3;
            */

            // Set the correct icon
            if (statusMsg.level == 0) { // OK
                statusObject.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(infoTexture, new Rect(0, 0, infoTexture.width, infoTexture.height), new Vector2(0.5f, 0.5f));
            } else if(statusMsg.level == 1) { // WARN
                statusObject.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(warningTexture, new Rect(0, 0, warningTexture.width, warningTexture.height), new Vector2(0.5f, 0.5f));
            } else { // ERROR, STALE and others
                statusObject.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(errorTexture, new Rect(0, 0, errorTexture.width, errorTexture.height), new Vector2(0.5f, 0.5f));
            }


            // Replace Name & Message
            TextMeshProUGUI[] textsUGUI = statusObject.GetComponentsInChildren<TextMeshProUGUI>();
            for (int i = 0; i < textsUGUI.Length; i++) {
                if (textsUGUI[i] == null)
                    continue;
                if (textsUGUI[i].text.Contains("[Name]", System.StringComparison.OrdinalIgnoreCase)) {
                    textsUGUI[i].text = statusMsg.name;
                } else if (textsUGUI[i].text.Contains("[Message]", System.StringComparison.OrdinalIgnoreCase)) {
                    string messageContent = statusMsg.message;
                    if (statusMsg.hardware_id != "")
                        messageContent += "\nHardware ID: " + statusMsg.hardware_id;
                    textsUGUI[i].text = messageContent;
                } else if (textsUGUI[i].text.Contains("[TimeStamp]", System.StringComparison.OrdinalIgnoreCase)) {
                    textsUGUI[i].text = DateTime.Now.ToString("hh:mm:ss");
                }
            }

        }

        /// <summary>
        /// Call this function in order to remove a DiagnosticStatusMsg from the ROS Status Menu.
        /// <para><paramref name="statusMsg"/> = DiagnosticStatusMsg you want to remove</para>
        /// </summary>
        public void RemoveStatusEntry(DiagnosticStatusMsg statusMsg) {

            if (rosConnectionType == ROSConnectionType.deactivated || (rosConnectionType == ROSConnectionType.requestIP && !hasConnectionThread())) {
                Debug.LogError("Please enable the ROS connection by either not setting the 'ROS Connection Type' to 'disabled' or by entering the IP address of the ROS workstation in order to use the 'ROS Status Menu'! (RemoveStatusEntry in TPI_ROSController)");
                return;
            }

            if (!_statusList.Contains(statusMsg)) {
                Debug.LogError("The 'ROS Status Menu' does not contain the given DiagnosticStatusMsg! (RemoveStatusEntry in TPI_ROSController)");
                return;
            }

            // Remove and Destroy Status Entry
            GameObject go = _statusObjectsList[_statusList.IndexOf(statusMsg)];
            _statusObjectsList.Remove(go);
            Destroy(go);
            _statusList.Remove(statusMsg);
            if (_statusList.Count == 0) { // Activate No-Content Text in ROS Status Menu
                mainController.rosStatusMenu.transform.GetChild(4).gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Call this function in order to remove a DiagnosticStatusMsg from the ROS Status Menu.
        /// <para><paramref name="position"/> = positiob of the DiagnosticStatusMsg in the ROS Status List you want to remove</para>
        /// </summary>
        public void RemoveStatusEntry(int position) {

            if (rosConnectionType == ROSConnectionType.deactivated || (rosConnectionType == ROSConnectionType.requestIP && !hasConnectionThread())) {
                Debug.LogError("Please enable the ROS connection by either not setting the 'ROS Connection Type' to 'disabled' or by entering the IP address of the ROS workstation in order to use the 'ROS Status Menu'! (RemoveStatusEntry in TPI_ROSController)");
                return;
            }

            // Position index cannot be out of bounds
            int removalIndex = position;
            if (removalIndex < 0)
                removalIndex = 0;
            if (removalIndex > _statusList.Count - 1)
                removalIndex = _statusList.Count - 1;
            // Remove and Destroy Status Entry
            GameObject go = _statusObjectsList[removalIndex];
            _statusObjectsList.RemoveAt(removalIndex);
            Destroy(go);
            _statusList.RemoveAt(removalIndex);
            if (_statusList.Count == 0) { // Activate No-Content Text in ROS Status Menu
                mainController.rosStatusMenu.transform.GetChild(4).gameObject.SetActive(true);
            }
        }

        #endregion rosStatusMenu



        /// <summary>
        /// Helper enum that gives the user to choice to determine what happens with the ROS Connection.
        /// </summary>
        private enum ROSConnectionType {
            [InspectorName("ROS Connection deactivated")]
            deactivated, // 0
            [InspectorName("Enter the ROS IP and Port to start the Connection")]
            requestIP, // 1
            [InspectorName("Automatically start the ROS Connection")]
            autoStart, // 2
        }

    }

}