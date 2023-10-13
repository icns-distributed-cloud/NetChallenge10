﻿using System;

//Main Program. Run program and first running script is this script.
namespace GameServer
{
    class Program
    {
        //dotnet ChatServer.dll --uniqueID 1 --roomMaxCount 16 --roomMaxUserCount 4 --roomStartNumber 1 --maxUserCount 100
        static void Main(string[] args)
        {
            var serverOption = ParseCommandLine(args);
            if(serverOption == null)
            {
                return;
            }
           
            var serverApp = new MainServer();
            serverApp.InitConfig(serverOption);

            serverApp.CreateStartServer();

            Console.WriteLine("Press q to shut down the server");

            while (true)
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        static GameServerOption ParseCommandLine(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<GameServerOption>(args) as CommandLine.Parsed<GameServerOption>;

            if (result == null)
            {
                System.Console.WriteLine("Failed Command Line");
                return null;
            }

            return result.Value;
        }                  

    } // end Class
}
