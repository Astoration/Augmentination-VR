using UnityEngine;
using System.Collections;

public class tree : MonoBehaviour {
	public bool isGem = false;
	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
		transform.Translate (Vector3.forward * 30f * Time.deltaTime);
		if (transform.position.z > 95f) {
			Destroy (this.gameObject);
		}
	}

	void OnCollisionStay(Collision other){
		if (other.gameObject.CompareTag ("aim")&&isGem) {
			if (Input.touchCount > 0) {
				LifeManager.instance.life += 1;
				Destroy (this.gameObject);
			}
		}
	}
}
