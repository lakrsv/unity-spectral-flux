// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioVisualizer.cs" author="Lars" company="None">
// Copyright (c) 2018, Lars-Kristian Svenoey. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioVisualizer : MonoBehaviour
{
    private AudioProcessor _audioProcessor;

    private AudioSource _audioSource;

    private bool _playClip;

    private readonly IAudioVisualizationBehaviour _bassVisualizationBehaviour = new PeakAudioVisualizerBehaviour();

    private readonly IAudioVisualizationBehaviour _midRangeVisualizationBehaviour = new PeakAudioVisualizerBehaviour();

    private readonly IAudioVisualizationBehaviour _highRangeVisualizationBehaviour = new PeakAudioVisualizerBehaviour();

    [SerializeField]
    private GameObject _bassObj, _midRangeObj, _highRangeObj;

    private SpectralFluxAnalyzer _bassAnalyzer, _midRangeAnalyzer, _highRangeAnalyzer;

    // Use this for initialization
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioProcessor = new AudioProcessor(1024);

        // Bass
        _audioProcessor.ProcessClip(
            _audioSource.clip,
            new SpectralFluxAnalyzer(1024, _audioSource.clip.frequency, 20, 250),
            analyzer => { _bassAnalyzer = analyzer; });

        // Midrange
        _audioProcessor.ProcessClip(
            _audioSource.clip,
            new SpectralFluxAnalyzer(1024, _audioSource.clip.frequency, 250, 4000),
            analyzer => { _midRangeAnalyzer = analyzer; });

        // High
        _audioProcessor.ProcessClip(
            _audioSource.clip,
            new SpectralFluxAnalyzer(1024, _audioSource.clip.frequency, 4000, 20000),
            analyzer => { _highRangeAnalyzer = analyzer; });
    }

    // Update is called once per frame
    private void Update()
    {
        if (_bassAnalyzer == null || _midRangeAnalyzer == null || _highRangeAnalyzer == null) return;

        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
        }

        var currentPoint = _audioProcessor.GetCurrentPlayingPointIndex(_audioSource);
        _bassVisualizationBehaviour.VisualizePoint(_bassAnalyzer.SpectralFluxSamples[currentPoint], _bassObj);
        _midRangeVisualizationBehaviour.VisualizePoint(_midRangeAnalyzer.SpectralFluxSamples[currentPoint], _midRangeObj);
        _highRangeVisualizationBehaviour.VisualizePoint(_highRangeAnalyzer.SpectralFluxSamples[currentPoint], _highRangeObj);
    }
}