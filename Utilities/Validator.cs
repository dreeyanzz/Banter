using System.Text.RegularExpressions;

namespace Banter
{
    public static class Validator
    {
        // A commonly used, comprehensive regex pattern for basic email validation
        // Note: A perfect regex is impossible, but this one covers 99.9% of real cases.
        private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        /// <summary>
        /// Checks if a given string is a valid email address format.
        /// </summary>
        /// <param name="email">The string to validate.</param>
        /// <returns>True if the email format is valid, otherwise False.</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Use the Regex class to perform the match
            try
            {
                // RegexOptions.IgnoreCase is often useful for email addresses
                return Regex.IsMatch(
                    email.Trim(),
                    EmailRegexPattern,
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromMilliseconds(250)
                );
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle cases where the regex engine takes too long (rare, but good practice)
                return false;
            }
        }
    }
}
