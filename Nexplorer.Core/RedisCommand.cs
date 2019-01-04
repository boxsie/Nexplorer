using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Nexplorer.Core
{
    public class RedisCommand
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisCommand(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var val = await _redis.GetDatabase().StringGetAsync(key);

            return val.HasValue 
                ? Helpers.ProtoDeserialize<T>(val) 
                : default(T);
        }

        public T Get<T>(string key)
        {
            var val = _redis.GetDatabase().StringGet(key);

            return val.HasValue
                ? Helpers.ProtoDeserialize<T>(val)
                : default(T);
        }

        public async Task SetAsync<T>(string key, T val)
        {
            var serialisedVal = Helpers.ProtoSerialize(val);

            await _redis.GetDatabase().StringSetAsync(key, serialisedVal);
        }

        public async Task DeleteAsync(string key)
        {
            await _redis.GetDatabase().KeyDeleteAsync(key);
        }

        public void Subscribe<T>(string key, Func<T, Task> onPublish)
        {
            _redis.GetSubscriber().Subscribe(key, (channel, value) =>
            {
                var deserializedVal = value.HasValue
                    ? Helpers.ProtoDeserialize<T>(value)
                    : default(T);

                onPublish.Invoke(deserializedVal);
            });
        }

        public async Task SubscribeAsync<T>(string key, Func<T, Task> onPublish)
        {
            await _redis.GetSubscriber().SubscribeAsync(key, async (channel, value) =>
            {
                var deserializedVal = value.HasValue 
                    ? Helpers.ProtoDeserialize<T>(value)
                    : default(T);

                await onPublish.Invoke(deserializedVal);
            });
        }

        public void Publish<T>(string key, T val)
        {
            var serialisedVal = Helpers.ProtoSerialize(val);

            _redis.GetSubscriber().Publish(key, serialisedVal);
        }

        public async Task PublishAsync<T>(string key, T val)
        {
            var serialisedVal = Helpers.ProtoSerialize(val);

            await _redis.GetSubscriber().PublishAsync(key, serialisedVal);
        }
    }
}
