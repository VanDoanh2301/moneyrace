using System.Collections;
using UnityEngine;

public class PlayerPowerController : MonoBehaviour
{
    public float moneyAmount;
    private int collisionCount;

    [SerializeField] private GameObject moneyIndicator;
    [SerializeField] private GameObject moneyIndicatorPos;
    [SerializeField] private GameObject playerBlock;

    private void Start()
    {
        moneyAmount = 0;
    }

    //When hit a power portals
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PowerPortal"))
        {
            collisionCount++;
            if (collisionCount < 2)
            {
                moneyAmount *= other.gameObject.GetComponent<PowerPortalValue>().powerValue;
                moneyAmount = Mathf.Round(moneyAmount);
            }
            if (moneyAmount < Constants.MIN_POWER_LEVEL)
            {
                moneyAmount = Constants.MIN_POWER_LEVEL;
            }

            Debug.Log(moneyAmount);
            //update money value

            GameObject newIndicator = Instantiate(moneyIndicator, moneyIndicatorPos.transform.position, Quaternion.identity);
            newIndicator.GetComponent<MoneyIndicatorValue>().impactValue.text = string.Format("{0:0}", moneyAmount);
            newIndicator.transform.SetParent(playerBlock.transform);
            StartCoroutine("DelayIndicatorDisable", newIndicator);

            StartCoroutine("ResetCollisionCount");
        }
    }

    IEnumerator ResetCollisionCount()
    {
        yield return new WaitForSeconds(0.5f);
        collisionCount = 0;
    }

    IEnumerator DelayIndicatorDisable(GameObject newIndicator)
    {
        yield return new WaitForSeconds(0.36f);
        Destroy(newIndicator);
    }
}
