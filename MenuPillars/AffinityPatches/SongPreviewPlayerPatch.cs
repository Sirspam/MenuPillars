using System;
using SiraUtil.Affinity;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace MenuPillars.AffinityPatches
{
	internal sealed class SongPreviewPlayerPatch : IAffinity
	{
		public static event Action<AudioSource>? DefaultAudioSourceStarted;
		public static event Action<AudioSource>? SongPreviewAudioSourceStarted;

		[AffinityPostfix]
		[AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.CrossfadeTo), argumentTypes: new[] { typeof(AudioClip), typeof(float), typeof(float), typeof(float), typeof(bool), typeof(Action) })]
		private void CrossFadeToPatch(bool isDefault, int ____activeChannel, SongPreviewPlayer.AudioSourceVolumeController[] ____audioSourceControllers)
		{
			if (isDefault)
			{
				DefaultAudioSourceStarted?.Invoke(____audioSourceControllers[____activeChannel].audioSource);
				return;
			}
			
			SongPreviewAudioSourceStarted?.Invoke(____audioSourceControllers[____activeChannel].audioSource);
		}
	}
}