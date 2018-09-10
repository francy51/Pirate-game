using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Timers;
using System;

public class CloudState : MonoBehaviour
{

    public int numberOfCloudsToInstantiate;

    public List<GameObject> clouds;

    public List<Color> materialColorBasedOnState;

    public GameObject prefab;

    public Material cloudMaterial;

    public Transform container;

    Thread weatherThread;

    //
    //1 minute == 60000 milliseconds
    //
    [Header("1 minute == 60000 milliseconds")]
    public int timerInervalInMilliseconds;

    System.Timers.Timer timer;

    public static WeatherState weatherState;

    public WeatherState curState;

    public float chanceOfStorm;
    public float chanceOfCloud;

    static System.Random random = new System.Random();
    public float GetRandomNumber(float minimum, float maximum)
    {
        return (float)(random.NextDouble() * (maximum - minimum) + minimum);
    }

    public GameObject player;       //Public variable to store a reference to the player game object



    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        transform.position = new Vector3(player.transform.position.x, 100, player.transform.position.z);
    }

    private void Awake()
    {
        clouds = new List<GameObject>();
        for (int i = 0; i < numberOfCloudsToInstantiate; i++)
        {
            var obj = Instantiate(prefab, new Vector3(UnityEngine.Random.Range(-366, 366), UnityEngine.Random.Range(20, 25), UnityEngine.Random.Range(-366, 366)), Quaternion.identity, container);

            obj.GetComponent<ParticleSystem>().maxParticles = UnityEngine.Random.Range(0, 5);
            obj.GetComponent<ParticleSystem>().startDelay = UnityEngine.Random.Range(0, 5);
            clouds.Add(obj);
        }
        ThreadStart start = new ThreadStart(weatherTick);
        Thread weatherThread = new Thread(start);
        weatherThread.Start();

    }

    private void Update()
    {
        //checked the randomise function
        //Debug.Log(GetRandomNumber(0, 100));
        if (curState != weatherState)
        {
            switch (weatherState)
            {
                case WeatherState.clear:
                    clearSky();
                    break;
                case WeatherState.cloudy:
                    break;
                case WeatherState.storm:
                    cloudMaterial.color = Color.grey;
                    break;
                default:
                    break;
            }
            curState = weatherState;
        }
    }

    private void clearSky()
    {
        for (int i = 0; i < numberOfCloudsToInstantiate - ((numberOfCloudsToInstantiate / 0.1) * 6); i++)
        {
            clouds[i].SetActive(false);
        }
        cloudMaterial.color = new Color(255, 255, 255, .5f);
    }

    void weatherTick()
    {
        Thread.Sleep(timerInervalInMilliseconds);
        RandomiseState();
    }

    private void RandomiseState()
    {
        float randomChance = GetRandomNumber(0f, 100f);
        if (randomChance > chanceOfCloud)
        {
            weatherState = WeatherState.clear;

        }
        else if (randomChance > chanceOfStorm)
        {
            weatherState = WeatherState.cloudy;
        }
        else
        {
            weatherState = WeatherState.storm;
        }
        weatherTick();
    }
}

public enum WeatherState
{
    clear,
    cloudy,
    storm
}