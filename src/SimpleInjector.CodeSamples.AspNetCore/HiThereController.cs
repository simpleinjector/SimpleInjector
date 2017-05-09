namespace SimpleInjector.CodeSamples.AspNetCore
{
    using Microsoft.AspNetCore.Mvc;


    public class HiThereController : Controller, IHiThere
    {
        private readonly IHiThere _hithere;

        public HiThereController(IHiThere hithere)
        {
            _hithere = hithere;
        }

        [HttpGet("/")]
        public string SayHi()
        {
            return _hithere.SayHi();
        }
    }
}