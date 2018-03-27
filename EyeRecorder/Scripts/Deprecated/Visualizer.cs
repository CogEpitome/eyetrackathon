/*========================================================================
Product:    #HMDEyeTracking#
Developer:  #Jonas Iacobi#
Company:    #KTH#
Date:       #2018-03-20#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.Research.Unity;

namespace HMDEyeTracking
{
    /// <summary>
    /// Temporary class to show a rudimentary visualization of the collected data points.
    /// </summary>
    public class Visualizer : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        [Tooltip("The camera representing the user's eyes. Corresponds to [CameraRig]->Camera(head)->Camera(eye) in the SteamVR prefab.")]
        public Camera mainCamera;
        [Tooltip("The color of the gaze points when nearest the camera.")]
        public Color pointColorNear;
        [Tooltip("The color of the gaze points when farthest from the camera.")]
        public Color pointColorFar;

        public Transform point;
        #endregion

        #region Private fields
        //The point the user is looking at translated onto the viewport.
        private Vector3 viewPortPoint;
        //The depths of the point the user is looking at
        private float depth;
        //The maximum distance to which the point color blends.
        private float farDistance;
        //Contains the average size of the pupils for a given Gaze instant.
        private float pupilSize;
        //The image representing the point.
        private Image pointImage;
        // The Unity EyeTracker helper object, included in the Tobii Pro VR SDK for Unity.
        private VREyeTracker eyeTracker;
        #endregion

        #region Unity methods

        private void Start()
        {
            eyeTracker = VREyeTracker.Instance;
            if (eyeTracker == null)
            {
                Debug.Log("The Visualizer could not find an instance of VREyeTracker in the scene. It is included in the Tobii Pro VR Unity SDK.");
                Debug.Log("The Visualizer will now terminate");
                Destroy(gameObject);
            }

            farDistance = 4f;// mainCamera.farClipPlane;
            pointImage = point.GetComponent<Image>();
            pupilSize = 1f;
        }

        private void Update()
        {
            while (eyeTracker.GazeDataCount > 0)
            {
                //While there is eye tracking data left in the eyetracker's data queue, show it.
                SetPoint(eyeTracker.NextData);
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Draws a point on the plane based on an IVRGazeData object
        /// </summary>
        /// <param name="gazeData"></param>
        public void SetPoint(IVRGazeData gazeData)
        {
            //Collect poistion data
            if (gazeData.CombinedGazeRayWorldValid)
            {
                viewPortPoint = mainCamera.WorldToViewportPoint(gazeData.CombinedGazeRayWorld.direction);
                depth = viewPortPoint.z;            
            }

            //Collect pupil data
            if (Utils.IsPupilDataValid(gazeData))
            {
                pupilSize = Utils.GetAveragePupilDiameter(gazeData);

            }

            //Set point
            point.localPosition = new Vector3(viewPortPoint.x-0.5f, viewPortPoint.y, 0f)*2f;
            if(pupilSize > 0) point.localScale = Vector3.one * pupilSize / 2f;
            pointImage.color = Color.Lerp(pointColorNear, pointColorFar, Mathf.Min(depth / farDistance, 1f));
        }
        #endregion

        #region Private methods
        #endregion
    }
}
