using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChange : MonoBehaviour
{
    public Texture change;

    public void Start()
    {
        if (MenuScript.mul == 0)
        {
            gameObject.transform.gameObject.SetActive(false);
        }
    }

    public void OnCollisionEnter(Collision ditched)
    {
        if (MenuScript.mul==1)
        {
            ditched.gameObject.GetComponent<MeshRenderer>().material.mainTexture = change;
            Destroy(this.gameObject);
        }
    }
}
