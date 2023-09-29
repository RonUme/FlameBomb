using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyChild : MonoBehaviour
{
    public GameObject[] parentObjects;

    void Start()
    {
        foreach(GameObject parentObject in parentObjects)
        {
            int childCount = parentObject.transform.childCount;
            if(childCount > 0)
            {
                for(int i = 0; i < childCount; i++)
                {
                    GameObject childObject = parentObject.transform.GetChild(0).gameObject;
                    Destroy(childObject);
                }
            }
        }
    }
}
