using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{


    [SerializeField] float fuerzaMotor;
    [SerializeField] float fuerzaFrenado;
    [SerializeField] float anguloGiro;
    [SerializeField] WheelCollider wheelFL, wheelFR, wheelBL, wheelBR;
    [SerializeField] Transform transFL, transBL, transBR, transFR;
    private Material lucesFreno;
    private Rigidbody rb;
    [SerializeField] private float maxSpeed;
    public int vueltas;
    public int indexCheckPoint;
    private LevelManager lm;
    public string traccion;
    private IACar iaCar;

    //tiempos

    public List<TimeSpan> lapTimes;
    public List<TimeSpan> checkPointsTimes;
    private DateTime startRaceTime;
    private DateTime lastCheckPoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = Vector3.down;
        lucesFreno = GetComponent<MeshRenderer>().materials[2];
        lucesFreno.DisableKeyword("_EMISSION");
        rb = GetComponent<Rigidbody>();
        lm = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        lapTimes = new List<TimeSpan>();
        checkPointsTimes = new List<TimeSpan>();
    }

    private void OnEnable()
    {
        startRaceTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        if (rb.linearVelocity.magnitude * 3.6f > maxSpeed)
        {
            wheelBL.motorTorque = 0;
            wheelBR.motorTorque = 0;
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
        else
        {
            wheelBL.motorTorque = fuerzaMotor * vertical;
            wheelBR.motorTorque = fuerzaMotor * vertical;
            wheelFL.motorTorque = fuerzaMotor * vertical;
            wheelFR.motorTorque = fuerzaMotor * vertical;
        }

        wheelFL.steerAngle = anguloGiro * horizontal;
        wheelFR.steerAngle = anguloGiro * horizontal;

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

        if (Input.GetButton("Jump"))
        {
            wheelBL.brakeTorque = fuerzaFrenado;
            wheelBR.brakeTorque = fuerzaFrenado;
            lucesFreno.EnableKeyword("_EMISSION");
        }
        else 
        {
            wheelBL.brakeTorque = 0;
            wheelBR.brakeTorque = 0;
            lucesFreno.DisableKeyword("_EMISSION");
        }
        float speedInKMH = rb.linearVelocity.magnitude * 3.6f;
        lm.speedText.text = speedInKMH.ToString("###00.0") + "Km/h";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "CheckPoint")
        {
            if (indexCheckPoint == lm.checkPoins.Count -1) 
            {
                if (other.transform == lm.checkPoins[0])
                {
                    if (vueltas == 1)
                    {
                        TimeSpan tiempoSector = DateTime.Now - lastCheckPoint;
                        checkPointsTimes.Add(tiempoSector);
                        lastCheckPoint = DateTime.Now;                       
                    }
                    else if (vueltas > 1)
                    {
                        //compararlos
                        TimeSpan tiempoSector = DateTime.Now - lastCheckPoint;
                        if (tiempoSector < checkPointsTimes[indexCheckPoint])
                        {
                            checkPointsTimes[indexCheckPoint] = tiempoSector;
                        }
                        lastCheckPoint = DateTime.Now;
                    }
                    //vueltas
                    if (lapTimes.Count > 0)
                    {
                        TimeSpan tiempoCarrera = new TimeSpan();
                        for ( int i = 0; i < lapTimes.Count; i++)
                        {
                            tiempoCarrera += lapTimes[i];
                        }
                        TimeSpan tiempoVuelta = DateTime.Now - (startRaceTime + tiempoCarrera);
                        lapTimes.Add(tiempoVuelta);
                    }
                    else
                    {
                        TimeSpan tiempoVuelta = DateTime.Now - startRaceTime;
                        lapTimes.Add(tiempoVuelta);
                    }
                    indexCheckPoint = 0;
                    vueltas += 1;
                    lm.UpdateLaps(vueltas);
                    if (lm.totalVueltas < vueltas)
                    {
                        Debug.Log("He ganado!");
                        lm.ShowFinishRace();
                        iaCar.SetIACar(wheelFL, wheelFR, wheelBL, wheelBR, transFL, transBL, transBR, transFR, fuerzaMotor, anguloGiro, fuerzaFrenado, maxSpeed, 0, lm);
                        GetComponent<IACar>().AsignPath(lm.raceTracks[GameManager.instance.raceTrackSelected]);
                        this.enabled = false;
                    }
                }
            }
            else
            {
                if (other.transform == lm.checkPoins[indexCheckPoint + 1])
                {
                    if(vueltas == 1)
                    {
                        //añadir checkpoints
                        if(checkPointsTimes.Count == 0)
                        {
                            TimeSpan tiempoSector = DateTime.Now - startRaceTime;
                            lastCheckPoint = DateTime.Now;
                            checkPointsTimes.Add(tiempoSector);
                            lm.ShowSectorTimeText(new TimeSpan(), tiempoSector);
                        }
                        else
                        {
                            TimeSpan tiempoSector = DateTime.Now - lastCheckPoint;
                            checkPointsTimes.Add(tiempoSector);
                            lastCheckPoint = DateTime.Now;
                            lm.ShowSectorTimeText(new TimeSpan(), tiempoSector);
                        }
                    }
                    else if (vueltas > 1)
                    {
                        //compararlos
                        TimeSpan tiempoSector = DateTime.Now - lastCheckPoint;
                        lm.ShowSectorTimeText(checkPointsTimes[indexCheckPoint], tiempoSector);
                        if (tiempoSector < checkPointsTimes[indexCheckPoint])
                        {
                            checkPointsTimes[indexCheckPoint] = tiempoSector;
                        }
                        lastCheckPoint = DateTime.Now;
                        
                    }
                    indexCheckPoint += 1;
                }
            }      
        }
    }

    public string FuerzaMotor()
    {
        return fuerzaMotor.ToString();
    }

    public string VelocidadMax() 
    {
        return maxSpeed.ToString();
    }

    public string FuerzaFreno() 
    {
        return fuerzaFrenado.ToString();
    }
}
