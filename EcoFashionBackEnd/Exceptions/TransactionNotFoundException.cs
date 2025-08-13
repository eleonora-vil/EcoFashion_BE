namespace EcoFashionBackEnd.Exceptions
{
    public class TransactionNotFoundException : Exception
    {
        public TransactionNotFoundException()
            : base("Transaction not found.")
        {
        }

        public TransactionNotFoundException(string message)
            : base(message)
        {
        }

        public TransactionNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
