using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserAction {
	void moveForward();
	void moveBack();
	void shoot();
	void turn (float degree);
	// 防止碰撞带来的旋转
	void noTurn();
	void noMove();
	int getResult();
}