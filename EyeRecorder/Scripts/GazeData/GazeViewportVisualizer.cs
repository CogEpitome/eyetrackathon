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
    public class GazeViewportVisualizer : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        [Tooltip("The color of the gaze point when valid.")]
        public Color pointColorValid;
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
        //Whether to run or not.
        private bool enabled;

        private Canvas canvas;
        #endregion

        #region Unity methods

        protected void Start()
        {
            pointImage = point.GetComponent<Image>();
            canvas = GetComponentInChildren<Canvas>();
        }

        protected void Update()
        {

        }

        #endregion

        #region Public methods
        public void UpdateViewportPoint(Utils.GazeData gazeData)
        {
            if(gazeData == null)
            {
                return;
            }

            if (gazeData.valid && canvas != null)
            {
                pointPosition = new Vector3((gazeData.point.x - 0.5f) * canvas.GetComponent<RectTransform>().rect.width, -0.5f * Screen.height + gazeData.point.y * canvas.GetComponent<RectTransform>().rect.height);
            }

            if (gazeData.pupilsValid)
            {
                pupilSize = gazeData.pupilsSize;
                pointImage.color = pointColorValid;
            } else
            {
                pointImage.color = pointColorBlink;
            }

            point.localPosition = new Vector3(pointPosition.x, pointPosition.y, 0f);
        }
        #endregion

        #region Private methods
        #endregion
    }
}