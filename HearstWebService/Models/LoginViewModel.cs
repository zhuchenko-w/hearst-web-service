using System.ComponentModel.DataAnnotations;

namespace HearstWebService.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите домен")]
        [Display(Name = "Домен")]
        public string Domain { get; set; }

        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя пользователя")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }
}