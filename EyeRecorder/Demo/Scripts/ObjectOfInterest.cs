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
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    public class ObjectOfInterest : MonoBehaviour, IInteractible, ILookable
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
        private Animator anim;
        #endregion

        #region Unity methods

        protected void Start()
        {
            anim = GetComponent<Animator>();
        }

        protected void Update()
        {

        }

        #endregion

        #region Public methods
        /// <summary>
        /// Called when this object is looked at.
        /// </summary>
        public void LookedAt()
        {
            anim.SetBool("play", true);
        }

        /// <summary>
        /// Called when this object is looked away from.
        /// </summary>
        public void LookedAwayFrom()
        {
            anim.SetBool("play", false);
        }

        /// <summary>
        /// Called when this object is clicked on.
        /// </summary>
        public void OnClick()
        {
            SelectObject();
        }

        /// <summary>
        /// Set this object as selected.
        /// </summary>
        public void Select()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
        #endregion

        #region Private methods

        //Called when the user has chosen this object and clicked it.
        private void SelectObject()
        {
            Debug.Log("Selected " + gameObject.name);
        }
        #endregion
    }
}