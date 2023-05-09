using System;
using MenuPillars.Configuration;
using SiraUtil.Logging;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class TomfooleryManager : IInitializable, IDisposable
	{
		private bool _activeOnPreviousLevel;
		
		private readonly SiraLog _siraLog;
		private readonly PluginConfig _pluginConfig;
		private readonly MenuPillarsManager _menuPillarsManager;
		private readonly LevelCollectionViewController _levelCollectionViewController;

		public TomfooleryManager(SiraLog siraLog, PluginConfig pluginConfig, MenuPillarsManager menuPillarsManager, LevelCollectionViewController levelCollectionViewController)
		{
			_siraLog = siraLog;
			_pluginConfig = pluginConfig;
			_menuPillarsManager = menuPillarsManager;
			_levelCollectionViewController = levelCollectionViewController;
		}

		public void Initialize()
		{
			if (_pluginConfig.EasterEggs)
			{
				_levelCollectionViewController.didSelectLevelEvent += LevelCollectionViewControllerOndidSelectLevelEvent;
			}
		}

		public void Dispose()
		{
			_levelCollectionViewController.didDeactivateEvent -= LevelCollectionViewControllerOndidDeactivateEvent;
			_levelCollectionViewController.didSelectLevelEvent -= LevelCollectionViewControllerOndidSelectLevelEvent;
		}

		private void LevelCollectionViewControllerOndidSelectLevelEvent(LevelCollectionViewController viewController, IPreviewBeatmapLevel previewBeatmapLevel)
		{
			if (_pluginConfig.EasterEggs && (previewBeatmapLevel.songName.ToLower().Contains("caramelldansen") || previewBeatmapLevel.songSubName.ToLower().Contains("caramelldansen")))
			{
				_siraLog.Info("o-o-woa-woah-oh"); // Yea, this should be a pretty clear log as to what's happening
				
				_activeOnPreviousLevel = true;
				_menuPillarsManager.SetPillarLightBrightness(15f);
				_menuPillarsManager.ToggleRainbowColors(true, 0.75f);
				_menuPillarsManager.TogglePillarDance(true);
				
				_levelCollectionViewController.didDeactivateEvent += LevelCollectionViewControllerOndidDeactivateEvent;
				return;
			}

			if (_activeOnPreviousLevel)
			{
				RevertCaramelldansen();
			}
		}

		private void RevertCaramelldansen()
		{
			_activeOnPreviousLevel = false;
			_menuPillarsManager.SetPillarLightBrightness(_pluginConfig.LightsBrightness);
			_menuPillarsManager.ToggleRainbowColors(_pluginConfig.EnableLights && _pluginConfig.RainbowLights);
			_menuPillarsManager.TogglePillarDance(false);
			
			_levelCollectionViewController.didDeactivateEvent -= LevelCollectionViewControllerOndidDeactivateEvent;
		}

		private void LevelCollectionViewControllerOndidDeactivateEvent(bool removedfromhierarchy, bool screensystemdisabling)
		{
			RevertCaramelldansen();
			_levelCollectionViewController.didDeactivateEvent -= LevelCollectionViewControllerOndidDeactivateEvent;
		}
	}
}