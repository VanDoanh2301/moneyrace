using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GraterColliderScript : MonoBehaviour
{
    public GameObject cucuparticale, CutParticles;
    public Rigidbody rb;
    [HideInInspector]
    public float totalscore;
    public GraterGameManager gamManager;
    public AIScript aiControl;
    public Transform pos;
    public Material cutMaterial;
    

    void Start()
    {
        
        if (SceneManager.GetActiveScene().buildIndex <= 10)
        {
            totalscore =100;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 20)
        {
            totalscore = 120;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 30)
        {
            totalscore = 130;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 50)
        {
            totalscore = 140;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 60)
        {
            totalscore = 150;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 70)
        {
            totalscore = 160;

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 100)
        {
            totalscore = 180;
        }
        rb = GetComponent<Rigidbody>();
        gamManager = FindObjectOfType<GraterGameManager>();
        aiControl = FindObjectOfType<AIScript>();
    }


    public void OnTriggerEnter(Collider other)
    {  
        if (other.gameObject.tag == "cuttingobjects")
        {
            Testparticle(other.gameObject);
        }        
    }

    public void Testparticle(GameObject Myobject)
    {
        // cutMaterial.color = new Color32(255, 249, 6, 255);
        CutParticles.GetComponent<ParticleSystemRenderer>().material = Myobject.GetComponent<MeshRenderer>().materials[0];

        Myobject.gameObject.SetActive(true);
        if (this.gameObject.tag == "Player")
        {
            Myobject.gameObject.SetActive(false);
            GameObject pARTICLES = Instantiate(CutParticles, Myobject.transform.position, Quaternion.Euler(-Myobject.transform.rotation.x,
                                                                        Myobject.transform.rotation.y, Myobject.transform.rotation.z));
            pARTICLES.transform.SetParent(pos);
            Destroy(pARTICLES, 3f);
            gamManager.AudioSource.PlayOneShot(gamManager.KnifeCutSound);
            FindObjectOfType<GraterGameManager>().score++;
            FindObjectOfType<GraterGameManager>().fill1.fillAmount = ((FindObjectOfType<GraterGameManager>().score) / totalscore);
            // Đập fruit → cộng điểm bền vững (Coin HUD / ví)
            CoinWallet.Add(1);
        }

        if (this.gameObject.tag == "Ai")
        {
            Myobject.gameObject.SetActive(false);
            GameObject pARTICLES = Instantiate(CutParticles, Myobject.transform.position, Quaternion.Euler(-Myobject.transform.rotation.x,
                                                                        Myobject.transform.rotation.y, Myobject.transform.rotation.z));
            pARTICLES.transform.SetParent(pos);
            Destroy(pARTICLES, 3f);
            gamManager.AudioSource.PlayOneShot(gamManager.KnifeCutSound);
            FindObjectOfType<GraterGameManager>().aiscore++;
            FindObjectOfType<GraterGameManager>().ailevelfill.fillAmount = ((FindObjectOfType<GraterGameManager>().aiscore) / totalscore);
        }
        rb.AddForce(0, 0, -0.5f);
        GameObject dummyCucu = Instantiate(cucuparticale, Myobject.transform.position, Myobject.transform.rotation);
        Destroy(dummyCucu, 3f);
    }

    public void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Avoid" || other.gameObject.tag == "Enemy")
        {
            //FindObjectOfType<AiPlayer>().aistop = true;
            aiControl.AIgrateranimation.SetTrigger("AIMouseUp");
            FindObjectOfType<AIScript>().aistop = true;
            //aiControl.AIgrateranimation.SetBool("Idle", false);
           

        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Avoid" || other.gameObject.tag == "Enemy")
        {
          
            aiControl.AIgrateranimation.SetTrigger("AIMouseDown");
            //aiControl.GetComponent<Animator>().enabled = true;
            FindObjectOfType<AIScript>().aistop = false;
        }
    }    
}
