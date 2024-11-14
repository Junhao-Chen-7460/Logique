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
        }
    }
    public static void UnregisterWire(Wires wire)
    {
        if (instance != null && instance.wiresList.Contains(wire))
        {
            instance.wiresList.Remove(wire);
        }
    }
}
