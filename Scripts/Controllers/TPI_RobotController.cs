using RosMessageTypes.Geometry;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

namespace TaskPlanningInterface.Controller {

    /// <summary>
    /// <para>
    /// The TPI_RobotController handles the movements of the virtual twin in the Unity Mixed Reality Environment.
    /// <br></br>IMPORTANT: This script was specifically setup for the "FRANKA RESEARCH 3" Robotic Arm and needs to be reconfigured if a different one should be used.
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
    /// Source:
    /// <br></br>Heavily adapted Version of parts of the "JointController" script found in the Master Thesis "Human-Robot Object Handovers using Mixed Reality" by Manuel Koch at PDZ. -> Therefore, he will be also listed as author.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Manuel Koch (mankoch@student.ethz.ch)
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [System.Serializable]
    public class TPI_RobotController : MonoBehaviour {

        private TPI_ROSController _rosController;

        [Tooltip("Add the reference to the ArticulationBody Component on the Robot Base Link. If you do not add anything, it will automatically set it to the 'FRANKA RESEARCH 3' standard settings.")]
        public ArticulationBody _robotBase;

        // Name of all the links containing movable joints. The order is important!
        [Tooltip("Add the names of all the links that contain movable joints. Please pay attention, as the order is important!")]
        public string[] robotLinkNames =
        {
        "panda_link0/panda_link1",
        "panda_link0/panda_link1/panda_link2",
        "panda_link0/panda_link1/panda_link2/panda_link3",
        "panda_link0/panda_link1/panda_link2/panda_link3/panda_link4",
        "panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5",
        "panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6",
        "panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7",
        //"panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7/panda_link8/panda_hand/panda_rightfinger", // If you also want to move the fingers, you can turn both options back on.
        //"panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7/panda_link8/panda_hand/panda_leftfinger",
        };

        // Base Pose Options
        [Tooltip("Add the topic name under which the robot base pose will be either published or to which Unity will subscribe.")]
        public string robotBaseTopic = "tpi_robot_basepose";
        [Tooltip("Determine whether the robot base pose should be received.")]
        [SerializeField]private ReceiveRobotBasePose receiveRobotBasePose;
        [Tooltip("Determine whether the robot base pose should be published.")]
        [SerializeField]private PublishRobotBasePose publishRobotBasePose;

        [Tooltip("In the case of publishRobotBasePose being set to automatically publishing, set the frequency how many times each seconds the robot base pose should be published.")]
        public float robotBase_publishFrequency = 1;
        private float timeElapsed;

        // Robot Joint Options
        [Tooltip("Add the topic name to which Unity will subscribe in order to display the movement of the robot.")]
        public string robotJointTopic = "tpi_robot_joints";
        private ArticulationBody[] robotJoints = new ArticulationBody[9];

        // Robot Pose Options
        [Tooltip("Add the topic name under which a pose should be published to ROS.")]
        public string robotPoseTopic = "tpi_robot_pose";
        [Tooltip("Add the topic name to which Unity will subscribe in order to receive the information whether a pose is reachable or not.")]
        public string poseReachableTopic = "tpi_robot_posereachable";
        [Tooltip("Designate what happens if a pose is reachable.")]
        public UnityEngine.Events.UnityEvent poseReachableEvent;
        [Tooltip("Designate what happens if a pose is not reachable.")]
        public UnityEngine.Events.UnityEvent poseNotReachableEvent;

        private void Start() {

            _rosController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().rosController.GetComponent<TPI_ROSController>();
            if(_robotBase == null)
                _robotBase = transform.GetChild(1).GetComponent<ArticulationBody>(); // located on the panda_link0 GameObject

            for (var i = 0; i < robotLinkNames.Length; i++) {
                robotJoints[i] = transform.Find(robotLinkNames[i]).GetComponent<ArticulationBody>(); ;
            }
            _robotBase.immovable = true;

            // Publishers & Subscribers will be registered once a connection to ROS has been established without errors.
            if (receiveRobotBasePose != ReceiveRobotBasePose.doNot) {
                TPI_ROSController.RegisterDataStream += delegate { _rosController.Subscribe<TransformMsg>(robotBaseTopic, SetRobotBase); };
                TPI_ROSController.RegisterDataStream += delegate { _rosController.RegisterPublisher<TransformMsg>(robotBaseTopic); };
            }
            TPI_ROSController.RegisterDataStream += delegate { _rosController.Subscribe<JointStateMsg>(robotJointTopic, MoveRobotJoints); };
            TPI_ROSController.RegisterDataStream += delegate { _rosController.RegisterPublisher<PoseMsg>(robotPoseTopic); };
            TPI_ROSController.RegisterDataStream += delegate { _rosController.Subscribe<BoolMsg>(poseReachableTopic, IsPoseReachable); };

            TPI_ROSController.RegisterDataStream += delegate { transform.GetChild(1).gameObject.SetActive(true); };

        }

        private void LateUpdate() {

            // Publish RobotBase Pose every 1 / robotBase_publishFrequency seconds
            if (_rosController.hasConnectionThread() && !_rosController.hasConnectionError() && publishRobotBasePose == PublishRobotBasePose.automatically) {
                timeElapsed += Time.deltaTime;
                if (timeElapsed > 1 / robotBase_publishFrequency) {
                    PublishRobotBase();
                    timeElapsed = 0;
                }
            }

        }

        /// <summary>
        /// This function allows you to enable (true) or disable (false) the visibility of the robot.
        /// <para><paramref name="status"/>: status = true -> robot is visible, status = false -> robot is disabled</para>
        /// </summary>
        public void SetRobotActive(bool status) {
            _robotBase.gameObject.SetActive(status);
        }

        /// <summary>
        /// Return whether the robot is currently visible (not disabled).
        /// </summary>
        public bool isRobotActive() {
            return _robotBase.gameObject.activeSelf;
        }


        /// <summary>
        /// This function is used as the Callback function for the robotBaseTopic.
        /// <br></br>Once it receives a pose (TransformMsg), it applies it to the base of the robot.
        /// <para><paramref name="msg"/>= Message that was received from ROS</para>
        /// </summary>
        public void SetRobotBase(TransformMsg msg) {

            _robotBase.transform.position = (Vector3)new Vector3<FLU>((float)msg.translation.x, (float)msg.translation.y, (float)msg.translation.z).To<RUF>();
            _robotBase.transform.rotation = (Quaternion)new Quaternion<FLU>((float)msg.rotation.x, (float)msg.rotation.y, (float)msg.rotation.z, (float)msg.rotation.w).To<RUF>();

            _robotBase.immovable = false;
            _robotBase.TeleportRoot((Vector3)new Vector3<FLU>((float)msg.translation.x, (float)msg.translation.y, (float)msg.translation.z).To<RUF>(), (Quaternion)new Quaternion<FLU>((float)msg.rotation.x, (float)msg.rotation.y, (float)msg.rotation.z, (float)msg.rotation.w).To<RUF>());
            _robotBase.velocity = Vector3.zero;
            _robotBase.angularVelocity = Vector3.zero;
            _robotBase.immovable = true;
            
            if(receiveRobotBasePose != ReceiveRobotBasePose.continuously)
                _rosController.Unsubscribe(robotBaseTopic);
        }

        /// <summary>
        /// This function is used to publish the current pose of the robot base in the robotBaseTopic (publishes a TransformMsg).
        /// </summary>
        public void PublishRobotBase() {
            TransformMsg msg = GetTransformMsg(_robotBase.gameObject);
            _rosController.Publish(robotBaseTopic, msg);
        }

        /// <summary>
        /// This function is used to build the TransformMsg for a GameObject.
        /// <para><paramref name="_gameObject"/>= GameObject for which you want to create the TransformMsg</para>
        /// </summary>
        private TransformMsg GetTransformMsg(GameObject _gameObject) {
            Vector3Msg position = new Vector3Msg(_gameObject.transform.position.x,  _gameObject.transform.position.y, _gameObject.transform.position.z);
            QuaternionMsg rotation = new QuaternionMsg(_gameObject.transform.rotation.x, _gameObject.transform.rotation.y,  _gameObject.transform.rotation.z, _gameObject.transform.rotation.w);
            return new TransformMsg(position, rotation);
        }

        /// <summary>
        /// This function is used as the Callback function for the robotJointTopic.
        /// <br></br>Once it receives the values for the joints (JointStateMsg), it applies them to all the movable joints.
        /// <br></br>In the case of the "FRANKA RESEARCH 3" Robotic Arm, the JointStateMsg should have a position array of size 9 (as there are 9 movable joints).
        /// <para><paramref name="jointMsg"/>= Message that was received from ROS</para>
        /// </summary>
        private void MoveRobotJoints(JointStateMsg jointMsg) {
            for (int i = 0; i < robotLinkNames.Length; i++) {
                float angle = (float)jointMsg.position[i];
                robotJoints[i].jointPosition = new ArticulationReducedSpace(angle);
            }
        }

        /// <summary>
        /// This function is used as the Callback function for the poseReachableTopic.
        /// <br></br>Depending on whether the pose that was sent to ROS is reachable or not, either the poseReachableEvent or the poseNotReachableEvent is invoked.
        /// <para><paramref name="boolMsg"/>= Message that was received from ROS</para>
        /// </summary>
        private void IsPoseReachable(BoolMsg boolMsg) {
            if (boolMsg != null && boolMsg.data) {
                poseReachableEvent.Invoke();
            } else {
                poseNotReachableEvent.Invoke();
            }
        }

        /// <summary>
        /// This function can be used to publish the current Pose of the hand of the "FRANKA RESEARCH 3" Robotic Arm.
        /// <br></br>IMPORTANT: If you use other robots than the "FRANKA RESEARCH 3", you should not use this function, but rather the PublishRobotPose(ArticulationBody articulationBody) or ublishRobotPose(GameObject gameObject) functions, as this function will otherwise throw an error.
        /// <para><paramref name="convertToRelativeCoordinates"/>= Determine whether the TPI_RobotController should convert the global coordinates to coordinates that are relative to the position of the robot base</para>
        /// </summary>
        public void PublishRobotPose(bool convertToRelativeCoordinates = true) {
            PublishRobotPose(transform.Find("panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7/panda_link8/panda_hand").gameObject, convertToRelativeCoordinates);
        }

        /// <summary>
        /// This function can be used to publish the current Pose of ArticulationBody that you provide.
        /// <para><paramref name="articulationBody"/>= the ArticulationBody Component on the GameObject of which you want to publish the current Pose</para>
        /// <para><paramref name="convertToRelativeCoordinates"/>= Determine whether the TPI_RobotController should convert the global coordinates to coordinates that are relative to the position of the robot base</para>
        /// </summary>
        public void PublishRobotPose(ArticulationBody articulationBody, bool convertToRelativeCoordinates = true) {
            PublishRobotPose(articulationBody.transform.position, articulationBody.transform.rotation, convertToRelativeCoordinates);
        }

        /// <summary>
        /// This function can be used to publish the current Pose of GameObject that you provide.
        /// <para><paramref name="gameObject"/>= GameObject of which you want to publish the current Pose</para>
        /// <para><paramref name="convertToRelativeCoordinates"/>= Determine whether the TPI_RobotController should convert the global coordinates to coordinates that are relative to the position of the robot base</para>
        /// </summary>
        public void PublishRobotPose(GameObject gameObject, bool convertToRelativeCoordinates = true) {
            PublishRobotPose(gameObject.transform.position, gameObject.transform.rotation, convertToRelativeCoordinates);
        }

        /// <summary>
        /// This function can be used to publish the current Pose of GameObject that you provide.
        /// <br></br>Compared to the other versions of the 'PublishRobotPose' function, this specific function can be also used if you want to alter the extracted position or rotation of a GameObject before you publish it.
        /// <para><paramref name="_worldPosition"/>= Vector3 of the world position that you want to publish</para>
        /// <para><paramref name="_worldRotation"/>= Quaternion of the world rotation that you want to publish</para>
        /// <para><paramref name="convertToRelativeCoordinates"/>= Determine whether the TPI_RobotController should convert the global coordinates to coordinates that are relative to the position of the robot base</para>
        /// </summary>
        public void PublishRobotPose(Vector3 _worldPosition, Quaternion _worldRotation, bool convertToRelativeCoordinates = true) {

            PoseMsg poseMsg;
            if (convertToRelativeCoordinates) {
                poseMsg = GetRelativePose(_worldPosition, _worldRotation);
            } else {
                Vector3<FLU> worldPosition = _worldPosition.To<FLU>(); // .To<FLU>() Converts the Unity Coordinate System into the ROS Coordinate System
                Quaternion<FLU> worldRotation = _worldRotation.To<FLU>();
                PointMsg position = new PointMsg(worldPosition.x, worldPosition.y, worldPosition.z);
                QuaternionMsg rotation = new QuaternionMsg(worldRotation.x, worldRotation.y, worldRotation.z, worldRotation.w);
                poseMsg = new PoseMsg(position, rotation);
            }
            if (poseMsg != null)
                _rosController.Publish(robotPoseTopic, poseMsg);
            else
                Debug.LogError("The TPI_RobotController was not able to create the PoseMsg. Therefore, nothing was published to ROS! (PublishRobotPose in TPI_RobotController)");
        }

        /// <summary>
        /// This function converts the world position and rotation of a given GameObject into the relative local coordinates of the Robot Base Link.
        /// </summary>
        public PoseMsg GetRelativePose(GameObject gameObject) {
            return GetRelativePose(gameObject.transform.position, gameObject.transform.rotation);
        }

        /// <summary>
        /// This function converts the given world position and rotation into the relative local coordinates of the Robot Base Link.
        /// </summary>
        public PoseMsg GetRelativePose(Vector3 worldPosition, Quaternion worldRotation) {
            Matrix4x4 worldToLocalMatrix = _robotBase.transform.worldToLocalMatrix;
            Vector3<FLU> localPosition = worldToLocalMatrix.MultiplyPoint(worldPosition).To<FLU>(); // .To<FLU>() Converts the Unity Coordinate System into the ROS Coordinate System
            Quaternion<FLU> localRotation = Quaternion.LookRotation(worldToLocalMatrix.MultiplyVector(worldRotation * Vector3.forward), worldToLocalMatrix.MultiplyVector(worldRotation * Vector3.up)).To<FLU>();
            PointMsg position = new PointMsg(localPosition.x, localPosition.y, localPosition.z);
            QuaternionMsg rotation = new QuaternionMsg(localRotation.x, localRotation.y, localRotation.z, localRotation.w);
            return new PoseMsg(position, rotation);
        }

        /// <summary>
        /// This enum is used to give the user a choice on whether the RobotController should receive the robot base TransformMsg (position + rotation).
        /// </summary>
        private enum ReceiveRobotBasePose {
            [InspectorName("Do not receive")]
            doNot, // 0
            [InspectorName("Receive once")]
            once, // 1
            [InspectorName("Receive continuously")]
            continuously, // 2
        }

        /// <summary>
        /// This enum is used to give the user a choice on whether the RobotController should publish the robot base TransformMsg (position + rotation).
        /// </summary>
        private enum PublishRobotBasePose {
            [InspectorName("Do not publish")]
            doNot, // 0
            [InspectorName("Publish manually")]
            manually, // 1
            [InspectorName("Publish automatically")]
            automatically, // 2
        }

    }

}


