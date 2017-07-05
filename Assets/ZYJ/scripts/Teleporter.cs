using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace Valve.VR.InteractionSystem
{
    public class Teleporter : MonoBehaviour
    {
        public enum HandType
        {
            Left,
            Right,
            Any
        };
		public GameObject menu;
		public Transform headTransform;
		public Vector3 menuOffset;
        public Vector3 UIpos= new Vector3(-0.0041f, 0.1212f, 0.2516f);
        public GameObject information;
        public Text infoText;
        private Vector3 lateUpdatePos;
        private Quaternion lateUpdateRot;
        private bool deferNewPoses = false;
        private TextMesh debugText;
        private float minStrainTickTime = 0.1f;
        private Hand hand;
        public Transform pivotTransform;
        public Transform handleTransform;

        SteamVR_Events.Action newPosesAppliedAction;


        //-------------------------------------------------
        private void OnAttachedToHand(Hand attachedHand)
        {
            hand = attachedHand;
			ControllerButtonHints.HideButtonHint (this.hand,EVRButtonId.k_EButton_SteamVR_Trigger);
            ControllerButtonHints.HideTextHint(this.hand, EVRButtonId.k_EButton_SteamVR_Trigger);
			ControllerButtonHints.ShowTextHint (this.hand,EVRButtonId.k_EButton_Grip,"玩家信息",true);
			ControllerButtonHints.ShowTextHint (this.hand,EVRButtonId.k_EButton_ApplicationMenu,"功能菜单",true);
			ControllerButtonHints.ShowTextHint (this.hand,EVRButtonId.k_EButton_SteamVR_Touchpad,"传送",true);
			Invoke ("HideText",10f);
			PlayerManager.instance.FindPlayerText ();
        }
		void HideText(){
			ControllerButtonHints.HideAllTextHints (this.hand);
			ControllerButtonHints.HideAllButtonHints (this.hand);
		}


        //-------------------------------------------------
        void Awake()
        {
            newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
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
            if (deferNewPoses)
            {
                lateUpdatePos = transform.position;
                lateUpdateRot = transform.rotation;
            }

			if (menu==null) {
				menu = PlayerManager.instance.menu;
			}
			if (headTransform==null) {
				headTransform = GameObject.Find ("VRCamera").transform;
                menu.SetActive(false);
            }
        }


        //-------------------------------------------------
        private void OnNewPosesApplied()
        {
            if (deferNewPoses)
            {
                // Set longbow object back to previous pose position to avoid jitter
                transform.position = lateUpdatePos;
                transform.rotation = lateUpdateRot;

                deferNewPoses = false;
            }
        }


        //-------------------------------------------------
        private void HandAttachedUpdate(Hand hand)
        {
            // Reset transform since we cheated it right after getting poses on previous frame
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
			if (this.hand && this.hand.controller != null && this.hand.controller.GetTouch(EVRButtonId.k_EButton_Grip))
            {
				
                information.SetActive(true);
            }
            else
            {
                information.SetActive(false);
            }
			if (this.hand && this.hand.controller != null && this.hand.controller.GetTouchDown (EVRButtonId.k_EButton_ApplicationMenu)) {
				if (menu.activeInHierarchy) {
					menu.SetActive (false);
					return;
				}
				menu.SetActive (true);
				menu.transform.position = headTransform.position + headTransform.forward * 0.4f - headTransform.right * 0.3f;
				menu.transform.rotation = headTransform.rotation;

			}
			//Debug.Log (transform.position);

        }
			
        



        private void ShutDown()
        {
            
        }


        //-------------------------------------------------
        private void OnHandFocusLost(Hand hand)
        {
            gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void OnHandFocusAcquired(Hand hand)
        {
            gameObject.SetActive(true);
            OnAttachedToHand(hand);
        }


        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            Destroy(gameObject);
        }


        //-------------------------------------------------
        void OnDestroy()
        {
            ShutDown();
        }
    }
}

