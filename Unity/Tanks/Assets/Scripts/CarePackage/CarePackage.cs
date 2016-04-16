using UnityEngine;
using System.Collections;

public class CarePackage : MonoBehaviour
{
    public enum Type
    {
        Bullet, ThreeBurst, Health, Speed, ConeShot, BigBullet
    }

    public Type type { get; set; }
    public float HealthBenefit = 25.0f;
    private int hits = 1;


    public static void SpawnCarePkg(ref Rigidbody CarePkgPrefab,Transform transform, Type CPtype)
    {
        Rigidbody newCarePkg = Instantiate(CarePkgPrefab, transform.position, transform.rotation) as Rigidbody;
        newCarePkg.GetComponent<CarePackage>().type = CPtype;
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision)
        {
            if (contact.otherCollider.tag == "Tank" && hits < 2)
            {
                ++hits;
                TankShooting shootingComp = contact.otherCollider.GetComponent<TankShooting>();
                TankHealth healthComp = contact.otherCollider.GetComponent<TankHealth>();
                TankMovement movementComp = contact.otherCollider.GetComponent<TankMovement>();
                switch (type)
                {
                    case CarePackage.Type.Bullet:
                        if (shootingComp.currentAmmo < shootingComp.ammoCapacity)
                        {
                            shootingComp.currentAmmo++;
                        }
                        break;
                    case CarePackage.Type.ThreeBurst:
                        if ((shootingComp.currentAmmo + 3) > shootingComp.ammoCapacity)
                        {
                            shootingComp.currentAmmo = shootingComp.ammoCapacity;
                        }
                        else
                        {
                            shootingComp.currentAmmo += 3;
                        }
                        if (!shootingComp.ThreeBurstShotActive && !shootingComp.BigBulletActive)
                        {
                            shootingComp.ThreeBurstShotActive = true;
                        }
                        break;
                    case CarePackage.Type.Health:
                        float healthMax = 100.0f;
                        if ((healthComp.m_CurrentHealth + HealthBenefit) > healthMax)
                        {
                            healthComp.m_CurrentHealth = healthMax;
                            healthComp.SetHealthUI();
                        }
                        else
                        {
                            healthComp.m_CurrentHealth += HealthBenefit;
                            healthComp.SetHealthUI();
                        }
                        break;
                    case CarePackage.Type.Speed:
                        movementComp.BuffTimer = 0.0f;
                        movementComp.SpeedBoosted = true;
                        break;
                    case CarePackage.Type.ConeShot:
                        if ((shootingComp.currentAmmo + 3) > shootingComp.ammoCapacity)
                        {
                            shootingComp.currentAmmo = shootingComp.ammoCapacity;
                        }
                        else
                        {
                            shootingComp.currentAmmo += 3;
                        }
                        if (!shootingComp.ThreeBurstShotActive && !shootingComp.BigBulletActive)
                        {
                            shootingComp.ConeShotActive = true;
                        }
                        break;
                    case CarePackage.Type.BigBullet:
                        if ((shootingComp.currentAmmo + 1) < shootingComp.ammoCapacity)
                        {
                            shootingComp.currentAmmo++;
                        }
                        if (!shootingComp.ThreeBurstShotActive && !shootingComp.ConeShotActive)
                        {
                            shootingComp.BigBulletActive = true;
                        }
                        break;
                    default:
                        break;
                }
                --GameObject.Find("Care Package Spawn Points").GetComponent<SpawnPtManager>().ActiveCarePackages;
                Destroy(gameObject);
            }
        }

    }
}
