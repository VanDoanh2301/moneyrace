using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPortalsSetting : MonoBehaviour
{
    [SerializeField] private GameObject[] powerPortalsPosition;
    public int[] portalId;

    // Start is called before the first frame update
    void Start()
    {
        if (portalId != null)
        {
            for (int i = 0; i < portalId.Length; i++)
            {
                GameObject portal = Instantiate(powerPortalsPosition[portalId[i]], gameObject.transform.GetChild(i).transform.position, Quaternion.Euler(0, 90, 0));
                portal.transform.SetParent(gameObject.transform);
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
        }
    }
}
