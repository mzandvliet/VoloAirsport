//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reactive.Concurrency;
//using System.Reactive.Linq;
//using RamjetAnvil.Coroutine.Time;
//using RamjetAnvil.DependencyInjection;
//using RamjetAnvil.GameObjectFactories;
//using RamjetAnvil.Unity.Utility;
//using RamjetAnvil.Util;
//using RamjetAnvil.Volo.CourseEditing;
//using RamjetAnvil.Volo.Util;
//using RxUnity.Schedulers;
//using UnityEngine;
//using Fmod = FMODUnity.RuntimeManager;
//
//public class CourseManager : MonoBehaviour {
//    public struct ActiveCourse {
//        public string CourseId;
//        public int TotalRings;
//        public int CurrentRing;
//        public IPooledObject<HashSet<int>> RingsPassed; 
//        public double StartTime;
//    }
//
//    public struct CourseInstance : IDisposable {
//        public string Id;
//        public IPooledGameObject Holder;
//        public IPooledGameObject Label;
//        public IList<Tuple<PropType, IPooledGameObject>> Rings;
//        public void Dispose() {
//            Label.Dispose();
//            for (int i = 0; i < Rings.Count; i++) {
//                Rings[i]._2.Dispose();
//            }
//            Holder.Dispose();
//        }
//    }
//
//    [Serializable]
//    public struct CourseSettings : IEquatable<CourseSettings> {
//        [SerializeField]
//        public bool IsRingCollision;
//        [SerializeField]
//        public bool IsCoursesEnabled;
//
//        public bool Equals(CourseSettings other) {
//            return IsRingCollision.Equals(other.IsRingCollision) && IsCoursesEnabled.Equals(other.IsCoursesEnabled);
//        }
//
//        public override bool Equals(object obj) {
//            if (ReferenceEquals(null, obj)) return false;
//            return obj is CourseSettings && Equals((CourseSettings) obj);
//        }
//
//        public override int GetHashCode() {
//            unchecked {
//                return (IsRingCollision.GetHashCode() * 397) ^ IsCoursesEnabled.GetHashCode();
//            }
//        }
//
//        public static bool operator ==(CourseSettings left, CourseSettings right) {
//            return left.Equals(right);
//        }
//
//        public static bool operator !=(CourseSettings left, CourseSettings right) {
//            return !left.Equals(right);
//        }
//    }
//
//    [Dependency, SerializeField] private AbstractUnityClock _clock;
//    [Dependency, SerializeField] private NotificationList _notificationList;
//    [Dependency, SerializeField] private ActiveLanguage _activeLanguage;
//    [SerializeField] private Transform _cameraTransform;
//    [SerializeField] private float _notificationTimeout = 10f;
//    [SerializeField] private GameObject _courseLabelPrefab;
//    [SerializeField] private RingProps _ringProps;
//    [SerializeField] private int _courseNameLimit = 35;
//
//    private IObservable<IEnumerable<CourseData>> _courseUpdates;
//
//    private IList<CourseData> _allCourses;
//
//    private IList<CourseInstance> _courseInstances;
//    private IList<ActiveCourse> _activeCourses;
//    private CourseSettings _courseSettings;
//
//    private GameObject _courseParent;
//    private IObjectPool<HashSet<int>> _ringsPassedPool;
//    private IPrefabPool _coursePool;
//    private IPrefabPool _courseLabelPool;
//    private IDisposable _courseChangeListener;
//
//    void Awake() {
//        _courseSettings = new CourseSettings { IsCoursesEnabled = true, IsRingCollision = true };
//        _courseInstances = new List<CourseInstance>();
//        _activeCourses = new List<ActiveCourse>();
//
//        _ringsPassedPool = new ObjectPool<HashSet<int>>(
//            factory: () => {
//                var h = new HashSet<int>();
//                return new ManagedObject<HashSet<int>>(h, h.Clear);
//            }, 
//            growthStep: 10);
//        _courseParent = new GameObject("Courses");
//        _courseLabelPool = new PrefabPool("CourseLabelPool", _courseLabelPrefab, 50);
//        _coursePool = new PrefabPool("CoursePool", () => new GameObject(), 200);
//
//        _allCourses = new List<CourseData>();
//        _courseChangeListener = CoursesFileStorage.CustomCourses.Value
//            .ObserveOn(UnityThreadScheduler.MainThread)
//            .Subscribe(customCourses => {
//                _allCourses.Clear();
//                for (int i = 0; i < customCourses.Count; i++) {
//                    _allCourses.Add(customCourses[i]);
//                }
//                var stockCourses = CoursesFileStorage.StockCourses.Value;
//                for (int i = 0; i < stockCourses.Count; i++) {
//                    _allCourses.Add(stockCourses[i]);
//                }
//                CreateCourses();
//            });
//    }
//
//    void OnDestroy() {
//        _courseChangeListener.Dispose();
//    }
//    
//    public void UpdateSettings(CourseSettings settings) {
//        if (!settings.Equals(_courseSettings)) {
//            _courseSettings = settings;
//            UpdateCoursesState();    
//        }
//    }
//
//    public void Reset() {
//        _activeCourses.Clear();
//        UpdateCoursesState();
//    }
//
//    public void OnEnable() {
//        UpdateCoursesState();
//    }
//
//    public void OnDisable() {
//        UpdateCoursesState();
//    }
//
//    void CreateCourses() {
//        CleanupCourses();
//
//        foreach (var courseData in _allCourses) {
//            var courseId = courseData.Id;
//            var courseName = courseData.Name.Trim().Length > 0 ? courseData.Name.Limit(_courseNameLimit, "..") : _activeLanguage.Table["unnamed_course"];
//
//            var course = _coursePool.Spawn(Vector3.zero, Quaternion.identity);
//            course.GameObject.name = courseName;
//
//            var courseLabelObject = _courseLabelPool.Spawn(Vector3.zero, Quaternion.identity);
//
//            var ringCount = courseData.Props.Count;
//            var rings = new List<Tuple<PropType, IPooledGameObject>>(ringCount);
//            for (int i = 0; i < courseData.Props.Count; i++) {
//                var ringIndex = i;
//                var prop = courseData.Props[i];
//                var ringObject = _ringProps.Factory[prop.PropType]
//                    .Spawn(prop.Transform.Position, prop.Transform.Rotation);
//                //ringObject.GameObject.transform.localScale = prop.Transform.Scale;
//                ringObject.GameObject.SetParent(course.GameObject);
//                var ring = ringObject.GameObject.GetComponent<Ring>();
//
//                var isFirstRing = ringIndex == 0;
//                var isLastRing = ringIndex == courseData.Props.Count - 1;
//                if (isFirstRing) {
//                    var courseLabel = courseLabelObject.GameObject.GetComponent<CourseLabel>();
//                    courseLabel.CourseName = courseName;
//                    courseLabel.Ring = ring.gameObject.FindInChildren("Model");
//                    if (_cameraTransform != null) {
//                        courseLabel.CameraTransform = _cameraTransform;
//                    } else {
//                        courseLabel.CameraTransform = transform;
//                    }
//                    courseLabel.gameObject.SetParent(ring.gameObject);
//                    courseLabel.transform.localPosition = Vector3.zero;
//                    courseLabel.transform.localRotation = Quaternion.identity;
//
//                    // Add Ring OnPlayerContact first ring contact
//                    ring.OnPlayerContact += player => StartCourse(courseId, courseName, ringCount);
//                }
//
//                if (isLastRing) {
//                    // Add ring finish handler
//                    ring.OnPlayerContact += player => FinishCourse(courseId);
//                }
//
//                if (!(isFirstRing || isLastRing)) {
//                    ring.OnPlayerContact += player => PassRing(courseId, ringIndex);
//                }
//
//                rings.Add(new Tuple<PropType, IPooledGameObject>(prop.PropType, ringObject));
//            }
//
//            course.GameObject.SetParent(_courseParent);
//            _courseInstances.Add(new CourseInstance {
//                Holder = course,
//                Id = courseId,
//                Label = courseLabelObject,
//                Rings = rings
//            });
//        }
//
//        UpdateCoursesState();
//    }
//
//    private void CleanupCourses() {
//        // Reset the course currently active to invalidate any previous state
//        _activeCourses.Clear();
//        for (int i = 0; i < _courseInstances.Count; i++) {
//            _courseInstances[i].Dispose();
//        }
//        _courseInstances.Clear();
//    }
//
//    private void StartCourse(string courseId, string courseName, int totalRings) {
//        if (!_activeCourses.HasValue(IsActiveCourse, courseId)) {
//            const int ringIndex = 0;
//            var ringsPassed = _ringsPassedPool.Take();
//            ringsPassed.Instance.Add(ringIndex);
//            _activeCourses.Add(new ActiveCourse {
//                CourseId = courseId,
//                RingsPassed = ringsPassed,
//                StartTime = _clock.CurrentTime,
//                TotalRings = totalRings
//            });
//
//            UpdateCoursesState();
//
//            _notificationList.AddTimedNotification(_activeLanguage.Table["course"] + ": " + courseName, _notificationTimeout.Seconds());
//
//            Fmod.PlayOneShot("event:/Objects/ring_pass_start");
//        }
//    }
//
//    private void PassRing(string courseId, int ringIndex) {
//        var activeCourse = _activeCourses.FindElement(IsActiveCourse, courseId);
//        if (activeCourse.HasValue) {
//            var updatedCourse = activeCourse.Value;
//            updatedCourse.CurrentRing = ringIndex;
//            updatedCourse.RingsPassed.Instance.Add(ringIndex);
//            _activeCourses.UpdateAt(IsActiveCourse, courseId, updatedCourse);
//
//            Fmod.PlayOneShot("event:/Objects/ring_pass_norm");
//        }
//    }
//
//    private void FinishCourse(string courseId) {
//        var activeCourse = _activeCourses.FindElement(IsActiveCourse, courseId);
//        if (activeCourse.HasValue) {
//            var totalRingsPassed = activeCourse.Value.RingsPassed.Instance.Count + 1;
//            var ringsSkipped = activeCourse.Value.TotalRings - totalRingsPassed;
//            var endTime = _clock.CurrentTime - activeCourse.Value.StartTime;
//            string ringsSkippedStr;
//            if (ringsSkipped <= 0) {
//                ringsSkippedStr = "";
//            } else if (ringsSkipped == 1) {
//                ringsSkippedStr = " (" + _activeLanguage.Table["ring_skipped"] + ")";
//            } else {
//                ringsSkippedStr = " (" + _activeLanguage.Table["rings_skipped"].Replace("$n", ringsSkipped.ToString()) + ")";
//            }
//            _notificationList.AddTimedNotification(
//                _activeLanguage.Table["time"] + ": " +
//                MathUtils.RoundToDecimals(endTime, 3).ToString("N3") +
//                " " + _activeLanguage.Table["seconds"] + 
//                ringsSkippedStr, _notificationTimeout.Seconds());
//
//            Fmod.PlayOneShot("event:/Objects/ring_pass_start");
//        }
//    }
//
//    private static bool IsActiveCourse(ActiveCourse activeCourse, string courseId) {
//        return activeCourse.CourseId.Equals(courseId);
//    }
//
//    private void RemoveActiveCourse(ActiveCourse activeCourse) {
//        activeCourse.RingsPassed.Dispose();
//        _activeCourses.RemoveAt(IsActiveCourse, activeCourse.CourseId);
//    }
//
//    private bool IsNextRing(ActiveCourse activeCourse, int ringIndex) {
//        return activeCourse.CurrentRing + 1 == ringIndex;
//    }
//
//    private void UpdateCoursesState() {
//        var isCourseActive = _activeCourses.Count > 0;
//        for (int i = 0; i < _courseInstances.Count; i++) {
//            var courseInstance = _courseInstances[i];
//            var activeCourse = _activeCourses.FindElement(IsActiveCourse, courseInstance.Id);
//            var isActiveCourse = activeCourse.HasValue;
//            for (int j = 0; j < courseInstance.Rings.Count; j++) {
//                var ringIndex = j;
//                var isFirstRing = ringIndex == 0;
//                var isNextRing = activeCourse.HasValue && IsNextRing(activeCourse.Value, ringIndex);
//                // Enable ring if it is part of the active course
//                // or if there is no active course only enable it when it is the first ring.
//                bool isEnabled = (isActiveCourse || (isFirstRing && !isCourseActive)) && _courseSettings.IsCoursesEnabled && enabled;
//
//                var ring = courseInstance.Rings[ringIndex]._2;
//                if (!ring.GameObject.IsDestroyed()) {
//                    ring.GameObject.GetComponent<Ring>().UpdateState(isEnabled, isActiveCourse, _courseSettings.IsRingCollision, isNextRing);    
//                }
//            }
//
//            if (!courseInstance.Label.GameObject.IsDestroyed()) {
//               courseInstance.Label.GameObject.SetActive((isActiveCourse || !isCourseActive) && _courseSettings.IsCoursesEnabled && enabled);    
//            }
//        }
//    }
//
//
//    [Dependency("cameraTransform")]
//    public Transform CameraTransform {
//        get { return _cameraTransform; }
//        set {
//            _cameraTransform = value;
//            for (int i = 0; i < _courseInstances.Count; i++) {
//                var courseInstance = _courseInstances[i];
//                var courseLabel = courseInstance.Label.GameObject.GetComponent<CourseLabel>();
//                courseLabel.CameraTransform = value;
//            }
//        }
//    }
//}
//
