using System;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities.Async;
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
			var sprite = await previewBeatmapLevel.GetCoverImageAsync(CancellationToken.None);

			Color[] pixels = {};
			try
			{
				pixels = sprite.texture.GetPixels();
			}
			catch (Exception)
			{
				await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
				{
					if (previewBeatmapLevel is not CustomPreviewBeatmapLevel)
					{
						pixels = GetUnreadableTexture(sprite.texture, InvertImageAtlas(sprite.textureRect)).GetPixels();
					}
					else
					{
						pixels = GetUnreadableTexture(sprite.texture, sprite.textureRect).GetPixels();
					}
				});
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

			var averageColour = new Color(r / pixels.Length, g / pixels.Length, b / pixels.Length);
			Color.RGBToHSV(averageColour, out var h, out var s, out _);
			averageColour = Color.HSVToRGB(h, s, 1f);
			_menuPillarsManager.TweenToPillarLightColor(averageColour.ColorWithAlpha(_menuPillarsManager.CurrentColor.a), 0.2f);
		}

		private Rect InvertImageAtlas(Rect rect)
		{
			rect.y = 2048 - rect.y - 160;
			return rect;
		}
		
		private Texture2D GetUnreadableTexture(Texture2D texture, Rect rect)
		{
			var tempRenderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			Graphics.Blit(texture, tempRenderTexture);
			var previous = RenderTexture.active;
			RenderTexture.active = tempRenderTexture;
			var readableTexture = new Texture2D((int)rect.width, (int)rect.height);
			readableTexture.ReadPixels(rect, 0, 0);
			readableTexture.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(tempRenderTexture);

			return readableTexture;
		}
		
		private void LevelCollectionViewControllerOnDidDeactivateEvent(bool removedfromhierarchy, bool screensystemdisabling)
		{
			if (!_pluginConfig.EnableLights || !_pluginConfig.UseCoverColor)
			{
				return;
			}
			
			_menuPillarsManager.TweenToUserColors();
		}

		private void LevelCollectionViewControllerOnDidSelectLevelEvent(LevelCollectionViewController viewController, IPreviewBeatmapLevel previewBeatmapLevel)
		{
			if (!_pluginConfig.EnableLights || !_pluginConfig.UseCoverColor)
			{
				return;
			}
			
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