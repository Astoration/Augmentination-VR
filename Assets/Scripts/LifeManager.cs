using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LifeManager : MonoBehaviour {
	public GameObject[] lifes = new GameObject[3];
	public int life = 3;

	public static LifeManager instance;

	void Awake(){
		if (instance == null)
			instance = this;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < 3; i++) {
			if (i < life)
				lifes [i].SetActive (true);
			else
				lifes [i].SetActive (false);
		}
		if (life <= 0) {
			SceneManager.LoadScene ("TitleScene");
		}
	}
}
