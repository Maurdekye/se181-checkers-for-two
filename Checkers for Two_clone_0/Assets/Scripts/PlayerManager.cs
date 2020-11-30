using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public PieceType color;
    public bool isReady;
    // Start is called before the first frame update
    void Start()
    {
        isReady = false;
        name = "Player" + netIdentity.netId;
        if (netIdentity.netId == 1)
        {
            color = PieceType.RED;
        }
        else {
            color = PieceType.WHITE;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeBoard0(Vector2 piece, MoveResult result, Vector2 newPosition,PieceType c) {
        if (hasAuthority) {
            ChangeBoard(piece, result, newPosition);
        }
    }
    [Command]
    public void ChangeBoard(Vector2 piece, MoveResult result, Vector2 newPosition) {
        //ChangeBoard2(piece,result,newPosition,netIdentity.netId);
        //Debug.Log(piece.GetComponent<GamePiece>().GridPosition);
        //GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        ///foreach (GameObject p in pieces)
        //{
        //    if (p.GetComponent<GamePiece>().GridPosition == piece) {
        //        GameObject.Find("GameManager").GetComponent<GameManager>().DoMove(p.GetComponent<GamePiece>(), result, newPosition);
        //    }
        //}
        //GameObject.Find("GameManager").GetComponent<GameManager>().DoMove(piece.GetComponent<GamePiece>(), result, newPosition);
        ChangeBoard2(piece, result, newPosition, netId);
    }

    [ClientRpc]
    public void ChangeBoard2(Vector2 piece, MoveResult result, Vector2 newPosition,uint i)
    {
        //Debug.Log("I'm" + name);
        //if (hasAuthority) {
        //    Debug.Log("I'm" + name+"in line 38");
        //    GameObject.Find("GameManager").GetComponent<GameManager>().DoMove(piece.GetComponent<GamePiece>(),result,newPosition);
        //}
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject p in pieces)
        {
            if (p.GetComponent<GamePiece>().GridPosition == piece)
            {
                GameObject.Find("GameManager").GetComponent<GameManager>().DoMove(p.GetComponent<GamePiece>(), result, newPosition);
            }
        }
    }

    [Command]
    public void SetIsReady(bool b)
    {
        SetIsReady2(b, netId);
    }
    [ClientRpc]
    public void SetIsReady2(bool b, uint i)
    {
        if (netIdentity.netId == i)
        {
            isReady = b;
            if (isReady)
            {
                GameObject.Find("GameManager").GetComponent<GameManager>().StartGame();
            }
        }
    }

    [Command]
    public void EndGame(WinResult w)
    {
        EndGame2(w);
    }
    [ClientRpc]
    public void EndGame2(WinResult w)
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.EndGame(w);
    }
}
