using MenuPillars.AffinityPatches;
using MenuPillars.Configuration;
using MenuPillars.Managers;
using MenuPillars.UI.FlowCoordinator;
using MenuPillars.UI.ViewControllers;
using MenuPillars.Utils;
using Zenject;

namespace MenuPillars.Installers
{
	internal sealed class MenuPillarsMenuInstaller : Installer
	{
		private readonly PluginConfig _config;

		public MenuPillarsMenuInstaller(PluginConfig config)
		{
			_config = config;
		}

		public override void InstallBindings()
		{
			Container.BindInstance(_config);

			Container.BindInterfacesAndSelfTo<SongPreviewPlayerPatch>().AsSingle();
			
			Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
			
			Container.BindInterfacesTo<AudioVisualizerManager>().AsSingle();
			Container.BindInterfacesTo<CoverColorManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<TomfooleryManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<MenuPillarsManager>().AsSingle();

			Container.Bind<PillarGrabber>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
			Container.Bind<MenuPillarsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

			Container.Bind<GitHubPageModalController>().AsSingle();
			Container.Bind<MenuPillarsSettingsViewController>().FromNewComponentAsViewController().AsSingle();
			Container.Bind<MenuPillarErrorViewController>().FromNewComponentAsViewController().AsSingle();
		}
	}
}