//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: The bow
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class MyGun : MonoBehaviour
	{
		public enum Handedness { Left, Right };
        public GameObject bulletPrefab;
        public Transform firePos;
		public Transform laserPos;
		public float coldTime=0.01f;
		private float currTime=0;
		public int remainBulletNum;
		private int currBulletNum;
		public GameObject information;
		public AudioClip reloadAudio;
		private Animator ani;
		SteamVR_LaserPointer laser;

        [HideInInspector]
        public Handedness currentHandGuess = Handedness.Left;
        private float timeOfPossibleHandSwitch = 0f;
        private float timeBeforeConfirmingHandSwitch = 1.5f;
        private bool possibleHandSwitch = false;
        private Hand hand;
		private ArrowHand arrowHand;        
		private const float minPull = 0.05f;
		private const float maxPull = 0.5f;
		private float nockDistanceTravelled = 0f;
		private float hapticDistanceThreshold = 0.01f;
		private float lastTickDistance;
		private const float bowPullPulseStrengthLow = 100;
		private const float bowPullPulseStrengthHigh = 500;
		private Vector3 bowLeftVector;
		private float arrowVelocity = 30f;
		private float minStrainTickTime = 0.1f;
		private float maxStrainTickTime = 0.5f;
		private float nextStrainTick = 0;
		private bool lerpBackToZeroRotation;
		private float lerpStartTime;
		private float lerpDuration = 0.15f;
		private Quaternion lerpStartRotation;
		private float nockLerpStartTime;
		private Quaternion nockLerpStartRotation;
		private bool deferNewPoses = false;
		private Vector3 lateUpdatePos;
		private Quaternion lateUpdateRot;
		private float drawTension;


		SteamVR_Events.Action newPosesAppliedAction;


		//-------------------------------------------------
		private void OnAttachedToHand( Hand attachedHand )
		{
			hand = attachedHand;
			ControllerButtonHints.ShowTextHint (this.hand,EVRButtonId.k_EButton_Grip,"更换弹夹",true);
			ControllerButtonHints.ShowTextHint (this.hand,EVRButtonId.k_EButton_Axis0,"红外线",true);
			ControllerButtonHints.ShowTextHint (this.hand,EVRButtonId.k_EButton_SteamVR_Trigger,"射击",true);
			Invoke ("HideText",10f);

		}
		void HideText(){
			ControllerButtonHints.HideAllTextHints (this.hand);
			ControllerButtonHints.HideAllButtonHints (this.hand);
		}

		//-------------------------------------------------
		void Awake()
		{
			newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction( OnNewPosesApplied );
			ani = GetComponent <Animator> ();
            //InvokeRepeating("HandAttachedUpdate", 0.5f, 0.5f);
        }


		//-------------------------------------------------
		void OnEnable()
		{
			newPosesAppliedAction.enabled = true;
		}


		//-------------------------------------------------
		void OnDisable()
		{
			newPosesAppliedAction.enabled = false;
		}


		//-------------------------------------------------
		void LateUpdate()
		{
			if ( deferNewPoses )
			{
				lateUpdatePos = transform.position;
				lateUpdateRot = transform.rotation;
			}

		}


		//-------------------------------------------------
		private void OnNewPosesApplied()
		{
			if ( deferNewPoses )
			{
				// Set longbow object back to previous pose position to avoid jitter
				transform.position = lateUpdatePos;
				transform.rotation = lateUpdateRot;

				deferNewPoses = false;
			}
		}


		//-------------------------------------------------
		private void HandAttachedUpdate( Hand hand )
		{
			currTime += Time.deltaTime;
			// Reset transform since we cheated it right after getting poses on previous frame
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			if (this.hand && this.hand.controller != null && this.hand.controller.GetHairTrigger ())
			{
				/*Instantiate(bulletPrefab, firePos.position, firePos.rotation);
                StartCoroutine(FirePules());*/
				//shoot
				if (currTime>=coldTime&&PlayerManager.instance.currBulletNum > 0) {
					Instantiate(bulletPrefab, firePos.position, firePos.rotation);
					StartCoroutine(FirePules());
					PlayerManager.instance.currBulletNum--;
					currTime = 0;
				}
				ani.SetBool ("isAttack",true);

			}
			if (this.hand.controller.GetHairTriggerUp ()) {
				ani.SetBool ("isAttack",false);
			}
			if (this.hand && this.hand.controller != null && this.hand.controller.GetPressDown (SteamVR_Controller.ButtonMask.Touchpad)) {
				if (laser==null) {
					laser = new GameObject ("laser").AddComponent <SteamVR_LaserPointer> ();
					laser.transform.SetParent (laserPos);
					laser.transform.localPosition = Vector3.zero;
					laser.transform.localRotation = Quaternion.identity;
					laser.color = Color.red;
					return;
				}
				if (laser.gameObject.activeInHierarchy) {
					laser.gameObject.SetActive (false);
				}else
					laser.gameObject.SetActive (true);

			}/*else if (this.hand.controller.GetTouchUp (EVRButtonId.k_EButton_SteamVR_Touchpad)) {
				laser.gameObject.SetActive (false);
			}*/

			//reload
			if (this.hand && this.hand.controller != null && this.hand.controller.GetTouchDown (EVRButtonId.k_EButton_Grip)) {
				if (PlayerManager.instance.remainBulletNum>0) {
					ani.SetTrigger ("reload");
					AudioSource.PlayClipAtPoint (reloadAudio, transform.position,0.8f);
					Reload (ref PlayerManager.instance.currBulletNum,ref PlayerManager.instance.remainBulletNum);
				}

			}


            

		}


		void Reload(ref int curr,ref int remain){
			if (curr+remain>=30) {
				remain = curr + remain - 30;
				curr = 30;

			}else if (curr+remain<30) {
				curr = remain + curr;
				remain = 0;
			}
		}



		IEnumerator FirePules()
        {
            for (int i = 0; i < 5; i++)
            {
                ushort temp = (ushort)Random.Range(200, 2000);
                hand.controller.TriggerHapticPulse(temp);
                yield return new WaitForFixedUpdate();
            }
        }
		private void ShutDown()
		{
			if ( hand != null && hand.otherHand.currentAttachedObject != null )
			{
				if ( hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>() != null )
				{
					
				}
			}
		}


		//-------------------------------------------------
		private void OnHandFocusLost( Hand hand )
		{
			gameObject.SetActive( false );
		}


		//-------------------------------------------------
		private void OnHandFocusAcquired( Hand hand )
		{
			gameObject.SetActive( true );
			OnAttachedToHand( hand );
		}


		//-------------------------------------------------
		private void OnDetachedFromHand( Hand hand )
		{
			Destroy( gameObject );
		}


		//-------------------------------------------------
		void OnDestroy()
		{
			ShutDown();
		}

	}
}
