using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;

public class AlphaManager : MonoBehaviour {
    [SerializeField] private List<Renderer> _renderers;
    [Dependency("menuClock"), SerializeField] private AbstractUnityClock _clock;

    private float _alpha = 1f;

    public IEnumerator<WaitCommand> SetAlphaAsync(float targetAlpha, TimeSpan duration) {
        var startAlpha = _alpha;
        yield return Routines.Animate(_clock.PollDeltaTime, duration, lerp => {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);
            SetAlpha(alpha);
        }).AsWaitCommand();
    }
    
    public void SetAlpha(float alpha) {
        _alpha = alpha;
        for (int i = 0; i < _renderers.Count; i++) {
            _renderers[i].material.SetFloat("_Alpha", _alpha);
        }
    }
}
