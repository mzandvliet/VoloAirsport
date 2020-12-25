using System;
using System.Collections.Generic;
using System.Linq;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.DependencyInjection.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class UnityDependencyResolver : MonoBehaviour {

    [SerializeField] private List<SerializableDependencyRef> _dependencies;
    private IList<DependencyReference> _nonSerializableRefs;

    private DependencyContainer _dependencyContainer;

    void Awake() {
        _nonSerializableRefs = _nonSerializableRefs ?? new List<DependencyReference>();
        Resolve();
    }

    public List<SerializableDependencyRef> Dependencies {
        get { return _dependencies; }
    }

    public DependencyContainer DependencyContainer {
        get { return _dependencyContainer; }
    }

    public void Resolve() {
        _dependencies = FindDependencies();
        _dependencyContainer = new DependencyContainer();
        foreach (var dependencyReference in _dependencies) {
            _dependencyContainer.AddDependency(new DependencyReference(dependencyReference.Name, dependencyReference.Reference));
        }
        //Debug.Log("non serializable refs " + NonSerializableRefs);
        foreach (var dependencyReference in NonSerializableRefs) {
            _dependencyContainer.AddDependency(dependencyReference);
        }

        var rootSceneObjects = Enumerable.Range(0, SceneManager.sceneCount)
            .Select(sceneIndex => SceneManager.GetSceneAt(sceneIndex))
            .Where(scene => scene.isLoaded)
            .SelectMany(scene => scene.GetRootGameObjects());
        foreach (var sceneObject in rootSceneObjects) {
            //Debug.Log("resolving for " + sceneObject);
            Resolve(sceneObject);
        }
    }

    public void Resolve(GameObject gameObject) {
        DependencyInjector.Default.Inject(gameObject, _dependencyContainer, overrideExisting: false);
    }

    private static List<SerializableDependencyRef> FindDependencies() {
        return FindObjectsOfType<IsDependency>()
            .Where(dependency => dependency.Reference != null)
            .Select(dependency => {
                var dependencyName = dependency.Name == "" ? dependency.Reference.gameObject.name : dependency.Name;
                return new SerializableDependencyRef(dependencyName, dependency.Reference);
            })
            .OrderBy(dependency => dependency.Reference.GetType().FullName)
            .ToList();
    }

    [Serializable]
    public struct SerializableDependencyRef {
        [SerializeField] public string Name;
        [SerializeField] public UnityEngine.Object Reference;

        public SerializableDependencyRef(string name, UnityEngine.Object reference) {
            Name = name;
            Reference = reference;
        }
    }

    public IList<DependencyReference> NonSerializableRefs {
        get {
            if (_nonSerializableRefs == null) {
                _nonSerializableRefs = new List<DependencyReference>();
            }
            return _nonSerializableRefs;
        }
    }
}
