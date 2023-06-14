using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;

public class AudioSourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private AudioBeamFrameReader _Reader;
    private byte[] _Data = null;
    private int cnt = 0;
    public byte[] GetData()
    {
        return _Data;
    }

    // Start is called before the first frame update
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.AudioSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }

            if(_Reader != null)
            {
                _Reader.FrameArrived += Reader_FrameArrived;
            }
        }

    }
    private void Reader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
    {
        AudioBeamFrameReference frameReference = e.FrameReference;
        var frameList = frameReference.AcquireBeamFrames();
        if (frameList != null)
        {
            cnt++;
        }

        print(cnt);
    }

    // Update is called once per frame
    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestBeamFrames();
            if(frame != null)
            {
                //print("frame.Count: " + frame.Count);
                //if (frame[0]. != null)
                //    print(frame[0].AudioBeam);
                //else
                //    print("FRAME NULL!!!");
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}