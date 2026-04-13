using System;
using System.Collections.Generic;

namespace VoyageForge.Depot.Runtime.Utilities
{
    /// <summary>
    /// 状态机
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class StateMachine<TContext> where TContext : class, new()
    {
        private readonly TContext _context = new();

        public TContext Context => _context;
        
        private IState<TContext> current;

        private Dictionary<string, IState<TContext>> _nodes = new();

        public void To(string name)
        {
            current?.OnExit();

            if (_nodes.TryGetValue(name, out IState<TContext> state))
            {
                current = state;
                current?.OnEnter();
            }
            else
            {
                throw new Exception($"State '{name}' doesn't exist");
            }
        }

        public void To<TState>() where TState : IState<TContext>, new()
        {
            To(typeof(TState).Name);
        }

        public void Create<TState>() where TState : IState<TContext>, new()
        {
            var name = typeof(TState).Name;
            Create<TState>(name);
        }

        public void Create<TState>(string name) where TState : IState<TContext>, new()
        {
            var node = new TState();

            node.OnCreate(this, _context);

            if (!_nodes.TryAdd(name, node))
                throw new Exception($"State with name {name} already exists");
        }


        public void Update()
        {
            current?.Update();
        }
    }
}