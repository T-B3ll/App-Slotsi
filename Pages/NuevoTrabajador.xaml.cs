using slotsi_citas.ViewModels;
using System.Text;

namespace slotsi_citas.Pages;

public partial class NuevoTrabajador : ContentPage
{
    private const string PrefijoTelefono = "+505 ";
    private readonly NuevoTrabajadorViewModel viewModel;
    private bool _isUpdating = false;
    public NuevoTrabajador(NuevoTrabajadorViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private void OnTelefonoFocused(object? sender, FocusEventArgs e)
    {
        if (string.IsNullOrEmpty(EntryTelefono.Text))
            AplicarTexto(EntryTelefono, PrefijoTelefono);
    }

    private void OnTelefonoUnfocused(object? sender, FocusEventArgs e)
    {
        if (EntryTelefono.Text?.Trim() == PrefijoTelefono.Trim())
            AplicarTexto(EntryTelefono, string.Empty);
    }

    private void OnTelefonoChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdating || sender is not Entry entry) return;

        string texto = e.NewTextValue ?? string.Empty;
        if (texto.Length == 0) return; 
        string formateado = FormatearTelefono(texto);
        if (formateado != texto)
            AplicarTexto(entry, formateado);
    }

    private static string FormatearTelefono(string texto)
    {
        string digitos = new string(texto.Where(c => c >= '0' && c <= '9').ToArray());

        if (texto.TrimStart().StartsWith("+"))
        {
            int i = 0;
            while (i < 3 && i < digitos.Length && digitos[i] == "505"[i]) i++;
            digitos = digitos.Substring(i);
        }
        else if (digitos.Length > 8 && digitos.StartsWith("505"))
        {
            digitos = digitos.Substring(3);
        }

        if (digitos.Length > 8)
            digitos = digitos.Substring(0, 8);

        string local = digitos.Length > 4 ? digitos.Insert(4, "-") : digitos;
        return PrefijoTelefono + local;
    }

    private void OnCedulaChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdating || sender is not Entry entry) return;

        string texto = e.NewTextValue ?? string.Empty;
        string formateado = FormatearCedula(texto);

        if (formateado != texto)
            AplicarTexto(entry, formateado);
    }

    private static string FormatearCedula(string texto)
    {
        var limpio = new StringBuilder();
        foreach (char c in texto)
        {
            if (limpio.Length < 13)
            {
                if (c >= '0' && c <= '9') limpio.Append(c);
            }
            else if (limpio.Length == 13)
            {
                char u = char.ToUpperInvariant(c);
                if (u >= 'A' && u <= 'Z') limpio.Append(u);
            }
        }

        string s = limpio.ToString();
        if (s.Length > 9) return $"{s[..3]}-{s[3..9]}-{s[9..]}";
        if (s.Length > 3) return $"{s[..3]}-{s[3..]}";
        return s;
    }

    private void AplicarTexto(Entry entry, string texto)
    {
        _isUpdating = true;
        try
        {
            entry.Text = texto;
        }
        finally
        {
            _isUpdating = false;
        }

        entry.Dispatcher.Dispatch(() => entry.CursorPosition = entry.Text?.Length ?? 0);
    }
}
