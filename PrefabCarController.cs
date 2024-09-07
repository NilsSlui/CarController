using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabCarController : MonoBehaviour
{
    public GameObject WheelFrontLeft, WheelFrontRight, WheelRearLeft, WheelRearRight;
    public WheelCollider WheelFrontLeft_Collider, WheelFrontRight_Collider, WheelRearLeft_Collider, WheelRearRight_Collider;
    public Rigidbody CarRb;
    public Text Speedo, DebugTextUI;
    public GameObject WheelColliderPrefab, SkidMarker, Exhaust, ExhaustExplosion;
    public MeshRenderer BreakLight;
    public Material BreaklightActive, BreaklightInactive;
    public float MaxSteerAngle = 37, MotorForce = 550;
    public Vector3 CenterOfMass;

    private GameObject WheelFrontLeft_WC, WheelFrontRight_WC, WheelRearLeft_WC, WheelRearRight_WC;
    private GameObject SkidLeft, SkidRight;
    private TouchDisplay touchDisplay;
    private float steeringAngle, horizontalInput, verticalInput, speed;
    private bool isSkidding, upToSpeed;
    private int score, frames;
    private float deltaAcceleration;

    private void Start()
    {
        touchDisplay = GameObject.Find("Main Camera").GetComponent<TouchDisplay>();
        CarRb.centerOfMass = CenterOfMass;
        FindWheels();
        CreateWheelColliders();
        CreateSkidPositions();
    }

    private void FixedUpdate()
    {
        GetInput();
        Steer();
        Accelerate();
        ApplyBrakes();
        UpdateWheelPoses();
        UpdateSpeed();
        CheckSkidding();
        ExhaustBang();
        SetBreakLight();
        frames++;
    }

    private void LateUpdate()
    {
        UpdateDebugText();
    }

    void FindWheels()
    {
        WheelFrontLeft = GameObject.Find("WheelFrontLeft");
        WheelFrontRight = GameObject.Find("WheelFrontRight");
        WheelRearLeft = GameObject.Find("WheelRearLeft");
        WheelRearRight = GameObject.Find("WheelRearRight");
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    void Steer()
    {
        steeringAngle = MaxSteerAngle * horizontalInput;
        WheelFrontLeft_Collider.steerAngle = steeringAngle;
        WheelFrontRight_Collider.steerAngle = steeringAngle;
    }

    void Accelerate()
    {
        WheelRearLeft_Collider.motorTorque = verticalInput * MotorForce;
        WheelRearRight_Collider.motorTorque = verticalInput * MotorForce;
    }

    void ApplyBrakes()
    {
        if (verticalInput < -0.1f)
        {
            float brakeAmount = -verticalInput;
            CarRb.drag = brakeAmount;
            CarRb.angularDrag = brakeAmount;
        }
        else
        {
            CarRb.drag = 0.015f;
            CarRb.angularDrag = 0.027f;
        }
    }

    void UpdateWheelPoses()
    {
        UpdateWheelPose(WheelFrontLeft_Collider, WheelFrontLeft.transform);
        UpdateWheelPose(WheelFrontRight_Collider, WheelFrontRight.transform);
        UpdateWheelPose(WheelRearLeft_Collider, WheelRearLeft.transform);
        UpdateWheelPose(WheelRearRight_Collider, WheelRearRight.transform);
    }

    void UpdateWheelPose(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 position = wheelTransform.position;
        Quaternion rotation = wheelTransform.rotation;
        collider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    void UpdateSpeed()
    {
        speed = CarRb.velocity.magnitude * 5;
        Speedo.text = Mathf.Round(speed).ToString();
    }

    void CheckSkidding()
    {
        float skidFactor = 0.7f;
        Vector3 localVelocity = transform.InverseTransformDirection(CarRb.velocity);
        if (Mathf.Abs(localVelocity.x) > skidFactor || (speed > 6 && verticalInput < -0.5f) || (speed > 10 && horizontalInput > 0.8f))
        {
            StartSkid();
        }
        else
        {
            EndSkid();
        }
    }

    void StartSkid()
    {
        if (!isSkidding)
        {
            Instantiate(SkidMarker, SkidLeft.transform);
            Instantiate(SkidMarker, SkidRight.transform);
            isSkidding = true;
        }
    }

    void EndSkid()
    {
        SkidLeft.transform.DetachChildren();
        SkidRight.transform.DetachChildren();
        isSkidding = false;
    }

    void ExhaustBang()
    {
        if (frames > 40 && deltaAcceleration > 0.7f && touchDisplay.VerticalInput < 0.1f)
        {
            ExhaustExplosion.SetActive(true);
        }
        deltaAcceleration = verticalInput;
        frames = 0;
    }

    void SetBreakLight()
    {
        BreakLight.material = verticalInput < -0.1f ? BreaklightActive : BreaklightInactive;
    }

    void UpdateDebugText()
    {
        DebugTextUI.text = "H: " + Mathf.Round(horizontalInput * 100).ToString() + " V: " + Mathf.Round(verticalInput * 100).ToString();
    }

    void CreateWheelColliders()
    {
        WheelFrontLeft_WC = Instantiate(WheelColliderPrefab, WheelFrontLeft.transform.position, Quaternion.identity, WheelColliders.transform);
        WheelFrontLeft_WC.name = "WheelFrontLeft_WC";
        WheelFrontLeft_Collider = WheelFrontLeft_WC.GetComponent<WheelCollider>();

        WheelFrontRight_WC = Instantiate(WheelColliderPrefab, WheelFrontRight.transform.position, Quaternion.identity, WheelColliders.transform);
        WheelFrontRight_WC.name = "WheelFrontRight_WC";
        WheelFrontRight_Collider = WheelFrontRight_WC.GetComponent<WheelCollider>();

        WheelRearLeft_WC = Instantiate(WheelColliderPrefab, WheelRearLeft.transform.position, Quaternion.identity, WheelColliders.transform);
        WheelRearLeft_WC.name = "WheelRearLeft_WC";
        WheelRearLeft_Collider = WheelRearLeft_WC.GetComponent<WheelCollider>();

        WheelRearRight_WC = Instantiate(WheelColliderPrefab, WheelRearRight.transform.position, Quaternion.identity, WheelColliders.transform);
        WheelRearRight_WC.name = "WheelRearRight_WC";
        WheelRearRight_Collider = WheelRearRight_WC.GetComponent<WheelCollider>();
    }

    void CreateSkidPositions()
    {
        float skidVertOffset = 0.25f;

        SkidLeft = new GameObject("SkidL", typeof(Transform));
        SkidLeft.transform.position = new Vector3(WheelRearLeft.transform.position.x, WheelRearLeft.transform.position.y - skidVertOffset, WheelRearLeft.transform.position.z);
        SkidLeft.transform.parent = this.transform;

        SkidRight = new GameObject("SkidR", typeof(Transform));
        SkidRight.transform.position = new Vector3(WheelRearRight.transform.position.x, WheelRearRight.transform.position.y - skidVertOffset, WheelRearRight.transform.position.z);
        SkidRight.transform.parent = this.transform;
    }
}
