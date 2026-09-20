using UnityEngine;
using System.Collections;
using System;

namespace PrabuddhaSingh.FinalCharachterController
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Enemy Model")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Wave Settings")]
        [SerializeField] private int startingEnemyCount =3;
        [SerializeField] private int enemyIncreasePerWave = 2;
        [SerializeField] private float timeBetweenWaves = 2f;
        [SerializeField] private int maxWaves =3;

        [Header("Spawn points")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Completion UI")]
        [SerializeField] private GameObject waveCompletionUI;

        private int currentWave=0;
        private int currentEnemyCount=0;

        public event Action<int,int> OnWaveStarted;

        private void Start()
        {
            StartWave();
        }

        private void StartWave()
        {
            currentWave++;
            if(currentWave <= maxWaves)
            {
                OnWaveStarted?.Invoke(currentWave, maxWaves);
            }
            else{
                Debug.Log("All waves completed.");

                waveCompletionUI.SetActive(true);
                StartCoroutine(HideCompletionUI());
                return;
            }
            int enemyCount = startingEnemyCount +  (currentWave -1) * enemyIncreasePerWave;
            currentEnemyCount = enemyCount;
            Debug.Log($"Starting Wave {currentWave} with {enemyCount} enemies.");
            SpawnWave(currentEnemyCount);
        }

        private IEnumerator StartNextWave(){

            Debug.Log($"Wave {currentWave} completed. Next wave in {timeBetweenWaves} seconds.");
           yield return new WaitForSeconds(timeBetweenWaves);
           StartWave();
        }

        private void SpawnWave(int enemyCount)
        {
            for(int i=0; i<enemyCount; i++)
            {
                Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
                GameObject enemy = Instantiate(enemyPrefab , spawnPoint.position , spawnPoint.rotation);
                
                EnemyController enemyController = enemy.GetComponent<EnemyController>();

                enemyController.SetWaveManager(this);
            }
        }

        public void EnemyDead()
        {
            Debug.Log("Enemy dead. Remaining enemies in wave: " + (currentEnemyCount - 1));
            currentEnemyCount--;
            if(currentEnemyCount <= 0)
            {
                Debug.Log("all enemies dead in wave");
                StartCoroutine(StartNextWave());
            }
        }

        private IEnumerator HideCompletionUI()
        {
            yield return new WaitForSeconds(3f);
            waveCompletionUI.SetActive(false);
        }
    }
}

