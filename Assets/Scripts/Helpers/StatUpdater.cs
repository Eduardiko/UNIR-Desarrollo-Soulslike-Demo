using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatUpdater : MonoBehaviour
{
    public DataCollector playerDataCollector;
    public int testIndex = 0;

    private TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        string text = playerDataCollector.testDataList[testIndex].performedTopAttacks.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].performedBottomAttacks.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].performedThrustAttacks.ToString() + "\n\n"

                    + playerDataCollector.testDataList[testIndex].failedAttacksByProtectedArea.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].failedAttacksbyTopWeapon.ToString() + "\n\n"

                    + playerDataCollector.testDataList[testIndex].successfulCounterThrusts.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].successfulParries.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].successfulGuards.ToString() + "\n\n"

                    + playerDataCollector.testDataList[testIndex].dodgesCount.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].backstepsCount.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].hitsReceived.ToString() + "\n\n"

                    + playerDataCollector.testDataList[testIndex].deathsCount.ToString() + "\n"
                    + playerDataCollector.testDataList[testIndex].timePassed.ToString("F2");

        textMesh.text = text;
    }
}
