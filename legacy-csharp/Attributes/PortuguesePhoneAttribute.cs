using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sistema.Attributes
{
    public class PortuguesePhoneAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true; // Deixa o [Required] lidar com valores nulos

            string phone = value.ToString();
            
            // Remove todos os espaços e caracteres não numéricos exceto +
            string cleanPhone = Regex.Replace(phone, @"[^\d+]", "");
            
            // Remove o +351 se presente
            if (cleanPhone.StartsWith("+351"))
            {
                cleanPhone = cleanPhone.Substring(4);
            }
            else if (cleanPhone.StartsWith("351"))
            {
                cleanPhone = cleanPhone.Substring(3);
            }
            
            // Deve ter exatamente 9 dígitos
            return cleanPhone.Length == 9 && Regex.IsMatch(cleanPhone, @"^\d{9}$");
        }

        public override string FormatErrorMessage(string name)
        {
            return "Digite um telemóvel português válido (ex: 912 345 678)";
        }
    }
}
