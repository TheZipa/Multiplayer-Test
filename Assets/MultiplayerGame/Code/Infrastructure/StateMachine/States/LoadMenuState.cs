using Cysharp.Threading.Tasks;
using MultiplayerGame.Code.Core.UI;
using MultiplayerGame.Code.Infrastructure.StateMachine.GameStateMachine;
using MultiplayerGame.Code.Services.Factories.UIFactory;
using MultiplayerGame.Code.Services.Multiplayer;
using MultiplayerGame.Code.Services.SaveLoad;
using MultiplayerGame.Code.Services.SceneLoader;
using UnityEngine;

namespace MultiplayerGame.Code.Infrastructure.StateMachine.States
{
    public class LoadMenuState : IState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly IUIFactory _uiFactory;
        private readonly ISaveLoad _saveLoad;
        private readonly IMultiplayerConnection _multiplayerConnection;
        private const string MenuScene = "Menu";

        public LoadMenuState(IGameStateMachine stateMachine, ISceneLoader sceneLoader,
            IUIFactory uiFactory, ISaveLoad saveLoad, IMultiplayerConnection multiplayerConnection)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _uiFactory = uiFactory;
            _saveLoad = saveLoad;
            _multiplayerConnection = multiplayerConnection;
        }
        
        public void Enter()
        {
            _multiplayerConnection.OnConnectingSuccess += SwitchToMenu;
            _sceneLoader.LoadScene(MenuScene, PrepareMenu);
        }

        public void Exit() => _multiplayerConnection.OnConnectingSuccess -= SwitchToMenu;

        private void SwitchToMenu() => _stateMachine.Enter<MenuState>();

        private async void PrepareMenu()
        {
            await CreateMenuElements();
            _multiplayerConnection.Connect();
        }
        
        private async UniTask CreateMenuElements()
        {
            await _uiFactory.WarmUpMainMenu();
            GameObject rootCanvas = await _uiFactory.CreateRootCanvas();
            await _uiFactory.CreateMainMenu(rootCanvas.transform);
            await _uiFactory.CreateRoomListScreen(rootCanvas.transform);
            await _uiFactory.CreateRoomCreateScreen(rootCanvas.transform);
            await _uiFactory.CreateRoomScreen(rootCanvas.transform);
            await _uiFactory.InstantiateAsRegistered<ErrorScreen>(rootCanvas.transform);
            await _uiFactory.CreateSettingsPanel(rootCanvas.transform);
        }
    }
}