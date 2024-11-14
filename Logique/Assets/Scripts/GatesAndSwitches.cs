using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatesAndSwitches : MonoBehaviour, IComponentInterface
{

    private bool isDragging = false;
    private Vector3 offset;
    public bool inputA { get; set; }
    public bool inputB { get; set; }
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
                //Debug.LogWarning("Unknown type: " + tag);
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
        //Debug.Log($"IsConnected called for {collider.name}: {connected}");
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
        //Debug.Log($"SetConnection called for {collider.name}: {isConnected}");
    }

}
