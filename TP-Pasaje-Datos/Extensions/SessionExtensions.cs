using System.Text.Json;

namespace TP_Pasaje_Datos.Extensions {
    public static class SessionExtensions {
        public static void Set<T>(this ISession session, string clave, T valor) {
            session.SetString(clave, JsonSerializer.Serialize(valor));
        }
        public static T Get<T>(this ISession session, string clave) {
            var valor = session.GetString(clave);
            return valor == null ? default(T) : JsonSerializer.Deserialize<T>(valor);
        }
    }
}
