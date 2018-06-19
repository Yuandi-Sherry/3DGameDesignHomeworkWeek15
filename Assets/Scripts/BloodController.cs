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