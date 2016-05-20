using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {

	Camera Cam;
	Transform Transform = null;

	public bool Show;

	// Use this for initialization
	void Start () {
	
		Cam = Camera.main;

	}
	
	// Update is called once per frame
	void Update () {
	
		if(Show)
		{
			Transform.LookAt(Cam.transform);
			Transform.position = Transform.root.position + Vector3.up;
		}

	}
}
