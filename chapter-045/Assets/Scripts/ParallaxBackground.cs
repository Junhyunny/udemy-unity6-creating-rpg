using Unity.Cinemachine;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPositionX;
    private float cameraHalfWidth;

    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize - mainCamera.aspect;
        lastCameraPositionX = mainCamera.transform.position.x;
        CalculateImageLength();
    }

    // CinemachineBrain은 모든 FixedUpdate가 끝난 뒤에 카메라를 옮기므로,
    // FixedUpdate에서 배경을 옮기면 카메라보다 한 스텝 늦어 흔들린다.
    // 카메라 갱신 직후 호출되는 이벤트에서 배경을 옮겨 같은 시점에 맞춘다.
    private void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void CalculateImageLength()
    {
        foreach(ParallaxLayer layer in backgroundLayers)
        {
            layer.CalculateImageWidth();
        }
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
        if (brain.OutputCamera != mainCamera)
        {
            return;
        }

        float currentCameraPositionX = mainCamera.transform.position.x;
        float distanceToMove = currentCameraPositionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMove);
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
        }
    }
}
