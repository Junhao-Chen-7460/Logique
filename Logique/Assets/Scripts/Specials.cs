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
    private SpriteRenderer lightRenderer;

    // Start is called before the first frame update
    void Start()
    {
        if (Switch())
        {
            FindStick();
        }
        if (IsBulb())
        {
            lights = transform.Find("lights")?.gameObject;
            if (lights != null)
            {
                lightRenderer = lights.GetComponent<SpriteRenderer>();
                if (lightRenderer != null)
                {
                    lightRenderer.enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleDragging();
        HandleRightClickToggle();

        if (IsBulb() && lightRenderer != null)
        {
            GameObject portObject = GameObject.FindWithTag("Ports");
            bool portInputA = portObject != null && portObject.GetComponent<IComponentInterface>()?.inputA == true;

            bool EnableLights = inputA && portInputA;
            lightRenderer.enabled = EnableLights;
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
}
