/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ArrowAction : MonoBehaviour {

    [Tooltip("箭头闪光粒子特效")]
    public ParticleSystem glintParticle;
    [Tooltip("箭头上的刚体")]
    public Rigidbody arrowHeadRigidbody;
    [Tooltip("箭矢本身的刚体")]
    public Rigidbody arrowRigidbody;

    [Tooltip("物理材质")]
    public PhysicMaterial targetPhysMaterial;

    // 预测的位置
    private Vector3 prevPosition;
    // 预测的旋转
    private Quaternion prevRotation;
    // 预测的速度
    private Vector3 prevVelocity;
    // 预测的头部的位置
    private Vector3 prevHeadPosition;

    [Tooltip("发射箭矢的音效")]
    public SoundPlayOneshot fireReleaseSound;
    [Tooltip("发射箭矢的空气音效")]
    public SoundPlayOneshot airReleaseSound;
    [Tooltip("击中目标时的音效")]
    public SoundPlayOneshot hitTargetSound;
    [Tooltip("击中地面的音效")]
    public PlaySound hitGroundSound;
    // 是否在飞行
    private bool inFlight;
    // 释放
    private bool released;
    // 飞行的帧数
    private int travelledFrames = 0;
    // 缩放的父物体
    private GameObject scaleParentObject = null;



    void FixedUpdate()
    {
        if (released && inFlight)
        {
            prevPosition = transform.position;
            prevRotation = transform.rotation;
            prevVelocity = GetComponent<Rigidbody>().velocity;
            prevHeadPosition = arrowHeadRigidbody.transform.position;
            travelledFrames++;
        }
    }


    /// <summary>
    /// 释放箭矢 播放音效 粒子特效
    /// </summary>
    /// <param name="inputVelocity"></param>
    public void ArrowReleased()
    {
        inFlight = true;
        released = true;

        airReleaseSound.Play();

        if (glintParticle != null)
        {
            glintParticle.Play();
        }

        //if (gameObject.GetComponentInChildren<FireSource>().isBurning)
        //{
        //    fireReleaseSound.Play();
        //}

        // 碰撞检测 避免一开始就插入到碰撞体
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.01f, transform.forward, 0.80f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != gameObject && hit.collider.gameObject != arrowHeadRigidbody.gameObject)
            {
                // 销毁箭矢
                Destroy(gameObject);
                return;
            }
        }

        travelledFrames = 0;
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        prevHeadPosition = arrowHeadRigidbody.transform.position;
        prevVelocity = GetComponent<Rigidbody>().velocity;

        // 30秒后销毁箭矢
        Destroy(gameObject, 30);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (inFlight)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            float rbSpeed = rb.velocity.sqrMagnitude;
            // 判断是否可以钉住
            bool canStick = (targetPhysMaterial != null && collision.collider.sharedMaterial == targetPhysMaterial && rbSpeed > 0.2f);


            //bool hitBalloon = collision.collider.gameObject.GetComponent<Balloon>() != null;

            // 不能钉住的处理
            if (travelledFrames < 2 && !canStick)
            {
                // Reset transform but halve your velocity
                transform.position = prevPosition - prevVelocity * Time.deltaTime;
                transform.rotation = prevRotation;

                // 计算反射方向
                Vector3 reflfectDir = Vector3.Reflect(arrowHeadRigidbody.velocity, collision.contacts[0].normal);
                arrowHeadRigidbody.velocity = reflfectDir * 0.25f;
                arrowRigidbody.velocity = reflfectDir * 0.25f;

                travelledFrames = 0;
                return;
            }

            // 粒子效果停止
            if (glintParticle != null)
            {
                glintParticle.Stop(true);
            }

            // 速度大于0.1则播放碰撞地面的声音
            if (rbSpeed > 0.1f)
            {
                hitGroundSound.Play();
            }

            //FireSource arrowFire = gameObject.GetComponentInChildren<FireSource>();
            //FireSource fireSourceOnTarget = collision.collider.GetComponentInParent<FireSource>();

            //if (arrowFire != null && arrowFire.isBurning && (fireSourceOnTarget != null))
            //{
            //    if (!hasSpreadFire)
            //    {
            //        collision.collider.gameObject.SendMessageUpwards("FireExposure", gameObject, SendMessageOptions.DontRequireReceiver);
            //        hasSpreadFire = true;
            //    }
            //}
            //else
            //{
            //    // Only count collisions with good speed so that arrows on the ground can't deal damage
            //    // always pop balloons
            //    if (rbSpeed > 0.1f || hitBalloon)
            //    {
            //        collision.collider.gameObject.SendMessageUpwards("ApplyDamage", SendMessageOptions.DontRequireReceiver);
            //        gameObject.SendMessage("HasAppliedDamage", SendMessageOptions.DontRequireReceiver);
            //    }
            //}

            //if (hitBalloon)
            //{
            //    // Revert my physics properties cause I don't want balloons to influence my travel
            //    transform.position = prevPosition;
            //    transform.rotation = prevRotation;
            //    arrowHeadRigidbody.velocity = prevVelocity;
            //    Physics.IgnoreCollision(arrowHeadRigidbody.GetComponent<Collider>(), collision.collider);
            //    Physics.IgnoreCollision(arrowRigidbody.GetComponent<Collider>(), collision.collider);
            //}

            if (canStick)
            {
                StickInTarget(collision, travelledFrames < 2);
            }

            // Player Collision Check (self hit)
            //            if (Player.instance && collision.collider == Player.instance.headCollider)
            //            {
            //                Player.instance.PlayerShotSelf();
            //            }
        }
    }


    /// <summary>
    /// 钉在目标上 如果小于2帧 则忽略射线检测
    /// </summary>
    /// <param name="collision">Collision.</param>
    /// <param name="bSkipRayCast">If set to <c>true</c> b skip ray cast.</param>
    private void StickInTarget(Collision collision, bool bSkipRayCast)
    {
        Vector3 prevForward = prevRotation * Vector3.forward;

        // 仅插在箭头前面的碰撞体上
        if (!bSkipRayCast)
        {
            var hitInfo = Physics.RaycastAll(prevHeadPosition - prevVelocity * Time.deltaTime, prevForward, prevVelocity.magnitude * Time.deltaTime * 2.0f);
            bool properHit = false;
            for (int i = 0; i < hitInfo.Length; ++i)
            {
                RaycastHit hit = hitInfo[i];

                if (hit.collider == collision.collider)
                {
                    properHit = true;
                    break;
                }
            }

            if (!properHit)
            {
                return;
            }
        }

        // 销毁箭头粒子效果
        Destroy(glintParticle);

        inFlight = false;

        arrowRigidbody.velocity = Vector3.zero;
        arrowRigidbody.angularVelocity = Vector3.zero;
        arrowRigidbody.isKinematic = true;
        arrowRigidbody.useGravity = false;
        arrowRigidbody.transform.GetComponent<Collider>().enabled = false;

        arrowHeadRigidbody.velocity = Vector3.zero;
        arrowHeadRigidbody.angularVelocity = Vector3.zero;
        arrowHeadRigidbody.isKinematic = true;
        arrowHeadRigidbody.useGravity = false;
        arrowHeadRigidbody.transform.GetComponent<Collider>().enabled = false;

        hitTargetSound.Play();

        //
        scaleParentObject = new GameObject("Arrow Scale Parent");
        Transform parentTransform = collision.collider.transform;

        //if (parentTransform.parent)
        //{
        //    parentTransform = parentTransform.parent;
        //}

        scaleParentObject.transform.parent = parentTransform;

        //
        transform.parent = scaleParentObject.transform;
        transform.rotation = prevRotation;
        transform.position = collision.contacts[0].point - transform.forward * (0.75f - (Util.RemapNumberClamped(prevVelocity.magnitude, 0f, 10f, 0.0f, 0.1f) + Random.Range(0.0f, 0.05f)));
    }


    //-------------------------------------------------
    void OnDestroy()
    {
        if (scaleParentObject != null)
        {
            Destroy(scaleParentObject);
        }
    }
}
