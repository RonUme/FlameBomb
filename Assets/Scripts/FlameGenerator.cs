using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

public class FlameGenerator : MonoBehaviour
{
    [SerializeField] private GestureDetector gestureDetector;
    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private OVRHand _ovrHand;
    [SerializeField] private VelocityEstimator VE;
    public GameObject parent;
    public ThrowController throwController;
    //public float speedRate = 1.0f;
    public bool isCreated = false;
    //public bool scaleLock = false;

    // Start is called before the first frame update
    void Start()
    {
        isCreated = false;
    }

    void Update()
    {
        isCreated = this.transform.childCount > 0;
    }


    public void Spawn()
    {
        if (!isCreated)
        {
            GameObject flameClone = Instantiate(flamePrefab, transform.position, transform.rotation);
            flameClone.name = "FlameBomb";
            flameClone.transform.parent = parent.transform;
            Rigidbody flameRig = flameClone.GetComponent<Rigidbody>();
            flameRig.useGravity = false;
            flameRig.constraints = RigidbodyConstraints.FreezePosition;
            FlameBomb flameBomb = flameClone.GetComponent<FlameBomb>();
            flameBomb.flameGenerator = this;
            flameBomb._ovrHand = _ovrHand;
            flameBomb.VE = VE;
            if(throwController != null)
            {
                flameBomb.throwController = throwController;
                throwController.grabTarget = flameBomb;
                throwController.flameGrab = true;
            }

            gestureDetector.gestures[0].onRecognized.AddListener(flameBomb.UnlockParent);

            isCreated = true;
        }
    }
}
