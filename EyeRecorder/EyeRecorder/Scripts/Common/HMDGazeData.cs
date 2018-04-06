/*========================================================================
Product:    #HMDGazeAnalyzing
Developer:  #Jonas Iacobi
Company:    #KTH | SVRVIVE Studios
Date:       #2018-04-06
========================================================================*/

using UnityEngine;

namespace HMDGazeAnalyzing
{
    /*
     * This class contains data on a single gaze data point.
     * Can be serialized as JSON for writing to file.
     */
    [System.Serializable]
    public class HMDGazeData : MonoBehaviour
    {
        #region Public fields
        /// <summary>
        /// Whether the position and direction data is valid.
        /// </summary>
        public bool valid;
        /// <summary>
        /// The time at which the data point was recorded, in seconds.
        /// </summary>
        public float timestamp;
        /// <summary>
        /// The point on the viewPort at which the user was looking.
        /// </summary>
        public Vector2 viewPortPoint;
        /// <summary>
        /// The user's position in space when the data was recorded.
        /// </summary>
        public Vector3 origin;
        /// <summary>
        /// The direction of the user's gaze.
        /// </summary>
        public Vector3 direction;
        /// <summary>
        /// The distance from the camera to the viewed object.
        /// </summary>
        public float distance;
        /// <summary>
        /// The name of the object looked at, if any.
        /// </summary>
        public string objectName = "None";
        /// <summary>
        /// Whether the pupil size data is valid.
        /// </summary>
        public bool pupilsValid;
        /// <summary>
        /// The average pupil size of the user's eyes.
        /// </summary>
        public float pupilSize;
        #endregion
    }
}