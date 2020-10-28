using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PochinkiBot.Client.Commands
{
    public class CommandArgsParser : ICommandArgsParser
    {
        private readonly ConcurrentDictionary<Type, Func<string[], IEnumerable<CommandArgAttribute>, object>> _parsers = new ConcurrentDictionary<Type, Func<string[], IEnumerable<CommandArgAttribute>, object>>();
        private readonly ConcurrentDictionary<Type, IEnumerable<CommandArgAttribute>> _cachedAttributes = new ConcurrentDictionary<Type, IEnumerable<CommandArgAttribute>>();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, object>>> _initialization =  new ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, object>>>();

        public T ParseArgs<T>(IBotCommand command, SocketUserMessage message, int argsPos)
            where T : class, new()
        {
            var commandType = command.GetType();
            var args = message.Content.Substring(argsPos).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();

            if (!_parsers.TryGetValue(commandType, out var parser))
            {
                var argsDesc = commandType.GetCustomAttributes<CommandArgAttribute>().OrderBy(a => a.Order).ToList();
                _cachedAttributes.TryAdd(commandType, argsDesc);
                
                Expression<Func<string[], IEnumerable<CommandArgAttribute>, object>> expr = (arguments, attributes) =>
                    Parse<T>(arguments, attributes);

                _initialization.TryAdd(commandType, new ConcurrentDictionary<string, Action<object, object>>());

                foreach (var attribute in argsDesc)
                {
                    var r1 = Expression.Parameter(typeof(object));
                    var v1 = Expression.Parameter(typeof(object));
                    var member = (MemberInfo) typeof(T).GetProperty(attribute.Name) ?? typeof(T).GetField(attribute.Name);

                    var t = (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType;
                    if (t == null)
                        continue;

                    var c = Expression.Convert(v1, t);
                    var e = Expression.Bind(member, c).Expression;
                    var l = Expression.Lambda<Action<object, object>>(e, r1, v1).Compile();
                    _initialization[commandType].TryAdd(attribute.Name, l);
                }
                
                parser = expr.Compile();
                _parsers.TryAdd(commandType, parser);
            }

            return (T) parser(args, _cachedAttributes[commandType]);
        }

        private object Parse<T>(string[] args, IEnumerable<CommandArgAttribute> attributes)
            where T : class, new()
        {
            var i = 0;
            var result = new T();
            
            foreach (var attribute in attributes)
            {
                try
                {
                    var v = args.Length == i 
                        ? attribute.DefaultValue 
                        : attribute.Type == typeof(string) 
                            ? args[i++] 
                            : JToken.Parse(args[i++]).ToObject(attribute.Type);
                    _initialization[typeof(T)][attribute.Name](result, v);
                }
                catch (JsonException e)
                {
                    return null;
                }
            }

            return result;
        }
    }

    public interface ICommandArgsParser
    {
        T ParseArgs<T>(IBotCommand command, SocketUserMessage message, int argsPos) where T: class, new();
    }
}