using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
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

		private SceneInstance sceneInstance;

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
				var loadScene = Addressables.LoadSceneAsync("BigMirrorEnvironment", LoadSceneMode.Additive);
				yield return new WaitUntil(() => loadScene.IsDone);
				sceneIsLoaded = true;
				sceneInstance = loadScene.Result;
				
				var environmentObject = sceneInstance.Scene
					.GetRootGameObjects()
					.First(go => go.name == "Environment");
				TemplatePillarLeft = environmentObject.transform.Find("NearBuildingLeft").gameObject;
				TemplatePillarRight = environmentObject.transform.Find("NearBuildingRight").gameObject;
				_ = TemplatePillarLeft.transform.Find("Mesh").GetComponent<MeshRenderer>().material;
				_ = TemplatePillarRight.transform.Find("Mesh").GetComponent<MeshRenderer>().material;
			}
			finally
			{
				if (sceneIsLoaded)
				{
					Addressables.UnloadSceneAsync(sceneInstance);
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