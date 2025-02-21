using System.Collections.Generic;
using System;
using UnityEngine;


public class IACar : MonoBehaviour
{
    [SerializeField] private PathCircuit circuit;

    [SerializeField] float fuerzaMotor;
    [SerializeField] float fuerzaFrenado;
    [SerializeField] float anguloGiro;
    [SerializeField] float maxAnguloGiro;
    [SerializeField] WheelCollider wheelFL, wheelFR, wheelBL, wheelBR;
    [SerializeField] Transform transFL, transBL, transBR, transFR;
    private Material lucesFreno;
    private Rigidbody rb;
    private int currentNode;
    [SerializeField] private float distToChange;
    [SerializeField] private float maxSpeed;
    public int vueltas;
    public int indexCheckPoint;

    [SerializeField] private Transform[] sensors;
    [SerializeField] private float sensorsLength;
    [SerializeField] private float maxSensorLength;
    private bool isDodging;
    private float dodgeMultiplier;
    private LevelManager lm;

    public List<TimeSpan> lapTimes;
    private DateTime startRaceTime;
    
    public void SetIACar(WheelCollider _wheelFL, WheelCollider _wheelFR, WheelCollider _wheelBL, WheelCollider _wheelBR, 
                 Transform _transFL, Transform _transBL, Transform _transBR, Transform _transFR,
                 float _fuerzaMotor, float _maxAnguloGiro, float _fuerzaFrenado, float _maxSpeed, int _currentNode, LevelManager _lm
                )
    {
        wheelBL = _wheelBL;
        wheelBR = _wheelBR;
        wheelFL = _wheelFL;
        wheelFR = _wheelFR;
        transBL = _transBL;
        transBR = _transBR;
        transFL = _transFL;
        transFR = _transFR;
        fuerzaMotor = _fuerzaMotor;
        maxAnguloGiro = _maxAnguloGiro;
        maxSpeed = _maxSpeed;
        fuerzaFrenado = _fuerzaFrenado;
        currentNode = _currentNode;
        lm = _lm;

    }

    //tiempos
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = Vector3.down;
        lucesFreno = GetComponent<MeshRenderer>().materials[2];
        lucesFreno.DisableKeyword("_EMISSION");
        rb = GetComponent<Rigidbody>();
        lm = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        maxSensorLength = sensorsLength;
        lapTimes = new List<TimeSpan>();
    }

    private void OnEnable()
    {
        startRaceTime = DateTime.Now;
    }

    private void Update()
    {
        if (rb.linearVelocity.magnitude * 3.6f > maxSpeed)
        {
            wheelBL.motorTorque = 0;
            wheelBR.motorTorque = 0;
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
        else
        {
            wheelBL.motorTorque = fuerzaMotor;
            wheelBR.motorTorque = fuerzaMotor;
            wheelFL.motorTorque = fuerzaMotor;
            wheelFR.motorTorque = fuerzaMotor;
        }

        CheckSensors();

        if (isDodging == false)
        {
            CheckSteerAngle();
        }

        wheelFL.steerAngle = anguloGiro;
        wheelFR.steerAngle = anguloGiro;

        Vector3 pos;
        Quaternion rot;
        wheelBL.GetWorldPose(out pos, out rot);
        transBL.transform.position = pos;
        transBL.transform.rotation = rot;
        wheelBR.GetWorldPose(out pos, out rot);
        transBR.transform.position = pos;
        transBR.transform.rotation = rot;
        wheelFL.GetWorldPose(out pos, out rot);
        transFL.transform.position = pos;
        transFL.transform.rotation = rot;
        wheelFR.GetWorldPose(out pos, out rot);
        transFR.transform.position = pos;
        transFR.transform.rotation = rot;
    }
    void CheckSensors()
    {
        RaycastHit hit;
        dodgeMultiplier = 0;
        isDodging = false;
        for (int i = 0; i < sensors.Length; i++)
        {
            if (i > 1 && i < 4) //Cojemos los elementos 2 y 3 del array
            {
                sensorsLength = maxSensorLength * 0.5f;
            }
            else if (i == 4)
            {
                sensorsLength = maxSensorLength;
            }
            if (Physics.Raycast(sensors[i].position, sensors[i].forward, out hit, sensorsLength))
            {
                switch (i)
                {
                    case 0://FrontalIzquierdo
                        isDodging = true;
                        dodgeMultiplier += 1;
                    break;
                        
                    case 1://FrontalDerecho
                        isDodging = true;
                        dodgeMultiplier += -1;
                        break;

                    case 2://LateralIzquierdo
                        if (isDodging == false)
                        {
                            isDodging = true;
                            dodgeMultiplier = 0.5f;
                        }
                        break;

                    case 3://LateralDerecho
                        if (isDodging == false)
                        {
                            isDodging = true;
                            dodgeMultiplier = -0.5f;
                        }
                        break;

                    case 4: //Frontal
                        if ( dodgeMultiplier == 0)
                        {
                            isDodging = true;
                            if (hit.normal.x > 0)
                            {
                                dodgeMultiplier = 1;
                            }
                            else
                            {
                                dodgeMultiplier = -1;
                            }
                        }
                    break;
                }
            }
            Vector3 finalLinePos = sensors[i].position + (sensors[i].forward*sensorsLength);
            Debug.DrawLine(sensors[i].position, finalLinePos, Color.red);
        }
        if (isDodging == true) 
        {
            anguloGiro = maxAnguloGiro * dodgeMultiplier;
        }
       
    }
    void CheckSteerAngle()
    {
        Vector3 direction = circuit.nodes[currentNode].position - transform.position;
        float distance = direction.magnitude;
        if (distance < distToChange)
        {
            currentNode += 1;
            if (currentNode == circuit.nodes.Count)
            {
                currentNode = 0;
            }
        }
        //en mi caso es vector 3 back
        Quaternion rot = Quaternion.FromToRotation(transform.forward * -1, direction.normalized);

        anguloGiro = rot.eulerAngles.y;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BrakeZone")
        {
            wheelBL.brakeTorque = fuerzaFrenado;
            wheelBR.brakeTorque = fuerzaFrenado;
            wheelFL.brakeTorque = fuerzaFrenado;
            wheelFR.brakeTorque = fuerzaFrenado;
            wheelBL.motorTorque = 0;
            wheelBR.motorTorque = 0;
            lucesFreno.EnableKeyword("_EMISSION");
        }
        if (other.gameObject.tag == "CheckPoint")
        {
            if (indexCheckPoint == lm.checkPoins.Count - 1)
            {
                if (other.transform == lm.checkPoins[0])
                {
                    if (vueltas > 0)
                    {
                        TimeSpan tiempoCarrera = new TimeSpan();
                        for (int i = 0; i < lapTimes.Count; i++)
                        {
                            tiempoCarrera += lapTimes[i];
                        }
                        TimeSpan tiempoVuelta = DateTime.Now - (startRaceTime + tiempoCarrera);
                        lapTimes.Add(tiempoVuelta);
                    }

                    indexCheckPoint = 0;
                    vueltas += 1;
                    if (lm.totalVueltas < vueltas)
                    {
                        Debug.Log("He ganado!");
                        lm.ShowFinishRace();
                    }
                }
            }
            else
            {
                if (other.transform == lm.checkPoins[indexCheckPoint + 1])
                {
                    indexCheckPoint += 1;
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "BrakeZone")
        {
            wheelBL.brakeTorque = 0;
            wheelBR.brakeTorque = 0;
            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
            wheelBL.motorTorque = fuerzaMotor;
            wheelBR.motorTorque = fuerzaMotor;
            lucesFreno.DisableKeyword("_EMISSION");
        }
    }

    public void AsignPath(PathCircuit camino)
    {
        circuit = camino;
    }
}
