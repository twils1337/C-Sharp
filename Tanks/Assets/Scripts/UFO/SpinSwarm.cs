using UnityEngine;
using System.Collections;

public class SpinSwarm : MonoBehaviour
{
    public float m_RotationYAmmount = 0.0f;

    // Update is called once per frame
    void Update ()
    {
        GetComponent<Transform>().Rotate(0.0f, m_RotationYAmmount, 0.0f);
    }
}
