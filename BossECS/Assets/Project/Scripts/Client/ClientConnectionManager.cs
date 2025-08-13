using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Assets.Project.Scripts;
using Common;
public class ClientConnectionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _addressField;
    [SerializeField] private TMP_InputField _portField;
    [SerializeField] private TMP_Dropdown _connectionModeDropdown;
    [SerializeField] private TMP_Dropdown _teamDropdown;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Text _connectButtonLabel;

    private ushort Port => ushort.Parse(_portField.text);
    private string Address => _addressField.text;

    private void OnEnable()
    {
        _connectionModeDropdown.onValueChanged.AddListener(OnConnectionModeChanged);
        _connectButton.onClick.AddListener(OnButtonConnected);
        OnConnectionModeChanged(_connectionModeDropdown.value);
    }

    private void OnDisable()
    {
        _connectionModeDropdown.onValueChanged.RemoveAllListeners();
        _connectButton.onClick.RemoveAllListeners();
    }

    private void OnConnectionModeChanged(int connectionMode)
    {
        string buttonLabel;
        _connectButton.enabled = true;

        switch (connectionMode)
        {
            case 0:
                buttonLabel = "Start Host";
                break;
            case 1:
                buttonLabel = "Start Server";
                break;
            case 2:
                buttonLabel = "Start Client";
                break;
            default:
                buttonLabel = "ERROR";
                _connectButton.enabled = false;
                break;
        }
        _connectButtonLabel.text = buttonLabel;
    }

    private void OnButtonConnected()
    {
        DestroyLocalSimulationWorld();
        switch (_connectionModeDropdown.value)
        {
            case 0:
                StartServer();
                StartClient();
                break;
            case 1:
                StartServer();
                break;
            case 2:
                StartClient();
                break;
            default:
                Debug.LogError("Invalid connection mode selected.");
                break;
        }
        SceneManager.LoadSceneAsync(1);
    }
    private static void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }
    private void StartServer()
    {
        Debug.Log("✅ StartServer() запущен");
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Player Server World");
        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
        {
            using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }
    }
    private void StartClient()
    {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Player Client World");
        var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        {
            using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        }
        var team = _teamDropdown.value switch
        {
            0 => TeamType.AutoAsign,
            1 => TeamType.Team1,
            2 => TeamType.Team2,
            _ => TeamType.None
        };

        var teamRequestEntity = clientWorld.EntityManager.CreateEntity();
        clientWorld.EntityManager.AddComponentData(teamRequestEntity, new ClientTeamRequest { Value = team });

        World.DefaultGameObjectInjectionWorld = clientWorld;
    }
}