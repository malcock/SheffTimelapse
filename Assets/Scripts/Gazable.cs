using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VR;

public class Gazable : MonoBehaviour
{

    public UnityEvent OnGazeEnter, OnGazeExit, OnGazeStay, OnGazeComplete;
    public AudioClip EnterSound, ExitSound, CompleteSound;

    public bool isLookedAt = false;
    public float GazeTime = 3;

    public float ActivationDelay = 0;

    public bool showReticle = true;

    VRController vrController;
    IEnumerator reticleFader;
    // Use this for initialization
    void Start()
    {
        vrController = Camera.main.GetComponent<VRController>();

        //if (GetComponent<HomeTrigger>() == null)
        //{
        //    if (showReticle)
        //    {
        //        Debug.Log(name + " adding events");
        //        OnGazeExit.AddListener(vrController.FadeReticleOut);
        //        OnGazeEnter.AddListener(vrController.FadeReticleIn);
        //    }
        //}

    }

    // Update is called once per frame
    void Update()
    {
        if (ActivationDelay > 0)
        {
            ActivationDelay -= Time.deltaTime;
            return;
        }


        Ray ray;
#if UNITY_EDITOR
        ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
#else
        ray = Camera.main.ScreenPointToRay(new Vector3((float)VRSettings.eyeTextureWidth / 2, (float)VRSettings.eyeTextureHeight / 2, 0));
#endif
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                //this object is being hit by ray
                if (!isLookedAt)
                {
                    Debug.Log(name + "onGazeEnter");
                    if (OnGazeEnter != null) OnGazeEnter.Invoke();
                    isLookedAt = true;
                    if (reticleFader != null) StopCoroutine(reticleFader);
                    reticleFader = GazeCountdown();
                    StartCoroutine(reticleFader);
                }
                else
                {
                    if (OnGazeStay != null) OnGazeStay.Invoke();

                }

            }
            else
            {
                if (isLookedAt)
                {
                    isLookedAt = false;
                    Debug.Log(name + " onGazeExit");
                    if (OnGazeExit != null) OnGazeExit.Invoke();
                    if (reticleFader != null) StopCoroutine(reticleFader);
                    vrController.UpdateReticle(0);
                }

            }
        }



    }

    IEnumerator GazeCountdown()
    {

        float timeout = GazeTime;
        while (timeout > 0)
        {
            vrController.UpdateReticle(1 - (timeout / GazeTime));
            timeout -= Time.deltaTime;

            yield return null;
        }

        if (OnGazeComplete != null) OnGazeComplete.Invoke();

    }
}
