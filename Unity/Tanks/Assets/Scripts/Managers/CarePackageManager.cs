using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarePackageManager : MonoBehaviour
{
    public int m_ActiveCarePackages { get; set; }
    public List<Rigidbody> m_CarePackages;
    public List<GameObject> m_SpawnPoints;
    private int MaxCarePackagesActive = 7;
    private float SpawnTimer = 0.0f;
    private bool FirstSpawn = true;      //will spawn the max amount of care packages initially, then spawn more as needed
    private float TimeToGoOff = 30.0f;
    // Use this for initialization
    void Start ()
    {
        m_ActiveCarePackages = MaxCarePackagesActive;
    }
	
	// Update is called once per frame
	void Update ()
    {
        SpawnTimer += Time.deltaTime;
        if (FirstSpawn)
        {
            FirstSpawn = false;
            for (int i = 0; i < MaxCarePackagesActive; ++i)
            {
                SpawnCarePackage();
            }
            m_ActiveCarePackages = MaxCarePackagesActive;
        }
        else if (m_ActiveCarePackages < MaxCarePackagesActive &&
                SpawnTimer >= TimeToGoOff)
        {
            SpawnCarePackage();
            ++m_ActiveCarePackages;
            SpawnTimer = 0.0f;
        }
    }

    private void SpawnCarePackage()
    {
        var randomSpawn = Random.Range(0, 10);
        var randomCarePackage = Random.Range(0, m_CarePackages.Count);
        Transform spawnTransform = m_SpawnPoints[randomSpawn].transform;
        Rigidbody carePkg = m_CarePackages[randomCarePackage];
        CarePackage.Type CPType = GetCarePackageTypeByID(randomCarePackage);
        CarePackage.SpawnCarePackage(ref carePkg, spawnTransform, CPType);
    }

    private CarePackage.Type GetCarePackageTypeByID(int carePackageID)
    {
        switch (carePackageID)
        {
            case 1:
                return CarePackage.Type.Health;
            case 2:
                return CarePackage.Type.ThreeBurst;
            case 3:
                return CarePackage.Type.Speed;
            case 4:
                return CarePackage.Type.ConeShot;
            case 5:
                return CarePackage.Type.BigBullet;
            default:
                return CarePackage.Type.Bullet;
        }
    }

    //Used to gather all the care packages active and get rid of them for resetting the 
    //scene every round
    public void Reset()
    {
        m_ActiveCarePackages = 0;
        GameObject[] AllCarePackages = GameObject.FindGameObjectsWithTag("CarePackage");
        for (int i = 0; i < AllCarePackages.Length; i++)
        {
            Destroy(AllCarePackages[i]);
        }
        FirstSpawn = true;
    }
}
