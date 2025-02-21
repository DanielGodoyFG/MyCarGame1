using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPannel, selectCarPannel, selectTrackPannel;

    [Header("Select Car")]

    [SerializeField] List<CarController> cars;
    [SerializeField] private float carRotSpeed;
    [SerializeField] private float transitionCarSpeed;
    private int carSelected = 0;
    [SerializeField] TextMeshProUGUI[] carStatsText;
    [SerializeField] Transform middlePoint, leftPoint, rightPoint;
    private GameObject carInCenter, newCar;
    private IEnumerator ChangeCarCoroutine;

    /*Fechas y tiempo
    public DateTime fecha;
    public TimeSpan lapsoDeTiempo;
    fecha = new DateTime(Año, Mes, dia);
    */
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenuPannel.SetActive(true);
        selectCarPannel.SetActive(false);
        selectTrackPannel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (carInCenter != null)
        {
            carInCenter.transform.Rotate(Vector3.up * carRotSpeed * Time.deltaTime , Space.Self);
        }
    }

    public void ButtonPlay()
    {
        mainMenuPannel.SetActive(false);
        selectCarPannel.SetActive(true);
        carInCenter = Instantiate(cars[carSelected].gameObject, middlePoint.position, middlePoint.rotation);
        UpdateCarStats();
    }
    public void BackToMain()
    {
        mainMenuPannel.SetActive(true);
        selectCarPannel.SetActive(false);
        if (carInCenter != null)
        {
            Destroy(carInCenter);
        }
        if (newCar != null)
        {
            Destroy(newCar);
        }
    }

    public void BackToSelectCar()
    {
        ButtonPlay();
        selectTrackPannel.SetActive(false);
    }

    public void ConfirmButon()
    {
        selectCarPannel.SetActive(false);
        selectTrackPannel.SetActive(true);
        if (carInCenter != null)
        {
            Destroy(carInCenter);
        }
        if (newCar != null)
        {
            Destroy(newCar);
        }

        GameManager.instance.carSelectedByPlayer = cars[carSelected].gameObject.name;

    }

    public void ButtonExit()
    {
        Application.Quit();
    }

    public void FlechaSelectCar(bool derecha)
    {
        if (ChangeCarCoroutine != null)
        {
            StopCoroutine(ChangeCarCoroutine);
            Destroy(carInCenter);
            carInCenter = newCar;
        }

        ChangeCarCoroutine = ChangeCar(derecha);

        StartCoroutine(ChangeCarCoroutine);
    }

    IEnumerator ChangeCar(bool _derecha)
    {
        //coche del medio
        Transform destinoMiddleCar;
        Transform newCarStart;
        if (_derecha)
        {
            destinoMiddleCar = leftPoint;
            newCarStart = rightPoint;
            carSelected += 1;
            if(carSelected >= cars.Count) 
            { 
                carSelected = 0;
            }
        }
        else
        {
            destinoMiddleCar = rightPoint;
            newCarStart = leftPoint;
            carSelected -= 1;
            if (carSelected < 0)
            {
                carSelected = cars.Count - 1;
            }
        }

        newCar = Instantiate(cars[carSelected].gameObject, newCarStart.position, newCarStart.rotation);

        float t = 0;
        while (t < 1)
        {
            t+= Time.deltaTime * transitionCarSpeed;

            carInCenter.transform.position = Vector3.Lerp(middlePoint.position, destinoMiddleCar.position, t);
            newCar.transform.position = Vector3.Lerp(newCarStart.position, middlePoint.position, t);

            yield return null;
        }
        Destroy(carInCenter);
        carInCenter = newCar;
        UpdateCarStats();
        ChangeCarCoroutine = null;
    }

    void UpdateCarStats()
    {
        carStatsText[0].text = cars[carSelected].gameObject.name;
        carStatsText[1].text = "Traccion\n" + cars[carSelected].traccion;
        carStatsText[2].text = "Potencia Motor\n" + cars[carSelected].FuerzaMotor() + "N/m";
        carStatsText[3].text = "Velocidad Max.\n" + cars[carSelected].VelocidadMax() + "Km/h";
        carStatsText[4].text = "Fuerza Freno\n" + cars[carSelected].FuerzaFreno() + "N/m";
        carStatsText[5].text = "Peso\n" + cars[carSelected].GetComponent<Rigidbody>().mass.ToString() + "Kg";
    }

    public void TrackSelected(int selectedTrack)
    {
        GameManager.instance.raceTrackSelected = selectedTrack;
        SceneManager.LoadScene(1);
    }

    public void OnButtonSelectOrPointerOn(Transform button)
    {
        button.transform.localScale = Vector3.one * 1.1f;
        button.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void OnButtonDeselect(Transform button)
    {
        button.localScale = Vector3.one;
        button.transform.GetChild (0).gameObject.SetActive(false);
    }
}
