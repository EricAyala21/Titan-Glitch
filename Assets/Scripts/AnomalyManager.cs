using UnityEngine;

public class AnomalyManager : MonoBehaviour
{

    public Transform[] spawnPoints;
    public GameObject[] anomalies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // SpawnAnomaly();


    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void SpawnAnomaly()
    {
        int randAnomaly = Random.Range(0, anomalies.Length);
        int randSpawnLoc = Random.Range(0, spawnPoints.Length);
        Instantiate(anomalies[randAnomaly], spawnPoints[randSpawnLoc].position, Quaternion.identity); 
    }
    
  
}
