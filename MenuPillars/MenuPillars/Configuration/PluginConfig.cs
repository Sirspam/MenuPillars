using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace MenuPillars.Configuration
{
	internal class PluginConfig
	{
		public virtual bool EnableLights { get; set; } = true;
		public virtual Color PillarLightsColor { get; set; } = Color.cyan;
		public virtual float LightsBrightness { get; set; } = 0.75f;
		public virtual bool RainbowLights { get; set; } = false;
		public virtual float RainbowLoopSpeed { get; set; } = 5f;
		public virtual bool EasterEggs { get; set; } = true;
		public virtual bool SunModeActive { get; set; } = false;
		public virtual bool SunMode { get; set; } = false;
		
	}
}