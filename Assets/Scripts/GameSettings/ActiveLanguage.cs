using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using RamjetAnvil.Volo;
using UnityEngine;

public class ActiveLanguage : MonoBehaviour {

    private Languages _languages;
    private LanguageTable _table;
    private ISubject<LanguageTable> _tableUpdates;

    private bool _isInitialized;

    void Awake() {
        Initialize();
    }

    public Languages Languages {
        get {
            Initialize();
            return _languages;
        }
    }

    public IImmutableDictionary<string, string> Table {
        get {
            Initialize();
            return _table.Table;
        }
    }

    public IObservable<LanguageTable> TableUpdates {
        get {
            Initialize();
            return _tableUpdates;
        }
    }

    private void Initialize() {
        if (!_isInitialized) {
            SetLanguage(new CultureCode("en", "US"));
        }
    }

    private void OnDestroy() {
        _tableUpdates.OnCompleted();
    }

    public void SetLanguage(CultureCode culture) {
        _languages = _languages ?? LanguageSettings.ReadLanguages();
        var languageTable = GetLanguageTable(_languages, culture);
        _table = languageTable;
        if (_tableUpdates == null) {
            _tableUpdates = new BehaviorSubject<LanguageTable>(_table);
        } else {
            _tableUpdates.OnNext(_table);
        }
        //_tableUpdates = _tableUpdates ?? new ReplaySubject<LanguageTable>(1);
        _tableUpdates.OnNext(languageTable);

        _isInitialized = true;
    }

    private static LanguageTable GetLanguageTable(Languages languages, CultureCode culture) {
        LanguageTable languageTable;
        if (languages.LanguageTables.TryGetValue(culture, out languageTable)) {
            return languageTable;
        }
        throw new Exception("Unknown language: " + culture);
    } 
}
