using System.ComponentModel.DataAnnotations;

namespace Slotsi.Api.Configuration
{
    public class JwtSetting
    {
        public const string SectionName = "Jwt";

        [Required, MinLength(32)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = "Slotsi.Api";

        [Required]
        public string Audience { get; set; } = "slotsi-citas";

        [Range(1, 1440)]
        public int ExpirationMinutes { get; set; } = 15;
    }
}
