/*========================================================================
Product:    #HMDGazeAnalyzing
Developer:  #Jonas Iacobi
Company:    #KTH | SVRVIVE Studios
Date:       #2018-04-06
========================================================================*/

using System.Collections.Generic;
using System.IO;
using Tobii.Research.Unity; //Supplies the gaze data object from the Tobii eye tracker. Remove this if you are using a custom eye tracker.
using UnityEngine;

namespace HMDGazeAnalyzing
{
    public class HMDGazeRecorder : MonoBehaviour 
    {
        #region Constants
        #endregion

        #region Public fields
        /// <summary>
        /// The HMDGazeReocrder instance.
        /// </summary>
        public static HMDGazeRecorder instance;

        [Tooltip("The camera representing the user's eyes. Corresponds to [CameraRig]->Camera(head)->Camera(eye) in the SteamVR prefab.")]
        public Camera mainCamera;
        [Tooltip("Enter a custom file name here for easier organization of data files.")]
        public string customFileName;
        [Tooltip("Whether the recorder should raycast to see which object the user looks at. Uncheck to disable collection of object name and distance data. May impact performance.")]
        public bool raycast = true;
        [Tooltip("Check this to make the recorder overwrite existing data files.")]
        public bool overwrite;
        [Tooltip("The button used to start and stop recording.")]
        public KeyCode recordButton;
        #endregion

        #region Private fields
        //Whether the recorder is recording data.
        private bool record;
        //The Unity EyeTracker helper object, included in the Tobii Pro VR SDK for Unity.
        private VREyeTracker eyeTracker;
        //Whether the eye tracker has been calibrated. Set this from an external script that's responsible for ev. calibration.
        private bool calibrated;
        //A list containing recorded gaze data objects that have not yet been written to file.
        private List<HMDGazeData> gazeDataBuffer;
        //Keeps track of the last recorded distance, used to set distance of points with no associated distance data.
        private float lastDistance;
        //The index of the file currently written to. Reset when recorder starts. If overwrite is disabled, the index will automatically be set to the current file count + 1.
        private int fileIndex;
        //The name of the file to use.
        private string fileName;
	    #endregion

	    #region Unity methods

	    private void Awake () 
	    {
            //Singleton
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            //Check that the camera has been assigned.
            if(mainCamera == null)
            {
                Debug.Log("Attention: No camera has been assigned to the recorder. Disabling.");
                enabled = false;
            }

            fileName = customFileName ?? HMDDataSettings.DATA_FILE_NAME;
        }

        private void Start()
        {
            //Create directory and set file index.
            InitializeFile();

            //Create the gaze data buffer list
            gazeDataBuffer = new List<HMDGazeData>();

            //Get the eye tracker from the scene.
            eyeTracker = VREyeTracker.Instance;
        }

	
	    private void Update () 
	    {
            //Only attempt recording if the eyetracker is connected.
            if (!eyeTracker.Connected)
            {
                return;
            }

            //Toggle recording on or off if the recording button is pressed.
            if (Input.GetKeyDown(recordButton))
            {
                ToggleRecording();
            }

            //Handle the data objects from the eye tracker each frame.
            while (eyeTracker.GazeDataCount > 0)
            {
                //The eyetracker will still enqueue data even if it's not used, this removes data not used by the recorder.
                //ATTENTION: This will remove all data not used by this object. If you want to use the eye tracking data in other objects, please make a class that stores it and then passes it here.
                IVRGazeData nextData = eyeTracker.NextData;
                if (record)
                {
                    //If we are recording, convert the eyetracking data to gaze data objects and write them to the buffer.
                    EnqueueGazeData(EncodeGazeData(nextData));
                }

                //This is only here for the demo scene. Ugly solution, but fast.
                if (HMDDemo.GazeRaycaster.instance != null && nextData.CombinedGazeRayWorldValid)
                {
                    HMDDemo.GazeRaycaster.instance.Raycast(nextData.CombinedGazeRayWorld);
                }
            }

	    }

        #endregion

        #region Public methods
        /// <summary>
        /// Start recording and write to default filename.
        /// </summary>
        public void StartRecording()
        {
            if (!record)
            {
                ToggleRecording();
            }
        }

        /// <summary>
        /// Start the recording and write to file with name fName.
        /// </summary>
        /// <param name="fName"></param>
        public void StartRecording(string fName)
        {
            if (!record)
            {
                customFileName = fName;
                InitializeFile();
                ToggleRecording();
            }
        }

        /// <summary>
        /// Stop the recording and write to file.
        /// </summary>
        public void StopRecording()
        {
            if (record)
            {
                ToggleRecording();
            }
        }


        #endregion

        #region Private methods

        #region Recording methods
        // Toggle the recording of gaze data on and off. Toggling off will automatically write data to file.
        private void ToggleRecording()
        {
            record = !record;
            Debug.Log("HMDGazeRecorder recording is set to " + record);
            //Write data to file when the recording is stopped. Restarting it will create a new data file.
            if (!record)
            {
                WriteGazeData(GetFileName());
            }
        }

        //Turn a data object from the eye tracker into a custom gaze tracker object, ready to be serialized.
        private HMDGazeData EncodeGazeData(IVRGazeData _IVRGazeData)
        {
            if(_IVRGazeData == null)
            {
                return null;
            }

            HMDGazeData gazeData = new HMDGazeData();
            gazeData.valid = _IVRGazeData.CombinedGazeRayWorldValid;
            gazeData.timestamp = Time.time;

            Vector3 viewportPoint = Vector3.zero;
            if (gazeData.valid)
            {
                viewportPoint = GazeToViewportPoint(mainCamera, _IVRGazeData.CombinedGazeRayWorld.direction);
            }

            string objectName = "None";
            float distance = lastDistance;
            if (raycast)
            {
                RaycastHit hit;
                if (Physics.Raycast(_IVRGazeData.CombinedGazeRayWorld, out hit))
                {
                    distance = hit.distance;
                    lastDistance = distance;

                    GameObject target = hit.collider.gameObject;
                    if(target != null)
                    {
                        objectName = target.name;
                    }
                }
            } else
            {
                objectName = "Not Tracked";
            }

            gazeData.viewPortPoint = viewportPoint;
            gazeData.distance = distance;
            gazeData.origin = _IVRGazeData.CombinedGazeRayWorld.origin;
            gazeData.direction = _IVRGazeData.CombinedGazeRayWorld.direction;
            gazeData.objectName = objectName;
            gazeData.pupilsValid = IsPupilDataValid(_IVRGazeData);
            gazeData.pupilSize = AveragePupilDiameter(_IVRGazeData);

            return gazeData;
        }

        //Adds the gaze data to the list.
        private void EnqueueGazeData(HMDGazeData gazeData)
        {
            if (gazeData != null)
            {
                gazeDataBuffer.Add(gazeData);
            }
        }


        // Returns a point on the viewport corrsponding to the supplied camera and direction vector.
        private Vector3 GazeToViewportPoint(Camera mainCamera, Vector3 direction)
        {
            return mainCamera.WorldToViewportPoint(direction);
        }

        //Return whether the pupil data is valid.
        private bool IsPupilDataValid(IVRGazeData igd)
        {
            return (igd.Left.PupilDiameterValid && igd.Right.PupilDiameterValid);
        }

        //Return the average pupil diameter in meters.
        private float AveragePupilDiameter(IVRGazeData igd)
        {
            return (igd.Right.PupilDiameter + igd.Left.PupilDiameter) / 2f * 1000f;
        }
        #endregion

        #region IO methods
        //Writes the data buffer to file and clears the list.
        private void WriteGazeData(string fName)
        {
            //If the file is to be overwritten, delete it first.
            if(overwrite && File.Exists(Path.Combine(HMDDataSettings.DATA_FILE_DIRECTORY, fName)))
            {
                File.Delete(Path.Combine(HMDDataSettings.DATA_FILE_DIRECTORY, fName));
            }

            using (StreamWriter file = File.AppendText(Path.Combine(HMDDataSettings.DATA_FILE_DIRECTORY, fName))){
                for(int i = 0; i < gazeDataBuffer.Count; i++)
                {
                    file.WriteLine(JsonUtility.ToJson(gazeDataBuffer[i]));
                }
                gazeDataBuffer.Clear();

                //If the write was successful, incement the file index.
                if (fileIndex < HMDDataSettings.MAX_DATA_FILE_COUNT)
                {
                    fileIndex++;
                }
                else
                {
                    Debug.Log("There are more than " + HMDDataSettings.MAX_DATA_FILE_COUNT + " data files in the " + HMDDataSettings.DATA_FILE_DIRECTORY + "directory. The Recorder will not create more. Please remove some of the existing data files, or change the MAX_DATA_FILE_COUNT const in the Recorder.");
                }

                file.Flush();
                file.Close();
            }
        }

        //Creates the data file directory and sets the file index.
        private void InitializeFile()
        {
            if (!Directory.Exists(HMDDataSettings.DATA_FILE_DIRECTORY))
            {
                Directory.CreateDirectory(HMDDataSettings.DATA_FILE_DIRECTORY);
            }

            if(!string.IsNullOrEmpty(customFileName))
            {
                fileName = customFileName.Trim();
            }
            else
            {
                fileName = HMDDataSettings.DATA_FILE_NAME;
            }

            if (!overwrite && File.Exists(Path.Combine(HMDDataSettings.DATA_FILE_DIRECTORY, fileName + HMDDataSettings.DATA_FILE_ENDING)))
            {
                fileIndex = 1;
                while (File.Exists(Path.Combine(HMDDataSettings.DATA_FILE_DIRECTORY, fileName + "(" + fileIndex + ")" + HMDDataSettings.DATA_FILE_ENDING)))
                {
                    fileIndex++;
                }
            } else
            {
                fileIndex = 0;
            }
        }

        //Returns a file name for the next data file.
        private string GetFileName()
        {
            string name;

            if (fileIndex == 0)
            {
                name = fileName + HMDDataSettings.DATA_FILE_ENDING;
            }
            else
            {
                name = fileName + "(" + fileIndex + ")" + HMDDataSettings.DATA_FILE_ENDING;
            }

            return name;
        }
        #endregion

        #endregion
    }
}