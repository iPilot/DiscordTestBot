using System.Collections.Generic;
using System.Reflection;

namespace PochinkiBot.Client.Commands
{
    public class CommandCollection : ICommandCollection
    {
        private class CommandCollectionNode
        {
            public Dictionary<char, CommandCollectionNode> ChildNodes = new Dictionary<char, CommandCollectionNode>();
            public IBotCommand Command;

            public override string ToString()
            {
                return $"[{string.Join(' ', ChildNodes.Keys)}]";
            }
        }

        private readonly CommandCollectionNode _collectionRoot;

        public CommandCollection(IEnumerable<IBotCommand> commands)
        {
            _collectionRoot = new CommandCollectionNode();
            foreach (var command in commands)
            {
                var attribute = command.GetType().GetCustomAttribute<CommandAttribute>();
                var currentNode = _collectionRoot;
                foreach (var c in attribute.Command)
                {
                    currentNode.ChildNodes.TryAdd(char.ToLower(c), new CommandCollectionNode());
                    currentNode = currentNode.ChildNodes[c];
                }

                currentNode.Command = command;
            }
        }

        public IBotCommand GetCommand(string messageText, int startPosition)
        {
            var i = startPosition;
            var currentNode = _collectionRoot;
            while (i < messageText.Length && currentNode.ChildNodes.TryGetValue(char.ToLower(messageText[i]), out var child))
            {
                currentNode = child;
                i++;
            }

            return currentNode?.Command;
        }
    }
}