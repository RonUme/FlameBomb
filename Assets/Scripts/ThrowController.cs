using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pixeye.Unity;
using UnityEngine;
using Valve.VR.InteractionSystem; //To use VelocityEstimator 
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum HandSide
{
    None,
    Left,
    Right
}
public enum ForceMode
{
    None,
    Standerd,
    Reverse,
}

public class ThrowController : MonoBehaviour
{
    //[Header("First Settings")]
    public HandSide handSide;
    public ForceMode forceMode;
    private Transform indexTip;
    private Transform thumbTip;
    public VelocityEstimator handVE;

    //[Header("Cubes Rotation")]
    //[SerializeField]
    private Transform[] Cubes;
    [Foldout("Cubes")]  public Transform palmSideCubeL;
    [Foldout("Cubes")]  public Transform palmSideCubeR;
    [Foldout("Cubes")]  public Transform shellSideCubeL;
    [Foldout("Cubes")]  public Transform shellSideCubeR;
    private float[] rotateValues;
    private Vector3[] indexBase;
    private Vector3[] thumbBase;
    private Vector3 indexForce;
    private Vector3 thumbForce;
    public float rotateCoefficient = 1f;
    [SerializeField] private float _smoothTime = 0.3f;
    //[SerializeField] 
    private float _maxSpeed = float.PositiveInfinity;
    private float _currentVerocity = 0f;

    private Vector3 handAcceleration;
    private Vector3 gravity = new Vector3(0f, -9.8f, 0f);

    //[Header("Flame Bomb")]
    [NonSerialized] public FlameBomb grabTarget;
    [NonSerialized] public bool flameGrab = false;
    public float flameSize = 0.01f;
    public float flameConfficient = 0.2f;
    public float sinCoefficient = 0.25f;
    public float thetaConfficient = 1f;
    private float flameNoise;
    private float[] flameRotateValues;

    // Start is called before the first frame update
    void Start()
    {
        flameGrab = false;
        Cubes = new Transform[4] { palmSideCubeL,palmSideCubeR, shellSideCubeL, shellSideCubeR };
        for(int i = 0; i < Cubes.Length; i++)
        {
            Cubes[i].rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        if (handSide == HandSide.Right)
        {
            thumbTip = GameObject.Find("OVRPlayerController/OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefabR/Bones/Hand_WristRoot/Hand_Thumb0/Hand_Thumb1/Hand_Thumb2/Hand_Thumb3/Hand_ThumbTip").transform;
            indexTip = GameObject.Find("OVRPlayerController/OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefabR/Bones/Hand_WristRoot/Hand_Index1/Hand_Index2/Hand_Index3/Hand_IndexTip").transform;
        }
        else if (handSide == HandSide.Left)
        {
            thumbTip = GameObject.Find("OVRPlayerController/OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefabL/Bones/Hand_WristRoot/Hand_Thumb0/Hand_Thumb1/Hand_Thumb2/Hand_Thumb3/Hand_ThumbTip").transform;
            indexTip = GameObject.Find("OVRPlayerController/OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefabL/Bones/Hand_WristRoot/Hand_Index1/Hand_Index2/Hand_Index3/Hand_IndexTip").transform;
        }
        else
        {
            Debug.LogWarning("Can't Find Tip Objects");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (flameGrab)
        {
            indexBase = GetFingerBase(indexTip);
            thumbBase = GetFingerBase(thumbTip);
            indexForce = GetForceValuesForAcceleration(indexBase, handAcceleration, gravity);
            thumbForce = GetForceValuesForAcceleration(thumbBase, handAcceleration, gravity);
            flameSize = grabTarget.gameObject.transform.localScale.x;
            flameNoise = GetFlameNoise(flameSize, sinCoefficient);
            SetCubeRotation( Cubes, indexForce, thumbForce, flameNoise);
        }
        else
        {
            foreach(Transform cube in Cubes)
            {
                float currentRotationZ = cube.rotation.eulerAngles.z;
                cube.rotation = Quaternion.Euler(new Vector3(0f, 0f, Mathf.SmoothDampAngle(currentRotationZ, 0f, ref _currentVerocity, _smoothTime, _maxSpeed)));
            }
        }
    }

    void SetCubeRotation(Transform[] Cubes, Vector3 indexForce, Vector3 thumbForce, float flameNoise)
    {
        if(handSide == HandSide.Right)
        {
            if(forceMode == ForceMode.None)
            {
                return;
            }
            else
            {
                float inedxForceLR = indexForce.x;
                float indexForceFB = indexForce.y;
                float indexForceUD = indexForce.z;
                float thumbForceLR = thumbForce.x;
                float thumbForceFB = thumbForce.y;
                float thumbForceUD = thumbForce.z;

                rotateValues = new float[4];
                rotateValues[0] = (inedxForceLR + indexForceFB + indexForceUD) * rotateCoefficient + flameSize * flameConfficient;
                rotateValues[1] = (inedxForceLR - indexForceFB - indexForceUD) * rotateCoefficient - flameSize * flameConfficient;
                rotateValues[2] = (-thumbForceLR + thumbForceFB - thumbForceUD) * rotateCoefficient - flameSize * flameConfficient;
                rotateValues[3] = (-thumbForceLR - thumbForceFB + thumbForceUD) * rotateCoefficient + flameSize * flameConfficient;

                if (forceMode == ForceMode.Standerd)
                {
                    for(int i = 0; i < Cubes.Length; i++)
                    {
                        float currentRotationZ = Cubes[i].rotation.eulerAngles.z;
                        //Cubes[i].rotation = Quaternion.Euler(0f, 0f, Mathf.SmoothDampAngle(currentRotationZ, rotateValues[i], ref _currentVerocity, _smoothTime, _maxSpeed) + flameNoise);
                        Cubes[i].rotation = Quaternion.Euler(0f, 0f, Mathf.SmoothDampAngle(currentRotationZ, rotateValues[i], ref _currentVerocity, _smoothTime, _maxSpeed) + flameNoise);
                        //Cubes[i].rotation = Quaternion.Euler(0f, 0f, Mathf.SmoothDampAngle(currentRotationZ, rotateValues[i] + flameNoise, ref _currentVerocity, _smoothTime, _maxSpeed));
                    }
                }
                else
                {
                    for (int i = 0; i < Cubes.Length; i++)
                    {
                        float currentRotationZ = Cubes[i].rotation.eulerAngles.z;
                        Cubes[i].rotation = Quaternion.Euler(0f, 0f, Mathf.SmoothDampAngle(currentRotationZ, -rotateValues[i], ref _currentVerocity, _smoothTime, _maxSpeed) + flameNoise);
                        //Cubes[i].rotation = Quaternion.Euler(0f, 0f, Mathf.SmoothDampAngle(currentRotationZ, -rotateValues[i] - flameNoise, ref _currentVerocity, _smoothTime, _maxSpeed));
                    }
                }
            }
        }
        else if(handSide == HandSide.Left)
        {
            return;
        }
        else
        {
            return;
        }
    }

    Vector3 GetForceValuesForAcceleration(Vector3[] fingerBase, Vector3 acceleration, Vector3 gravity)
    {
        Vector3 a = -acceleration + gravity;
        Vector3 Answer = decompositionMatrix(a, fingerBase);
        return Answer;
    }

    float GetFlameNoise(float size, float coefficient)
    {
        return size * coefficient * MathF.Sin(Time.time * thetaConfficient);
    }
    Vector3[] GetFingerBase(Transform fingerTip)
    {
        Vector3 tipVecLR = Vector3.zero;
        Vector3 tipVecFB = Vector3.zero;
        Vector3 tipVecUD = Vector3.zero;
        if (handSide == HandSide.Right)
        {
            tipVecLR = fingerTip.forward;
            tipVecFB = fingerTip.right;
            tipVecUD = fingerTip.up;
        }
        else if (handSide == HandSide.Left)
        {
            tipVecLR = -fingerTip.forward;
            tipVecFB = -fingerTip.right;
            tipVecUD = -fingerTip.up;
        }
        return new Vector3[] { tipVecLR, tipVecFB, tipVecUD };
    }

    //Matrix functions
    double[,] MatrixTimesMatrix(double[,] A, double[,] B)
    {

        double[,] product = new double[A.GetLength(0), B.GetLength(1)];

        for (int i = 0; i < A.GetLength(0); i++)
        {
            for (int j = 0; j < B.GetLength(1); j++)
            {
                for (int k = 0; k < A.GetLength(1); k++)
                {
                    product[i, j] += A[i, k] * B[k, j];
                }
            }
        }

        return product;

    }

    double[,] inverseMatrix(double[,] A)
    {

        int n = A.GetLength(0);
        int m = A.GetLength(1);

        double[,] invA = new double[n, m];

        if (n == m)
        {

            int max;
            double tmp;

            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    invA[j, i] = (i == j) ? 1 : 0;
                }
            }

            for (int k = 0; k < n; k++)
            {
                max = k;
                for (int j = k + 1; j < n; j++)
                {
                    if (Math.Abs(A[j, k]) > Math.Abs(A[max, k]))
                    {
                        max = j;
                    }
                }

                if (max != k)
                {
                    for (int i = 0; i < n; i++)
                    {
                        // “ü—Ís—ñ‘¤
                        tmp = A[max, i];
                        A[max, i] = A[k, i];
                        A[k, i] = tmp;
                        // ’PˆÊs—ñ‘¤
                        tmp = invA[max, i];
                        invA[max, i] = invA[k, i];
                        invA[k, i] = tmp;
                    }
                }

                tmp = A[k, k];

                for (int i = 0; i < n; i++)
                {
                    A[k, i] /= tmp;
                    invA[k, i] /= tmp;
                }

                for (int j = 0; j < n; j++)
                {
                    if (j != k)
                    {
                        tmp = A[j, k] / A[k, k];
                        for (int i = 0; i < n; i++)
                        {
                            A[j, i] = A[j, i] - A[k, i] * tmp;
                            invA[j, i] = invA[j, i] - invA[k, i] * tmp;
                        }
                    }
                }

            }


            //‹ts—ñ‚ªŒvŽZ‚Å‚«‚È‚©‚Á‚½Žž‚Ì‘[’u
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (double.IsNaN(invA[j, i]))
                    {
                        Console.WriteLine("Error : Unable to compute inverse matrix");
                        invA[j, i] = 0;//‚±‚±‚Å‚ÍC‚Æ‚è‚ ‚¦‚¸ƒ[ƒ‚É’u‚«Š·‚¦‚é‚±‚Æ‚É‚·‚é
                    }
                }
            }


            return invA;

        }
        else
        {
            Console.WriteLine("Error : It is not a square matrix");
            return invA;
        }

    }
    double[] ArrayAddSub(double[] A, double[] B, bool isAdd)
    {
        double[] C = new double[A.GetLength(0)];
        for (int i = 0; i < A.GetLength(0); i++)
        {
            if (isAdd)
            {
                C[i] = A[i] + B[i];
            }
            else
            {
                C[i] = A[i] - B[i];
            }
        }
        return C;
    }
    Vector3 decompositionMatrix(Vector3 A, Vector3[] fingerBase)
    {
        float[] Answer = new float[3];
        double[,] AccelerationVec = { { A.x }, { A.y }, { A.z } };
        double[,] HandsMatrix = { { fingerBase[0].x, fingerBase[1].x, fingerBase[2].x }, { fingerBase[0].y, fingerBase[1].y, fingerBase[2].y }, { fingerBase[0].z, fingerBase[1].z, fingerBase[2].z } };
        double[,] HandMatrixInverse = inverseMatrix(HandsMatrix);
        double[,] MyoSynthRotate = MatrixTimesMatrix(HandMatrixInverse, AccelerationVec);
        for (int i = 0; i < MyoSynthRotate.GetLength(0); i++)
        {
            Answer[i] = (float)MyoSynthRotate[i, 0];
        }
        return new Vector3( Answer[0], Answer[1], Answer[2] );
    }

    public void LostGrabTaregt()
    {
        grabTarget = null;
    }
}
