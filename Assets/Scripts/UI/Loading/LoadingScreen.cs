using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreen : MonoBehaviour
{
    
    public string NextLevel = "";

    [Space(5)]

    public bool SkipWhenLoadLevel = true;
    public bool HideLoadingWhenLoad = true;
    [Range(0.1f, 5)]
    public float SkipFadeSpeed = 1;
	[Range(0.1f, 15)]
	public float TimeForSkip;
    [Space(5)]
    public GameObject SkipPrefab = null;
    public Image Black;
    public Slider ProgressSlider = null;
    [Space(5)]
    public LoadingEffect Loading = null;


    private AsyncOperation Async = null;
    private bool IsDone = false;


    void Start()
    {
		
    	StartCoroutine(LevelProgress());
        if (!SkipWhenLoadLevel)
        {
        	InvokeRepeating("CountSkip", 1, 1);
        }
			

		if (SkipPrefab != null && SkipPrefab.activeSelf)
        {
			SkipPrefab.SetActive(false);
        }


    }
		
		
    void Update()
    {
		ProgreesLoad();
    }

    void ProgreesLoad()
    {
        if (ProgressSlider != null && Async != null)
        {
			
            float progress = (Async.progress + 0.1f);

            ProgressSlider.value = Mathf.Lerp(ProgressSlider.value, progress, Time.deltaTime * 2);

            if (Async.isDone || ProgressSlider.value >= 0.98f)
            {
                if (!IsDone)
                {
                    IsDone = true;

                    if (SkipWhenLoadLevel)
                    {
						if (SkipPrefab != null)
                        {
							SkipPrefab.SetActive(true);
                        }
                    }
                    if (HideLoadingWhenLoad) 
					{ 
						Loading.LoadingAnimation = false; 
					}
                }
            }
        }
    }

    public void Skip()
    {
        StartCoroutine(SkipIE());
    }

    IEnumerator SkipIE()
    {
		
        Color c = Black.color;
        while (c.a < 1.0f)
        {
            c.a += Time.deltaTime * SkipFadeSpeed;
            Black.color = c;
            yield return null;
        }

		Async.allowSceneActivation = true;

    }

    void CountSkip()
    {
        TimeForSkip--;
        if (TimeForSkip <= 0)
        {
            CancelInvoke("CountSkip");

			if (SkipPrefab != null)
            {
				SkipPrefab.SetActive(true);
            }
        }
    }

    private IEnumerator LevelProgress()
    {
		Async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(NextLevel);
        Async.allowSceneActivation = false;
        yield return Async;
    }
		
}