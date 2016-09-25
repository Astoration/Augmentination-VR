using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
	public const float Speed = 1.5f;
	private Animator anime;
	private Rigidbody rig;
	private bool isFirst = true;
	private bool isJump = false;
	public GameObject Target;
	private Queue<Vector3> root;
	public GameObject myCamera;
	// Use this for initialization
	void Start () {
		anime = GetComponent<Animator> ();
		rig = GetComponent<Rigidbody> ();
		root = new Queue<Vector3> ();
		root.Enqueue (Target.transform.position);
	}

	IEnumerator Jump(){
		isJump = true;
		anime.SetBool ("Attack", false);
		anime.SetBool ("Jump", true);
		anime.SetBool ("Run", false);
		rig.AddForce (Vector3.up * 60);
		yield return new WaitForSeconds (0.3f);
		anime.SetBool ("Jump", false);
		isJump = false;
	}

	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * Speed * Time.deltaTime);
		if(Input.GetKeyDown(KeyCode.Space)&&!isJump)
			StartCoroutine(Jump());
		Vector3 targetpos = Target.transform.position;
		targetpos.x = transform.position.x-2;
		Target.transform.position = targetpos;
		Target.transform.Translate(new Vector3(Input.GetAxis("Horizontal")*2f*Time.deltaTime,0,0));
		root.Enqueue (Target.transform.position);
		while (transform.position.x < root.Peek().x)
			root.Dequeue ();
		Vector3 target = root.Peek ();
		float dy = transform.position.z-target.z;
		float dx = transform.position.x - target.x;
		float angle = Mathf.Atan2 (dx, dy) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(new Vector3(0,angle-180,0));
		Vector3 pos = myCamera.transform.position;
		pos.x = transform.position.x+3;
		pos.z = transform.position.z;
		myCamera.transform.position = pos;


	}
}
