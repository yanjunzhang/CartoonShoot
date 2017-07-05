using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 手柄模型的渲染
/// </summary>
public class MyModel : MyHandInteraction {

	void Start () {
		
	}
	
	void Update () {
		
	}
    /// <summary>
    /// 渲染模型以后触发的事件
    /// </summary>
    SteamVR_Events.Action renderModelLoadedAction;
    /// <summary>
    /// 渲染手柄模型
    /// </summary>
    SteamVR_RenderModel renderModel = null;
    /// <summary>
    /// 手柄模型的容器
    /// </summary>
    MyHand hand = null;
    private void Awake()
    {
        renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoadedAction);
        
    }
    private void OnEnable()
    {
        renderModelLoadedAction.enabled = true;
        ShowController();
    }
    private void OnDisable()
    {
        renderModelLoadedAction.enabled = false;
        HideController();
    }
    /// <summary>
    /// 渲染模型以后的回调方法
    /// </summary>
    /// <param name="renderModel"></param>
    /// <param name="success"></param>
    void OnRenderModelLoadedAction(SteamVR_RenderModel renderModel,bool success)
    {
        Debug.Log("模型渲染好了");
    }
    /// <summary>
    /// 显示手柄控制器模型
    /// </summary>
    private void ShowController()
    {
        if (hand==null||hand.controller==null)
        {
            return;
        }
        //创建手柄模型
        if (renderModel == null)
        {
            renderModel = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
            renderModel.transform.parent = this.transform;
            renderModel.transform.localPosition = Vector3.zero;
            renderModel.transform.localRotation = Quaternion.identity;
        }
        renderModel.gameObject.SetActive(true);
        //设置渲染模型脚本中的设备索引值
        renderModel.SetDeviceIndex((int)hand.controller.index);
    }
    private void HideController()
    {
        if (renderModel!=null)
        {
            renderModel.gameObject.SetActive(false);
        }
    }
    #region 交互方法
    public override void OnAttachToHand(MyHand hand)
    {
        this.hand = hand;
        ShowController();
    }
    public override void OnDetachFromHand(MyHand hand)
    {
        this.hand = hand;
        HideController();
    }
    #endregion
    public override void OnHandHoverBegin(MyHand hand)
    {
        Debug.Log("onhanhoverbegin");
    }
}
