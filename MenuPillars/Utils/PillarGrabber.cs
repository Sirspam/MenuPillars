﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuPillars.Utils
{
	// Based off of UITweaks' object grabber
	// https://github.com/Exomanz/UITweaks/blob/sira3/UITweaks/Utilities/SettingsPanelObjectGrabber.cs
	public class PillarGrabber : MonoBehaviour
	{

		public Action? CompletedEvent;
		public bool completed;

		public static GameObject? TemplatePillarLeft;
		public static GameObject? TemplatePillarRight;
		
		public void Start()
		{
			StartCoroutine(GetPillars());
		}

		private IEnumerator GetPillars()
		{
            var sceneIsLoaded = false;
            try
            {
	            if (TemplatePillarLeft != null || TemplatePillarRight != null)
	            {
		            yield return null;
	            }
	            
	            var loadScene = SceneManager.LoadSceneAsync("BigMirrorEnvironment", LoadSceneMode.Additive);
                while (!loadScene.isDone) yield return null;

                sceneIsLoaded = true;
                yield return new WaitForSecondsRealtime(0.1f); // Allow objects to fully load
                
	                foreach (var gamerObject in Resources.FindObjectsOfTypeAll<GameObject>()) // I love performance
	                {
		                switch (gamerObject.name)
		                {
			                case "NearBuildingLeft":
				                TemplatePillarLeft = gamerObject;
				                break;
			                case "NearBuildingRight":
				                TemplatePillarRight = gamerObject;
				                break;
		                }
	                }
            }
            finally
            {
	            if (sceneIsLoaded)
	            {
		            SceneManager.UnloadSceneAsync("BigMirrorEnvironment");
	            }

	            completed = true;
	            CompletedEvent?.Invoke();
            }
		}
	}
}