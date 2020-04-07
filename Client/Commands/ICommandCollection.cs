namespace PochinkiBot.Client.Commands
{
    public interface ICommandCollection
    {
        IBotCommand GetCommand(string messageText, ref int position);
    }
}