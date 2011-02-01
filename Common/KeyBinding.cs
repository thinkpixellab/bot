using System;
using System.Linq;
using System.Windows.Input;

namespace PixelLab.Common
{
    public class KeyBinding : IEquatable<KeyBinding>
    {
        public KeyBinding(Key key, ModifierKeys modifierKeys)
        {
            Key = key;
            ModifierKeys = modifierKeys;
        }
        public Key Key { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }

        public bool Equals(KeyBinding other)
        {
            return other != null && other.Key == Key && other.ModifierKeys == ModifierKeys;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KeyBinding);
        }

        public override int GetHashCode()
        {
            return Util.GetHashCode(Key, ModifierKeys);
        }

        public override string ToString()
        {
            var output = Key.ToString();
            (new[] { ModifierKeys.Control, ModifierKeys.Shift, ModifierKeys.Alt, ModifierKeys.Apple, ModifierKeys.Windows })
              .Reverse()
              .Where(mk => ModifierKeys.HasFlag(mk))
              .ForEach(mk => output = "{0}+{1}".DoFormat(mk, output));
            return output;
        }
    }
}
