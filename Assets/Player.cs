using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public const float Speed = 0.3f;
	private Animator anime;
	// Use this for initialization
	void Start () {
		anime = GetComponent<Animator> ();
	}

	IEnumerator Attack(){
		anime.SetBool ("Attack", true);
		anime.SetBool ("Jump", false);
		anime.SetBool ("Run", false);
		yield return new WaitForSeconds (0.3f);
		anime.SetBool ("Attack", false);
	}

	// Update is called once per frame
	void Update () {
		float x = Input.GetAxis ("Horizontal");
		float y = Input.GetAxis ("Vertical");
		if(x!=0||y!=0)
			transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
		transform.Translate (new Vector3 (x * Speed, 0, y*Speed));
		if (x < 0) {
			transform.rotation = Quaternion.Euler (new Vector3 (0, -90, 0));
		} else if (0 < x) {
			transform.rotation = Quaternion.Euler (new Vector3 (0, 90, 0));
		} else if (y < 0) {
			transform.rotation = Quaternion.Euler (new Vector3 (0, 180, 0));
		} else if (0 < y) {
			transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
		}
		if (x != 0 || y != 0) {
			anime.SetBool ("Run", true);
		} else {
			anime.SetBool ("Run", false);
		}
		if (Input.GetKey (KeyCode.Space)) {
			StartCoroutine (Attack ());
		}
	}
}
