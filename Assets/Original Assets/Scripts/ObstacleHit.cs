using DG.Tweening;
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    [SerializeField] private GameObject invisibleTrack;
    [SerializeField] private GameObject playerBlock;
    [SerializeField] private GameObject player;

    [SerializeField] private float knockBackDistance;

    [SerializeField] private GameObject stackPos;
    private float stackCheckpoint = 10400f;

    private Animator characterAnim;

    [SerializeField] private GameObject moneyIndicator;
    [SerializeField] private GameObject moneyIndicatorPos;

    void Start()
    {
        characterAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        for (int i = stackPos.GetComponent<StackPosController>().moneyStack.Length - 1; i >= 0; i--)
        {
            if (gameObject.GetComponent<PlayerPowerController>().moneyAmount <= 0)
            {
                stackPos.GetComponent<StackPosController>().moneyStack[0].gameObject.SetActive(false);
            }
            if (gameObject.GetComponent<PlayerPowerController>().moneyAmount < stackCheckpoint)
            {
                stackPos.GetComponent<StackPosController>().moneyStack[i].gameObject.SetActive(false);
                stackCheckpoint -= 100f;
            }
            else
            {
                stackCheckpoint = 10400f;
                return;
            }
        }
    }

    //When hit an obstacle
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            //minus some Money on Player
            gameObject.GetComponent<PlayerPowerController>().moneyAmount -= collision.gameObject.GetComponent<ObstacleValue>().obstacleValue;

            invisibleTrack.GetComponent<SplineFollower>().follow = false;
            player.GetComponent<control>().enabled = false;

            GameObject newIndicator = Instantiate(moneyIndicator, moneyIndicatorPos.transform.position, Quaternion.identity);
            newIndicator.GetComponent<MoneyIndicatorValue>().impactValue.text = string.Format("-" + "{0:0}", collision.gameObject.GetComponent<ObstacleValue>().obstacleValue);
            newIndicator.transform.SetParent(playerBlock.transform);
            StartCoroutine("DelayIndicatorDisable", newIndicator);

            //if player have no money left ... else...
            if (gameObject.GetComponent<PlayerPowerController>().moneyAmount < 0)
            {
                characterAnim.SetTrigger("Dead");
                gameObject.GetComponent<Collider>().enabled = false;
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                PlayerBlockMovement.instance.playerBlockSpeed = 0;
            }
            else
            {
                characterAnim.SetBool("Hit", true);
                playerBlock.transform.DOMoveX(transform.position.x + knockBackDistance, 1f);
                StartCoroutine("PlayerMovingToPosition");
            }
        }
    }
    IEnumerator PlayerMovingToPosition()
    {
        yield return new WaitForSeconds(1.2f);
        characterAnim.SetBool("Hit", false);
        PlayerBlockMovement.instance.playerBlockSpeed = 2.5f;
        player.GetComponent<control>().enabled = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    IEnumerator DelayIndicatorDisable(GameObject newIndicator)
    {
        yield return new WaitForSeconds(0.36f);
        Destroy(newIndicator);
    }
}
