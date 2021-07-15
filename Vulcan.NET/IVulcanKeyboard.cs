using System;
using System.Collections.Generic;

namespace Vulcan.NET
{
    /// <summary>
    /// Represents a Vulcan Keyboard
    /// </summary>
    public interface IVulcanKeyboard : IDisposable
    {
        /// <summary>
        /// Which keyboard model is represented by the object. Either fullsize or tkl.
        /// </summary>
        KeyboardType KeyboardType { get; }

        /// <summary>
        /// Contains all the keys present on the keyboard.
        /// </summary>
        IEnumerable<Key> Keys { get; }

        /// <summary>
        /// Connection status of the keyboard
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Sets the whole keyboard to a color
        /// </summary>
        void SetColor(byte r, byte g, byte b);

        /// <summary>
        /// Sets a given key to a given color
        /// </summary>
        void SetKeyColor(Key key, byte r, byte g, byte b);

        /// <summary>
        /// Writes data to the keyboard
        /// </summary>
        bool Update();
    }
}
