using Supabase;
using Supabase.Storage;
using System.IO;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class SupabaseStorageService
    {
        private readonly Supabase.Client _client;

        private const string BucketName = "slotsi-images";
        public SupabaseStorageService()
        {
            var supabaseUrl = "https://vnasklmkkamytgwymzih.supabase.co";
            var supabaseKey = "sb_publishable_z8qlsYsGV7mYU4iJGGu8NA_fljnaFtq";

            var options = new SupabaseOptions { AutoRefreshToken = true };
            _client = new Supabase.Client(supabaseUrl, supabaseKey, options);

        }


        public async Task<string> SubirArchivoAsync(Stream stream, string folder, string fileName)
        {
            try
            {
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var bucket = _client.Storage.From(BucketName);
                var path = $"{folder}/{fileName}";

                await bucket.Upload(fileBytes, path, new Supabase.Storage.FileOptions { Upsert = true });
                return bucket.GetPublicUrl(path);



            }
            catch (Exception ex)
            {

                throw new Exception($"Error al subir archivo a Supabase: {ex.Message}");


            }
        }
    }
}
