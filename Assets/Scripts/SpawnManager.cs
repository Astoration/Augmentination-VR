using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour {
	public GameObject[] monsters = new GameObject[5];
	public GameObject item;

	public float time = 10f;
	// Use this for initialization
	IEnumerator spawn(){
		while (true) {
			Instantiate (monsters [Random.Range (0, monsters.Length)], new Vector3 (50f * (Random.Range (0, 2) == 0 ? -1 : 1), 0, Random.Range(-95f,95f)), Quaternion.identity);
			yield return new WaitForSeconds (time);
		}
	}
	IEnumerator itemSpawn(){
		while (true) {
			Instantiate (item, new Vector3 (10f * (Random.Range (0, 2) == 0 ? -1 : 1), 1.37f, Random.Range(-95f,95f)), Quaternion.identity);
			yield return new WaitForSeconds (time*4);
		}
	}
	void Start () {
		StartCoroutine (spawn ());
		StartCoroutine (itemSpawn ());

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
