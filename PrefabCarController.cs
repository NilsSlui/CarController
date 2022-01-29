using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabCarController: MonoBehaviour {

  public GameObject WheelFrontLeft, WheelFrontRight;
  public GameObject WheelRearLeft, WheelRearRight;

  public float maxSteerAngle = 37;
  public float motorTorque = 650;
  public float m_steeringAngle;
  public float motorForce = 550;

  public bool Skidding;
  public Vector3 LocalVelocity;

  public Rigidbody CarRb;
  public Vector3 CenterOfMass;

  public WheelCollider WheelFrontLeft_Collider, WheelFrontRight_Collider;
  public WheelCollider WheelRearLeft_Collider, WheelRearRight_Collider;

  public GameObject WheelColliders;
  private GameObject WheelFrontLeft_WC, WheelFrontRight_WC, WheelRearLeft_WC, WheelRearRight_WC;

  public float m_horizontalInput, m_verticalInput;

  public GameObject WheelColliderPrefab;

  public TouchDisplay TouchDisplay;

  public float Speed;
  public Text Speedo;

  public int Score;
  public Text DebugTextUI;

  //Skidding Object
  public GameObject SkidMarker;
  private GameObject SkidLeft;
  private GameObject SkidRight;

  //car accesoires
  public GameObject Exhaust;
  public GameObject ExhaustExplosion;
  public MeshRenderer BreakLight;

  //materials
  public Material Breaklight_active;
  public Material Breaklight_inactive;

  //number of frames since last reset
  public int frames;
  //acceleration stored up to X frames
  public float delta_acceleration;

  private bool UpToSpeed;
  //binds game wheels to variables
  void FindWheels() {
    WheelFrontLeft = GameObject.Find("WheelFrontLeft");
    WheelFrontRight = GameObject.Find("WheelFrontRight");
    WheelRearLeft = GameObject.Find("WheelRearLeft");
    WheelRearRight = GameObject.Find("WheelRearRight");
  }
  //updateWheelPoses
  public void UpdateWheelPoses() {
    UpdateWheelPose(WheelFrontLeft_Collider, WheelFrontLeft.transform);
    UpdateWheelPose(WheelFrontRight_Collider, WheelFrontRight.transform);
    UpdateWheelPose(WheelRearLeft_Collider, WheelRearLeft.transform);
    UpdateWheelPose(WheelRearRight_Collider, WheelRearRight.transform);
  }
  //sub method for each individual wheel
  private void UpdateWheelPose(WheelCollider _collider, Transform _transform) {
    Vector3 _pos = _transform.position;
    Quaternion _quat = _transform.rotation;
    _collider.GetWorldPose(out _pos, out _quat);
    _transform.position = _pos;
    _transform.rotation = _quat;
  }

  public void GetInput() {
    //KEYBOARD AND MOUSE
    m_horizontalInput = Input.GetAxis("Horizontal");
    m_verticalInput = Input.GetAxis("Vertical");

    //TOUCHSCREEN
    //m_horizontalInput = TouchDisplay.HorizontalInput;
    //m_verticalInput = TouchDisplay.VerticalInput;
  }

  public void DebugText() {
    string tempdebugtext;
    tempdebugtext = "H:" + Mathf.Round(m_horizontalInput * 100).ToString() + " V:" + Mathf.Round(m_verticalInput * 100).ToString();
    DebugTextUI.text = tempdebugtext;
  }
  public void Steer() {
    m_steeringAngle = maxSteerAngle * m_horizontalInput;
    WheelFrontLeft_Collider.steerAngle = m_steeringAngle;
    WheelFrontRight_Collider.steerAngle = m_steeringAngle;
  }
  public void Accelerate() {
    WheelRearLeft_Collider.motorTorque = m_verticalInput * motorForce;
    WheelRearRight_Collider.motorTorque = m_verticalInput * motorForce;
  }
  public void UpdateSpeed() {
    Speed = CarRb.velocity.magnitude * 5;
    Speedo.text = Mathf.Round(Speed).ToString();
  }

  //Check velocity and apply skidmarks when needed
  private void IsSkidding() {
    float _skidfactor = .7 f; //lower number, easier skid
    LocalVelocity = transform.InverseTransformDirection(CarRb.velocity);
    if (LocalVelocity.x > _skidfactor || LocalVelocity.x < -_skidfactor) {
      Skid();
    } else if (Speed > 6 && m_verticalInput < -.5 f) {
      Skid();
    } else if (Speed > 10 && m_horizontalInput > .8 f || Speed > 6 && m_verticalInput < -.8 f) {
      Skid();
    } else if (m_verticalInput == 1 && Speed < 4) {
      Skid();
    } else {
      EndSkid();
    }
  }

  private void Skid() {
    if (Skidding == false) {
      Instantiate(SkidMarker, SkidLeft.transform);
      Instantiate(SkidMarker, SkidRight.transform);
      Debug.Log("Skidding!");
      Skidding = true;
    }
  }
  private void EndSkid() {
    SkidLeft.transform.DetachChildren();
    SkidRight.transform.DetachChildren();
    Skidding = false;
  }

  //simulate backfire when acceleration has sudden drop
  private void ExhaustBang() {
    if (frames > 40) {
      if (delta_acceleration > 0.7 & TouchDisplay.VerticalInput < 0.1) {
        Bang();
      }
      delta_acceleration = m_verticalInput;
      frames = 0;
    }
  }
  private void Bang() {
    //ExhaustExplosion deactivates after X frames
    ExhaustExplosion.SetActive(true);
  }

  //changes breaklight texture when breaking
  private void SetBreakLight() {
    if (m_verticalInput < -.1 f) {
      BreakLight.material = Breaklight_active;
    } else {
      BreakLight.material = Breaklight_inactive;
    }
  }

  //increases drag when player reverses
  private void Breaks() {
    if (m_verticalInput < -.1 f) {
      float _breakamount = (m_verticalInput * -1);
      CarRb.drag = _breakamount;
      CarRb.angularDrag = _breakamount;
    } else {
      CarRb.drag = 0.015 f;
      CarRb.angularDrag = 0.027 f;
    }
  }

  // Use this for initialization
  void Start() {
    GameObject maincam = GameObject.Find("Main Camera");
    TouchDisplay = maincam.GetComponent < TouchDisplay > ();
    CarRb.centerOfMass = CenterOfMass;
    FindWheels();
    CreateWheelColliders();
    CreateSkidPositions();
    UpToSpeed = false;
  }

  // Update is called once per frame
  void FixedUpdate() {
    GetInput();
    Steer();
    Accelerate();
    Breaks();
    UpdateWheelPoses();
    UpdateSpeed();
    IsSkidding();
    ExhaustBang();
    SetBreakLight();
    frames++;
  }
  private void LateUpdate() {
    DebugText();
  }

  //creates wheel collider at each wheel position (4 wheels only)
  void CreateWheelColliders() {
    WheelFrontLeft_WC = Instantiate(WheelColliderPrefab, WheelFrontLeft.transform.position, Quaternion.identity);
    WheelFrontLeft_WC.name = "WheelFrontLeft_WC";
    WheelFrontLeft_WC.transform.parent = WheelColliders.transform;
    WheelFrontLeft_Collider = WheelFrontLeft_WC.GetComponent < WheelCollider > ();

    WheelFrontRight_WC = Instantiate(WheelColliderPrefab, WheelFrontRight.transform.position, Quaternion.identity);
    WheelFrontRight_WC.name = "WheelFrontRight_WC";
    WheelFrontRight_WC.transform.parent = WheelColliders.transform;
    WheelFrontRight_Collider = WheelFrontRight_WC.GetComponent < WheelCollider > ();

    WheelRearLeft_WC = Instantiate(WheelColliderPrefab, WheelRearLeft.transform.position, Quaternion.identity);
    WheelRearLeft_WC.name = "WheelRearLeft_WC";
    WheelRearLeft_WC.transform.parent = WheelColliders.transform;
    WheelRearLeft_Collider = WheelRearLeft_WC.GetComponent < WheelCollider > ();

    WheelRearRight_WC = Instantiate(WheelColliderPrefab, WheelRearRight.transform.position, Quaternion.identity);
    WheelRearRight_WC.name = "WheelRearRight_WC";
    WheelRearRight_WC.transform.parent = WheelColliders.transform;
    WheelRearRight_Collider = WheelRearRight_WC.GetComponent < WheelCollider > ();
  }
  //set skidmark location based on wheel pos
  private void CreateSkidPositions() {
    float _SkidVertOffset = .25 f;

    SkidLeft = new GameObject();
    SkidLeft.transform.position = new Vector3(WheelRearLeft.transform.position.x, WheelRearLeft.transform.position.y - _SkidVertOffset, WheelRearLeft.transform.position.z);
    SkidLeft.name = "SkidL";
    SkidLeft.transform.parent = this.transform;

    SkidRight = new GameObject();
    SkidRight.transform.position = new Vector3(WheelRearRight.transform.position.x, WheelRearRight.transform.position.y - _SkidVertOffset, WheelRearRight.transform.position.z);
    SkidRight.name = "SkidR";
    SkidRight.transform.parent = this.transform;
  }
}
