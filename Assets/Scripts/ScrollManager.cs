using UnityEngine;
using System.Collections;

public class ScrollManager : MonoBehaviour {
	public GameObject[] plane = new GameObject[2];
	private Material[] materials = new Material[2];

	public GameObject tree;
	public GameObject spawnPoint;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < 2; i++) {
			materials [i] = plane [i].GetComponent<Renderer> ().material;
		}
	}
	float time = 0;
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		for (int i = 0; i < 2; i++) {
			materials [i].mainTextureOffset = new Vector2 (0,-time);
		}
		if (((int)(time * 10)) % 5 == 0) {
			Vector3 randomPos = spawnPoint.transform.position;
			randomPos.x += Random.Range (8f, 150f) * (Random.Range (0, 2) == 0 ? -1 : 1);
			((GameObject)Instantiate (tree, randomPos, Quaternion.identity)).transform.localScale = tree.transform.localScale;
		}
	}
}
