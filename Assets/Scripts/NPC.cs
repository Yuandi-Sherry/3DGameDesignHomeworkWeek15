using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : Blood {
	public delegate void recycle(GameObject tank);
	public delegate void win(int result);
	
	public static event recycle recycleEvent;
	public static event win winGame;

	private int countNPC;

	// Use this for initialization
	private Vector3 target; // player.position
	private FirstController sceneController;
	void Awake () {
		setBlood(300f);
		sceneController = Director.getDirector().currentSceneController;
	}

	void Start() {
		StartCoroutine(shoot());
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log("NPC count" + countNPC);
		if(sceneController.getResult()==0) {
			target = sceneController.getPlayerPosition();
			// 如果NPC没血了，则被摧毁
			if(getBlood() <= 0.0f && recycleEvent != null) {
				recycleEvent(this.gameObject);
				sceneController.decreaseCountNPC();
			}
			else {
			// 否则专注攻击玩家
				NavMeshAgent agent = GetComponent<NavMeshAgent>();
				agent.SetDestination(target);
			}
		}
		else {
			// 游戏结束，停止寻路？？
			NavMeshAgent agent = GetComponent<NavMeshAgent> ();
			agent.velocity = Vector3.zero;
			agent.ResetPath();
		}
	}

	IEnumerator shoot() { // 协程实现npc坦克射击
		while(sceneController.getResult()==0) {
			// 控制发射子弹间隔
			for(double i = 3; i > 0; i -= Time.deltaTime) {
				yield return 0;
			}
			if (Vector3.Distance(transform.position, target) < 20) {
				Factory myFactory = Singleton<Factory>.Instance;
				GameObject bullet = myFactory.getBullet(tankType.NPC);
				bullet.transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z) + transform.forward*1.5f;
				bullet.transform.forward = transform.forward;
				Rigidbody rb = bullet.GetComponent<Rigidbody>();
				rb.AddForce(bullet.transform.forward * 20, ForceMode.Impulse);
			}
		}
	}
}
