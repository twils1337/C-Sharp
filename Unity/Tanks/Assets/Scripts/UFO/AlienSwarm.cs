using UnityEngine;
using System.Collections;

public class AlienSwarm : MonoBehaviour
{
    public bool m_IsAttachedtoTargetTank;
    public int m_TargetTank = 1;
    public float m_Timer = 0.0f;
    private float m_LifeTime = 7.0f;
    private float m_AttackTimer = 0.0f;
    private float m_AttackInterval = 1.0f;
    private float m_Damage = 1.0667f;

    private void Start()
    {
        Transform endLight = transform.GetChild(0);
        endLight.GetComponent<Light>().enabled = false;
    }
	// Update is called once per frame
	void Update ()
    {
        m_Timer += Time.deltaTime;
        GameObject enemyTank = GetEnemyTank(m_TargetTank);
        if (enemyTank)
        {
            transform.position = m_IsAttachedtoTargetTank ? enemyTank.transform.position : Vector3.MoveTowards(transform.position, enemyTank.transform.position, .07f);
            TankMovement enemyMovement = enemyTank.GetComponent<TankMovement>();
            if (m_IsAttachedtoTargetTank)
            {
                Attack(ref enemyTank, ref enemyMovement);
            }
            else   //swarm is moving towards target
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

    public static void SpawnAlienSwarm(ref GameObject swarmPrefab,Transform landingTransform, int targetID, bool hitEnemyTank)
    {
        GameObject swarm = Instantiate(swarmPrefab, landingTransform.position, landingTransform.rotation) as GameObject;
        swarm.GetComponent<AlienSwarm>().m_IsAttachedtoTargetTank = hitEnemyTank;
        swarm.GetComponent<AlienSwarm>().m_TargetTank = targetID;
    }


    private GameObject GetEnemyTank(int playerID)
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
                m_IsAttachedtoTargetTank = true;
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
            Transform UFO_Shooter = transform.GetChild(i).transform.GetChild(1).transform.GetChild(0);
            //parent -> child
            //alien -> UFO -> UFO Shooter
            UFO_Shooter.GetComponent<EllipsoidParticleEmitter>().enabled = on;
            UFO_Shooter.GetComponent<ParticleRenderer>().enabled = on;
            UFO_Shooter.GetComponent<LightningBolt>().enabled = on;
        }
    }

    private void Attack(ref GameObject enemyTank, ref TankMovement enemyMovement)
    {
        m_AttackTimer += Time.deltaTime;
        ToggleAliensShooting(true);
        enemyMovement.m_AliensSlowingSpeed = true;
        if (!transform.GetChild(0).GetComponent<Light>().enabled)
        {
            Transform endLight = transform.GetChild(0);
            endLight.GetComponent<Light>().enabled = true;
        }
        if (m_AttackTimer > m_AttackInterval)
        {
            m_AttackTimer = 0.0f;
            enemyTank.GetComponent<TankHealth>().TakeDamage(m_Damage);
        }
    }
}
