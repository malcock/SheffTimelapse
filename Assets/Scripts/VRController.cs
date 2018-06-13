using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class  VRController : MonoBehaviour {
    public float sceneFadeTime = 2;
    public float reticleFadeTime = 1;

    public CanvasGroup reticle;
    public Image radialSelector;
    public Image fader;

    IEnumerator reticleFader;

	// Use this for initialization
	void Start () {
        StartCoroutine(FadeIn());
        FadeReticleOut();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateReticle(float percentage){
        percentage = Mathf.Round(percentage * 4);
        percentage /= 4;

        radialSelector.fillAmount = percentage;
    }

    public void LoadScene(string scene){
        StartCoroutine(FadeToScene(scene));
    }

    public void FadeReticleOut(){
        FadeReticleTo(0);
    }


    public void FadeReticleIn(){
        FadeReticleTo(1);
    }

    void FadeReticleTo(float value){
        if(reticleFader!=null) StopCoroutine(reticleFader);
        reticleFader = FadeReticle(value);
        StartCoroutine(FadeReticle(value));
    }

    IEnumerator FadeReticle(float value){
        float timeout = reticleFadeTime;
        while(timeout>0){
            float a = reticle.alpha;
            a = Mathf.Lerp(value, a, timeout / reticleFadeTime);
            reticle.alpha = a;
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeIn(){
        float timeout = sceneFadeTime;
        fader.enabled = true;
        while(timeout>0){
            Color c = fader.color;
            c.a = timeout / sceneFadeTime;
            fader.color = c;
            timeout -= Time.deltaTime;
            yield return null;
        }
        fader.enabled = false;
    }

    IEnumerator FadeToScene(string scene){
        float timeout = sceneFadeTime;
        fader.enabled = true;
        while(timeout>0){
            Color c = fader.color;
            c.a = 1- timeout / sceneFadeTime;
            fader.color = c;
            timeout -= Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(scene);
    }
}
