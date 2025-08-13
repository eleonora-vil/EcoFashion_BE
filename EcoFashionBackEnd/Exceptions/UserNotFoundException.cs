namespace EcoFashionBackEnd.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException()
            : base("User not found.")
        {
        }

        public UserNotFoundException(string message)
            : base(message)
        {
        }

        public UserNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
