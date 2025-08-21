namespace EcoFashionBackEnd.Common.Payloads.Requests.Variant
{
    public class DesignsVariantCreateRequest
    {
        public int SizeId { get; set; }
        public string ColorCode { get; set; }
        public int Quantity { get; set; }

    }
}
//[
//  {
//    "SizeId": 1,
//    "ColorCode": "#FF0000",
//    "Quantity": 10
//  },
//  {
//    "SizeId": 2,
//    "ColorCode": "#0000FF",
//    "Quantity": 5
//  },
//  {
//    "SizeId": 3,
//    "ColorCode": "#00FF00",
//    "Quantity": 7
//  }
//]
