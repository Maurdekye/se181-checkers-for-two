using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class PieceManager : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public GameObject Board;
    [Range(1,32)]
    public int GridSize = 8;
    public Vector2 GridPosition;

    Vector2 initialPosition;

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
            GridPosition = snappedScaledPosition;
            snappedScaledPosition += new Vector2(0.5f, 0.5f);
            Vector3 snappedPosition = snappedScaledPosition / GridSize;
            Vector3 finalPosition = snappedPosition * boardDimension + topLeft;

            transform.position = finalPosition;
        }
    }

}
