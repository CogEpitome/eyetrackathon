/*========================================================================
Product:    #HMDEyeTracking#
Developer:  #Jonas Iacobi#
Company:    #KTH#
Date:       #2018-03-20#
========================================================================*/
/*
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace HMDEyeTracking
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Rect))]
    public class SceneGraph : MonoBehaviour
    {
        #region Constants
        //The name of the directory in which data from the recorder will be stored.
        private const string GRAPH_FILE_DIRECTORY = "EyeRecorderData";
        //The name of the XML file in which data from the recorder will be stored.
        private const string GRAPH_FILE_NAME = "graph.jpg";
        #endregion

        #region Classes, Structs and Enumerations
        #endregion

        #region Events and Delegates
        #endregion

        #region Public fields
        [Tooltip("Check to show this graph in the editor, uncheck to hide.")]
        public bool show;
        [Tooltip("Set the size of the graph, as shown in the scene.")]
        public Rect graphSize;
        #endregion

        #region Private fields
        //[HideInInspector]
        private Texture2D graphTexture;
        #endregion

        #region Unity methods
        private void Awake()
        {
            graphTexture = new Texture2D(2, 2);
            byte[] imageData = File.ReadAllBytes(Path.Combine(GRAPH_FILE_DIRECTORY, GRAPH_FILE_NAME));
            graphTexture.LoadImage(imageData);
        }

        #endregion

        #region Public methods
        public Texture2D GetGraphTexture()
        {
            return graphTexture;
        }
        #endregion

    }

#if UNITY_EDITOR
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
            }
            else
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
            }
            else
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
    }
#endif
}
*/