# Youtube Live Chat Spam Filter

A Windows-based tool that monitors a YouTube live chat and automatically removes spam messages. It also responds to simple commands sent in the chat.<br>
- Filters known spam patterns
- Allows dynamic spam additions via chat
- Uses Google OAuth2 for authentication

Quick Start<br>
1 - Download and extract the program files.  
2 - Set up Google OAuth2 credentials. (See below)  
3 - Copy appsettings.template.json to appsettings.json and fill in your YouTube LiveChat ID.  
4 - Run YoutubeSpamFilter.exe.  
<br>

Setup Instructions<br>
1. Google OAuth2 Credentials<br>
<br>
1a. Log in to google with the user that will be the bot. It should be a user that has moderator or owner access to the channel since these can send messages more frequently. <br>
1b. In google search for Youtube API Console Click the link called Youtube Data API Overview - Google Developers On the left click Get Auth Credentials. Where it says "Open the Credentials page" click that Click Create to create a project Accept the terms of service Name the project anything and then click create. Click Create credentials Select OAuth client ID Click configure consent screen Enter a product name, can be anything Leave other boxes blank Click Save at the bottom Under Create OAuth client ID select "Other" Type a name in the name box such as "My Client" On the next screen where it says OAuth client, click OK. Under OAuth 2.0 client IDs, click the download arrow on the far right side Download the file In Windows Explorer rename the long name of client_secret_345342432362-kjkklkewjfqlkf to simply client_secret.json Under API & Services Click Dashboard Click Enable APIS and Services Search for YouTube Data API v3 and click on it Click the blue Enable box. If you have multiple google ID's it's important to keep an eye on the upper right to make sure it doesn't switch to another one while doing these steps<br>
<br>
1. 2. Find Your LiveChatID<br>
2a. Find the livechatid for the live stream chat using the Youtube API Console (Live stream must be created first) Each livestream chat has a unique id. You can get this id using the Youtube API Console. 
2b. Make sure you are logged in to youtube with the channel owner's account.
2c. In API Explorer click youtube.liveBroadcasts.list
2d. In the "part" box put "snippet" In the broadcastStatus box
2e. Select Active (or Upcoming if it is still upcoming)
2f. Click Execute at the bottom
2g. Scroll down and find the value for liveChatId
2h. Copy this value to notepad temporarily 
<br>
3. install Microsoft .NET 4.6.1 if not installed
4. Create a directory for the spam filter program, such as c:\spamfilter <br>
5. Uncompress the spam filter program into the directory you just created.<br>
6. Edit appsettings.template.json<br>
6a. Rename appsettings.template.json to appsettings.json<br>
6b. Open appsettings.json in a text editor and fill in the LiveChatID value with the value you copied earlier from the API Explorer.<br>
6c. Save the file<br>
7. Copy the client_secret.json file you downloaded earlier into the same directory as the appsettings.json file<br>
8. Run the program<br>
