using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bot
{
    /// <summary>
    /// Returns a List of every legal move which can be made by a player.<br/>
    /// This does not rate the moves.
    /// </summary>
    /// <param name="colour">The colour of the player</param>
    /// <param name="boardPosition">The position on which the move is to be played</param>
    /// <returns>A List of all legal moves</returns>
    public static List<Move> AllMoves(Colour colour, byte[] boardPosition)
    {
        List<Move> moves = new();

        for (int i = 0; i < boardPosition.Length; i++)
        {
            if (Utility.IsColour(boardPosition[i], colour))
            {
                List<int> intMoves = new();
                switch (Utility.TypeCode(boardPosition[i]))
                {
                    case Piece.Pawn:
                        intMoves.AddRange(MoveSets.CalculatePawnMoves(i, colour, Utility.HasMoved(boardPosition[i])));
                        break;
                    case Piece.Rook:
                        intMoves.AddRange(MoveSets.CalculateRookMoves(i, colour));
                        break;
                    case Piece.Knight:
                        intMoves.AddRange(MoveSets.CalculateKnightMoves(i, colour));
                        break;
                    case Piece.Bishop:
                        intMoves.AddRange(MoveSets.CalculateBishopMoves(i, colour));
                        break;
                    case Piece.Queen:
                        intMoves.AddRange(MoveSets.CalculateQueenMoves(i, colour));
                        break;
                    case Piece.King:
                        intMoves.AddRange(MoveSets.CalculateKingMoves(i, colour, Utility.HasMoved(boardPosition[i])));
                        break;
                }
                List<int> intMovesPostFilter = new();
                foreach (int move in intMoves)
                {
                    if (MoveSets.ProtectsCheck(i, move, colour))
                        intMovesPostFilter.Add(move);
                }

                foreach (int intMove in intMovesPostFilter)
                {
                    moves.Add(new(i, intMove));
                }
            }
        }
        return moves;
    }

    /// <summary>
    /// Finds the best move in a List of <paramref name="moves"/>, based on the move.rating value
    /// </summary>
    /// <param name="moves">A List of moves</param>
    /// <param name="colour">The perspective of the check</param>
    /// <returns>The best move for the player of the specified <paramref name="colour"/></returns>
    public static Move BestMove(List<Move> moves, Colour colour)
    {
        Move bestMove = moves[0];
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];
            if (colour == Colour.White && move.GetRating() > bestMove.GetRating())
            {
                bestMove = move;
            }
            else if (colour == Colour.Black && move.GetRating() < bestMove.GetRating())
            {
                bestMove = move;
            }
        }
        return bestMove;
    }

    /// <summary>
    /// Finds the worst move in a List of moves, based on the move.rating value
    /// </summary>
    /// <param name="moves">A List of moves</param>
    /// <param name="colour">The perspective of the check</param>
    /// <returns>The worst move for the player of the specified <paramref name="colour"/>.</returns>
    public static Move WorstMove(List<Move> moves, Colour colour)
    {
        Move worstMove = moves[0];
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];
            if (colour == Colour.White && move.GetRating() < worstMove.GetRating())
            {
                worstMove = move;
            }
            else if (colour == Colour.Black && move.GetRating() > worstMove.GetRating())
            {
                worstMove = move;
            }
        }
        return worstMove;
    }

    public static Move BestNormalisedMove(List<Move> moves)
    {
        return BestMove(moves, Colour.White);
    }

    /// <summary>
    /// Finds the rating of a given <paramref name="boardPosition"/>, normalised so that the rating is positive <br/>
    /// when the player of <paramref name="colour"/> is winning.
    /// </summary>
    /// <param name="boardPosition">The board position to be evaluated</param>
    /// <param name="colour">The colour of the player to play the move</param>
    /// <param name="depth">The number of recursive searches to perform</param>
    /// <param name="alpha">Unused (will be used for optimisation later)</param>
    /// <param name="beta">Unused (will be used for optimisation later)</param>
    /// <returns>The normalised rating of <paramref name="boardPosition"/></returns>
    public static int RateBoardNormalised(byte[] boardPosition, Colour colour, int depth, int alpha, int beta)
    {
        Colour enemyColour = (colour == Colour.White) ? Colour.Black : Colour.White;
        
        // black has to multiply by -1 for - positions to be good for black
        int perspective = (colour == Colour.Black) ? -1 : 1;

        List<Move> moves = AllMoves(colour, boardPosition);
        Move best = new(moves[0].GetOldPos(), moves[0].GetNewPos()); // when in doubt, do the first move you can think of
        

        // base case, end recursion (< 1 for invalid depths, stopping infinite recursion)
        if (depth < 1)
        {
            byte[] theoryBoard = (byte[])boardPosition.Clone();
            MakeTheoryMove(theoryBoard, best);
            best.SetRating(Evaluation.EvalBoard(theoryBoard) * perspective); // evaluate the first move

            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                theoryBoard = (byte[])boardPosition.Clone();
                MakeTheoryMove(theoryBoard, move);
                move.SetRating(Evaluation.EvalBoard(theoryBoard) * perspective);
                if (move.GetRating() > best.GetRating())
                {
                    best.SetOldPos(move.GetOldPos());
                    best.SetNewPos(move.GetNewPos());
                    best.SetRating(move.GetRating());
                }
            }
            return best.GetRating();
        }
        if (moves.Count == 0) // bot lost or stalemate
        {
            if (Board.CheckForMate(colour)) // bot loses
                return int.MinValue * perspective; // you don't wanna lose
            return 0; // sometimes drawing is desirable
        }

        // the rating of this position is the worst thing the enemy can respond with
        int worstNormalisedRating = 0;
        for (int i = 0; i < moves.Count; i++)
        {
            Move move = new(moves[i].GetOldPos(), moves[i].GetNewPos());
            byte[] theoryBoard = (byte[])boardPosition.Clone();
            MakeTheoryMove(theoryBoard, move);
            int normalisedRating = RateBoardNormalised(theoryBoard, enemyColour, depth - 1, alpha, beta);
            if (normalisedRating < worstNormalisedRating)
                worstNormalisedRating = normalisedRating;
        }
        //foreach (Move move in moves)
        //{
        //    byte[] theoryBoard = (byte[])boardPosition.Clone();
        //    MakeTheoryMove(theoryBoard, move);
        //    int eval = RateBoardNormalised(theoryBoard, enemyColour, depth - 1, -alpha, -beta);
        //    if (eval > beta)
        //    {
        //        return beta;
        //    }
        //    alpha = Mathf.Max(alpha, eval);
        //}
        //return alpha;
        return worstNormalisedRating;
    }

    /// <summary>
    /// Simulates a <paramref name="move"/> being played on a theoretical position.<br/>
    /// This method <b>will</b> modify the values in the <paramref name="theoryPos"/> array
    /// </summary>
    /// <param name="theoryPos">The theoretical board to play the move on</param>
    /// <param name="move"></param>
    public static void MakeTheoryMove(byte[] theoryPos, Move move)
    {
        theoryPos[move.GetNewPos()] = theoryPos[move.GetOldPos()];
        theoryPos[move.GetNewPos()] |= Piece.HasMoved;
        theoryPos[move.GetOldPos()] = Piece.None;
        RemoveEnPassant(theoryPos);
        if (Utility.TypeCode(theoryPos[move.GetNewPos()]) == Piece.Pawn)
        {
            if (move.GetNewPos() == move.GetOldPos() + 16)
            {
                theoryPos[move.GetOldPos() + 8] = (byte)(Piece.EnPassant | Utility.ColourCode(theoryPos[move.GetNewPos()]));
            }
            else if (move.GetNewPos() == move.GetOldPos() - 16)
            {
                theoryPos[move.GetOldPos() - 8] = (byte)(Piece.EnPassant | Utility.ColourCode(theoryPos[move.GetNewPos()]));
            }
        }
        else if (Utility.TypeCode(theoryPos[move.GetOldPos()]) == Piece.King)
        {
            if (move.GetNewPos() == move.GetOldPos() - 2) // castled queenside
            {
                theoryPos[move.GetNewPos() + 1] = theoryPos[move.GetNewPos() - 2]; // add rook to new position
                theoryPos[move.GetNewPos() - 2] = Piece.None; // remove rook from old position
            }
            else if (move.NewPos == move.GetOldPos() + 2) // castled kingside
            {
                theoryPos[move.GetNewPos() - 1] = theoryPos[move.GetNewPos() + 1]; // add rook to new position
                theoryPos[move.GetNewPos() + 1] = Piece.None; // remove rook from old position
            }
        }
    }

    public static void RemoveEnPassant(byte[] boardPosition)
    {
        for (int i = 0; i < boardPosition.Length; i++)
        {
            if (Utility.TypeCode(boardPosition[i]) == Piece.EnPassant)
            {
                boardPosition[i] = Piece.None;
            }
        }
    }
}
