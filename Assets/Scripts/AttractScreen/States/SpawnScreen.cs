using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Cameras;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Impero.StandardInput;
using RamjetAnvil.StateMachine;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.UI;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.UI;
using Fmod = FMODUnity.RuntimeManager;
using Animation = RamjetAnvil.Coroutine.Routines.Animation;

namespace RamjetAnvil.Volo.States {

    public class SpawnScreen : State {
        [Serializable]
        public class Data {
            [SerializeField] public AbstractUnityEventSystem EventSystem;
            [SerializeField] public AbstractUnityClock MenuClock;
            [SerializeField] public AbstractUnityClock GameClock;
            [SerializeField] public AbstractUnityClock FixedClock;
            [SerializeField] public SpawnpointCameraAnimator CameraAnimator;
            [SerializeField] public GameSettingsProvider GameSettingsProvider;
            [SerializeField] public Text SelectedSpawnpointName;
            [SerializeField] public GameObject SpawnpointBillboardPrefab;
            [SerializeField] public CameraManager CameraManager;
            [SerializeField] public GameObject CameraMount;
            [SerializeField] public FaderSettings FaderSettings;
            [SerializeField] public MusicPlayer MusicPlayer;
            [SerializeField] public SoundMixer SoundMixer;
            [SerializeField] public GameObject QuickControlOverview;

            public IReadonlyRef<MenuActionMap> MenuActionMap;
        }

        private readonly UISketch.NavigableUIList _uiList;
        private readonly Data _data;
        private readonly IList<Spawnpoint> _spawnpoints;
        private readonly ITypedDataCursor<SpawnScreenUIState> _uiState;
        
        private IDisposable _onSpawnpoinSelected;
        private IDisposable _onPausePressed;

        public SpawnScreen(IStateMachine machine, Data data) : base(machine) {
            _data = data;

            var billboardDependencies = new DependencyContainer(new Dictionary<string, object> {
                {"clock", _data.MenuClock},
                {"cameraTransform", _data.CameraMount.transform}
            });

            _spawnpoints = GameObject.FindObjectOfType<SpawnpointDiscoverer>().Spawnpoints;
            var spawnpointBillboards = new List<GameObject>(_spawnpoints.Count);
            var billboardGroup = new GameObject("SpawnpointBillboards");
            for (int i = 0; i < _spawnpoints.Count; i++) {
                var spawnpoint = _spawnpoints[i];
                var spawnpointBillBoard = (GameObject)GameObject.Instantiate(_data.SpawnpointBillboardPrefab);
                spawnpointBillBoard.SetActive(spawnpoint.IsDiscovered);
                DependencyInjector.Default.Inject(spawnpointBillBoard, billboardDependencies);
                spawnpointBillBoard.SetParent(billboardGroup);
                spawnpointBillBoard.transform.Set(spawnpoint.Location.AsTransform);
                spawnpointBillBoard.SetActive(false);
                spawnpointBillboards.Add(spawnpointBillBoard);
            }

            _uiState = TypedDataCursor<SpawnScreenUIState>.Root(new SpawnScreenUIState(
                spawnpointUiList: new UIListState(spawnpointBillboards)));
            var spawnpointUiState = _uiState.To(s => s.SpawnpointUIList);

            var spawnpointIndexChanged = spawnpointUiState
                .To(s => s.HighlightedIndex)
                .OnUpdate
                .DistinctUntilChanged(IntComparer.Instance)
                .Skip(1);

            _data.SelectedSpawnpointName.text =_spawnpoints[spawnpointUiState.Get().HighlightedIndex].Name;
            spawnpointIndexChanged.Subscribe(i => {
                Fmod.PlayOneShot("event:/ui/drop_hover");
                var spawnpoint = _spawnpoints[i];
                _data.SelectedSpawnpointName.text = spawnpoint.Name;
                _data.CameraAnimator.LookTarget = spawnpointBillboards[i].transform;
            });

            _uiList = new UISketch.NavigableUIList(
                spawnpointUiState,
                data.CameraManager.Rig.GetMainCamera(),
                spawnpointIndex => {
                    _data.EventSystem.Emit(new Events.SpawnpointSelected(_spawnpoints[spawnpointIndex].Location));
                });
        }

        IEnumerator<WaitCommand> OnEnter() {
            _data.CameraManager.SwitchMount(_data.CameraMount.GetComponent<ICameraMount>());

            var spawnpointBillboards = _uiState.To(s => s.SpawnpointUIList).To(s => s.Items).Get();
            for (int i = 0; i < _spawnpoints.Count; i++) {
                var spawnpoint = _spawnpoints[i];
                spawnpointBillboards[i].SetActive(spawnpoint.IsDiscovered);
            }

            OnResume();

            yield return WaitCommand.WaitRoutine(CameraTransitions.FadeIn(_data.CameraManager.Rig.ScreenFader, _data.MenuClock, _data.FaderSettings));
        }



        void OnResume() {
            Cursor.lockState = CursorLockMode.None;

            _data.CameraAnimator.enabled = _data.GameSettingsProvider.ActiveVrMode == VrMode.None;
            _data.SelectedSpawnpointName.gameObject.SetActive(true);

            Subscribe();
            _uiList.Resume();
            _data.QuickControlOverview.gameObject.SetActive(true);

            _data.SoundMixer.Unpause(SoundLayer.GameEffects);
        }

        void OnSuspend() {
            Unsubscribe();
            
            _uiList.Suspend();
            _data.QuickControlOverview.gameObject.SetActive(false);
            _data.SelectedSpawnpointName.gameObject.SetActive(false);
        }

        IEnumerator<WaitCommand> OnExit() {
            OnSuspend();

            yield return WaitCommand.WaitRoutine(CameraTransitions.FadeOut(_data.CameraManager.Rig.ScreenFader, _data.MenuClock, _data.FaderSettings));

            _data.CameraAnimator.enabled = false;
            var spawnpointBillboards = _uiState.To(s => s.SpawnpointUIList).To(s => s.Items).Get();
            for (int i = 0; i < spawnpointBillboards.Count; i++) {
                spawnpointBillboards[i].SetActive(false);
            }
        }

        void Subscribe() {
            _onSpawnpoinSelected = _data.EventSystem.Listen<Events.SpawnpointSelected>(SpawnpointSelected);
            _onPausePressed = _data.EventSystem.Listen<Events.OnPausePressed>(OnPausePressed);
        }

        void Unsubscribe() {
            _onSpawnpoinSelected.Dispose();
            _onPausePressed.Dispose();
        }

        void Update() {
            _uiList.Update(new UISketch.NavigableUIList.Input {
                Confirm = _data.MenuActionMap.V.PollButtonEvent(MenuAction.Confirm) == ButtonEvent.Down,
                Cursor = _data.MenuActionMap.V.PollDiscreteCursor()
            });
        }

        void SpawnpointSelected(Events.SpawnpointSelected @event) {
            Fmod.PlayOneShot("event:/ui/drop_select");

            Machine.Transition(VoloStateMachine.States.Playing, @event.Spawnpoint);
        }

        void OnPausePressed() {
            Machine.Transition(VoloStateMachine.States.OptionsMenu, MenuId.StartSelection);
        }
    }

    public class SpawnScreenUIState {
        public readonly UIListState SpawnpointUIList;

        public SpawnScreenUIState(UIListState spawnpointUiList) {
            SpawnpointUIList = spawnpointUiList;
        }
    }

    public class UIListState {
        public IList<GameObject> Items;
        public bool IsInteractable;
        public int HighlightedIndex;

        public UIListState(IList<GameObject> items) {
            Items = items;
            IsInteractable = false;
            HighlightedIndex = 1;
        }
    }
}