using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerCar;
    [SerializeField] private float camDamping;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void SetPlayer(Transform _player)
    {
        playerCar = _player;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = playerCar.position; //Vector3.Lerp(transform.position, playerCar.position, camDamping *Time.deltaTime);
        this.transform.rotation = Quaternion.Slerp(transform.rotation, playerCar.rotation, camDamping * Time.deltaTime);
    }
}
