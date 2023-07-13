using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuPillars.Utils
{
	// Based off of UITweaks' object grabber
	// https://github.com/Exomanz/UITweaks/blob/sira3/UITweaks/Utilities/SettingsPanelObjectGrabber.cs
	public class PillarGrabber : MonoBehaviour
	{
		public bool completed;
		public static GameObject? TemplatePillarLeft;
		public static GameObject? TemplatePillarRight;

		public Action? CompletedEvent;

		public void Start()
		{
			StartCoroutine(GetPillars());
		}

		private IEnumerator GetPillars()
		{
			if (completed)
			{
				yield break;
			}
			
			var sceneIsLoaded = false;
			try
			{
				var loadScene = SceneManager.LoadSceneAsync("BigMirrorEnvironment", LoadSceneMode.Additive);
				yield return new WaitUntil(() => loadScene.isDone);

				sceneIsLoaded = true;
				GameObject[] gameObjects = { };
				yield return new WaitUntil(() => (gameObjects = SceneManager.GetSceneByName("BigMirrorEnvironment").GetRootGameObjects()) != null);
				foreach (var go in gameObjects)
				{
					if (go.name == "Environment")
					{
						TemplatePillarLeft = go.transform.Find("NearBuildingLeft").gameObject;
						TemplatePillarRight = go.transform.Find("NearBuildingRight").gameObject;
						
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

				if (TemplatePillarLeft != null && TemplatePillarRight != null)
				{
					completed = true;
					CompletedEvent?.Invoke();	
				}
			}
		}
	}
}