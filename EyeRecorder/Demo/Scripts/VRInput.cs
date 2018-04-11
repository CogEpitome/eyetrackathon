/*========================================================================
Product:    #PROJECTNAME#
Developer:  #DEVELOPERNAME#
Company:    #COMPANY#
Date:       #CREATIONDATE#
========================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace HMDDemo
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    public class VRInput : MonoBehaviour
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
        private SteamVR_LaserPointer laserPointer;
        private SteamVR_TrackedController trackedController;

        private Animator anim;
        #endregion

        #region Unity methods

        private void Start()
        {
            laserPointer = GetComponent<SteamVR_LaserPointer>();
            laserPointer.PointerIn -= HandlePointerIn;
            laserPointer.PointerIn += HandlePointerIn;
            laserPointer.PointerOut -= HandlePointerOut;
            laserPointer.PointerOut += HandlePointerOut;

            trackedController = GetComponent<SteamVR_TrackedController>();
            if(trackedController == null)
            {
                trackedController = GetComponentInParent<SteamVR_TrackedController>();
            }
            trackedController.TriggerClicked -= HandleTriggerClicked;
            trackedController.TriggerClicked += HandleTriggerClicked;
        }


        private void HandleTriggerClicked(object sender, ClickedEventArgs e)
        {
            if(EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.currentSelectedGameObject.GetComponent<IInteractible>().OnClick();
            }
        }

        private void HandlePointerIn(object sender, PointerEventArgs e)
        {
            IInteractible interactible = e.target.GetComponent<IInteractible>();
            if(interactible != null)
            {
                interactible.Select();
            }
        }

        private void HandlePointerOut(object sender, PointerEventArgs e)
        {
            IInteractible interactible = e.target.GetComponent<IInteractible>();
            if (interactible != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        #endregion

        #region Public methods
        #endregion

        #region Private methods

        #endregion
    }
}