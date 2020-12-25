using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Assets.Scripts.CourseEditor;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.CourseEditing {
    public class CourseSelectorActions {

        private readonly ISubject<string> _selectCourse;
        private readonly ISubject<Unit> _deselectCourse;

        public CourseSelectorActions() {
            _selectCourse = new Subject<string>();
            _deselectCourse = new Subject<Unit>();
        }

        public ISubject<string> SelectCourse {
            get { return _selectCourse; }
        }

        public ISubject<Unit> DeselectCourse {
            get { return _deselectCourse; }
        }
    }

    public static class CourseSelectorStore {

        public static IConnectableObservable<CourseSelectorState> Create(CourseSelectorActions actions, IObservable<IList<CourseData>> courseUpdates, 
            IObservable<bool> isCourseEditorOpen) {

            var commands = actions.SelectCourse.Select(StoreAction.Create<string>("selectCourse"))
                .Merge(actions.DeselectCourse.Select(StoreAction.Create<Unit>("deselectCourse")));

            // Only route commands when the course editor is open
            var filteredCommands = isCourseEditorOpen
                .Select(isOpen => isOpen ? commands : Observable.Empty<StoreAction<object>>())
                .Switch()
                .StartWith(new StoreAction<object>("deselectCourse", Unit.Default));

            return courseUpdates
                .CombineLatest(filteredCommands, (courses, command) => {
                    Maybe<string> selectedCourse = Maybe.Nothing<string>();
                    if (command.Id.Equals("selectCourse")) {
                        var selectedCourseId = (string)command.Arguments;
                        selectedCourse = courses
                            .Find(c => c.Id.Equals(selectedCourseId))
                            .Select(c => c.Id);
                    }

                    return new CourseSelectorState(
                        availableCourses: courses,
                        selectedCourse: selectedCourse,
                        // TODO Enable delete
                        isUndoDeleteCoursePossible: false);
                })
                .Replay(1);
        }

        public static IConnectableObservable<CourseEditorState> CreateCourseEditorStore(
            Func<CourseData, IObservable<CourseEditorState>> createStore, IObservable<Maybe<CourseData>> selectedCourse) {
            return selectedCourse
                .DistinctUntilChanged(c => c.GetOrElse(x => x.Id, () => null))
                .Select(course => course.IsJust ? createStore(course.Value) : Observable.Empty<CourseEditorState>())
                .Switch()
                .Replay(1);
        }

        public struct CourseSelectorState {
            private readonly Maybe<string> _selectedCourse;
            private readonly IList<CourseData> _availableCourses;
            private readonly bool _isUndoDeleteCoursePossible;

            public CourseSelectorState(Maybe<string> selectedCourse, IList<CourseData> availableCourses) {
                _selectedCourse = selectedCourse;
                _availableCourses = availableCourses;
                _isUndoDeleteCoursePossible = false;
            }

            public CourseSelectorState(Maybe<string> selectedCourse, IList<CourseData> availableCourses, bool isUndoDeleteCoursePossible) {
                _selectedCourse = selectedCourse;
                _availableCourses = availableCourses;
                _isUndoDeleteCoursePossible = isUndoDeleteCoursePossible;
            }

            public Maybe<string> SelectedCourse {
                get { return _selectedCourse; }
            }

            public IList<CourseData> AvailableCourses {
                get { return _availableCourses; }
            }

            public bool IsUndoDeleteCoursePossible {
                get { return _isUndoDeleteCoursePossible; }
            }
        }

        public struct CourseEditorAppState {
            private readonly CourseSelectorState _courseSelectorState;
            private readonly Maybe<CourseEditorState> _selectedCourse;

            public CourseEditorAppState(CourseSelectorState courseSelectorState, Maybe<CourseEditorState> selectedCourse) {
                _courseSelectorState = courseSelectorState;
                _selectedCourse = selectedCourse;
            }
            
            public IList<CourseData> AvailableCourses {
                get { return _courseSelectorState.AvailableCourses; }
            }

            public bool IsUndoDeleteCoursePossible {
                get { return _courseSelectorState.IsUndoDeleteCoursePossible; }
            }

            public Maybe<CourseEditorState> SelectedCourse {
                get { return _selectedCourse; }
            }
        }
    }
}
