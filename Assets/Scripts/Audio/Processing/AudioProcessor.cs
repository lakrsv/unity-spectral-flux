// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioProcessor.cs" author="Lars" company="None">
// Copyright (c) 2018, Lars-Kristian Svenoey. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

using DSPLib;

using JetBrains.Annotations;

using UnityEngine;

public class AudioProcessor
{
    private readonly uint _fftSampleSize;

    private ThreadSafeAudioClip _threadClip;

    public AudioProcessor(uint fftSampleSize)
    {
        _fftSampleSize = fftSampleSize;
    }

    public delegate void OnAudioClipProcessed(SpectralFluxAnalyzer analyzer);

    public int GetCurrentPlayingPointIndex(AudioSource playingSource)
    {
        var currentTime = playingSource.time / _fftSampleSize;
        var currentPointIndex = GetIndexFromTime(currentTime, _threadClip.Duration, _threadClip.SampleCount);

        return currentPointIndex;
    }

    public void ProcessClip(AudioClip clip, SpectralFluxAnalyzer spectralFluxAnalyzer, OnAudioClipProcessed callback)
    {
        _threadClip = new ThreadSafeAudioClip(clip);

        var t = new Thread(() =>
            {
                ProcessFullSpectrum(
                        spectralFluxAnalyzer,
                        _threadClip,
                        callback);
            });
        t.Start();
    }

    [NotNull]
    private float[] GetChannelsCombined([NotNull] float[] channelSamples, int numChannels, int numSamples)
    {
        var combinedSamples = new float[numSamples];

        var numProcessed = 0;
        var combinedChannelAverage = 0f;
        for (var i = 0; i < channelSamples.Length; i++)
        {
            combinedChannelAverage += channelSamples[i];

            if ((i + 1) % numChannels != 0) continue;

            combinedSamples[numProcessed] = combinedChannelAverage / numChannels;
            numProcessed++;
            combinedChannelAverage = 0f;
        }

        return combinedSamples;
    }

    private int GetIndexFromTime(float curTime, float clipLength, int numTotalSamples)
    {
        var lengthPerSample = clipLength / numTotalSamples;

        return Mathf.FloorToInt(curTime / lengthPerSample);
    }

    private float GetTimeFromIndex(int index, int sampleRate)
    {
        return ((1f / sampleRate) * index);
    }

    private void ProcessFullSpectrum(
        SpectralFluxAnalyzer analyzer,
        ThreadSafeAudioClip clip,
        OnAudioClipProcessed callback)
    {
        var processedSamples = GetChannelsCombined(clip.Samples, clip.ChannelCount, clip.SampleCount);
        Debug.Log("Channels have been combined");

        var iterations = processedSamples.Length / _fftSampleSize;

        var fft = new FFT();
        fft.Initialize(_fftSampleSize);

        var chunk = new double[_fftSampleSize];
        for (var i = 0; i < iterations; ++i)
        {
            Array.Copy(processedSamples, i * _fftSampleSize, chunk, 0, _fftSampleSize);

            var windowCoefficients = DSP.Window.Coefficients(DSP.Window.Type.Hamming, _fftSampleSize);
            var scaledSpectrumChunk = DSP.Math.Multiply(chunk, windowCoefficients);
            var scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefficients);

            var fftSpectrum = fft.Execute(scaledSpectrumChunk);
            var scaledFftSpectrum = DSP.ConvertComplex.ToMagnitude(fftSpectrum);
            scaledFftSpectrum = DSP.Math.Multiply(scaledFftSpectrum, scaleFactor);

            var currentSongTime = GetTimeFromIndex(i, clip.Frequency) * _fftSampleSize;
            analyzer.AnalyzeSpectrum(Array.ConvertAll(scaledFftSpectrum, x => (float)x), currentSongTime);
        }

        callback(analyzer);
    }

    private class ThreadSafeAudioClip
    {
        public ThreadSafeAudioClip(AudioClip audioClip)
        {
            Samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(Samples, 0);

            ChannelCount = audioClip.channels;
            SampleCount = audioClip.samples;
            Duration = audioClip.length;
            Frequency = audioClip.frequency;
        }

        public int ChannelCount { get; private set; }

        public float Duration { get; private set; }

        public int Frequency { get; private set; }

        public int SampleCount { get; private set; }

        public float[] Samples { get; private set; }
    }
}