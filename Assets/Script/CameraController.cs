using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public string roomName;
        public Rect roomBounds; // 房间的矩形边界
        public Vector2 cameraCenter;
        public float cameraSize;
    }

    public Room[] rooms;
    public Camera mainCamera;
    public Transform player;
    private void Start()
    {
        player = GetComponent<Transform>();
    }
    void Update()
    {
        CheckPlayerRoom();
    }

    void CheckPlayerRoom()
    {
        Vector2 playerPos = player.position;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].roomBounds.Contains(playerPos))
            {
                SetCameraToRoom(i);
                return;
            }
        }
    }

    void SetCameraToRoom(int roomIndex)
    {
        Vector3 newPos = new Vector3(
            rooms[roomIndex].cameraCenter.x,
            rooms[roomIndex].cameraCenter.y,
            mainCamera.transform.position.z
        );

        mainCamera.transform.position = newPos;
        //mainCamera.orthographicSize = rooms[roomIndex].cameraSize;

        //Debug.Log($"切换到房间: {rooms[roomIndex].roomName}");
    }
}
