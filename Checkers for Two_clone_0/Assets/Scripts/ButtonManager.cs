using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ButtonManager : MonoBehaviour
{
    public GameObject NM;
    NetworkManager manager;
    public InputField ip_InputField;
    public GameObject canvas;
    // Start is called before the first frame update
    private void Awake()
    {
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Host()
    {
        NM = GameObject.Find("NetworkManager");
        manager = NM.GetComponent<NetworkManager>();
        manager.StartHost();
        canvas.SetActive(false);

    }
    public void Connect()
    {
        NM = GameObject.Find("NetworkManager");
        manager = NM.GetComponent<NetworkManager>();
        if (ip_InputField.text == "") { manager.networkAddress = "localhost"; }
        else
        {
            manager.networkAddress = ip_InputField.text;
        }
        manager.StartClient();
        //canvas.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Disconnect()
    {
        NM = GameObject.Find("NetworkManager");
        manager = NM.GetComponent<NetworkManager>();
        //manager.StopHost();
        //canvas.SetActive(false);
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            manager.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }
    }
    public void ReverseReady()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerManager>().hasAuthority)
            {
                Debug.Log(player.name);
                if (player.GetComponent<PlayerManager>().isReady)
                {
                    player.GetComponent<PlayerManager>().SetIsReady(false);
                    GetComponent<Image>().color = Color.white;
                }
                else
                {
                    player.GetComponent<PlayerManager>().SetIsReady(true);
                    GetComponent<Image>().color = Color.red;
                }
            }
        }
    }

    public void Forfeit() {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerManager>().hasAuthority)
            {
                if (player.GetComponent<PlayerManager>().color == PieceType.RED)
                {
                    player.GetComponent<PlayerManager>().EndGame(WinResult.WHITE);
                }
                else {
                    player.GetComponent<PlayerManager>().EndGame(WinResult.RED);
                }
            }
        }
    }
}