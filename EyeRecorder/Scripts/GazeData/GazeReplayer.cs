/*========================================================================
Product:    #PROJECTNAME#
Developer:  #DEVELOPERNAME#
Company:    #COMPANY#
Date:       #CREATIONDATE#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace HMDEyeTracking {
    [ExecuteInEditMode]
    public class GazeReplayer : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        public static GazeReplayer instance;
        public Texture gazePointView;
        [Header("Replay Settings")]
        [Tooltip("Represents the index of the next data point. Change to move gaze data time forwards or back.")]
        public int gazeIndex;
        [Tooltip("Pause playback.")]
        public bool paused;
        [Tooltip("If this in enabled, the replayer will try to sync the time of the replay point to real time. This will cause real time playback, but sacrifice accuracy.")]
        public bool syncTime;

        [Header("3D Visualization Settings")]
        [Tooltip("The visual representation of the gaze point.")]
        public Transform gazeReplayPoint;
        [Tooltip("Increase this value to make the gaze point move smoother. Accuracy is sacrificed.")]
        [Range(0, 0.2f)]
        public float minGazeDeltaPosition;
        [Tooltip("If the gaze has moved farther than this value, the gaze point is instantly moved there. If the replay does not account for quick shifts in gaze, decrease this number.")]
        [Range(0, 2)]
        public float maxGazeDeltaPosition;
        [Tooltip("The speed at which the gaze point will lerp between points. Reduce for smoother movement. Warning: too low values will make the gaze point lag behind the actual gaze position resulting in inaccurate replays.")]
        [Range(0, 1)]
        public float gazePointSpeed;

        [Header("Plane Visualizer Settings")]
        [Tooltip("Whether to show the plane visualization.")]
        public bool showPlaneVisualization = true;
        [Tooltip("Determines the size of the 2D gaze visualization in the scene view")]
        public Rect size;
        #endregion

        #region Private fields
        //The time since game started that the recording began, in seconds.
        private float gazeStartTime;
        //The time that recording was resumed. Used to calculate total time passed.
        private float resumeTime;
        //The total time elapsed of the replay until the last resume. To get the actual total time, add the latest resumeTime to this value.
        private float sumtime;
        //Whether an automatic replay is in progress. The data point can still be manually operated if false.
        private bool isReplaying;
        //The position the gaze point moves towards.
        private Vector3 targetPosition;
        //The current gaze data.
        private Utils.GazeData currentData;
        //Reference to the data loader with the list of gaze points.
        private GazeDataLoader dataLoader;
        //Reference to the gaze plane visualizer.
        private GazePlaneVisualizer gazePlaneVisualizer;
        //The bool deciding whether the plane visualization is shown. Private to prevent users from changing it during runtime.
        private bool showingPlaneVisualization;
        //Whether the replayer was successfully initialized
        private bool initialized;
        #endregion

        #region Unity methods
        private void OnEnable()
        {
            //Singleton
            if (instance != null && instance != this)
            {
                enabled = false;
                return;
            }
            instance = this;

            initialized = Initialize();
        }

        private void Awake()
        {
            initialized = false;
            gazeIndex = 0;
            resumeTime = 0;
            gazeStartTime = 0;
            targetPosition = Vector3.zero;
        }

        private void Update()
        {
            if (initialized || !EditorApplication.isPlaying)
            {
                if(currentData == null)
                {
                    initialized = false;
                    return;
                }
                UpdateGazeData();
                UpdateGazeReplayPoint();
                gazePlaneVisualizer.UpdatePlaneGazePoint(currentData);
                UpdateGazeIndex();
            }
            else
            {
                initialized = Initialize();
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Get the relative time signature of the current gaze point, i.e. how long afte recording began that the gaze point was created.
        /// </summary>
        /// <returns></returns>
        public float GetReplayTime()
        {
            Utils.GazeData data = GetCurrentData();
            float t = 0;
            if (data != null)
            {
                t = data.timestamp - GetGazeStartTime();
            }
            return t;
        }

        /// <summary>
        /// Get the real time that has elapsed since replaying began.
        /// </summary>
        /// <returns></returns>
        public float GetRealTime()
        {
            return GetSumTime() + Time.time - GetResumeTime();
        }

        /// <summary>
        /// Get the total replay time 
        /// </summary>
        /// <returns></returns>
        public float GetSumTime()
        {
            return sumtime;
        }

        public float GetResumeTime()
        {
            return resumeTime;
        }

        /// <summary>
        /// Get the time of the first gaze point. Represents the time since the game started that the recording began.
        /// </summary>
        /// <returns></returns>
        public float GetGazeStartTime()
        {
            return gazeStartTime;
        }

        /// <summary>
        /// Get the current Gaze data object processed by the replayer.
        /// </summary>
        /// <returns></returns>
        public Utils.GazeData GetCurrentData()
        {
            return currentData;
        }

        /// <summary>
        /// Whether data is currently being replayed.
        /// </summary>
        /// <returns></returns>
        public bool IsReplaying()
        {
            return isReplaying;
        }

        /// <summary>
        /// Start, pause, or resume the replay of gaze data. Use IsReplaying() to check the current replay state.
        /// </summary>
        public void ToggleReplay()
        {
            if (!isReplaying)
            {
                if(gazeStartTime == 0) gazeStartTime = GazeDataLoader.instance.GetGazeData(0).timestamp;
                resumeTime = Time.time;
                isReplaying = true;
                Debug.Log("Started replay at t + "+ GetReplayTime() + "s");
            } else
            {
                sumtime += Time.time - resumeTime;
                isReplaying = false;
                Debug.Log("Stopped replay + " + GetReplayTime()+"s");
            }
        }

        /// <summary>
        /// Stop and reset the replay of gaze data.
        /// </summary>
        public void StopReplay()
        {
            gazeIndex = 0;
            isReplaying = false;
        }
        #endregion

        #region Private methods
        //Initializes the replayer. Returns true if ready.
        private bool Initialize()
        {
            isReplaying = false;
            dataLoader = GazeDataLoader.instance;
            if (dataLoader == null) return false;
            currentData = dataLoader.GetGazeData(0);
            if (currentData == null) return false;
            gazeStartTime = currentData.timestamp;
            gazePlaneVisualizer = GetComponent<GazePlaneVisualizer>();
            if (showingPlaneVisualization && gazePlaneVisualizer == null) showingPlaneVisualization = false;
            return true;
        }

        private void UpdateGazeData()
        {
            //Check the amount of data points in the list
            int listCount = GazeDataLoader.instance.GetGazeData().Count;
            //Check the gaze index to ensure it is within the allowed range
            if (gazeIndex >= listCount) gazeIndex = listCount - 1;
            if (gazeIndex < 0) gazeIndex = 0; 

            if (listCount == 0)
            {
                gazeReplayPoint.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (resumeTime == 0) resumeTime = GazeDataLoader.instance.GetGazeData(0).timestamp;
                gazeReplayPoint.gameObject.SetActive(true);
            }

            if (gazeIndex < listCount)
            {
                Utils.GazeData newData = GazeDataLoader.instance.GetGazeData(gazeIndex);
                if(newData != null)
                    currentData = newData;
            }
        }

        //Updates the position of the gaze replay point
        private void UpdateGazeReplayPoint()
        {
            if (currentData != null)
            {
                if (currentData.valid)
                {
                    targetPosition = currentData.origin + (currentData.direction * currentData.distance);
                    if (Vector3.Distance(gazeReplayPoint.position, targetPosition) > minGazeDeltaPosition)
                        gazeReplayPoint.position = targetPosition;

                }
                if (Vector3.Distance(gazeReplayPoint.position, targetPosition) > maxGazeDeltaPosition)
                    gazeReplayPoint.position = targetPosition;
                else
                    gazeReplayPoint.position = Vector3.Lerp(gazeReplayPoint.position, targetPosition, gazePointSpeed);

                if (currentData.pupilsValid && !float.IsNaN(currentData.pupilsSize))
                    gazeReplayPoint.transform.localScale = Vector3.one * 0.1f * currentData.pupilsSize;
                else
                    gazeReplayPoint.transform.localScale = Vector3.zero;
            }
     
        }

        private void UpdateGazeIndex()
        {
            if (!syncTime)
            {
                if ((EditorApplication.isPlaying && isReplaying))
                {
                    gazeIndex++;
                }
            } else
            {
                int whileKilla = 0;
                while (GetReplayTime(gazeIndex) < GetSumTime() + Time.time - GetResumeTime() && gazeIndex < GazeDataLoader.instance.GetGazeData().Count)
                {
                    whileKilla++;
                    if(whileKilla > 10000)
                    {
                        Debug.Log("While loop in GazeReplayer UpdateGazeIndex ran for flippin ever");
                        return;
                    }
                    gazeIndex++;
                }
            }
        }

        private float GetReplayTime(int index)
        {
            Utils.GazeData data = dataLoader.GetGazeData(index);
            float t = 0;
            if (data != null)
            {
                t = data.timestamp - GetGazeStartTime();
            }
            return t;
        }
        #endregion
    }

#region EDITOR
#if UNITY_EDITOR
    [CustomEditor(typeof(GazeReplayer))]
    public class GazeReplayerEditor : Editor
    {
        private float realTime = 0;

        #region Unity methods
        private void OnEnable()
        {
        }

        private void OnSceneGUI()
        {
            float replayTime = 0;

            if (GazeReplayer.instance != null)
            {
                if (GazeReplayer.instance.IsReplaying())
                {
                    realTime = GazeReplayer.instance.GetRealTime();
                    replayTime = GazeReplayer.instance.GetReplayTime();
                }
            } else
            {
                return;
            }
            

            Handles.BeginGUI();
            GUILayout.BeginArea(GazeReplayer.instance.size);

            Rect rect = EditorGUILayout.BeginVertical();
            GUI.color = Color.green;
            GUI.Box(rect, GUIContent.none);

            GUI.color = Color.white;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Eyetracking Gaze Graph");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Label("Replay Time: " + replayTime + "s");
            GUILayout.Label("Elapsed Time: " + realTime + "s");



            if (GazeReplayer.instance.showPlaneVisualization)
            {
                GUIContent content = new GUIContent();
                content.image = GazeReplayer.instance.gazePointView;
                GUILayout.Box(content);
            }
            GUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
        #endregion
    }
#endif
#endregion
}