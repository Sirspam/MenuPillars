using BeatSaberMarkupLanguage;
using HMUI;
using MenuPillars.UI.ViewControllers;
using MenuPillars.Utils;
using Zenject;

namespace MenuPillars.UI.FlowCoordinator
{
	internal sealed class MenuPillarsFlowCoordinator : HMUI.FlowCoordinator
	{
		private PillarGrabber _pillarGrabber = null!;
		private MainFlowCoordinator _mainFlowCoordinator = null!;
		private MenuPillarErrorViewController _menuPillarErrorViewController = null!;
		private MenuPillarsSettingsViewController _menuPillarsSettingsViewController = null!;

		[Inject]
		private void Construct(PillarGrabber pillarGrabber, MainFlowCoordinator mainFlowCoordinator, MenuPillarErrorViewController menuPillarErrorViewController, MenuPillarsSettingsViewController menuPillarsSettingsViewController)
		{
			_pillarGrabber = pillarGrabber;
			_mainFlowCoordinator = mainFlowCoordinator;
			_menuPillarErrorViewController = menuPillarErrorViewController;
			_menuPillarsSettingsViewController = menuPillarsSettingsViewController;
		}
		
		protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
		{
			SetTitle(nameof(MenuPillars));
			showBackButton = true;

			if (_pillarGrabber.completed)
			{
				ProvideInitialViewControllers(_menuPillarsSettingsViewController);	
			}
			else
			{
				ProvideInitialViewControllers(_menuPillarErrorViewController);
			}
		}
		
		protected override void BackButtonWasPressed(ViewController topViewController) => _mainFlowCoordinator.DismissFlowCoordinator(this);
	}
}