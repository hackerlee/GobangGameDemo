public class TurnManager
{
    public bool IsPlayerTurn { get; private set; }

    public void Initialize(bool isPlayerTurn)
    {
        IsPlayerTurn = isPlayerTurn;
    }

    public void SwitchTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
    }

    public void SetPlayerTurn(bool isPlayerTurn)
    {
        IsPlayerTurn = isPlayerTurn;
    }
}
