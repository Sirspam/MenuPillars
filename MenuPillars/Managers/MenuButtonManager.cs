using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using MenuPillars.UI.FlowCoordinator;
using Zenject;

namespace MenuPillars.Managers
{
	internal sealed class MenuButtonManager : IInitializable, IDisposable
	{
		private readonly MenuButton _menuButton;
		private readonly MainFlowCoordinator _mainFlowCoordinator;
		private readonly MenuPillarsFlowCoordinator _menuPillarsFlowCoordinator;

		public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, MenuPillarsFlowCoordinator menuPillarsSettingsViewController)
		{
			_menuButton = new MenuButton(nameof(MenuPillars), "Are these things even pillars? like the game only internally refers to them as 'near buildings'", MenuButtonClicked);
			_mainFlowCoordinator = mainFlowCoordinator;
			_menuPillarsFlowCoordinator = menuPillarsSettingsViewController;
		}

		public void Initialize() => MenuButtons.instance.RegisterButton(_menuButton);

		public void Dispose()
		{
			if (MenuButtons.IsSingletonAvailable)
			{
				MenuButtons.instance.UnregisterButton(_menuButton);
			}
		}

		private void MenuButtonClicked() => _mainFlowCoordinator.PresentFlowCoordinator(_menuPillarsFlowCoordinator);
	}
}