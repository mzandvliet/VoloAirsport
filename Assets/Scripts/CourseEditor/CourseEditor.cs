using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Assets.Scripts.CourseEditor;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.CourseEditing;
using RxUnity.Schedulers;
using UnityEngine;

public class CourseEditor : MonoBehaviour {

    [Dependency, SerializeField] private RingProps _renderableProps;
    [Dependency("menuClock"), SerializeField] private AbstractUnityClock _clock;
    [Dependency, SerializeField] private CourseEditorGui _courseEditorGui;
    [Dependency, SerializeField] private Camera _gameCamera;
    [Dependency, SerializeField] private SpectatorCamera _spectatorCamera;
    [Dependency, SerializeField] private ActiveLanguage _activeLanguage;

    // TODO Enable something to turn the course editor on/off
    // TODO Propagate camera/cameraMovement

    private ISubject<Unit> _openEditor;
    private ISubject<Unit> _closeEditor;

    private CompositeDisposable _disposables;
    private bool _isInitialized;

    void Awake() {
        _openEditor = _openEditor ?? new Subject<Unit>();
        _closeEditor = _closeEditor ?? new Subject<Unit>();
        enabled = false;
        _disposables = _disposables ?? new CompositeDisposable();
    }

    void OnEnable() {
        _openEditor.OnNext(Unit.Default);
    }

    void OnDisable() {
        _closeEditor.OnNext(Unit.Default);
    }

    public IObservable<Unit> CloseEditor {
        get { return _closeEditor; }
    }

    public void Initialize() {
        if (!_isInitialized) {
            if (_openEditor == null) {
                _openEditor = new Subject<Unit>();
            }
            if (_closeEditor == null) {
                _closeEditor = new Subject<Unit>();
            }
            if (_disposables == null) {
                _disposables = new CompositeDisposable();
            }

            // Get the Gui camera, this will break in Oculus mode
            var cameraMovement = _spectatorCamera;

            var cameraTransform = new Ref<ImmutableTransform>(
                () => cameraMovement.transform.MakeImmutable(),
                transform => cameraMovement.transform.Set(transform));

            var isCourseEditorOpen = _openEditor.Select(_ => true)
                .Merge(_closeEditor.Select(_ => false))
                .StartWith(false);

            var courseStorageActions = new CourseStorageActions();
            var courseSelectorActions = new CourseSelectorActions();
            var courseEditorActions = new CourseEditorActions();
            var closeCurrentCourse = new Subject<Unit>();

            var fileStorage = CoursesFileStorage.CourseUpdater(courseStorageActions, CoursesFileStorage.CoursesDir.Value);
            _disposables.Add(fileStorage);

            var customCourses = CoursesFileStorage.CustomCourses.Value
                .ObserveOn(UnityThreadScheduler.MainThread);
            var courseSelectorStore = CourseSelectorStore.Create(courseSelectorActions, customCourses, isCourseEditorOpen);
            _disposables.Add(courseSelectorStore.Connect());
            var courseEditorStore = CourseSelectorStore.CreateCourseEditorStore(
                course => CourseEditorStore.Create(courseEditorActions, course, cameraTransform),
                courseSelectorStore.Select(s => {
                    if (s.SelectedCourse.IsJust) {
                        return s.AvailableCourses.Find(c => c.Id.Equals(s.SelectedCourse.Value));
                    }
                    return Maybe.Nothing<CourseData>();
                }));
            _disposables.Add(courseEditorStore.Connect());

            _disposables.Add(_openEditor.Subscribe(_ => _courseEditorGui.Show()));
            _disposables.Add(closeCurrentCourse
                .WithLatestFrom(courseEditorStore, (_, finalCourseState) => finalCourseState)
                .Subscribe(finalCourseState => {
                    // Storing course changes to disk
                    Debug.Log("deselecting a course");
                    courseStorageActions.UpdateCourse.OnNext(finalCourseState.ToSerializableFormat());

                    courseSelectorActions.DeselectCourse.OnNext(Unit.Default);
                }));

            // Initialize input for course editor store
            var propRenderer = new PropRenderer<PropId>(propId => "__CourseEditor-" + propId);
            // Clear the prop renderer when done editing
            _disposables.Add(courseEditorStore.Subscribe(_ => propRenderer.Clear()));
            CourseEditorInput.InitializeCourseEditor(courseEditorActions, courseEditorStore, _gameCamera, cameraTransform, _renderableProps, propRenderer, _clock);

            _courseEditorGui.CourseStorageActions = courseStorageActions;
            _courseEditorGui.CourseEditorActions = courseEditorActions;
            _courseEditorGui.CourseSelectorActions = courseSelectorActions;
            _courseEditorGui.CourseEditorChanges = courseEditorStore;
            _courseEditorGui.CourseSelectorChanges = courseSelectorStore;
            _courseEditorGui.CloseCurrentCourse = closeCurrentCourse;
            _courseEditorGui.CloseEditor = _closeEditor;
            _courseEditorGui.ActiveLanguage = _activeLanguage;
            _courseEditorGui.Initialize();

            var rightMouseButtonPressed = UnityObservable.CreateUpdate<bool>(observer => {
                if (Input.GetMouseButtonDown(1)) {
                    observer.OnNext(true);
                } else if (Input.GetMouseButtonUp(1)) {
                    observer.OnNext(false);
                }
            });

            _disposables.Add(courseSelectorStore
                .CombineLatest(isCourseEditorOpen, rightMouseButtonPressed, (state, isOpen, isRightMouseButtonIsPressed) => {
                    return state.SelectedCourse.IsJust && isOpen && isRightMouseButtonIsPressed;
                })
                .StartWith(false)
                .Subscribe(isMovementEnabled => {
                    if (isMovementEnabled) {
                        cameraMovement.EnableInputProcessing();
                    } else {
                        cameraMovement.DisableInputProcessing();
                    }
                }));

            _disposables.Add(_closeEditor.Subscribe(_ => {
                _courseEditorGui.Hide();
                propRenderer.Clear();
            }));

            _isInitialized = true;
        }
    }

    void OnDestroy() {
        _disposables.Dispose();
        _openEditor.OnCompleted();
        _closeEditor.OnCompleted();
    }
}
