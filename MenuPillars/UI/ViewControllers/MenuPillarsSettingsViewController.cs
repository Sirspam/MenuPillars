using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Loader;
using MenuPillars.Configuration;
using MenuPillars.Managers;
using SiraUtil.Logging;
using SiraUtil.Web.SiraSync;
using SiraUtil.Zenject;
using Tweening;
using UnityEngine;
using Zenject;
using BeatSaberMarkupLanguage.Components.Settings;
using IPA.Utilities;
using UnityEngine.UI;
using System;

namespace MenuPillars.UI.ViewControllers
{
	[HotReload(RelativePathToLayout = @"..\Views\MenuPillarsSettingsView")]
	[ViewDefinition("MenuPillars.UI.Views.MenuPillarsSettingsView.bsml")]
	internal class MenuPillarsSettingsViewController : BSMLAutomaticViewController
	{
		private bool _updateAvailable;
		private Button? _brightnessSliderIncButton;
		private int _brightnessSliderIncButtonPressedCount;

		[UIComponent("update-text")]
		private readonly CurvedTextMeshPro _updateText = null!;
		[UIComponent("version-text")] 
		private readonly CurvedTextMeshPro _versionText = null!;
		[UIComponent("slider-brightness")]
		private readonly SliderSetting _sliderBrightness = null!;

		private SiraLog _siraLog = null!;
		private PluginConfig _pluginConfig = null!;
		private PluginMetadata _pluginMetadata = null!;
		private TrollageManager _trollageManager = null!;
		private ISiraSyncService _siraSyncService = null!;
		private MenuPillarsManager _menuPillarsManager = null!;
		private TimeTweeningManager _timeTweeningManager = null!;
		private GitHubPageModalController _gitHubPageModalController = null!;

		[Inject]
		private void Construct(SiraLog siraLog, PluginConfig pluginConfig, UBinder<Plugin, PluginMetadata> pluginMetadata, TrollageManager trollageManager, ISiraSyncService siraSyncService, MenuPillarsManager menuPillarsManager, TimeTweeningManager timeTweeningManager, GitHubPageModalController gitHubPageModalController)
		{
			_siraLog = siraLog;
			_pluginConfig = pluginConfig;
			_pluginMetadata = pluginMetadata.Value;
			_trollageManager = trollageManager;
			_siraSyncService = siraSyncService;
			_menuPillarsManager = menuPillarsManager;
			_timeTweeningManager = timeTweeningManager;
			_gitHubPageModalController = gitHubPageModalController;
		}

		[UIValue("update-available")]
		private bool UpdateAvailable
		{
			get => _updateAvailable;
			set
			{
				_updateAvailable = value;
				NotifyPropertyChanged();
			}
		}
		
		[UIValue("enable-lights")]
		private bool EnableLights
		{
			get => _pluginConfig.EnableLights;
			set
			{
				_pluginConfig.EnableLights = value;
				_menuPillarsManager.ToggleRainbowColors(value && _pluginConfig.RainbowLights);
				NotifyPropertyChanged();
			}
		}
		
		[UIValue("lights-color")]
		private Color LightsColor
		{
			get => _pluginConfig.PillarLightsColor;
			set
			{
				_pluginConfig.PillarLightsColor = value;
				NotifyPropertyChanged();
			}
		}

		[UIValue("lights-brightness")]
		private float LightsBrightness
		{
			get => _pluginConfig.LightsBrightness;
			set
			{
				_pluginConfig.LightsBrightness = value;
				_menuPillarsManager.SetPillarLightBrightness(value);
				NotifyPropertyChanged();
			}
		}

		[UIValue("brightness-cap-raised")]
		private bool BrightnessCapRaised
		{
			get => _pluginConfig.BrightnessCapRaised;
			set
			{
				_pluginConfig.BrightnessCapRaised = value;
				NotifyPropertyChanged();
			}
		}

		[UIValue("brightness-cap")]
		private int BrightnessCap => !BrightnessCapRaised ? 10 : 250;
		
		[UIValue("rainbow-lights")]
		private bool RainbowLights
		{
			get => _pluginConfig.RainbowLights;
			set
			{
				_pluginConfig.RainbowLights = value;
				_menuPillarsManager.ToggleRainbowColors(value && _pluginConfig.EnableLights);
				NotifyPropertyChanged();
			}
		}

		[UIValue("rainbow-loop-speed")]
		private float RainbowLoopSpeed
		{
			get => _pluginConfig.RainbowLoopSpeed;
			set
			{
				_pluginConfig.RainbowLoopSpeed = value;
				if (_pluginConfig.RainbowLights && _pluginConfig.EnableLights)
				{
					_menuPillarsManager.ToggleRainbowColors(true);
				}
			}
		}

		[UIValue("easter-eggs")]
		private bool EasterEggs
		{
			get => _pluginConfig.EasterEggs;
			set
			{
				if (value != _pluginConfig.EasterEggs)
				{
					_pluginConfig.EasterEggs = value;
				
					switch (value)
					{
						case true:
							_trollageManager.Initialize();
							break;
						case false:
							_trollageManager.Dispose();
							break;
					}
				}
			}
		}

		[UIValue("version-text-value")]
		private string VersionText => $"{_pluginMetadata.Name} v{_pluginMetadata.HVersion} by {_pluginMetadata.Author}";

		[UIAction("#post-parse")]
		private async void PostParse()
		{
			_brightnessSliderIncButton = _sliderBrightness.GetField<Button, GenericSliderSetting>("incButton");
			_brightnessSliderIncButton.onClick.AddListener(LightBrightnessChanged);

			
			var gitVersion = await _siraSyncService.LatestVersion();
			if (gitVersion != null && gitVersion > _pluginMetadata.HVersion)
			{
				_siraLog.Info($"{nameof(MenuPillars)} v{gitVersion} is available on GitHub!");
				_updateText.text = $"{nameof(MenuPillars)} v{gitVersion} is available on GitHub!";
				_updateText.alpha = 0f;
				UpdateAvailable = true;
				_timeTweeningManager.AddTween(new FloatTween(0f, 1f, val => _updateText.alpha = val, 0.4f, EaseType.InCubic), this);
			}
		}

		[UIAction("lights-color-changed")]
		private void LightsColorChanged(Color value)
		{
			if (RainbowLights)
			{
				RainbowLights = false;
			}
			
			_menuPillarsManager.SetPillarLightColors(value);
		}

		[UIAction("lower-brightness-cap-clicked")]
		private void LowerBrightnessCapClicked()
		{
			ChangeBrightnessCap(false);
		}
		
		[UIAction("version-text-clicked")]
		private void VersionTextClicked()
		{
			if (_pluginMetadata.PluginHomeLink == null)
			{
				return;
			}
			
			_gitHubPageModalController.ShowModal(_versionText.transform, _pluginMetadata.Name,
				_pluginMetadata.PluginHomeLink!.ToString());
		}

		private void LightBrightnessChanged()
		{
			if (!BrightnessCapRaised && _sliderBrightness.slider.value.Equals(_sliderBrightness.slider.maxValue))
			{
				_brightnessSliderIncButtonPressedCount += 1;
				if (_brightnessSliderIncButtonPressedCount == 3)
				{
					_brightnessSliderIncButtonPressedCount = 0;
					ChangeBrightnessCap(true);
				}
			}
		}

		private void ChangeBrightnessCap(bool toggle)
		{
			switch (toggle)
			{
				case true:
				{
					BrightnessCapRaised = true;
					_sliderBrightness.slider.maxValue = BrightnessCap;
					_sliderBrightness.slider.value = 10;
					break;
				}
				case false:
				{
					BrightnessCapRaised = false;
					_sliderBrightness.slider.maxValue = BrightnessCap;
					LightsBrightness = BrightnessCap;
					break;
				}
			}
		}
		
		public void Dispose()
		{
			_brightnessSliderIncButton!.onClick.RemoveListener(LightBrightnessChanged);
		}
	}
}
