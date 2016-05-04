using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;       
    public Rigidbody m_Shell;            
    public Transform m_FireTransform;    
    public Slider m_AimSlider;           
    public AudioSource m_ShootingAudio;  
    public AudioClip m_ChargingClip;     
    public AudioClip m_FireClip;         
    public float m_MinLaunchForce = 15f; 
    public float m_MaxLaunchForce = 30f; 
    public float m_MaxChargeTime = 0.75f;

    
    private string m_FireButton;         
    private float m_CurrentLaunchForce;  
    private float m_ChargeSpeed;         
    private bool m_Fired;

    //extension
    public int m_AmmoCapacity = 5;
    public int m_CurrentAmmo;
    public bool m_HasThreeBurst = false;
    public bool m_HasConeShot = false;
    public bool m_HasBigBullet = false;
    public bool m_HasAlienSignal = false;
    public Rigidbody m_BigShell;  


    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start()
    {
        m_FireButton = "Fire" + m_PlayerNumber;

        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

        m_CurrentAmmo = m_AmmoCapacity;

    }

    private void Update()
    {
        // Track the current state of the fire button and make decisions based on the current launch force.
        m_AimSlider.value = m_MinLaunchForce;
        if (m_CurrentAmmo >= 1)
        {
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                //max charged, not yet fired
                m_CurrentLaunchForce = m_MaxLaunchForce;
                if (m_HasThreeBurst)
                {
                    StartCoroutine(FireThree());
                    m_HasThreeBurst = false;
                }
                else if (m_HasConeShot)
                {
                    StartCoroutine(ConeShot());
                    m_HasConeShot = false;
                }
                else
                {
                    Fire();
                    if (m_HasBigBullet)
                    {
                        m_HasBigBullet = false;
                    }
                }
            }
            else if (Input.GetButtonDown(m_FireButton))
            {
                //fire button pressed for first time
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play();
            }
            else if (Input.GetButton(m_FireButton) && !m_Fired)
            {
                //charging shot, fire button held down
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
                m_AimSlider.value = m_CurrentLaunchForce;
            }
            else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
            {
                //released fire button, shell will be fired
                if (m_HasThreeBurst)
                {
                    StartCoroutine(FireThree());
                    m_HasThreeBurst = false;
                }
                else if (m_HasConeShot)
                {
                    StartCoroutine(ConeShot());
                    m_HasConeShot = false;
                }
                else if (m_HasAlienSignal)
                {
                    Fire();
                    m_HasAlienSignal = false;
                }
                else
                {
                    Fire();
                    if (m_HasBigBullet)
                    {
                        m_HasBigBullet = false;
                    }
                }
            }
        }
    }

    private void Fire()
    {
        // Instantiate and launch the shell.
        AutoCorrectFireTransform();
        m_Fired = true;
        --m_CurrentAmmo;
        CreateShellAndFire();
        m_CurrentLaunchForce = m_MinLaunchForce;
    }

    //For Burst shot
    IEnumerator FireThree()
    {
        FireBurst(false);
        yield return new WaitForSeconds(.5f);
        FireBurst(false);
        yield return new WaitForSeconds(.5f);
        FireBurst(true);
    }

    private void FireBurst(bool last)
    {
        AutoCorrectFireTransform();
        m_Fired = true;
        --m_CurrentAmmo;
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
        if (last)
        {
            m_CurrentLaunchForce = m_MinLaunchForce;
        }
        
    }

    IEnumerator ConeShot()
    {
        FireTwo();
        yield return new WaitForSeconds(.1f);
        Fire();
        m_HasConeShot = false;
    }

    //Gives the Cone spread of the shot
    private void FireTwo()
    {
        AutoCorrectFireTransform();
        m_Fired = true;
        m_CurrentAmmo -= 2;
        CreateShellAndFire(5.0f, 1.0f);
        CreateShellAndFire(-10.0f, -2.0f);
        m_CurrentLaunchForce = m_MinLaunchForce;
        FireTransformRotateYandTranslateX(5.0f, 1.0f);
    }

    private void CreateShellAndFire(float rotationY = 0.0f, float positionX = 0.0f)
    {
        FireTransformRotateYandTranslateX(rotationY, positionX);
        Rigidbody shellInstance = m_HasBigBullet ? Instantiate(m_BigShell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody :
                                                      Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        ShellExplosion explosion = shellInstance.GetComponent<ShellExplosion>();
        explosion.m_IsBigBullet = m_HasBigBullet;
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
        explosion.m_ShootingPlayer = m_PlayerNumber;
        if (m_HasAlienSignal)
        {
            explosion.m_IsAlienSignal = true;
            m_HasAlienSignal = false;
        }
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
    }

    private void FireTransformRotateYandTranslateX(float rotationY = 0.0f, float positionX = 0.0f)
    {
        m_FireTransform.Rotate(0, rotationY, 0);
        m_FireTransform.Translate(positionX, 0, 0);
    }

    private void AutoCorrectFireTransform()
    {
      m_FireTransform.localPosition = new Vector3(0f, 1.7f, 1.35f); //x,y,z in this new Vector3 are the original values of the fire transform set on initialization
      m_FireTransform.localEulerAngles = new Vector3(0f, 0f, 0f); 
    }

    public void RemoveAllBulletBuffs()
    {
        m_HasThreeBurst = false;
        m_HasBigBullet = false;
        m_HasConeShot = false;
    }

}