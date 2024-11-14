using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wires : MonoBehaviour
{
    [SerializeField] GameObject linePrefab;
    private LineRenderer currentLine;
    private GameObject starts;
    private GameObject ends;
    private bool isDrawing = false;
    private bool outputB = false;

    private IComponentInterface startComponent;
    private IComponentInterface endComponent;
    private bool isA;
    private bool lastSignal;

    private struct LineConnection
    {
        public LineRenderer line;
        public GameObject start;
        public GameObject end;
        public bool signal;
    }
    private List<LineConnection> lineConnections = new List<LineConnection>();
    // Start is called before the first frame update
    void Start()
    {
        lastSignal = GetSignal();
        GameManager.RegisterWire(this);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            HandleRightClick();
        }

        if (isDrawing && currentLine != null)
        {
            currentLine.SetPosition(1, GetMouseWorldPos());
        }

        foreach (var connection in lineConnections)
        {
            if (connection.line != null && connection.start != null && connection.end != null)
            {
                Vector3 startCenter = connection.start.GetComponent<Collider2D>().bounds.center;
                Vector3 endCenter = connection.end.GetComponent<Collider2D>().bounds.center;
                connection.line.SetPosition(0, startCenter);
                connection.line.SetPosition(1, endCenter);
            }
        }

        if (startComponent == null)
        {
            return;
        }

        bool currentSignal = GetSignal();

        if (currentSignal != lastSignal)
        {
            lastSignal = currentSignal;
        }
    }


    private void HandleRightClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedCollider = hit.collider.gameObject;
            IComponentInterface component = clickedCollider.transform.parent?.GetComponent<IComponentInterface>();
            //Debug.Log("Hit object: " + clickedCollider.name);

            if (component == null)
            {
                //Debug.LogWarning("IComponentInterface not found on clicked object or its parent.");
                return;
            }

            if (component != null && component.IsConnected(hit.collider))
            {
                Debug.Log("Clearing connection for: " + hit.collider.gameObject.name);
                ClearConnection(hit.collider.gameObject);
            }
            else
            {
                if (!isDrawing)
                {
                    if ((clickedCollider.CompareTag("Input A") || clickedCollider.CompareTag("Output A")) ||
                        (clickedCollider.CompareTag("Input B") || clickedCollider.CompareTag("Output B")) &&
                        !component.IsConnected(hit.collider))
                    {
                        StartNewLine(clickedCollider);
                    }
                }
                else
                {
                    if ((clickedCollider.CompareTag("Input A") || clickedCollider.CompareTag("Output A")) ||
                        (clickedCollider.CompareTag("Input B") || clickedCollider.CompareTag("Output B")) &&
                        !component.IsConnected(hit.collider))
                    {
                        AttemptConnection(clickedCollider);
                    }
                    else
                    {
                        Destroy(currentLine.gameObject);
                        isDrawing = false;
                        starts = null;
                    }
                }
            }
        }
        else if (isDrawing)
        {
            Destroy(currentLine.gameObject);
            isDrawing = false;
            starts = null;
        }
    }

    private bool IsColliderConnected(GameObject collider)
    {
        foreach (var connection in lineConnections)
        {
            if (connection.start == collider || connection.end == collider)
            {
                return true;
            }
        }
        return false;
    }

    private void ClearConnection(GameObject collider)
    {
        Debug.Log("called");
        for (int i = lineConnections.Count - 1; i >= 0; i--)
        {
            var connection = lineConnections[i];
            Debug.Log("Checking connection at index " + i);
            if (connection.start == collider || connection.end == collider)
            {
                Debug.Log("Match found for connection with collider: " + collider.name);
                Destroy(connection.line.gameObject);
                Debug.Log("Destroyed line object for connection.");
                IComponentInterface startComponent = connection.start.transform.parent?.GetComponent<IComponentInterface>();
                IComponentInterface endComponent = connection.end.transform.parent?.GetComponent<IComponentInterface>();
                Debug.Log("start: " + startComponent + ", end: " + endComponent);

                if (startComponent != null && (connection.start.CompareTag("Output A") || connection.start.CompareTag("Output B")))
                {
                    startComponent.SetConnection(connection.start.GetComponent<Collider2D>(), false);
                }

                if (endComponent != null)
                {
                    endComponent.SetConnection(connection.end.GetComponent<Collider2D>(), false);

                    if (connection.end.CompareTag("Input A"))
                    {
                        endComponent.inputA = false;
                    }
                    else if (connection.end.CompareTag("Input B"))
                    {
                        endComponent.inputB = false;
                    }
                }

                lineConnections.RemoveAt(i);
                Debug.Log("Connection cleared for collider: " + collider.name);
            }
        }
    }


    private void StartNewLine(GameObject startPoint)
    {
        starts = startPoint;
        isDrawing = true;

        IComponentInterface startComponent = startPoint.transform.parent?.GetComponent<IComponentInterface>();
        if (startComponent == null)
        {
            //Debug.Log("Component null");
            isDrawing = false;
            return;
        }

        GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        currentLine = line.GetComponent<LineRenderer>();
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, startPoint.GetComponent<Collider2D>().bounds.center);
        currentLine.SetPosition(1, GetMouseWorldPos());

        UpdateLineColor(startComponent.outputA);
    }

    private void AttemptConnection(GameObject endPoint)
    {
        IComponentInterface startGate = starts.transform.parent?.GetComponent<IComponentInterface>();
        IComponentInterface endGate = endPoint.transform.parent?.GetComponent<IComponentInterface>();
        Collider2D startCollider = starts.GetComponent<Collider2D>();
        Collider2D endCollider = endPoint.GetComponent<Collider2D>();

        if (startGate == null || endGate == null)
        {
            //Debug.LogWarning("IComponentInterface component not found on either start or end GameObject.");
            return;
        }

        if (startGate.IsConnected(startCollider) || endGate.IsConnected(endCollider))
        {
            //Debug.LogWarning("One of the colliders is already connected.");
            return;
        }

        bool signal = startGate.outputA;


        currentLine.SetPosition(1, endCollider.bounds.center);
        isDrawing = false;

        lineConnections.Add(new LineConnection
        {
            line = currentLine,
            start = starts,
            end = endPoint,
            signal = signal
        });

        UpdateLineColor(signal);

        if (endCollider.CompareTag("Input A"))
        {
            endGate.inputA = signal;
        }
        else if (endCollider.CompareTag("Input B"))
        {
            endGate.inputB = signal;
        }

        endGate.SetConnection(endCollider, true);
        startGate.SetConnection(startCollider, true);

        Wires wire = currentLine.GetComponent<Wires>();
        if (wire != null)
        {
            wire.SetComponents(startGate, endGate, startCollider.CompareTag("Output A"));
        }
    }

    public void CheckAndUpdateSignal()
    {
        if (startComponent == null)
        {
            return;
        }

        bool currentSignal = GetSignal();

        if (currentSignal != lastSignal)
        {
            lastSignal = currentSignal;
            UpdateLineColor(currentSignal);
        }
    }

    private void UpdateLineColor(bool signal)
    {
        if (signal)
        {
            currentLine.startColor = Color.blue;
            currentLine.endColor = Color.blue;
        }
        else
        {
            currentLine.startColor = Color.red;
            currentLine.endColor = Color.red;
        }
    }

    private bool GetSignal()
    {
        if (startComponent != null)
        {
            return isA ? startComponent.outputA : startComponent.outputB;
        }

        return false;
    }

    public void SetComponents(IComponentInterface start, IComponentInterface end, bool useA)
    {
        if (start == null || end == null)
        {
            return;
        }

        startComponent = start;
        endComponent = end;
        isA = useA;
    }

    private void UpdateInputState()
    {
        if (starts == null || ends == null)
        {
            return;
        }

        GatesAndSwitches startGate = starts.transform.parent?.GetComponent<GatesAndSwitches>();
        GatesAndSwitches endGate = ends.transform.parent?.GetComponent<GatesAndSwitches>();

        if (startGate == null || endGate == null)
        {
            if (startGate == null)
            {
                //Debug.Log("start");
            }
            else if (endGate == null)
            {
                //Debug.Log("end");
            }
            else if (startGate == null && endGate == null)
            {
                //Debug.Log("both");
            }
            return;
        }

        if (starts.CompareTag("Output A") || starts.CompareTag("Output B"))
        {
            endGate.inputA = startGate.outputA;
            //Debug.Log("Updated endGate inputA with startGate output: " + startGate.outputA);
        }
        else
        {
            startGate.inputA = endGate.outputA;
            //Debug.Log("Updated startGate inputA with endGate output: " + endGate.outputA);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnDestroy()
    {
        GameManager.UnregisterWire(this);
    }

}
