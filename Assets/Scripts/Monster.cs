using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {
	public GameObject player;
	public GameObject particle;
	// Use this for initialization
	void Start () {
		player = GameObject.Find ("player");
		MagnetSensor.OnCardboardTrigger += magnetTrigger;
	}
	public bool isMagnet=false;

	public void magnetTrigger(){
		StartCoroutine (magnetOn ());
	}

	IEnumerator magnetOn(){
		isMagnet = true;
		yield return new WaitForSeconds (0.1f);
		isMagnet = false;
	}
	// Update is called once per frame
	void Update () {
		float dy = player.transform.position.z - transform.position.z;
		float dx = player.transform.position.x - transform.position.x;
		float angle = Mathf.Atan2 (dx, dy) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler (new Vector3 (0, angle, 0));
		transform.position = Vector3.MoveTowards (transform.position, player.transform.position, Time.deltaTime*5);
		if (Vector3.Distance (transform.position, player.transform.position) < 1.5f) {
			Instantiate (particle, transform.position, Quaternion.identity);
			LifeManager.instance.life -= 1;
			Destroy (this.gameObject);
		}
	}

	void OnCollisionStay(Collision other){
		if (other.gameObject.CompareTag ("aim")) {
			if (Input.touchCount > 0) {
				Instantiate (particle, transform.position, Quaternion.identity);
				Destroy (this.gameObject);
			}else if(isMagnet){
				Instantiate (particle, transform.position, Quaternion.identity);
				Destroy (this.gameObject);
			}
		}
	}
}
