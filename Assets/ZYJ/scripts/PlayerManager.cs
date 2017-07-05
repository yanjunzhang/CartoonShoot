using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;
using UnityEngine.AI;

public class PlayerManager : MonoBehaviour {
	public GameObject[] enemyPrefabs;
	public Transform[] enemyPoints;
    Transform headTransform;
    bool isOver;
    public static PlayerManager instance;
	public string playerName="zyj";
	public int currBulletNum=30;
	public int remainBulletNum=60;
	public int hp=5;
	public int remainEnemy;
	public GameObject menu;
	private Transform player;
	private Text weaponText;
	private Text playerText;
	// Use this for initialization
	void Awake(){
		
	}
	void Start () {
		instance = this;
		player = GameObject.Find ("Player").transform;
        if (headTransform == null)
        {
            headTransform = GameObject.Find("VRCamera").transform;
        }
		if (menu==null) {
			menu = GameObject.Find ("MenuHintCanvas");
		}

    }
	
	// Update is called once per frame
	void Update () {
		if (weaponText==null) {
			try {
				weaponText = GameObject.Find ("WeaponText").GetComponent <Text> ();
			} catch (System.Exception ex) {
				
			}


		}else
		weaponText.text = string.Format ("{0}/{1}", currBulletNum.ToString (), remainBulletNum.ToString ());
		
		if (playerText==null) {
			try {
				playerText=GameObject.Find ("PlayerText").GetComponent <Text> ();
			} catch (System.Exception ex) {
				
			}

		}else
			playerText.text = string.Format (
				"player:{0}\n"+
				"hp:{1}\n"+
				"enemy:{2}\n",
				playerName,hp.ToString (),remainEnemy.ToString ()
			);
		remainEnemy = GameObject.FindGameObjectsWithTag ("EnemyBody").Length;
        if (hp<=0)
        {
            GameOver();
        }
	}
	public void FindPlayerText(){
		try {
			playerText=GameObject.Find ("PlayerText").GetComponent <Text> ();
		} catch (System.Exception ex) {

		}
	}
	public void AddEnemy(){
		GameObject newEnemy =GameObject.Instantiate (enemyPrefabs[Random.Range (0,enemyPrefabs.Length)],enemyPoints[Random.Range (0,enemyPoints.Length)].position,Quaternion.identity);
		newEnemy.GetComponent <NavMeshAgent> ().SetDestination (player.position);
	}

    public void GameOver()
    {
        if (!isOver)
        {
            GameObject[] remains = GameObject.FindGameObjectsWithTag("EnemyBody");
            for (int i = 0; i < remains.Length; i++)
            {
                Destroy(remains[i]);
            }
            Transform gameOver = GameObject.Find("GameOverCanvas").transform;
            gameOver.position = headTransform.position + headTransform.forward * 2f+headTransform.up*0.15f ;
            gameOver.LookAt(new Vector3(headTransform.position.x, gameOver.position.y, headTransform.position.z));
            
            isOver = true;
        }
        
    }
    public void RushMode()
    {
        GameObject[] remains = GameObject.FindGameObjectsWithTag("EnemyBody");
        for (int i = 0; i < remains.Length; i++)
        {
            remains[i].GetComponent<NavMeshAgent>().SetDestination(player.position);
        }
        
    }
}
