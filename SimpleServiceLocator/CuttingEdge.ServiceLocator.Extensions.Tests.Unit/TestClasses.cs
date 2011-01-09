namespace CuttingEdge.ServiceLocator.Extensions.Tests.Unit
{
    public interface IWeapon
    {
        void Hit(string target);
    }

    public class Katana : IWeapon
    {
        public void Hit(string target)
        {
        }
    }
}