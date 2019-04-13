// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectralFluxAnalyzer.cs" author="Lars" company="None">
// Copyright (c) 2018, Lars-Kristian Svenoey. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using UnityEngine;

public class SpectralFluxInfo
{
    public bool IsPeak;

    public float PrunedSpectralFlux;

    public float SpectralFlux;

    public float Threshold;

    public float Time;

    public override string ToString()
    {
        return
            $"IsPeak {IsPeak}\nPrunedSpectralFlux {PrunedSpectralFlux}\nSpectralFlux {SpectralFlux}\nThreshold {Threshold}\nTime {Time}";
    }
}

public class SpectralFluxAnalyzer
{
    public int MinFrequency => _minFrequency;

    public int MaxFrequency => _maxFrequency;

    private readonly float[] _currentSpectrum;

    private readonly int _minFrequency;

    private readonly int _maxFrequency;

    private readonly int _numSamples;

    private readonly float[] _previousSpectrum;

    // Sensitivity multiplier to scale the average threshold.
    // In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
    private readonly float _thresholdMultiplier = 1.5f;

    // Number of samples to average in our window
    private readonly int _thresholdWindowSize = 50;

    private int _indexToProcess;

    private readonly float _sampleRate;

    public SpectralFluxAnalyzer(
        int numSamples,
        float sampleRate,
        int minFrequency = -1,
        int maxFrequency = -1,
        float thresholdMultiplier = 1.5f,
        int thresholdWindowSize = 50)
    {
        SpectralFluxSamples = new List<SpectralFluxInfo>();

        // Start processing from middle of first window and increment by 1 from there
        _indexToProcess = thresholdWindowSize / 2;

        _currentSpectrum = new float[numSamples];
        _previousSpectrum = new float[numSamples];

        _numSamples = numSamples;
        _minFrequency = minFrequency;
        _maxFrequency = maxFrequency;

        _thresholdMultiplier = thresholdMultiplier;
        _thresholdWindowSize = thresholdWindowSize;
        _sampleRate = sampleRate;
    }

    public List<SpectralFluxInfo> SpectralFluxSamples { get; }

    public void AnalyzeSpectrum(float[] spectrum, float time)
    {
        // Set spectrum
        SetCurrentSpectrum(spectrum);

        // Get current spectral flux from spectrum
        var currentInfo = new SpectralFluxInfo();
        currentInfo.Time = time;
        currentInfo.SpectralFlux = CalculateRectifiedSpectralFlux();
        SpectralFluxSamples.Add(currentInfo);

        // We have enough samples to detect a peak
        if (SpectralFluxSamples.Count >= _thresholdWindowSize)
        {
            // Get Flux threshold of time window surrounding index to process
            SpectralFluxSamples[_indexToProcess].Threshold = GetFluxThreshold(_indexToProcess);

            // Only keep amp amount above threshold to allow peak filtering
            SpectralFluxSamples[_indexToProcess].PrunedSpectralFlux = GetPrunedSpectralFlux(_indexToProcess);

            // Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
            var indexToDetectPeak = _indexToProcess - 1;

            var curPeak = IsPeak(indexToDetectPeak);

            if (curPeak)
            {
                SpectralFluxSamples[indexToDetectPeak].IsPeak = true;
            }

            _indexToProcess++;
        }
        else
        {
            Debug.Log(
                string.Format(
                    "Not ready yet.  At spectral flux sample size of {0} growing to {1}",
                    SpectralFluxSamples.Count,
                    _thresholdWindowSize));
        }
    }

    public void SetCurrentSpectrum(float[] spectrum)
    {
        _currentSpectrum.CopyTo(_previousSpectrum, 0);
        spectrum.CopyTo(_currentSpectrum, 0);
    }

    private float CalculateRectifiedSpectralFlux()
    {
        if (_minFrequency == -1 || _maxFrequency == -1)
        {
            var sum = 0f;

            // Aggregate positive changes in spectrum data
            for (var i = 0; i < _numSamples; i++)
            {
                sum += Mathf.Max(0f, _currentSpectrum[i] - _previousSpectrum[i]);
            }

            return sum;
        }
        else
        {
            var hertzPerBin = _sampleRate / 2f / _numSamples;
            var minIndex = (int)Mathf.Max(0, (_minFrequency / hertzPerBin) - 2);
            var maxIndex = (int)Mathf.Min(_numSamples - 1, (_maxFrequency / hertzPerBin) + 2);

            var sum = 0f;
            for (var i = minIndex; i <= maxIndex; i++)
            {
                sum += Mathf.Max(0f, _currentSpectrum[i] - _previousSpectrum[i]);
            }
            return sum;
        }
    }

    private float GetFluxThreshold(int spectralFluxIndex)
    {
        // How many samples in the past and future we include in our average
        var windowStartIndex = Mathf.Max(0, spectralFluxIndex - _thresholdWindowSize / 2);
        var windowEndIndex = Mathf.Min(SpectralFluxSamples.Count - 1, spectralFluxIndex + _thresholdWindowSize / 2);

        // Add up our spectral flux over the window
        var sum = 0f;
        for (var i = windowStartIndex; i < windowEndIndex; i++)
        {
            sum += SpectralFluxSamples[i].SpectralFlux;
        }

        // Return the average multiplied by our sensitivity multiplier
        var avg = sum / (windowEndIndex - windowStartIndex);
        return avg * _thresholdMultiplier;
    }

    private float GetPrunedSpectralFlux(int spectralFluxIndex)
    {
        return Mathf.Max(
            0f,
            SpectralFluxSamples[spectralFluxIndex].SpectralFlux - SpectralFluxSamples[spectralFluxIndex].Threshold);
    }

    private bool IsPeak(int spectralFluxIndex)
    {
        if (SpectralFluxSamples[spectralFluxIndex].PrunedSpectralFlux
            > SpectralFluxSamples[spectralFluxIndex + 1].PrunedSpectralFlux
            && SpectralFluxSamples[spectralFluxIndex].PrunedSpectralFlux
            > SpectralFluxSamples[spectralFluxIndex - 1].PrunedSpectralFlux)
        {
            return true;
        }

        return false;
    }
}