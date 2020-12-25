using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Assets.Scripts.CourseEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Util;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;
using Action = System.Action;

public class CourseEditorGui : MonoBehaviour {

    public void Initialize() {
    }

    private Action CreateBindable<T>(ISubject<T> action, T value) {
        return () => action.OnNext(value);
    }

    private Action<TInput> CreateBindable<TInput, TOutput>(ISubject<TOutput> action, System.Func<TInput, TOutput> converter) {
        return value => action.OnNext(converter(value));
    }

    public CourseStorageActions CourseStorageActions {
        set { }
    }

    public CourseEditorActions CourseEditorActions {
        set {  }
    }

    public CourseSelectorActions CourseSelectorActions {
        set {  }
    }

    public IObservable<CourseSelectorStore.CourseSelectorState> CourseSelectorChanges {
        set { }
    }

    public IObservable<CourseEditorState> CourseEditorChanges {
        set {  }
    }

    public ISubject<Unit> CloseEditor {
        set {  }
    }

    public ISubject<Unit> CloseCurrentCourse {
        set { }
    }

    public ActiveLanguage ActiveLanguage {
        set {  }
    }

    public void Show() {
    }

    public void Hide() {
    }
}
