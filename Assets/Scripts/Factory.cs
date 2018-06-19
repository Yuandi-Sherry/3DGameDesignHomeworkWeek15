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
