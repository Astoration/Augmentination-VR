using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour {
	public GameObject[] monsters = new GameObject[5];

	public float time = 10f;
	// Use this for initialization
	IEnumerator spawn(){
		while (true) {
			Instantiate (monsters [Random.Range (0, monsters.Length)], new Vector3 (50f * (Random.Range (0, 2) == 0 ? -1 : 1), 0, Random.Range(-95f,95f)), Quaternion.identity);
			yield return new WaitForSeconds (time);
		}
	}
	void Start () {
		StartCoroutine (spawn ());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
