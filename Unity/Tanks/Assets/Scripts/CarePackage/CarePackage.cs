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
    public bool m_WasSpawned = false;
    
    public static Rigidbody SpawnCarePackage(ref Rigidbody CarePkgPrefab,Transform transform, Type CPtype, bool fromManager)
    {
        Rigidbody newCarePkg = Instantiate(CarePkgPrefab, transform.position, transform.rotation) as Rigidbody;
        newCarePkg.GetComponent<CarePackage>().m_Type = CPtype;
        newCarePkg.GetComponent<CarePackage>().m_WasSpawned = fromManager;
        return newCarePkg;
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
                    if (m_Type == Type.Health || m_Type == Type.Speed)
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

    void ProcessBulletPackage(ContactPoint contact, Type bulletType)
    {
        TankShooting shootingComponent = contact.otherCollider.GetComponent<TankShooting>();
        if (bulletType != Type.Bullet && bulletType != Type.BigBullet)
        {
            ReloadAndUpdateBullets(ref shootingComponent, bulletType, reload: 3);
        }
        else
        {
            ReloadAndUpdateBullets(ref shootingComponent, bulletType, reload: 1);
        }
    }

    private void ReloadAndUpdateBullets(ref TankShooting shootingComponent, Type bulletType, int reload)
    {
        if (OverCapacity(shootingComponent.m_CurrentAmmo, reload, shootingComponent.m_AmmoCapacity))
        {
            shootingComponent.m_CurrentAmmo = shootingComponent.m_AmmoCapacity;
        }
        else
        {
            shootingComponent.m_CurrentAmmo += reload;
        }
        RemoveBulletBuffs(ref shootingComponent);
        switch (bulletType)
        {
            case Type.ThreeBurst:
                shootingComponent.m_ThreeBurstShotActive = true;
                break;
            case Type.ConeShot:
                shootingComponent.m_ConeShotActive = true;
                break;
            case Type.BigBullet:
                shootingComponent.m_BigBulletActive = true;
                break;
            default:
                break;
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