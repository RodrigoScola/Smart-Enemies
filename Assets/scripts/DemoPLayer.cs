using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoPlayer : MonoBehaviour
{
    [SerializeField]
    [Range(0, 1000)]
    private float force;

    [SerializeField]
    [Range(0, 1000)]
    private float bufferDistance;

    private readonly Dictionary<float, Vector3> predictions = new();

    public GameObject goIndicator;
    public Vector3 v3AverageVelocity;
    public Vector3 v3AverageAcceleration;

    private Vector3 v3PrevVel;
    private Vector3 PrevAccel;
    private Vector3 v3PrevPos;

    public GameObject demoBall;

    private void Start() { }

    private void LateUpdate()
    {
        StartCoroutine(Check());
    }

    public void Update()
    {
        ApplyForce();
    }

    private IEnumerator Check()
    {
        yield return new WaitForEndOfFrame();

        predictions.Clear();

        Vector3 v3Velocity = (gameObject.transform.position - v3PrevPos) / Time.deltaTime;
        Vector3 v3Accel = v3Velocity - v3PrevVel;

        v3AverageVelocity = v3Velocity;
        v3AverageAcceleration = v3Accel;

        demoBall.transform.position = PredictPlayerPosition(0.5f);

        v3PrevPos = gameObject.transform.position;
        v3PrevVel = v3Velocity;
        PrevAccel = v3Accel;
    }

    public Vector3 PredictPlayerPosition(float fTime)
    {
        if (predictions.ContainsKey(fTime))
        {
            predictions.TryGetValue(fTime, out Vector3 val);
            return val;
        }

        //X0 + v0 * t + 1/2 a t^2
        Vector3 v3Ret =
            gameObject.transform.position
            + (v3AverageVelocity * Time.deltaTime * (fTime / Time.deltaTime))
            + (0.5f * v3AverageAcceleration * Time.deltaTime * Mathf.Pow(fTime / Time.deltaTime, 2));

        predictions.TryAdd(fTime, v3Ret);

        return v3Ret;
    }

    private void ApplyForce()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * force * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * force * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.forward * force * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.back * force * Time.deltaTime;
        }
    }
}
