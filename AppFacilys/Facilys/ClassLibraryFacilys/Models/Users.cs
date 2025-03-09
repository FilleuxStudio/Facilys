using BCrypt.Net;
using System.ComponentModel.DataAnnotations;

namespace ClassLibraryFacilys.Models
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Lname { get; set; } = string.Empty;
        [Required]
        public string Fname { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string Login {  get; set; } = string.Empty;
        public string? Picture {  get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public RoleUser Role { get; set; } = RoleUser.Guest;
        [Required]
        public DateTime DateAdded { get; set; } = DateTime.Now;


        /// <summary>
        /// Méthode pour hacher le mot de passe
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Méthode pour vérifier le mot de passe
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }

    public enum RoleUser
    {
        Guest = 0,
        User = 1,
        SuperUser = 2,
        Manager = 3,
        Administrator = 4,
    }
}
