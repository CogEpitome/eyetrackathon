/*=====================================================================.===
Product:    #HMDEyeTracking#
Developer:  #Jonas Iacobi#
Company:    #KTH#
Date:       #2018-03-20#
========================================================================*/

using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Tobii.Research.Unity;


namespace HMDEyeTracking {
    /// <summary>
    /// Stores the data output of a VR Eyetracker in a local data XML file.
    /// Based on example code by Tobii AB.
    /// 
    /// IMPORTANT NOTICE: the recorder will continuosly empty the Gaze Data queue of the VREyeTracker.
    /// If you need to use that data elsewhere, remove all instances of "eyeTracker.NextData" in the Update method
    /// and create a separate script to store them before passing them here.
    /// </summary>
    public class GazeRecorder : MonoBehaviour
    {
        #region Constants
        //The name of the directory in which data from the recorder will be stored.
        private const string DATA_FILE_DIRECTORY = "EyeRecorderData";
        //The name of the XML file in which data from the recorder will be stored.
        private const string DATA_FILE_NAME = "data.txt";
        //The maximum buffer size before force writing to file
        private const int MAX_BUFFER_SIZE = 500;
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        public static GazeRecorder instance;
        [Tooltip("Whether to start recording automatically")]
        public bool autoRecord;
        [Tooltip("The camera representing the user's eyes. Corresponds to [CameraRig]->Camera(head)->Camera(eye) in the SteamVR prefab.")]
        public Camera mainCamera;
        [Tooltip("Whether the recorder should require the eyetracker to be calibrated.")]
        public bool requireCalibration;
        [Tooltip("Whether the recorder should collect data on which object is looked at.")]
        public bool trackObjects = true;
        [Tooltip("Whether the recorder should overwrite the data file.")]
        public bool overwrite;
        #endregion

        #region Private fields
        //Whether the class should be recording eye tracking data.
        private bool isRecording;
        // The Unity EyeTracker helper object, included in the Tobii Pro VR SDK for Unity.
        private VREyeTracker eyeTracker;
        //Whether the eyetracking has been calibrated
        private bool calibrated;
        //Ordered list of gaze data structs to write to file.
        private List<Utils.GazeData> gazeDataBuffer;
        //Whether the file has already been overwritten this play session.
        private bool overwritten;
        //The last distance recorded.
        private float lastDistance;
        //Whether the recorder has been enabled.
        private bool enable;
        #endregion

        #region Unity methods
        private void Awake()
        {
            //Singleton
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        

        private void Update()
        {
            if (enable)
            {
                //Only record data if the eyetracker is connected.
                if (eyeTracker.Connected)
                {
                    if (!isRecording)
                    {
                        while (eyeTracker.GazeDataCount > 0)
                        {
                            //While not recording, empty the eyetracker queue.
                            var discardData = eyeTracker.NextData;
                        }
                    }

                    //If recording, save the recorded data to the data file.
                    if (isRecording)
                    {

                        //Check if it is time to write to disk
                        if (gazeDataBuffer.Count > MAX_BUFFER_SIZE)
                        {
                            //Write buffer to disk and empty list
                            WriteGazeData();
                        }
                        else
                        {
                            while (eyeTracker.GazeDataCount > 0)
                            {
                                //While there is eye tracking data left in the eyetracker's data queue, add it to the list.
                                EnqueueGazeData(eyeTracker.NextData);
                            }
                        }


                    }
                }
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Enable the recorder.
        /// </summary>
        public void Enable()
        {
            //Check if there is an eyetracker in the scene.
            eyeTracker = VREyeTracker.Instance;
            if (eyeTracker == null)
            {
                Debug.Log("The GazeRecorder could not find an instance of VREyeTracker in the scene. It is included in the Tobii Pro VR Unity SDK.");
                Debug.Log("The GazeRecorder will now terminate");
                Destroy(gameObject);
            }

            isRecording = false;
            calibrated = false;
            lastDistance = 0f;
            gazeDataBuffer = new List<Utils.GazeData>();
            enable = true;
            Debug.Log("Recorder enabled");
        }

        /// <summary>
        /// Enable the recorder.
        /// </summary>
        public void Disable()
        {
            enable = false;
            Debug.Log("Recorder disabled");
        }

        /// <summary>
        /// Starts the recording.
        /// </summary>
        public void StartRecording()
        {
            //If the recorder is not recording data, and calibration is finished or not required - the recorder will enable recording.
            if (!isRecording && (!requireCalibration || calibrated))
            {
                isRecording = true;
                OpenDataFile();
                Debug.Log("Recording gaze data");
            }

        }

        /// <summary>
        /// Stops/pauses the recording and writes data to file.
        /// </summary>
        public void StopRecording()
        {
            WriteGazeData();
            isRecording = false;
            Debug.Log("The recording of gaze data has stopped.");
        }

        /// <summary>
        ///Get whether the recorder is currently recording.
        /// </summary>
        public bool GetRecording()
        {
            return isRecording;
        }

        /// <summary>
        ///Get whether the recorder is currently enabled.
        /// </summary>
        public bool GetEnable()
        {
            return enable;
        }
        #endregion

        #region Private methods
        //Adds a gazeData struct to the encoding queue
        private void EnqueueGazeData(IVRGazeData _IVRGazeData)
        {
            //Check if the data is valid.
            if (_IVRGazeData == null) return;
            //Create a gazedata object and set its variables
            Utils.GazeData gazeData = new Utils.GazeData();
            gazeData.valid = _IVRGazeData.CombinedGazeRayWorldValid;
            gazeData.timestamp = Time.time;

            Vector3 gazeViewPortPoint = Vector3.zero;
            if (gazeData.valid)
            {
                gazeViewPortPoint = Utils.GetGazeViewPortPoint(mainCamera, _IVRGazeData);
            }
            gazeData.point = new Vector2(gazeViewPortPoint.x, gazeViewPortPoint.y);
            

            string objName = "None";

            RaycastHit rayHit;
            if (Physics.Raycast(_IVRGazeData.CombinedGazeRayWorld, out rayHit))
            {
                //Get the name of the object hit if trackObjects is enabled.
                if (trackObjects) {
                    GameObject gazeObject = rayHit.collider.gameObject;
                    if (gazeObject != null)
                    {
                        objName = gazeObject.name;
                    }
                } else
                {
                    objName = "Not tracked";
                }

                gazeData.distance = rayHit.distance;
                lastDistance = gazeData.distance;
            } else
            {
                gazeData.distance = lastDistance;
            }


            gazeData.origin = _IVRGazeData.CombinedGazeRayWorld.origin;
            gazeData.direction = _IVRGazeData.CombinedGazeRayWorld.direction;
            gazeData.objectName = objName;
            gazeData.pupilsValid = Utils.IsPupilDataValid(_IVRGazeData);
            gazeData.pupilsSize = Utils.GetAveragePupilDiameter(_IVRGazeData);

            //Add the object to the list of gazedatas.
            gazeDataBuffer.Add(gazeData);
        }

        //Writes gazeData buffer to file
        private void WriteGazeData()
        {
            if (overwrite && !overwritten)
            {
                overwritten = true;
                File.Delete(Path.Combine(DATA_FILE_DIRECTORY, DATA_FILE_NAME));
            }

            using (StreamWriter file = File.AppendText(Path.Combine(DATA_FILE_DIRECTORY, DATA_FILE_NAME)))
            {
                for (int i = 0; i < gazeDataBuffer.Count; i++)
                {
                    file.WriteLine(JsonUtility.ToJson(gazeDataBuffer[i]));
                }
                gazeDataBuffer.Clear();
                file.Flush();
                file.Close();
            }
            
        }

        //Handles opening the data file
        private void OpenDataFile()
        {
            //Check if the directory exists, otherwise create it.
            if (!Directory.Exists(DATA_FILE_DIRECTORY))
            {
                Directory.CreateDirectory(DATA_FILE_DIRECTORY);
            }

            overwritten = false;
        }
        #endregion
    }
}