using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using RamjetAnvil.DependencyInjection;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedTextLabel : MonoBehaviour {

    [SerializeField, Dependency] private ActiveLanguage _activeLanguage;
    [SerializeField] private Text _label;
    [SerializeField] private string _phraseId;

    private IDisposable _subscription;

    void Awake() {
        Initialize();
    }

    void Initialize() {
        if (Application.isPlaying && _activeLanguage != null) {
            _subscription = _subscription ?? Disposable.Empty;
            _subscription.Dispose();

            _subscription = _activeLanguage.TableUpdates.Subscribe(languageTable => {
                string phrase;
                if (languageTable.Table.TryGetValue(_phraseId, out phrase)) {
                    _label.text = phrase;   
                }
            });
        }
    }

    [Dependency]
    public ActiveLanguage ActiveLanguage {
        get { return _activeLanguage; }
        set {
            _activeLanguage = value;
            Initialize();
        }
    }
}
