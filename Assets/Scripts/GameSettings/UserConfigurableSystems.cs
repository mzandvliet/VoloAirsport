using System;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Landmass;
using RamjetAnvil.Volo;
using UnityEngine;

public class UserConfigurableSystems : MonoBehaviour {

    [Dependency, SerializeField] private StaticTiledTerrain _terrainManager;
    [Dependency, SerializeField] private GrassManager _grassManager;
    [Dependency, SerializeField] private SoundMixer _SoundMixer;
    [Dependency, SerializeField] private GameHud _GameHud;
    [Dependency, SerializeField] private CameraManager _cameraManager;
    [Dependency, SerializeField] private ChallengeManager _challengeManager;
    [Dependency, SerializeField] private ActiveLanguage _activeLanguage;

    public StaticTiledTerrain TerrainManager {
        get { return _terrainManager; }
    }

    public GrassManager GrassManager {
        get { return _grassManager; }
    }

    public SoundMixer SoundMixer {
        get { return _SoundMixer; }
    }

    public GameHud GameHud {
        get { return _GameHud; }
    }

    public CameraManager CameraManager {
        get { return _cameraManager; }
    }

    public ChallengeManager CourseManager {
        get { return _challengeManager; }
    }

    public ActiveLanguage ActiveLanguage {
        get { return _activeLanguage; }
    }
}
