using System;

namespace WarzoneConnect.Player
{
    internal sealed class Terminal
    {
        private readonly Shell _thisShell;

        internal Terminal(Shell sh)
        {
            _thisShell = sh;
        }

        internal void Open()
        {
            while (true)
            {
                _thisShell.PreInputPrint();
                _thisShell.CommandIdentify(Console.ReadLine());
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}