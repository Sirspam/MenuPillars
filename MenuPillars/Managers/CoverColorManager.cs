using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities.Async;
using MenuPillars.Configuration;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class CoverColorManager : IInitializable, IDisposable
	{
		private readonly SiraLog _siraLog;
		private readonly PluginConfig _pluginConfig;
		private readonly MenuPillarsManager _menuPillarsManager;
		private readonly LevelCollectionViewController _levelCollectionViewController;

		public CoverColorManager(SiraLog siraLog, PluginConfig pluginConfig, MenuPillarsManager menuPillarsManager, LevelCollectionViewController levelCollectionViewController)
		{
			_siraLog = siraLog;
			_pluginConfig = pluginConfig;
			_menuPillarsManager = menuPillarsManager;
			_levelCollectionViewController = levelCollectionViewController;
		}

		private async Task<Color> GetAverageCoverColorAsync(BeatmapLevel beatmapLevel)
		{
			var sprite = await beatmapLevel.previewMediaData.GetCoverSpriteAsync();

			Color32[] pixels = [];
			try
			{
				pixels = sprite.texture.GetPixels32();
			}
			catch (Exception ex)
			{
				_siraLog.Debug(ex.Message);
				
				await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
				{
					pixels = GetUnreadableTexture(sprite.texture, sprite.textureRect).GetPixels32();
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

			var averageColor = new Color(r / pixels.Length, g / pixels.Length, b / pixels.Length);
			Color.RGBToHSV(averageColor, out var h, out var s, out _);
			return Color.HSVToRGB(h, s, 1f);
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
			
			/*// Write the texture to disk for debugging purposes
			byte[] bytes = readableTexture.EncodeToPNG();
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "debug_texture.png");
			File.WriteAllBytes(path, bytes);*/

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

		private async void LevelCollectionViewControllerOnDidSelectLevelEvent(LevelCollectionViewController viewController, BeatmapLevel beatmapLevel)
		{
			if (!_pluginConfig.EnableLights || !_pluginConfig.UseCoverColor)
			{
				return;
			}

			var averageColor = await GetAverageCoverColorAsync(beatmapLevel);
			_menuPillarsManager.TweenToPillarLightColor(averageColor.ColorWithAlpha(_menuPillarsManager.CurrentColor.a), 0.2f);
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