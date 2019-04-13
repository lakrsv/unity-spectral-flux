using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PeakAudioVisualizerBehaviour : IAudioVisualizationBehaviour
{

    private Animator _animator;

    private bool _lastWasPeak;

    public void VisualizePoint([NotNull] SpectralFluxInfo point)
    {
    }

    public void VisualizePoint([NotNull] SpectralFluxInfo point, GameObject target)
    {
        if (_animator == null) _animator = target.GetComponent<Animator>();
        if (point.IsPeak)
        {
            _animator.SetTrigger("Flash");
        }
    }
}
