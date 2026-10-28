namespace RouterApp;
using System;

static class Program
{

    [STAThread]
    static void Main()
    {

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
