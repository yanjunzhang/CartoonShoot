using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyHandInteraction : MonoBehaviour {


	void Start () {
		
	}
	
	void Update () {
		
	}
    /// <summary>
    /// 物体附加到手部容器时
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnAttachToHand(MyHand hand) { }
    /// <summary>
    /// 物体脱离手部容器时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnDetachFromHand(MyHand hand) { }
    /// <summary>
    /// 拿到手上物体 持续更新
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandAttachedUpdate(MyHand hand) { }
    /// <summary>
    /// 当手部接触到可交互物体上时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandHoverBegin(MyHand hand) { }
    /// <summary>
    /// 当手部脱离可交互物体时调用
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandHoverEnd(MyHand hand) { }
    /// <summary>
    /// 当手部持续与可交互物体接触时 持续更新
    /// </summary>
    /// <param name="hand"></param>
    public virtual void OnHandHoverUpdate(MyHand hand) { }


}
