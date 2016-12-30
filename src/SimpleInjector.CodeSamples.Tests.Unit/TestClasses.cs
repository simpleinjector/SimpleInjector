namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;

    public interface ICommandHandler<TCommand>
    {
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public interface ICommand
    {
        void Execute();
    }

    public interface IValidator<T>
    {
        void Validate(T instance);
    }

    public sealed class InjectAttribute : Attribute
    {
    }

    public class CommandWithILoggerDependency : ICommand
    {
        public readonly ILogger Logger;

        public CommandWithILoggerDependency(ILogger logger)
        {
            this.Logger = logger;
        }

        public void Execute()
        {
        }
    }

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public sealed class NullLogger : ILogger
    {
        public void Log(string message)
        {
        }
    }

    public class NullValidator<T> : IValidator<T>
    {
        public void Validate(T instance)
        {
        }
    }

    public class LoggingValidator<T> : IValidator<T>
    {
        private readonly ILogger logger;
        public LoggingValidator(ILogger logger)
        {
            this.logger = logger;
        }

        public void Validate(T instance)
        {
            this.logger.Log("Validating ");
        }
    }

    public class LoggingValidatorDecorator<T> : IValidator<T>
    {
        private readonly ILogger logger;
        private readonly IValidator<T> decoratee;

        public LoggingValidatorDecorator(ILogger logger, IValidator<T> decoratee)
        {
            this.logger = logger;
            this.decoratee = decoratee;
        }

        public void Validate(T instance)
        {
            this.logger.Log("Decorating ");
            this.decoratee.Validate(instance);
            this.logger.Log("Decorated ");
        }
    }

    public sealed class ListLogger : List<string>, ILogger
    {
        public void Log(string message)
        {
            this.Add(message);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this);
        }
    }

    public class RealCommand
    {
    }

    public class NullCommandHandler<TCommand> : ICommandHandler<TCommand>
    {
    }

    public class CommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        public readonly ICommandHandler<TCommand> Decoratee;

        public CommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.Decoratee = decoratee;
        }
    }

    public class AnotherCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    {
        public readonly ICommandHandler<TCommand> Decoratee;

        public AnotherCommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.Decoratee = decoratee;
        }
    }

    public class RealCommandCommandHandlerWithDependency<TDependency> : ICommandHandler<RealCommand>
    {
        public readonly TDependency Dependency;

        public RealCommandCommandHandlerWithDependency(TDependency dependency)
        {
            this.Dependency = dependency;
        }
    }

    public class CommandHandlerProxy<TCommand> : ICommandHandler<TCommand>
    {
        public readonly Func<ICommandHandler<TCommand>> DecorateeFactory;

        public CommandHandlerProxy(Func<ICommandHandler<TCommand>> decorateeFactory)
        {
            this.DecorateeFactory = decorateeFactory;
        }
    }
}