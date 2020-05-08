using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("Set if video player's render mode is set to Material override")]
    [SerializeField] private Renderer videoDisplayRenderer;
    [Tooltip("Set if video player's render mode is set to Render texture")]
    [SerializeField] private RawImage videoDisplayImage;
    [Tooltip("If not set, Camera.main will be used")]
    [SerializeField] private Camera mainCamera;

    [Header("Parametre vypnutia videa")]
    [Tooltip("If camera pitches below this angle for a certain time, video will be stopped")]
    [SerializeField] private float pitchThreshold;
    [Tooltip("If camera pitch stays below angle threshold for this amount of time, video will be stopped")]
    [SerializeField] private float timeThreshold;
    [Tooltip("Whether video should be reset and start from the beginning or just paused and then resume")]
    [SerializeField] private bool resetVideo;

    public VideoPlayer VideoPlayer { get => videoPlayer; }
    private Camera cam;

    private void Awake()
    {
        ShowVideoDisplay(false);
    }

    void Start()
    {
        cam = mainCamera != null ? mainCamera : Camera.main;

        videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
        StartCoroutine(CameraWatcher());
    }

    private void VideoPlayer_prepareCompleted(VideoPlayer source)
    {
        ShowVideoDisplay(true);
    }

    IEnumerator CameraWatcher()
    {
        //To give AR camera time to rotate itself after start
        yield return new WaitForSeconds(.5f);

        float cameraPitch = GetCameraPitch(cam.transform);
        bool inActionMode = cameraPitch < pitchThreshold;
        float startTime = float.NaN;

        while(true)
        {
            cameraPitch = GetCameraPitch(cam.transform);

            if (inActionMode)
            {
                if (cameraPitch < pitchThreshold)
                {
                    if (!videoPlayer.isPlaying)
                    {
                        Debug.Log("Davam play");
                        videoPlayer.Play();
                    }
                }
                else
                {
                    if (float.IsNaN(startTime))
                        startTime = Time.time;
                    else if (Time.time - startTime >= timeThreshold)
                    {
                        inActionMode = false;
                        startTime = float.NaN;
                        Debug.Log("Prechadzam na pause mode");
                    }
                }
            }
            else  // pause mode
            {
                if (cameraPitch > pitchThreshold)
                {
                    if (videoPlayer.isPlaying)
                    {
                        Debug.Log(string.Format("Davam {0}", resetVideo ? "Stop" : "Pauzu"));
                        if (resetVideo)
                        {
                            videoPlayer.Stop();
                            ShowVideoDisplay(false);
                        }
                        else
                            videoPlayer.Pause();
                    }
                }
                else
                {
                    inActionMode = true;
                    Debug.Log("Prechadzam na action mode");
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ShowVideoDisplay(bool show)
    {
        if (videoDisplayRenderer != null)
            videoDisplayRenderer.enabled = show;
        if (videoDisplayImage != null)
            videoDisplayImage.enabled = show;
    }

    private float GetCameraPitch(Transform cameraTransform)
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 levelVector = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
        Vector3 axis = Quaternion.Euler(0, 90, 0) * levelVector;

        float angle = Vector3.SignedAngle(levelVector, cameraForward, axis);
        return angle;
    }
}
