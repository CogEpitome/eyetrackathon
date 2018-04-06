/*========================================================================
Product:    #PROJECTNAME#
Developer:  #DEVELOPERNAME#
Company:    #COMPANY#
Date:       #CREATIONDATE#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HMDEyeTracking
{
    [ExecuteInEditMode]
    public class GazePlaneVisualizer : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        //[Tooltip("The camera representing the user's eyes. Corresponds to [CameraRig]->Camera(head)->Camera(eye) in the SteamVR prefab.")]
        //public Camera mainCamera;
        [Tooltip("The color of the gaze point when agitated.")]
        public Color pointColorRed;
        [Tooltip("The color of the gaze point when calm.")]
        public Color pointColorGreen;
        [Tooltip("The color of the gaze point when position data is invalid.")]
        public Color pointColorBlink;
        [Tooltip("The transform representing the gaze point.")]
        public Transform point;
        [Tooltip("The size of the gaze point.")]
        [Range(0.01f, 1f)]
        public float pupilScale = 0.1f;
        [Tooltip("Modify to scale the magnitude of the gaze point plane representation.")]
        [Range(0.5f, 3f)]
        public float gazeMagnitude = 1.5f;
        #endregion

        #region Private fields
        //The image representing the gaze point.
        private Image pointImage;
        //The last known position of the gaze point.
        private Vector2 pointPosition;
        //The last measured pupil size.
        private float pupilSize;
        //The blend factor between the point colors. 0 = pointcolorGreen and 1 = pointcolorRed.
        private float blendFactor;
        #endregion

        #region Unity methods
        private void OnEnable()
        {
            if(point != null && pointImage == null)
            {
                pointImage = point.GetComponent<Image>();
            }
            //Set default values
            pupilSize = 1f;
            pointPosition = Vector2.zero;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Set the position of the Plane Visualizer's gaze point based on a GazeData object.
        /// </summary>
        /// <param name="gazeData"></param>
        public void UpdatePlaneGazePoint(Utils.GazeData gazeData)
        {
            if(gazeData == null)
            {
                return;
            }
            
            if (gazeData.valid)
            {
                pointPosition = gazeData.point;
                pointImage.color = SetPointColor();
            } else
            {
                pointImage.color = pointColorBlink;
            }

            if (gazeData.pupilsValid)
            {
                pupilSize = gazeData.pupilsSize;
            }

            SetPlaneGazePoint();
            //point.localPosition = new Vector3(0.5f,0.5f, 0f);
        }
        #endregion

        #region Private methods
        //Set the size and position of the gaze point based on the local variables. To update these variables, see UpdatePlaneGazePoint.
        private void SetPlaneGazePoint()
        {
            //If there's a point, set its transform according to the local variables.
            if (point != null)
            {
                //Adjust the origin point so that (0,0) corresponds to the lower left corner of the plane.
                if (!float.IsNaN(pointPosition.x) && !float.IsNaN(pointPosition.y))
                {
                    point.localPosition = new Vector3(gazeMagnitude * (pointPosition.x-0.5f), gazeMagnitude*pointPosition.y, 0f);
                }
                Vector3 newPupilSize = Vector3.one * pupilScale * pupilSize;
                if (!float.IsNaN(newPupilSize.x))
                    point.localScale = newPupilSize;
            }
        }

        //TODO: Sets the poin color based on factors.
        private Color SetPointColor()
        {
            blendFactor = 1f;
            return Color.Lerp(pointColorGreen, pointColorRed, blendFactor);
        }
        #endregion
    }
}