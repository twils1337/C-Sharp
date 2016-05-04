using UnityEngine;
using System.Collections;

public class AlienSwarm : MonoBehaviour
{
    public bool m_isAttachedtoTargetTank;
    public int m_TargetTank = 1;
    public float m_Timer;
    private float m_LifeTime = 15.0f;

	
	// Update is called once per frame
	void Update ()
    {
        m_Timer += Time.deltaTime;
        Transform endLight = transform.GetChild(0); //Will get EndLight
        endLight.GetComponent<Light>().enabled = m_isAttachedtoTargetTank;
        GameObject enemyTank = GetEnemyTank(m_TargetTank);
        if (enemyTank)
        {
            transform.position = m_isAttachedtoTargetTank ? enemyTank.transform.position : Vector3.MoveTowards(transform.position, enemyTank.transform.position, .07f);
            TankMovement enemyMovement = enemyTank.GetComponent<TankMovement>();
            if (m_isAttachedtoTargetTank)
            {
                ToggleAliensShooting(true);
                enemyMovement.m_AliensSlowingSpeed = true;
            }
            else
            {
                ToggleAliensShooting(false);
                enemyMovement.m_AliensSlowingSpeed = false;
            }
            if (m_Timer > m_LifeTime)
            {
                enemyMovement.m_AliensSlowingSpeed = false;
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
	}
    
    public static void SpawnSwarm(ref GameObject swarmPrefab,Transform landingTransform, int targetID, bool hitEnemyTank)
    {
        GameObject swarm = Instantiate(swarmPrefab, landingTransform.position,landingTransform.rotation) as GameObject;
        swarm.GetComponent<AlienSwarm>().m_isAttachedtoTargetTank = hitEnemyTank;
        swarm.GetComponent<AlienSwarm>().m_TargetTank = targetID;
    }

    public static void SpawnSwarm(ref GameObject swarmPrefab, Transform landingTransform, bool hitEnemyTank)
    {
        GameObject swarm = Instantiate(swarmPrefab, landingTransform.position, landingTransform.rotation) as GameObject;
        swarm.GetComponent<AlienSwarm>().m_isAttachedtoTargetTank = hitEnemyTank;
    }

    public GameObject GetEnemyTank(int playerID)
    {
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
        if (tanks.Length > 1)
        {
            if (tanks[0].GetComponent<TankMovement>().m_PlayerNumber == playerID)
            {
                return tanks[0];
            }
            else
            {
                return tanks[1];
            }
        }
        return null;
    }

    public void OnTriggerEnter(Collider other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Tank" && colliders[i].GetComponent<TankMovement>().m_PlayerNumber == m_TargetTank)
            {
                m_isAttachedtoTargetTank = true;
                transform.position = other.GetComponent<Transform>().position;
                break;
            }
        }
    }

    public void ToggleAliensShooting(bool on)
    {
        int aliensInSwarm = 3;
        for (int i = 1; i <= aliensInSwarm; i++)
        {
            Transform UFO_Shooter = transform.GetChild(i).transform.GetChild(1).transform.GetChild(0); //alien -> UFO -> UFO Shooter
            UFO_Shooter.GetComponent<EllipsoidParticleEmitter>().enabled = on;
            UFO_Shooter.GetComponent<ParticleRenderer>().enabled = on;
            UFO_Shooter.GetComponent<LightningBolt>().enabled = on;
        }
    }
}
