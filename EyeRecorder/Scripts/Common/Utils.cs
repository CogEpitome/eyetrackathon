/*========================================================================
Product:    #PROJECTNAME#
Developer:  #DEVELOPERNAME#
Company:    #COMPANY#
Date:       #CREATIONDATE#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;

namespace HMDGazeAnalyzing
{
    public static class Utils
    {
        #region Constants
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        #endregion

        #region Private fields
        #endregion

        #region Public methods
        /// <summary>
        /// Returns a point on the viewport corrsponding to the supplied camera and direction vector.
        /// </summary>
        /// <param name="mainCamera"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector3 GazeToViewportPoint(Camera mainCamera, Vector3 direction)
        {
            return mainCamera.WorldToViewportPoint(direction);
        }
        #endregion

        #region Private methods
        #endregion
    }
}