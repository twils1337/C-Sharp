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
    public bool m_IsBigBullet;
    public Rigidbody m_BulletCarePackage;


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
        if (m_IsBigBullet)
        {
            canPickUp = false;
        }
        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].GetComponent<Rigidbody>())
            {
                if (colliders[i].tag != "CarePackageSafe")
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
            float damage = CalculateDamage(targetRigidBody.position);
            targetHealth.TakeDamage(damage);
            canPickUp = false;
        }
        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();
        Transform Care_Pkg_Transform = gameObject.transform;
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
        Destroy(gameObject);
        if (canPickUp)
        {
            Care_Pkg_Transform.transform.Translate(0, 1f, 0);
            CarePackage.SpawnCarePackage(ref m_BulletCarePackage, Care_Pkg_Transform, CarePackage.Type.Bullet);
        }
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
}