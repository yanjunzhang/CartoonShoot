/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAction : MonoBehaviour {

    public GameObject effectPrefab;
	public GameObject bloodEffect;
    public AudioClip hitSound;
	public AudioClip shootSound;

    public float moveSpeed;

	void Start () {

        Destroy(this.gameObject, 5f);
        if (hitSound)
        {
			AudioSource.PlayClipAtPoint(shootSound, transform.position,0.8f);
        }
    }
	
	void Update () {

        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        
    }

    private void OnTriggerEnter(Collider other)
    {
		/*if (other.tag == "Enemy") {
			Destroy (this.gameObject);

			Vector3 hitPoint = other.ClosestPoint (transform.position);
			if (hitSound) {
				AudioSource.PlayClipAtPoint (hitSound, transform.position, 0.8f);
			}
			if (effectPrefab) {
				// 攻击特效
				Instantiate (effectPrefab, hitPoint, Quaternion.identity);
			}
		} else */
		if(other.tag != "Player"&&other.tag !="EnemyBody"){
			if (effectPrefab!=null) {
				GameObject effect = GameObject.Instantiate (effectPrefab,transform.position,transform.rotation);
				Destroy (effect,2f);
			}
			Destroy (this.gameObject);

		}
		if (other.tag =="EnemyBody") {
			if (bloodEffect!=null) {
				GameObject effect = GameObject.Instantiate (bloodEffect,transform.position,transform.rotation);
				Destroy (effect,2f);
			}
		}
			


    }
	void OnDestroy(){
		if (hitSound!=null) {
			AudioSource.PlayClipAtPoint(hitSound, transform.position);
		}
	}
}
