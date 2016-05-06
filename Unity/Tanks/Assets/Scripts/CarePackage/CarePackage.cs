using UnityEngine;
using System.Collections;

public class CarePackage : MonoBehaviour
{
    public enum PackageType
    {
        Bullet, ThreeBurst, Health, Speed, ConeShot, BigBullet, AlienSignalBullet
    }
    public PackageType m_Type { get; set; }
    private float m_HealthBenefit = 25.0f;
    public bool m_WasSpawned = false;

    public static Rigidbody SpawnCarePackage(ref Rigidbody CarePkgPrefab, Transform transform, PackageType CPtype, bool fromManager)
    {
        Rigidbody newCarePkg = Instantiate(CarePkgPrefab, transform.position, transform.rotation) as Rigidbody;
        newCarePkg.GetComponent<CarePackage>().m_Type = CPtype;
        newCarePkg.GetComponent<CarePackage>().m_WasSpawned = fromManager;
        return newCarePkg;
    }

    private void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            ContactPoint contact = collision.contacts[i];
            if (collision.contacts[i].otherCollider.tag == "Tank")
            {
                TankMovement movementComponent = collision.contacts[i].otherCollider.GetComponent<TankMovement>();
                if (!movementComponent.m_HasCollided)
                {
                    movementComponent.m_HasCollided = true;
                    if (m_Type == PackageType.Health || m_Type == PackageType.Speed)
                    {
                        ProcessBuffPackage(collision.contacts[i], m_Type);
                    }
                    else
                    {
                        ProcessBulletPackage(contact, m_Type);
                    }
                    RemoveCarePackage();
                }
            }
        }
    }

    private void RemoveCarePackage()
    {
        if (m_WasSpawned)
        {
            GameObject carePackageManager = GameObject.Find("Care Package Spawn Points");
            if (carePackageManager)
            {
                --carePackageManager.GetComponent<CarePackageManager>().m_ActiveCarePackages;
            }
        }
        Destroy(gameObject);
    }

    void ProcessBulletPackage(ContactPoint contact, PackageType bulletType)
    {
        TankShooting shootingComponent = contact.otherCollider.GetComponent<TankShooting>();
        if (bulletType != PackageType.Bullet && bulletType != PackageType.BigBullet)    //Tri-Burst Shot, Cone Shot
        {
            ReloadAndUpdateBullets(ref shootingComponent, bulletType, reload: 3);
        }
        else //Bullet, BigBullet, Alien Signal
        {
            ReloadAndUpdateBullets(ref shootingComponent, bulletType, reload: 1);
        }
    }

    private void ReloadAndUpdateBullets(ref TankShooting shootingComponent, PackageType bulletType, int reload)
    {
        if (OverCapacity(shootingComponent.m_CurrentAmmo, reload, shootingComponent.m_AmmoCapacity))
        {
            shootingComponent.m_CurrentAmmo = shootingComponent.m_AmmoCapacity;
        }
        else
        {
            shootingComponent.m_CurrentAmmo += reload;
        }
        if (bulletType != PackageType.Bullet)
        {
            RemoveBulletBuffs(ref shootingComponent);
        }
        switch (bulletType)
        {
            case PackageType.ThreeBurst:
                shootingComponent.m_HasThreeBurst = true;
                break;
            case PackageType.ConeShot:
                shootingComponent.m_HasConeShot = true;
                break;
            case PackageType.BigBullet:
                shootingComponent.m_HasBigBullet = true;
                break;
            case PackageType.AlienSignalBullet:
                shootingComponent.m_HasAlienSignal = true;
                break;
            default:
                break;
        }
    }
    private void ProcessBuffPackage(ContactPoint contact, PackageType buffType)
    {
        TankHealth healthComponent = contact.otherCollider.GetComponent<TankHealth>();
        TankMovement movementComponent = contact.otherCollider.GetComponent<TankMovement>();
        switch (buffType)
        {
            case PackageType.Health:
                float healthMax = 100.0f;
                if ((healthComponent.m_CurrentHealth + m_HealthBenefit) > healthMax)
                {
                    healthComponent.m_CurrentHealth = healthMax;
                    healthComponent.SetHealthUI();
                }
                else
                {
                    healthComponent.m_CurrentHealth += m_HealthBenefit;
                    healthComponent.SetHealthUI();
                }
                break;
            case PackageType.Speed:
                movementComponent.m_HasSpeedBuff = true;
                break;
            default:
                break;
        }
    }

    private bool OverCapacity(int currentAmmo, int reloadAmmo, int ammoCapacity)
    {
        return currentAmmo + reloadAmmo > ammoCapacity;
    }

    private void RemoveBulletBuffs(ref TankShooting tank)
    {
        tank.RemoveAllBulletBuffs();
    }
}