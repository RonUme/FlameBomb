using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject gameModeObjects;
    public GameObject clearModeObjects;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform playAreaA;
    [SerializeField] private Transform playAreaB;
    [SerializeField] private Text timeText;
    [SerializeField] private Text scoreText;

    public float respawnTime = 1.2f;
    private float time;
    private int pawnNumber;
    public int destroyNumber;
    public int gameClearNumber = 10;


    // Start is called before the first frame update
    void Start()
    {
        time = 0f;
        pawnNumber = 0;
        destroyNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        timeText.text = time.ToString();
        scoreText.text = destroyNumber.ToString() + " / " + gameClearNumber.ToString();
        if(time > respawnTime && pawnNumber < gameClearNumber)
        {
            float x = Random.Range(playAreaA.position.x, playAreaB.position.x);
            float y = playAreaA.position.y;
            float z = Random.Range(playAreaA.position.z, playAreaB.position.z);

            Instantiate(enemyPrefab, new Vector3(x, y, z), enemyPrefab.transform.rotation);
            pawnNumber += 1;
            time = 0f;
        }

        if(destroyNumber >= gameClearNumber)
        {
            //Debug.Log("Game Clear");
            clearModeObjects.SetActive(true);
            gameModeObjects.SetActive(false);
        }
    }
}
