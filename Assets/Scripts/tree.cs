using UnityEngine;
using System.Collections;

public class tree : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * 30f * Time.deltaTime);
		if (transform.position.z > 95f) {
			Destroy (this.gameObject);
		}
	}
}
