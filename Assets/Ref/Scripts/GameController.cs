using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour {

    public GameObject cucuparticale, tamatoparticale, brizalparticale, mashroomparticale, bombparticale;
    public Rigidbody rb;
    [HideInInspector]
    public  float totalscore;
    public GameManager gamManager;
   

    void Start ()
    {
        if (SceneManager.GetActiveScene().buildIndex<=10)
        {
            totalscore = 100;
        }else if (SceneManager.GetActiveScene().buildIndex <= 20)
        {
            totalscore = 120;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 30)
        {
            totalscore = 150;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 50)
        {
            totalscore = 250;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 60)
        {
            totalscore = 300;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 70)
        {
            totalscore = 350;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 100)
        {
            totalscore = 400;
        }
        rb = GetComponent<Rigidbody>();
        gamManager = FindObjectOfType<GameManager>();
    }

    

    public void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.tag == "cucu")
        {

            if (this.gameObject.tag == "Player")
            {
                gamManager.AudioSource.PlayOneShot(gamManager.KnifeCutSound);
                FindObjectOfType<GameManager>().score++;
                FindObjectOfType<GameManager>().fill1.fillAmount = ((FindObjectOfType<GameManager>().score) / totalscore);
            }
            if (this.gameObject.tag == "Ai")
            {
                FindObjectOfType<GameManager>().aiscore++;
                FindObjectOfType<GameManager>().ailevelfill.fillAmount = ((FindObjectOfType<GameManager>().aiscore) / totalscore);

            }
            Debug.Log(((FindObjectOfType<GameManager>().score))/totalscore);

            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            rb.AddForce(0, 0,-0.5f);
            
         GameObject dummyCucu=  Instantiate(cucuparticale, this.transform.position, this.transform.rotation);

            Destroy(dummyCucu,3f);
        }

        if (other.gameObject.tag == "tamoto")
        {
            if (this.gameObject.tag == "Player")
            {
                gamManager.AudioSource.PlayOneShot(gamManager.KnifeCutSound);
                FindObjectOfType<GameManager>().score++;
            }
            if (this.gameObject.tag == "Ai")
            {
                FindObjectOfType<GameManager>().aiscore++;
            }

            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            GameObject dummyTomato = Instantiate(tamatoparticale, this.transform.position, this.transform.rotation);
            Destroy(dummyTomato,3f);

        }

        if (other.gameObject.tag == "brizal")
        {
            if (this.gameObject.tag == "Player")
            {
                gamManager.AudioSource.PlayOneShot(gamManager.KnifeCutSound);
                FindObjectOfType<GameManager>().score++;
            }
            if (this.gameObject.tag == "Ai")
            {
                FindObjectOfType<GameManager>().aiscore++;
            }
            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            GameObject dummybrinjal = Instantiate(brizalparticale, this.transform.position, this.transform.rotation);
            Destroy(dummybrinjal,3f);
        }

        if (other.gameObject.tag == "mashroom")
        {
            if (this.gameObject.tag == "Player")
            {
                gamManager.AudioSource.PlayOneShot(gamManager.KnifeCutSound);
                FindObjectOfType<GameManager>().score++;
            }
            if (this.gameObject.tag == "Ai")
            {
                FindObjectOfType<GameManager>().aiscore++;
            }

            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            GameObject dummymashroom = Instantiate(mashroomparticale, this.transform.position, this.transform.rotation);
            Destroy(dummymashroom,3f);
        }

        if (other.gameObject.tag == "bomb")
        { 
            Destroy(other.gameObject);
            GameObject dummybomb = Instantiate(bombparticale, this.transform.position, this.transform.rotation);
            Destroy(dummybomb,3f);
        }

    }
    public void OnCollisionStay(Collision collision)
    {
        Debug.Log(collision.gameObject);
        if (collision.gameObject.tag == "Avoid")
        {
           FindObjectOfType<AiPlayer>(). aistop = true;
            Debug.Log("stay");

        }
    }
    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Avoid")
        {
            Debug.Log("exit");

            FindObjectOfType<AiPlayer>().aistop = false;
        }
    }
    
    
}
