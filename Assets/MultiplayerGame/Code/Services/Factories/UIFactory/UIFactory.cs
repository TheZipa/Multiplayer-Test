using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MultiplayerGame.Code.Core.UI;
using MultiplayerGame.Code.Core.UI.MainMenu;
using MultiplayerGame.Code.Core.UI.Rooms;
using MultiplayerGame.Code.Core.UI.Rooms.CreateRoom;
using MultiplayerGame.Code.Core.UI.Settings;
using MultiplayerGame.Code.Services.Assets;
using MultiplayerGame.Code.Services.EntityContainer;
using MultiplayerGame.Code.Services.Multiplayer;
using MultiplayerGame.Code.Services.Quality;
using MultiplayerGame.Code.Services.SaveLoad;
using MultiplayerGame.Code.Services.StaticData;
using UnityEngine;
using UnityEngine.Pool;

namespace MultiplayerGame.Code.Services.Factories.UIFactory
{
    public class UIFactory : BaseFactory.BaseFactory, IUIFactory
    {
        private readonly ISaveLoad _saveLoad;
        private readonly IStaticData _staticData;
        private readonly IMultiplayerRooms _multiplayerRooms;
        private readonly IQualityService _qualityService;
        private const string RootCanvasKey = "RootCanvas";

        public UIFactory(IStaticData staticData, IAssets assets, IEntityContainer entityContainer, 
            IMultiplayerRooms multiplayerRooms, ISaveLoad saveLoad, IQualityService qualityService)
        : base(assets, entityContainer)
        {
            _staticData = staticData;
            _multiplayerRooms = multiplayerRooms;
            _saveLoad = saveLoad;
            _qualityService = qualityService;
        }

        public async UniTask WarmUpPersistent() => await _assets.LoadPersistent<GameObject>(RootCanvasKey);

        public async UniTask WarmUpMainMenu()
        {
            await _assets.Load<GameObject>(nameof(MainMenuView));
            await _assets.Load<GameObject>(nameof(RoomListScreen));
            await _assets.Load<GameObject>(nameof(RoomConnectField));
            await _assets.Load<GameObject>(nameof(RoomCreateScreen));
            await _assets.Load<GameObject>(nameof(RoomPlayerField));
            await _assets.Load<GameObject>(nameof(RoomScreen));
            await _assets.Load<GameObject>(nameof(SettingsPanel));
            await _assets.Load<GameObject>(nameof(MapSelectElement));
            await _assets.Load<GameObject>(nameof(MapSelectPanel));
            await _assets.Load<GameObject>(nameof(ErrorScreen));
        }

        public async UniTask WarmUpGameplay()
        {
            await _assets.Load<GameObject>(nameof(InGameMenuPanel));
        }

        public async UniTask<GameObject> CreateRootCanvas() => await _assets.Instantiate<GameObject>(RootCanvasKey);

        public async UniTask<MainMenuView> CreateMainMenu(Transform root)
        {
            MainMenuView mainMenuView = await InstantiateAsRegistered<MainMenuView>(root);
            mainMenuView.SetSavedNickname(_saveLoad.Progress.Nickname);
            return mainMenuView;
        }

        public async UniTask<SettingsPanel> CreateSettingsPanel(Transform root)
        {
            SettingsPanel settingsPanel = await InstantiateAsRegistered<SettingsPanel>(root);
            settingsPanel.Construct(_qualityService);
            return settingsPanel;
        }

        public async UniTask<RoomListScreen> CreateRoomListScreen(Transform root)
        {
            RoomListScreen roomListScreen = await InstantiateAsRegistered<RoomListScreen>(root);
            IObjectPool<RoomConnectField> objectPool = new ObjectPool<RoomConnectField>(() =>
                    Instantiate<RoomConnectField>(roomListScreen.RoomsContent).GetAwaiter().GetResult(), 
                roomField => roomField.Show(), roomField => roomField.Hide());
            roomListScreen.Construct(_multiplayerRooms, objectPool, _staticData.WorldData.Maps);
            return roomListScreen;
        }

        public async UniTask<RoomCreateScreen> CreateRoomCreateScreen(Transform root)
        {
            RoomCreateScreen roomCreateScreen = await InstantiateAsRegistered<RoomCreateScreen>(root);
            MapSelectPanel mapSelectPanel = await Instantiate<MapSelectPanel>(roomCreateScreen.transform);
            mapSelectPanel.Construct(await CreateMapSelectElements(mapSelectPanel.Content));
            roomCreateScreen.Construct(mapSelectPanel);
            return roomCreateScreen;
        }

        public async UniTask<RoomScreen> CreateRoomScreen(Transform root)
        {
            RoomScreen roomScreen = await InstantiateAsRegistered<RoomScreen>(root);
            roomScreen.Construct(_multiplayerRooms, await CreateRoomPlayerFields(roomScreen.PlayerFieldContent),
                _staticData.GameConfiguration.MaxPlayers, _staticData.GameConfiguration.MinPlayersForStart);
            return roomScreen;
        }

        private async UniTask<Stack<RoomPlayerField>> CreateRoomPlayerFields(Transform content)
        {
            Stack<RoomPlayerField> roomPlayerFields = new Stack<RoomPlayerField>(_staticData.GameConfiguration.MaxPlayers);
            for (int i = 0; i < _staticData.GameConfiguration.MaxPlayers; i++)
            {
                RoomPlayerField roomPlayerField = await Instantiate<RoomPlayerField>(content);
                roomPlayerField.Hide();
                roomPlayerFields.Push(roomPlayerField);
            }
            return roomPlayerFields;
        }

        private async UniTask<MapSelectElement[]> CreateMapSelectElements(Transform content)
        {
            MapSelectElement[] mapSelectElements = new MapSelectElement[_staticData.WorldData.Maps.Length];
            for (int i = 0; i < mapSelectElements.Length; i++)
            {
                MapSelectElement mapSelectElement = await Instantiate<MapSelectElement>(content);
                mapSelectElement.Construct(_staticData.WorldData.Maps[i], i);
                mapSelectElements[i] = mapSelectElement;
            }
            return mapSelectElements;
        }
    }
}