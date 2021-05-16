using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class finish : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Transform parent_ = other.transform.parent;
        if (parent_)
        {
            Train train = parent_.GetComponent<Train>();
            if (train)
                train.ComeOnFinish();
        }
    }
}