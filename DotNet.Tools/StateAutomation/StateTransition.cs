namespace DotNet.Tools.StateAutomation
{
    public struct StateTransition<TState, TInput>
    {
        public TState State { get; init; }

        public TInput Input { get; init; }

        public StateTransition(TState state, TInput input)
        {
            State = state;
            Input = input;
        }
    }

    public static class StateTransition
    {
        public static StateTransition<TState, TInput> Create<TState, TInput>(TState state, TInput input)
        {
            return new StateTransition<TState, TInput>(state, input);
        }
    }
}
