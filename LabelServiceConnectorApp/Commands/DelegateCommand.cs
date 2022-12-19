using System;

namespace LabelServiceConnector.Commands
{
    public class DelegateCommand : CommandBase
    {
        private Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }
        
        public override void Execute(object? parameter)
        {
            _action();
        }
    }
}

