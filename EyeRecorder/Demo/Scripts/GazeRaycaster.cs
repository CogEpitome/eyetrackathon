/*========================================================================
Product:    #HMDGazeAnalyzer
Developer:  #Jonas Iacobi
Company:    #KTH | SVRVIVE Studios
Date:       #2018-04-011
========================================================================*/

using System.Collections.Generic;
using UnityEngine;

namespace HMDDemo
{
    public class GazeRaycaster : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        public static GazeRaycaster instance;

        [Tooltip("The camera representing the user's eyes.")]
        public Camera mainCamera;
        #endregion

        #region Private fields
        private ILookable lookingAt;
        #endregion

        #region Unity methods
        private void Awake()
        {
            //Singleton
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            //Check that the camera has been assigned.
            if (mainCamera == null)
            {
                Debug.Log("Attention: No camera has been assigned to the recorder. Disabling.");
                enabled = false;
            }
        }
        #endregion

        #region Public methods
        //Used specifically to cast a ray onto an ILookable object.
        public void Raycast(Ray ray)
        {
            bool lookingAtLookable = false;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                ILookable lookedAt = hit.collider.GetComponent<ILookable>() as ILookable;
                if (lookedAt != null)
                {
                    lookingAt = lookedAt;
                    lookingAt.LookedAt();
                    lookingAtLookable = true;
                }
            }

            if (!lookingAtLookable)
            {
                if(lookingAt != null)
                {
                    lookingAt.LookedAwayFrom();
                }
            }
        }
        #endregion

        #region Private methods
        #endregion
    }
}