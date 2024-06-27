using System;
using System.Collections.Generic;
using MenuPillars.Configuration;
using MenuPillars.Utils;
using ModestTree;
using Tweening;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace MenuPillars.Managers
{
	internal sealed class MenuPillarsManager : IInitializable, ILateTickable, IDisposable
	{
		private Color _currentColor;
		private bool _needColourUpdate;
		private ColorTween? _colourTween;
		private GameObject? _menuPillars;
		private FloatTween? _rainbowTween;
		private bool _instantiatedPillars;
		private GameObject? _pillarFrontLeft;
		private GameObject? _pillarFrontRight;
		private GameObject? _pillarBackLeft;
		private GameObject? _pillarBackRight;
		private List<TubeBloomPrePassLight>? _pillarLights;

        private List<TubeBloomPrePassLight> Lights => _pillarLights ??= [.. _menuPillars!.GetComponentsInChildren<TubeBloomPrePassLight>()];

        public Color CurrentColor
		{
			get => _currentColor;
			set
			{
				_currentColor = value;
				_needColourUpdate = true;
			}
			
		}

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

		public void LateTick()
		{
			if (_needColourUpdate && _instantiatedPillars)
			{
				foreach (var light in Lights)
				{
					light.color = CurrentColor;
				}

				_needColourUpdate = false;	
			}
		}
		
		public void Dispose() => _pillarGrabber.CompletedEvent -= InstantiatePillars;

		public void TweenToUserColors()
		{
			if (_colourTween is not null && _colourTween.isActive)
			{
				return;
			}
			
			if (_rainbowTween is not null && _rainbowTween.isActive)
			{
				_timeTweeningManager.AddTween(new FloatTween(CurrentColor.a, 1f, val => CurrentColor = CurrentColor.ColorWithAlpha(val), 0.2f, EaseType.Linear), this);
				return;
			}
			
			if (_pluginConfig.RainbowLights)
			{
				TweenToPillarLightColor(Color.red, callback: () => ToggleRainbowColors(true));
			}
			else
			{
				TweenToPillarLightColor(_pluginConfig.PillarLightsColor);	
			}
		}

		public void TweenToPillarLightColor(Color newColor, float duration = 0.5f,  Action? callback = null)
		{
			_colourTween?.Kill();
			_colourTween = new ColorTween(CurrentColor, newColor, val => CurrentColor = val, duration, EaseType.Linear);
			if (callback is not null)
			{
				_colourTween.onCompleted = callback.Invoke;
			}
			_timeTweeningManager.AddTween(_colourTween, this);
		}

		public void SetPillarLightBrightness(float brightness)
		{
			if (!_instantiatedPillars)
			{
				return;
			}
			
			foreach (var light in Lights)
			{
				light.bloomFogIntensityMultiplier = brightness;
			}
		}

        public void ToggleRainbowColors(bool toggle)
        {
            if (!_instantiatedPillars)
            {
                return;
            }

            _rainbowTween?.Kill();

            CurrentColor = _pluginConfig.EnableLights
                ? _pluginConfig.PillarLightsColor
                : Color.clear;

            if (toggle)
            {
                _rainbowTween = new FloatTween(0f, 1f, val => CurrentColor = Color.HSVToRGB(val, 1f, 1f).ColorWithAlpha(CurrentColor.a), _pluginConfig.RainbowLoopSpeed, EaseType.Linear)
                {
                    loop = true
                };
                _timeTweeningManager.AddTween(_rainbowTween, this);
            }
        }

        private void InstantiatePillars()
        {
            _pillarGrabber.CompletedEvent -= InstantiatePillars;

            _menuPillars = new GameObject
            {
                name = "MenuPillars"
            };
            _menuPillars.transform.SetParent(GameObject.Find("DefaultMenuEnvironment").transform);

            _pillarFrontLeft = Object.Instantiate(PillarGrabber.TemplatePillarLeft, new Vector3(-30f, 15f, 20f), Quaternion.Euler(new Vector3(45f, 0f)), _menuPillars.transform);
            _pillarFrontLeft!.name = "PillarFrontLeft";
            _pillarFrontRight = Object.Instantiate(PillarGrabber.TemplatePillarRight, new Vector3(30f, 15f, 20f), Quaternion.Euler(new Vector3(45f, 0f)), _menuPillars.transform);
            _pillarFrontRight!.name = "PillarFrontRight";
            _pillarBackLeft = Object.Instantiate(PillarGrabber.TemplatePillarLeft, new Vector3(-20f, 12f, -40f), Quaternion.Euler(new Vector3(45f, 270f)), _menuPillars.transform);
            _pillarBackLeft!.name = "PillarBackLeft";
            _pillarBackRight = Object.Instantiate(PillarGrabber.TemplatePillarRight, new Vector3(20f, 12f, -40f), Quaternion.Euler(new Vector3(45f, 90f)), _menuPillars.transform);
            _pillarBackRight!.name = "PillarBackRight";
            CurrentColor = _pluginConfig.PillarLightsColor;
            _instantiatedPillars = true;

            ToggleRainbowColors(_pluginConfig.EnableLights && _pluginConfig.RainbowLights);
            SetPillarLightBrightness(_pluginConfig.LightsBrightness);
        }
    }
}
