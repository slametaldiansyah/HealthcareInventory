namespace OrderInventory.Domain.Exceptions;

public class OrderConflictException : Exception
{
    public OrderConflictException(string message) : base(message)
    {
    }
}
