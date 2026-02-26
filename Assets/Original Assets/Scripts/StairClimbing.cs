using DG.Tweening;
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairClimbing : MonoBehaviour
{
    [SerializeField] private GameObject playerBlock;
    [SerializeField] private GameObject stair;
    [SerializeField] private GameObject stackPos;
    [SerializeField] private GameObject playerController;
    public GameObject moneyPath;

    private Animator characterAnim;

    // Start is called before the first frame update
    void Start()
    {
        characterAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Stair"))
        {
            playerController.GetComponent<control>().enabled = false;
            playerBlock.GetComponent<PlayerBlockMovement>().enabled = false;
            StartCoroutine("StartClimbing");
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Stair"))
    //    {
    //        StartCoroutine("StartClimbing");
    //    }
    //}

    IEnumerator StartClimbing()
    {
        yield return new WaitForSeconds(0.5f);
        //gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        float stackMoneyCollected = gameObject.GetComponent<PlayerPowerController>().moneyAmount;
        int checkpoint = 1000;

        for (int i = 0; i < stair.transform.childCount; i++)
        {
            if (stackMoneyCollected < checkpoint)
            {
                playerBlock.transform.DOMove(stair.transform.GetChild(i).transform.position + new Vector3(0, 0.9f, 0), 0.5f + (0.5f * i)).SetEase(Ease.Linear);
                StartCoroutine("StairClimbingDelay", i);
                yield break;
            }
            else if (stackMoneyCollected == checkpoint)
            {
                int j = i + 1;
                playerBlock.transform.DOMove(stair.transform.GetChild(j).transform.position + new Vector3(0, 0.9f, 0), 0.5f + (0.5f * i)).SetEase(Ease.Linear);
                StartCoroutine("StairClimbingDelay", j);
                yield break;
            }
            else if (checkpoint >= 10000)
            {
                playerBlock.transform.DOMove(stair.transform.GetChild(i + 1).transform.position + new Vector3(0, 0.9f, 0), 0.5f + (0.5f * i)).SetEase(Ease.Linear);
                StartCoroutine("StairClimbingDelay", i);
                yield break;
            }
            else
            {
                checkpoint += 500;
            }
        }
    }

    IEnumerator StairClimbingDelay(int i)
    {
        yield return new WaitForSeconds(0.5f + (0.5f * i));
        stackPos.SetActive(false);
        moneyPath.SetActive(false);
        characterAnim.SetTrigger("Interupt");

        StartCoroutine("FallingInTheStaircase");
    }

    IEnumerator FallingInTheStaircase()
    {
        yield return new WaitForSeconds(0.4f);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        
        StartCoroutine("Celebrating");
    }

    IEnumerator Celebrating()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
        characterAnim.SetTrigger("Won");

        StartCoroutine("DelayGameWinning");
    }

    IEnumerator DelayGameWinning()
    {
        yield return new WaitForSeconds(3f);
        GameManager.instance.hasWon = true;
    }
}
