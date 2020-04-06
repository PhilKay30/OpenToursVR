using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		Debug.Log("Made it to loading");
		StartCoroutine(LoadAsyncOp());
	}

	IEnumerator LoadAsyncOp()
	{
		AsyncOperation loadLevel = SceneManager.LoadSceneAsync(2);
		while (loadLevel.progress < 1)
		{
			Debug.Log(loadLevel.progress);
			yield return new WaitForEndOfFrame();
		}

	}

}
