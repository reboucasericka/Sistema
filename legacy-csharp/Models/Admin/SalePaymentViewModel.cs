namespace Sistema.Models.Admin
{
    public class SalePaymentViewModel
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Received { get; set; }
        public decimal Change { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
