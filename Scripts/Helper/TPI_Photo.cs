using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.Sensor;
using System;
using RosMessageTypes.BuiltinInterfaces;
using TaskPlanningInterface.Controller;

namespace TaskPlanningInterface.Helper {

    /// <summary>
    /// <para>
    /// This script takes a picture and sends it to ROS for calibration.
    /// <br></br>The main aim of this class is to calibrate the position of the robot's base using arUco markers.
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
    /// <br></br>Adapted Version of the "Photo" script found in the Master Thesis "Human-Robot Object Handovers using Mixed Reality" by Manuel Koch at PDZ. -> Therefore, he will be also listed as author.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Manuel Koch (mankoch@student.ethz.ch)
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public class TPI_Photo : MonoBehaviour {

        private PhotoCapture photoCaptureObject = null;
        private Texture2D PhotoTexture = null;

        [SerializeField] private GameObject camAxes;

        private TPI_ROSController rosController;
        [Tooltip("Topic name under which the HoloLens camera image (ImageMsg) should be published to ROS.")]
        public string topic_Image = "tpi_camera_image";
        [Tooltip("Topic name under which the camera matrix (TransformMsg) should be published to ROS.")]
        public string topic_CameraTransform = "tpi_camera_transform";
        private int imgHeight;
        private int imgWidth;


        void Start() {
            rosController = GameObject.FindGameObjectWithTag("TPI_Manager").GetComponent<TPI_MainController>().rosController.GetComponent<TPI_ROSController>();

            // Publishers will be registered once a connection to ROS has been established without errors.
            TPI_ROSController.RegisterDataStream += delegate { rosController.RegisterPublisher<ImageMsg>(topic_Image); };
            TPI_ROSController.RegisterDataStream += delegate { rosController.RegisterPublisher<TransformMsg>(topic_CameraTransform); };
        }

        /// <summary>
        /// Takes a picture with the HoloLens RGB camera and sends it to ROS.
        /// </summary>
        public void TakePhoto() {
            Debug.Log("TakePhoto was invoked.");

            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            imgHeight = 720;
            imgWidth = 1080;

            PhotoTexture = new Texture2D(imgWidth, imgHeight);


            // Create a PhotoCapture object
            PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
                photoCaptureObject = captureObject;
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 0.0f;
                cameraParameters.cameraResolutionWidth = imgWidth;
                cameraParameters.cameraResolutionHeight = imgHeight;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;


                // Activate the camera
                photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                    // Take a picture
                    Debug.Log("Camera activated.");
                    photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                });
            });


        }

        /// <summary>
        /// While taking a picture send the camera pose to ROS and visualize it.
        /// </summary>
        void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {

            photoCaptureFrame.TryGetCameraToWorldMatrix(out Matrix4x4 camToWorldMat);

#if UNITY_EDITOR
            camToWorldMat = GetCamToWorld();
#endif

            // camToWorldMat has a turned corrdinate system compared to unity
            Matrix4x4 turnCoordinates = Matrix4x4.identity;
            turnCoordinates.m00 = -1; // flip x-axis
            turnCoordinates.m22 = -1; // flip z-axis

            // Transformation matrix of camera in unity coordinates
            Matrix4x4 uT_uc = camToWorldMat * turnCoordinates;

            // set visualization
            Vector3 trans = uT_uc.GetColumn(3);
            Quaternion quat = Quaternion.LookRotation(uT_uc.GetColumn(2), uT_uc.GetColumn(1));
            camAxes.transform.position = trans;
            camAxes.transform.rotation = quat;

            // Copy the raw image data into the target texture
            photoCaptureFrame.UploadImageDataToTexture(PhotoTexture);

            //Send message with raw image
            SendUncompressedImage(PhotoTexture, imgHeight, imgWidth);

            // Send camera matrix
            SendMatrix(uT_uc, topic_CameraTransform);

            // Deactivate the camera
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }

        /// <summary>
        /// Shutdown the photo capture resource.
        /// </summary>
        void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
            photoCaptureObject.Dispose();
            photoCaptureObject = null;
        }

        /// <summary>
        /// Publishes the image to ROS.
        /// <para><paramref name="_image"/> = Texture of the image that should be sent</para>
        /// <para><paramref name="imgheight"/> = height of the image that should be sent</para>
        /// <para><paramref name="imgwidth"/> = width of the image that should be sent</para>
        /// </summary>
        void SendUncompressedImage(Texture2D _image, int imgheight, int imgwidth) {

            // create header
            var second = (uint)Time.time;
            var nanosecond = (uint)((Time.time - second) * 1000000000);
            var timeMessage = new TimeMsg(second, nanosecond);
            var headerMessage = new HeaderMsg(0, timeMessage, "camera");

            // create image message
            UInt32 height = (uint)imgheight;
            UInt32 width = (uint)imgwidth;
            string encoding = "bgra8";
            byte is_bigendian = Convert.ToByte(false);
            UInt32 step = width * 4;
            byte[] data = _image.GetRawTextureData();

            ImageMsg image_msg = new ImageMsg(
                headerMessage,
                height,
                width,
                encoding,
                is_bigendian,
                step,
                data
                );

            // publish message
            rosController.Publish(topic_Image, image_msg);
            Debug.Log("Webcam Image sent.");
        }

        /// <summary>
        /// Publish the TransformMsg of the given camera matrix.
        /// <para><paramref name="matrix"/> = Matrix of the Camera that should be sent to ROS</para>
        /// <para><paramref name="topic"/> = Topic name under which the Camera Matrix should be sent</para>
        /// </summary>
        void SendMatrix(Matrix4x4 matrix, string topic) {

            Vector3Msg pos = new Vector3Msg(
               x: matrix.m03,
               y: matrix.m13,
               z: matrix.m23
                );

            Quaternion quat = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
            QuaternionMsg rot = new QuaternionMsg(
                x: quat.x,
                y: quat.y,
                z: quat.z,
                w: quat.w
                );

            TransformMsg transform_msg = new TransformMsg(
                       pos,
                       rot
                       );

            rosController.Publish(topic, transform_msg);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns the camera position when executing the function in the Unity editor (not applicable on the HoloLens).
        /// </summary>
        public Matrix4x4 GetCamToWorld() {
            // Get camera position when executing in unity editor

            var camTransl = Camera.main.transform.position;
            var camOrient = Camera.main.transform.rotation;
            var worldToCam = Matrix4x4.TRS(camTransl, camOrient, Vector3.one);
            worldToCam.m20 *= -1f;
            worldToCam.m21 *= -1f;
            worldToCam.m22 *= -1f;
            worldToCam.m23 *= -1f;

            return worldToCam;
        }
#endif

    }

}