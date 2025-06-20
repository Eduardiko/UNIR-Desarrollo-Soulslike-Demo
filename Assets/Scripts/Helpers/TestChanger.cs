using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChanger : MonoBehaviour
{
    public int testIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            other.gameObject.GetComponent<DataCollector>().UpdateCurrentTest(testIndex);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            other.gameObject.GetComponent<DataCollector>().UpdateData(testIndex);
    }
}
