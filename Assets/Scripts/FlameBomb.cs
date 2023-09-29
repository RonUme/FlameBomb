using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class FlameBomb : MonoBehaviour
{
    public ThrowController throwController;
    public FlameGenerator flameGenerator;
    public OVRHand _ovrHand;
    public GameObject centerEyeAnchor;
    public VelocityEstimator VE;
    public float scaleRate = 1.0f;
    public float speedRate = 1.0f;
    public float deleteTime = 3.0f;
    public bool scaleLock = false;
    public List<OVRHand.HandFinger> _handFingers;
    public float indexStrength;

    [SerializeField] public float _smoothTime = 0.2f;
    private float _maxSpeed = float.PositiveInfinity;
    private float _currentVerocity = 0f;

    private Material thisMat;
    public float dissolveRate = 0.25f;
    public float refreshRate = 0.025f;

    // Start is called before the first frame update
    void Start()
    {
        thisMat = this.GetComponent<Material>();
        this.GetComponent<LookAtCamera>().centerEyeAnchor = GameObject.Find("OVRPlayerController/OVRCameraRig/TrackingSpace/CenterEyeAnchor").GetComponent<Camera>();
        centerEyeAnchor = GameObject.Find("OVRPlayerController/OVRCameraRig/TrackingSpace/CenterEyeAnchor");
        scaleLock = false;
        _handFingers = new List<OVRHand.HandFinger>
        {   
            OVRHand.HandFinger.Index,
            OVRHand.HandFinger.Middle
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!scaleLock)
        {
            List<float> pintchStrengths = new List<float>();
            foreach (var finger in _handFingers)
            {
                float strength = _ovrHand.GetFingerPinchStrength(finger);
                pintchStrengths.Add(strength);
            }
            indexStrength = pintchStrengths[0];
            float currentScale = this.transform.localScale.x;
            float nextScale = Mathf.Pow(pintchStrengths.Average(), 2f) * scaleRate;
            float scale = Mathf.Clamp(Mathf.SmoothDamp(currentScale, nextScale, ref _currentVerocity, _smoothTime, _maxSpeed), 0.01f, 0.36f);
            this.transform.localScale = new Vector3(scale, scale, scale);

            if (scale == 0.36f) scaleLock = true;
        }

    }

    public void UnlockParent()
    {
        if (scaleLock)
        {
            this.transform.parent = null;
            Rigidbody rig = this.GetComponent<Rigidbody>();
            rig.useGravity = true;
            rig.constraints = RigidbodyConstraints.None;
            Vector3 forceAcceleration = VE.GetAccelerationEstimate();
            //rig.AddForce(forceAcceleration * rig.mass * speedRate);
            rig.AddForce(forceAcceleration.magnitude * centerEyeAnchor.transform.forward * rig.mass * speedRate);
            if(throwController != null)
            {
                throwController.flameGrab = false;
            }
        }
        Invoke("DeleteObject", deleteTime);
    }

    public IEnumerator Dissolve()
    {
        if (thisMat != null)
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

    private void DeleteObject()
    {
        Destroy(this.gameObject);
    }
}
