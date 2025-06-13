# Youtube Live Chat Spam Filter

A program that runs in Windows and filters spam messages from a Youtube live stream chat. <br>
<br>
    Responds to commands in the youtube chat.<br>
    Spam can be added to the block list from the channel.<br>
<br>
Quick Start<br>
<br>
    Download and extract the program.<br>
    Configure Google OAuth2 (see below).<br>
    Edit configuration (YoutubeSpamFilter.config).<br>
    Run YoutubeSpamFilter.exe<br>
<br>
Commands<br>
<br>
!add blockedtext - Add string of text to block from the channel. <br>
<br>
Setup Instructions<br>
1. Google OAuth2 Credentials<br>
<br>
Log in to google with the user that will be the bot. It should be a user that has moderator or owner access to the channel since these can send messages more frequently. In google search for Youtube API Console Click the link called Youtube Data API Overview - Google Developers On the left click Get Auth Credentials. Where it says "Open the Credentials page" click that Click Create to create a project Accept the terms of service Name the project anything and then click create. Click Create credentials Select OAuth client ID Click configure consent screen Enter a product name, can be anything Leave other boxes blank Click Save at the bottom Under Create OAuth client ID select "Other" Type a name in the name box such as "My Client" On the next screen where it says OAuth client, click OK. Under OAuth 2.0 client IDs, click the download arrow on the far right side Download the file In Windows Explorer rename the long name of client_secret_345342432362-kjkklkewjfqlkf to simply client_secret.json Under API & Services Click Dashboard Click Enable APIS and Services Search for YouTube Data API v3 and click on it Click the blue Enable box <br>
<br>
If you have multiple google ID's it's important to keep an eye on the upper right to make sure it doesn't switch to another one while doing these steps<br>
2. Find Your LiveChatID<br>
<br>
find the livechatid for the live stream chat using the Youtube API Console (Live stream must be created first) Each livestream chat has a unique id. You can get this id using the Youtube API Console. Make sure you are logged in to youtube with the channel owner's account In API Explorer click youtube.liveBroadcasts.list In the "part" box put "snippet" In the broadcastStatus box select Active (or Upcoming if it is still upcoming) Click Execute at the bottom Scroll down and find the value for liveChatId Copy this value to notepad temporarily 3) install Microsoft .NET 4.6.1 if not installed 4) create a directory for the trivia bot, such as c:\triviabot 5) uncompress the trivia bot zip file into the triviabot directory 6) Add questions and answers to the questions.txt and answers.txt. The question and answer should be on the same line in each file. Don't put question mark at the end of the questions. The bot adds the question mark. The cursor should be on the next line at the end of the document. There shouldn't be any extra lines. Using NotePad++ seems to show more if there are hidden lines or text that could interfere with reading the file.<br>
3. Edit configuration file (triviabot.config)<br>
<br>
Open YoutubeSpamFilter.config Edit the following values. Use double backslashes for directory names. Also might want to use a directory with a short simple path with no spaces or special characters, etc.<br>
<br>
clientSecretsLocation - Location of the client_secrets.json <br>
getMsgDelay - how often in seconds that the bot retrieves chat messages from youtube <br>
<br>
4. Run the program<br>
<br>
    Double click the YoutubeSpamFilter.exe to run the program After about 10 seconds it will finish startup If you have the correct LiveChatID you should see messages in the console window when someone types something in the chat There aren't any commands that can be entered in the console, everything is done in the chat. To exit the program hit any key<br>
<br>
5. Demostration Video<br>
<br>
www.youtube.com/video (coming soon)<br>
<br>
FAQ / Troubleshooting<br>
(coming soon)<br>
