using System;
using System.Threading;
using System.Threading.Tasks;
using dFakto.States.Workers.Interfaces;

namespace dFakto.States.Workers
{
    internal class FuncWorker : IWorker
    {
        private readonly Func<string, Task<string>> _func;

        public FuncWorker(string name, Func<string, Task<string>> func)
        {
            _func = func;
            ActivityName = name;
        }

        public string ActivityName { get; }
        public TimeSpan HeartbeatDelay => TimeSpan.MaxValue;
        public int MaxConcurrency => 1;

        public async Task<string> DoRawJsonWorkAsync(string input, CancellationToken token)
        {
            return await _func(input);
        }
    }
}