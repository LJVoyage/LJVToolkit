namespace LJVoyage.LJVToolkit.Runtime.Utilities
{
    public interface IState<TContext>
        where TContext : class, new()
    {
        public StateMachine<TContext> Machine { get; set; }
        
        public TContext Context { get; set; }
        
        void OnCreate(StateMachine<TContext> machine, TContext context)
        {
            Machine = machine;
            Context = context;
        }

        void OnEnter();
        void Update();
        void OnExit();
    }
}