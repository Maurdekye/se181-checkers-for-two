using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
}

public class GamePiece : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public GameObject Board;
    public GameManager Manager;
    public Vector2 GridPosition;
    public PieceType color = PieceType.RED;
    public Sprite unkingedSprite;
    public Sprite kingedSprite;
    public bool king {
        get {
            return isKing;
        }
        set {
            isKing = value;
            if (isKing)
                this.gameObject.GetComponent<Image>().sprite = kingedSprite;
            else
                this.gameObject.GetComponent<Image>().sprite = unkingedSprite;
        }
    }

    int GridSize = 8;
    Vector2 initialPosition;
    bool isKing = false;

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        initialPosition = transform.position;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        BoxCollider2D boardCollider = Board.GetComponent<BoxCollider2D>();
        if (!boardCollider.OverlapPoint(Input.mousePosition))
        {
            transform.position = initialPosition;
        } 
        else
        {

            Vector2 piecePosition = transform.position;
            Bounds boardBounds = boardCollider.bounds;
            Vector2 boardDimension = boardBounds.extents * 2;
            Vector2 topLeft = boardBounds.center - boardBounds.extents;
            Vector2 relativePosition = (piecePosition - topLeft) / boardDimension;
            Vector2 scaledPosition = relativePosition * GridSize;
            Vector2 snappedScaledPosition = new Vector2(Mathf.Floor(scaledPosition.x), Mathf.Floor(scaledPosition.y));
            Vector2 newGridPosition = snappedScaledPosition;
            snappedScaledPosition += new Vector2(0.5f, 0.5f);
            Vector3 snappedPosition = snappedScaledPosition / GridSize;
            Vector3 finalPosition = snappedPosition * boardDimension + topLeft;
           
            bool moveResult = Manager.TryMove(this, newGridPosition);
            if (moveResult)
            {
                GridPosition = newGridPosition;
                transform.position = finalPosition;
            }
            else
            {
                transform.position = initialPosition;
            }
        }
    }

}
