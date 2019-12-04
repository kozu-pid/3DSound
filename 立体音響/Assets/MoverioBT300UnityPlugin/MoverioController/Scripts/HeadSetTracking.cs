using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadSetTracking : MonoBehaviour {

	void Start () {
		
		Input.gyro.enabled = true;

	}

	void Update () {
		
		this.gameObject.transform.rotation = Quaternion.AngleAxis (90.0f, Vector3.right) * Input.gyro.attitude * Quaternion.AngleAxis (180.0f, Vector3.forward);

	}
}
