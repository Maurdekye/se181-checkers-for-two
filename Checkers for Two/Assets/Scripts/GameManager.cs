using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MoveResult
{
    public bool IsValid;
    public bool wasJumpingMove;
    public bool ShouldTakePiece;
    public GameObject PieceToTake;
    
    public MoveResult(bool isValid, bool shouldContinueTurn, GameObject pieceToTake)
    {
        IsValid = isValid;
        wasJumpingMove = shouldContinueTurn;
        ShouldTakePiece = true;
        PieceToTake = pieceToTake;
    }

    public MoveResult(bool isValid, bool shouldContinueTurn)
    {
        IsValid = isValid;
        wasJumpingMove = shouldContinueTurn;
        ShouldTakePiece = false;
        PieceToTake = null;
    }

    public MoveResult(bool isValid)
    {
        IsValid = isValid;
        wasJumpingMove = !isValid;
        ShouldTakePiece = false;
        PieceToTake = null;
    }
}

public enum WinResult
{
    NOT_OVER, RED, WHITE, DRAW
}

public enum TurnState
{
    NORMAL, CONSECUTIVE_JUMP
}

public class GameManager : MonoBehaviour
{
    public GameObject GameBoard;
    public GameObject UICanvas;
    public ResultsPanel ResultsPanel;

    [Space()]
    public PieceType Turn = PieceType.RED;
    public TurnState TurnState = TurnState.NORMAL;
    public bool GameOver = false;

    [Space()]
    public GameObject RedTurnIndicator;
    public GameObject WhiteTurnIndicator;

    [Space()]
    public GameObject RedPiecePrefab;
    public GameObject WhitePiecePrefab;

    [HideInInspector]
    public List<GameObject> GamePieceObjs;

    // DO NOT modify this variable, only modify GamePieceObjs above
    public List<GamePiece> GamePieces {
        get {
            return GamePieceObjs.ConvertAll(p => p.GetComponent<GamePiece>());
        }
    }

    GamePiece JumpingPiece;

    public void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        Turn = PieceType.RED;
        TurnState = TurnState.NORMAL;
        JumpingPiece = null;
        GameOver = false;
        InitializeBoard();
        SetTurnIndicators();
    }

    public void InitializeBoard()
    {
        // Delete existing pieces
        GamePieceObjs.ForEach(p => Destroy(p));
        GamePieceObjs.Clear();

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
            GamePieceObjs.Add(newRedPieceObj);

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
            GamePieceObjs.Add(newWhitePieceObj);
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

    public bool CanMove(GamePiece piece)
    {
        if (Turn != piece.color || GameOver)
            return false;

        if (TurnState == TurnState.CONSECUTIVE_JUMP && piece != JumpingPiece)
            return false;

        return true;
    }

    public void DoMove(GamePiece piece, MoveResult result, Vector2 newPosition)
    {
        // Move piece to new position
        piece.GridPosition = newPosition;

        // Take piece if one should be taken
        if (result.ShouldTakePiece)
        {
            Destroy(result.PieceToTake);
            GamePieceObjs.Remove(result.PieceToTake);
        }

        // King piece if they have moved to the other end of the board
        if (piece.GridPosition.y == piece.color.OpponentBoardEdge())
            piece.king = true;

        // Check if game over
        WinResult turnResult = CheckIfGameWon();
        if (turnResult != WinResult.NOT_OVER)
        {
            EndGame(turnResult);
            return;
        }

        // Check if another jumping move can be made
        if (result.wasJumpingMove && CanMakeAdditionalJump(piece))
        {
            TurnState = TurnState.CONSECUTIVE_JUMP;
            JumpingPiece = piece;
        }
        else
        {
            TurnState = TurnState.NORMAL;
            JumpingPiece = null;
            ChangeTurn();
        }

    }

    public void EndGame(WinResult winner)
    {
        GameOver = true;
        ResultsPanel.DecideWinner(winner);
    }

    public void Draw()
    {
        EndGame(WinResult.DRAW);
    }

    public MoveResult CheckMove(GamePiece piece, Vector2 newPosition)
    {
        //Debug.Log($"{piece.color.ToString()} trying to move from {piece.GridPosition} > {newPosition}");

        // Can only move a piece of the right turn color
        if (piece.color != Turn)
            return new MoveResult(false);

        // Can only move to white squares
        if ((newPosition.x + newPosition.y) % 2 == 1)
            return new MoveResult(false);

        // Can't move into a position with another piece
        if (GamePieces.Exists(p => p.GridPosition == newPosition))
            return new MoveResult(false);

        foreach (Vector2 dir in piece.MovementDirections())
        {
            // Piece can move to grid squares diagonally beside it
            Vector2 movement = piece.GridPosition + dir;
            if (newPosition == movement)
                return new MoveResult(true);

            // Piece can move over another piece diagonally to take it
            Vector2 longMovement = piece.GridPosition + dir * 2;
            if (newPosition == longMovement)
            {
                foreach (GameObject pieceObj in GamePieceObjs)
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

    public bool CanMakeAdditionalJump(GamePiece piece)
    {
        foreach (Vector2 dir in piece.MovementDirections())
        {
            Vector2 movement = piece.GridPosition + dir;
            Vector2 longMovement = piece.GridPosition + dir * 2;

            bool emptySpace = !GamePieces.Exists(p => p.GridPosition == longMovement);
            bool pieceToJumpOver = GamePieces.Exists(p => p.GridPosition == movement && p.color != piece.color);
            bool movementInBounds = longMovement.x >= 0 && longMovement.y >= 0 && longMovement.x <= 7 && longMovement.y <= 7;
            Debug.Log($"{movement}, {longMovement}; {emptySpace}, {pieceToJumpOver}, {movementInBounds}");
            if (emptySpace && pieceToJumpOver && movementInBounds)
                return true;
        }

        return false;
    }

    public WinResult CheckIfGameWon()
    {
        bool anyRed = false;
        bool anyWhite = false;

        foreach (GamePiece piece in GamePieces)
        {
            if (piece.color == PieceType.RED)
                anyRed = true;
            else if (piece.color == PieceType.WHITE)
                anyWhite = true;
            if (anyRed && anyWhite)
                return WinResult.NOT_OVER;
        }

        if (anyRed)
            return WinResult.RED;
        else if (anyWhite)
            return WinResult.WHITE;
        else
            return WinResult.DRAW;
    }
}
