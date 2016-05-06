using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
using System;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;


    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    //extension
    public CarePackageManager m_CarePackageManager;
    public Text m_Player1_Info;
    public Text m_Player2_Info;
    public Image m_Player1_SpeedBuff;
    public Image m_Player2_SpeedBuff;


    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        m_CarePackageManager.Reset();
        DestroyAllSwarms();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;
        yield return m_StartWait;
    }

    private void DestroyAllSwarms()
    {
        GameObject[] swarms = GameObject.FindGameObjectsWithTag("Aliens");
        for (int i = 0; i < swarms.Length; i++)
        {
            Destroy(swarms[i].gameObject);
        }
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;
        m_Player1_SpeedBuff.enabled = true;
        m_Player2_SpeedBuff.enabled = true;
        while (!OneTankLeft())
        {
            DisplayUI();
            DisplayAmmo();
            yield return null;
        }
    }

    private void DisplayUI()
    {
        DisplaySpeedBuff();
        DisplayAmmo();
    }

    private void DisplaySpeedBuff()
    {
        Color player1Color = m_Player1_SpeedBuff.color;
        Color player2Color = m_Player2_SpeedBuff.color;
        if (m_Tanks[0].m_Instance.GetComponent<TankMovement>().m_HasSpeedBuff)
        {
            player1Color[3] = 255f;
            m_Player1_SpeedBuff.color = player1Color;
        }
        else
        {
            player1Color[3] = 0f;
            m_Player1_SpeedBuff.color = player1Color;
        }
        if (m_Tanks[1].m_Instance.GetComponent<TankMovement>().m_HasSpeedBuff)
        {
            player2Color[3] = 255f;
            m_Player2_SpeedBuff.color = player2Color;
        }
        else
        {
            player2Color[3] = 0f;
            m_Player2_SpeedBuff.color = player2Color;
        }
    }

    private void DisplayAmmo()
    {
        string Shooting_Disp_Player1 = getAmmoDisplay(1);
        string Shooting_Disp_Player2 = getAmmoDisplay(2);
        m_Player1_Info.text = Shooting_Disp_Player1;
        m_Player2_Info.text = Shooting_Disp_Player2;
    }

    private string getAmmoDisplay(int Player_i)
    {
        TankManager player = m_Tanks[Player_i - 1];
        TankShooting playerShootComponent = player.m_Instance.GetComponent<TankShooting>();
        StringBuilder Player_Info = new StringBuilder("<color=#" + ColorUtility.ToHtmlStringRGB(player.m_PlayerColor) + ">Ammo: " + playerShootComponent.m_CurrentAmmo + "</color>");
        Player_Info.Append("\n");
        Player_Info.Append("<color=#" + ColorUtility.ToHtmlStringRGB(player.m_PlayerColor) + ">Special: ");
        if (playerShootComponent.m_HasThreeBurst)
        {
            Player_Info.Append("Three Burst Shot</color>");
        }
        else if (playerShootComponent.m_HasConeShot)
        {
            Player_Info.Append("Cone Shot</color>");
        }
        else if (playerShootComponent.m_HasBigBullet)
        {
            Player_Info.Append("Big Bullet</color>");
        }
        else if (playerShootComponent.m_HasAlienSignal)
        {
            Player_Info.Append("Alien Signal Bullet</color>");
        }
        else
        {
            Player_Info.Append("None</color>");
        }
        return Player_Info.ToString();
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        m_RoundWinner = null;
        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;
        }
        m_Player1_Info.text = string.Empty;
        m_Player2_Info.text = string.Empty;
        m_Player1_SpeedBuff.enabled = false;
        m_Player2_SpeedBuff.enabled = false;
        m_GameWinner = GetGameWinner();
        string message = EndMessage();
        m_MessageText.text = message;
        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }


    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        string message = "DRAW!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}