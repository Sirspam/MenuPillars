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
			var sceneIsLoaded = false;
			try
			{
				if (completed)
				{
					yield break;
				}

				var loadScene = SceneManager.LoadSceneAsync("BigMirrorEnvironment", LoadSceneMode.Additive);
				yield return new WaitUntil(() => loadScene.isDone);

				sceneIsLoaded = true;
				yield return new WaitForSecondsRealtime(0.1f); // Allow objects to fully load

				foreach (var gamerObject in Resources.FindObjectsOfTypeAll<BloomFogEnvironment>())
				{
					if (gamerObject.name == "Environment")
					{
						TemplatePillarLeft = gamerObject.transform.Find("NearBuildingLeft").gameObject;
						TemplatePillarRight = gamerObject.transform.Find("NearBuildingRight").gameObject;
						
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