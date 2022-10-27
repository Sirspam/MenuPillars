using System;
using MenuPillars.AffinityPatches;
using MenuPillars.Configuration;
using UnityEngine;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class AudioVisualizerManager : IInitializable, IDisposable, ITickable
	{
		private const int SampleNumber = 1024;

		private AudioSource? _currentAudioSource = null;
		private float _peakAmplitude;
		
		private readonly PluginConfig _pluginConfig;
		private readonly MenuPillarsManager _menuPillarsManager;

		public AudioVisualizerManager(PluginConfig pluginConfig, MenuPillarsManager menuPillarsManager)
		{
			_pluginConfig = pluginConfig;
			_menuPillarsManager = menuPillarsManager;
		}

		// Unfortunately I am stupid so this solution is kinda trash :(
		public void Tick()
		{
			if (_currentAudioSource is null)
			{
				return;
			}
			
			float[] samples = new float[SampleNumber];
			
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
			
			_menuPillarsManager.SetPillarLightColors(_menuPillarsManager.CurrentColor.ColorWithAlpha(Mathf.InverseLerp(0f, _peakAmplitude, amplitude)));
		}

		private void SongPreviewPlayerPatchOnDefaultAudioSourceStarted(AudioSource audioSource)
		{ 
			if (_currentAudioSource is not null)
			{
				if (_pluginConfig.RainbowLights)
				{
					_menuPillarsManager.TweenPillarColorAlpha(1f);
				}
				else
				{
					_menuPillarsManager.TweenToPillarLightColor(_pluginConfig.PillarLightsColor);	
				}
			}
			
			_currentAudioSource = null;
		}

		private void SongPreviewPlayerPatchOnSongPreviewAudioSourceStarted(AudioSource audioSource)
		{
			_currentAudioSource = audioSource;
			_peakAmplitude = 0.06f;
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