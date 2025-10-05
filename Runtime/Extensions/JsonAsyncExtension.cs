using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.Library;
using AceLand.Library.Models;
using Newtonsoft.Json;

namespace AceLand.TaskUtils.Extensions
{
    public static class JsonAsyncExtension
    {
        public static Task<string> ToJsonAsync<T>(this T data, CancellationToken token, bool withTypeName = false)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var settings = withTypeName ? ALib.JsonSerializerSettingsWithType : ALib.JsonSerializerSettings;
                    var json = JsonConvert.SerializeObject(data, Formatting.None, settings);
                    return json;
                }
                catch (Exception e)
                {
                    return token.IsCancellationRequested
                        ? null
                        : throw new Exception($"Serialize {typeof(T).Name} to json error\n{e}");
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static Task<T> ToDataAsync<T>(this JsonData jsonData, CancellationToken token, bool withTypeName = false)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var settings = jsonData.WithTypeName ? ALib.JsonSerializerSettingsWithType : ALib.JsonSerializerSettings;
                    var data = JsonConvert.DeserializeObject<T>(jsonData.Text, settings);
                    return data;
                }
                catch (Exception e)
                {
                    throw new Exception($"Deserialize json to {typeof(T).Name} error\n{e}");
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}