/*========================================================================
Product:    #HMDEyeTracking#
Developer:  #Jonas Iacobi#
Company:    #KTH#
Date:       #2018-03-20#
========================================================================*/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace HMDEyeTracking
{
    /// <summary>
    /// Used to retrieve GazeData objects from the data file. Triggered externally.
    /// </summary>
    [ExecuteInEditMode]
    public class GazeDataLoader : MonoBehaviour
    {
        #region Constants
        //The name of the directory in which data from the recorder will be stored.
        private const string DATA_FILE_DIRECTORY = "EyeRecorderData";
        //The name of the XML file in which data from the recorder will be stored.
        private const string DATA_FILE_NAME = "data.txt";
        //The maximum amount of gaze data objects to load into memory. Large values may cause RAM shortage.
        private const int MAX_DATALIST_LENGTH = 10000;
        #endregion

        #region Classes, Structs and Enumerations

        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        public static GazeDataLoader instance;

        [Tooltip("Uncheck to discard data points with invalid point and pupil data. Disable to discard data from time spent with eyes closed or not in VR headset.")]
        public bool loadInvalid = true;
        #endregion

        #region Private fields
        private List<Utils.GazeData> gazeDataList;
        #endregion

        #region Unity methods
        private void OnEnable()
        {
            //Singleton
            if (instance != null && instance != this)
            {
                Debug.Log("There are multiple instances of GazeDataLoader in the scene, please make sure there is only one. Or don't, it works either way.");
                return;
            }
            instance = this;

            gazeDataList = new List<Utils.GazeData>();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Loads all lines from the data text file and converts it to a list of GazeData objects for later use.
        /// </summary>
        public void LoadGazeData()
        {
            string path = Path.Combine(DATA_FILE_DIRECTORY, DATA_FILE_NAME);

            if (!File.Exists(path))
            {
                Debug.Log("GazeDataLoader could not find a file matching the path: "+ path + ". Please ensure the GazeRecorder has been running.");
                return;
            }

            //Read file content into memory.
            string[] jsonStrings = File.ReadAllLines(path);
            //If any lines were successfully read, repopulate the list of GazeData.
            if (jsonStrings.Length > 0)
            {
                gazeDataList.Clear();
                int invalidData = 0;
                Utils.GazeData tempGazeData;
                int forLength = Mathf.Min(jsonStrings.Length, MAX_DATALIST_LENGTH);
                for (int i = 0; i < forLength; i++)
                {
                    tempGazeData = (Utils.GazeData)JsonUtility.FromJson(jsonStrings[i], typeof(Utils.GazeData));
                    if (tempGazeData.valid || tempGazeData.pupilsValid || loadInvalid)
                    {
                        gazeDataList.Add(tempGazeData);
                    }
                    else
                    {
                        invalidData++;
                    }
                }
                Debug.Log("GazeDataLoader loaded " + gazeDataList.Count + " objects, and discarded " + invalidData + " invalid data objects.");
            } else
            {
                Debug.Log("GazeDataLoader failed to load strings from file. Is it empty?");
            }
        }

        public List<Utils.GazeData> GetGazeData()
        {
            return gazeDataList;
        }

        public Utils.GazeData GetGazeData(int i)
        {
            if (gazeDataList.Count > i)
                return gazeDataList[i];
            else return null;
        }
        #endregion

        #region Private methods
        #endregion
    }
}
