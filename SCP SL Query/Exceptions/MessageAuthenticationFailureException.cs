using System;

public class MessageAuthenticationFailureException : Exception {

    public MessageAuthenticationFailureException()
    {
    }

    public MessageAuthenticationFailureException(string message)
        : base(message)
    {
    }

    public MessageAuthenticationFailureException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
