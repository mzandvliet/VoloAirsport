using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts.CourseEditor;
using FMOD;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.Util.Serialization;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Guid = System.Guid;

namespace RamjetAnvil.Volo.CourseEditing {

    public class CourseStorageActions {
        private readonly ISubject<Unit> _createCourse;
        private readonly ISubject<CourseData> _updateCourse;
        private readonly ISubject<string> _deleteCourse;
        private readonly ISubject<Unit> _undoDeleteCourse;

        public CourseStorageActions() {
            _createCourse = new Subject<Unit>();
            _updateCourse = new Subject<CourseData>();
            _deleteCourse = new Subject<string>();
            _undoDeleteCourse = new Subject<Unit>();
        }

        public ISubject<Unit> CreateCourse {
            get { return _createCourse; }
        }

        public ISubject<CourseData> UpdateCourse {
            get { return _updateCourse; }
        }

        public ISubject<string> DeleteCourse {
            get { return _deleteCourse; }
        }

        public ISubject<Unit> UndoDeleteCourse {
            get { return _undoDeleteCourse; }
        }
    }

    public class CoursesFileStorage {

        public static IDisposable CourseUpdater(CourseStorageActions actions, string coursesDirectory) {
            var commands = actions.CreateCourse.Select(StoreAction.Create<Unit>("createCourse"))
                .Merge(actions.UpdateCourse.Select(StoreAction.Create<CourseData>("updateCourse")))
                .Merge(actions.DeleteCourse.Select(StoreAction.Create<string>("deleteCourse")))
                .Merge(actions.UndoDeleteCourse.Select(StoreAction.Create<Unit>("undoDeleteCourse")));

            return commands
                .Subscribe(command => {
                    if (command.Id.Equals("createCourse")) {
                        var newCourse = CourseData.CreateNew();
                        CreateCourse(coursesDirectory, newCourse);
                    } else if (command.Id.Equals("updateCourse")) {
                        var updatedCourse = (CourseData) command.Arguments;
                        UpdateCourse(coursesDirectory, updatedCourse);
                    } else if (command.Id.Equals("deleteCourse")) {
                        var deletedCourseId = (string) command.Arguments;
                        DeleteCourse(coursesDirectory, deletedCourseId);
                    } else if (command.Id.Equals("undoDeleteCourse")) {
                        var deletedCourseId = (string) command.Arguments;
                        UndoDeleteCourse(coursesDirectory, deletedCourseId);
                    }
                });
        }

        public static void UpdateCourse(string coursesDirectory, CourseData courseData) {
            PermanentlyDeleteCourse(coursesDirectory, courseData.Id);
            CreateCourse(coursesDirectory, courseData);
        }

        public static void DeleteCourse(string coursesDirectory, string courseId) {
            var courseFiles = Directory.GetFiles(coursesDirectory)
                .Where(fullPath => Path.GetFileName(fullPath).Contains(courseId) && Path.GetExtension(fullPath).Equals(".json"));
            foreach (var courseFile in courseFiles) {
                File.Move(courseFile, courseFile + ".deleted");
            }
        }

        public static void UndoDeleteCourse(string coursesDirectory, string courseId) {
            // Only undo delete when the course does not exist
            var courseFiles = Directory.GetFiles(coursesDirectory)
                .Where(fullPath => Path.GetFileName(fullPath).Contains(courseId) && Path.GetExtension(fullPath).Equals(".deleted"));
            foreach (var courseFile in courseFiles) {
                File.Move(courseFile, courseFile.Replace(".deleted", ""));
            }
        }
        
        public static void CreateCourse(string coursesDirectory, CourseData courseData) {
            var tempFilePath = "~" + FileName(courseData) + ".temp." + Guid.NewGuid();
            tempFilePath = Path.Combine(coursesDirectory, tempFilePath);
            Write2Disk(tempFilePath, courseData);
            var coursePath = Path.Combine(coursesDirectory, FileName(courseData));
            File.Delete(coursePath);
            File.Move(tempFilePath, coursePath);
        }

        public static void PermanentlyDeleteCourse(string coursesDirectory, string courseId) {
            var courseFiles = Directory.GetFiles(coursesDirectory)
                .Where(fullPath => Path.GetFileName(fullPath).Contains(courseId));
            foreach (var courseFile in courseFiles) {
                File.Delete(courseFile);
            }
        }

        public static void RemoveAllDeletedCourses(string coursesDirectory) {
            var courseFiles = Directory.GetFiles(coursesDirectory)
                .Where(fullPath => Path.GetFileName(fullPath).EndsWith(".deleted"));
            foreach (var courseFile in courseFiles) {
                File.Delete(courseFile);
            }
        }


        public static readonly Lazy<IObservable<IList<CourseData>>> CustomCourses = new Lazy<IObservable<IList<CourseData>>>(
            () => {
                Directory.CreateDirectory(CoursesDir.Value);
                return WatchChanges(CoursesDir.Value);
            });

        public static readonly Lazy<IImmutableList<CourseData>> StockCourses = new Lazy<IImmutableList<CourseData>>(
            () => CourseSerialization.DeserializeCourses(StockCoursesDir(Application.streamingAssetsPath))
                .ToImmutableList());

        public static IObservable<IList<CourseData>> WatchChanges(string coursesDirectory) {
            return FileWatching.TrackDirectory(FileWatching.WatcherSettings.Create(coursesDirectory, "*.json"))
                .Select(files => files
                    .Where(f => File.Exists(f.FullPath))
                    .Select(f => CourseSerialization.Deserialize(f.FullPath))
                    .ToList() as IList<CourseData>);


            /*var watcherSettings = FileWatching.WatcherSettings.Create(coursesDirectory, "*.json");
            FileWatching.FileChanges(watcherSettings);


            return Observable.Create<IImmutableList<CourseData>>(observer => {
                var initialCourses = FileWatching.GetCurrentFiles(watcherSettings)
                    .Select(courseFile => CourseSerialization.Deserialize(courseFile.FullPath))
                    .ToImmutableList() as IImmutableList<CourseData>;

                var disposable = FileWatching.FileChanges(watcherSettings)
                    .Scan(initialCourses, (courses, fileEvent) => {
                        if (fileEvent.ChangeType == WatcherChangeTypes.Renamed || fileEvent.ChangeType == WatcherChangeTypes.Changed ||
                            fileEvent.ChangeType == WatcherChangeTypes.Created) {
                            var updatedCourse = CourseSerialization.Deserialize(fileEvent.FullPath);
                            UnityEngine.Debug.Log("updated course " + fileEvent.FullPath);
                            for (int i = courses.Count - 1; i >= 0; i--) {
                                var course = courses[i];
                                if (course.Id.Equals(updatedCourse.Id)) {
                                    courses = courses.RemoveAt(i);
                                }
                            }
                            courses = courses.Add(updatedCourse);
                        } else if (fileEvent.ChangeType == WatcherChangeTypes.Deleted) {
                            UnityEngine.Debug.Log("deleted course " + fileEvent.FullPath);
                            var deletedCourseId = CourseSerialization.CourseIdFromFileName(fileEvent.FullPath);
                            for (int i = courses.Count - 1; i >= 0; i--) {
                                var course = courses[i];
                                if (course.Id.Equals(deletedCourseId)) {
                                    courses = courses.RemoveAt(i);
                                }
                            }
                        }

                        return courses;
                    })
                    .Synchronize(observer)
                    .Subscribe(observer);

                observer.OnNext(initialCourses);

                return disposable;
            }).Publish().RefCount();*/
        }


        public static string FileName(string courseId, string courseName) {
            // Remove non-ASCII chars
            var name = Regex.Replace(courseName, @"[^\u0000-\u007F]", "");
            name = name.Replace(" ", "-");
            name = name.Replace("_", "-");
            name = name.ToLower();

            return name + "_" + courseId + ".json";
        }

        public static string FileName(CourseData courseData) {
            return FileName(courseData.Id, courseData.Name);
        }

        public static string CourseIdFromFileName(string fullPath) {
            try {
                return Path.GetFileNameWithoutExtension(fullPath).Split('_')[1];
            } catch (Exception e) {
                Debug.LogException(new Exception("Cannot extract course id from filename " + fullPath, e));
                return null;
            } 
        }

        public static void Write2Disk(string courseFileName, CourseData courseData) {
            using (var writer = File.CreateText(courseFileName)) {
                CourseSerialization.Serializer.Serialize(writer, courseData);
            }
        }

        public static readonly Lazy<string> CoursesDir = new Lazy<string>(() => {
            var os = PlatformUtil.CurrentOs();
            string path;
            if (os == PlatformUtil.OperatingSystem.MacOsx) {
                path = Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "Courses");
            } else if (os == PlatformUtil.OperatingSystem.Windows) {
                path = Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "Courses");
            } else if (os == PlatformUtil.OperatingSystem.Linux) {
                path = Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "courses");
            } else {
                throw new Exception("Unsupported operating system: " + os);
            }

            return Path.GetFullPath(path);
        });

        public static string StockCoursesDir(string streamingAssetsPath) {
            return Path.Combine(streamingAssetsPath, "StockCourses");
        }

        public static IEnumerable<string> AvailableCourseFiles(string coursesDirPath) {
            Directory.CreateDirectory(coursesDirPath);
            return Directory.GetFiles(coursesDirPath)
                .Where(file => file.EndsWith(".json"));
        }
    }

    
    public static class CourseSerialization {

        public static readonly JsonSerializer Serializer;

        static CourseSerialization() {
            Serializer = new JsonSerializer {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            Serializer.Converters.Add(new StringEnumConverter());
            Serializer.Converters.Add(new ImmutableTransformConverter());
        }

        public static CourseData ToSerializableFormat(this CourseEditorState editorState) {
            var props = ImmutableList.Create<Prop>();
            for (int i = 0; i < editorState.PropOrder.Count; i++) {
                var editorProp = editorState.Props[editorState.PropOrder[i]];
                props = props.Add(new Prop{ PropType = editorProp.PropType, Transform = editorProp.Transform });
            }
            return new CourseData { Id = editorState.Id, Name = editorState.CourseName, Props = props};
        }
        
        public static HistoricalCourseEditorState ToCourseEditorState(this CourseData courseData) {
            var selectedPropType = PropType.RingNormal;
            var propOrder = ImmutableList<PropId>.Empty;
            var props = ImmutableDictionary<PropId, EditorProp>.Empty;
            for (int i = 0; i < courseData.Props.Count; i++) {
                var prop = courseData.Props[i];
                var editorProp = new EditorProp {Id = PropId.CreateRandomId(), PropType = prop.PropType, Transform = prop.Transform};

                propOrder = propOrder.Add(editorProp.Id);
                props = props.Add(editorProp.Id, editorProp);

                selectedPropType = editorProp.PropType;
            }
            // Automatically select the first prop if possible
            //var selectedProp = propOrder.Count > 0 ? Maybe.Just(propOrder[0]) : Maybe.Nothing<PropId>();
            var selectedProp = Maybe.Nothing<PropId>();
            return new HistoricalCourseEditorState(courseData.Id, selectedPropType, selectedProp, props,
                propOrder, false, false);
        }


        public static IEnumerable<CourseData> DeserializeCourses(string coursesDirPath) {
            return CoursesFileStorage.AvailableCourseFiles(coursesDirPath)
                .Select(file => Deserialize(file));
        }

        public static CourseData Deserialize(string filePath) {
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(reader)) {
                return Serializer.Deserialize<CourseData>(jsonReader);
            }
        }
    }
}
