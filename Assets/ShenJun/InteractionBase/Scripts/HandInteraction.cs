/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteraction : MonoBehaviour {

	protected virtual void Start() { }
    protected virtual void Update() { }

    /// <summary>
    /// 物体附加到手部容器时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnAttachToHand(HandDevice hand) { }
    /// <summary>
    /// 物体脱离手部容器时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnDetachFromHand(HandDevice hand) { }
    /// <summary>
    /// 拿到手上的物体 持续更新
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandAttachedUpdate(HandDevice hand) { }

    /// <summary>
    /// 当手部接触到可交互物体上时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandHoverBegin(HandDevice hand) { }
    /// <summary>
    /// 当手部脱离可交互物体时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandHoverEnd(HandDevice hand) { }
    /// <summary>
    /// 当手部持续与可交互物体接触时 持续更新
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandHoverUpdate(HandDevice hand) { }
}
