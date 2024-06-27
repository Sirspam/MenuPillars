using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using MenuPillars.Configuration;
using MenuPillars.Installers;
using SiraUtil.Zenject;

namespace MenuPillars
{
	[Plugin(RuntimeOptions.DynamicInit)][NoEnableDisable]
	public class Plugin
	{
		[Init]
		public void Init(Config config, Logger logger, Zenjector zenjector)
		{
			zenjector.UseSiraSync();
			zenjector.UseLogger(logger);
			zenjector.UseMetadataBinder<Plugin>();

			zenjector.Install<MenuPillarsMenuInstaller>(Location.Menu, config.Generated<PluginConfig>());
		}
	}
}