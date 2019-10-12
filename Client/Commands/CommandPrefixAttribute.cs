using System;

namespace PochinkiBot.Client.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandPrefixAttribute : Attribute
    {
    }
}