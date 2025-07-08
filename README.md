# Youtube Live Chat Spam Filter

A Windows-based tool that monitors a YouTube live chat and automatically removes spam messages. It also responds to simple commands sent in the chat.<br>
- Filters known spam patterns<br>
- Allows dynamic spam additions via chat<br>
- Uses Google OAuth2 for authentication<br>
<br>
Quick Start<br>
1 - Download and extract the program files.  <br>
2 - Set up Google OAuth2 credentials. (See below)  <br>
3 - Copy appsettings.template.json to appsettings.json and fill in your YouTube LiveChat ID.  <br>
4 - Run YoutubeSpamFilter.exe.  <br>
<br>
<br>
Setup Instructions<br>
1. Google OAuth2 Credentials<br>
<br>
- Log in to google with the user that will be the bot. It should be a user that has moderator or owner access to the channel since these can send messages more frequently. <br>
- In google search for Youtube API Console <br>
- Click the link called Youtube Data API Overview - Google Developers <br>
- On the left click Get Auth Credentials.<br>
- Click the link that says "Open the Credentials page"<br>
- Click "Create to create a project"<br>
- Accept the terms of service <br>
- Name the project anything and then click create. <br>
- Click Create credentials <br>
- Select OAuth client ID <br>
- Click configure consent screen <br>
- nter a product name, can be anything <br>
- Leave other boxes blank <br>
- Click Save at the bottom <br>
- Under Create OAuth client ID select "Other" <br>
- Type a name in the name box such as "My Client" <br>
- On the next screen where it says OAuth client, click OK. <br>
- Under OAuth 2.0 client IDs, click the download arrow on the far right side <br>
- Download the file <br>
- In Windows Explorer rename the long name of client_secret_345342432362-kjkklkewjfqlkf to simply client_secret.json <br>
- Under API & Services Click Dashboard <br>
- Click Enable APIS and Services <br>
- Search for YouTube Data API v3 and click on it <br>
- Click the blue Enable box. <br>
- If you have multiple google ID's it's important to keep an eye on the upper right to make sure it doesn't switch to another one while doing these steps<br>
<br>
2. Find Your LiveChatID<br>
- a. Find the livechatid for the live stream chat using the Youtube API Console (Live stream must be created first) Each livestream chat has a unique id. You can get this id using the Youtube API Console. <br>
- b. Make sure you are logged in to youtube with the channel owner's account.<br>
- c. In API Explorer click youtube.liveBroadcasts.list<br>
- d. In the "part" box put "snippet" In the broadcastStatus box<br>
- e. Select Active (or Upcoming if it is still upcoming)<br>
- f. Click Execute at the bottom<br>
- g. Scroll down and find the value for liveChatId<br>
- h. Copy this value to notepad temporarily <br>
<br>
3. install Microsoft .NET 4.6.1 if not installed<br>
<br>
4. Create a directory for the spam filter program, such as c:\spamfilter <br>
<br>
5. Uncompress the spam filter program into the directory you just created.<br>
<br>
6. Edit appsettings.template.json<br>
6a. Rename appsettings.template.json to appsettings.json<br>
6b. Open appsettings.json in a text editor and fill in the LiveChatID value with the value you copied earlier from the API Explorer.<br>
6c. Save the file<br>
<br>
7. Copy the client_secret.json file you downloaded earlier into the same directory as the appsettings.json file<br>
<br>
8. Run the program<br>
