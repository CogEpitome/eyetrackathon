/*========================================================================
Product:    #PROJECTNAME#
Developer:  #DEVELOPERNAME#
Company:    #COMPANY#
Date:       #CREATIONDATE#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;

public class followme : MonoBehaviour 
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
    private bool go = false;
	#endregion

	#region Unity methods

	protected void Start () 
	{
		
	}
	
	protected void Update () 
	{
        if (Input.GetKeyDown(KeyCode.A))
        {
            go = true;
            if(HMDEyeTracking.GazeReplayer.instance != null)
                HMDEyeTracking.GazeReplayer.instance.ToggleReplay();
        }
        if(go)
        transform.position = new Vector3(transform.position.x+(1f*Time.deltaTime), transform.position.y, transform.position.z);
	}

	#endregion

	#region Public methods
	#endregion

	#region Private methods
	#endregion
}
