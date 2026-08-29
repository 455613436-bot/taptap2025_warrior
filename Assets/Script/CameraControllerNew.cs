using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerNew : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public string roomName;
        public Rect roomBounds;
        public float cameraSize;

        // --- (!!) 第 1 处修改：添加布尔值 ---
        [Tooltip("勾选此项，进入该房间时将激活 ArrowIndicator 并隐藏 KeyHold UI")]
        public bool isBossRoom;
        // --- 修改结束 ---
    }

    public Room[] rooms;
    public Camera mainCamera;
    public GameObject player;

    // --- (!!) 第 2 处修改：添加 UI 引用 ---
    [Header("UI 切换")]
    [Tooltip("包含'A'、'D'、'Space'长按提示的父级 GameObject")]
    public GameObject keyHoldUIParent;
    [Tooltip("包含 ArrowIndicator 脚本的那个箭头 UI GameObject")]
    public GameObject arrowIndicatorObject;
    // --- 修改结束 ---
    public bool firsttimeinboss=true;
    public BossBGMControl bgm;
    private Room currentRoom;
    private float camHalfHeight;
    private float camHalfWidth;
    private float cameraZ;
    public GameObject princess;
    private PlayerMove pm;
    public GameObject Spawner;
    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (princess == null)
        {
            princess = GameObject.FindGameObjectWithTag("Princess");
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        cameraZ = mainCamera.transform.position.z;

        if (currentRoom == null && rooms.Length > 0)
        {
            // (!!) 注意：这里我们立即调用 CheckPlayerRoom，
            // 它会反过来调用 SetCurrentRoom 来设置正确的初始 UI 状态
            // SetCurrentRoom(rooms[0]); // 这行变得多余了
        }
        pm = player.GetComponent<PlayerMove>();

        // (!!) 这一行将处理初始的房间检测和 UI 设置
        CheckPlayerRoom();
    }

    void LateUpdate()
    {
        if (player == null) return;
        CheckPlayerRoom();
        FollowPlayerInBounds();
    }

    void CheckPlayerRoom()
    {
        Vector2 playerPos = player.transform.position;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].roomBounds.Contains(playerPos))
            {
                if (currentRoom != rooms[i])
                {
                    SetCurrentRoom(rooms[i]);
                }

                // (!!) 我们将这个逻辑也移到 SetCurrentRoom 中，使其更整洁
                // if (currentRoom == rooms[2]) ...

                return;
            }
        }
    }

    // --- (!!) 第 3 处修改：在 SetCurrentRoom 中添加切换逻辑 ---
    void SetCurrentRoom(Room newRoom)
    {
        currentRoom = newRoom;
        mainCamera.orthographicSize = currentRoom.cameraSize;
        UpdateCameraDimensions();
        Debug.Log($"切换到房间: {currentRoom.roomName}");
        if (currentRoom != rooms[0])
        {
            Spawner.SetActive(false);
        }
        // --- (!!) 这是核心的 UI 切换逻辑 ---
        if (currentRoom.isBossRoom)
        {
            // --- 进入 Boss 房 ---
            // 激活 Princess
            //princess.SetActive(true);
            
            if(firsttimeinboss){
                bgm.PlayBossroomBGM();
                firsttimeinboss = false;
            }
            // 切换 UI
            if (keyHoldUIParent != null)
                keyHoldUIParent.SetActive(false); // 隐藏常规UI
            //if (arrowIndicatorObject != null)
                //arrowIndicatorObject.SetActive(true); // 显示Boss战UI
        }
        else
        {
            firsttimeinboss = true;
            // --- 进入普通房间 ---
            // (你可以选择在这里隐藏 Princess)
            // princess.SetActive(false); 
            // (你可以选择在这里恢复速度)
            // pm.moveSpeed = DEFAULT_SPEED; // 假设你有一个默认速度
            pm.moveSpeed = 2f;
            // 切换 UI
            if (keyHoldUIParent != null)
                keyHoldUIParent.SetActive(true); // 显示常规UI
            if (arrowIndicatorObject != null)
                arrowIndicatorObject.SetActive(false); // 隐藏Boss战UI
        }
        // --- 逻辑结束 ---
    }

    private void UpdateCameraDimensions()
    {
        camHalfHeight = mainCamera.orthographicSize;
        camHalfWidth = camHalfHeight * mainCamera.aspect;
    }

    void FollowPlayerInBounds()
    {
        if (currentRoom == null) return;

        Vector3 targetPos = new Vector3(player.transform.position.x, player.transform.position.y, cameraZ);

        float roomMinX = currentRoom.roomBounds.xMin;
        float roomMaxX = currentRoom.roomBounds.xMax;
        float roomMinY = currentRoom.roomBounds.yMin;
        float roomMaxY = currentRoom.roomBounds.yMax;

        float clampMinX = roomMinX + camHalfWidth;
        float clampMaxX = roomMaxX - camHalfWidth;
        float clampMinY = roomMinY + camHalfHeight;
        float clampMaxY = roomMaxY - camHalfHeight;

        if (clampMinX > clampMaxX)
        {
            clampMinX = (roomMinX + roomMaxX) / 2;
            clampMaxX = clampMinX;
        }
        if (clampMinY > clampMaxY)
        {
            clampMinY = (roomMinY + roomMaxY) / 2;
            clampMaxY = clampMinY;
        }

        float clampedX = Mathf.Clamp(targetPos.x, clampMinX, clampMaxX);
        float clampedY = Mathf.Clamp(targetPos.y, clampMinY, clampMaxY);

        mainCamera.transform.position = new Vector3(clampedX, clampedY, cameraZ);
    }
}