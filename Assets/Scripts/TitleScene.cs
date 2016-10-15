using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time>3&&Input.touchCount > 0) {
			SceneManager.LoadScene ("InGameScene");
		}
			
	}
}
