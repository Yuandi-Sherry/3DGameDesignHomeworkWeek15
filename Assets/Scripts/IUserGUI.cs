using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IUserGUI : MonoBehaviour {
	IUserAction action;
	GUIStyle labelStyle;

	// Use this for initialization
	void Start () {
		action = Director.getDirector().currentSceneController as IUserAction;
	}
	
	// Update is called once per frame
	void Update () {
		if(action.getResult() == 0) {
			// 读取键盘输入，根据键盘输入调用相应函数
			if(Input.GetKey(KeyCode.W)) {
				//Debug.Log("moveForward in IUserGUI");
				action.moveForward();
			}
			else if(Input.GetKey(KeyCode.S)) {
				action.moveBack();
			}
			else {
				action.noMove();
			}

			if(Input.GetKeyDown(KeyCode.Space)) {
				action.shoot();
			}
			float deltaX = 1f;
			if(Input.GetKey(KeyCode.A)) {
				//Debug.Log("turn in IUserGUI");
				action.turn((-1)*deltaX);
			}
			else if(Input.GetKey(KeyCode.D)) {
			//	Debug.Log("turn D in IUserGUI");
				action.turn(deltaX);
			}
			else {
				action.noTurn();
				
			}
		}
	}

	void OnGUI() {
		labelStyle = new GUIStyle("label");
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontSize = Screen.height/15;
		GUI.color = Color.black;
		if(action.getResult() == 1) {
			Debug.Log("YOU WIN!");
			GUI.Label(new Rect(Screen.width/2 - Screen.width/8,Screen.height/2 - Screen.height/8,Screen.width/4,Screen.height/4), "YOU WIN!",labelStyle);
		}
		else if(action.getResult() == -1) {
			Debug.Log("Game Over!");
			GUI.Label(new Rect(Screen.width/2 - Screen.width/8,Screen.height/2 - Screen.height/8,Screen.width/4,Screen.height/4), "Game Over!",labelStyle);
		}
	}
}
