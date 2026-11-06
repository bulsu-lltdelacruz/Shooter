using System;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    Camera camera;
    public Camera gunCamera;
    Player player;
    Vector3 lookInput;
    InputHandler playerInputs;
    PlayerStateMachine playerStateMachine;
    [SerializeField] float minPitch = -20;
    [SerializeField] float maxPitch = 20;
    [SerializeField] float sensitivity = 1f;
    float horizontal;
    float vertical;
    [Header("bobbingTransforms")]
    [SerializeField]Transform gun;
    [SerializeField]public GameObject gunRotateParent;
    private Vector3 gunBasePos;
    private Quaternion gunBaseRot;
    [Header("Sway")]
    [SerializeField] float step = 0.01f;
    [SerializeField] float maxStepDistance = 0.06f;
    [SerializeField] float smooth = 10f;
    Vector3 swayPos;
    [Header("SwayRotaion")]
    [SerializeField] float rotationStep = 4f;
    [SerializeField] float maxRotationDistance = 5f;
    [SerializeField] float smoothRot = 10f;
    Vector3 swayEulorRot; 
    public const string SensitivityPrefKey = "sensitivityMultiplier";
    float sensitivityMultiplier = 1f;

    internal void SetInstances(InputHandler playerInputs, PlayerStateMachine playerStateMachine)
    {
        this.playerInputs = playerInputs;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.playerStateMachine = playerStateMachine;
    }

    private void Start()
    {
        camera = GetComponentInChildren<Camera>();
        player = GetComponent<Player>();
        gunBasePos = gun.localPosition;
        gunBaseRot = gun.localRotation;
        sensitivityMultiplier = PlayerPrefs.GetFloat(SensitivityPrefKey, 1f);
    }

    public void SetSensitivityMultiplier(float multiplier)
    {
        sensitivityMultiplier = multiplier;
    }
    private int skipFrames = 2;
    private void Update()
    {
        GetInput();
        //horiz = yaw?
        //vert = pitcj
        if (skipFrames > 0)
        {
            skipFrames--;
            lookInput = Vector3.zero; // discard this frame's input entirely
        }
        horizontal += lookInput.x * sensitivity * sensitivityMultiplier * Time.deltaTime;
        vertical -= lookInput.y * sensitivity * sensitivityMultiplier * Time.deltaTime;

        vertical = Mathf.Clamp(vertical, minPitch, maxPitch);

        player.transform.rotation = Quaternion.Euler(0, horizontal, 0);
        camera.transform.localRotation = Quaternion.Euler(vertical, 0, 0);

        //bobbing
        
        Sway();
        SwayRotation();
        CompositePositionRotation();
    }
    private void GetInput()
    {
        if(playerInputs.mousePosition == null)
            Debug.LogError("lokounull");
        lookInput = playerInputs.mousePosition;
    }
    private void Sway()
    {
        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
        swayPos = invertLook;
    }
    private void SwayRotation()
    {
        Vector3 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationDistance, maxRotationDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationDistance, maxRotationDistance);
        swayEulorRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }
    private void CompositePositionRotation()
    {
        if(playerStateMachine.currentState is PlayerMeleeState)//because of the stretching thing, this resets it
            return;
        Vector3 targetPos = gunBasePos + swayPos;
        gun.localPosition = Vector3.Lerp(gun.localPosition, targetPos, Time.deltaTime * smooth);

        Quaternion targetRot = gunBaseRot * Quaternion.Euler(swayEulorRot);
        gun.localRotation = Quaternion.Slerp(gun.localRotation, targetRot, Time.deltaTime * smoothRot);

    }
    public Vector3 GetForward()
    {
        return camera.transform.forward;
    }



}
