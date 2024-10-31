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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            HandleRightClick();
        }

        if (isDrawing && currentLine != null)
        {
            currentLine.SetPosition(1, GetMouseWorldPos());
        }
    }

    private void HandleRightClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero);
        

        if (hit.collider != null)
        {
            GameObject clickedCollider = hit.collider.gameObject;

            if (!isDrawing)
            {
                if (clickedCollider.CompareTag("Input collider") || clickedCollider.CompareTag("Output collider"))
                {
                    StartNewLine(clickedCollider);
                }
            }
            else
            {
                if (clickedCollider.CompareTag("Input collider") || clickedCollider.CompareTag("Output collider"))
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
            Debug.Log("Canceled line: No collider hit.");
        }
    }
    private void StartNewLine(GameObject startPoint)
    {
        starts = startPoint;
        isDrawing = true;

        GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        Vector3 startCenter = startPoint.GetComponent<Collider2D>().bounds.center;

        currentLine = line.GetComponent<LineRenderer>();
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, startCenter);
        currentLine.SetPosition(1, GetMouseWorldPos());
    }
    private void AttemptConnection(GameObject endPoint)
    {
        Vector3 endCenter = endPoint.GetComponent<Collider2D>().bounds.center;
        if ((starts.CompareTag("Input collider") && endPoint.CompareTag("Output collider")) ||
            (starts.CompareTag("Output collider") && endPoint.CompareTag("Input collider")))
        {
            ends = endPoint;
            currentLine.SetPosition(1, endCenter);
            isDrawing = false;

            UpdateLineColor();
            UpdateInputState();
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
