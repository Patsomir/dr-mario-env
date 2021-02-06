using UnityEngine;

public class NormalGame : GameAPI
{
    public float cycleDurationSlow = 1.0f;
    public float cycleDurationFast = 0.5f;

    private float currentCycleDuration;
    private float cycleEndTimestamp;

    [SerializeField]
    private Game game;

    override public void ResetGame(int width, int height, int virusHeight, int virusCount)
    {
        game.ResetGame(width, height, virusHeight, virusCount);
        cycleEndTimestamp = Time.time + cycleDurationSlow;
    }

    private bool shouldMoveLeft = false;
    override public void MoveLeft()
    {
        shouldMoveLeft = true;
    }

    private bool shouldMoveRight = false;
    override public void MoveRight()
    {
        shouldMoveRight = true;
    }

    private bool shouldMoveDown = false;
    override public void MoveDown()
    {
        shouldMoveDown = true;
    }

    private bool shouldRotateLeft = false;
    override public void RotateLeft()
    {
        shouldRotateLeft = true;
    }

    private bool shouldRotateRight = false;
    override public void RotateRight()
    {
        shouldRotateRight = true;
    }

    void HandleMovement()
    {
        if (shouldMoveLeft)
        {
            game.MoveLeft();
            shouldMoveLeft = false;
        }
        if (shouldMoveRight)
        {
            game.MoveRight();
            shouldMoveRight = false;
        }
        if (shouldMoveDown)
        {
            if (game.MoveDown())
            {
                cycleEndTimestamp = Time.time + cycleDurationSlow;
            }
            shouldMoveDown = false;
        }
        if (shouldRotateLeft)
        {
            game.RotateLeft();
            shouldRotateLeft = false;
        }
        if (shouldRotateRight)
        {
            game.RotateRight();
            shouldRotateRight = false;
        }
    }

    void HandleCycleDuration()
    {
        if (game.PlayerCanPlay())
        {
            currentCycleDuration = cycleDurationSlow;
        }
        else
        {
            currentCycleDuration = cycleDurationFast;
        }
    }

    override public void Wait() { }

    void Update()
    {
        if (game.GameHasEnded())
        {
            return;
        }
        HandleMovement();
        if (Time.time > cycleEndTimestamp)
        {
            game.Wait();
            HandleCycleDuration();
            cycleEndTimestamp += currentCycleDuration;
        }
    }
}
