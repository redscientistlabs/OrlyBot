# OrlyBot
Discord Bot that restores a user's roles when they rejoin the server and reports suspicious activity

# Features
- Monitors and backups user roles locally.
- Restores roles to users when they rejoin the server
- Blacklist certain roles from being backuped
- Monitor channels for suspicious activity and report in a bot channel
- Monitor users leaving and rejoining fast for attempt of ban/role evasion

# Deployment and usage
- Create a bot application for your instance on the discord website. There are a few tutorials for this on youtube.
- Download the source and edit the config.yml to include the Discord Token for your bot instance.
- Add the bot to your servers and compile/run the application.
- Use the command orly.help to display the list of available commands.

# Database and data saving
This bot only saves Discord ulong IDs and data submitted via bot commands. It does not require any sort of database system as it uses serialized objects to json for data retention.

# Commands 
orly.add_blacklisted_roles @role1 @role2 @role3
orly.remove_blacklisted_roles @role1 @role2 @role3
orly.clear_blacklisted_roles
orly.show_blacklisted_roles
orly.set_report_channel #channel_name
orly.disable_report_channel
orly.show_report_channel
orly.add_suspicious_words fries, ketchup, hot dog, double cheeseburger
orly.remove_suspicious_words bicycle, tennis, golf shoes, soccer ball
orly.clear_suspicious_words
orly.show_suspicious_words
orly.add_ignored_channels #channel1 #channel2 #channel3
orly.remove_ignored_channels #channel1 #channel2 #channel3
orly.clear_ignored_channels
orly.show_ignored_channels

# Special Thanks
I have to give a special thanks to "phase jeff" the developper of the Role Saver Discord Bot for giving me a reason for creating this Open Source free alternative to his bot. While Role Saver does have cool features, I couldn't accept the bot paywall that triggers after a server reaches 500 members. In fact, the Role Server bot will stop working once a server reaches 500 members and the only way to get it working again is by subscribing to jeff's patreon. All I wanted was a free pass Jeff, but now it's a bit too late for that. Thanks for nothing Jeff.



free role saver
role saver alternative
bot that keeps roles discord
user keep role when rejoin
Jail discord bot
backup discord roles
discord bot detect words
discord bot detect fast rejoin
automatically remember role
automatically get roles back
role management
