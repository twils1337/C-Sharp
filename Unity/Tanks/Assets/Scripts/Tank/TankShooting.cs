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
    public bool m_ThreeBurstShotActive = false;
    public bool m_ConeShotActive = false;
    public bool m_BigBulletActive = false;
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
                if (m_ThreeBurstShotActive)
                {
                    StartCoroutine(FireThree());
                    m_ThreeBurstShotActive = false;
                }
                else if (m_ConeShotActive)
                {
                    StartCoroutine(ConeShot());
                    m_ConeShotActive = false;
                }
                else
                {
                    Fire();
                    if (m_BigBulletActive)
                    {
                        m_BigBulletActive = false;
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
                if (m_ThreeBurstShotActive)
                {
                    StartCoroutine(FireThree());
                    m_ThreeBurstShotActive = false;
                }
                else if (m_ConeShotActive)
                {
                    StartCoroutine(ConeShot());
                    m_ConeShotActive = false;
                }
                else
                {
                    Fire();
                    if (m_BigBulletActive)
                    {
                        m_BigBulletActive = false;
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
        Rigidbody shellInstance = m_BigBulletActive ? Instantiate(m_BigShell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody:
                                              Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        shellInstance.GetComponent<ShellExplosion>().m_IsBigBullet = m_BigBulletActive;
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
        m_ConeShotActive = false;
    }

    //Gives the Cone spread of the shot
    private void FireTwo()
    {
        AutoCorrectFireTransform();
        m_Fired = true;
        m_CurrentAmmo -= 2;
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

    private void AutoCorrectFireTransform()
    {
      m_FireTransform.localPosition = new Vector3(0f, 1.7f, 1.35f);
      m_FireTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
    }
}