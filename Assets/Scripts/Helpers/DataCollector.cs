using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{

    public struct TestData
    {
        public int performedTopAttacks;
        public int performedBottomAttacks;
        public int performedThrustAttacks;

        public int failedAttacksByProtectedArea;
        public int failedAttacksbyTopWeapon;

        public int successfulCounterThrusts;
        public int successfulParries;
        public int successfulGuards;

        public int dodgesCount;
        public int backstepsCount;
        public int hitsReceived;

        public int deathsCount;

        public float timePassed;
    }

    public GameObject StatsUICanvas;
    [HideInInspector] public TestData[] testDataList = new TestData[7];
    public static TestData currentTest;

    private bool showData = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            SwitchRenderStats();
    }

    private void SwitchRenderStats()
    {
        showData = !showData;
        StatsUICanvas.SetActive(showData);
    }

    public void UpdateCurrentTest(int index)
    {
        currentTest = testDataList[index];
    }

    public void UpdateData(int index)
    {
        currentTest.timePassed += Time.deltaTime;
        testDataList[index] = currentTest;
    }
}
