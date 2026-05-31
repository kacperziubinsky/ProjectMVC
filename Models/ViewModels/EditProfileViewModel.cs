using System.ComponentModel.DataAnnotations;

namespace MVCProject.Models.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "E-mail jest wymagany.")]
    [EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwa wyświetlana jest wymagana.")]
    [StringLength(100)]
    [Display(Name = "Nazwa wyświetlana")]
    public string DisplayName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Nowe hasło (opcjonalnie)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Potwierdź nowe hasło")]
    [Compare("NewPassword", ErrorMessage = "Hasła nie są identyczne.")]
    public string? ConfirmNewPassword { get; set; }
}
