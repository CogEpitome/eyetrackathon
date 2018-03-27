/*========================================================================
Product:    #HMDEyeTracking#
Developer:  #Jonas Iacobi#
Company:    #KTH#
Date:       #2018-03-20#
========================================================================*/
/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace HMDEyeTracking
{
    [CustomEditor(typeof(SceneGraph))]
    public class SceneGraphEditor : Editor
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
        SceneGraph sceneGraph;
        #endregion

        #region Unity methods
        private void OnEnable()
        {
            if (GazeDataLoader.instance != null)
            {
                GazeDataLoader.instance.LoadGazeData();
            } else
            {
                Debug.Log("The SceneGraphEditor could not find a GazeDataLoader instance. Please ensure there is a GazeDataLoader in the scene, and that this script executes after GazeDataLoader.cs due to OnEnables execution order being impolite.");
            }
        }

        private void OnSceneGUI()
        {
            if (sceneGraph != (SceneGraph)target && target != null)
                sceneGraph = (SceneGraph)target;

            if (!sceneGraph.show)
            {
                return;
            }

            Handles.BeginGUI();
            GUILayout.BeginArea(sceneGraph.graphSize);

            Rect rect = EditorGUILayout.BeginVertical();
            GUI.color = Color.green;
            GUI.Box(rect, GUIContent.none);

            GUI.color = Color.white;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Eyetracking Grapher");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (GazeDataLoader.instance != null)
            {
                GUILayout.Label("Dataset size: " + GazeDataLoader.instance.GetGazeData().Count);
            } else
            {
                Debug.Log("The SceneGraphEditor could not find a GazeDataLoader instance. Please ensure there is a GazeDataLoader in the scene, and that this script executes after GazeDataLoader.cs due to OnEnables execution order being impolite.");
            }

            GUIContent graphImage = new GUIContent();
            GUIStyle graphStyle = new GUIStyle();
            graphStyle.stretchHeight = true;
            graphStyle.stretchWidth = true;
            graphImage.image = sceneGraph.GetGraphTexture();
            GUILayout.Box(graphImage.image, graphStyle);
            GUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
        #endregion

        #region Public methods
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        #endregion

        #region Private methods
        #endregion
    }
}*/