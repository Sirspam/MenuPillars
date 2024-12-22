using System;
using System.Linq;
using MenuPillars.AffinityPatches;
using MenuPillars.Configuration;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class AudioVisualizerManager : IInitializable, IDisposable, ITickable
	{
		private const int SampleNumber = 256;
		private readonly float[] _samples;

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
			
			_samples = new float[SampleNumber];
		}

		// Either I am dumb or real time audio visualization is hard, likely the former
		public void Tick()
		{
			if (!_pluginConfig.VisualizeAudio || !_pluginConfig.EnableLights || _currentAudioSource is null)
			{
				return;
			}
			
			_currentAudioSource.GetOutputData(_samples, 0);

			var amplitude = Mathf.Sqrt(_samples.Sum(sample => sample * sample) / SampleNumber);
			
			_peakAmplitude = Mathf.Lerp(_peakAmplitude, Mathf.Max(amplitude, _peakAmplitude), 0.1f);

			var normalizedAmplitude = Mathf.InverseLerp(0f, _peakAmplitude, amplitude);
			_menuPillarsManager.CurrentColor = _menuPillarsManager.CurrentColor.ColorWithAlpha(normalizedAmplitude);
		}

		private void SongPreviewPlayerPatchOnDefaultAudioSourceStarted(AudioSource audioSource)
		{
			if (!_pluginConfig.EnableLights || !_pluginConfig.VisualizeAudio)
			{
				return;
			}
			
			if (_currentAudioSource is not null)
			{
				if (_pluginConfig.RainbowLights)
				{
					_menuPillarsManager.ToggleRainbowColors(true);
				}
				else
				{
					_menuPillarsManager.TweenToPillarLightColor(_menuPillarsManager.CurrentColor.ColorWithAlpha(1f));	
				}
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