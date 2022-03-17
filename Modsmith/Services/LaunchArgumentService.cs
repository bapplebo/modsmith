using CommandLine;
using Modsmith.Services.Interfaces;
using Modsmith.Utils;
using System.Diagnostics;

namespace Modsmith.Services
{
    public class LaunchArguments
    {
        public uint AppId { get; set; }
    }

    public class LaunchArgumentService : ILaunchArgumentService
    {
        private LaunchArguments LaunchArguments { get; set; }

        public LaunchArgumentService(ParserResult<LaunchArgumentsInput> launchArguments)
        {
            var newLaunchArguments = new LaunchArguments();
            launchArguments.WithParsed(args =>
            {
                Debug.WriteLine($"FROM MAIN: Running with appid: {args.AppId}");
                if (args.AppId == 0)
                {
                    newLaunchArguments.AppId = (uint)GameUtils.Game.WARHAMMER_2;
                }
                else
                {
                    newLaunchArguments.AppId = args.AppId;
                }

            }).WithNotParsed(err =>
            {
                Debug.WriteLine("FROM MAIN: Failed to map the result, defaulting appId to Warhammer 2");
                newLaunchArguments.AppId = (uint)GameUtils.Game.WARHAMMER_2;
            });

            LaunchArguments = newLaunchArguments;
        }

        public uint GetLaunchArgAppId()
        {
            return LaunchArguments.AppId;
        }
    }
}
