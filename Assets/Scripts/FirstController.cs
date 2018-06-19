using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstController : MonoBehaviour, IUserAction {
	public GameObject playerTank;
	private bool gameOver = false;
	private int result = 0; // 0->onGame, 1->win, -1->lose;

	private int countNPC = 5;
	private Factory myFactory;
	private float speed = 30f;

	float lastX;
	float lastY;

	void Awake() {
		Director director = Director.getDirector();
		director.currentSceneController = this;
		myFactory = Singleton<Factory>.Instance;
		playerTank = myFactory.getPlayer();
	}
	// Use this for initialization
	void Start () {
		for(int i = 0; i < countNPC; i++) { // 从工厂获得一定数目的NPC
			myFactory.getTank();
		}
		// 订阅Player发布的destroy事件，并响应
		Player.destroyEvent += setResult; 
	}

	void Update() {
		if(countNPC <= 0) {
			setResult(1);
		}
	}


	// implementation of IUserAction
	// >??
	public void moveForward() {
		Debug.Log("moveForward in FirstController");
		playerTank.GetComponent<Rigidbody>().velocity = playerTank.transform.forward*speed;
	}

	public void moveBack() {
		playerTank.GetComponent<Rigidbody>().velocity = playerTank.transform.forward * (-1)*speed;
	}

	public void turn(float deltaX) {
		float x = playerTank.transform.localEulerAngles.y + deltaX*5;
		float y = playerTank.transform.localEulerAngles.x;
		lastX = x;
		lastY = y;
		playerTank.transform.localEulerAngles = new Vector3(y,x,0);
	}

	public void noTurn() {
		playerTank.transform.localEulerAngles = new Vector3(lastY,lastX,0);
	}

	public void noMove() {
		playerTank.GetComponent<Rigidbody>().velocity = Vector3.zero;
	}

	public void shoot() {
		GameObject bullet = myFactory.getBullet(tankType.PLAYER);
		bullet.transform.position = new Vector3(playerTank.transform.position.x, 1.5f, playerTank.transform.position.z)
			+ playerTank.transform.forward * 1.5f; // 坦克的位置 + 子弹相对坦克的位置
		bullet.transform.forward = playerTank.transform.forward;
		Rigidbody rb = bullet.GetComponent<Rigidbody>();
		rb.AddForce(bullet.transform.forward * speed * 10, ForceMode.Impulse);
	}

	public Vector3 getPlayerPosition() {
		return playerTank.transform.position;
	}

	public void setResult(int result) {
		this.result = result;
	}

	public int getResult() {
		return result;
	} 
	public void decreaseCountNPC() {
		countNPC--;
	}
}
