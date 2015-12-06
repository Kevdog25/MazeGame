using System;

[Serializable]
class MazeGameException : Exception
{
    public MazeGameException()
    {

    }

    public MazeGameException(string message) 
        : base(message)
    {

    }
    
    public MazeGameException(string message, Exception inner)
        : base(message, inner)
    {

    }
}
