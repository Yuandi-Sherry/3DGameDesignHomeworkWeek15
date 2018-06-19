using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Blood {
	public delegate void destroy(int result);
	public static event destroy destroyEvent;
	// Use this for initialization
	void Start () {
		setBlood(500f);
	}
	
	// Update is called once per frame
	void Update () {
		if (getBlood() <= 0) {
			this.gameObject.SetActive(false);
			if(destroyEvent != null) {
				destroyEvent(-1);
			}
		}		
	}
}
