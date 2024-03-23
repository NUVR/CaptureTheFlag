using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransform : MonoBehaviour
{

    public GameObject copyFrom;
    public Transform originalOrigin;
    public Transform copyOrigin;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = copyOrigin.transform.position - originalOrigin.transform.position;

        this.transform.position = copyFrom.transform.position + offset;
        this.transform.rotation = copyFrom.transform.rotation;
    }
}
