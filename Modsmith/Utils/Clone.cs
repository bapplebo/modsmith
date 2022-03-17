using Newtonsoft.Json;

namespace Modsmith.Utils
{
    public static class SystemExtension
    {
        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
