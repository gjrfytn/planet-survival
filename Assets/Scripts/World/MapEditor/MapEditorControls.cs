using UnityEngine;
using System.Collections;

public class MapEditorControls : MonoBehaviour {


	public float Speed = 2f;

	public KeyCode Right, Left, Up, Down;


	// Use this for initialization
	void Start () {


	}

	
	// Update is called once per frame
	void Update () {

		if (Input.GetKey(Right)){
			transform.position += Vector3.right * Speed * Time.deltaTime;
		}
		if (Input.GetKey(Left)){
			transform.position += Vector3.left * Speed * Time.deltaTime;
		}
		if (Input.GetKey(Up)){
			transform.position += Vector3.up * Speed * Time.deltaTime;
		}
		if (Input.GetKey(Down)){
			transform.position += Vector3.down * Speed * Time.deltaTime;
		}

	}
}
