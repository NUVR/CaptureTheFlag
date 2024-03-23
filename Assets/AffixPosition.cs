using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffixPosition : MonoBehaviour
{
    Vector3 originalPosition;
    Quaternion originalRoation;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        originalRoation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = originalPosition;
        transform.rotation = originalRoation;
    }
}
