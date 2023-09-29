using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private EnemyGenerator enemyGenerator;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    [SerializeField] private float radius = 3f;
    [SerializeField] private float waitTime = 2f;
    private float time;
    [SerializeField] public float _smoothTime = 0.2f;
    private float _maxSpeed = float.PositiveInfinity;
    private Vector3 _currentVerocity = Vector3.zero;
    private Vector3 targetPosition;
    public Material thisMat;

    public GameObject blazePrefab;
    private bool isStop = false;
    private bool canCount = true;

    void Start()
    {
        enemyGenerator = GameObject.Find("Scripts/Enemy Generator").GetComponent<EnemyGenerator>();
        thisMat = this.GetComponent<MeshRenderer>().material;
        time = 0f;
        isStop = false;
        canCount = true;
        GoNextPoint();
    }

    void Update()
    {
        time += Time.deltaTime;
        Vector3 currentPos = this.transform.position;
        
        if(currentPos != targetPosition && time <= waitTime && !isStop)
        {
            //Move to the destination
            transform.position = Vector3.SmoothDamp(currentPos, targetPosition, ref _currentVerocity, _smoothTime, _maxSpeed);
        }
        else
        {
            time = 0f;
            GoNextPoint();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //If the collision target tag is "Bullet", call the destroy function
        if(collision.gameObject.tag == "Bullet")
        {
            if (canCount)
            {
                enemyGenerator.destroyNumber += 1;
                this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                Destroy(collision.gameObject);
                GameObject blazeClone = Instantiate(blazePrefab, this.transform.position + new Vector3(0f, 0.2f, 0f), this.transform.rotation);
                blazeClone.transform.parent = this.transform;
                blazeClone.GetComponent<LookAtCamera>().centerEyeAnchor = collision.gameObject.GetComponent<LookAtCamera>().centerEyeAnchor;
                isStop = true;
                StartCoroutine(Dissolve());
                StartCoroutine(collision.gameObject.GetComponent<FlameBomb>().Dissolve());
                //StartCoroutine(blazeClone.GetComponent<>().Dissolve());
                canCount = false;
            }
        }
    }

    void GoNextPoint()
    {
        //Deside the destination randomly within tha range
        float posX = Random.Range(-1 * radius, radius);
        float posZ = Random.Range(-1 * radius, radius);

        Vector3 pos = this.transform.position;
        pos.x += posX;
        pos.z += posZ;

        targetPosition = pos;
    }

    IEnumerator Dissolve()
    {
        //Change visuals by changing the material values
        if(thisMat != null)
        {
            float counter = 0f;
            while (thisMat.GetFloat("_DissolveAmount") < 1)
            {
                counter += dissolveRate;
                thisMat.SetFloat("_DissolveAmount", counter);
                yield return new WaitForSeconds(refreshRate);
            }
            Destroy(this.gameObject);
        }
    }
}
