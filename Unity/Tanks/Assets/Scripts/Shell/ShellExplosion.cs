using UnityEngine;


public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask;
    public ParticleSystem m_ExplosionParticles;       
    public AudioSource m_ExplosionAudio;              
    public float m_MaxDamage = 100f;                  
    public float m_ExplosionForce = 1000f;            
    public float m_MaxLifeTime = 2f;              
    public float m_ExplosionRadius = 5f;

    //extension
    public bool m_IsBigBullet= false;
    public Rigidbody m_BulletCarePackage;
    public int m_ShootingPlayer = 1;
    public bool m_IsAlienSignal = false;
    public bool m_DidSignalHitTank;
    public GameObject m_SwarmPrefab;


    private void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
        if (m_IsBigBullet)
        {
            m_MaxDamage = 150f;
            m_ExplosionRadius = 10f;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Find all the tanks in an area around the shell and damage them.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius);
        bool canPickUp = true;
        if (m_IsBigBullet || m_IsAlienSignal)
        {
            canPickUp = false;
        }
        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].GetComponent<Rigidbody>())
            {
                if ( colliders[i].tag != "CarePackageSafe" && colliders[i].tag != "Structure" && colliders[i].tag != "Shell" )
                {
                    canPickUp = false;
                }
                continue;
            }
            Rigidbody targetRigidBody = colliders[i].GetComponent<Rigidbody>();
            targetRigidBody.AddExplosionForce(m_ExplosionForce,transform.position,m_ExplosionRadius);
            TankHealth targetHealth = targetRigidBody.GetComponent<TankHealth>();
            if(!targetHealth)
            {
                continue;
            }
            if (m_ShootingPlayer != colliders[i].GetComponent<TankMovement>().m_PlayerNumber)
            {
                if (m_IsAlienSignal)
                {
                    AlienSwarm.SpawnSwarm(ref m_SwarmPrefab, colliders[i].transform, colliders[i].GetComponent<TankMovement>().m_PlayerNumber, true);
                    m_IsAlienSignal = false;
                }
                else
                {
                    float damage = CalculateDamage(targetRigidBody.position);
                    targetHealth.TakeDamage(damage);
                    canPickUp = false;
                }
            }
        }
        ProcessExplosionParticlesAndDestroy(playExplosion: !m_IsAlienSignal);
        if (m_IsAlienSignal)
        {
            int targetID = m_ShootingPlayer == 1 ? 2 : 1;
            AlienSwarm.SpawnSwarm(ref m_SwarmPrefab, gameObject.transform, targetID, false);
        }
        if (canPickUp)
        {
            SpawnBulletPackage();
        }
        Destroy(gameObject);
    }


    private float CalculateDamage(Vector3 targetPosition)
    {
        // Calculate the amount of damage a target should take based on it's position.
        Vector3 explosionToTarget = targetPosition - transform.position;
        float explosionDistance = explosionToTarget.magnitude;
        float relativeDistance = (m_ExplosionRadius - explosionDistance)/m_ExplosionRadius;
        float damage = relativeDistance * m_MaxDamage;
        damage = Mathf.Max(0f, damage);
        return damage;
    }

    private void ProcessExplosionParticlesAndDestroy(bool playExplosion)
    {
        m_ExplosionParticles.transform.parent = null;
        if (playExplosion)
        {
            m_ExplosionParticles.Play();
            m_ExplosionAudio.Play();
        }
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
    }

    private void SpawnBulletPackage()
    {
        Transform CarePackageTransform = gameObject.transform;
        CarePackageTransform.transform.Translate(0, 1f, 0);
        CarePackage.SpawnCarePackage(ref m_BulletCarePackage, CarePackageTransform, CarePackage.PackageType.Bullet, false);
    }
}