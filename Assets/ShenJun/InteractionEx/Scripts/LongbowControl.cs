/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class LongbowControl : HandInteraction {

    [Tooltip("猜测当前手的位置")]
    public HandType currentHandGuess = HandType.LeftHand;

    [Tooltip("箭矢定位点")]
    public Transform arrowResetPivotTrans;

    [Tooltip("手握住的位置")]
    public Transform handleTrans;

    private HandDevice hand;
    private ArrowControl arrowHand;

    [Tooltip("箭矢和弓弦匹配的位置")]
    public Transform nockTrans;

    [Tooltip("箭矢和弓弦匹配的复位位置")]
    public Transform nockResetTrans;

    [Tooltip("是否自动生成新的箭矢")]
    public bool autoSpawnArrowHand = true;

    [Tooltip("箭矢的预设")]
    public GameObject arrowHandPrefab;

    // 是否是弓箭上弦
    public bool nocked;
    // 是否开始拉弓
    public bool pulled;


    // 弓弦张开的距离
    private const float minPull = 0.05f;
    private const float maxPull = 0.5f;

    // 弓弦被拉开的距离
    private float nockDistanceTravelled = 0f;

    // 触碰距离的阈值
    private float hapticDistanceThreshold = 0.01f;

    // 上一次弓弦拉动的距离
    private float lastTickDistance;

    // 弓被拉开的震动强度插值范围
    private const float bowPullPulseStrengthMin = 100;
    private const float bowPullPulseStrengthMax = 500;

    // 弓向左的向量
    private Vector3 bowLeftVector;

    // 箭矢的速度
    private float arrowVelocityMin = 3;
    private float arrowVelocityMax = 30;
    private float arrowVelocity = 30;

    // 弓被拉紧后震动时间间隔的范围
    private float minStrainTickTime = 0.1f;
    private float maxStrainTickTime = 0.5f;
    private float nextStrainTick = 0;   // 上一次拉动弓的时间

    // 复位插值
    private bool lerpBack2ZeroRotation = false;
    private float lerpStartTime = 0f;
    private float lerpDuration = 0.15f;
    private Quaternion lerpStartRotation;

    // 弓箭弦中点的差值起始时间
    private float nockLerpStartTime = 0f;
    // 弓箭弦起始旋转
    private Quaternion nockLerpStartRotation;
    // 绘制的偏移
    //public float drawOffset = 0.06f;
    // 控制弓弦动画
    private LinearMapping bowDrawLinearMapping;

    // 弓弦拉紧的声音
    public SoundBowClick drawSound;
    // 弓弦拉紧的数值 用于控制动画的进度
    private float drawTension;
    // 弓箭抽动的声音
    public SoundPlayOneshot arrowSlideSound;
    // 发射弓箭的声音
    public SoundPlayOneshot releaseSound;
    // 弓箭上弦的声音
    public SoundPlayOneshot nockSound;

    // 位置变化
    //SteamVR_Events.Action newPosesAppliedAction;

    public override void OnAttachToHand(HandDevice hand)
    {
        this.hand = hand;
    }

    public override void OnDetachFromHand(HandDevice hand)
    {
        Destroy(this.gameObject);
    }



    void Awake()
    {
        //newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesAppliedAction);

        bowDrawLinearMapping = GetComponent<LinearMapping>();
    }

    //private void OnNewPosesAppliedAction()
    //{
    //    Debug.Log("OnNewPosesAppliedAction");
    //}

    //void OnEnable()
    //{
    //    newPosesAppliedAction.enabled = true;
    //}

    //void OnDisable()
    //{
    //    newPosesAppliedAction.enabled = false;
    //}

    public override void OnHandAttachedUpdate(HandDevice hand)
    {
        // 如果是匹配状态
        if (nocked)
        {
            // 弓弦指向手的向量
            Vector3 nock2ArrowHand = arrowHand.arrowNockTransform.parent.position - nockResetTrans.position;

            //            float pullLerp = Util.RemapNumberClamped(nock2ArrowHand.magnitude, minPull, maxPull, 0, 1);

            // 弓上的支点指向箭矢上的定位点
            Vector3 pivotToString = (arrowHand.arrowNockTransform.position - arrowResetPivotTrans.position).normalized;
            // 弓上的箭矢定位点指向握住弓的位置
            Vector3 pivotToLowerHandle = (handleTrans.position - arrowResetPivotTrans.position).normalized;

            // 根据弓和拿箭矢的手的位置 算出弓向左的方向
            bowLeftVector = -Vector3.Cross(pivotToLowerHandle, pivotToString);

            // 根据握箭矢手的位置调整弓上定位点的旋转 同时也调整了弓的旋转
            float lerp = Util.RemapNumberClamped(Time.time, nockLerpStartTime, nockLerpStartTime + lerpDuration, 0, 1);
            arrowResetPivotTrans.rotation = Quaternion.Lerp(nockLerpStartRotation, Quaternion.LookRotation(pivotToString, bowLeftVector), lerp);

            // 正向 弓被拉动
            if (Vector3.Dot(nock2ArrowHand, -nockTrans.forward) > 0)
            {
                float distance2ArrowHand = nock2ArrowHand.magnitude * lerp;

                nockTrans.localPosition = new Vector3(0f, 0f, Mathf.Clamp(-distance2ArrowHand, -maxPull, 0f));

                nockDistanceTravelled = -nockTrans.localPosition.z;

                // 根据弓拉开的距离 设置箭矢的速度
                arrowVelocity = Util.RemapNumber(nockDistanceTravelled, minPull, maxPull, arrowVelocityMin, arrowVelocityMax);

                // 设置动画的时间
                drawTension = Util.RemapNumberClamped(nockDistanceTravelled, 0, maxPull, 0f, 1f);

                this.bowDrawLinearMapping.value = drawTension; // Send drawTension value to LinearMapping script, which drives the bow draw animation

                if (nockDistanceTravelled > minPull)
                {
                    pulled = true;
                }
                else
                {
                    pulled = false;
                }

                // 弓被拉动了0.01的距离
                if ((nockDistanceTravelled > (lastTickDistance + hapticDistanceThreshold)) || nockDistanceTravelled < (lastTickDistance - hapticDistanceThreshold))
                {
                    // 根据拉动距离算出手柄震动强度
                    ushort hapticStrength = (ushort)Util.RemapNumber(nockDistanceTravelled, 0, maxPull, bowPullPulseStrengthMin, bowPullPulseStrengthMax);
                    hand.controller.TriggerHapticPulse(hapticStrength);
                    hand.otherHand.controller.TriggerHapticPulse(hapticStrength);
                    // 播放震动音效
                    drawSound.PlayBowTensionClicks(drawTension);

                    lastTickDistance = nockDistanceTravelled;
                }

                // 弓被拉紧后 持续震动
                if (nockDistanceTravelled >= maxPull)
                {
                    if (Time.time > nextStrainTick)
                    {
                        hand.controller.TriggerHapticPulse(400);
                        hand.otherHand.controller.TriggerHapticPulse(400);

                        drawSound.PlayBowTensionClicks(drawTension);

                        nextStrainTick = Time.time + Random.Range(minStrainTickTime, maxStrainTickTime);
                    }
                }
            }
            else // 反向拉动弓 则复位
            {
                nockTrans.localPosition = new Vector3(0f, 0f, 0f);

                this.bowDrawLinearMapping.value = 0f;
            }
        }
        else // 如果是没有匹配状态
        {
            // 复位
            if (lerpBack2ZeroRotation)
            {
                float lerp = Util.RemapNumber(Time.time, lerpStartTime, lerpStartTime + lerpDuration, 0, 1);

                arrowResetPivotTrans.localRotation = Quaternion.Lerp(lerpStartRotation, Quaternion.identity, lerp);

                if (lerp >= 1)
                {
                    lerpBack2ZeroRotation = false;
                }
            }
        }
    }

    // 猜测手的位置 调整弓的朝向
    private void DoHandednessCheck()
    {
        if (hand.handType == HandType.LeftHand)
        {
            currentHandGuess = HandType.LeftHand;
        }
        else
        {
            currentHandGuess = HandType.RightHand;
        }

        if (currentHandGuess == HandType.LeftHand)
        {
            arrowResetPivotTrans.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            arrowResetPivotTrans.localScale = new Vector3(1f, -1f, 1f);
        }
    }

    /// <summary>
    /// 调整弓的朝向 播放匹配音效
    /// </summary>
    public void ArrowInPosition()
    {
        DoHandednessCheck();

        if (nockSound != null)
        {
            nockSound.Play();
        }
    }

    /// <summary>
    /// 发射箭矢 播放音效
    /// </summary>
    public void ArrowReleased()
    {
        nocked = false;
        //hand.HoverUnlock(GetComponent<Interactable>());
        //hand.otherHand.HoverUnlock(arrowHand.GetComponent<Interactable>());

        if (releaseSound != null)
        {
            releaseSound.Play();
        }

        this.StartCoroutine(this.ResetDrawAnim());
    }


    /// <summary>
    /// 设置弓弦动画的播放位置
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResetDrawAnim()
    {
        float startTime = Time.time;
        float startLerp = drawTension;

        while (Time.time < (startTime + 0.02f))
        {
            float lerp = Util.RemapNumberClamped(Time.time, startTime, startTime + 0.02f, startLerp, 0f);
            this.bowDrawLinearMapping.value = lerp;
            yield return null;
        }

        this.bowDrawLinearMapping.value = 0;

        yield break;
    }

    /// <summary>
    /// 获取箭矢当前的速度
    /// </summary>
    /// <returns></returns>
    public float GetArrowVelocity()
    {
        return arrowVelocity;
    }

    /// <summary>
    /// 恢复到控制手柄的旋转
    /// </summary>
    public void StartRotationLerp()
    {
        lerpStartTime = Time.time;
        lerpBack2ZeroRotation = true;
        lerpStartRotation = arrowResetPivotTrans.localRotation;

        Util.ResetTransform(nockTrans);
    }


    /// <summary>
    /// 开始匹配 播放箭矢抽动音效
    /// </summary>
    /// <param name="currentArrowHand"></param>
    public void StartNock(ArrowControl currentArrowHand)
    {
        arrowHand = currentArrowHand;
        //hand.HoverLock(GetComponent<Interactable>());
        nocked = true;
        nockLerpStartTime = Time.time;
        nockLerpStartRotation = arrowResetPivotTrans.rotation;

        // 箭矢抽动音效
        arrowSlideSound.Play();

        // 调整弓的朝向
        DoHandednessCheck();
    }

    /// <summary>
    /// 脱离就位 弓弦动画恢复
    /// </summary>
    public void ReleaseNock()
    {
        nocked = false;
        //hand.HoverUnlock(GetComponent<Interactable>());
        this.StartCoroutine(this.ResetDrawAnim());
    }

}
