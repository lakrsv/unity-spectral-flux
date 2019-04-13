// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAudioVisualizationBehaviour.cs" author="Lars" company="None">
// Copyright (c) 2018, Lars-Kristian Svenoey. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using JetBrains.Annotations;

using UnityEngine;

public interface IAudioVisualizationBehaviour
{
    void VisualizePoint([NotNull] SpectralFluxInfo point);

    void VisualizePoint([NotNull] SpectralFluxInfo point, GameObject target);
}