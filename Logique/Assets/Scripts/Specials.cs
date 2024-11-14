using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Specials : MonoBehaviour, IComponentInterface
{
    public bool inputA { get; set; }
    public bool outputA => Switch() ? (turnedOn ? inputA : false) 
                         : (IsPort() ? true : inputA);
    public bool inputB { get; set; }
    public bool outputB => inputA;

    private Dictionary<Collider2D, bool> colliderConnections = new Dictionary<Collider2D, bool>();

    private bool isDragging = false;
    private Vector3 offset;


    //switch
    public bool turnedOn = true;
    private GameObject stick;
    private Quaternion initialRotation;

    //bulb
    private GameObject lights;
    private bool isBulbInitialized = false;

    //fan
    private GameObject fanLeaves;
    private bool isFanInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        if (Switch())
        {
            FindStick();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleDragging();
        HandleRightClickToggle();

        if (IsBulb() && !isBulbInitialized)
        {
            FindLights();
            isBulbInitialized = true;
        }

        if (IsBulb())
        {
            UpdateLightsVisibility();
        }

        if (IsFan() && !isFanInitialized)
        {
            FindFanLeaves();
            isFanInitialized = true;
        }

        if (IsFan())
        {
            UpdateFanRotation();
        }

        
    }

    private bool IsPort()
    {
        return gameObject.CompareTag("Ports");
    }

    private bool IsBulb()
    {
        return gameObject.CompareTag("bulb");
    }

    private bool IsFan()
    {
        return gameObject.CompareTag("fan");
    }

    private bool Switch()
    {
        return gameObject.CompareTag("switch");
    }


    private void FindStick()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("stick"))
            {
                stick = child.gameObject;
                initialRotation = stick.transform.rotation;
                break;
            }
        }
        if (stick == null)
        {
            //Debug.Log("No stick");
        }
    }

    private void FindLights()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("lights"))
            {
                lights = child.gameObject;
                lights.SetActive(false);
                break;
            }
        }
    }

    private void FindFanLeaves()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("fan leaves"))
            {
                fanLeaves = child.gameObject;
                break;
            }
        }
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Drag collider") && hit.collider.transform.parent == this.transform)
            {
                isDragging = true;
                offset = transform.position - GetMouseWorldPos();
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    public bool IsConnected(Collider2D collider)
    {
        return colliderConnections.ContainsKey(collider) && colliderConnections[collider];
    }

    public void SetConnection(Collider2D collider, bool isConnected)
    {
        if (colliderConnections.ContainsKey(collider))
        {
            colliderConnections[collider] = isConnected;
        }
        else
        {
            colliderConnections.Add(collider, isConnected);
        }
    }

    private void HandleRightClickToggle()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Drag collider") &&
                hit.collider.transform.parent == this.transform)
            {
                if (Switch())
                {
                    ToggleSwitch();
                }
            }
        }
    }
    private void ToggleSwitch()
    {
        FindStick();
        bool previousState = turnedOn;
        turnedOn = !turnedOn;
        UpdateStickRotation(previousState);
    }
    private void UpdateStickRotation(bool previousState)
    {
        if (previousState && !turnedOn)
        {
            stick.transform.rotation = initialRotation * Quaternion.Euler(0, 0, -30);
        }
        else if (!previousState && turnedOn)
        {
            stick.transform.rotation = initialRotation * Quaternion.Euler(0, 0, 30);
        }
    }

    private void UpdateLightsVisibility()
    {
        
        bool shouldLightBeVisible = inputA && outputA && 
                                    IsConnectedToPortInputA();

        if (lights != null)
        {
            lights.SetActive(shouldLightBeVisible);
        }
    }

    private void UpdateFanRotation()
    {
        bool shouldFanRotate = inputA && outputA && 
                               IsConnectedToPortInputA();

        if (fanLeaves != null)
        {
            if (shouldFanRotate)
            {
                fanLeaves.transform.Rotate(0, 0, -5);
            }
        }
    }

    private bool IsConnectedToPortInputA()
    {
        GameObject port = GameObject.FindGameObjectWithTag("Ports");
        if (port != null)
        {

            Transform inputA = port.transform.Find("PortIn");
            if (inputA != null)
            {
                
                IComponentInterface portComponent = port.GetComponent<IComponentInterface>();
                Collider2D inputACollider = inputA.GetComponent<Collider2D>();

                if (portComponent != null && inputACollider != null)
                {
                    return portComponent.inputA && portComponent.IsConnected(inputACollider);
                } 
            }
        }
        return false;
    }
}


// when should fan turn: Input A && Output A connected for bulb
//                      Input A for prot == T Output A for port connected
