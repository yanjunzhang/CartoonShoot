/*
 * Author : shenjun
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;     // OpenVR

public class HandDevice : MonoBehaviour
{

    /// <summary>
    /// 手柄类型
    /// </summary>
    public HandType handType = HandType.LeftHand;

    public HandDevice otherHand = null;

    /// <summary>
    /// 控制手柄的设备
    /// </summary>
    public SteamVR_Controller.Device controller;

    /// <summary>
    /// 手柄类型设置的索引值
    /// </summary>
    uint index;

    /// <summary>
    /// 手柄模型的预设
    /// </summary>
    public GameObject controllerPrefab;

    /// <summary>
    /// 手柄模型的实例
    /// </summary>
    private GameObject controllerInstance = null;

    /// <summary>
    /// 当前手上拿着的物体
    /// </summary>
    public GameObject currentAttachObj = null;

    #region 悬停相关

    /// <summary>
    /// 当前悬停在哪个物体上
    /// </summary>
    private HandInteraction currentHoverInteraction = null;
    public HandInteraction CurrentHoverInteraction
    {
        get
        {
            return currentHoverInteraction;
        }
        set
        {
            if (currentHoverInteraction != value)
            {
                //当悬停在新物体上时，原来被悬停的物体 调用悬停结束方法
                if (currentHoverInteraction != null) currentHoverInteraction.OnHandHoverEnd(this);
            }
            currentHoverInteraction = value;
            if (currentHoverInteraction != null)
            {
                // 新的物体被悬停，调用开始悬停方法
                currentHoverInteraction.OnHandHoverBegin(this);
            }
        }
    }

    /// <summary>
    /// 悬停检测的时间间隔
    /// </summary>
    public float hoverUpdateInterval = 0.1f;
    /// <summary>
    /// 悬停检测的球形半径
    /// </summary>
    public float hoverRadius = 0.5f;
    /// <summary>
    /// 悬停检测的层 "Hover"
    /// </summary>
    private LayerMask hoverLayer;

    #endregion


    /// <summary>
    /// 手柄获得焦点时（被激活）触发的事件
    /// </summary>
    SteamVR_Events.Action inputFocusAction;

    void Awake()
    {
        inputFocusAction = SteamVR_Events.InputFocusAction(OnInputFocus);
        // 设置悬停的层
        hoverLayer = LayerMask.GetMask("Hover");
    }

    private void OnInputFocus(bool isFocus)
    {
        if (isFocus)
        {
            Debug.Log("按了手柄激活键，切换到场景状态");

            // 更新一次手柄的方位
            UpdateHandPoses();
            // 更新一次悬停的逻辑
            UpdateHovering();
        }
        else
        {
            Debug.Log("按了手柄激活键，切换到系统菜单状态");
        }
    }

    private void OnEnable()
    {
        inputFocusAction.enabled = true;

        InvokeRepeating("UpdateHovering", 0.5f, 0.5f);
    }

    private void OnDisable()
    {
        inputFocusAction.enabled = false;
    }

    IEnumerator Start()
    {

        // 每隔1秒侦测1次手柄设备是否激活，如果没有激活则一直侦测，如果激活则结束循环
        while (controller == null)
        {
            yield return new WaitForSeconds(1.0f);

            var system = OpenVR.System;
            if (system != null)
            {
                if (handType == HandType.LeftHand)
                {
                    index = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                }
                else
                {
                    index = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
                }

                // 未获得正确的索引则跳过后面的手柄设置，继续下一次循环
                if (index == OpenVR.k_unTrackedDeviceIndexInvalid) continue;

                InitController((int)index);
            }
        }
    }

    /// <summary>
    /// 根据索引初始化手柄设备
    /// </summary>
    /// <param name="index"></param>
    private void InitController(int index)
    {
        if (controller == null)
        {
            // 根据设备的索引值获取设备
            controller = SteamVR_Controller.Input(index);
            // 创建手柄模型的实例
            controllerInstance = Instantiate<GameObject>(controllerPrefab);

            AttachObjToHand(controllerInstance);
        }
        else
        {
            if (controllerInstance)
            {
                AttachObjToHand(controllerInstance);
            }
        }

    }

    void Update()
    {

        // 如果手上有拿物品，则该物品则持续调用逻辑更新
        if (currentAttachObj)
        {
            HandInteraction interaction = currentAttachObj.GetComponent<HandInteraction>();
            if (interaction)
            {
                Debug.Log("AttachedUpdate!");
                interaction.OnHandAttachedUpdate(this);
            }
        }

        // 如果当前手部正悬停在其他物品上时，则被悬停的物体持续调用被悬停的逻辑
        if (CurrentHoverInteraction)
        {
            CurrentHoverInteraction.OnHandHoverUpdate(this);
        }

        // 测试用 临时
        //InputDebug();
    }

    /// <summary>
    /// 输入控制设备 方法测试
    /// </summary>
    private void InputDebug()
    {
        if (controller == null) return;

        // 按扳机键
        if (controller.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger)
            || controller.GetPressDown(EVRButtonId.k_EButton_Axis1))
        {
            Debug.Log("按下扳机键");
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.localScale = Vector3.one * 0.1f;
            obj.transform.position = transform.position;
            Rigidbody rig = obj.AddComponent<Rigidbody>();
            rig.AddForce(new Vector3(0, 5, 5) * 0.3f);
            Destroy(obj, 2f);
        }

        //if (controller.GetTouch(EVRButtonId.k_EButton_Axis0))
        //{
        //    Debug.Log("接触了触控盘");
        //}

        if (controller.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad)
            || controller.GetPressDown(EVRButtonId.k_EButton_Axis0))
        {
            Debug.Log("按下触控盘");
        }

        // 获得触控盘轴值 轴值在半径为1的圆上
        Vector2 touchPad = controller.GetAxis();
        if (touchPad != Vector2.zero)
        {
            Debug.Log("touchPad : " + touchPad);
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.localScale = Vector3.one * 0.01f;
            obj.transform.position = new Vector3(touchPad.x, 0, touchPad.y);
        }



        if (controller.GetPressDown(EVRButtonId.k_EButton_Grip))
        {
            Debug.Log("Grip...侧面按键被按下");
            StartCoroutine(TriggerHapticPulse());
        }


    }

    IEnumerator TriggerHapticPulse()
    {
        if (controller == null) yield break;

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.02f);
            controller.TriggerHapticPulse(300, EVRButtonId.k_EButton_SteamVR_Touchpad);
        }
    }

    /// <summary>
    /// 固定帧更新手部方位
    /// </summary>
    private void FixedUpdate()
    {
        UpdateHandPoses();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, hoverRadius);
    }

    /// <summary>
    /// 更新手部位置
    /// </summary>
    void UpdateHandPoses()
    {
        if (controller != null)
        {
            SteamVR vr = SteamVR.instance;
            if (vr != null)
            {
                var pose = new TrackedDevicePose_t();
                var gamePoses = new TrackedDevicePose_t();
                var err = vr.compositor.GetLastPoseForTrackedDeviceIndex(
                    controller.index, ref pose, ref gamePoses);
                if (err == EVRCompositorError.None)
                {
                    var t = new SteamVR_Utils.RigidTransform(gamePoses.mDeviceToAbsoluteTracking);
                    transform.localPosition = t.pos;
                    transform.localRotation = t.rot;
                }
            }
        }
    }

    /// <summary>
    /// 根据球形范围检测，检测该范围内被悬停的可交互物体
    /// </summary>
    void UpdateHovering()
    {
        var cols = Physics.OverlapSphere(transform.position, hoverRadius, hoverLayer);
        if (cols.Length > 0)
        {
            HandInteraction hover = cols[0].gameObject.GetComponent<HandInteraction>();
            if (hover && CurrentHoverInteraction != hover)
            {
                CurrentHoverInteraction = hover;
            }
        }
        else
        {
            CurrentHoverInteraction = null;
        }
    }

    /// <summary>
    /// 把物体添加到手部容器
    /// </summary>
    /// <param name="obj"></param>
    public void AttachObjToHand(GameObject obj)
    {
        if (currentAttachObj == obj) return;

        // 如果新添加的游戏物体并非原来手上的，则原来手上的物体需要移除
        HandInteraction interaction = null;
        if (currentAttachObj)
        {
            interaction = currentAttachObj.GetComponent<HandInteraction>();
            if (interaction)
            {
                interaction.OnDetachFromHand(this);
            }
        }

        // 新添加的物体设置好父物体以后，调用OnAttachToHand回调方法
        currentAttachObj = obj;
        currentAttachObj.transform.parent = this.transform;
        currentAttachObj.transform.localPosition = Vector3.zero;
        currentAttachObj.transform.localRotation = Quaternion.identity;
        interaction = currentAttachObj.GetComponent<HandInteraction>();
        if (interaction)
        {
            interaction.OnAttachToHand(this);
        }
    }

    /// <summary>
    /// 从手部容器移除物体
    /// </summary>
    public void DetachObjFromHand()
    {
        if (currentAttachObj)
        {
            HandInteraction interaction = currentAttachObj.GetComponent<HandInteraction>();
            if (interaction)
            {
                interaction.OnDetachFromHand(this);
            }
            currentAttachObj = null;
        }
    }
}

/// <summary>
/// 手柄类型
/// </summary>
public enum HandType
{
    LeftHand,
    RightHand
}
