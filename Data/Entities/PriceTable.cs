namespace Sistema.Data.Entities
{
    public class PriceTable : IEntity
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public string? ServiceName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

}
