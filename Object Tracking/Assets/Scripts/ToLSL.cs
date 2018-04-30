using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using Assets.LSL4Unity.Scripts;
using Assets.LSL4Unity.Scripts.Common;


public class ToLSL : MonoBehaviour {

    private const string unique_source_id = "J3O4P895THJQ9PA0ROG8HINAEOPIRIHTGQA340P9O8";
    private liblsl.StreamOutlet outlet;
    private liblsl.StreamInfo streamInfo;

    public string StreamName = "Object Selection";
    public string StreamType = "Unity.Quaterion";
    public float dataRate;
    public MomentForSampling sampling;

    public int ChannelCount = 1;

    private int[] currentSample;

    public GameObject sampleSource;

    public void pushSample()
    {
        if (outlet == null)
            return;
        currentSample = sampleSource.GetComponent<BallManager>().sendInfo;
        outlet.push_sample(currentSample, liblsl.local_clock());
    }




    // Use this for initialization
    void Start()
    {
        dataRate = LSLUtils.GetSamplingRateFor(sampling);

        streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, dataRate, liblsl.channel_format_t.cf_int32, unique_source_id);

        outlet = new liblsl.StreamOutlet(streamInfo);
    }

}
