using System;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo;
using RamjetAnvil.Volo.TitleScreen;
using UnityEngine;

public class TitleScreenCameraAnimator : MonoBehaviour {

    [Dependency, SerializeField] private AbstractUnityClock _clock;
    [Dependency, SerializeField] private UnityCoroutineScheduler _coroutineScheduler;
    [Dependency, SerializeField] private CameraRig _rig;
    [Dependency, SerializeField] private CameraPaths _cameraPaths;
    [Dependency, SerializeField] private GameSettingsProvider _gameSettingsProvider;

    [SerializeField] private FaderSettings _faderSettings;

    public IEnumerator<WaitCommand> Animate(Transform mount, Predicate isRunning) {
        var pathSelection = _cameraPaths.Paths;
        var selectedPath = pathSelection.RandomElement();
        var isFirst = true;
        while (isRunning()) {
            selectedPath = pathSelection.GetNext(selectedPath);

            if (_gameSettingsProvider.IsVrActive) {
                selectedPath = new StaticCameraPath(
                    Routines.LinearAnimation, 
                    selectedPath.Duration, 
                    selectedPath.To, 
                    to: selectedPath.To);
            }

            IEnumerator<WaitCommand> fadeIn;
            if (isFirst) {
                fadeIn = WaitCommand.DontWait.AsRoutine;
                isFirst = false;
            } else {
                fadeIn = _rig.ScreenFader.FadeIn(_clock, _faderSettings);
            }

            var fadeOut = WaitCommand.Wait(selectedPath.Duration - _faderSettings.FadeOutDuration)
                .AndThen(_rig.ScreenFader.FadeOut(_clock, _faderSettings));

            var cameraAnimation = CameraAnimator.Animate(mount, selectedPath, _clock);

            yield return WaitCommand
                .Interleave(fadeIn, cameraAnimation, fadeOut)
                .RunWhile(isRunning)
                .AsWaitCommand();
        }
        _rig.ScreenFader.Opacity = 0f;
    }
}
