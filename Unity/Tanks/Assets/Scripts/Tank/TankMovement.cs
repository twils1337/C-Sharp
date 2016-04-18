using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;         
    public float m_Speed = 12f;            
    public float m_TurnSpeed = 180f;       
    public AudioSource m_MovementAudio;    
    public AudioClip m_EngineIdling;       
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;

    
    private string m_MovementAxisName;     
    private string m_TurnAxisName;         
    private Rigidbody m_Rigidbody;         
    private float m_MovementInputValue;    
    private float m_TurnInputValue;        
    private float m_OriginalPitch;

    //extension
    public bool m_HasSpeedBuff = false;
    private float m_BoostFactor = 2.5f;
    public float m_BuffTimer = 0.0f;
    private float m_BuffPeriod = 1.5f;
    private string m_TurboButton;
    private bool m_TurboPressed;
    private bool m_ActiveTurbo;


    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable ()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        m_OriginalPitch = m_MovementAudio.pitch;

        m_TurboButton = "Turbo" + m_PlayerNumber;
    }


    private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
        m_TurboPressed = Input.GetButtonDown(m_TurboButton);
        EngineAudio();
    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        if (Mathf.Abs(m_MovementInputValue) < 0.01f && Mathf.Abs(m_TurnInputValue) < .01f)
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }


    private void FixedUpdate()
    {
        // Move and turn the tank.
        m_BuffTimer += Time.deltaTime;
        Move();
        Turn();
    }


    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
        Vector3 movement;
        if (m_TurboPressed)
        {
            if (m_HasSpeedBuff) //if speed buff not active, and activate speed buff
            {
                m_ActiveTurbo = true;
                m_HasSpeedBuff = false;
                m_BuffTimer = 0.0f;
            }
        }
        if (m_ActiveTurbo && m_BuffTimer <= m_BuffPeriod)
        {
            movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime * m_BoostFactor;
        }
        else
        {
            //Debug.Log("O");
            if (m_ActiveTurbo && m_BuffTimer > m_BuffPeriod)
            {
               m_ActiveTurbo = false;
            }
            movement = transform.forward* m_MovementInputValue *m_Speed * Time.deltaTime;
        }
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

}