using System;
using Xamarin.Forms;

namespace SignKeys.Effects
{
    public interface ICustomTabView
    {
        event EventHandler<int> IndexChanged;
        void OnTabSelected(int index);
    }
}