using JetBrains.Annotations;

namespace SER.Code.Examples;

[UsedImplicitly]
public class DiscordServerInfoScript : Example
{
    public override string Name => "discordServerInfo";

    public override string Content =>
        """
        !-- OnEvent WaitingForPlayers
        
        # this is the webhook URL, please set one here in order for this script to work
        $url = "https://discord.com/api/webhooks/1464230955945431217/jzQw9SZCFc3yhLf22yzsgiktbQk0Ra4J-_kuVqADOgNNKERSTw9qlHmVjRJz3DKj7vVi"
        
        func SetDiscordMessage
            # get player amount
            $text = "There are {AmountOf @all} players on the server"
            
            # list each player
            foreach @all
                with @plr
                
                # <br> creates a new line
                $text = JoinText $text "<br>- {@plr name}"
            end
        
            *embed = DiscordEmbed "📡 {ServerInfo name} status 📡" $text
            *msg = DiscordMessage _ "{ServerInfo name} status" _ *embed
        
            # remove unneeded variables
            PopVariable local $text
            PopVariable local *embed
        end
        
        if {TextLength $url} is 0
            Error "Script '{This name}' cannot run, because the webhook URL was not set!"
            stop
        end
        
        # create a message to later edit
        # IMPORTANT! this will create message every round
        # if you wish to not, just hardcode the message id
        run SetDiscordMessage
        $messageId = SendDiscordMessageAndWait $url *msg
        
        # update the message every 2 seconds
        forever
            Wait 2s
        
            run SetDiscordMessage
            EditDiscordMessage $url $messageId *msg
        end
        """;
}