using System;
using System.Linq;
using ClassicBot.Common;
using ClassicBot.World;

namespace Cli.Commands {
    public class SayCommand : ICommand {
        public string CommandName => "!say";

        public void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split) {
            var minusCmd = string.Join(" ", split.Skip(1).ToArray());
            bot.McClient.ClientPlayer.SendMessage(minusCmd);
        }
    }

    public class PlayersCommand : ICommand {
        public string CommandName => "!players";

        public void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split) {
            
        }
    }

    public class WhereAmICommand : ICommand {
        public string CommandName => "!where";
        public void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split) {
            var senderEntity = bot.McClient.ClientPlayer.Entities.Values.FirstOrDefault(a => a.Name == sender);

            if (senderEntity == null) {
                bot.McClient.ClientPlayer.SendMessage("Couldn't find you!");
                return;
            }

            var locationString = $"{senderEntity.Position.X} {senderEntity.Position.Y} {senderEntity.Position.Z}";
            var locationStringReal = $"{((ushort)senderEntity.Position.X)/32} {((ushort)senderEntity.Position.Y)/32} {(((ushort)senderEntity.Position.Z)-51)/32}";
            bot.McClient.ClientPlayer.SendMessage("You are located at " + locationString + " EntityCoords.");
            bot.McClient.ClientPlayer.SendMessage("You are located at " + locationStringReal + " RealCoords");
        }
    }

    public class FollowCommand : ICommand {
        public string CommandName => "!follow";
        
        private bool _isFollowing = false;
        private sbyte _currentlyFollowing = 0;
        private ClassicBot.ClassicBot _bot;
        
        public void Execute(ClassicBot.ClassicBot bot, string sender, string ogMessage, string[] split) {
            _bot = bot;
            if (_isFollowing) {
                _isFollowing = false;
                bot.McClient.ClientPlayer.EntityMoved -= ClientPlayerOnEntityMoved;
                _currentlyFollowing = 0;
                bot.McClient.ClientPlayer.SendMessage("Stopped following.");
                return;
            }
            
            var senderEntity = bot.McClient.ClientPlayer.Entities.Values.FirstOrDefault(a => a.Name == sender);

            if (senderEntity == null) {
                bot.McClient.ClientPlayer.SendMessage("Couldn't find you!");
                return;
            }

            _currentlyFollowing = senderEntity.PlayerId;
            _isFollowing = true;
            bot.McClient.ClientPlayer.SendMessage("Now following!");
            bot.McClient.ClientPlayer.EntityMoved += ClientPlayerOnEntityMoved;
        }

        private void ClientPlayerOnEntityMoved(Entity value) {
            if (_isFollowing && _currentlyFollowing != 0 && value.PlayerId == _currentlyFollowing) {
                _bot.McClient.ClientPlayer.RefreshLocation(value.Position, value.Yaw, value.Pitch);
            }
        }
        
    }
}