using UnityEngine;
using System.Collections;

public class CarePackage : MonoBehaviour
{
    public enum Type
    {
        Bullet, ThreeBurst, Health, Speed, ConeShot, BigBullet
    }
    public Type m_Type { get; set; }

    private float HealthBenefit = 25.0f;
    private int Hits = 1;
    
    public static Rigidbody SpawnCarePackage(ref Rigidbody CarePkgPrefab,Transform transform, Type CPtype)
    {
        Rigidbody newCarePkg = Instantiate(CarePkgPrefab, transform.position, transform.rotation) as Rigidbody;
        newCarePkg.GetComponent<CarePackage>().m_Type = CPtype;
        return newCarePkg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision)
        {
            if (contact.otherCollider.tag == "Tank" && Hits < 2)
            {
                ++Hits;
                if (m_Type == Type.Health || m_Type == Type.Speed)
                {
                    ProcessBuffPackage(contact, m_Type);
                }
                else
                {
                    ProcessBulletPackage(contact, m_Type);
                }
                RemoveCarePackage();
            }
        }

    }

    private void RemoveCarePackage()
    {
        GameObject carePackageManager;
        if ( carePackageManager = GameObject.Find("Care Package Spawn Points") )
        {
            --carePackageManager.GetComponent<CarePackageManager>().m_ActiveCarePackages;
            Destroy(gameObject);
        }
    }

    void ProcessBulletPackage(ContactPoint contact, Type bulletType)
    {
        TankShooting shootingComponent = contact.otherCollider.GetComponent<TankShooting>();
        int reload = 0;
        if (bulletType != Type.Bullet && bulletType != Type.BigBullet)
        {
            reload = 3;
            if (OverCapacity(shootingComponent.m_CurrentAmmo, reload, shootingComponent.m_AmmoCapacity))
            {
                shootingComponent.m_CurrentAmmo = shootingComponent.m_AmmoCapacity;
            }
            else
            {
                shootingComponent.m_CurrentAmmo += reload;
            }
            if (bulletType == Type.ThreeBurst)
            {
                if (!shootingComponent.m_ConeShotActive && !shootingComponent.m_BigBulletActive)
                {
                    shootingComponent.m_ThreeBurstShotActive = true;
                }
            }
            else
            {
                    if (!shootingComponent.m_ThreeBurstShotActive && !shootingComponent.m_BigBulletActive)
                    {
                        shootingComponent.m_ConeShotActive = true;
                    }
            }
        }
        else
        {
            reload = 1;
            if ( !OverCapacity(shootingComponent.m_CurrentAmmo, reload, shootingComponent.m_AmmoCapacity))
            {
                shootingComponent.m_CurrentAmmo += reload;
            }
            if (bulletType == Type.BigBullet)
            {
                if (!shootingComponent.m_ThreeBurstShotActive && !shootingComponent.m_ConeShotActive)
                {
                    shootingComponent.m_BigBulletActive = true;
                }
            }
        }
    }

    private void ProcessBuffPackage(ContactPoint contact, Type buffType)
    {
        TankHealth healthComponent = contact.otherCollider.GetComponent<TankHealth>();
        TankMovement movementComponent = contact.otherCollider.GetComponent<TankMovement>();
        switch (buffType)
        {
            case Type.Health:
                float healthMax = 100.0f;
                if ((healthComponent.m_CurrentHealth + HealthBenefit) > healthMax)
                {
                    healthComponent.m_CurrentHealth = healthMax;
                    healthComponent.SetHealthUI();
                }
                else
                {
                    healthComponent.m_CurrentHealth += HealthBenefit;
                    healthComponent.SetHealthUI();
                }
                break;
            case Type.Speed:
                movementComponent.BuffTimer = 0.0f;
                movementComponent.SpeedBoosted = true;
                break;
            default:
                break;
        }
    }

    private bool OverCapacity(int currentAmmo, int reloadAmmo, int ammoCapacity)
    {
        return currentAmmo + reloadAmmo > ammoCapacity;
    }
}