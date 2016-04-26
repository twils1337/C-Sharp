using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarePackageManager : MonoBehaviour
{
    public int m_ActiveCarePackages { get; set; }
    public List<Rigidbody> m_CarePackages;
    public List<GameObject> m_SpawnPoints;
    private int m_MaxCarePackagesActive = 7;
    private float m_SpawnTimer = 0.0f;
    private bool m_FirstSpawn = true;      //will spawn the max amount of care packages initially, then spawn more as needed
    private float m_TimeToGoOff = 30.0f;
	
	// Update is called once per frame
	void Update ()
    {
        m_SpawnTimer += Time.deltaTime;
        if (m_FirstSpawn)
        {
            m_FirstSpawn = false;
            for (int i = 0; i < m_MaxCarePackagesActive; ++i)
            {
                SpawnCarePackage();
            }
            m_ActiveCarePackages = m_MaxCarePackagesActive;
        }
        else if (m_ActiveCarePackages < m_MaxCarePackagesActive &&
                m_SpawnTimer >= m_TimeToGoOff)
        {
            SpawnCarePackage();
            ++m_ActiveCarePackages;
            m_SpawnTimer = 0.0f;
        }
    }

    private void SpawnCarePackage()
    {
        var randomSpawn = Random.Range(0, 10);
        var randomCarePackage = Random.Range(0, m_CarePackages.Count);
        Transform spawnTransform = m_SpawnPoints[randomSpawn].transform;
        Rigidbody carePkg = m_CarePackages[randomCarePackage];
        CarePackage.Type CPType = GetCarePackageTypeByID(randomCarePackage);
        CarePackage.SpawnCarePackage(ref carePkg, spawnTransform, CPType, true);
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
        m_FirstSpawn = true;
    }
}
