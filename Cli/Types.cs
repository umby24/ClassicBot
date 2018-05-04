namespace Cli {
    public interface ICommand {
        string CommandName { get; }
        void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split);
    }
}