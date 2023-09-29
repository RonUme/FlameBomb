using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlazeDissolve : MonoBehaviour
{
    private Material thisMat;
    public float dissolveRate = 0.25f;
    public float refreshRate = 0.025f;

    void Start()
    {
        thisMat = this.GetComponent<MeshRenderer>().material;
    }

    public IEnumerator Dissolve()
    {
        if (thisMat != null)
        {
            float dissolveCounter = 0f;
            while (thisMat.GetFloat("_DissolveAmount") < 1)
            {
                dissolveCounter += dissolveRate;
                thisMat.SetFloat("_DissolveAmount", dissolveCounter);
                yield return new WaitForSeconds(refreshRate);
            }
            Destroy(this.gameObject);
        }
    }
}
