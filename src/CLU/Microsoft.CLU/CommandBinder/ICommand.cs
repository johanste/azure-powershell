using System.Threading.Tasks;

namespace Microsoft.CLU
{
    /// <summary>
    /// The contract that needs to be implemented by an entity representing
    /// entry point of a command in a specific "Programming Model".
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Invokes a synchronous command.
        /// </summary>
        void Invoke();

    }
}
