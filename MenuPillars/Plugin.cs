using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using MenuPillars.Configuration;
using MenuPillars.Installers;
using IPALogger = IPA.Logging.Logger;

namespace MenuPillars
{
	[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
	public class Plugin
	{
		[Init]
		public void Init(Config config, IPALogger logger, Zenjector zenjector)
		{
			zenjector.UseSiraSync();
			zenjector.UseLogger(logger);
			zenjector.UseMetadataBinder<Plugin>();

			zenjector.Install<MenuPillarsMenuInstaller>(Location.Menu, config.Generated<PluginConfig>());
		}
	}
}