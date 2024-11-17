using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatesAndSwitches : MonoBehaviour, IComponentInterface
{

    private bool isDragging = false;
    private Vector3 offset;
    private bool _inputA;
    private bool _inputB;

    public event Action SignalUpdated;

    public bool inputA
    {
        get => _inputA;
        set
        {
            if (_inputA != value)
            {
                _inputA = value;
                SignalUpdated?.Invoke();
            }
        }
    }

    public bool inputB
    {
        get => _inputB;
        set
        {
            if (_inputB != value)
            {
                _inputB = value;
                SignalUpdated?.Invoke();
            }
        }
    }

    public bool outputA { get; private set; }
    public bool outputB { get; private set; }


    public Dictionary<Collider2D, bool> colliderConnections = new Dictionary<Collider2D, bool>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleDragging();
        ProcessLogic();
        DisplayOutput();

        UpdateConnectedWires();
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
            Vector3 position = GetMouseWorldPos() + offset;
            position = CameraView(position);

            transform.position = position;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
    public bool IsDragging()
    {
        return isDragging;
    }

    private void ProcessLogic()
    {
        switch (tag)
        {
            case "AND":
                outputA = inputA && inputB;
                break;
            case "OR":
                outputA = inputA || inputB;
                break;
            case "NOT":
                outputA = !inputA;
                break;
            case "NAND":
                outputA = !(inputA && inputB);
                break;
            case "NOR":
                outputA = !(inputA || inputB);
                break;
            case "XOR":
                outputA = inputA ^ inputB;
                break;
            case "XNOR":
                outputA = !inputA ^ inputB;
                break;
            case "shunt":
                outputA = inputA;
                outputB = inputA;
                break;
            default:
                outputA = false;
                break;
        }
    }
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0; 
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void DisplayOutput()
    {
        if (outputA)
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
    public bool IsConnected(Collider2D collider)
    {
        bool connected = colliderConnections.ContainsKey(collider) && colliderConnections[collider];
        return connected;
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
    private Vector3 CameraView(Vector3 Position)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 minWorldPoint = cam.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z - cam.transform.position.z));
            Vector3 maxWorldPoint = cam.ViewportToWorldPoint(new Vector3(1, 1, transform.position.z - cam.transform.position.z));

            Position.x = Mathf.Clamp(Position.x, minWorldPoint.x, maxWorldPoint.x);
            Position.y = Mathf.Clamp(Position.y, minWorldPoint.y, maxWorldPoint.y);
        }
        return Position;
    }
    private void UpdateConnectedWires()
    {

        Wires[] allWires = FindObjectsOfType<Wires>();

        foreach (Wires wire in allWires)
        {
            if (wire != null && wire.startComponent == this)
            {
                wire.CheckAndUpdateSignal();
            }
        }
    }
}
