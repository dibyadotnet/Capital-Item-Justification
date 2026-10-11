using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Capital_Item_Justification.Validation
{
    public class ValidEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string email || string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                var address = new MailAddress(email);

                if (address.Address != email)
                    return false;

                var parts = email.Split('@');

                if (parts.Length != 2)
                    return false;

                var domain = parts[1];

                // Require a valid-looking domain and extension.
                if (domain.EndsWith('.') ||
                    domain.StartsWith('.') ||
                    !domain.Contains('.'))
                    return false;

                var labels = domain.Split('.');

                if (labels.Any(label =>
                    string.IsNullOrWhiteSpace(label) ||
                    label.StartsWith('-') ||
                    label.EndsWith('-') ||
                    !label.All(c =>
                        char.IsLetterOrDigit(c) || c == '-')))
                    return false;

                // Require at least two characters in the final extension.
                if (labels[^1].Length < 2 ||
                    !labels[^1].All(char.IsLetter))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return "Please enter a valid email address.";
        }
    }
}

