using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public static class MongoDbSettings
    {

        // #if ANDROID es una directiva de compilación condicional.
        // Cuando compilas para Android (emulador o físico), entra aquí.
#if ANDROID
        // Para EMULADOR de Android usarías 10.0.2.2.
        // Pero para TELÉFONO FÍSICO necesitamos la IP real de la PC.
        // Como estamos probando en físico, vamos a usar la IP real también aquí
        // o dejar la lógica de la guía y modificar el #else si compilas como Windows.

        // ⚠️ CAMBIO CRÍTICO PARA TU CASO:
        // Tu error muestra que intenta conectar a 192.168.1.34 y falla.
        // Vamos a poner tu IP ACTUAL aquí directamente para forzarla en Android.
        public const string ConnectionString = "mongodb://172.16.138.222";
#else
    // Windows usa localhost o la IP real si pruebas desde la PC hacia otro servidor.
    // Para tu caso, esto no se ejecuta en el celular, pero lo dejamos por si pruebas en Windows.
    public const string ConnectionString = "mongodb://localhost:27017";
#endif

        // Nombre de la base de datos que creaste en Compass (paso 1.2).
        public const string DatabaseName = "AppDeTareasDB";
    }
}
