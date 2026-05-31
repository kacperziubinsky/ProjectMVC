using System.ComponentModel.DataAnnotations;

namespace MVCProject.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "E-mail jest wymagany.")]
    [EmailAddress(ErrorMessage = "Podaj prawidłowy adres e-mail.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwa wyświetlana jest wymagana.")]
    [StringLength(100)]
    [Display(Name = "Nazwa wyświetlana")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej {2} znaków.")]
    [DataType(DataType.Password)]
    [Display(Name = "Hasło")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Potwierdź hasło")]
    [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
