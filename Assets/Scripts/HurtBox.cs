using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour{

    private BoxCollider hurtBox;
    public List<GameObject> damageTargets = new List<GameObject>();

    // Start is called before the first frame update
    void Start(){
        hurtBox = this.gameObject.GetComponent<BoxCollider>();
    }

    //Add targets to damageTargets list while they exist for duration of explosion
    void OnTriggerEnter(Collider other){
        if (other.gameObject.GetComponent<Monster>() != null){
            damageTargets.Add(other.gameObject);
        }
    }

    public void colliderControl(bool activation){
        if (activation)
            hurtBox.enabled = true;
        else if (!activation)
            hurtBox.enabled = false;
    }

    public void sizeAddition(float x, float y, float z){
        hurtBox.size += new Vector3(x, y, z);
    }

    public void sizeReset(float x, float y, float z){
        hurtBox.size = new Vector3(x, y, z);
    }

    public void dealDamage(float damage){

        foreach (GameObject target in damageTargets) {
            if (target != null)
                target.GetComponent<Monster>().TakeDamage(damage);
        }

        damageTargets.Clear();
    }
}
