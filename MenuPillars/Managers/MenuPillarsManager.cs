using System;
using System.Collections.Generic;
using MenuPillars.Configuration;
using MenuPillars.Utils;
using Tweening;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace MenuPillars.Managers
{
	internal class MenuPillarsManager : IInitializable, IDisposable
	{
		private bool _instantiatedPillars;
		private GameObject? _menuPillars;
		private GameObject? _pillarFrontLeft;
		private GameObject? _pillarFrontRight;
		private GameObject? _pillarBackLeft;
		private GameObject? _pillarBackRight;
		private List<TubeBloomPrePassLight>? _pillarLights;

		private readonly PluginConfig _pluginConfig;
		private readonly PillarGrabber _pillarGrabber;
		private readonly TimeTweeningManager _timeTweeningManager;

		public MenuPillarsManager(PluginConfig pluginConfig, PillarGrabber pillarGrabber, TimeTweeningManager timeTweeningManager)
		{
			_pluginConfig = pluginConfig;
			_pillarGrabber = pillarGrabber;
			_timeTweeningManager = timeTweeningManager;
		}

		public void Initialize()
		{
			if (_pillarGrabber.completed)
			{
				InstantiatePillars();
				return;
			}

			_pillarGrabber.CompletedEvent += InstantiatePillars;
		}

		public void Dispose()
		{
			_pillarGrabber.CompletedEvent -= InstantiatePillars;
		}

		private void InstantiatePillars()
		{
			_pillarGrabber.CompletedEvent -= InstantiatePillars;
			
			_menuPillars = new GameObject
			{
				name = "MenuPillars"
			};

			_pillarFrontLeft = Object.Instantiate(PillarGrabber.TemplatePillarLeft, new Vector3(-30f, 15f, 20f), Quaternion.Euler(new Vector3(45f, 0f)), _menuPillars.transform);
			_pillarFrontLeft!.name = "PillarFrontLeft";
			_pillarFrontRight = Object.Instantiate(PillarGrabber.TemplatePillarRight, new Vector3(30f, 15f, 20f), Quaternion.Euler(new Vector3(45f, 0f)), _menuPillars.transform);
			_pillarFrontRight!.name = "PillarFrontRight";
			_pillarBackLeft = Object.Instantiate(PillarGrabber.TemplatePillarLeft, new Vector3(-20f, 12f, -40f), Quaternion.Euler(new Vector3(45f, 270f)), _menuPillars.transform);
			_pillarBackLeft!.name = "PillarBackLeft";
			_pillarBackRight = Object.Instantiate(PillarGrabber.TemplatePillarRight, new Vector3(20f, 12f, -40f), Quaternion.Euler(new Vector3(45f, 90f)), _menuPillars.transform);
			_pillarBackRight!.name = "PillarBackRight";
			_instantiatedPillars = true;

			ToggleRainbowColors(_pluginConfig.EnableLights && _pluginConfig.RainbowLights, _pluginConfig.RainbowLoopSpeed);
			SetPillarLightBrightness(_pluginConfig.LightsBrightness);
		}

		private List<TubeBloomPrePassLight> GetLights()
		{
			if (!_instantiatedPillars)
			{
				InstantiatePillars();
			}
			
			if (_pillarLights == null)
			{
				_pillarLights = new List<TubeBloomPrePassLight>();
				_pillarLights.AddRange(_menuPillars!.GetComponentsInChildren<TubeBloomPrePassLight>());
			}

			return _pillarLights;
		}
		
		public void SetPillarLightColors(Color color)
		{
			foreach (var light in GetLights())
			{
				light.color = color;
			}
		}

		public void SetPillarLightBrightness(float brightness)
		{
			foreach (var light in GetLights())
			{
				light.bloomFogIntensityMultiplier = brightness;
			}
		}

		public void ToggleRainbowColors(bool toggle, float duration)
		{
			_timeTweeningManager.KillAllTweens(this);
			if (!toggle)
			{
				if (!_pluginConfig.EnableLights)
				{
					SetPillarLightColors(Color.clear);
					return;
				}
				
				SetPillarLightColors(_pluginConfig.PillarLightsColor);
				return;
			}

			var tween = new FloatTween(0f, 1f, val => SetPillarLightColors(Color.HSVToRGB(val, 1f, 1f)), duration, EaseType.Linear)
			{
				loop = true
			};
			_timeTweeningManager.AddTween(tween, this);
		}
	}
}