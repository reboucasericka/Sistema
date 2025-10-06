namespace Sistema.Models.Admin
{
    /// <summary>
    /// Representa uma notificação para o painel administrativo.
    /// </summary>
    public class Notification
    {
        public string Message { get; set; }   // Texto da notificação
        public string Icon { get; set; }      // Ícone (ex: "fas fa-calendar")
        public string Time { get; set; }      // Ex: "3 mins", "2 dias"
        public string Link { get; set; }      // Para onde o usuário vai ao clicar
    }
}