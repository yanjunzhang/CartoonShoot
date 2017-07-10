/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Pickable : HandInteraction {

    public ItemPackage itemPackage;

    private Canvas canvas;

    private void Awake()
    {
        canvas = transform.GetComponentInChildren<Canvas>();
        if(canvas)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    public override void OnHandHoverBegin(HandDevice hand)
    {
        if(canvas)
        {
            canvas.gameObject.SetActive(true);
        }
    }

    public override void OnHandHoverUpdate(HandDevice hand)
    {
        if(hand && hand.controller != null)
        {
            if(hand.controller.GetHairTriggerDown())
            {
                OnAttachToHand(hand);
            }
        }
    }

    public override void OnHandHoverEnd(HandDevice hand)
    {
        if(canvas)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    public override void OnAttachToHand(HandDevice hand)
    {
        // 如果是单手武器类型的话 则移除
        if(itemPackage.packageType == ItemPackage.ItemPackageType.OneHanded)
        {
            hand.DetachObjFromHand();
        }
        else if(itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded)
        {
            hand.DetachObjFromHand();
            hand.otherHand.DetachObjFromHand();
        }

        if (itemPackage.packageType == ItemPackage.ItemPackageType.OneHanded)
        {
            GameObject oneHandObj = Instantiate(itemPackage.itemPrefab);
            oneHandObj.transform.SetParent(hand.transform);
            oneHandObj.transform.localPosition = Vector3.zero;
            oneHandObj.transform.localRotation = Quaternion.identity;
            hand.AttachObjToHand(oneHandObj);
        }
        else if (itemPackage.packageType == ItemPackage.ItemPackageType.TwoHanded)
        {
            GameObject oneHandObj = Instantiate(itemPackage.itemPrefab);
            oneHandObj.transform.SetParent(hand.transform);
            oneHandObj.transform.localPosition = Vector3.zero;
            oneHandObj.transform.localRotation = Quaternion.identity;
            hand.AttachObjToHand(oneHandObj);

            GameObject twoHandObj = Instantiate(itemPackage.otherHandItemPrefab);
            twoHandObj.transform.SetParent(hand.otherHand.transform);
            twoHandObj.transform.localPosition = Vector3.zero;
            twoHandObj.transform.localRotation = Quaternion.identity;
            hand.otherHand.AttachObjToHand(twoHandObj);
        }
    }
}
