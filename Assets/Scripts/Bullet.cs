using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public float explosionRadius = 3f;
	
	//public GameObject gameObject;
	private tankType type;


	public void setTankType(tankType type) {
		this.type = type;
	}
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

}
