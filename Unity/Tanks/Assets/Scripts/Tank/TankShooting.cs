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
    public int ammoCapacity = 5;
    public int currentAmmo;
    public bool ThreeBurstShotActive = false;
    public bool ConeShotActive = false;
    public bool BigBulletActive = false;
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

        currentAmmo = ammoCapacity;

    }

    private void Update()
    {
        // Track the current state of the fire button and make decisions based on the current launch force.
        m_AimSlider.value = m_MinLaunchForce;
        if (currentAmmo >= 1)
        {
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                //max charged, not yet fired
                m_CurrentLaunchForce = m_MaxLaunchForce;
                if (ThreeBurstShotActive)
                {
                    StartCoroutine(FireThree());
                    ThreeBurstShotActive = false;
                }
                else if (ConeShotActive)
                {
                    StartCoroutine(ConeShot());
                    ConeShotActive = false;
                }
                else
                {
                    Fire();
                    if (BigBulletActive)
                    {
                        BigBulletActive = false;
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
                if (ThreeBurstShotActive)
                {
                    StartCoroutine(FireThree());
                    ThreeBurstShotActive = false;
                }
                else if (ConeShotActive)
                {
                    StartCoroutine(ConeShot());
                    ConeShotActive = false;
                }
                else
                {
                    Fire();
                    if (BigBulletActive)
                    {
                        BigBulletActive = false;
                    }
                }
            }
        }
    }

    private void Fire()
    {
        // Instantiate and launch the shell.
        m_Fired = true;
        --currentAmmo;
        Rigidbody shellInstance = BigBulletActive ? Instantiate(m_BigShell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody:
                                              Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        shellInstance.GetComponent<ShellExplosion>().BigBullet = BigBulletActive;
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
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
        // Instantiate and launch the shell.
        m_Fired = true;
        --currentAmmo;
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
        ConeShotActive = false;
    }

    //Gives the Cone spread of the shot
    private void FireTwo()
    {
        m_Fired = true;
        currentAmmo -= 2;
        m_FireTransform.Rotate(0, 5.0f, 0);
        m_FireTransform.Translate(1.0f, 0, 0);
        Rigidbody shellInstance1 = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        shellInstance1.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
        m_FireTransform.Rotate(0, -10.0f, 0);
        m_FireTransform.Translate(-2.0f, 0, 0);
        Rigidbody shellInstance2 = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        shellInstance2.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_FireTransform.Rotate(0, 5.0f, 0);
        m_FireTransform.Translate(1.0f, 0, 0);
    }
}