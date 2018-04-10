/*========================================================================
Product:    #HMDGazeAnalyzing
Developer:  #Jonas Iacobi
Company:    #KTH | SVRVIVE Studios
Date:       #2018-04-06
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMDGazeAnalyzing {
    public class HMDGazeReplayer : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        public static HMDGazeReplayer instance;

        [Header("Replay Settings")]
        [Tooltip("The index of the data file from which to load the data.")]
        public int fileIndex;
        [Tooltip("If this in enabled, the replayer will try to sync the time of the replay point to real time. This will cause real time playback, but sacrifice accuracy.")]
        public bool syncTime;
        [Tooltip("The button to pause and start playback.")]
        public KeyCode pauseButton;
        [Tooltip("The button to toggle rewind of playback.")]
        public KeyCode rewindButton;
        [Tooltip("The button to get the next data file..")]
        public KeyCode nextFileButton;
        [Tooltip("The button to get the previous data file..")]
        public KeyCode prevFileButton;

        [Header("Scene Visualization Settings")]
        [Tooltip("Toggle display of 3D gaze visualization.")]
        public bool showScenePoint;
        [Tooltip("Toggle display of viewport gaze visualization.")]
        public bool showViewportPoint;
        [Tooltip("The visual representation of the gaze point.")]
        public Transform gazeReplayPoint;
        [Tooltip("Increase this value to make the gaze point move smoother. Accuracy is sacrificed.")]
        [Range(0f, 0.2f)]
        public float minGazeDeltaPosition;
        [Tooltip("If the gaze has moved farther than this value, the gaze point is instantly moved there. If the replay does not account for quick shifts in gaze, decrease this number.")]
        [Range(0f, 2f)]
        public float maxGazeDeltaPosition;
        [Tooltip("The speed at which the gaze point will lerp between points. Reduce for smoother movement. Warning: too low values will make the gaze point lag behind the actual gaze position resulting in inaccurate replays.")]
        [Range(0f, 1f)]
        public float gazePointSpeed;

        [Header("Viewport Visualization Settings")]
        [Tooltip("The color of the viewport gaze point when valid.")]
        public Color pointColorValid;
        [Tooltip("The color of the viewport gaze point when position data is invalid.")]
        public Color pointColorBlink;
        [Tooltip("The transform representing the viewport gaze point.")]
        public Transform point;
        [Tooltip("The size of the viewport gaze point.")]
        [Range(0.01f, 1f)]
        public float pupilScale = 0.1f;
        [Tooltip("Modify to scale the magnitude of the viewport gaze point.")]
        [Range(0.5f, 3f)]
        public float gazeMagnitude = 1.5f;

        #endregion

        #region Private fields
        //The current index of the gaze data point.
        private int dataIndex;
        //The number of data points.
        private int dataCount;

        //The timestamp of the first gaze data.
        private float startTime;
        //Used to calculate total time passed.
        private float playTime;
        //The total time that has elapsed of the playback.
        private float totalTime;
        //Whether the replayer should be replaying.
        private bool replaying;
        //Whether the replay is rewinding or not.
        private bool rewinding;
        //Whether the replayer is properly initialized.
        private bool initialized;

        //The position the gaze point moves towards.
        private Vector3 targetPosition;
        //The current gaze data.
        private HMDGazeData currentData;
        //The mesh renderer of the gaze replay point.
        private MeshRenderer gazeReplayPointMeshRenderer;

        //The image representing the viewport gaze point.
        private Image pointImage;
        //The last known position of the viewport gaze point.
        private Vector2 pointPosition;
        //The last measured pupil size.
        private float pupilSize;
        //This canvas.
        private Canvas canvas;

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

        private void Start()
        {
            initialized = Initialize();
        }

        private void Update()
        {
            //Toggle replay
            if (Input.GetKeyDown(pauseButton))
            {
                ToggleReplay();
            }

            //Toggle rewind
            if (Input.GetKeyDown(rewindButton))
            {
                ToggleRewind();
            }

            //Change file index.
            if (Input.GetKeyDown(nextFileButton))
            {
                SetFileIndex(fileIndex + 1, true);
            }
            if (Input.GetKeyDown(prevFileButton))
            {
                SetFileIndex(fileIndex - 1, true);
            }

            if (initialized)
            {
                if(currentData == null)
                {
                    initialized = false;
                    return;
                }

                if (replaying)
                {
                    UpdateGazeData();
                    UpdateGazePoint();
                    UpdateViewportPoint();
                    UpdateGazeIndex();
                }
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Change the file index. Passing the second argument as true will reload the file immediately.
        /// </summary>
        /// <param name="index"></param>
        public void SetFileIndex(int index, bool reload)
        {
            if (HMDDataLoader.instance.DataFileExists(index) && index >= 0)
            {
                fileIndex = index;
                if (reload) initialized = Initialize();
            }
        }

        /// <summary>
        /// Toggle replay of the data on and off.
        /// </summary>
        public void ToggleReplay()
        {
            replaying = !replaying;

            if (replaying)
            {
                playTime = Time.time;
                if (!initialized)
                {
                    initialized = Initialize();
                    replaying = initialized;
                }
            } else
            {
                float dTime = Time.time - playTime;
                totalTime += rewinding ? -dTime : dTime;
            }
            
        }

        /// <summary>
        /// Toggle rewind on and off.
        /// </summary>
        public void ToggleRewind()
        {
            float dTime = Time.time - playTime;
            totalTime += rewinding ? -dTime : dTime;

            rewinding = !rewinding;
 
            playTime = Time.time;
        }
        #endregion

        #region Private methods
        //Load data and initialize the replayer.
        private bool Initialize()
        {
            if (initialized)
            {
                return true;
            }
            replaying = false;

            if (gazeReplayPoint == null)
            {
                Debug.Log("The Gaze Replay Point has not been assigned. Couldn't initialize.");
                return false;
            }
            
            gazeReplayPointMeshRenderer = gazeReplayPoint.GetComponent<MeshRenderer>();

            if(gazeReplayPointMeshRenderer == null)
            {
                Debug.Log("The gaze replay point has no Mesh Renderer. Did not initialize.");
                return false;
            }

            pointImage = point.GetComponent<Image>();
            pointPosition = Vector2.zero;
            canvas = GetComponentInChildren<Canvas>();
            if(canvas == null)
            {
                Debug.Log("Could not find a canvas component in children. Init failed.");
                return false;
            }

            if(pointImage == null)
            {
                Debug.Log("pointImage was not found, did not initalize.");
                return false;
            }

            HMDDataLoader.instance.LoadData(fileIndex);
            dataCount = HMDDataLoader.instance.GetData().Count;
            if (dataCount > 0)
            {
                currentData = HMDDataLoader.instance.GetData(0);
                if (currentData != null)
                {
                    startTime = currentData.timestamp;
                    playTime = startTime;
                    totalTime = 0;
                    return true;
                }
            }
            return false;
        }

        //Update the position and size of the gaze point
        private void UpdateGazePoint()
        {
            if (!showScenePoint)
            {
                if (gazeReplayPointMeshRenderer.enabled)
                {
                    gazeReplayPointMeshRenderer.enabled = false;
                }
            }
            else
            {
                if (!gazeReplayPointMeshRenderer.enabled)
                {
                    gazeReplayPointMeshRenderer.enabled = true;
                }

                if (currentData != null)
                {
                    //Update position
                    if (currentData.valid)
                    {
                        targetPosition = currentData.origin + (currentData.direction * currentData.distance);

                    }

                    //If distance to target point is lower than the minimum value or higher than the maximum value, set it to target position.
                    if (Vector3.Distance(gazeReplayPoint.position, targetPosition) > minGazeDeltaPosition || Vector3.Distance(gazeReplayPoint.position, targetPosition) > maxGazeDeltaPosition)
                    {
                        gazeReplayPoint.position = targetPosition;
                    }
                    else
                    {
                        gazeReplayPoint.position = Vector3.Lerp(gazeReplayPoint.position, targetPosition, gazePointSpeed);
                    }

                    //Update pupils size.
                    if (currentData.pupilsValid && !float.IsNaN(currentData.pupilSize))
                    {
                        gazeReplayPoint.transform.localScale = Vector3.one * 0.1f * currentData.pupilSize;
                    }
                    else
                    {
                        gazeReplayPoint.transform.localScale = Vector3.zero;
                    }
                }
            }
        }

        //Update the position and size of the viewport point
        private void UpdateViewportPoint()
        {
            if (!showViewportPoint)
            {
                if (pointImage.enabled)
                {
                    pointImage.enabled = false;
                }
            }
            else
            {
                if (!pointImage.enabled)
                {
                    pointImage.enabled = true;
                }

                //Set position and pupil size.
                if (currentData != null)
                {
                    if (currentData.valid)
                    {
                        pointPosition = new Vector2((currentData.viewPortPoint.x - 0.5f) * Screen.width, currentData.viewPortPoint.y * canvas.GetComponent<RectTransform>().rect.height);
                        // canvas.GetComponent<RectTransform>().rect.width,0);// (currentData.viewPortPoint.y * canvas.GetComponent<RectTransform>().rect.height));
                        //pointPosition = new Vector2((currentData.viewPortPoint.x - 0.5f) * canvas.GetComponent<RectTransform>().rect.width, -0.5f * Screen.height + currentData.viewPortPoint.y * canvas.GetComponent<RectTransform>().rect.height);
                    }

                    //Set pupil size and blink color.
                    if (currentData.pupilsValid && !float.IsNaN(currentData.pupilSize))
                    {
                        pupilSize = currentData.pupilSize * pupilScale;
                        pointImage.color = pointColorValid;
                    } else
                    {
                        pointImage.color = pointColorBlink;
                    }

                    //Set the position of the point.
                    point.localPosition = new Vector3(pointPosition.x, pointPosition.y, 20f);
                }
            }
        }
        
        private void UpdateGazeData()
        {
            //Check if there is data in the list.
            if(dataCount == 0)
            {
                initialized = false;
                return;
            }

            //Update current data.
            if(dataIndex < dataCount)
            {
                HMDGazeData nextData = HMDDataLoader.instance.GetData(dataIndex);
                if(nextData != null)
                {
                    currentData = nextData;
                }
            }
        }

        //Sets the next data index.
        private void UpdateGazeIndex()
        {
            //If time sync is off, increment index.
            if (!syncTime)
            {
                if(replaying)
                {
                    IncrementIndex();
                }
            } else
            {
                int whileKiller = 0;
                while (SyncTime() && dataIndex < dataCount-1 && (!rewinding || dataIndex > 0))
                {
                    if(whileKiller > 10000)
                    {
                        Debug.Log("While loop exceeded 10000, terminated.");
                        Debug.Log("Data index: " + dataIndex + " | Data count: " + dataCount);
                        return;
                    }
                    whileKiller++;
                    IncrementIndex();
                }
            }
        }

        //Increase or decrease the data index by one, based on whether rewind is on or not.
        private void IncrementIndex()
        {
            dataIndex += rewinding ? -1 : 1;

            //Keep index within bounds.
            if (dataIndex > dataCount - 1)
            {
                dataIndex = dataCount - 1;
            }
            if (dataIndex < 0)
            {
                dataIndex = 0;
            }
        }

        //Calculate total elapsed time
        private float GetElapsedTime()
        {
            float dTime = Time.time - playTime;
            return totalTime + (rewinding ? -dTime : dTime);
        }

        //Check whether time needs syncing
        private bool SyncTime()
        {
            float timestamp = HMDDataLoader.instance.GetData(dataIndex).timestamp - startTime;
            if (rewinding)
            {
                return timestamp > GetElapsedTime();
            }
            else
            {
                return timestamp < GetElapsedTime();
            }
            
        }

        #endregion
    }
}