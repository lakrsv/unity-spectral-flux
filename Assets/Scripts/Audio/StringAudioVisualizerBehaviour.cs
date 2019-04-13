using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringAudioVisualizer : IAudioVisualizationBehaviour 
{
    public void VisualizePoint(SpectralFluxInfo point)
    {
        Debug.Log(point.ToString());
    }

    public void VisualizePoint(SpectralFluxInfo point, GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
