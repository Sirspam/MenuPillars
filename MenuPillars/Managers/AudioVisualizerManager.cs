using System;
using MenuPillars.AffinityPatches;
using MenuPillars.Configuration;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class AudioVisualizerManager : IInitializable, IDisposable, ITickable
	{
		private const int SampleNumber = 1024;

		private AudioSource? _currentAudioSource;
		private float _peakAmplitude;
		
		private readonly PluginConfig _pluginConfig;
		private readonly SiraLog _siraLog;
		private readonly MenuPillarsManager _menuPillarsManager;

		public AudioVisualizerManager(PluginConfig pluginConfig, MenuPillarsManager menuPillarsManager, SiraLog siraLog)
		{
			_pluginConfig = pluginConfig;
			_menuPillarsManager = menuPillarsManager;
			_siraLog = siraLog;
		}

		// Unfortunately I am stupid so this looks trash :(
		public void Tick()
		{
			if (!_pluginConfig.VisualizeAudio || !_pluginConfig.EnableLights || _currentAudioSource is null)
			{
				return;
			}
			
			var samples = new float[SampleNumber];
			
			_currentAudioSource.GetOutputData(samples, 0);

			var amplitude = 0f;
			foreach (var sample in samples)
			{
				amplitude += Mathf.Abs(sample);
			}
			
			amplitude /= SampleNumber;
			
			if (amplitude > _peakAmplitude)
			{
				_peakAmplitude = amplitude;
			}
			
			_menuPillarsManager.CurrentColor = _menuPillarsManager.CurrentColor.ColorWithAlpha(Mathf.InverseLerp(0f, _peakAmplitude, amplitude));
		}

		private void SongPreviewPlayerPatchOnDefaultAudioSourceStarted(AudioSource audioSource)
		{
			if (!_pluginConfig.EnableLights || !_pluginConfig.VisualizeAudio)
			{
				return;
			}
			
			if (_currentAudioSource is not null)
			{
				_menuPillarsManager.TweenToUserColors();
			}
			
			_currentAudioSource = null;
		}

		private void SongPreviewPlayerPatchOnSongPreviewAudioSourceStarted(AudioSource audioSource)
		{
			if (!_pluginConfig.EnableLights || !_pluginConfig.VisualizeAudio)
			{
				return;
			}
			
			_currentAudioSource = audioSource;
			_peakAmplitude = 0.065f;
		}

		public void Initialize()
		{
			SongPreviewPlayerPatch.DefaultAudioSourceStarted += SongPreviewPlayerPatchOnDefaultAudioSourceStarted;
			SongPreviewPlayerPatch.SongPreviewAudioSourceStarted += SongPreviewPlayerPatchOnSongPreviewAudioSourceStarted;
		}

		public void Dispose()
		{
			SongPreviewPlayerPatch.DefaultAudioSourceStarted -= SongPreviewPlayerPatchOnDefaultAudioSourceStarted;
			SongPreviewPlayerPatch.SongPreviewAudioSourceStarted -= SongPreviewPlayerPatchOnSongPreviewAudioSourceStarted;
		}
	}
}