# Youtube Live Chat Spam Filter

A Windows-based tool that monitors a YouTube live chat and automatically removes spam messages. It also responds to simple commands sent in the chat.<br>
- Filters known spam patterns<br>
- Allows dynamic spam additions via chat<br>
- Uses Google OAuth2 for authentication<br>
<br>
Quick Start<br>
Download and extract the program files.  <br>
Set up Google OAuth2 credentials. (See below)  <br>
Copy appsettings.template.json to appsettings.json and fill in your YouTube LiveChat ID.  <br>
Run YoutubeSpamFilter.exe.  <br>
<br>
Setup Instructions<br>
Google OAuth2 Credentials<br>
<br>
1 Log in to google with the user that will be the bot. It should be a user that has moderator or owner access to the channel since these can send messages more frequently. <br>
2 In google search for Youtube API Console <br>
3 Click the link called Youtube Data API Overview - Google Developers <br>
4 On the left click Get Auth Credentials.<br>
5 Click the link that says "Open the Credentials page"<br>
6 Click "Create to create a project"<br>
7 Accept the terms of service <br>
8 Name the project anything and then click create. <br>
9 Click Create credentials <br>
10 Select OAuth client ID <br>
11 Click configure consent screen <br>
12 nter a product name, can be anything <br>
13 Leave other boxes blank <br>
14 Click Save at the bottom <br>
15 Under Create OAuth client ID select "Other" <br>
16 Type a name in the name box such as "My Client" <br>
17 On the next screen where it says OAuth client, click OK. <br>
18 Under OAuth 2.0 client IDs, click the download arrow on the far right side <br>
19 Download the file <br>
20 In Windows Explorer rename the long name of client_secret_345342432362-kjkklkewjfqlkf to simply client_secret.json <br>
21 Under API & Services Click Dashboard <br>
22 Click Enable APIS and Services <br>
23 Search for YouTube Data API v3 and click on it <br>
24 Click the blue Enable box. <br>
25 If you have multiple google ID's it's important to keep an eye on the upper right to make sure it doesn't switch to another one while doing these steps<br>
<br>
Find Your LiveChatID<br>
1 Find the livechatid for the live stream chat using the Youtube API Console (Live stream must be created first) Each livestream chat has a unique id. You can get this id using the Youtube API Console. <br>
2 Make sure you are logged in to youtube with the channel owner's account.<br>
3 In API Explorer click youtube.liveBroadcasts.list<br>
4 In the "part" box put "snippet" In the broadcastStatus box<br>
5 Select Active (or Upcoming if it is still upcoming)<br>
6 Click Execute at the bottom<br>
7 Scroll down and find the value for liveChatId<br>
8 Copy this value to notepad temporarily <br>
<br>
Install Microsoft .NET 4.6.1 if not installed<br>
<br>
Create a directory for the spam filter program, such as c:\spamfilter <br>
<br>
Uncompress the spam filter program into the directory you just created.<br>
<br>
Edit appsettings.template.json<br>
Rename appsettings.template.json to appsettings.json<br>
Open appsettings.json in a text editor and fill in the LiveChatID value with the value you copied earlier from the API Explorer.<br>
Save the file<br>
<br>
Copy the client_secret.json file you downloaded earlier into the same directory as the appsettings.json file<br>
<br>
Run the program<br>
