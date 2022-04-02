using System.Collections.Generic;
using System.Reflection;

namespace PochinkiBot.Client.Commands
{
    public class CommandCollection : ICommandCollection
    {
        private class CommandCollectionNode
        {
            public readonly Dictionary<char, CommandCollectionNode> ChildNodes = new Dictionary<char, CommandCollectionNode>();
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
                if (attribute == null)
                    continue;

                var currentNode = _collectionRoot;
                foreach (var c in attribute.Command)
                {
                    currentNode.ChildNodes.TryAdd(char.ToLower(c), new CommandCollectionNode());
                    currentNode = currentNode.ChildNodes[c];
                }

                currentNode.Command = command;
            }
        }

        public IBotCommand GetCommand(string messageText, ref int position)
        {
            var currentNode = _collectionRoot;
            while (position < messageText.Length && currentNode.ChildNodes.TryGetValue(char.ToLower(messageText[position]), out var child))
            {
                currentNode = child;
                position++;
            }

            return currentNode?.Command;
        }
    }
}