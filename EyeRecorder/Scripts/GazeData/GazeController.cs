/*========================================================================
Product:    #PROJECTNAME#
Developer:  #DEVELOPERNAME#
Company:    #COMPANY#
Date:       #CREATIONDATE#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HMDEyeTracking
{
    [RequireComponent(typeof(GazeRecorder))]
    [RequireComponent(typeof(GazeReplayer))]
    [ExecuteInEditMode]
    public class GazeController : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        public static GazeController instance;
        [Tooltip("Control whether the Gaze Controller is running.")]
        public bool activate;
        [Tooltip("Enable the Gaze Recorder. This will record eyetracking data according to the options selected in the GazeRecorder component.")]
        public bool recordGaze;
        [Tooltip("Enable the Gaze Replayer. This will replay eyetracking data according to the options selected in the GazeReplayer component.")]
        public bool replayGaze;
        [Tooltip("Key to start and stop the recording of eyetracking data.")]
        public KeyCode recordKey;
        [Tooltip("Key to start and pause the replay of eyetracking data.")]
        public KeyCode replayKey;
        #endregion

        #region Private fields
        //Whether the gaze controller is running.
        private bool activated;
        //Internal checks for whether the controller is recording/replaying gaze data. For the purpose of avoiding reliance on public bools.
        private bool recordingGaze, replayingGaze;
        #endregion

        #region Unity methods
        private void Awake()
        {
            //Singleton
            if (instance != null && instance != this)
            {
                enabled = false;
                return;
            }
            instance = this;
        }

        private void OnEnable()
        {
            if(activate)
                Activate();
        }

        private void Update()
        {
            if (activated)
            {
                if(GazeReplayer.instance == null && replayGaze)
                {
                    activated = false;
                    Debug.Log("lost the gazereplayer, deactivating");
                    return;
                }

                if (GazeReplayer.instance.GetEnable())
                {
                    if (!replayGaze)
                    {
                        GazeReplayer.instance.Disable();
                        replayingGaze = false;
                    }
                } else
                {
                    if (replayGaze)
                    {
                        Activate();
                    }
                }

                if (EditorApplication.isPlaying)
                {
                    if (GazeRecorder.instance.GetEnable())
                    {
                        if (!recordGaze)
                        {
                            GazeRecorder.instance.Disable();
                            recordingGaze = false;
                        }
                    }
                    else
                    {
                        if (recordGaze)
                        {
                            Activate();
                        }
                    }
                }

                //Make sure the recorder only starts in play mode.
                if (EditorApplication.isPlaying)
                {
                    //Handle replaying Gaze data in play mode.
                    if (replayingGaze)
                    {
                        if (Input.GetKeyDown(replayKey))
                        {
                            GazeReplayer.instance.ToggleReplay();
                        }
                    }

                    //Handle recording Gaze Data
                    if (recordingGaze)
                    {
                        //
                        if (!GazeRecorder.instance.GetRecording() && GazeRecorder.instance.autoRecord)
                        {
                            GazeRecorder.instance.StartRecording();
                        }
                        //Start or stop the recording on key press.
                        if (Input.GetKeyDown(recordKey))
                        {
                            if (!GazeRecorder.instance.GetRecording()) GazeRecorder.instance.StartRecording();
                            else GazeRecorder.instance.StopRecording();
                        }
                    }
                
                }
            } else
            {
                if (EditorApplication.isPlaying)
                {
                    //Disable recorder and replayer if controller is deactivated
                    if (recordingGaze)
                    {
                        recordingGaze = false;
                        GazeRecorder.instance.StopRecording();
                    }
                    
                }

                if (replayingGaze)
                {
                    replayingGaze = false;
                    GazeReplayer.instance.StopReplay();
                }

                //If the controller is set to be active but is not, try to activate it.
                if (activate)
                    Activate();
            }
        }
        #endregion

        #region Public methods
        #endregion

        #region Private methods
        //Safely activates the controller by checking requirements before setting the activated boolean.
        private void Activate()
        {
            //If there is no gazedataloader, the controller can't operate. Exit.
            /*if (GazeDataLoader.instance == null)
            {
                Debug.Log("Fatal: Could not find a GazeDataLoader instance. Disabling GazeController.");
                enabled = false;
                return;
            }
            else 
            */
            GazeDataLoader.LoadGazeData();

            //If the controller is set to enable the replayer, 
            if (replayGaze)
            {
                if (GazeReplayer.instance == null)
                {
                    Debug.Log("Could not find a GazeReplayer instance. Disabling replaying of gaze data.");
                    replayGaze = false;
                    replayingGaze = false;
                } else
                {
                    replayingGaze = replayGaze;
                    GazeReplayer.instance.Enable();
                }
            }

            //If the application is playing and the controller is set to activate the recorder.
            if (EditorApplication.isPlaying)
            {
                if (recordGaze)
                {
                    if (GazeRecorder.instance == null)
                    {
                        Debug.Log("Could not find a GazeRecorder instance. Disabling recording of gaze data.");
                        recordGaze = false;
                        recordingGaze = false;
                    }
                    else
                    {
                        recordingGaze = recordGaze;
                        GazeRecorder.instance.Enable();
                    }
                }
                       
            }
        
            //Prepare the data loader by reading data from file for later access.
            GazeDataLoader.LoadGazeData();

            //Activation confirmed.
            activated = true;
        }
        #endregion
    }
}