/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ArrowControl : HandInteraction {

    private HandDevice hand;
    private LongbowControl bow;

    [Tooltip("箭矢预设")]
    public GameObject arrowPrefab;
    // 当前箭矢
    private GameObject currentArrow;
    [Tooltip("箭矢发射初始位置")]
    public Transform arrowNockTransform;

    // 弓箭就位的距离
    public float nockDistance = 0.1f;
    // 插值的全部距离   小于该距离认为是插值完成
    public float lerpCompleteDistance = 0.08f;
    // 开始旋转插值的距离阈值  
    public float rotationLerpThreshold = 0.15f;
    // 位置插值的阈值
    public float positionLerpThreshold = 0.15f;
    // 是否允许生成箭矢
    private bool allowArrowSpawn = true;
    // 是否与弓弦匹配
    public bool nocked;
    // 是否在匹配范围内
    private bool inNockRange = false;
    // 箭矢是否插值完成
    private bool arrowLerpComplete = false;
    // 生成箭矢的声音
    public SoundPlayOneshot arrowSpawnSound;

    //private List<GameObject> arrowList;


    void Awake()
    {
        //arrowList = new List<GameObject>();
    }

    public override void OnAttachToHand(HandDevice hand)
    {
        this.hand = hand;
        FindBow();
    }

    /// <summary>
    /// 获取弓
    /// </summary>
    private bool FindBow()
    {
        if (bow != null) return true;

        bow = hand.otherHand.GetComponentInChildren<LongbowControl>();
        return bow != null;
    }

    /// <summary>
    /// 实例化箭矢 并设置父物体
    /// </summary>
    private GameObject InstantiateArrow()
    {
        var arrow = Instantiate<GameObject>(arrowPrefab, arrowNockTransform.position, arrowNockTransform.rotation);
        arrow.name = "arrow";
        arrow.transform.parent = arrowNockTransform;
        Util.ResetTransform(arrow.transform);

        //arrowList.Add(arrow);
        return arrow;
    }

    /// <summary>
    /// 匹配箭矢与弓弦的位置
    /// </summary>
    /// <param name="hand"></param>
    public override void OnHandAttachedUpdate(HandDevice hand)
    {
        if (!FindBow()) return;

        if (allowArrowSpawn && (currentArrow == null))
        {
            currentArrow = InstantiateArrow();
            arrowSpawnSound.Play();
        }

        // 手部和匹配点的距离
        float distanceToNockPosition = Vector3.Distance(transform.parent.position, bow.nockTrans.position);

        // 如果手上没有箭矢并且也没有匹配
        if (!nocked)
        {
            // 如果距离小于开始 旋转插值 的阈值 则旋转插值
            if (distanceToNockPosition < rotationLerpThreshold)
            {
                float lerp = Util.RemapNumber(distanceToNockPosition, rotationLerpThreshold, lerpCompleteDistance, 0, 1);
                arrowNockTransform.rotation = Quaternion.Lerp(arrowNockTransform.parent.rotation, bow.nockResetTrans.rotation, lerp);
            }
            else
            {
                arrowNockTransform.localRotation = Quaternion.identity;
            }

            // 如果距离小于开始 位移插值的阈值 则开始位移插值
            if (distanceToNockPosition < positionLerpThreshold)
            {
                float posLerp = Util.RemapNumber(distanceToNockPosition, positionLerpThreshold, lerpCompleteDistance, 0, 1);
                posLerp = Mathf.Clamp(posLerp, 0f, 1f);

                arrowNockTransform.position = Vector3.Lerp(arrowNockTransform.parent.position, bow.nockResetTrans.position, posLerp);
            }
            else
            {
                arrowNockTransform.position = arrowNockTransform.parent.position;
            }

            // 插值完成 弓箭就位
            if (distanceToNockPosition < lerpCompleteDistance)
            {
                if (!arrowLerpComplete)
                {
                    arrowLerpComplete = true;
                    hand.controller.TriggerHapticPulse(500);
                }
            }
            else
            {
                if (arrowLerpComplete)
                {
                    arrowLerpComplete = false;
                }
            }



            // 当手足够近了 则可以匹配弓箭
            if (distanceToNockPosition < nockDistance)
            {
                if (!inNockRange)
                {
                    inNockRange = true;
                    bow.ArrowInPosition();
                }
            }
            else
            {
                if (inNockRange)
                {
                    inNockRange = false;
                }
            }

            // 开始拉弓
            if (inNockRange && hand.controller.GetHairTrigger() && !nocked)
            {
                if (currentArrow == null)
                {
                    currentArrow = InstantiateArrow();
                }

                nocked = true;
                bow.StartNock(this);
                //hand.HoverLock(GetComponent<Interactable>());
                //allowTeleport.teleportAllowed = false;

                // 将箭矢做为弓弦的就位物体的子物体
                currentArrow.transform.parent = bow.nockTrans;
                Util.ResetTransform(currentArrow.transform);    // 箭矢改变父物体后 箭矢复位
                Util.ResetTransform(arrowNockTransform);        // 箭矢的原来存放的父物体 复位
            }
        }


        // 放箭
        if (nocked && (!hand.controller.GetHairTrigger() || hand.controller.GetHairTriggerUp()))
        {
            if (bow.pulled) // 如果弓弦拉的足够开，否则箭矢复位
            {
                FireArrow();
            }
            else
            {
                arrowNockTransform.rotation = currentArrow.transform.rotation;
                currentArrow.transform.parent = arrowNockTransform;
                Util.ResetTransform(currentArrow.transform);
                nocked = false;
                bow.ReleaseNock();
                //hand.HoverUnlock(GetComponent<Interactable>());
                //allowTeleport.teleportAllowed = true;
            }

            // 箭矢离开弓 弓恢复到控制手柄的旋转
            bow.StartRotationLerp();
        }

    }

    /// <summary>
    /// 放箭
    /// </summary>
    private void FireArrow()
    {
        // 将当前弓箭的父物体取消
        currentArrow.transform.parent = null;

        ArrowAction arrow = currentArrow.GetComponent<ArrowAction>();
        arrow.arrowRigidbody.isKinematic = false;
        arrow.arrowRigidbody.useGravity = true;
        arrow.arrowRigidbody.transform.GetComponent<BoxCollider>().enabled = true;

        arrow.arrowHeadRigidbody.isKinematic = false;
        arrow.arrowHeadRigidbody.useGravity = true;
        arrow.arrowHeadRigidbody.transform.GetComponent<BoxCollider>().enabled = true;

        arrow.arrowHeadRigidbody.AddForce(currentArrow.transform.forward * bow.GetArrowVelocity(), ForceMode.VelocityChange);
        arrow.arrowHeadRigidbody.AddTorque(currentArrow.transform.forward * 10);

        arrow.ArrowReleased();
        bow.ArrowReleased();

        nocked = false;


        allowArrowSpawn = false;
        // 过0.5秒后生成新的箭矢
        Invoke("EnableArrowSpawn", 0.5f);
        // 让拿弓的手震动几下
        StartCoroutine(ArrowReleaseHaptics());

        currentArrow = null;
        //allowTeleport.teleportAllowed = true;
    }

    private void EnableArrowSpawn()
    {
        allowArrowSpawn = true;
    }

    /// <summary>
    /// 拿弓的手震动几下
    /// </summary>
    /// <returns></returns>
    private IEnumerator ArrowReleaseHaptics()
    {
        yield return new WaitForSeconds(0.05f);

        hand.otherHand.controller.TriggerHapticPulse(1500);
        yield return new WaitForSeconds(0.05f);

        hand.otherHand.controller.TriggerHapticPulse(800);
        yield return new WaitForSeconds(0.05f);

        hand.otherHand.controller.TriggerHapticPulse(500);
        yield return new WaitForSeconds(0.05f);

        hand.otherHand.controller.TriggerHapticPulse(300);
    }

    public override void OnDetachFromHand(HandDevice hand)
    {
        Destroy(this.gameObject);
    }
}
