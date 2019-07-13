using System;
using System.Linq;
using WyvernFramework;

namespace Bloom.Handlers
{
    /// <summary>
    /// Handler for player input, player information, etc.
    /// </summary>
    public class PlayerInterfaceHandler : IDebug
    {
        /// <summary>
        /// Used for interfacing with a single player's input, information, etc.
        /// </summary>
        public class PlayerInterface : IDebug, IDisposable
        {
            public string Name => nameof(PlayerInterface);

            public string Description => $"A player interface for player {PlayerNumber}";

            public bool Disposed { get; private set; }

            /// <summary>
            /// Number of player this is (starting from 0)
            /// </summary>
            public int PlayerNumber { get; }

            /// <summary>
            /// The input controller for the player
            /// </summary>
            public PlayerInput Input { get; }

            public PlayerInterface(int number)
            {
                PlayerNumber = number;
                // TODO: load control settings from file instead of using hardcoded ones
                Input = new PlayerInput();
            }

            ~PlayerInterface()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (Disposed)
                    return;
                Input.Dispose();
            }
        }

        public string Name => nameof(PlayerInterfaceHandler);

        public string Description => "The player interface handler";

        private PlayerInterface[] _PlayerInterfaces = new PlayerInterface[] { };
        private int _Count;

        /// <summary>
        /// The player interfaces
        /// </summary>
        public PlayerInterface[] PlayerInterfaces => _PlayerInterfaces.ToArray();

        /// <summary>
        /// Get or set the player count
        /// </summary>
        public int Count
        {
            get => _Count;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value < _Count)
                {
                    for (var i = value; i < _Count; i++)
                        _PlayerInterfaces[i].Dispose();
                    _PlayerInterfaces = _PlayerInterfaces.Take(value).ToArray();
                    _Count = value;
                }
                else if (value > _Count)
                {
                    var newPlayers = _PlayerInterfaces.AsEnumerable();
                    for (; _Count < value; _Count++)
                        newPlayers = Enumerable.Append(newPlayers, new PlayerInterface(_Count));
                    _PlayerInterfaces = newPlayers.ToArray();
                }
            }
        }

        public PlayerInterfaceHandler(int playerCount = 1)
        {
            Count = playerCount;
        }
    }
}
