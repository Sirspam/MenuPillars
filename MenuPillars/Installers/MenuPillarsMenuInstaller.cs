using MenuPillars.Configuration;
using MenuPillars.Managers;
using MenuPillars.UI.FlowCoordinator;
using MenuPillars.UI.ViewControllers;
using MenuPillars.Utils;
using Zenject;

namespace MenuPillars.Installers
{
	internal class MenuPillarsMenuInstaller : Installer
	{
		private readonly PluginConfig _config;

		public MenuPillarsMenuInstaller(PluginConfig config)
		{
			_config = config;
		}

		public override void InstallBindings()
		{
			Container.BindInstance(_config);
			Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<TrollageManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<MenuPillarsManager>().AsSingle();

			Container.Bind<PillarGrabber>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
			Container.Bind<MenuPillarsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

			Container.Bind<GitHubPageModalController>().AsSingle();
			Container.Bind<MenuPillarsSettingsViewController>().FromNewComponentAsViewController().AsSingle();
		}
	}
}