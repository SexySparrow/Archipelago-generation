using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Daytime : MonoBehaviour
{
    public GameObject sun;
    private void FixedUpdate()
    {
        sun.transform.Rotate(Vector3.right * Time.deltaTime * -0.5f);
    }
}
