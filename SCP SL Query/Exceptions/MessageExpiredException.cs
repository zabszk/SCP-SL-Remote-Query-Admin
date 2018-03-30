using System;

public class MessageExpiredException : Exception
{

    public MessageExpiredException()
    {
    }

    public MessageExpiredException(string message)
        : base(message)
    {
    }

    public MessageExpiredException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
