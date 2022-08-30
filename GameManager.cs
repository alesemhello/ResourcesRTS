using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameParameters gameParameters;
    private void Awake()
    {
        DataHandler.LoadGameData();
        GetComponent<DayAndNightCycler>().enabled = gameParameters.enableDayAndNightCycle;
    }
}