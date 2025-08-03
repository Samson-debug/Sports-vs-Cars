using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting.Dependencies.NCalc;
using Random = UnityEngine.Random;

public class CarManager : MonoBehaviour
{
    [Header("Spawn Config")]
    public float spawnDistanceFromTruck = 30;
    public List<GameObject> cars;
    public List<Transform> lanes;

    [Header("Ram")]
    public float timeBetweenNextRam = 3f;
    
    //For Granade throw
    [SerializedDictionary("Lane", "Racquet")]
    public SerializedDictionary<Transform, BatController> batControllers;
    
    Truck truck;
    private Dictionary<Transform, GameObject> carsOnLane = new ();


    private void Awake()
    {
        truck = FindFirstObjectByType<Truck>();
    }

    private void Start()
    {
        if(lanes == null || lanes.Count == 0)
            Debug.LogError("Assign Lanes to Spawn car");
        if(cars == null || cars.Count == 0)
            Debug.LogError("Assign Cars to Spawn");
        if(truck == null)
            Debug.LogError("No Truck to follow");
        
        foreach (Transform lane in lanes){
            var car = SpawnCarOnPos(lane.position);
            Chaser chaser = car.GetComponent<Chaser>();
            Health carHealth = car.GetComponent<Health>();

            chaser.OnRamComplete += StartRamming;
            carHealth.OnDeath += DisableCar;
            carHealth.OnDestroyed += SpawnCar;
            carsOnLane[lane] = car;
            
            if(batControllers.ContainsKey(lane))
                batControllers[lane].SetTargetCar(car);
        }
        
        StartRamming();
    }

    private void SpawnCar(GameObject destroyedCar)
    {
        print($"Destroyed Car Position : {destroyedCar.transform.position}");
        if(GameManager.Instance.Paused) return;
        
        Transform lane = FindLane(destroyedCar);
        
        if(lane == null) return;
        
        Vector3 spawnPos = new Vector3(lane.position.x, lane.position.y, truck.Position.z - spawnDistanceFromTruck);
        print($"New Car Position : {spawnPos}");
        var car = SpawnCarOnPos(spawnPos);
        Chaser chaser = car.GetComponent<Chaser>();
        Health carHealth = car.GetComponent<Health>();

        chaser.OnRamComplete += StartRamming;
        carHealth.OnDeath += DisableCar;
        carHealth.OnDestroyed += SpawnCar;
        carsOnLane[lane] = car;
        
        //grenade throw
        if(batControllers.ContainsKey(lane))
            batControllers[lane].SetTargetCar(car);
    }

    private Transform FindLane(GameObject car)
    {
        foreach (var carAndLane in carsOnLane){
            if (carAndLane.Value == car)
                return carAndLane.Key;
        }

        return null;
    }

    private GameObject SpawnCarOnPos(Vector3 position)
    {
        return Instantiate(cars[Random.Range(0, cars.Count)], position, Quaternion.identity);
    }
    

    private void DisableCar(GameObject car)
    {
        Transform lane = FindLane(car);
        
        if(lane == null) return;
        
        BatController racquet = batControllers[lane];
        racquet.RemoveTargetCar();
    }

    private void StartRamming()
    {
        if(GameManager.Instance.Paused) return;
        StartCoroutine(AskToRam());
    }

    private IEnumerator AskToRam()
    {
        yield return new WaitForSeconds(timeBetweenNextRam);

        List<Chaser> cars = new List<Chaser>();
        foreach (var carObj in carsOnLane.Values){
            Health carHealth = carObj.GetComponent<Health>();
            if(carHealth.Dead) continue;
            cars.Add(carObj.GetComponent<Chaser>());
        }
        //var cars = carsOnLane.Values.ToList();

        if (cars.Count == 0){
            yield return new WaitForSeconds(timeBetweenNextRam);
            
            StartRamming();
            yield break; //end corotine early
        }
        
        var car = cars[Random.Range(0, cars.Count)];
        
        if(car == null)
            Debug.LogError("Car has no Chaser script");

        car.StartRam();

    }
}
