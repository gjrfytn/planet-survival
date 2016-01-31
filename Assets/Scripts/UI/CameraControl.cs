using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	
	public Transform currentMount;
	public float speedFactor = 0.1f;
	
	void  Start (){
		
	}
	void  Update (){
		transform.position = Vector3.Lerp(transform.position,currentMount.position,speedFactor);
		transform.rotation = Quaternion.Slerp(transform.rotation,currentMount.rotation,speedFactor);
	}
	
	public void  setMount ( Transform newMount  ){
		currentMount = newMount;
	}
}