namespace Sistema.DTOs.MAUI
{
    public class ServiceModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Propriedades para UI
        public string PriceDisplay => $"€{Price:F2}";
        public string DurationDisplay => $"{DurationInMinutes} min";
    }
}
