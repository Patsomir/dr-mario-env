using UnityEngine;

public class StepGame : GameAPI
{
    [SerializeField]
    private Game game;

    override public void ResetGame(int width, int height, int virusHeight, int virusCount)
    {
        game.ResetGame(width, height, virusHeight, virusCount);
    }

    override public void MoveLeft()
    {
        game.MoveLeft();
    }

    override public void MoveRight()
    {
        game.MoveRight();
    }

    override public void MoveDown()
    {
        game.MoveDown();
    }

    override public void RotateLeft()
    {
        game.RotateLeft();
    }

    override public void RotateRight()
    {
        game.RotateRight();
    }

    override public void Wait()
    {
        game.Wait();
    }
}
