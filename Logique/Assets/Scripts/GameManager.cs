using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private List<Wires> wiresList = new List<Wires>();

    private HashSet<GameObject> pendingRemoval = new HashSet<GameObject>();

    [SerializeField] TextMeshProUGUI CountText;
    public int ANDCount = 0;
    public int NOTCount = 0;
    public int ORCount = 0;
    public int NANDCount = 0;
    public int NORCount = 0;
    public int XNORCount = 0;
    public int XORCount = 0;
    public int SwitchCount = 0;
    public int BulbCount = 0;
    public int FanCount = 0;
    public int ShuntCount = 0;

    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject GameScene;

    [SerializeField] GameObject InstructionText;

    [SerializeField] GameObject MainInstructionText;
    [SerializeField] GameObject MainTutorText;
    [SerializeField] GameObject MainGoalText;
    [SerializeField] GameObject SubMenu;
    [SerializeField] GameObject Header;

    [SerializeField] GameObject RestartButton;
    [SerializeField] GameObject MainMenuButton;
    [SerializeField] GameObject ShowTutorButton;
    [SerializeField] GameObject HideTutorButton;
    [SerializeField] GameObject ShowPlaceButton;
    [SerializeField] GameObject HidePlaceButton;
    //[SerializeField] GameObject PlaceANDButton;
    //[SerializeField] GameObject PlaceNOTButton;
    //[SerializeField] GameObject PlaceORButton;
    //[SerializeField] GameObject PlaceNANDButton;
    //[SerializeField] GameObject PlaceNORButton;
    //[SerializeField] GameObject PlaceXNORButton;
    //[SerializeField] GameObject PlaceXORButton;
    //[SerializeField] GameObject PlaceSwitchButton;
    //[SerializeField] GameObject PlaceBulbButton;
    //[SerializeField] GameObject PlaceFanButton;
    //[SerializeField] GameObject PlaceShuntButton;
    

    [SerializeField] GameObject ANDPrefab;
    [SerializeField] GameObject NOTPrefab;
    [SerializeField] GameObject ORPrefab;
    [SerializeField] GameObject NANDPrefab;
    [SerializeField] GameObject NORPrefab;
    [SerializeField] GameObject XNORPrefab;
    [SerializeField] GameObject XORPrefab;
    [SerializeField] GameObject SwitchPrefab;
    [SerializeField] GameObject BulbPrefab;
    [SerializeField] GameObject FanPrefab;
    [SerializeField] GameObject ShuntPrefab;

    private void Awake()
    {
        instance = this;
        Debug.Log("GameManager initialized.");
        if (CountText == null)
        {
            Debug.LogError("CountText is not assigned in GameManager!");
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
            Transform inputA = port.transform.Find("PortIn");
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        IComponentInterface component = collision.GetComponentInParent<IComponentInterface>();
        if (component != null)
        {

            if (!IsBeingDragged(collision.transform))
            {
                Transform rootTransform = collision.transform.root;
                if (rootTransform == null || pendingRemoval.Contains(rootTransform.gameObject))
                {
                    return;
                }

                pendingRemoval.Add(rootTransform.gameObject);

                RemoveComponentAndWires(rootTransform);
            }
        }
    }
    
    private bool IsBeingDragged(Transform componentTransform)
    {
        var specials = componentTransform.GetComponentInParent<Specials>();
        if (specials != null && specials.IsDragging())
        {
            return true;
        }

        var gatesAndSwitches = componentTransform.GetComponentInParent<GatesAndSwitches>();
        if (gatesAndSwitches != null && gatesAndSwitches.IsDragging())
        {
            return true;
        }

        return false;
    }
    private void RemoveComponentAndWires(Transform componentTransform)
    {
        IComponentInterface component = componentTransform.GetComponentInParent<IComponentInterface>();
        if (component == null)
        {
            Debug.LogWarning("No valid component found in parent. Deletion skipped.");
            return;
        }

        foreach (var wire in wiresList.ToList())
        {
            wire.ClearConnectionsRelatedToComponent(component);
        }

        Destroy(componentTransform.gameObject);
        switch (componentTransform.gameObject.tag)
        {
            case "AND":
                DecreaseCount(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                break;
            case "NOT":
                DecreaseCount(0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                break;
            case "OR":
                DecreaseCount(0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0);
                break;
            case "NAND":
                DecreaseCount(0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0);
                break;
            case "NOR":
                DecreaseCount(0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0);
                break;
            case "XNOR":
                DecreaseCount(0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0);
                break;
            case "XOR":
                DecreaseCount(0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0);
                break;
            case "switch":
                DecreaseCount(0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0);
                break;
            case "bulb":
                DecreaseCount(0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0);
                break;
            case "fan":
                DecreaseCount(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0);
                break;
            case "shunt":
                DecreaseCount(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
                break;
            default:
                break;
        }
    }

    public void IncreaseCount(int AND, int NOT, int OR, int NAND, int NOR, int XNOR, int XOR, int Switch, int Bulb, int Fan, int Shunt)
    {
        ANDCount += AND;
        NOTCount += NOT;
        ORCount += OR;
        NANDCount += NAND;
        NORCount += NOR;
        XNORCount += XNOR;
        XORCount += XOR;
        SwitchCount += Switch;
        BulbCount += Bulb;
        FanCount += Fan;
        ShuntCount += Shunt;
        CountText.text = "| AND: " + ANDCount +
            " | NOT: " + NOTCount +
            " | OR: " + ORCount +
            " | NAND: " + NANDCount +
            " | NOR: " + NORCount +
            " | XNOR: " + XNORCount +
            " | XOR: " + XORCount + 
            " | Switch: " + SwitchCount +
            " | Bulb: " + BulbCount +
            " | Fan: " + FanCount + 
            " | Shunt: " + ShuntCount + " |";
    }

    public void DecreaseCount(int AND, int NOT, int OR, int NAND, int NOR, int XNOR, int XOR, int Switch, int Bulb, int Fan, int Shunt)
    {
        ANDCount -= AND;
        NOTCount -= NOT;
        ORCount -= OR;
        NANDCount -= NAND;
        NORCount -= NOR;
        XNORCount -= XNOR;
        XORCount -= XOR;
        SwitchCount -= Switch;
        BulbCount -= Bulb;
        FanCount -= Fan;
        ShuntCount -= Shunt;
        CountText.text = "| AND: " + ANDCount +
            " | NOT: " + NOTCount +
            " | OR: " + ORCount +
            " | NAND: " + NANDCount +
            " | NOR: " + NORCount +
            " | XNOR: " + XNORCount +
            " | XOR: " + XORCount +
            " | Switch: " + SwitchCount +
            " | Bulb: " + BulbCount +
            " | Fan: " + FanCount +
            " | Shunt: " + ShuntCount + " |";
    }

    public void ClearCount()
    {
        ANDCount = 0;
        NOTCount = 0;
        ORCount = 0;
        NANDCount = 0;
        NORCount = 0;
        XNORCount = 0;
        XORCount = 0;
        SwitchCount = 0;
        BulbCount = 0;
        FanCount = 0;
        ShuntCount = 0;
        CountText.text = "| AND: " + ANDCount +
            " | NOT: " + NOTCount +
            " | OR: " + ORCount +
            " | NAND: " + NANDCount +
            " | NOR: " + NORCount +
            " | XNOR: " + XNORCount +
            " | XOR: " + XORCount +
            " | Switch: " + SwitchCount +
            " | Bulb: " + BulbCount +
            " | Fan: " + FanCount +
            " | Shunt: " + ShuntCount + " |";
    }

    public void ClearAllComponent()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {

            if (IsNonDeletableComponent(obj))
            {
                Debug.Log($"Skipping non-deletable object: {obj.name}");
                continue;
            }


            if (IsDeletableComponent(obj))
            {

                IComponentInterface component = obj.GetComponent<IComponentInterface>();
                if (component != null)
                {

                    Wires[] allWires = FindObjectsOfType<Wires>();
                    foreach (Wires wire in allWires)
                    {
                        wire.ClearConnectionsRelatedToComponent(component);
                    }
                }


                Destroy(obj);
                Debug.Log($"Deleted object: {obj.name}");
            }
        }


        ClearPortsConnections();

        ClearCount();
    }

    private void ClearPortsConnections()
    {
        GameObject port = GameObject.FindGameObjectWithTag("Ports");
        if (port != null)
        {
            IComponentInterface portComponent = port.GetComponent<IComponentInterface>();
            if (portComponent != null)
            {
                Transform inputA = port.transform.Find("PortIn");
                Transform inputB = port.transform.Find("PortOut");

                Collider2D inputACollider = inputA?.GetComponent<Collider2D>();
                Collider2D inputBCollider = inputB?.GetComponent<Collider2D>();

                if (inputACollider != null)
                {
                    portComponent.SetConnection(inputACollider, false);
                    portComponent.inputA = false;
                }

                if (inputBCollider != null)
                {
                    portComponent.SetConnection(inputBCollider, false);
                    portComponent.inputB = false;
                }
            }
        }
    }

    private bool IsDeletableComponent(GameObject obj)
    {
        string[] deletableTags = { "AND", "NOT", "OR", "NAND", "NOR", "XNOR", "XOR", "switch", "bulb", "fan", "shunt" };

        foreach (string tag in deletableTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }

        if (obj.GetComponent<IComponentInterface>() != null)
        {
            return true;
        }

        return false;
    }

    private bool IsNonDeletableComponent(GameObject obj)
    {
        if (obj.CompareTag("MainCamera") || obj.name == "MainCamera")
        {
            return true;
        }

        if (obj.CompareTag("Ports") || obj.name == "Ports")
        {
            return true;
        }

        return false;
    }
    public void ShowSubTutors()
    {
        SubMenu.SetActive(!SubMenu.activeInHierarchy);
    }
    public void SeeMainTutor()
    {
        MainTutorText.SetActive(!MainTutorText.activeInHierarchy);
        if (!Header.activeInHierarchy) 
        {
            Header.SetActive(true);
        }
        MainInstructionText.SetActive(false);
        MainGoalText.SetActive(false);
    }
    public void SeeMainInstruction()
    {
        MainInstructionText.SetActive(!MainInstructionText.activeInHierarchy);
        Header.SetActive(!Header.activeInHierarchy);
        MainGoalText.SetActive(false);
        MainTutorText.SetActive(false);
    }
    public void SeeMainGoal()
    {
        MainGoalText.SetActive(!MainGoalText.activeInHierarchy);
        MainInstructionText.SetActive(false);
        MainTutorText.SetActive(false);
        if (!Header.activeInHierarchy)
        {
            Header.SetActive(true);
        }
    }

    public void SeeInstruction()
    {
        InstructionText.SetActive(!InstructionText.activeInHierarchy);
    }
    public void StartPlay()
    {
        GameScene.SetActive(true);
        MainMenu.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void BackToMenu()
    {
        MainMenu.SetActive(true);
        GameScene.SetActive(false);
    }
    public void SwitchMenu()
    {
        if (RestartButton.activeInHierarchy)
        {
            RestartButton.SetActive(false);
            MainMenuButton.SetActive(false);
        }
        else
        {
            MainMenuButton.SetActive(true);
            RestartButton.SetActive(true);
        }
    }
    public void ShowTutor()
    {
        HideTutorButton.SetActive(true);
        ShowTutorButton.SetActive(false);
    }
    public void HideTutor()
    {
        ShowTutorButton.SetActive(true);
        HideTutorButton.SetActive(false);
    }
    public void ShowPlace()
    {
        HidePlaceButton.SetActive(true);
        ShowPlaceButton.SetActive(false);
    }
    public void HidePlace()
    {
        ShowPlaceButton.SetActive(true);
        HidePlaceButton.SetActive(false);
    }
    public void PlaceAND()
    {
        Instantiate(ANDPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
    public void PlaceNOT()
    {
        Instantiate(NOTPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
    public void PlaceOR()
    {
        Instantiate(ORPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0);
    }
    public void PlaceNAND()
    {
        Instantiate(NANDPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0);
    }
    public void PlaceNOR()
    {
        Instantiate(NORPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0);
    }
    public void PlaceXNOR()
    {
        Instantiate(XNORPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0);
    }
    public void PlaceXOR()
    {
        Instantiate(XORPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0);
    }
    public void PlaceSwitch()
    {
        Instantiate(SwitchPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0);
    }
    public void PlaceBulb()
    {
        Instantiate(BulbPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0);
    }
    public void PlaceFan()
    {
        Instantiate(FanPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0);
    }
    public void PlaceShunt()
    {
        Instantiate(ShuntPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        IncreaseCount(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    }

}
