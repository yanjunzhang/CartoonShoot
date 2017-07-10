/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControl : HandInteraction
{

    public GameObject bulletPrefab;

    Transform firePos;
    HandDevice hand;

    private void Awake()
    {
        firePos = transform.Find("FirePos");
    }

    public override void OnAttachToHand(HandDevice hand)
    {
        this.hand = hand;
        Debug.Log("获得来福枪");
    }

    public override void OnDetachFromHand(HandDevice hand)
    {
        this.hand = null;
        Destroy(this.gameObject);
    }

    public override void OnHandAttachedUpdate(HandDevice hand)
    {

        if(this.hand && this.hand.controller != null && this.hand.controller.GetHairTriggerDown())
        {
            Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            StartCoroutine(FirePulse());
        }
    }

    IEnumerator FirePulse()
    {
        for (int i = 0; i < 5; i++)
        {
            ushort tmp = (ushort)Random.Range(800, 2000);
            hand.controller.TriggerHapticPulse(tmp);
            yield return new WaitForSeconds(0.02f);
        }
    }
}
