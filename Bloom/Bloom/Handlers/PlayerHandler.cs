using System;
using System.Linq;

namespace Bloom.Handlers
{
    /// <summary>
    /// Handler for player input, player information, etc.
    /// </summary>
    public class PlayerHandler
    {
        /// <summary>
        /// Used for interfacing with a single player's input, information, etc.
        /// </summary>
        public class PlayerInterface : IDisposable
        {
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

        private PlayerInterface[] _Players = new PlayerInterface[] { };
        private int _Count;

        /// <summary>
        /// The player interfaces
        /// </summary>
        public PlayerInterface[] Players => _Players.ToArray();

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
                        _Players[i].Dispose();
                    _Players = _Players.Take(value).ToArray();
                    _Count = value;
                }
                else if (value > _Count)
                {
                    var newPlayers = _Players.AsEnumerable();
                    for (; _Count < value; _Count++)
                        newPlayers = newPlayers.Append(new PlayerInterface(_Count));
                    _Players = newPlayers.ToArray();
                }
            }
        }

        public PlayerHandler(int playerCount = 1)
        {
            Count = playerCount;
        }
    }
}
