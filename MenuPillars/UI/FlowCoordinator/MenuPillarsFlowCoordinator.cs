using BeatSaberMarkupLanguage;
using HMUI;
using MenuPillars.UI.ViewControllers;
using Zenject;

namespace MenuPillars.UI.FlowCoordinator
{
	internal class MenuPillarsFlowCoordinator : HMUI.FlowCoordinator
	{
		private MainFlowCoordinator _mainFlowCoordinator = null!;
		private MenuPillarsSettingsViewController _menuPillarsSettingsViewController = null!;

		[Inject]
		private void Construct(MainFlowCoordinator mainFlowCoordinator, MenuPillarsSettingsViewController menuPillarsSettingsViewController)
		{
			_mainFlowCoordinator = mainFlowCoordinator;
			_menuPillarsSettingsViewController = menuPillarsSettingsViewController;
		}
		
		protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
		{
			SetTitle(nameof(MenuPillars));
			showBackButton = true;
			
			ProvideInitialViewControllers(_menuPillarsSettingsViewController);
		}
		
		protected override void BackButtonWasPressed(ViewController topViewController)
		{
			_mainFlowCoordinator.DismissFlowCoordinator(this);
		}
	}
}