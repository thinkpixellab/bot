using PixelLab.Common;

namespace PixelLab.Test.Helpers
{
    internal class TestChangeable : Changeable
    {
        private int _foo, _bar, _baz;

        public int Foo
        {
            get { return _foo; }
            set { UpdateProperty("Foo", ref _foo, value); }
        }

        public int Bar
        {
            get { return _bar; }
            set { UpdateProperty("Foo", ref _bar, value); }
        }

        public int Baz
        {
            get { return _baz; }
            set { UpdateProperty("Baz", ref _baz, value); }
        }
    }
}
