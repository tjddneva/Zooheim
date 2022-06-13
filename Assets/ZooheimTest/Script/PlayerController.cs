using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
    * Scene의 Hierarchy 최상위 층에 3가지 추가할 것
        - "Main Camera" object (Tag명은 MainCamera) (Scene에 기본으로 생성되어 있음)
        - "Sub Camera" object  (Tag명은 SubCamera)  (Main Camera 복사해서 이름, Tag만 변경)
        - "User" object (PlayerController.cs, Rigidbody 등을 component로 갖는 투명 cylinder) (prefab으로 첨부했지만 직접 create해도 됨)

    * 시점 조작
        - Tab      : Main camera, sub camera 간 전환
        - W,A,S,D  : 이동 (방향키로도 조작 가능)
        - 마우스   : 시점 이동 (Main Camera에서는 조작 불가하도록 기본설정)
*/
public class PlayerController : MonoBehaviour {

    [SerializeField]
    private float walkSpeed = 20f;
    [SerializeField]
    private float lookSensitivity = 1.5f;
    private float lookSensitivity_Vertical_AddOn = 3.2f;

    private float upDownAngleLimit = 60f;
    float verticalRotation = 0f;

    //[SerializeField]
    private Camera mainCamera;
    //[SerializeField]
    private Camera subCamera;

    [SerializeField]
    private float mainCameraHeight = 25f;
    [SerializeField]
    private float subCameraHeight = 2f;
    [SerializeField]
    private float mainCameraAngle = 40f;
    [SerializeField]
    private float fieldOfView = 80f;
    
    Vector3 characterRotation_Y;

    private Rigidbody myRigid;

	void Awake() {
        /* initialize mainCamera */
        mainCamera = Camera.main;
        mainCamera.transform.position = new Vector3(0f, mainCameraHeight, 0f) + transform.position;
        mainCamera.transform.rotation = Quaternion.Euler(new Vector3(mainCameraAngle, 0f, 0f));
        mainCamera.fieldOfView = fieldOfView;
        
        /* initialize subCamera */
        subCamera = GameObject.FindWithTag("SubCamera").GetComponent<Camera>();
        subCamera.transform.position = new Vector3(0f, subCameraHeight, 0f) + transform.position;
        subCamera.fieldOfView = fieldOfView;

        mainCameraOn();

        myRigid = GetComponent<Rigidbody>();

        /* center-lock and hide mouse cursor when playing a scene in editor,
           press ESC to disable center-lock, click to set again */
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	

	void Update() {
        CharacterTranslation();
        CharacterRotation();
        if(Input.GetKeyDown(KeyCode.Tab))
            switchCamera();
	}

    void LateUpdate() {
        CameraTranslation();
        CameraRotation();
    }

    void mainCameraOn() {
        mainCamera.enabled = true;
        subCamera.enabled = false;
    }

    void subCameraOn() {
        mainCamera.enabled = false;
        subCamera.enabled = true;
    }

    void switchCamera() {
        if(mainCamera.enabled) {
            subCameraOn();
        }
        else {
            verticalRotation = 0f; // reset subCam's up/down angle
            myRigid.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f)); // reset User's rotation status
            mainCameraOn();
        }
    }

    private void CharacterTranslation() {
        float delta_X = Input.GetAxisRaw("Horizontal");
        float delta_Z = Input.GetAxisRaw("Vertical");

        Vector3 horizontalMove = transform.right * delta_X;
        Vector3 verticalMove = transform.forward * delta_Z;

        Vector3 velocity = (horizontalMove + verticalMove).normalized * walkSpeed;

        myRigid.MovePosition(transform.position + velocity * Time.deltaTime);
    }

    private void CharacterRotation() {
        if(subCamera.enabled) {
            float delta_LeftRight = Input.GetAxisRaw("Mouse X");
            Vector3 horizontalRotation = new Vector3(0f, delta_LeftRight, 0f) * lookSensitivity *lookSensitivity_Vertical_AddOn;
            myRigid.MoveRotation(myRigid.transform.rotation * Quaternion.Euler(horizontalRotation));
        }
    }


    private void CameraTranslation() {
        mainCamera.transform.position = new Vector3(0f, mainCameraHeight, 0f) + transform.position;
        subCamera.transform.position = new Vector3(0f, subCameraHeight, 0f) + transform.position;
    }

    private void CameraRotation() {      
        if(subCamera.enabled) {
            Vector3 characterRotated;
            Vector3 toRotateSub;
            float delta_UpDown = Input.GetAxisRaw("Mouse Y");
            verticalRotation -= delta_UpDown * lookSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -upDownAngleLimit, upDownAngleLimit);

            characterRotated = transform.rotation.eulerAngles;
            
            toRotateSub = new Vector3(verticalRotation, characterRotated.y, characterRotated.z);
            subCamera.transform.rotation = Quaternion.Euler(toRotateSub);
        }
    }
}
