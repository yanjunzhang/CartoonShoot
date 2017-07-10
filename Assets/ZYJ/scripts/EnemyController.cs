/*
 * Author : shenjun
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour {
    enum AIType
    {
        rock,
        slash,
        newRock,
        newSlash

    }
	public int hp=3;
	public float distance;
    public Animation ani;
    public NavMeshAgent agent;
    //public Transform target;
    private GameObject player;
	private bool isDead;
	public bool findTarget;
	void Start () {
		player = GameObject.Find ("Player");
        ani = GetComponent<Animation>();
        agent = GetComponent<NavMeshAgent>();
		agent.stoppingDistance = 1f;
        //agent.destination = target.position;
	}
	
	void Update () {
		if (isDead) {
			return;
		}
		distance = Vector3.Distance (player.transform.position, transform.position);
		SlashAI ();
		if (agent.hasPath) {
			findTarget = true;
		}
    }
	void SlashAI(){
		if (distance<=2.0f) {
			agent.isStopped = true;
			ani.Play ("attackSlash", PlayMode.StopAll);

			return;
		}else if ( distance>2.0f&&distance<15f||findTarget==true) {
			agent.isStopped = false;
			agent.SetDestination (player.transform.position);
			findTarget = true;
			//Debug.Log (agent.destination);
			ani.Play ("walk", PlayMode.StopAll);
			return;
		}else  if(!findTarget){
			agent.isStopped = true;
			ani.Play ("idle3",PlayMode.StopAll);
			//Debug.Log (this.name);
		}
	}

	void OnTriggerEnter(Collider col){
		if (col.gameObject.tag=="Bullet"&&isDead==false) {
			Destroy (col.gameObject);
			hp--;
			ani.Play ("hit1", PlayMode.StopAll);
			if (hp<=0) {
				agent.isStopped = true;
				ani.Play ("death", PlayMode.StopAll);
				isDead = true;
                
				Destroy (this.gameObject,5f);

			}
		}
	}
}
