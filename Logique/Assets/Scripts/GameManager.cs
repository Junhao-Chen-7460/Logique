using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private List<Wires> wiresList = new List<Wires>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var wire in wiresList)
        {
            if (wire != null)
            {
                wire.CheckAndUpdateSignal();
            }
        }
    }
    public static void RegisterWire(Wires wire)
    {
        if (instance != null && !instance.wiresList.Contains(wire))
        {
            instance.wiresList.Add(wire);
            instance.UpdatePortsInputAConnection();
        }
    }
    public static void UnregisterWire(Wires wire)
    {
        if (instance != null && instance.wiresList.Contains(wire))
        {
            instance.wiresList.Remove(wire);
            instance.UpdatePortsInputAConnection();
        }
    }

    private void UpdatePortsInputAConnection()
    {
        GameObject port = GameObject.FindGameObjectWithTag("Ports");
        if (port != null)
        {
            Transform inputA = port.transform.Find("Input A");
            if (inputA != null)
            {
                IComponentInterface portComponent = port.GetComponent<IComponentInterface>();
                Collider2D inputACollider = inputA.GetComponent<Collider2D>();

                if (portComponent != null && inputACollider != null)
                {
                    bool isConnected = false;

                    foreach (var wire in wiresList)
                    {
                        if (wire.IsConnectedTo(inputA.gameObject))
                        {
                            isConnected = true;
                            break;
                        }
                    }
                    portComponent.SetConnection(inputACollider, isConnected);
                    Debug.Log($"Ports Input A connection status updated to: {isConnected}");
                }
                else
                {
                    Debug.LogWarning("Ports object does not have IComponentInterface or Input A Collider2D component.");
                }
            }
            else
            {
                Debug.LogWarning("Input A child object not found under Ports.");
            }
        }
        else
        {
            Debug.LogWarning("Ports object not found in the scene.");
        }
    }
}
