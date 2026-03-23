namespace GraveyardHunter.Command
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
