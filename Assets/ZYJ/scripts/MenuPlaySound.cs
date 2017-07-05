using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlaySound : MonoBehaviour {
	public AudioClip buttonSound;
	// Use this for initialization
	void Start () {
        if (this.gameObject.name=="music")
        {
            AudioSource.PlayClipAtPoint(buttonSound, this.transform.position);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void PlaySound(){
		AudioSource.PlayClipAtPoint (buttonSound,this.transform.position);
	}
}
