using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCtrl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider col){
		
		if (col.tag=="EnemyWeapon"&& PlayerManager.instance.hp>0) {
			PlayerManager.instance.hp -= Random.value > 0.75f ? 1 : 0;
			//Debug.Log ("behit");
		}
	}
}
