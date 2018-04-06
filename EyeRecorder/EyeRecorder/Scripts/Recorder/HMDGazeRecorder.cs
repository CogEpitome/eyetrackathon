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
        //The name of the directory in which the data files are to be stored.
        private const string DATA_FILE_DIRECTORY = "HMDGazeAnalyzingData";
        //The name of the data files. Files are then separated by version numbers appended to this name.
        private const string DATA_FILE_NAME = "gazeRecording";
        //The file ending, determines the file's type.
        private const string DATA_FILE_ENDING = ".txt";
        //The maximum amount of data files allowed to be stored.
        private const int MAX_DATA_FILE_COUNT = 1000;
        #endregion

        #region Public fields
        public static HMDGazeRecorder instance;
        [Tooltip("The camera representing the user's eyes. Corresponds to [CameraRig]->Camera(head)->Camera(eye) in the SteamVR prefab.")]
        public Camera mainCamera;
        [Tooltip("Whether the recorder should raycast to see which object the user looks at. Uncheck to disable collection of object name and distance data. May impact performance.")]
        public bool raycast;
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

            //The eyetracker will still enqueue data even if it's not used, this removes data not used by the recorder.
            //ATTENTION: This will remove all data not used by this object. If you want to use the eye tracking data in other objects, please make a class that stores it and then passes it here.
            if (!record)
            {
                while (eyeTracker.GazeDataCount > 0)
                {
                    var discardMe = eyeTracker.NextData;
                }
                return;
            }

            //If we are recording, convert the eyetracking data to gaze data objects and write them to the buffer.
            if (record)
            {
                //Record data
                while(eyeTracker.GazeDataCount > 0)
                {
                    EnqueueGazeData(EncodeGazeData(eyeTracker.NextData));     
                }
                //Move this to a separate place for performance reasons.
                WriteGazeData(OpenFile());
            } 
	    }

        #endregion

        #region Public methods
        #endregion

        #region Private methods

        #region Recording methods
        //toggle the recording on or off.
        private void ToggleRecording()
        {
            record = !record;
        }

        //Turn a data object from the eye tracker into a custom gaze tracker object, ready to be serialized.
        private HMDGazeData EncodeGazeData(IVRGazeData igd)
        {
            if(igd == null)
            {
                return null;
            }

            HMDGazeData gazeData = new HMDGazeData();
            gazeData.valid = igd.CombinedGazeRayWorldValid;
            gazeData.timestamp = Time.time;

            Vector3 viewportPoint = Vector3.zero;
            if (gazeData.valid)
            {
                viewportPoint = Utils.GazeToViewportPoint(mainCamera, igd.CombinedGazeRayWorld.direction);
            }

            string objectName = "None";
            float distance = lastDistance;
            if (raycast)
            {
                RaycastHit hit;
                if (Physics.Raycast(igd.CombinedGazeRayWorld, out hit))
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

            gazeData.distance = distance;
            gazeData.origin = igd.CombinedGazeRayWorld.origin;
            gazeData.direction = igd.CombinedGazeRayWorld.direction;
            gazeData.objectName = objectName;
            gazeData.pupilsValid = IsPupilDataValid(igd);
            gazeData.pupilSize = AveragePupilDiameter(igd);

            return gazeData;
        }

        //Adds the gaze data to the list.
        private void EnqueueGazeData(HMDGazeData gazeData)
        {
            gazeDataBuffer.Add(gazeData);
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
        private void WriteGazeData(string fileName)
        {
            using (StreamWriter file = File.AppendText(Path.Combine(DATA_FILE_DIRECTORY, fileName))){
                for(int i = 0; i < gazeDataBuffer.Count; i++)
                {
                    file.WriteLine(JsonUtility.ToJson(gazeDataBuffer[i]));
                }
                gazeDataBuffer.Clear();

                file.Flush();
                file.Close();
            }
        }

        //Prepares a data file and returns its name.
        private string OpenFile()
        {
            if (!Directory.Exists(DATA_FILE_DIRECTORY))
            {
                Directory.CreateDirectory(DATA_FILE_DIRECTORY);
            }

            string name = DATA_FILE_NAME + DATA_FILE_ENDING;
            int i = 0;
            while(File.Exists(Path.Combine(DATA_FILE_DIRECTORY, name)))
            {
                name = DATA_FILE_NAME + "(" + i + ")" + DATA_FILE_ENDING;

                if (i < MAX_DATA_FILE_COUNT)
                {
                    i++;
                } else
                {
                    Debug.Log("There are more than " + MAX_DATA_FILE_COUNT + " data files in the " + DATA_FILE_DIRECTORY + "directory. The Recorder will not create more. Please remove some of the existing data files, or change the MAX_DATA_FILE_COUNT const in the Recorder.");
                    return null;
                }
            }

            return name;
        }
        #endregion

        #endregion
    }
}