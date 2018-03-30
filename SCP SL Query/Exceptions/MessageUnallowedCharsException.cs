using System;
public class MessageUnallowedCharsException : Exception
{

    public MessageUnallowedCharsException()
    {
    }

    public MessageUnallowedCharsException(string message)
        : base(message)
    {
    }

    public MessageUnallowedCharsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
