using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Camera mainCamera;
    private int localMapSizeX;
    private int localMapSizeY;

    public float cameraSizeFactor;
    public float cameraVerticalOffset;

    // Start is called before the first frame update
    void Start()
    {
        //Open camera up for access
        mainCamera = GetComponent<Camera>();

        //Get the map size from MapGeneration
        localMapSizeY = GameObject.Find("Tilemap").GetComponent<MapGeneration>().mapSizeY;
        localMapSizeX = 2 * localMapSizeY;

        mainCamera.orthographicSize = localMapSizeX / cameraSizeFactor; //Set the camera size to the X of the mapSize
        mainCamera.transform.position = new Vector3(localMapSizeX / 2f, localMapSizeY / 2f + cameraVerticalOffset, -10); //Move the camera to the center of the mine field
        //mainCamera.aspect = 16/9; //Set the apsect ratio of the camera
    }

    // Update is called once per frame
    void Update()
    {
    }
}
