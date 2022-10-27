using BeatSaberMarkupLanguage;
using HMUI;
using MenuPillars.UI.ViewControllers;
using Zenject;

namespace MenuPillars.UI.FlowCoordinator
{
	internal sealed class MenuPillarsFlowCoordinator : HMUI.FlowCoordinator
	{
		private MainFlowCoordinator _mainFlowCoordinator = null!;
		private MenuTransitionsHelper _menuTransitionsHelper = null!;
		private MenuPillarsSettingsViewController _menuPillarsSettingsViewController = null!;

		[Inject]
		private void Construct(MainFlowCoordinator mainFlowCoordinator, MenuTransitionsHelper menuTransitionsHelper, MenuPillarsSettingsViewController menuPillarsSettingsViewController)
		{
			_mainFlowCoordinator = mainFlowCoordinator;
			_menuTransitionsHelper = menuTransitionsHelper;
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
			if (_menuPillarsSettingsViewController.SoftRestartRequired)
			{
				_menuTransitionsHelper.RestartGame();
				return;
			}
			
			_mainFlowCoordinator.DismissFlowCoordinator(this);
		}
	}
}