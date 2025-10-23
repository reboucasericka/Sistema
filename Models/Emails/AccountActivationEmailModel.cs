namespace Sistema.Models.Emails
{
    public class AccountActivationEmailModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string ActivationLink { get; set; } = string.Empty;
    }
}
