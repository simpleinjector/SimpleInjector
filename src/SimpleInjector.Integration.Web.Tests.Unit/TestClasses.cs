namespace SimpleInjector.Integration.Web.Tests.Unit
{
    using System;

    public interface ICommand
    {
        void Execute();
    }

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public class DisposableCommand : ICommand, IDisposable
    {
        public int DisposeCount { get; private set; }

        public bool HasBeenDisposed
        {
            get { return this.DisposeCount > 0; }
        }

        public void Dispose()
        {
            this.DisposeCount++;
        }

        public void Execute()
        {
        }
    }

    public class DisposableCommandWithOverriddenEquality1 : DisposableCommandWithOverriddenEquality
    {
    }

    public class DisposableCommandWithOverriddenEquality2 : DisposableCommandWithOverriddenEquality
    {
    }

    public abstract class DisposableCommandWithOverriddenEquality : ICommand, IDisposable
    {
        public int HashCode { get; set; }

        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            this.DisposeCount++;
        }

        public void Execute()
        {
        }

        public override int GetHashCode() => this.HashCode;

        public override bool Equals(object obj) => this.GetHashCode() == obj.GetHashCode();
    }

    public class CommandDecorator : ICommand
    {
        public CommandDecorator(ICommand decorated)
        {
            this.DecoratedInstance = decorated;
        }

        public ICommand DecoratedInstance { get; }

        public void Execute()
        {
        }
    }
}