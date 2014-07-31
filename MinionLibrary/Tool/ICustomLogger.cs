using System;
namespace Minion.Tool
{
    interface ICustomLogger
    {
        event EventHandler<string> EventLogged;
    }
}
