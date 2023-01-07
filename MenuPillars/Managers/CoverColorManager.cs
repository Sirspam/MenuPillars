using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MenuPillars.Configuration;
using UnityEngine;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class CoverColorManager : IInitializable, IDisposable
	{
		private readonly PluginConfig _pluginConfig;
		private readonly MenuPillarsManager _menuPillarsManager;
		private readonly LevelCollectionViewController _levelCollectionViewController;

		public CoverColorManager(PluginConfig pluginConfig, MenuPillarsManager menuPillarsManager, LevelCollectionViewController levelCollectionViewController)
		{
			_pluginConfig = pluginConfig;
			_menuPillarsManager = menuPillarsManager;
			_levelCollectionViewController = levelCollectionViewController;
		}

		private async Task GetAverageCoverColorAsync(IPreviewBeatmapLevel previewBeatmapLevel)
		{
			if (!_pluginConfig.UseCoverColor && !_pluginConfig.EnableLights)
			{
				return;
			}
			
			var sprite = await previewBeatmapLevel.GetCoverImageAsync(CancellationToken.None);

			Color[] pixels;
			
			
			try
			{
				pixels = sprite.texture.GetPixels();
			}
			catch
			{
				// Sprite texture not readable on base game maps
				// Had some attempts with wacky shenanigans to get the pixels but nothing worked :(
				return;
			}

			var r = 0f;
			var g = 0f;
			var b = 0f;
			
			foreach (var pixel in pixels)
			{
				r += pixel.r;
				g += pixel.g;
				b += pixel.b;
			}

			if (_pluginConfig.VisualizeAudio)
			{
				_menuPillarsManager.SetPillarLightColors(new Color(r / pixels.Length, g / pixels.Length, b / pixels.Length, _menuPillarsManager.CurrentColor.a));
			}
			else
			{
				_menuPillarsManager.TweenToPillarLightColor(new Color(r / pixels.Length, g / pixels.Length, b / pixels.Length));	
			}
			
			var averageColour = new Color(r / pixels.Length, g / pixels.Length, b / pixels.Length);
			Color.RGBToHSV(averageColour, out var h, out var s, out _);
			averageColour = Color.HSVToRGB(h, s, 1f);
		}
		
		private void LevelCollectionViewControllerOnDidDeactivateEvent(bool removedfromhierarchy, bool screensystemdisabling)
		{
			// This is already handled by the audio visualizer manager
			// This logic really should be handled in the tween to user colors method but I am unfortunately lazy
			// It shall be future me's problem :)
			if (!_pluginConfig.VisualizeAudio && _pluginConfig.EnableLights)
			{
				_menuPillarsManager.TweenToUserColors();	
			}
		}

		private void LevelCollectionViewControllerOnDidSelectLevelEvent(LevelCollectionViewController viewController, IPreviewBeatmapLevel previewBeatmapLevel)
		{
			Task.Run(() => GetAverageCoverColorAsync(previewBeatmapLevel));
		}

		public void Initialize()
		{
			_levelCollectionViewController.didDeactivateEvent += LevelCollectionViewControllerOnDidDeactivateEvent;
			_levelCollectionViewController.didSelectLevelEvent += LevelCollectionViewControllerOnDidSelectLevelEvent;
		}

		public void Dispose()
		{
			_levelCollectionViewController.didDeactivateEvent -= LevelCollectionViewControllerOnDidDeactivateEvent;
			_levelCollectionViewController.didSelectLevelEvent -= LevelCollectionViewControllerOnDidSelectLevelEvent;
		}
	}
}