using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct MoveResult
{
    public bool IsValid;
    public bool ShouldContinueTurn;
    public bool ShouldTakePiece;
    public GameObject PieceToTake;
    
    public MoveResult(bool isValid, bool shouldContinueTurn, GameObject pieceToTake)
    {
        IsValid = isValid;
        ShouldContinueTurn = shouldContinueTurn;
        ShouldTakePiece = true;
        PieceToTake = pieceToTake;
    }

    public MoveResult(bool isValid, bool shouldContinueTurn)
    {
        IsValid = isValid;
        ShouldContinueTurn = shouldContinueTurn;
        ShouldTakePiece = false;
        PieceToTake = null;
    }

    public MoveResult(bool isValid)
    {
        IsValid = isValid;
        ShouldContinueTurn = !isValid;
        ShouldTakePiece = false;
        PieceToTake = null;
    }
}

public class GameManager : MonoBehaviour
{
    public PieceType Turn = PieceType.RED;
    public GameObject GameBoard;
    public GameObject UICanvas;
    public ResultsPanel ResultsPanel;

    [Space()]
    public GameObject RedTurnIndicator;
    public GameObject WhiteTurnIndicator;

    [Space()]
    public GameObject RedPiecePrefab;
    public GameObject WhitePiecePrefab;

    [HideInInspector]
    public List<GameObject> GamePieces;

    public void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        Turn = PieceType.RED;
        InitializeBoard();
        SetTurnIndicators();
    }

    public void InitializeBoard()
    {
        // Delete existing pieces
        GamePieces.ForEach(p => Destroy(p));
        GamePieces.Clear();

        // Hide Results Dialogue
        ResultsPanel.Disable();

        for (int i = 0; i < 12; i++)
        {
            // Add red pieces
            float irow = Mathf.Floor(i / 4);
            Vector2 redPiecePosition = new Vector2((i * 2 + irow % 2) % 8, irow);
            GameObject newRedPieceObj = Instantiate(RedPiecePrefab);
            newRedPieceObj.transform.SetParent(UICanvas.transform);
            GamePiece newRedPiece = newRedPieceObj.GetComponent<GamePiece>();
            newRedPiece.Board = GameBoard;
            newRedPiece.Manager = this;
            newRedPiece.GridPosition = redPiecePosition;
            GamePieces.Add(newRedPieceObj);

            // Add white pieces
            int j = i + 20;
            float jrow = Mathf.Floor(j / 4);
            Vector2 whitePiecePosition = new Vector2((j * 2 + jrow % 2) % 8, jrow);
            GameObject newWhitePieceObj = Instantiate(WhitePiecePrefab);
            newWhitePieceObj.transform.SetParent(UICanvas.transform);
            GamePiece newWhitePiece = newWhitePieceObj.GetComponent<GamePiece>();
            newWhitePiece.Board = GameBoard;
            newWhitePiece.Manager = this;
            newWhitePiece.GridPosition = whitePiecePosition;
            GamePieces.Add(newWhitePieceObj);
        }
    }

    public void ChangeTurn()
    {
        Turn = Turn.Opposite();
        SetTurnIndicators();
    }

    public void SetTurnIndicators()
    {
        RedTurnIndicator.SetActive(Turn == PieceType.RED);
        WhiteTurnIndicator.SetActive(Turn == PieceType.WHITE);
    }

    public bool TryMove(GamePiece piece, Vector2 newPosition)
    {
        MoveResult result = TestMove(piece, newPosition);

        if (!result.IsValid)
            return false;
        else
        {
            // Take piece if one should be taken
            if (result.ShouldTakePiece)
            {
                Destroy(result.PieceToTake);
                GamePieces.Remove(result.PieceToTake);
            }

            // King piece if they have moved to the other end of the board
            if (newPosition.y == piece.color.OpponentBoardEdge())
                piece.king = true;

            // Change turns if they should change
            if (!result.ShouldContinueTurn)
                ChangeTurn();

            return true;
        }
    }

    MoveResult TestMove(GamePiece piece, Vector2 newPosition)
    {
        Debug.Log($"{piece.color.ToString()} trying to move from {piece.GridPosition} > {newPosition}");

        // Can only move a piece of the right turn color
        if (piece.color != Turn)
            return new MoveResult(false);

        // Can only move to white squares
        if ((newPosition.x + newPosition.y) % 2 == 1)
            return new MoveResult(false);

        // Can't move into a position with another piece
        if (GamePieces.ConvertAll(p => p.GetComponent<GamePiece>()).Exists(p => p.GridPosition == newPosition))
            return new MoveResult(false);

        List<Vector2> directions = new List<Vector2>();

        // If a piece is a king, it can move in all four directions
        if (piece.king)
        {
            directions = new List<Vector2>()
            {
                new Vector2(1, 1),
                new Vector2(-1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1)
            };
        } 

        // Otherwise, it can only move diagonally forward, left or right
        else
        {
            directions = new List<Vector2>()
            {
                new Vector2(1, 0) + piece.color.ForwardDirection(),
                new Vector2(-1, 0) + piece.color.ForwardDirection()
            };
        }

        foreach (Vector2 dir in directions)
        {
            // Piece can move to grid squares diagonally beside it
            Vector2 movement = piece.GridPosition + dir;
            if (newPosition == movement)
                return new MoveResult(true);

            // Piece can move over another piece diagonally to take it
            Vector2 longMovement = piece.GridPosition + dir * 2;
            if (newPosition == longMovement)
            {
                foreach (GameObject pieceObj in GamePieces)
                {
                    GamePiece p = pieceObj.GetComponent<GamePiece>();
                    if (p.GridPosition == movement && p.color != piece.color)
                        return new MoveResult(true, true, pieceObj);
                }
                return new MoveResult(false);
            }
        }

        return new MoveResult(false);
    }
}
