using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class LevelManager : MonoBehaviour
{
    public List<Transform> checkPoins;
    public List<GameObject> cars;
    public List<GameObject> positionRace;
    public int totalVueltas;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private CameraController playerCamera;
    public PathCircuit[] raceTracks;

    [SerializeField] PlayableDirector introSequence;

    [Header("UI")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI countDownText;
    public TextMeshProUGUI posText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI sectorTimeText, sectorDifference;
    [SerializeField] private GameObject panelFinish;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int azar = UnityEngine.Random.Range(0, spawnPoints.Length);
            if(i == spawnPoints.Length - 1)
            {
                //Se instancia el player
                GameObject playerClone = Instantiate(Resources.Load<GameObject>(GameManager.instance.carSelectedByPlayer), spawnPoints[i].position, spawnPoints[i].rotation);
                playerCamera.SetPlayer(playerClone.transform);
                playerClone.GetComponent<Rigidbody>().isKinematic = false;
                cars.Add(playerClone);
            }
            else
            {
                int azarIA = UnityEngine.Random.Range(0, 4); 
                GameObject IACarClone = Instantiate(Resources.Load<GameObject>("IACars/" + azarIA.ToString()), spawnPoints[i].position, spawnPoints[i].rotation);
                IACarClone.GetComponent<IACar>().AsignPath(raceTracks[GameManager.instance.raceTrackSelected]);
                cars.Add(IACarClone);
            }
        }
        StartCoroutine(StartRace());
    }

    IEnumerator StartRace()
    {       
        yield return new WaitForSeconds((float)introSequence.duration);
        countDownText.gameObject.SetActive(true);
        countDownText.text = "3";
        countDownText.transform.localScale = Vector3.zero;
        yield return StartCoroutine(AnimCountDownText(countDownText.transform, Vector3.zero, Vector3.one, 0.5f));
        countDownText.text = "2";
        yield return StartCoroutine(AnimCountDownText(countDownText.transform, Vector3.zero, Vector3.one, 0.5f));
        countDownText.text = "1";
        yield return StartCoroutine(AnimCountDownText(countDownText.transform, Vector3.zero, Vector3.one, 0.5f));
        countDownText.text = "GO!";
        StartCoroutine(AnimCountDownText(countDownText.transform, Vector3.zero, Vector3.one, 0.5f));
        yield return new WaitForSeconds(0.5f);
        for( int i = 0; i < cars.Count; i++ )
        {
            try
            {
                cars[i].GetComponent<IACar>().enabled = true;
            }
            catch
            {
                cars[i].GetComponent<CarController>().enabled = true;

            }
        }
        speedText.gameObject.SetActive(true);
        posText.gameObject.SetActive(true);
        lapText.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(false);
    }

    IEnumerator AnimCountDownText( Transform objeto, Vector3 startScale, Vector3 finishScale, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime * 2;
            objeto.localScale = Vector3.Lerp(startScale, finishScale, t * (1/duration));
            yield return null;
        }
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            objeto.localScale = Vector3.Lerp(finishScale, startScale, t * (1/duration));
            yield return null;
        }
    }

    public void UpdateLaps(int userLaps)
    {
        lapText.text = Mathf.Clamp (userLaps, 0, totalVueltas).ToString() + "/" + totalVueltas.ToString();
    }

    public void ShowSectorTimeText(TimeSpan previousSector, TimeSpan newSector)
    {
        sectorTimeText.text = string.Format("{0}:{1}:{2}", Math.Abs(previousSector.Minutes), Math.Abs(previousSector.Seconds), Math.Abs(previousSector.Milliseconds).ToString("00"));
        TimeSpan difference = previousSector - newSector;
        if (difference.TotalSeconds < 0 && previousSector.TotalSeconds !=0) 
        {
            sectorDifference.color = Color.red;
        }
        else
        {
            sectorDifference.color = Color.green;
        }
        sectorDifference.text = string.Format("{0}:{1}:{2}", Math.Abs(difference.Minutes), Math.Abs(difference.Seconds), Math.Abs(difference.Milliseconds).ToString("00"));
        sectorTimeText.gameObject.SetActive(true);
        sectorDifference.gameObject.SetActive(true);
        Invoke("HideTimeText", 5);
    }

    void HideTimeText()
    {
        sectorTimeText.gameObject.SetActive(false);
        sectorDifference.gameObject.SetActive(false);
    }

    public void ShowFinishRace()
    {
        panelFinish.SetActive(true);
        for(int i = 0; i < positionRace.Count; i++)
        {
            try
            {
                IACar car = positionRace[i].GetComponent<IACar>();
                GameObject corredorClone = Instantiate(panelFinish.transform.GetChild(1).GetChild(0).gameObject, panelFinish.transform.GetChild(1));
                corredorClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = car.name;
                if (car.vueltas < totalVueltas)
                {
                    corredorClone.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "No ha terminao.";
                    corredorClone.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "No ha terminao.";
                }
                else
                {
                    int vueltaRapida = 0;
                    TimeSpan tiempoTotal = TimeSpan.Zero;
                    for (int j = 0; j < car.lapTimes.Count; j++)
                    {
                        if (car.lapTimes[j] < car.lapTimes[vueltaRapida])
                        {
                            vueltaRapida = j;
                        }
                        tiempoTotal += car.lapTimes[j];
                    }
                    corredorClone.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = car.lapTimes[vueltaRapida].ToString();
                    corredorClone.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = tiempoTotal.ToString();
                }               
            }
            catch 
            {
                CarController car = positionRace[i].GetComponent<CarController>();
                GameObject corredorClone = Instantiate(panelFinish.transform.GetChild(1).GetChild(0).gameObject, panelFinish.transform.GetChild(1));
                corredorClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = car.name;
                int vueltaRapida = 0;
                TimeSpan tiempoTotal = TimeSpan.Zero;
                for (int j = 0; j < car.lapTimes.Count; j++)
                {
                    if (car.lapTimes[j] < car.lapTimes[vueltaRapida])
                    {
                        vueltaRapida = j;
                    }
                    tiempoTotal += car.lapTimes[j];
                }
                corredorClone.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = car.lapTimes[vueltaRapida].ToString();
                corredorClone.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = tiempoTotal.ToString();
                panelFinish.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = (i+1).ToString()+"º";
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        positionRace = new List<GameObject>();
        //primero comparamos las vueltas
        for(int i = 0; i < cars.Count; i++)
        {
            if (i== 0)
            {
                positionRace.Add(cars[i]);
            }
            else
            {
                int indexInsert = positionRace.Count;
                for (int j = 0; j < positionRace.Count; j++)
                {
                    int vueltas_i = 0;
                    try
                    {
                        vueltas_i = cars[i].GetComponent<IACar>().vueltas;
                    }
                    catch
                    {
                        vueltas_i = cars[i].GetComponent<CarController>().vueltas;
                    }
                    int vueltas_j = 0;
                    try
                    {
                        vueltas_j = positionRace[j].GetComponent<IACar>().vueltas;
                    }
                    catch
                    {
                        vueltas_j = positionRace[j].GetComponent<CarController>().vueltas;
                    }
                    if(vueltas_i > vueltas_j)
                    {

                        indexInsert = j;
                        break;
                    }
                    else if (vueltas_i == vueltas_j)
                    {
                        int checkPoint_i = 0;
                        try
                        {
                            checkPoint_i = cars[i].GetComponent<IACar>().indexCheckPoint;
                        }
                        catch
                        {
                            checkPoint_i = cars[i].GetComponent<CarController>().indexCheckPoint;
                        }

                        int checkPoint_j = 0;
                        try
                        {
                            checkPoint_j = positionRace[j].GetComponent<IACar>().indexCheckPoint;
                        }
                        catch
                        {
                            checkPoint_j = positionRace[j].GetComponent<CarController>().indexCheckPoint;
                        }
                        if (checkPoint_i > checkPoint_j)
                        {
                            indexInsert = j; 
                            break;
                        }
                        else if (checkPoint_i == checkPoint_j)
                        {
                            float distancia_i = 0;
                            try
                            {
                                int tempIndex = cars[i].GetComponent<IACar>().indexCheckPoint + 1;
                                if (tempIndex >= checkPoins.Count)
                                {
                                    tempIndex = 0;
                                }
                                distancia_i = (checkPoins[tempIndex].position - cars[i].transform.position).sqrMagnitude;
                            }
                            catch
                            {
                                int tempIndex = cars[i].GetComponent<CarController>().indexCheckPoint + 1;
                                if (tempIndex >= checkPoins.Count)
                                {
                                    tempIndex = 0;
                                }
                                distancia_i = (checkPoins[tempIndex].position - cars[i].transform.position).sqrMagnitude;
                            }

                            float distancia_j = 0;
                            try
                            {
                                int tempIndex = positionRace[j].GetComponent<IACar>().indexCheckPoint + 1;
                                if (tempIndex >= checkPoins.Count)
                                {
                                    tempIndex = 0;
                                }
                                distancia_j = (checkPoins[tempIndex].position - positionRace[j].transform.position).sqrMagnitude;
                            }
                            catch
                            {
                                int tempIndex = positionRace[j].GetComponent<CarController>().indexCheckPoint + 1;
                                if (tempIndex >= checkPoins.Count)
                                {
                                    tempIndex = 0;
                                }
                                distancia_j = (checkPoins[tempIndex].position - positionRace[j].transform.position).sqrMagnitude; ;
                            }
                            if (distancia_i < distancia_j)
                            {
                                indexInsert = j;
                                break;
                            }
                        }

                    }
                }
                positionRace.Insert(indexInsert, cars[i]);
            }
        }
        for (int i = 0; i < positionRace.Count; i++)
        {
            if (positionRace[i].name.Contains("Car"))
            {
                posText.text = (i + 1).ToString()+ "º";
            }
        }

        //Por que checkpoint va
        //Distancia hasta el checkpoint
    }



    
}
