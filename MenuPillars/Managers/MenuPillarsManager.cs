using System;
using System.Collections.Generic;
using MenuPillars.Configuration;
using MenuPillars.Utils;
using SiraUtil.Logging;
using Tweening;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace MenuPillars.Managers
{
	internal sealed class MenuPillarsManager : IInitializable, ILateTickable, IDisposable
	{
		private Color _currentColor;
		private bool _needColorUpdate;
		private ColorTween? _colorTween;
		private FloatTween? _floatTween;
		private GameObject? _menuPillars;
		private FloatTween? _rainbowTween;
		private bool _instantiatedPillars;
		private GameObject? _pillarFrontLeft;
		private GameObject? _pillarFrontRight;
		private GameObject? _pillarBackLeft;
		private GameObject? _pillarBackRight;
		private List<TubeBloomPrePassLight>? _pillarLights;
		private Transform? _menuEnvironmentTransform;

        private List<TubeBloomPrePassLight> Lights => _pillarLights ??= [.. _menuPillars!.GetComponentsInChildren<TubeBloomPrePassLight>()];
		
		public event Action<Color>? CurrentColorChanged; 

        public Color CurrentColor
		{
			get => _currentColor;
			set
			{
				_currentColor = value;
				_needColorUpdate = true;
				CurrentColorChanged?.Invoke(CurrentColor);
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
			if (_menuEnvironmentTransform == null)
			{
				var menuEnvironmentManager = GameObject.Find("Wrapper/MenuEnvironmentManager").transform;
				for (int i = 0; i < menuEnvironmentManager.childCount; i++)
				{
					var child = menuEnvironmentManager.GetChild(i);
					if (child.gameObject.activeInHierarchy && child.name.Contains("MenuEnvironment"))
					{
						_menuEnvironmentTransform = child;
						break;
					}
				}
			}
			
			if (_pillarGrabber.completed)
			{
				InstantiatePillars();
				return;
			}
			
			_pillarGrabber.CompletedEvent += InstantiatePillars;
		}

		public void LateTick()
		{
			if (_needColorUpdate && _instantiatedPillars)
			{
				foreach (var light in Lights)
				{
					light.color = CurrentColor;
				}

				_needColorUpdate = false;	
			}
		}
		
		public void Dispose() => _pillarGrabber.CompletedEvent -= InstantiatePillars;

		public void TweenToUserColors()
		{
			if (_colorTween is not null && _colorTween.isActive || _rainbowTween is not null && _rainbowTween.isActive)
			{
				return;
			}
			
			if (_pluginConfig.RainbowLights)
			{
				ToggleRainbowColors(true);
			}
			else
			{
				TweenToPillarLightColor(_pluginConfig.PillarLightsColor);	
			}
		}

		public void TweenToPillarLightColor(Color newColor, float duration = 0.5f,  Action? callback = null)
		{
			_rainbowTween?.Kill();
			_colorTween?.Kill();
			_colorTween = new ColorTween(CurrentColor, newColor, val => CurrentColor = val, duration, EaseType.Linear);
			if (callback is not null)
			{
				_colorTween.onCompleted = callback.Invoke;
			}
			_timeTweeningManager.AddTween(_colorTween, this);
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

		public void ToggleRainbowColors(bool toggle) => ToggleRainbowColors(toggle, _pluginConfig.RainbowLoopSpeed);

		public void ToggleRainbowColors(bool toggle, float duration)
		{
			if (!_instantiatedPillars)
			{
				return;
			}
			
			_timeTweeningManager.KillAllTweens(this);
			
			if (!toggle)
			{
				if (!_pluginConfig.EnableLights)
				{
					TweenToPillarLightColor(Color.clear);
					return;
				}
				
				TweenToPillarLightColor(_pluginConfig.PillarLightsColor);
				return;
			}
			
			// Tweens the alpha to max
			_timeTweeningManager.AddTween(new FloatTween(CurrentColor.a, 1f, val => CurrentColor = CurrentColor.ColorWithAlpha(val), 0.5f, EaseType.Linear), this);
			
			Color.RGBToHSV(CurrentColor, out var startHue, out var startSat, out var startVal);
			// Tweens the color's saturation and value up to 1f while the rainbow tween is going, makes the transition from a custom oolor to the rainbow loop seamless
			_floatTween = new FloatTween(Mathf.Min(startSat, startVal), 1f, updateVal =>
			{
				Color.RGBToHSV(CurrentColor, out var hue, out var sat, out var val);
				
				if (updateVal > sat)
				{
					sat = updateVal;
				}
				
				if (updateVal > val)
				{
					val = updateVal;
				}
				
				CurrentColor = Color.HSVToRGB(hue, sat, val).ColorWithAlpha(CurrentColor.a);
			}, 0.5f, EaseType.Linear);
			_timeTweeningManager.AddTween(_floatTween, this);
			
			
			// This first tweens the current color's hue up to 1f then starts a second tween for the rainbow loop.
			_rainbowTween = new FloatTween(startHue, 1f, updateVal =>
			{
				Color.RGBToHSV(CurrentColor, out _, out var sat, out var val);
				CurrentColor = Color.HSVToRGB(updateVal, sat, val).ColorWithAlpha(CurrentColor.a);
			}, duration * (1 - startHue), EaseType.Linear)
			{
				onCompleted = () =>
				{
					if (_rainbowTween is null || _rainbowTween.isKilled)
					{
						return;
					}
					
					_rainbowTween = new FloatTween(0f, 1f, val =>
					{
						Color.RGBToHSV(CurrentColor, out _, out var s, out var v);
						CurrentColor = Color.HSVToRGB(val, s, v).ColorWithAlpha(CurrentColor.a);
					}, duration, EaseType.Linear)
					{
						loop = true
					};
					_timeTweeningManager.AddTween(_rainbowTween, this);
				}
			};
			_timeTweeningManager.AddTween(_rainbowTween, this);
		}
		
		public void KillAllTweens() => _timeTweeningManager.KillAllTweens(this);
		
		private void InstantiatePillars()
		{
			_pillarGrabber.CompletedEvent -= InstantiatePillars;

			if (_menuEnvironmentTransform == null)
			{
				return;
			}
            
			_menuPillars = new GameObject
			{
				name = "MenuPillars"
			};
			_menuPillars.transform.SetParent(_menuEnvironmentTransform);

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
