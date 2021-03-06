﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

public enum PieceType
{
    RED, WHITE
}

public static class PieceTypeMethods
{
    public static Vector2 ForwardDirection(this PieceType type)
    {
        if (type == PieceType.RED)
            return new Vector2(0, 1);
        else
            return new Vector2(0, -1);
    }

    public static float OpponentBoardEdge(this PieceType type)
    {
        if (type == PieceType.RED)
            return 7;
        else
            return 0;
    }

    public static PieceType Opposite(this PieceType type)
    {
        if (type == PieceType.RED)
            return PieceType.WHITE;
        else
            return PieceType.RED;
    }
}

public class GamePiece : NetworkBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public GameObject Board;
    public GameManager Manager;
    public Vector2 GridPosition {
        get {
            return _gridPosition;
        }
        set {
            SetGridPosition(value);
        }
    }
    public PieceType color = PieceType.RED;
    public Sprite unkingedSprite;
    public Sprite kingedSprite;
    public bool king {
        get {
            return _king;
        }
        set {
            _king = value;
            if (_king)
                GetComponent<Image>().sprite = kingedSprite;
            else
                GetComponent<Image>().sprite = unkingedSprite;
        }
    }

    int GridSize = 8;
    Vector2 initialPosition;
    bool _king = false;
    Vector2 _gridPosition;

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (Manager.CanMove(this))
            transform.position = Input.mousePosition;
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (Manager.CanMove(this))
            initialPosition = transform.position;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (!Manager.CanMove(this))
            return;

        if (!IsOnBoard(transform.position))
            transform.position = initialPosition;
        else
        {
            Vector2 newPosition = ToGridPosition(transform.position);
            MoveResult moveResult = Manager.CheckMove(this, newPosition);
            //Debug.Log(GridPosition);
            if (moveResult.IsValid)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players)
                {
                    player.GetComponent<PlayerManager>().ChangeBoard0(GridPosition, moveResult, newPosition,color);
                }
                //Manager.DoMove(this, moveResult, newPosition);
            }
            else
                transform.position = initialPosition;
        }
    }
    public bool IsOnBoard(Vector2 position)
    {
        BoxCollider2D boardCollider = Board.GetComponent<BoxCollider2D>();
        return boardCollider.OverlapPoint(position);
    }

    public Vector2 ToGridPosition(Vector2 rawPosition)
    {
        BoxCollider2D boardCollider = Board.GetComponent<BoxCollider2D>();
        Bounds boardBounds = boardCollider.bounds;
        Vector2 boardDimension = boardBounds.extents * 2;
        Vector2 topLeft = boardBounds.center - boardBounds.extents;
        Vector2 relativePosition = (rawPosition - topLeft) / boardDimension;
        Vector2 scaledPosition = relativePosition * GridSize;
        Vector2 gridPosition = new Vector2(Mathf.Floor(scaledPosition.x), Mathf.Floor(scaledPosition.y));
        return gridPosition;
    }

    public void SetGridPosition(Vector2 position)
    {
        BoxCollider2D boardCollider = Board.GetComponent<BoxCollider2D>();
        Bounds boardBounds = boardCollider.bounds;
        Vector2 boardDimension = boardBounds.extents * 2;
        Vector2 topLeft = boardBounds.center - boardBounds.extents;
        Vector2 snappedScaledPosition = position + new Vector2(0.5f, 0.5f);
        Vector3 snappedPosition = snappedScaledPosition / GridSize;
        Vector3 finalPosition = snappedPosition * boardDimension + topLeft;
        _gridPosition = position;
        transform.position = finalPosition;
    }

    public List<Vector2> MovementDirections()
    {
        List<Vector2> directions = new List<Vector2>();

        if (king)
        {
            directions = new List<Vector2>()
            {
                new Vector2(1, 1),
                new Vector2(-1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1)
            };
        }

        else
        {
            directions = new List<Vector2>()
            {
                new Vector2(1, 0) + color.ForwardDirection(),
                new Vector2(-1, 0) + color.ForwardDirection()
            };
        }

        return directions;
    }
}
