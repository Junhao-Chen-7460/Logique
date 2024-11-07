using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatesAndSwitches : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    public bool inputA;
    public bool inputB;
    public bool output;
    public bool connected = false;

    private GameObject DragCollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
        string gateType = gameObject.tag;
        if (gateType == "AND")
        {
            output = inputA && inputB;
        }
        else if (gateType == "OR")
        {
            output = inputA || inputB;
        }
        else if (gateType == "NOT")
        {
            output = !inputA;
        }
        
        DisplayOutput();
    }
    void OnMouseDown()
    {
        if (IsPointerOverDragCollider())
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPos();
        }
    }
    void OnMouseUp()
    {
        isDragging = false;
    }
    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    bool IsPointerOverDragCollider()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPos(), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == DragCollider)
        {
            return true;
        }
        return false;
    }


    void DisplayOutput()
    {
        if (output)
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

}
