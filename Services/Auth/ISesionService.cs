using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services.Auth
{
    public interface ISesionService
    {
        // null si no hay sesión o el token ya caducó.
        string? Token { get; }
        bool EstaAutenticado { get; }
        // Intenta recuperar la sesión guardada en el almacenamiento seguro.
        Task<bool> RestaurarAsync();
        Task IniciarAsync(TokenDto token);
        // Olvida el token (memoria y almacenamiento seguro).
        void Cerrar();

    }
}
