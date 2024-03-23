using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{

    public Transform[] Anchors;
    public GameObject playerRig;
    private int currentIndex = 0;

    public void TeleportToArea(int index)
    {
        Vector3 offset = playerRig.transform.position - Anchors[currentIndex].position;

        playerRig.transform.position = Anchors[index].position + offset;

        currentIndex = index;
    }
}
