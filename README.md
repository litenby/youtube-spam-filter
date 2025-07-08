# YouTube Live Chat Spam Filter - Setup & Usage Guide  
  
A Windows-based tool that monitors YouTube Live Chat and automatically removes   
spam messages. It can also respond to simple chat commands.  
  
--------------------------------------------------------------------------------  
Features:  
--------------------------------------------------------------------------------  
- Detects and filters known spam patterns  
- Allows real-time spam pattern updates via chat  
- Uses Google OAuth2 for authentication  
  
--------------------------------------------------------------------------------  
Quick Start:  
--------------------------------------------------------------------------------  
1. Download and extract the program files.  
2. Set up Google OAuth2 credentials (see detailed instructions below).  
3. Copy appsettings.template.json → appsettings.json  
4. Edit appsettings.json and add your YouTube LiveChat ID.  
5. Run YoutubeSpamFilter.exe  
  
--------------------------------------------------------------------------------  
Google OAuth2 Setup:  
--------------------------------------------------------------------------------  
1. Sign in to Google using the account that will run the bot (must be a moderator or owner).  
2. Search for "YouTube API Console" in Google.  
3. Open the "YouTube Data API Overview - Google Developers" link.  
4. In the left sidebar, click "Get Auth Credentials".  
5. Click "Open the Credentials page".  
6. Click "Create" to make a new project.  
7. Accept the terms of service.  
8. Name your project (any name is fine), then click Create.  
9. Click "Create Credentials" > "OAuth client ID".  
10. Click "Configure consent screen".  
11. Enter a product name (can be anything).  
12. Leave other fields blank and click Save.  
13. Under "Create OAuth client ID", select "Other" (or "Desktop App" if "Other" is unavailable).  
14. Name it (e.g., "My Client") and click Create.  
15. On the success screen, click OK.  
16. Click the download icon next to your new client to download the credentials.  
17. Rename the downloaded file to: client_secret.json  
18. Go to API & Services > Dashboard  
19. Click "Enable APIs and Services"  
20. Search for "YouTube Data API v3", click it, then click Enable.  
21. Ensure you're still signed into the correct Google account (top-right corner).  
  
--------------------------------------------------------------------------------  
Get Your YouTube LiveChat ID:  
--------------------------------------------------------------------------------  
1. Go to the YouTube API Explorer.  
2. Make sure you are signed in as the channel owner.  
3. Click youtube.liveBroadcasts.list  
4. In the "part" field, enter: snippet  
5. In "broadcastStatus", select: active (or upcoming)  
6. Click Execute.  
7. In the response, find and copy the value of: liveChatId  
  
--------------------------------------------------------------------------------  
Program Setup:  
--------------------------------------------------------------------------------  
1. Ensure Microsoft .NET Framework 4.6.1 is installed.  
2. Create a folder for the program (e.g., C:\spamfilter).  
3. Extract the program files into that folder.  
4. Rename appsettings.template.json to appsettings.json  
5. Edit appsettings.json and paste in your LiveChatID value.  
6. Save the file.  
7. Place client_secret.json into the same directory as appsettings.json  
  
--------------------------------------------------------------------------------  
Run the Program:  
--------------------------------------------------------------------------------  
Double-click YoutubeSpamFilter.exe to start monitoring and filtering live chat.   