/*========================================================================
Product:    #HMDEyeTracking#
Developer:  #Jonas Iacobi#
Company:    #KTH#
Date:       #2018-03-20#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Tobii.Research.Unity;

namespace HMDEyeTracking
{
    public static class Utils
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        [System.Serializable]
        //Contains information on a single gaze point. Serialized and saved as JSON.
        public class GazeData
        {
            public bool valid;
            public float timestamp;
            public Vector2 point;
            public Vector3 origin;
            public Vector3 direction;
            public float distance;
            public string objectName;

            public bool pupilsValid;
            public float pupilsSize;
        }
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        #endregion

        #region Private fields
        #endregion

        #region Public methods
        /// <summary>
        /// Translates a point in world space to a point on the viewport. Used to visualize a gaze point on a plane.
        /// </summary>
        /// <param name="mainCamera"></param>
        /// <param name="gazeData"></param>
        /// <returns></returns>
        public static Vector3 GetGazeViewPortPoint(Camera mainCamera, IVRGazeData gazeData)
        {
            return mainCamera.WorldToViewportPoint(gazeData.CombinedGazeRayWorld.direction);
        }

        /// <summary>
        /// Returns the average diameter of the user's pupils in millimeters. Returns 0.0f if pupil data is invalid. Usually means the user in blinking.
        /// </summary>
        /// <param name="gazeData"></param>
        public static float GetAveragePupilDiameter(IVRGazeData gazeData)
        {
            //Initalize diameter to 0f, acts as default value if data is invalid.
            float averagePupilDiameter = 0.0f;
            //If the pupil data is valid, calculate the average pupil size.
            if (IsPupilDataValid(gazeData))
            {
                averagePupilDiameter = (gazeData.Right.PupilDiameter + gazeData.Left.PupilDiameter) / 2f * 1000f;
            }

            return averagePupilDiameter;
        }

        /// <summary>
        /// Returns true if the pupil data of the IVRGazeData object is valid, else false. False usually means the user was blinking.
        /// </summary>
        /// <param name="gazeData"></param>
        /// <returns>True if data is valid, else false.</returns>
        public static bool IsPupilDataValid(IVRGazeData gazeData)
        {
            if (gazeData.Right.PupilDiameterValid && gazeData.Right.PupilDiameterValid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Private methods
        #endregion
    }
}
