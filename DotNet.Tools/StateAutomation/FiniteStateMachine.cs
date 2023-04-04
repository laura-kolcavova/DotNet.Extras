namespace DotNet.Tools.StateAutomation
{
    public class FiniteStateMachine<TState, TInput>
    {
        protected readonly Dictionary<StateTransition<TState, TInput>, TState> _stateTransitions;

        public TState CurrentState { get; private set; }

        public FiniteStateMachine(TState initState)
        {
            CurrentState = initState;

            _stateTransitions = new Dictionary<StateTransition<TState, TInput>, TState>();
        }

        public virtual void RegisterTransition(StateTransition<TState, TInput> transition, TState state)
        {
            _stateTransitions.Add(transition, state);
        }

        public virtual TState GetStateForTransition(TState sourceState, TInput input)
        {
            var transition = StateTransition.Create(sourceState, input);

            if (!_stateTransitions.TryGetValue(transition, out var destinationState))
            {
                throw new Exception($"Invalid transition for state: {sourceState} and input: {input}");
            }

            return destinationState;
        }

        public virtual TState Switch(TInput input)
        {
            var state = GetStateForTransition(CurrentState, input);
            CurrentState = state;
            return state;
        }
    }
}
