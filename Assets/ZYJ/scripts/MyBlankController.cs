using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem{
	public class MyBlankController : MonoBehaviour {
		Hand hand;
		// Use this for initialization
		void Start () {
			hand = GetComponentInParent <Hand> ();
			Invoke ("AddText",2f);
		}

		// Update is called once per frame
		void Update () {

		}
		void AddText(){
			ControllerButtonHints.ShowTextHint (hand, EVRButtonId.k_EButton_SteamVR_Trigger, "拾取你的武器", true);
		}
	}
}


