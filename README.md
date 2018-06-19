# 3DGameDesignHomeworkWeek15
![preview](https://github.com/Yuandi-Sherry/3DGameDesignHomeworkWeek15/blob/master/preview.gif?raw=true)
> 视频链接：https://www.bilibili.com/video/av25139467/

# Tanks

【这篇博客暂时就发到github包里，近日凉凉重装系统还没有把hexo装回来。我会尽快同步到github的blog上的】

首先看一下这次的游戏都有哪些对象以及他们所需要的各种操作：

### Tank

​	无论是Play还是NPC都具有Tank的属性，而其中的公有部分就是它们的血条。因而创建一个基类Blood，之后让Player和NPC都集成它并发展它们各自不同的功能。

```c#
public class Blood : MonoBehaviour {
	private float blood;
	public float getBlood() {
		return blood;
	}
	public void setBlood(float blood0) {
		blood = blood0;
	}
}
```

#### Player

​	如果血条降为0，则发送`destroyEvent`，并且在场控类中对该消息进行监听，以控制游戏的开始结束。

```csharp
// Player.cs
public delegate void destroy(int result);
public static event destroy destroyEvent;
...
void Update () {
	if (getBlood() <= 0) {
		this.gameObject.SetActive(false);
		if(destroyEvent != null) {
			destroyEvent(-1);
		}
	}		
}
```

​	场控一开始就要对Player的destroyEvent事件订阅，并作出将result设为1（表示胜利）的响应。

```csharp
void Start () {
	...
	Player.destroyEvent += setResult; 
}; 
```

#### NPC

NPC需要控制的主要有两方面：一方面，要判断自身是否还活着（即血量是否还>0），另一方面，要追逐Player并对其进行攻击。

##### NavMeshAgent

参考Unity3D官网文档：https://docs.unity3d.com/Manual/nav-MoveToDestination.html，利用`SeDestination`函数将Player作为寻路`destination`即可。

```csharp
void Update () {
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
		// 游戏结束，停止寻路
		NavMeshAgent agent = GetComponent<NavMeshAgent> ();
		agent.velocity = Vector3.zero;
		agent.ResetPath();
	}
}
```

##### Coroutines

 对于坦克射击的动作，并不是在游戏的一帧内就完成的，因此需要用到`Coroutines`.

> A coroutine is like a function that has the ability to pause execution and return control to Unity but then to continue where it left off on the following frame. 

这里还需要控制坦克发射子弹的间隔。

发射子弹的主要思路为从工厂中获得子弹，并用物理引擎Rigidbody赋予其水平速度，由于是子弹的发射，因而这里力的模式为`Impulse`. 

力的模式总结如[这篇博客](https://blog.csdn.net/littlepandas/article/details/56008898)：

> ForceMode.Force：给物体添加一个持续的力并使用其质量。
>
> ForceMode.Acceleration:：给物体添加一个持续的加速度，但是忽略其质量。
>
> ForceMode.Impulse;：给物体添加一个**瞬间的力**并使用其质量
>
> ForceMode.VelocityChange;：给物体添加一个瞬间的加速度，但是忽略其质量

```csharp
IEnumerator shoot() { // 协程实现npc坦克射击
	while(sceneController.getResult()==0) {
		// 控制发射子弹间隔
		for(double i = 3; i > 0; i -= Time.deltaTime) {
			yield return null;
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
```

### Bullet

作为挂载在子弹上的代码，需要处理的就是子弹的碰撞坦克的事件。游戏中的设定为子弹打击到物体会产生爆炸，而所有爆炸所波及到的对立物体（这里指player子弹产生的爆炸会使NPC的血条受损，同样NPC子弹产生的爆炸会使player的血条受损）都到受到损伤，因而采用API：Physics.OverlapSphere. 在[这篇博客](https://blog.csdn.net/u013700908/article/details/52888792)中有详细用法的介绍。

```csharp
void OnCollisionEnter(Collision other) {
	Debug.Log("OnCollisionEnter");
	Factory myFactory = Singleton<Factory>.Instance;
	ParticleSystem explosion = myFactory.getPS();
	explosion.transform.position = transform.position;
	Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
	for (int i = 0; i < colliders.Length; i++) {
		// 如果玩家子弹击中了NPC或者NPC子弹击中了玩家坦克，伤害才有效
		if(colliders[i].tag == "tankPlayer" && this.type == tankType.NPC || 
			colliders[i].tag == "tankNPC" && this.type == tankType.PLAYER ) {
			//Debug.Log("对" + colliders[i].tag + "造成伤害");
			float distance = Vector3.Distance(colliders[i].transform.position, transform.position);
			float hurt = 100f/distance; // 距离越远伤害越小;
			float current = colliders[i].GetComponent<Blood>().getBlood();
			colliders[i].GetComponent<Blood>().setBlood(current - hurt);
		}
	}
	explosion.Play();
	if(this.gameObject.activeSelf) {
		myFactory.recycleBullet(this.gameObject);
	}
}
```

### IUserGUI

​	与用户交互的界面主要分为两个部分：

#### 用户键盘输入操纵游戏

相应的接口在`FirstController`中实现：

```java
void Update () {
	if(action.getResult() == 0) {
		// 读取键盘输入，根据键盘输入调用相应函数
		if(Input.GetKey(KeyCode.W)) {
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
			action.turn((-1)*deltaX);
		}
		else if(Input.GetKey(KeyCode.D)) {
			action.turn(deltaX);
		}
		else {
			action.noTurn();
		}
	}
}
```

#### 信息输出告知结果

这里的内容第一节课就学过了，代码如下：

```csharp
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
```

### Director

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : System.Object {
	private static Director instance;
	public FirstController currentSceneController {get; set;}
	public static Director getDirector () {
		if(instance == null) {
			instance = new Director();
		}
		return instance;
	}
	
}

```

### Factory

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum tankType: int {PLAYER, NPC}
public class Factory : MonoBehaviour {
	public GameObject player;
	public GameObject npc;
	public GameObject bullet;
	public ParticleSystem ps;

	private Dictionary<int, GameObject> tanks;
    private Dictionary<int, GameObject> freeTanks;
    private Dictionary<int, GameObject> bullets;
    private Dictionary<int, GameObject> freeBullets;
	
    private List<ParticleSystem> psQueue;
	// Use this for initialization
	void Awake () {
		tanks = new Dictionary<int, GameObject>();
		freeTanks = new Dictionary<int, GameObject>();
		bullets = new Dictionary<int, GameObject>();
		freeBullets = new Dictionary<int, GameObject>();
		psQueue = new List<ParticleSystem>();
	}

	void Start() {
		NPC.recycleEvent += recycleTank;
	}
	
	public GameObject getPlayer() {
		return player;
	}

	public GameObject getTank() {
		// 如果没有坦克可以使用了，则新生成坦克
		if(freeTanks.Count == 0) {
			// 新生成一个坦克的血条
			GameObject newTank = Instantiate(Resources.Load<GameObject>("Prefabs/npc"), new Vector3(0,0,0),  Quaternion.identity) as GameObject;
			// 将坦克加入坦克生成队列
			tanks.Add(newTank.GetInstanceID(), newTank);
			// 新生成的坦克随机出现在地图上
			newTank.transform.position = new Vector3(Random.Range(-20,20), 0, Random.Range(-20,20));
			return newTank;
		}
		// 否则仍然有可用的坦克，则将工厂中的坦克激活
		foreach (KeyValuePair<int, GameObject> pair in freeTanks) {
			pair.Value.SetActive(true);
			freeTanks.Remove(pair.Key);
			tanks.Add(pair.Key, pair.Value);
			pair.Value.transform.position = new Vector3(Random.Range(-20,20), 0, Random.Range(-20,20));
			return pair.Value;
		}
		return null;
	}
	
	public GameObject getBullet(tankType type) {
		if(freeBullets.Count == 0) {
			GameObject newBullet = Instantiate(Resources.Load<GameObject>("Prefabs/Shell"), new Vector3(0,0,0),  Quaternion.identity) as GameObject;
			// npc 和 player 发射子弹的方式不同
			newBullet.GetComponent<Bullet>().setTankType(type);
			newBullet.tag = "bullet";
			bullets.Add(newBullet.GetInstanceID(), newBullet);
			return newBullet;
		}
		foreach (KeyValuePair<int, GameObject> pair in freeBullets) {
			pair.Value.SetActive(true);
			pair.Value.GetComponent<Bullet>().setTankType(type);
			freeBullets.Remove(pair.Key);
			bullets.Add(pair.Key, pair.Value); 
			return pair.Value;
		}
		return null;
	}

	public ParticleSystem getPS() {
		for (int i = 0; i < psQueue.Count; i++) {
			if(!psQueue[i].isPlaying) {
				return psQueue[i];
			}
		}
		ParticleSystem newPS = Instantiate<ParticleSystem>(ps);
		psQueue.Add(newPS);
		return newPS;
	}

	/*public void destroyEvent(GameObject tank) {
		tanks.Remove(tank.GetInstanceID());
		freeTanks.Add(tank.GetInstanceID(), tank);
		tank.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
		tank.SetActive(false);
	}
*/
	public void recycleTank(GameObject tank) {
        tanks.Remove(tank.GetInstanceID());
        freeTanks.Add(tank.GetInstanceID(), tank);
        tank.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        tank.SetActive(false);
    }


	public void recycleBullet(GameObject bullet) {
		//Debug.Log("recycleBullet");
		bullets.Remove(bullet.GetInstanceID());
		freeBullets.Add(bullet.GetInstanceID(), bullet);
		bullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
		bullet.SetActive(false);
		//Debug.Log("recycleBullet" + freeBullets.Count);
	}
}

```

### 控制主摄像机

将脚本挂载到主摄像机上，即可跟随player移动。

```java
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public GameObject player;
	private Vector3 offset;//it's a vector from player to camera
	//offset can be obtained by transform(camera) - player
	// Use this for initialization
	void Start () {
		offset = transform.position - player.transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = player.transform.position + offset;
	}
}
```

### 控制血条

将脚本挂载到player上即可。

```java
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BloodController : MonoBehaviour
{
    private float hScrollbarValue;
    public Slider healthSlider;
    private float fullBlood;
    void Start() {
    	fullBlood = 500f;

    }
    void OnGUI () {
    	GUI.color = Color.red;
        //hScrollbarValue = GUI.HorizontalScrollbar (new Rect (0, 0, 100, 30), 0.0f, this.gameObject.transform.parent.GetComponent<Blood>().getBlood()*0.02f, 0.0f, 10.0f);
    	healthSlider.value = this.gameObject.GetComponent<Blood>().getBlood()/fullBlood;
    	//Debug.Log(this.gameObject.GetComponent<Blood>().getBlood());
    }
}
```

### FirstController

```csharp
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

	public void moveForward() {
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

```

## 常见问题的处理方法

### 碰撞问题

#### 坦克之间不会互相穿越

​	修改tank的BoxCollider组件的size（有点像形成了一个盒装结界，就没有办法因为靠得太近发生碰撞啦）。

![BoxCollider](https://s1.ax1x.com/2018/06/19/Cz8hzd.png)

#### 碰撞不产生莫名的旋转

​	之前一直用代码控制transform的rotation保持不变，后来发现Rigidbody已经为我们做好了组件。把Constraints的freeze Rotation打勾即可，包括如果不希望发生位移的话，在freeze position打勾就好了。

![Rigidbody](https://s1.ax1x.com/2018/06/19/Cz85QA.png)

### GetKey和GetKeyDown的区别

顾名思义，一个是按着，一个是按下。作为一个玩家，在控制旋转操作的时候，一开始用的keyDown，但会发现如果想让坦克旋转，则必须一下一下敲击键盘，而更人性化的方法明显是长按，因而这里使用`GetKey`更为合理。

### 挂载脚本的顺序

​大坑！！制作过程中遇到虽然`Bullet`每次攻击都调用了`Blood`脚本的`SetBlood`函数，但`Debug.Log`仍然发现玩家和NPC的血条一直是初始值。

## NavMesh相关学习笔记

##### 启动面板

​	windows - navigation

##### 寻路地形

​	打勾Navigation Static - 使当前物体作为寻路功能的一部分

​	Object - 当前物体

​	Bake - 全局

### 各种具体功能的实现

#### 障碍物绕行

​	将障碍物作为`Not Walkable`

#### 爬楼梯/跳跃（OffMeshLink）

##### 爬楼梯

​	楼梯开始/结束位置放置两个顶点`startPoint`, `endPoint`，可以用empty GameObject制作。

​	选择**楼梯**，添加`OffMeshLink`，拖入`startPoint`和`endPoint`.

##### 跳跃

​	跳下：Navigatuon - Bake - Drop Height 掉落高度

​	跳过：... - Jump Distence 横向跳跃距离

 或

​	打勾 Navigation 的 OffMeshLink
