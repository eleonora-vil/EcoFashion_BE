namespace EcoFashionBackEnd.Common.Payloads.Requests
{
	public class UpsertCartItemRequest
	{
		public int MaterialId { get; set; }
		public int Quantity { get; set; }
	}

	public class UpdateCartItemQuantityRequest
	{
		public int Quantity { get; set; }
	}
}


