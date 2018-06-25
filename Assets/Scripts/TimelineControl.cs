using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;

[RequireComponent(typeof(Gazable))]
public class TimelineControl : MonoBehaviour
{

    public enum TimeType {HHMM,Percent,VidTime}

    public TimeType timeMode=TimeType.HHMM;

    public Image scrubMarker, timelineFull;

    public CanvasGroup returnPad, timeRotation;

    public Text currentTime;

    public float scrubSeekMultiplier = 5;

    MediaPlayer PlayingPlayer;

    bool isGazeComplete = false;

    public float scrubTime = 2f;

    float targetTime = 0;

    //key things
    public float minAngle = -127.5f, maxAngle = 126f;
    public int startHour = 5, endHour = 21;

    //Control for the return pad anims
    public float returnPadMinAlpha =0.5f, returnPadMaxAlpha=1f, returnPadTransitionTime = 1f;
    IEnumerator returnPadAnim,scrubber;

    Camera cam;
    float headY = 0;

    public float targ;

    float totalAngle;

    Gazable gazable;



    // Use this for initialization
    void Start()
    {
        cam = Camera.main;
        gazable = GetComponent<Gazable>();
        PlayingPlayer = FindObjectOfType<MediaPlayer>();

    }

    // Update is called once per frame
    void Update()
    {
        totalAngle = 288;
        float fillVisible = 0.8f;
        float startFill =0.1f;
        float stopFill = 0.9f;


        if (PlayingPlayer && PlayingPlayer.Info != null && PlayingPlayer.Info.GetDurationMs() > 0f)
        {
            float time = PlayingPlayer.Control.GetCurrentTimeMs();
            float duration = PlayingPlayer.Info.GetDurationMs();
            float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);

            // Debug.Log(string.Format("time: {0}, duration: {1}, d: {2}", time, duration, d));
            timelineFull.fillAmount = startFill + (d*fillVisible);
            timeRotation.transform.rotation = Quaternion.Euler(new Vector3(90,0,(180-minAngle) - (d*255)));
            //_setVideoSeekSliderValue = d;
            //_videoSeekSlider.value = d;
            string timeString = string.Empty;
            switch(timeMode){
                case TimeType.VidTime:
                    timeString = Mathf.RoundToInt(time * 0.001f) + "/" + Mathf.RoundToInt(duration * 0.001f);
                    break;
                case TimeType.HHMM:
                    string[] mapped = (MapRange(d, 0, 1, startHour, endHour)).ToString("00.##").Split('.');
                    string min = "00";
                    if(mapped.Length>1){
                        min = MapRange(float.Parse(mapped[1]), 0, 99, 0, 59).ToString("00");
                    }

                    timeString = mapped[0] + ":" + min;
                    break;
                case TimeType.Percent:
                    timeString = (d * 100).ToString();
                    break;
                default:
                    break;
            }
            currentTime.text = timeString;
        }

        headY = cam.transform.eulerAngles.y;

        returnPad.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180 - headY));


    }

    public float MapRange(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax){
        return (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
            
    }

    public void GazeLeave()
    {
        if(isGazeComplete){
            if (scrubber != null) StopCoroutine(scrubber);
            scrubber = Scrub();
            StartCoroutine(scrubber);
            isGazeComplete = false;
        }
    }

    IEnumerator Scrub(){
        float time = PlayingPlayer.Control.GetCurrentTimeMs();
        float duration = PlayingPlayer.Info.GetDurationMs();
        float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);
        Debug.Log("Scrubbing to " + targetTime);
        float t = scrubTime;
        while (t > 0)
        {
            float percent = Mathf.Lerp(d, targetTime, 1 - (t / scrubTime));
            PlayingPlayer.Control.Seek(percent * PlayingPlayer.Info.GetDurationMs());

            t -= Time.deltaTime;
            yield return null;
        }
    }


    public void GazeComplete(){
        isGazeComplete = true;
    }


    public void GazeEnter(){
        isGazeComplete = true;
    }

    public void GazeStay()
    {
        targ = headY;


        if (targ < 180 + minAngle)
        {
            targ = 180 + minAngle;
        }
        if (targ > 180 + maxAngle)
        {
            targ = 180 + maxAngle;
        }


        scrubMarker.transform.localRotation = Quaternion.Slerp(scrubMarker.transform.localRotation, Quaternion.Euler(new Vector3(0, 0, 180 - targ)), Time.deltaTime * scrubSeekMultiplier);


        targetTime = MapRange(targ, 180+minAngle, 180+maxAngle, 0f, 1f);

        float time = PlayingPlayer.Control.GetCurrentTimeMs();
        float duration = PlayingPlayer.Info.GetDurationMs();
        float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);

        if (targetTime < d)
        {
            scrubMarker.color = Color.black;
        } else {
            scrubMarker.color = Color.white;
        }
    }

    public void ShowReturn(){
        if(returnPadAnim!=null)
            StopCoroutine(returnPadAnim);
        returnPadAnim = FadeGroup(returnPad, returnPadMaxAlpha, returnPadTransitionTime);
        StartCoroutine(returnPadAnim);

    }

    public void HideReturn(){
        if(returnPadAnim != null)
            StopCoroutine(returnPadAnim);
        returnPadAnim = FadeGroup(returnPad, returnPadMinAlpha, returnPadTransitionTime);
        StartCoroutine(returnPadAnim);
    }

    IEnumerator FadeGroup(CanvasGroup grp,float alpha,float timeout){
        float startAlpha = grp.alpha;
        float t = timeout;
        while(t>0){
            grp.alpha = Mathf.Lerp(startAlpha, alpha, 1 - (t / timeout));
            t -= Time.deltaTime;
            yield return null;
        }
    }

}
