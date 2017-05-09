
namespace SimpleInjector.CodeSamples.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Http;

    public interface IHiThere
    {
        string SayHi();
    }

    public class HiThere : IHiThere
    {
        private readonly IHttpContextAccessor _accessor;

        public HiThere(IHttpContextAccessor a)
        {
            if (a == null) throw new ArgumentException(nameof(a));
            _accessor = a;
        }

        public string SayHi()
        {
            var remotename = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

            return $"Hi {remotename}! This is Asp.Net Core.";
        }
    }
}