using System;
using InputProvider;

namespace Test
{
    public class InputProviderFactory
    {
        public IInputProvider Create(InputProviderType type)
        {
            return type switch
            {
                InputProviderType.Mouse => new MouseInputProvider(),
                InputProviderType.TestInstant => new InstantTestInputProvider(),
                InputProviderType.TestGradual => new TestInputProvider(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}