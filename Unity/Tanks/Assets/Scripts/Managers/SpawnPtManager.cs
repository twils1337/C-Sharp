using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnPtManager : MonoBehaviour
{
    public int ActiveCarePackages { get; set; }
    private int MaxCarePackagesActive = 7;
    public List<Rigidbody> CarePkgs;
    public List<GameObject> SpawnPts;
    public float SpawnTimer = 0.0f;
    public bool firstSpawn = true;      //will spawn the max amount of care packages initially, then spawn more as needed
    private float timeToGoOff = 30.0f;
	
    void Start()
    {
        ActiveCarePackages = MaxCarePackagesActive;
    }
	// Update is called once per frame
	void Update ()
    {
        SpawnTimer += Time.deltaTime;
        if (firstSpawn)
        {
            firstSpawn = false;
            for (int i = 0; i < MaxCarePackagesActive; ++i)
            {
                SpawnPkg();
            }
            ActiveCarePackages = MaxCarePackagesActive;
        }
        else if (ActiveCarePackages < MaxCarePackagesActive &&
                SpawnTimer >= timeToGoOff)
        {
            SpawnPkg();
            ++ActiveCarePackages;
            SpawnTimer = 0.0f;
        }
	}

    private void SpawnPkg()
    {
        var randomSpawn = Random.Range(1, 10);
        var randomCarePackage = Random.Range(0, CarePkgs.Count);
        Transform spawnTransform = SpawnPts[randomSpawn].transform;
        Rigidbody carePkg = CarePkgs[randomCarePackage];
        CarePackage.Type CPType;
        switch (randomCarePackage)
        {
            case 0:
                CPType = CarePackage.Type.Bullet;
                break;
            case 1:
                CPType = CarePackage.Type.Health;
                break;
            case 2:
                CPType = CarePackage.Type.ThreeBurst;
                break;
            case 3:
                CPType = CarePackage.Type.Speed;
                break;
            case 4:
                CPType = CarePackage.Type.ConeShot;
                break;
            case 5:
                CPType = CarePackage.Type.BigBullet;
                break;
            default:
                CPType = CarePackage.Type.Bullet;
                break;
        }
        CarePackage.SpawnCarePkg(ref carePkg, spawnTransform, CPType);
    }

    //Used to gather all the care packages active and get rid of them for resetting the 
    //scene every round
    public void reset()
    {
        ActiveCarePackages = 0;
        GameObject[] AllCarePackages = GameObject.FindGameObjectsWithTag("CarePkg");
        foreach (var CarePkg in AllCarePackages)
        {
            Destroy(CarePkg);
        }
        firstSpawn = true;
    }
}
