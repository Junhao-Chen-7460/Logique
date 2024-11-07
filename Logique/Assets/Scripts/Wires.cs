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
    private struct LineConnection
    {
    public LineRenderer line; // 线条
    public GameObject start;  // 起点 Collider
    public GameObject end;    // 终点 Collider
    }
    private List<LineConnection> lineConnections = new List<LineConnection>();
    // Start is called before the first frame update
    void Start()
    {
        
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
    }

    private void HandleRightClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedCollider = hit.collider.gameObject;

            GatesAndSwitches gateScript = clickedCollider.transform.parent?.GetComponent<GatesAndSwitches>();

            if (!isDrawing)
            {
                if ((clickedCollider.CompareTag("Input collider") || clickedCollider.CompareTag("Output collider")) &&
                    gateScript != null && !gateScript.connected)
                {
                    Debug.Log("Conditions met to start new line.");
                    StartNewLine(clickedCollider);
                } 
                else if (gateScript == null) {
                    Debug.Log("gateScript == null");
                } 
                else if (gateScript.connected) {
                    Debug.Log("gateScript.connected problems");
                }
            }
            else
            {
                if ((clickedCollider.CompareTag("Input collider") || clickedCollider.CompareTag("Output collider")) &&
                    gateScript != null && !gateScript.connected)
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
        else if (isDrawing)
        {
            Destroy(currentLine.gameObject);
            isDrawing = false;
            starts = null;
        }
    }

    private void StartNewLine(GameObject startPoint)
    {
        GatesAndSwitches gateScript = startPoint.GetComponent<GatesAndSwitches>();
        if (gateScript != null && gateScript.connected)
        {
            return;
        }

        starts = startPoint;
        isDrawing = true;

        GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        Vector3 startCenter = startPoint.GetComponent<Collider2D>().bounds.center;

        currentLine = line.GetComponent<LineRenderer>();
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, startCenter);
        currentLine.SetPosition(1, GetMouseWorldPos());

        Debug.Log("Line created and drawing started.");

        
    }

    private void AttemptConnection(GameObject endPoint)
    {
        Vector3 endCenter = endPoint.GetComponent<Collider2D>().bounds.center;

        if ((starts.CompareTag("Input collider") && endPoint.CompareTag("Output collider")) ||
            (starts.CompareTag("Output collider") && endPoint.CompareTag("Input collider")))
        {
            GatesAndSwitches endGateScript = endPoint.transform.parent?.GetComponent<GatesAndSwitches>();
        GatesAndSwitches startGateScript = starts.transform.parent?.GetComponent<GatesAndSwitches>();

            if (endGateScript != null && !endGateScript.connected)
            {
                ends = endPoint;
                currentLine.SetPosition(1, endCenter);
                isDrawing = false;

                endGateScript.connected = true;

                if (startGateScript != null)
                {
                    startGateScript.connected = true;
                }
                if (startGateScript != null)
                {
                    startGateScript.connected = true;
                }

                
                lineConnections.Add(new LineConnection
                {
                    line = currentLine,
                    start = starts,
                    end = ends
                });

                UpdateLineColor();
                UpdateInputState();
            }
            else
            {
                Debug.Log("End point is already connected, cannot complete the connection.");
                Destroy(currentLine.gameObject);
                isDrawing = false;
                starts = null;
                ends = null;
            }
        }
        else
        {
            Destroy(currentLine.gameObject);
            isDrawing = false;
            starts = null;
            ends = null;
        }
    }
    private void UpdateLineColor()
    {

        if (starts.CompareTag("Output collider"))
        {
            outputB = starts.GetComponent<GatesAndSwitches>().output;
        }
        else
        {
            outputB = ends.GetComponent<GatesAndSwitches>().output;
        }

        if (outputB)
        {
            currentLine.startColor = Color.green;
            currentLine.endColor = Color.green;
        }
        else
        {
            currentLine.startColor = Color.red;
            currentLine.endColor = Color.red;
        }
    }

    private void UpdateInputState()
    {
        
        if (starts.CompareTag("Output collider"))
        {
            ends.GetComponent<GatesAndSwitches>().inputA = starts.GetComponent<GatesAndSwitches>().output;
        }
        else
        {
            starts.GetComponent<GatesAndSwitches>().inputA = ends.GetComponent<GatesAndSwitches>().output;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

}
